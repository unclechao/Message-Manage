using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

 namespace 消息总线程序
{
    public class SharedMemoryException : ApplicationException
    {
        public SharedMemoryException(string msg)
            : base(msg)
        {
        }
    }

    public sealed class SharedMemory : IDisposable
    {
        private IntPtr shmHandle = IntPtr.Zero;
        private IntPtr msgPtr = IntPtr.Zero;
        private IntPtr verPtr = IntPtr.Zero;

        public SharedMemory(string name, long size)
        {
            shmHandle = Win32Native.OpenFileMapping(Win32Native.FileMap.FILE_MAP_ALL_ACCESS, true, name);
            if (shmHandle == IntPtr.Zero)
            {
                int hi = (int)(size >> 32);
                int low = (int)size;
                Console.WriteLine("create sharememory");
                shmHandle = Win32Native.CreateFileMapping((IntPtr)Win32Native.INVALID_HANDLE_VALUE,
                    IntPtr.Zero,
                    Win32Native.ProtectionLevel.PAGE_READWRITE,
                    hi,
                    low,
                    name);
            }
            if (shmHandle == IntPtr.Zero)
            {
                uint i = Win32Native.GetLastError();
                if (i == Win32Native.ERROR_INVALID_HANDLE)
                    throw new SharedMemoryException("Shared memory segment already in use");
                else
                    throw new SharedMemoryException("Unable to access shared memory segment. GetLastError = " + i.ToString());
            }


            verPtr = Win32Native.MapViewOfFile(shmHandle, Win32Native.FileMap.FILE_MAP_ALL_ACCESS, 0, 0, 0);
            if (verPtr == IntPtr.Zero)
            {
                uint i = Win32Native.GetLastError();
                Win32Native.CloseHandle(shmHandle);
                shmHandle = IntPtr.Zero;
                throw new SharedMemoryException("Unable to map shared memory segment. GetLastError = " + i);
            }

            msgPtr = (IntPtr)(verPtr.ToInt32() + 8);
        }

        public int Version
        {
            get
            {
                if (verPtr != IntPtr.Zero)
                {
                    return Marshal.ReadInt32(verPtr);
                }
                else
                    return -1;
            }
            set
            {
                if (verPtr != IntPtr.Zero)
                {
                    Marshal.WriteInt32(verPtr, value);
                }
            }
        }

        public int DataLength
        {
            get
            {
                if (verPtr != IntPtr.Zero)
                {
                    return Marshal.ReadInt32(verPtr, 4);
                }
                else
                    return -1;
            }
            set
            {
                if (verPtr != IntPtr.Zero)
                {
                    Marshal.WriteInt32(verPtr, 4, value);
                }
            }
        }

        #region Message
        public string Message
        {
            get
            {
                if (msgPtr != IntPtr.Zero)
                {
                    return Marshal.PtrToStringAnsi(msgPtr);
                }
                else
                    return string.Empty;
            }

            set
            {
                if (msgPtr != IntPtr.Zero)
                {
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(value.ToString());
                    Marshal.Copy(data, 0, msgPtr, data.Length);
                    Marshal.WriteByte(msgPtr, data.Length, 0);
                    Version++;
                }
            }
        }
        #endregion

        #region Data
        public object Data
        {
            get
            {
                if (msgPtr != IntPtr.Zero)
                {
                    byte[] bytes = new byte[DataLength];
                    Marshal.Copy(msgPtr, bytes, 0, bytes.Length);

                    BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream(bytes);
                    return formatter.Deserialize(ms);
                }
                else
                    return null;
            }
            set
            {
                if (msgPtr != IntPtr.Zero)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream();
                    formatter.Serialize(ms, value);
                    ms.Flush();
                    byte[] bytes = ms.ToArray();
                    DataLength = bytes.Length;
                    Marshal.Copy(bytes, 0, msgPtr, bytes.Length);
                    Version++;
                }
            }
        }
        #endregion

        public void Dispose()
        {
            if (shmHandle != IntPtr.Zero)
            {
                Win32Native.UnmapViewOfFile(shmHandle);
            }

            if (shmHandle != IntPtr.Zero)
            {
                Win32Native.CloseHandle(shmHandle);
            }
        }
    }
}

