
namespace Services.Scramblers
{
    public class ScramblerBlockPermutation : IScrambler
    {
        private int _blockSize = 2;

        //public ScramblerBlockPermutation(int blockSize)
        //{
        //    _blockSize = blockSize;
        //}

        public byte[] Scramble(byte[] data, byte[] key)
        {
            if (data == null || data.Length == 0) return data;

            byte[] result = new byte[data.Length];
            int blocksCount = (int)Math.Ceiling((double)data.Length / _blockSize);

            for (int blockIndex = 0; blockIndex < blocksCount; blockIndex++)
            {
                int start = blockIndex * _blockSize;
                int end = Math.Min(start + _blockSize, data.Length);
                int blockLength = end - start;

                byte[] block = data.Skip(start).Take(blockLength).ToArray();
                byte[] permutationKey = GeneratePermutationKey(key, blockLength);
                byte[] scrambledBlock = PermuteBlock(block, permutationKey);

                for (int i = 0; i < blockLength; i++)
                {
                    result[start + i] = scrambledBlock[i];
                }
            }
            return result;
        }

        public byte[] Descramble(byte[] data, byte[] key)
        {
            if (data == null || data.Length == 0) return data;
            byte[] result = new byte[data.Length];
            int blocksCount = (int)Math.Ceiling((double)data.Length / _blockSize);

            for (int blockIndex = 0; blockIndex < blocksCount; blockIndex++)
            {
                int start = blockIndex * _blockSize;
                int end = Math.Min(start + _blockSize, data.Length);
                int blockLength = end - start;

                byte[] block = data.Skip(start).Take(blockLength).ToArray();
                byte[] permutationKey = GeneratePermutationKey(key, blockLength);
                byte[] descrambledBlock = PermuteBlock(block, InversePermutationKey(permutationKey));


                for (int i = 0; i < blockLength; i++)
                {
                    result[start + i] = descrambledBlock[i];
                }
            }
            return result;
        }

        private byte[] GeneratePermutationKey(byte[] key, int blockLength)
        {
            var permutationKey = key.Select(x => (byte)(x % blockLength)).ToArray();
            if (permutationKey.Length < blockLength)
            {
                var extendedKey = new byte[blockLength];
                for (int i = 0; i < blockLength; i++)
                {
                    extendedKey[i] = permutationKey[i % permutationKey.Length];
                }
                return extendedKey;
            }
            return permutationKey;
        }

        private byte[] PermuteBlock(byte[] block, byte[] permutationKey)
        {
            try
            {

                byte[] scrambledBlock = new byte[block.Length];
                for (int i = 0; i < block.Length; i++)
                {
                    scrambledBlock[i] = block[permutationKey[i]];
                }
                return scrambledBlock;
            }
            catch (Exception ex)
            {
                return block;
            }
        }

        private byte[] InversePermutationKey(byte[] permutationKey)
        {
            int length = permutationKey.Length;
            byte[] inverseKey = new byte[length];

            for (int i = 0; i < length; i++)
            {
                inverseKey[i] = 0xff;
            }

            for (int i = 0; i < length; i++)
            {
                if (permutationKey[i] < length)
                {
                    if (inverseKey[permutationKey[i]] == 0xff)
                        inverseKey[permutationKey[i]] = (byte)i;
                }
            }


            for (int i = 0; i < length; i++)
            {
                if (inverseKey[i] == 0xff)
                {
                    for (int j = 0; j < length; j++)
                    {
                        if (permutationKey[j] == i)
                        {
                            inverseKey[i] = (byte)j;
                            break;
                        }
                    }
                }
            }

            return inverseKey;
        }
    }
}
