using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace FileTracker.Models
{
    public static class Common
    {
        public static readonly string SnapFolder = @".snap\";
        private static readonly Encoding Encoder = Encoding.UTF8;
        private static readonly HashAlgorithm HashGenerator = new SHA1Cng();

        public static string GetHash(string input)
        {
            byte[] data = Encoder.GetBytes(input);
            byte[] result = HashGenerator.ComputeHash(data);
            var sb = new StringBuilder(result.Length * 2);
            foreach (byte b in result)
                sb.AppendFormat("{0:x2}", b);

            return sb.ToString();
        }

        ~Common()
        {
            HashGenerator.Dispose();
        }
    }
}
