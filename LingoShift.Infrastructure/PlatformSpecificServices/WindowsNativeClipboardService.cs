using System.Runtime.InteropServices;
using System.Text;

namespace LingoShift.Infrastructure.PlatformSpecificServices;

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

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalFree(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern UIntPtr GlobalSize(IntPtr hMem);

    private const uint CF_UNICODETEXT = 13;
    private const uint GMEM_MOVEABLE = 0x0002;

    public async Task<string> GetSelectedTextAsync()
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
                    int error = Marshal.GetLastWin32Error();
                    if (error != 0) // ERROR_SUCCESS
                    {
                        throw new InvalidOperationException($"Cannot get clipboard data. Error: {error}");
                    }
                    return string.Empty; // No text in clipboard
                }

                IntPtr pData = GlobalLock(hData);
                if (pData == IntPtr.Zero)
                {
                    throw new InvalidOperationException($"Cannot lock memory. Error: {Marshal.GetLastWin32Error()}");
                }

                try
                {
                    int size = (int)GlobalSize(hData);
                    byte[] bytes = new byte[size];
                    Marshal.Copy(pData, bytes, 0, size);
                    return Encoding.Unicode.GetString(bytes).TrimEnd('\0');
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

    public async Task ReplaceSelectedTextAsync(string newText)
    {
        if (string.IsNullOrEmpty(newText))
        {
            throw new ArgumentException("New text cannot be null or empty.", nameof(newText));
        }

        await Task.Run(() =>
        {
            if (!OpenClipboard(IntPtr.Zero))
            {
                throw new InvalidOperationException($"Cannot open clipboard. Error: {Marshal.GetLastWin32Error()}");
            }

            try
            {
                EmptyClipboard();

                byte[] bytes = Encoding.Unicode.GetBytes(newText + '\0');
                IntPtr hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes.Length);
                if (hGlobal == IntPtr.Zero)
                {
                    throw new OutOfMemoryException($"Cannot allocate memory. Error: {Marshal.GetLastWin32Error()}");
                }

                try
                {
                    IntPtr pData = GlobalLock(hGlobal);
                    if (pData == IntPtr.Zero)
                    {
                        throw new InvalidOperationException($"Cannot lock memory. Error: {Marshal.GetLastWin32Error()}");
                    }

                    try
                    {
                        Marshal.Copy(bytes, 0, pData, bytes.Length);
                    }
                    finally
                    {
                        GlobalUnlock(hGlobal);
                    }

                    if (SetClipboardData(CF_UNICODETEXT, hGlobal) == IntPtr.Zero)
                    {
                        throw new InvalidOperationException($"SetClipboardData failed. Error: {Marshal.GetLastWin32Error()}");
                    }

                    hGlobal = IntPtr.Zero; // The system now owns this memory
                }
                finally
                {
                    if (hGlobal != IntPtr.Zero)
                    {
                        GlobalFree(hGlobal);
                    }
                }
            }
            finally
            {
                CloseClipboard();
            }
        });
    }
}
