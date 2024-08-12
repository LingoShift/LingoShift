namespace LingoShift.Domain.DomainServices;

public interface IHotkeyService : IDisposable
{
    void RegisterHotkey(string hotkeyName, HotkeyDefinition definition, Action action);
    void RegisterSequence(string sequenceName, string sequence, Action action);
    void UnregisterHotkey(string hotkeyName);
}

public class HotkeyDefinition
{
    public ModifierKeys Modifiers { get; }
    public List<Keys> Keys { get; }

    public HotkeyDefinition(ModifierKeys modifiers, params Keys[] keys)
    {
        Modifiers = modifiers;
        Keys = new List<Keys>(keys);
    }
}

[Flags]
public enum ModifierKeys : uint
{
    None = 0x0000,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Win = 0x0008
}

public enum Keys : uint
{
    // Lettere dell'alfabeto
    A = 0x41,
    B = 0x42,
    C = 0x43,
    D = 0x44,
    E = 0x45,
    F = 0x46,
    G = 0x47,
    H = 0x48,
    I = 0x49,
    J = 0x4A,
    K = 0x4B,
    L = 0x4C,
    M = 0x4D,
    N = 0x4E,
    O = 0x4F,
    P = 0x50,
    Q = 0x51,
    R = 0x52,
    S = 0x53,
    T = 0x54,
    U = 0x55,
    V = 0x56,
    W = 0x57,
    X = 0x58,
    Y = 0x59,
    Z = 0x5A,

    // Numeri
    D0 = 0x30,
    D1 = 0x31,
    D2 = 0x32,
    D3 = 0x33,
    D4 = 0x34,
    D5 = 0x35,
    D6 = 0x36,
    D7 = 0x37,
    D8 = 0x38,
    D9 = 0x39,

    // Tasti funzione
    F1 = 0x70,
    F2 = 0x71,
    F3 = 0x72,
    F4 = 0x73,
    F5 = 0x74,
    F6 = 0x75,
    F7 = 0x76,
    F8 = 0x77,
    F9 = 0x78,
    F10 = 0x79,
    F11 = 0x7A,
    F12 = 0x7B,

    // Tasti di controllo e modifica
    ControlKey = 0x11,
    ShiftKey = 0x10,
    Menu = 0x12, // Alt key
    LWin = 0x5B, // Left Windows key
    RWin = 0x5C, // Right Windows key

    // Tasti di navigazione
    Left = 0x25,
    Up = 0x26,
    Right = 0x27,
    Down = 0x28,
    Home = 0x24,
    End = 0x23,
    PageUp = 0x21,
    PageDown = 0x22,

    // Tasti di editing
    Insert = 0x2D,
    Delete = 0x2E,
    Back = 0x08, // Backspace
    Tab = 0x09,
    Enter = 0x0D,
    Escape = 0x1B,
    Space = 0x20,

    // Tasti del tastierino numerico
    NumPad0 = 0x60,
    NumPad1 = 0x61,
    NumPad2 = 0x62,
    NumPad3 = 0x63,
    NumPad4 = 0x64,
    NumPad5 = 0x65,
    NumPad6 = 0x66,
    NumPad7 = 0x67,
    NumPad8 = 0x68,
    NumPad9 = 0x69,
    Multiply = 0x6A,
    Add = 0x6B,
    Separator = 0x6C,
    Subtract = 0x6D,
    Decimal = 0x6E,
    Divide = 0x6F,

    // Tasti di punteggiatura e simboli
    OemPlus = 0xBB,    // '=' on US standard keyboard
    OemComma = 0xBC,   // ',' on US standard keyboard
    OemMinus = 0xBD,   // '-' on US standard keyboard
    OemPeriod = 0xBE,  // '.' on US standard keyboard

    // Altri tasti comuni
    CapsLock = 0x14,
    NumLock = 0x90,
    ScrollLock = 0x91,
    PrintScreen = 0x2C,
    Pause = 0x13,

    // Tasti multimediali (se supportati dalla tastiera)
    VolumeMute = 0xAD,
    VolumeDown = 0xAE,
    VolumeUp = 0xAF,
    MediaNextTrack = 0xB0,
    MediaPreviousTrack = 0xB1,
    MediaStop = 0xB2,
    MediaPlayPause = 0xB3,
    LaunchMail = 0xB4,
    LaunchMediaSelect = 0xB5,
    LaunchApp1 = 0xB6,
    LaunchApp2 = 0xB7
}
