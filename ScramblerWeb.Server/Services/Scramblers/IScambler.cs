using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Services.Scramblers
{
    public interface IScrambler
    {
        byte[] Scramble(byte[] data, byte[] key);
        byte[] Descramble(byte[] data, byte[] key);
    }
}
