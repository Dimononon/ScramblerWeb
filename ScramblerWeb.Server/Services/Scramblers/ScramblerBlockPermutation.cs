
namespace Services.Scramblers
{
    public class ScramblerBlockPermutation : IScrambler
    {
        public byte[] Scramble(byte[] data, byte[] key)
        {
            if (data == null || data.Length == 0) return data;

            int blockSize = Math.Min(64, data.Length);
            byte[] result = new byte[data.Length];
            int blocksCount = (int)Math.Ceiling((double)data.Length / blockSize);

            for (int blockIndex = 0; blockIndex < blocksCount; blockIndex++)
            {
                int start = blockIndex * blockSize;
                int end = Math.Min(start + blockSize, data.Length);
                int blockLength = end - start;

                byte[] block = data.Skip(start).Take(blockLength).ToArray();
                byte[] permutationKey = GeneratePermutationKey(key, blockLength);
                byte[] scrambledBlock = PermuteBlock(block, permutationKey);

                Array.Copy(scrambledBlock, 0, result, start, blockLength);
            }
            return result;
        }

        public byte[] Descramble(byte[] data, byte[] key)
        {
            if (data == null || data.Length == 0) return data;

            int blockSize = Math.Min(64, data.Length);
            byte[] result = new byte[data.Length];
            int blocksCount = (int)Math.Ceiling((double)data.Length / blockSize);

            for (int blockIndex = 0; blockIndex < blocksCount; blockIndex++)
            {
                int start = blockIndex * blockSize;
                int end = Math.Min(start + blockSize, data.Length);
                int blockLength = end - start;

                byte[] block = data.Skip(start).Take(blockLength).ToArray();
                byte[] permutationKey = GeneratePermutationKey(key, blockLength);
                byte[] descrambledBlock = PermuteBlock(block, InversePermutationKey(permutationKey));

                Array.Copy(descrambledBlock, 0, result, start, blockLength);
            }
            return result;
        }

        private byte[] GeneratePermutationKey(byte[] key, int blockLength)
        {
            byte[] permutation = Enumerable.Range(0, blockLength).Select(i => (byte)i).ToArray();

            int seed = key.Sum(b => b);
            Random rng = new Random(seed);

            for (int i = blockLength - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (permutation[i], permutation[j]) = (permutation[j], permutation[i]);
            }

            return permutation;
        }

        private byte[] PermuteBlock(byte[] block, byte[] permutationKey)
        {
            byte[] scrambledBlock = new byte[block.Length];
            for (int i = 0; i < block.Length; i++)
            {
                scrambledBlock[i] = block[permutationKey[i]];
            }
            return scrambledBlock;
        }

        private byte[] InversePermutationKey(byte[] permutationKey)
        {
            int length = permutationKey.Length;
            byte[] inverseKey = new byte[length];

            for (int i = 0; i < length; i++)
            {
                inverseKey[permutationKey[i]] = (byte)i;
            }

            return inverseKey;
        }
    }
}
