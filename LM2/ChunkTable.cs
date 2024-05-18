using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Toolbox.Core.IO;
using Toolbox.Core;
using System.Xml.Serialization;

namespace NextLevelLibrary.LM2
{
    public class ChunkTable : IChunkTable
    {
        //The magic identifier for chunk file entries
        private const short ChunkFileIdenfier = 0x1301;

        /// <summary>
        /// The chunk file list that can store sub chunks or raw data.
        /// </summary>
        public List<ChunkFileEntry> Files { get; set; } = new List<ChunkFileEntry>();

        /// <summary>
        /// Data entries of each chunk. These can be separate or data from file chunks
        /// </summary>
        public List<ChunkEntry> DataEntries { get; set; } = new List<ChunkEntry>();

        public List<ChunkEntry> ChunkList { get; set; } = new List<ChunkEntry>();

        public ChunkTable() { }

        public ChunkTable(Stream stream)
        {
            using (var reader = new FileReader(stream, true)) {
                Read(reader);
            }
        }

        void Read(FileReader reader)
        {
            //File is empty so return
            if (reader.BaseStream.Length <= 4)
                return;

            Dictionary<int, ChunkEntry> globalChunkList = new Dictionary<int, ChunkEntry>();

            int globalIndex = 0;

            reader.SetByteOrder(false);
            while (reader.Position <= reader.BaseStream.Length - 12)
            {
                //Read through all sections that use an identifier
                //Determine when a file is used or else using raw data.
                ushort identifier = reader.ReadUInt16();
                if (identifier == ChunkFileIdenfier)
                {
                    //Extra identifier bytes
                    reader.ReadUInt16();

                    ChunkFileEntry entry = new ChunkFileEntry(this);
                    entry.FileHeaderSize = reader.ReadUInt32();
                    entry.FileHeaderOffset = reader.ReadUInt32();
                    entry.FileType = (ChunkFileType)reader.ReadUInt16();
                    entry.Flags = reader.ReadUInt16();
                    entry.ChunkSize = reader.ReadUInt32(); //Child Count or File Size
                    entry.ChunkOffset = reader.ReadUInt32(); //Child Start Index or File Offset     

                    Files.Add(entry);

                    //File entries shift global index by 2
                    globalChunkList.Add(globalIndex++, entry);
                    globalChunkList.Add(globalIndex++, entry);

                    ChunkList.Add(entry);
                }
                else
                {
                    reader.Seek(-2); //Seek back as no identifier is present, data is raw
                    ChunkEntry subEntry = new ChunkEntry(this);
                    subEntry.ChunkType = reader.ReadEnum<ChunkDataType>(false);
                    subEntry.Flags = reader.ReadUInt16();
                    subEntry.ChunkSize = reader.ReadUInt32();
                    subEntry.ChunkOffset = reader.ReadUInt32();

                    DataEntries.Add(subEntry);
                    ChunkList.Add(subEntry);

                    globalChunkList.Add(globalIndex, subEntry);
                    globalIndex += 1;
                }
            }

            for (int i = 0; i < DataEntries.Count; i++)
            {
                if (DataEntries[i].HasChildren)
                {
                    for (int f = 0; f < DataEntries[i].ChunkSize; f++)
                    {
                        DataEntries[i].AddChild((ChunkEntry)globalChunkList[(int)DataEntries[i].ChunkOffset + f]);
                    }
                }
            }

            for (int i = 0; i < Files.Count; i++)
            {
                if (Files[i].HasChildren)
                {
                    for (int f = 0; f < Files[i].ChunkSize; f++)
                    {
                        Files[i].AddChild((ChunkEntry)globalChunkList[(int)Files[i].ChunkOffset + f]);
                    }
                }
            }
        }

        public void SaveData(DictionaryFile dict)
        {
            var blocks = dict.Blocks.ToList();
            //save all the target blocks used
            var table_info = dict.FileTableReferences[0];
            //Compress all the used blocks
            for (int i = 0; i < table_info.BlockIndices.Length; i++)
            {
                var block_index = table_info.BlockIndices[i];
                if (block_index == 0)
                    continue;

                //Write all file data to the block that index the target
                var block_saved = WriteBlock(i);
                blocks[block_index].Data = blocks[block_index].Compress(block_saved);
            }
            //Finally write chunk table
            var chunkTable = WriteChunkTable();

            //Apply chunk table last
            blocks[0].Data = blocks[0].Compress(chunkTable);
        }

