using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LingoShift.Infrastructure.PlatformSpecificServices
{
    public class WindowsNativeClipboardService
    {
        // P/Invoke declarations
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public INPUTUNION u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
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

        private const int INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const ushort VK_CONTROL = 0x11;
        private const ushort VK_A = 0x41;
        private const ushort VK_C = 0x43;
        private const ushort VK_V = 0x56;
        private const uint CF_UNICODETEXT = 13;

        private const int MAX_RETRIES = 5;
        private const int RETRY_DELAY_MS = 100;

        public async Task<string> SelectCopyAndGetTextAsync()
        {
            for (int i = 0; i < MAX_RETRIES; i++)
            {
                try
                {
                    await SelectAllAndCopyToClipboardAsync();
                    await Task.Delay(200);
                    string clipboardText = await GetClipboardTextAsync();
                    if (!string.IsNullOrEmpty(clipboardText))
                    {
                        await SetClipboardTextAsync(clipboardText);
                        return clipboardText;
                    }
                    Debug.WriteLine($"Attempt {i + 1}: Clipboard text is empty, retrying...");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Attempt {i + 1} failed: {ex.Message}");
                }
                await Task.Delay(RETRY_DELAY_MS);
            }
            throw new InvalidOperationException("Failed to select, copy and get text after multiple attempts.");
        }

        private async Task SelectAllAndCopyToClipboardAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    SendKeyboardInput(VK_CONTROL, false);
                    SendKeyboardInput(VK_A, false);
                    Task.Delay(50).Wait(); // Small delay between key presses
                    SendKeyboardInput(VK_A, true);
                    SendKeyboardInput(VK_CONTROL, true);

                    Task.Delay(100).Wait();

                    SendKeyboardInput(VK_CONTROL, false);
                    SendKeyboardInput(VK_C, false);
                    Task.Delay(50).Wait();
                    SendKeyboardInput(VK_C, true);
                    SendKeyboardInput(VK_CONTROL, true);

                    Task.Delay(100).Wait();

                    Debug.WriteLine("Select All and Copy simulated");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in SelectAllAndCopyToClipboardAsync: {ex.Message}");
                    throw;
                }
            });
        }

        private void SendKeyboardInput(ushort keyCode, bool keyUp)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = keyCode;
            inputs[0].u.ki.dwFlags = keyUp ? KEYEVENTF_KEYUP : 0;

            uint result = SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
            if (result != 1)
            {
                throw new InvalidOperationException($"SendInput failed: {Marshal.GetLastWin32Error()}");
            }
        }

        public async Task<string> GetClipboardTextAsync()
        {
            for (int i = 0; i < MAX_RETRIES; i++)
            {
                try
                {
                    return await Task.Run(() =>
                    {
                        if (!OpenClipboard(IntPtr.Zero))
                        {
                            throw new InvalidOperationException($"Cannot open clipboard. Error: {Marshal.GetLastWin32Error()}");
                        }

                        try
                        {
                            IntPtr hData = GetClipboardData(CF_UNICODETEXT);
                            if (hData == IntPtr.Zero)
                            {
                                Debug.WriteLine("No text in clipboard.");
                                return string.Empty;
                            }

                            IntPtr pData = GlobalLock(hData);
                            if (pData == IntPtr.Zero)
                            {
                                throw new InvalidOperationException($"Cannot lock memory. Error: {Marshal.GetLastWin32Error()}");
                            }

                            try
                            {
                                string text = Marshal.PtrToStringUni(pData);
                                Debug.WriteLine($"Text read from clipboard: {text}");
                                return text;
                            }
                            finally
                            {
                                GlobalUnlock(hData);
                            }
                        }
                        finally
                        {
                            CloseClipboard();
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Attempt {i + 1} to read clipboard failed: {ex.Message}");
                    if (i == MAX_RETRIES - 1) throw;
                }
                await Task.Delay(RETRY_DELAY_MS);
            }
            throw new InvalidOperationException("Failed to read clipboard after multiple attempts.");
        }

        public async Task SetClipboardTextAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));
            }

            for (int i = 0; i < MAX_RETRIES; i++)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        if (!OpenClipboard(IntPtr.Zero))
                        {
                            throw new InvalidOperationException($"Cannot open clipboard. Error: {Marshal.GetLastWin32Error()}");
                        }

                        try
                        {
                            EmptyClipboard();

                            IntPtr hGlobal = Marshal.StringToHGlobalUni(text);
                            if (hGlobal == IntPtr.Zero)
                            {
                                throw new OutOfMemoryException($"Cannot allocate memory. Error: {Marshal.GetLastWin32Error()}");
                            }

                            if (SetClipboardData(CF_UNICODETEXT, hGlobal) == IntPtr.Zero)
                            {
                                Marshal.FreeHGlobal(hGlobal);
                                throw new InvalidOperationException($"SetClipboardData failed. Error: {Marshal.GetLastWin32Error()}");
                            }

                            Debug.WriteLine($"Text set to clipboard: {text}");
                        }
                        finally
                        {
                            CloseClipboard();
                        }
                    });
                    return; // Success, exit the method
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Attempt {i + 1} to set clipboard failed: {ex.Message}");
                    if (i == MAX_RETRIES - 1) throw;
                }
                await Task.Delay(RETRY_DELAY_MS);
            }
        }

        public async Task PasteFromClipboardAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    SendKeyboardInput(VK_CONTROL, false);
                    SendKeyboardInput(VK_V, false);
                    Task.Delay(50).Wait();
                    SendKeyboardInput(VK_V, true);
                    SendKeyboardInput(VK_CONTROL, true);

                    Debug.WriteLine("Paste simulated");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in PasteFromClipboardAsync: {ex.Message}");
                    throw;
                }
            });
        }

        public bool IsClipboardEmpty()
        {
            if (!OpenClipboard(IntPtr.Zero))
            {
                throw new InvalidOperationException($"Cannot open clipboard. Error: {Marshal.GetLastWin32Error()}");
            }

            try
            {
                return GetClipboardData(CF_UNICODETEXT) == IntPtr.Zero;
            }
            finally
            {
                CloseClipboard();
            }
        }
    }
}