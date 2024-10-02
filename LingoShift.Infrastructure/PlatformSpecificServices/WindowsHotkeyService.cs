using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using LingoShift.Domain.DomainServices;

namespace LingoShift.Infrastructure.PlatformSpecificServices
{
    public class WindowsHotkeyService : IHotkeyService
    {
        #region SendText
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        private const int INPUT_MOUSE = 0;
        private const int INPUT_KEYBOARD = 1;
        private const int INPUT_HARDWARE = 2;

        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_UNICODE = 0x0004;
        private const uint KEYEVENTF_SCANCODE = 0x0008;

        #endregion

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelKeyboardProc _proc;
        private readonly Dictionary<string, (HotkeyDefinition definition, Action action)> _registeredHotkeys = new Dictionary<string, (HotkeyDefinition, Action)>();
        private readonly Dictionary<string, (string sequence, Action action, Func<Task> asyncAction)> _registeredSequences = new Dictionary<string, (string, Action, Func<Task>)>();
        private List<Keys> _keySequence = new List<Keys>();
        private StringBuilder _charSequence = new StringBuilder();
        private DateTime _lastKeyPressTime = DateTime.MinValue;
        private const int MAX_SEQUENCE_DELAY = 5000; // milliseconds
        private bool _isProcessingSequence = false;

