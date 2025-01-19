using System.CodeDom.Compiler;
using System.Security.Cryptography;

namespace Services.Keys
{
    public class KeyGenerator : IKeyGenerator
    {
        public string Generate(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be a positive number.");
            }

            byte[] randomBytes = new byte[length];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            return string.Concat(randomBytes.Select(b => b.ToString("x2")));
        }
    }
}
