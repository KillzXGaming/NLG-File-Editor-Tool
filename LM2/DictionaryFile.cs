using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Toolbox.Core;
using Toolbox.Core.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace NextLevelLibrary.LM2
{
    public class DictionaryFile : IDictionaryData
    {
        /// <summary>
        /// Data referred to the file table.
        /// This determines what blocks to use for the table.
        /// </summary>
        public class FileTableReference
        {
            //Standard or debug
            public uint Hash;
            //The blocks to index to grab data from the file table
            public byte[] BlockIndices = new byte[8] { 2, 3, 4, 5, 0, 0, 0, 0 };
        }

        public IEnumerable<Block> BlockList => Blocks;
        public bool BlocksCompressed => IsCompressed;

        //List of file table references
        public List<FileTableReference> FileTableReferences = new List<FileTableReference>();
        //List of blocks to get blobs of raw data
        public List<Block> Blocks = new List<Block>();
        //List of strings used for external file extensions
        public List<string> StringList = new List<string>();

        public ushort HeaderFlags;
        public bool IsCompressed = false;

        public string FilePath { get; set; }

        byte[] Unknowns { get; set; }

        public DictionaryFile(Stream stream)
        {
            using (var reader = new FileReader(stream))
            {
                uint Identifier = reader.ReadUInt32();
                HeaderFlags = reader.ReadUInt16();
                IsCompressed = reader.ReadByte() == 1;
                reader.ReadByte(); //Padding
                uint numFiles = reader.ReadUInt32();
                uint SizeLargestFile = reader.ReadUInt32();
                byte FileTableCount = reader.ReadByte();
                reader.ReadByte(); //Padding
                byte numFileTableReferences = reader.ReadByte();
                byte numStrings = reader.ReadByte();
                for (int i = 0; i < numFileTableReferences; i++)
                    FileTableReferences.Add(new FileTableReference()
                    {
                        Hash = reader.ReadUInt32(),
                        BlockIndices = reader.ReadBytes(8),
                    });
                Unknowns = reader.ReadBytes((int)numFiles);
                for (int i = 0; i < numFiles; i++)
                {
                    Blocks.Add(new Block(this, i)
                    {
                        Offset = reader.ReadUInt32(),
                        DecompressedSize = reader.ReadUInt32(),
                        CompressedSize = reader.ReadUInt32(),
                        Flags = reader.ReadUInt32(),
                    });

                    //Handle the flags
                    uint resourceFlag = Blocks[i].Flags & 0xFF;
                    uint resourceFlag2 = Blocks[i].Flags >> 24 & 0xFF;
                    uint resourceIndex = Blocks[i].Flags >> 16 & 0xFF;

                    //This determines a file table
                    if (resourceFlag == 0x08 && resourceFlag2 == 1)
                        Blocks[i].SourceType = ResourceType.TABLE;
                    else if (resourceFlag != 0)
                        Blocks[i].SourceType = ResourceType.DATA;

                    //The source index determines which external file to use
                    Blocks[i].SourceIndex = (byte)resourceIndex;
                }
                for (int i = 0; i < numStrings; i++)
                    StringList.Add(reader.ReadZeroTerminatedString());

                for (int i = 0; i < numFiles; i++)
                    Blocks[i].FileExtension = StringList[Blocks[i].SourceIndex];
            }
        }

        public void Save(Stream stream)
        {
            using (var writer = new FileWriter(stream))
            {
                writer.SetByteOrder(false);
                writer.Write(0xA9F32458);
                writer.Write(HeaderFlags);
                writer.Write(IsCompressed);
                writer.Write((byte)0); //padding
                writer.Write(Blocks.Count);
                long maxValuePos = writer.Position;
                if (IsCompressed)
                    writer.Write(Blocks.Max(x => x.CompressedSize));
                else
                    writer.Write((uint)0);
                writer.Write((byte)1);
                writer.Write((byte)0);
                writer.Write((byte)FileTableReferences.Count);
                writer.Write((byte)StringList.Count);
                foreach (var info in FileTableReferences)
                {
                    writer.Write(info.Hash);
                    writer.Write(info.BlockIndices);
                }
                writer.Write(Unknowns);
                for (int i = 0; i < Blocks.Count; i++)
                {
                    writer.Write(Blocks[i].Offset);
                    writer.Write(Blocks[i].DecompressedSize);
                    writer.Write(IsCompressed ? Blocks[i].CompressedSize : 0);
                    writer.Write(Blocks[i].Flags);
                }
                foreach (var str in StringList)
                    writer.WriteString(str);
            }
        }

        public static DataFile ReadDictionaryData(string filePath)
        {
            var dictionary = new DictionaryFile(File.OpenRead(filePath));
            return new DataFile(File.OpenRead(filePath.Replace(".dict", ".data")), dictionary);
        }
    }
}