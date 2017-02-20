using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Noterium.Core.Helpers
{
    public class FileHelpers
    {
        #region wait for file release

        public static FileStream WaitForFileAccess(string filePath, FileMode fileMode, FileAccess access, FileShare share, TimeSpan timeout)
        {
            int errorCode;
            var start = DateTime.Now;

            while (true)
            {
                var fileHandle = CreateFile(filePath, ConvertFileAccess(access), ConvertFileShare(share), IntPtr.Zero,
                    ConvertFileMode(fileMode), EFileAttributes.Normal, IntPtr.Zero);

                if (!fileHandle.IsInvalid)
                {
                    return new FileStream(fileHandle, access);
                }

                errorCode = Marshal.GetLastWin32Error();

                if (errorCode != ERROR_SHARING_VIOLATION)
                {
                    break;
                }

                if ((DateTime.Now - start) > timeout)
                {
                    return null; // timeout isn't an exception
                }

                Thread.Sleep(100);
            }

            throw new IOException(new Win32Exception(errorCode).Message, errorCode);
        }

        private const int ERROR_SHARING_VIOLATION = 32;

        [Flags]
        private enum EFileAccess : uint
        {
            GenericRead = 0x80000000,
            GenericWrite = 0x40000000
        }

        [Flags]
        private enum EFileShare : uint
        {
            None = 0x00000000
        }

        private enum ECreationDisposition : uint
        {
            /// <summary>
            ///     Creates a new file. The function fails if a specified file exists.
            /// </summary>
            New = 1,

            /// <summary>
            ///     Creates a new file, always.
            ///     If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file
            ///     attributes,
            ///     and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES
            ///     structure specifies.
            /// </summary>
            CreateAlways = 2,

            /// <summary>
            ///     Opens a file. The function fails if the file does not exist.
            /// </summary>
            OpenExisting = 3,

            /// <summary>
            ///     Opens a file, always.
            ///     If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
            /// </summary>
            OpenAlways = 4,

            /// <summary>
            ///     Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
            ///     The calling process must open the file with the GENERIC_WRITE access right.
            /// </summary>
            TruncateExisting = 5
        }

        [Flags]
        private enum EFileAttributes : uint
        {
            Normal = 0x00000080
        }

        private static EFileAccess ConvertFileAccess(FileAccess access)
        {
            return access == FileAccess.ReadWrite ? EFileAccess.GenericRead | EFileAccess.GenericWrite : access == FileAccess.Read ? EFileAccess.GenericRead : EFileAccess.GenericWrite;
        }

        private static EFileShare ConvertFileShare(FileShare share)
        {
            return (EFileShare)((uint)share);
        }

        private static ECreationDisposition ConvertFileMode(FileMode mode)
        {
            return mode == FileMode.Open ? ECreationDisposition.OpenExisting : mode == FileMode.OpenOrCreate ? ECreationDisposition.OpenAlways : (ECreationDisposition)(uint)mode;
        }

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            EFileAccess dwDesiredAccess,
            EFileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            ECreationDisposition dwCreationDisposition,
            EFileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        #endregion

    }
}