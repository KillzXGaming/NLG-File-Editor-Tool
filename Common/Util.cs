using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLevelLibrary.Files.Common
{
    internal class Util
    {
        public static List<int> GetBitIndices(uint value)
        {
            List<int> indices = new List<int>();
            for (short i = 0; i < 32; i++)
            {
                //Check if the bit is set to 1 or not for an area present
                if ((value >> i & 1) != 0)
                    indices.Add(i);
            }
            return indices;
        }

        public static List<int> GetBitIndices(ulong value)
        {
            List<int> indices = new List<int>();
            for (short i = 0; i < 64; i++)
            {
                //Check if the bit is set to 1 or not for an area present
                if ((value >> i & 1) != 0)
                    indices.Add(i);
            }
            return indices;
        }
    }
}
