using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Noterium.Core.Security;

namespace Noterium.Core.Utilities
{
    public class FileSecurity
    {
        public static string Encrypt(string content, SecureString password)
        {
            return AESGCM.SimpleEncryptWithPassword(content, ConvertToUnSecureString(password));
        }

        public static string Decrypt(string content, SecureString password)
        {
            return AESGCM.SimpleDecryptWithPassword(content, ConvertToUnSecureString(password));
        }

        public static SecureString ConvertToSecureString(string s)
        {
            var secureStr = new SecureString();
            if (s.Length > 0)
            {
                foreach (var c in s.ToCharArray())
                    secureStr.AppendChar(c);
            }
            return secureStr;
        }

        public static string ConvertToUnSecureString(SecureString ss)
        {
            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(ss);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create(); //or use SHA1.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            var sb = new StringBuilder();
            foreach (var b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}