        private Stream WriteBlock(int id)
        {
            var mem = new MemoryStream();
            using (var writer = new FileWriter(mem))
            {
                writer.SetByteOrder(false);

                void WriteChunkData(ChunkEntry chunk)
                {
                    if (!DataEntries.Contains(chunk)) DataEntries.Add(chunk);

                    //Ensure the chunk uses the block to save
                    if (chunk.BlockIndex != id)
                        return;

                    uint prev = chunk.ChunkOffset;
                    uint size = chunk.ChunkSize;

                    if (chunk.AlignBytes)
                        writer.Align(8);
                    else
                        writer.Align(4);

                    chunk.ChunkOffset = (uint)writer.BaseStream.Position;
                    chunk.ChunkSize = 0;

                    if (chunk.Data != null)
                    {
                        chunk.ChunkSize = (uint)chunk.Data.Length;
                        writer.Write(chunk.Data.ReadAllBytes());
                    }

                    Console.WriteLine($"Saving Data_{id} {chunk.ChunkType} prev {prev} new {chunk.ChunkOffset} size {chunk.ChunkSize}");
                }

                for (int i = 0; i < ChunkList.Count; i++)
                {
                    var chunkEntry = ChunkList[i];
                    if (chunkEntry.Parent != null)
                        continue;

                    if (id == 0 && chunkEntry is ChunkFileEntry)
                    {
                        var file = chunkEntry as ChunkFileEntry;

                        writer.Align(4);
                        file.FileHeaderOffset = (uint)writer.BaseStream.Position;
                        file.FileHeaderSize = (uint)file.FileHeader.Length;
                        writer.Write(file.FileHeader.ReadAllBytes());
                      //  Console.WriteLine($"Saving File {file.FileType} {file.FileHeaderOffset} - {file.FileHeaderSize}");
                    }
                    foreach (var chunk in chunkEntry.Children)
                    {
                        if (!chunk.HasChildren)
                            WriteChunkData(chunk);
                        else
                        {
                            foreach (var s in chunk.Children)
                                WriteChunkData(s);
                        }
                    }
                    if (!chunkEntry.HasChildren)
                        WriteChunkData(chunkEntry);
                }
            }
            return new MemoryStream(mem.ToArray());
        }

        private Stream WriteChunkTable()
        {
            var mem = new MemoryStream();
            using (var writer = new FileWriter(mem))
            {
                writer.SetByteOrder(false);

                //Update indices if child
                List<ChunkEntry> subEntires = new List<ChunkEntry>();
                List<ChunkEntry> rawEntires = new List<ChunkEntry>();
                List<ChunkEntry> prevEntires = DataEntries.ToList();

                var count = DataEntries.Count;

                int childStartIndex = 0;

                void SetupChildren(ChunkEntry chunk)
                {
                    Console.WriteLine($"PREV {chunk.ChunkOffset} {chunk.ChunkSize}");

                    chunk.ChunkOffset = (uint)(childStartIndex);
                    chunk.ChunkSize = (uint)chunk.Children.Count;

                    Console.WriteLine($"SetupChildren {chunk.ChunkOffset} {chunk.ChunkSize}");

                    foreach (var child in chunk.Children)
                        subEntires.Add(child);

                    childStartIndex += chunk.Children.Count;

                    foreach (var child in chunk.Children)
                    {
                        if (child.HasChildren)
                            SetupChildren(child);
                    }
                }

                foreach (var file in this.ChunkList)
                {
                    if (file.Parent == null)
                    {
                        subEntires.Add(file);

                        if (file is ChunkFileEntry)
                            childStartIndex += 2;
                        else
                            childStartIndex++;
                    }
                }

                foreach (var file in this.ChunkList)
                {
                    if (file.Parent != null)
                        continue;

                    if (file.HasChildren)
                        SetupChildren(file);
                }

                foreach (var chunk in subEntires)
                {
                    if (chunk is ChunkFileEntry)    
                    {
                        var file = (ChunkFileEntry)chunk;
                        writer.Write(ChunkFileIdenfier);
                        writer.Write((ushort)512);
                        writer.Write(file.FileHeaderSize);
                        writer.Write(file.FileHeaderOffset);
                        writer.Write((ushort)file.FileType);
                        writer.Write((ushort)file.Flags);
                        writer.Write(file.ChunkSize);
                        writer.Write(file.ChunkOffset);
                    }
                    else
                    {
                        writer.Write((ushort)chunk.ChunkType);
                        writer.Write((ushort)chunk.Flags);
                        writer.Write(chunk.ChunkSize);
                        writer.Write(chunk.ChunkOffset);
                    }
                }
            }
            return new MemoryStream(mem.ToArray());
        }
    }
}