        public WindowsHotkeyService()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        public void SendText(string text)
        {
            List<INPUT> inputs = new List<INPUT>();

            foreach (char c in text)
            {
                INPUT inputDown = new INPUT();
                inputDown.type = INPUT_KEYBOARD;
                inputDown.u.ki.wScan = 0;
                inputDown.u.ki.wVk = 0;
                inputDown.u.ki.dwFlags = KEYEVENTF_UNICODE;
                inputDown.u.ki.time = 0;
                inputDown.u.ki.dwExtraInfo = IntPtr.Zero;
                inputDown.u.ki.wScan = (ushort)c;

                INPUT inputUp = inputDown;
                inputUp.u.ki.dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP;

                inputs.Add(inputDown);
                inputs.Add(inputUp);
            }

            SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf(typeof(INPUT)));
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Task.Run(() => ProcessKeyPress((Keys)vkCode)).Wait();
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private async Task ProcessKeyPress(Keys key)
        {
            if (_isProcessingSequence)
            {
                return; // Evita l'elaborazione ricorsiva delle sequenze
            }

            var currentTime = DateTime.Now;
            if ((currentTime - _lastKeyPressTime).TotalMilliseconds > MAX_SEQUENCE_DELAY)
            {
                _charSequence.Clear();
            }
            _lastKeyPressTime = currentTime;

            char? keyChar = KeyToChar(key);
            if (keyChar.HasValue)
            {
                _charSequence.Append(keyChar.Value);
                Debug.WriteLine($"Key pressed: {key}, Char: {keyChar.Value}");
                Debug.WriteLine($"Current sequence: {_charSequence}");

                // Mantieni solo gli ultimi 50 caratteri
                if (_charSequence.Length > 50)
                {
                    _charSequence.Remove(0, _charSequence.Length - 50);
                }

                // Controlla i trigger delle sequenze di caratteri
                string currentSequence = _charSequence.ToString();
                foreach (var sequence in _registeredSequences)
                {
                    Debug.WriteLine($"Checking sequence: {sequence.Key} ({sequence.Value.sequence})");
                    if (currentSequence.EndsWith(sequence.Value.sequence, StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine($"Sequence triggered: {sequence.Key}");
                        _isProcessingSequence = true;

                        if (sequence.Value.asyncAction != null)
                        {
                            await sequence.Value.asyncAction.Invoke();
                        }
                        else
                        {
                            sequence.Value.action?.Invoke();
                        }

                        _isProcessingSequence = false;

                        // Rimuovi solo la sequenza attivata, non l'intero buffer
                        int startIndex = _charSequence.Length - sequence.Value.sequence.Length;
                        _charSequence.Remove(startIndex, sequence.Value.sequence.Length);

                        return;
                    }
                }
            }


            // Controlla i trigger delle hotkey
            foreach (var hotkey in _registeredHotkeys)
            {
                if (IsHotkeyTriggered(hotkey.Value.definition))
                {
                    Debug.WriteLine($"Hotkey triggered: {hotkey.Key}");
                    hotkey.Value.action.Invoke();
                    _keySequence.Clear();
                    _charSequence.Clear();
                    return;
                }
            }
        }

        private bool IsHotkeyTriggered(HotkeyDefinition definition)
        {
            if (_keySequence.Count < definition.Keys.Count)
            {
                return false;
            }

            var relevantSequence = _keySequence.TakeLast(definition.Keys.Count).ToList();

            bool modifiersMatch =
                ((definition.Modifiers & ModifierKeys.Control) != 0) == IsKeyPressed(Keys.ControlKey) &&
                ((definition.Modifiers & ModifierKeys.Alt) != 0) == IsKeyPressed(Keys.Menu) &&
                ((definition.Modifiers & ModifierKeys.Shift) != 0) == IsKeyPressed(Keys.ShiftKey);

            return modifiersMatch && relevantSequence.SequenceEqual(definition.Keys);
        }

        private bool IsKeyPressed(Keys key)
        {
            return (GetAsyncKeyState((int)key) & 0x8000) != 0;
        }

        public void RegisterHotkey(string hotkeyName, HotkeyDefinition definition, Action action)
        {
            _registeredHotkeys[hotkeyName] = (definition, action);
            Debug.WriteLine($"Registered hotkey: {hotkeyName}");
        }

        public void RegisterSequence(string sequenceName, string sequence, Action action)
        {
            _registeredSequences[sequenceName] = (sequence.ToLower(), action, null);
            Debug.WriteLine($"Registered sequence: {sequenceName} ({sequence.ToLower()})");
        }

        public void RegisterAsyncSequence(string sequenceName, string sequence, Func<Task> asyncAction)
        {
            _registeredSequences[sequenceName] = (sequence.ToLower(), null, asyncAction);
            Debug.WriteLine($"Registered async sequence: {sequenceName} ({sequence.ToLower()})");
        }

        public void DebugPrintRegisteredSequences()
        {
            Debug.WriteLine("Registered Sequences:");
            foreach (var sequence in _registeredSequences)
            {
                Debug.WriteLine($"  {sequence.Key}: {sequence.Value.sequence}");
            }
        }

        public void UnregisterHotkey(string hotkeyName)
        {
            if (_registeredHotkeys.Remove(hotkeyName))
            {
                Debug.WriteLine($"Unregistered hotkey: {hotkeyName}");
            }
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private char? KeyToChar(Keys key)
        {
            // Gestione delle lettere (converti in minuscolo)
            if (key >= Keys.A && key <= Keys.Z)
            {
                return char.ToLower((char)key);
            }

            // Gestione dei numeri
            if (key >= Keys.D0 && key <= Keys.D9)
            {
                return (char)key;
            }

            // Gestione dei caratteri speciali
            switch (key)
            {
                case Keys.OemMinus: return ShiftPressed() ? '_' : '-';
                case Keys.OemPlus: return ShiftPressed() ? '+' : '=';
                case Keys.OemPeriod: return ShiftPressed() ? '>' : '.';
                case Keys.OemComma: return ShiftPressed() ? '<' : ',';
                case Keys.Space: return ' ';
                case Keys.D1: return ShiftPressed() ? '!' : '1';
                case Keys.D2: return ShiftPressed() ? '@' : '2';
                case Keys.D3: return ShiftPressed() ? '#' : '3';
                case Keys.D4: return ShiftPressed() ? '$' : '4';
                case Keys.D5: return ShiftPressed() ? '%' : '5';
                case Keys.D6: return ShiftPressed() ? '^' : '6';
                case Keys.D7: return ShiftPressed() ? '&' : '7';
                case Keys.D8: return ShiftPressed() ? '*' : '8';
                case Keys.D9: return ShiftPressed() ? '(' : '9';
                case Keys.D0: return ShiftPressed() ? ')' : '0';
                case Keys.Enter: return '\n';
                case Keys.Tab: return '\t';
                case Keys.Back: return '\b'; // Backspace
                default: return null;
            }
        }

        private bool ShiftPressed()
        {
            return (GetAsyncKeyState((int)Keys.ShiftKey) & 0x8000) != 0;
        }
    }
}