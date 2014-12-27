using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace FileTracker.Models
{
    public static class Common
    {
        public static readonly string SnapFolder = @".snap\";
        public static readonly int HashLength = 7;
        public static readonly string DateFormat = "yyMMddHHmmssff";
        public static readonly Encoding Encoder = Encoding.UTF8;
        private static readonly HashAlgorithm HashGenerator = new SHA1Cng();

        public static string GetHash(string input)
        {
            byte[] data = Encoder.GetBytes(input);
            byte[] result = HashGenerator.ComputeHash(data);
            var sb = new StringBuilder(result.Length * 2);
            foreach (byte b in result)
                sb.AppendFormat("{0:x2}", b);

            return sb.ToString().Substring(0, HashLength);
        }

        /// <summary>
        /// 指定のフォルダにおけるスナップファイルの格納先パスを返します。パスの末尾は"\"となります。
        /// </summary>
        /// <param name="parentFolder">スナップファイルの格納先を取得するフォルダパス</param>
        /// <returns>スナップファイルの格納先フォルダパス</returns>
        public static string GetSnapFolder(string parentFolder)
        {
            return parentFolder + (parentFolder.Substring(parentFolder.Length - 1, 1) == @"\" ? "" : @"\") + SnapFolder;
        }

        public static void CreateSnapFolder(string baseFolder)
        {
            var dir = Directory.CreateDirectory(GetSnapFolder(baseFolder));
            dir.Attributes |= FileAttributes.Hidden;
        }
    }
}
