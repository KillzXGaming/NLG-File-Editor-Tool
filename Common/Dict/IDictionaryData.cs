using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core;

namespace NextLevelLibrary
{
    /// <summary>
    /// Represents a dictionary that lookup block data in a binary .data file.
    /// </summary>
    public interface IDictionaryData
    {
        /// <summary>
        /// A list of blocks that store compressed data.
        /// </summary>
        IEnumerable<Block> BlockList { get; }

        /// <summary>
        /// The current path of the dictionary file.
        /// </summary>
        string FilePath { get; set; }

        /// <summary>
        /// Determines if the block data is compressed with zlib compression.
        /// </summary>
        bool BlocksCompressed { get; }
    }
}