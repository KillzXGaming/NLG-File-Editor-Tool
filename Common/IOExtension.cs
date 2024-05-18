using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.IO;

namespace NextLevelLibrary
{
    public static class IOExtension
    {
        public static HashString ReadHash(this FileReader reader)
        {
            return new HashString(reader.ReadUInt32());
        }
    }
}
