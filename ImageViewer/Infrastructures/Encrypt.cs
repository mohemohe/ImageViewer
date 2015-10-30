using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ImageViewer.Infrastructures
{
    public static class Encrypt
    {
        private static string GenerateKey()
        {
            var systemInfo = new List<string>
            {
                Environment.OSVersion.Platform.ToString(),
                Environment.Is64BitOperatingSystem ? "x64" : "x86",
                Environment.ProcessorCount.ToString(),
                Environment.MachineName,
                Environment.UserName
            };

            var keyBase = new StringBuilder();
            systemInfo.ForEach(x =>
            {
                var b = Encoding.UTF8.GetBytes(x);
                var sha512 = SHA512.Create();
                var hash = sha512.ComputeHash(b);
                hash.ToList().ForEach(y => keyBase.Append(y.ToString("x2")));
            });

            return keyBase.ToString();
        }

        private static string GenerateKey(string str)
        {
            var keyBase = new StringBuilder();
            var b = Encoding.UTF8.GetBytes(str);
            var sha512 = SHA512.Create();
            var hash = sha512.ComputeHash(b);
            hash.ToList().ForEach(y => keyBase.Append(y.ToString("x2")));

            return keyBase.ToString();
        }

        public static string EncryptString(string rawString)
        {
            var src = Encoding.UTF8.GetBytes(rawString);

            using (var aes = new AesCryptoServiceProvider
            {
                BlockSize = 128,
                KeySize = 128,
                IV = GenerateIvKey(512, 512),
                Key = GenerateIvKey(2048, 2048),
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            using (var enc = aes.CreateEncryptor())
            {
                var dest = enc.TransformFinalBlock(src, 0, src.Length);
                return Convert.ToBase64String(dest);
            }
        }

        public static string DecryptString(string encryptedString)
        {
            var src = Convert.FromBase64String(encryptedString);

            using (var aes = new AesCryptoServiceProvider
            {
                BlockSize = 128,
                KeySize = 128,
                IV = GenerateIvKey(512, 512),
                Key = GenerateIvKey(2048, 2048),
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            using (var dec = aes.CreateDecryptor())
            {
                var dest = dec.TransformFinalBlock(src, 0, src.Length);
                return Encoding.UTF8.GetString(dest);
            }
        }

        private static byte[] GenerateIvKey(int saltIterations, int passwordIterations, int deriveBytesIterations = 2048)
        {
            var saltBase = GenerateKey();
            for (var i = 1; i < saltIterations; i++)
            {
                saltBase = GenerateKey(saltBase);
            }
            var salt = Encoding.UTF8.GetBytes(saltBase);

            var pass = GenerateKey();
            for (var i = 1; i < passwordIterations; i++)
            {
                pass = GenerateKey(pass);
            }

            using (var deriveBytes = new Rfc2898DeriveBytes(pass, salt))
            {
                deriveBytes.IterationCount = deriveBytesIterations;
                return deriveBytes.GetBytes(128/8);
            }
        }
    }
}