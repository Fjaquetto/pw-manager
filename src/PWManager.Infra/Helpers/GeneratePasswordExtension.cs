using System.Security.Cryptography;

namespace PWManager.Infra.Helpers
{
    public static class GeneratePasswordExtension
    {
        private const int MinimumPasswordLength = 4;
        private const int RandomByteSize = 4;
        private static readonly char[] AvailableCharacters;

        static GeneratePasswordExtension()
        {
            AvailableCharacters = GenerateAvailableCharacters();
        }

        public static string Generate(int length)
        {
            if (length < MinimumPasswordLength)
                throw new ArgumentException($"Password length should be at least {MinimumPasswordLength} characters");
            var password = new char[length];

            using (var random = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < length; i++)
                {
                    password[i] = GetRandomCharacter(random);
                }
            }

            return new string(password);
        }

        private static char GetRandomCharacter(RandomNumberGenerator random)
        {
            var randomBytes = new byte[RandomByteSize];
            random.GetBytes(randomBytes);

            var index = BitConverter.ToUInt32(randomBytes, 0) % AvailableCharacters.Length;

            return AvailableCharacters[index];
        }

        private static char[] GenerateAvailableCharacters()
        {
            var upperCaseChars = Enumerable.Range('A', 26).Select(x => (char)x);
            var lowerCaseChars = Enumerable.Range('a', 26).Select(x => (char)x);
            var digitChars = Enumerable.Range('0', 10).Select(x => (char)x);
            var specialChars = new[] { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '+', '=', '.', '_', ':', ';', ',' };

            return upperCaseChars
                .Concat(lowerCaseChars)
                .Concat(digitChars)
                .Concat(specialChars)
                .ToArray();
        }
    }
}
