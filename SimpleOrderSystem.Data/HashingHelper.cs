using System.Security.Cryptography;

namespace SimpleOrderSystem.Data
{
    public static class HashingHelper
    {
        private const int Iterations = 210000;

        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            passwordSalt = RandomNumberGenerator.GetBytes(32);
            passwordHash = Rfc2898DeriveBytes.Pbkdf2(password, passwordSalt, Iterations, HashAlgorithmName.SHA256, 32);
        }

        public static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            if (string.IsNullOrEmpty(password) || passwordHash == null || passwordSalt == null ||
                passwordHash.Length != 32 || passwordSalt.Length != 32)
            {
                return false;
            }

            var hash = Rfc2898DeriveBytes.Pbkdf2(password, passwordSalt, Iterations, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(hash, passwordHash);
        }
    }
}
