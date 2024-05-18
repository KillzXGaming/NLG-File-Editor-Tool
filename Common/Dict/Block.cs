using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.AccessControl;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace NextLevelLibrary
{
    /// <summary>
    /// A block which can decompress part of the .data file for later loading chunk data.
    /// </summary>
    public class Block 
    {
        /// <summary>
        /// The raw data in the block. Compressed if the dictionary uses block compression.
        /// </summary>
        public Stream Data { get; set; }

        /// <summary>
        /// The parent dictionary file that the block uses.
        /// </summary>
        public IDictionaryData Dictionary { get; set; }

        /// <summary>
        /// The offset of the data.
        /// </summary>
        public uint Offset;

        /// <summary>
        /// The decompressed size of the data.
        /// </summary>
        public uint DecompressedSize;

        /// <summary>
        /// The compressed size of the data.
        /// </summary>
        public uint CompressedSize;

        /// <summary>
        /// The index of the block in the dictionary.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The flags of the block.
        /// </summary>
        public uint Flags;

        public Block(IDictionaryData dict, int index)
        {
            Dictionary = dict;
            Index = index;
        }

        public bool IsZSTDCompressed = false;

        /// <summary>
        /// The index of the data source file that gets the file extension.
        /// Most external sources may not exist for debug purposes and can be skipped
        /// </summary>
        public byte SourceIndex { get; set; }

        /// <summary>
        /// The detected resource type. Can either be a type of table or raw data.
        /// </summary>
        public ResourceType SourceType { get; set; }

        /// <summary>
        /// The file extension to determine what external file to load data on.
        /// Will always be .data. The .debug files are unused.
        /// </summary>
        public string FileExtension { get; set; }

        public Stream Compress(Stream decompressed)
        {
            if (Dictionary == null) return new MemoryStream();

            CompressedSize = (uint)decompressed.Length;
            DecompressedSize = (uint)decompressed.Length;

            if (Dictionary.BlocksCompressed)
            {
                var comp = new MemoryStream(CompressZLIB(decompressed.ToArray()));
                CompressedSize = (uint)comp.Length;
                return comp;
            }
            else
                return decompressed;
        }

        public byte[] CompressZLIB(byte[] input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (var zlibStream = new DeflaterOutputStream(ms, new Deflater(Deflater.DEFAULT_COMPRESSION, false)))
                {
                    zlibStream.Write(input, 0, input.Length);
                    zlibStream.Finish();
                }
                return ms.ToArray();
            }
        }

        public virtual Stream Decompress(Stream compressed)
        {
            using (var reader = new FileReader(compressed, true))
            {
                if (Offset > reader.BaseStream.Length || DecompressedSize == 0)
                    return new MemoryStream();

                reader.SeekBegin(Offset);

                //Check the dictionary if the files are compressed
                if (Dictionary.BlocksCompressed)
                {
                    //Check the magic to see if it's zlib compression
                    ushort Magic = reader.ReadUInt16();
                    bool IsZLIP = Magic == 0x9C78 || Magic == 0xDA78;
                    reader.SeekBegin(Offset);

                    if (IsZLIP)
                    {
                        return new MemoryStream(STLibraryCompression.ZLIB.Decompress(
                              reader.ReadBytes((int)CompressedSize)));
                    }
                    else //Unknown compression so skip it.
                        return new MemoryStream();
                } //File is decompressed so check if it's in the range of the current data file.
                else if (Offset + DecompressedSize <= reader.BaseStream.Length)
                    return new SubStream(reader.BaseStream, Offset, DecompressedSize);
            }
            return new MemoryStream();
        }

        public byte[] ReadBytes()
        {
            using (var reader = new FileReader(Data, true))
            {
                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }

        public bool IsZLIB()
        {
            using (var reader = new FileReader(Data, true))
            {
                ushort Magic = reader.ReadUInt16();
                bool IsZLIP = Magic == 0x9C78 || Magic == 0xDA78;
                return IsZLIP;
            }
        }
    }
}