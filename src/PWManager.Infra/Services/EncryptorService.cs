using System.Security.Cryptography;

namespace PWManager.Infra.Services
{
    public static class EncryptorService
    {
        public static string EncryptorPassword { get; set; }

        public static string Encrypt(string text)
        {
            byte[] iv = GenerateSalt(16);
            byte[] salt = GenerateSalt(16);
            byte[] key = GetKey(salt);
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(text);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(iv) + ":" + Convert.ToBase64String(array);
        }

        public static string Decrypt(string cipherText)
        {
            string[] parts = cipherText.Split(':');
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] iv = Convert.FromBase64String(parts[1]);
            byte[] buffer = Convert.FromBase64String(parts[2]);

            byte[] key = GetKey(salt);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        private static byte[] GetKey(byte[] salt, int keySize = 256)
        {
            const int Iterations = 1000;
            var keyGenerator = new Rfc2898DeriveBytes(EncryptorPassword, salt, Iterations);
            return keyGenerator.GetBytes(keySize / 8);
        }

        private static byte[] GenerateSalt(int size)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var salt = new byte[size];
                rng.GetBytes(salt);
                return salt;
            }
        }
    }
}

