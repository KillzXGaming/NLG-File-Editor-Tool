using NextLevelLibrary.LM2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLevelLibrary
{
    public interface IChunkTable
    {
        /// <summary>
        /// The chunk file list that can store sub chunks or raw data.
        /// </summary>
        List<ChunkFileEntry> Files { get; set; } 

        /// <summary>
        /// Data entries of each chunk. These can be seperate or data from file chunks
        /// </summary>
        List <ChunkEntry> DataEntries { get; set; }

        void SaveData(DictionaryFile dict);
    }
}
