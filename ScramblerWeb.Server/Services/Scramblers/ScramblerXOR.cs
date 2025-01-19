using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Scramblers
{
    public class ScramblerXOR : IScrambler
    {
        public byte[] Scramble(byte[] data, byte[] key)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (key == null || key.Length == 0)
            {
                return data;
            }

            byte[] scrambledData = new byte[data.Length];

            int keyIndex = 0;
            for (int i = 0; i < data.Length; i++)
            {
                scrambledData[i] = (byte)(data[i] ^ key[keyIndex]);
                keyIndex = (keyIndex + 1) % key.Length;
            }

            return scrambledData;
        }
        public byte[] Descramble(byte[] data, byte[] key)
        {
            return Scramble(data, key);
        }

    }
}
