using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLevelLibrary
{
    public class HashString
    {
        public uint Hash;

        public string String
        {
            get { return Hashing.CreateHashString(Hash); }
            set { Hash = Hashing.StringToHash(value); }
        }

        public HashString() { }

        public HashString(uint hash)
        {
            Hash = hash;
        }

        public override string ToString() => String;
    }
}
