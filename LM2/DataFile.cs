using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Toolbox.Core.IO;
using Toolbox.Core;

namespace NextLevelLibrary.LM2
{
    public class DataFile
    {
        public ChunkTable Table => Tables[0];

        /// <summary>
        /// Represents a list of file tables used to store file and data info.
        /// </summary>
        public List<ChunkTable> Tables = new List<ChunkTable>();

        /// <summary>
        /// The dictionary used for block handling.
        /// </summary>
        public IDictionaryData Dictionary { get; set; }

        private Stream _stream;

        public DataFile(Stream stream, DictionaryFile dict, int starting_block = 0)
        {
            _stream = stream;
            Dictionary = dict;

            //only use first table 
            //LM2 only uses 2, one for normal usage, one for debugging
            //Other games use this for overriding localization data
            var table_info = dict.FileTableReferences[0]; 
            //block list from dict
            var blocks = dict.BlockList.ToList();

            //Init all the blocks used into usable decompressed streams
            //The table info contains all the block indices used for the file table
            //starting_block is used as there can be groups of multiple file tables used (ie cutscene data)
            Stream[] data_streams = new Stream[table_info.BlockIndices.Length];
            for (int i = 0; i < table_info.BlockIndices.Length; i++)
            {
                //block not used, skip
                if (table_info.BlockIndices[i] == 0)
                    continue;

                data_streams[i] = blocks[starting_block + table_info.BlockIndices[i]].Decompress(stream);
            }

            var table = new ChunkTable(blocks[starting_block].Decompress(stream));
            SetupTableData(table, data_streams);
            Tables.Add(table);
        }

        private void SetupTableData(ChunkTable table, Stream[] dataStreams)
        {
            for (int i = 0; i < table.Files.Count; i++)
            {
                var file = table.Files[i];
                file.FileHeader = new SubStream(dataStreams[0], file.FileHeaderOffset, file.FileHeaderSize);
                ParseFileHeaders(file.FileHeader, file);

                if (!file.HasChildren)
                    file.Data = new SubStream(dataStreams[file.BlockIndex], file.ChunkOffset, file.ChunkSize);
            }

            foreach (var entry in table.DataEntries)
            {
                if (!entry.HasChildren)
                    entry.Data = new SubStream(dataStreams[entry.BlockIndex], entry.ChunkOffset, entry.ChunkSize);
            }
        }

        public void Save(string filePath)
        {
            //Save loaded data
            foreach (var file in Table.Files)
                file.Save();
            //Save data file
            this.Save(filePath, this.Dictionary);
            //Save dictionary
            string dictPath = filePath.Replace(".data", ".dict");
            using (var fileStream = new FileStream(dictPath, FileMode.Create, FileAccess.Write)) {
                ((LM2.DictionaryFile)Dictionary).Save(fileStream);
            }
        }

        public void Save(string filePath, IDictionaryData dictionary)
        {
            var dict = dictionary as DictionaryFile;

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (var writer = new FileWriter(fileStream))
                {
                    List<uint> offsets = new List<uint>();

                    //Compress and save block data from chunk table
                    Table.SaveData(dict);

                    foreach (var block in dict.Blocks)
                    {
                        //Ensure the block has data, not already written, and comes from .data source
                        if (block.Data == null || offsets.Contains(block.Offset) || block.SourceIndex != 0)
                            continue;

                        block.CompressedSize = (uint)block.Data.Length;
                        block.Offset = (uint)writer.Position;

                        writer.Write(block.ReadBytes());
                        writer.Align(8);

                        offsets.Add(block.Offset);
                    }
                }
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _stream?.Close();
            _stream = null;
        }

        private void ParseFileHeaders(Stream stream, ChunkFileEntry chunkEntry)
        {
            using (var reader = new FileReader(stream, true))
            {
                if (chunkEntry.FileHeaderSize >= 8)
                {
                    chunkEntry.Magic = reader.ReadUInt32();
                    chunkEntry.Hash = reader.ReadUInt32();
                }
            }
        }

        public void Extract(string folder)
        {
            foreach (var file in this.Table.Files)
            {
                string sub_folder = Path.Combine(folder, file.FileType.ToString());
                string file_path = Path.Combine(sub_folder, Hashing.CreateHashString(file.Hash));
                string dir = Path.GetDirectoryName(file_path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                file.Export(file_path);
            }
        }
    }
}
