using System.Security.Cryptography;
using System.Text;

namespace Noterium.Core.Helpers
{
    public static class StringHelper
    {
        public static string GetMD5Hash(this string s)
        {
            return CalculateMD5Hash(s);
        }

        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            var md5 = MD5.Create();

            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();

            foreach (byte t in hash)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}