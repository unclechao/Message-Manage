using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace 消息总线程序
{
    /// <summary>
    /// Summary description for Win32Native.
    /// </summary>
    /// 
    [SuppressUnmanagedCodeSecurity]
    internal class Win32Native
    {
        internal enum FileMap
        {
            FILE_MAP_READ = 0x4,
            FILE_MAP_WRITE = 0x2,
            FILE_MAP_COPY = 0x1,
            FILE_MAP_ALL_ACCESS = 0x1 + 0x2 + 0x4 + 0x8 + 0x10 + 0xF0000
        } ;

        internal enum StdHandle
        {
            STD_INPUT_HANDLE = -10,
            STD_OUTPUT_HANDLE = -11,
            STD_ERROR_HANDLE = -12
        }
        internal enum ProtectionLevel
        {
            PAGE_NOACCESS = 0x1,
            PAGE_READONLY = 0x2,
            PAGE_READWRITE = 0x4,
            PAGE_WRITECOPY = 0x8,
            PAGE_EXECUTE = 0x10
        } ;

        internal const int INVALID_HANDLE_VALUE = -1;
        internal const int ERROR_INVALID_HANDLE = 6;

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern IntPtr CreateFileMapping(IntPtr hFile,
                                                        IntPtr secAttributes,
                                                        ProtectionLevel dwProtect,
                                                        int dwMaximumSizeHigh,
                                                        int dwMaximumSizeLow,
                                                        string lpName);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr handle);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject,
                                                        FileMap dwDesiredAccess,
                                                        int dwFileOffsetHigh,
                                                        int dwFileOffsetLow,
                                                        int dwNumberOfBytesToMap);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool UnmapViewOfFile(IntPtr map);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenFileMapping(FileMap dwDesiredAccess,  // access mode
                                                        bool bInheritHandle,    // inherit flag
                                                        string lpName          // object name
                                                        );

        [DllImport("Kernel32.dll")]
        internal static extern uint GetLastError();

        [DllImport("Kernel32.dll", EntryPoint = "CopyMemory")]
        internal static extern void Kernel32CopyMemory(int dest, int source, int size);
    }
}
