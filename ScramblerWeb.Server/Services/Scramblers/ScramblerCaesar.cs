
namespace Services.Scramblers
{
    public class ScramblerCaesar : IScrambler
    {
        public byte[] Scramble(byte[] data, byte[] key)
        {
            if (data == null || data.Length == 0) return data;

            byte[] result = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                int keyVal = key[i % key.Length] % 256;
                result[i] = (byte)((data[i] + keyVal) % 256);
            }
            return result;
        }

        public byte[] Descramble(byte[] data, byte[] key)
        {
            if (data == null || data.Length == 0) return data;

            byte[] result = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                int keyVal = key[i % key.Length] % 256;
                result[i] = (byte)((data[i] - keyVal + 256) % 256);
            }
            return result;
        }
    }
}
