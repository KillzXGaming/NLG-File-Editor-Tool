using Newtonsoft.Json;
using NextLevelLibrary.LM2;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Toolbox.Core.IO;

namespace NextLevelLibrary
{
    public class ChunkEntry
    {
        /// <summary>
        /// The raw data of the chunk if "HasChildren" is not used.
        /// </summary>
        [XmlIgnore]
        public Stream Data;

        /// <summary>
        /// The chunk type to determine the purpose of the data.
        /// </summary>
        [XmlIgnore]
        public ChunkDataType ChunkType;

        /// <summary>
        /// The chunk flags.
        /// </summary>
        public ushort Flags;

        /// <summary>
        /// The size or sub count of the chunk, depending on the "HasChildren" flag.
        /// </summary>
        public uint ChunkSize;

        /// <summary>
        /// The size or sub index of the chunk, depending on the "HasChildren" flag.
        /// </summary>
        public uint ChunkOffset;

        //Flags

        /// <summary>
        /// </summary>
        public int UnknownFlag1
        {
            get { return BitUtils.GetBits(Flags, 0, 8); }
            set { Flags = (ushort)BitUtils.SetBits((int)Flags, (int)value, 0, 8); }
        }

        /// <summary>
        /// Determines if the raw data is aligned by 16 bytes on save.
        /// This is required for mesh and texture buffer data, and model matrices.
        /// </summary>
        public virtual bool AlignBytes
        {
            get { return BitUtils.GetBit(Flags, 8); }
            set { Flags = (ushort)BitUtils.SetBit((int)Flags, value, 8); }
        }

        /// <summary>
        /// Gets or sets an unknown flag value. Always 1
        /// </summary>
        public bool UnknownValue1
        {
            get { return BitUtils.GetBit(Flags, 9); }
            set { Flags = (ushort)BitUtils.SetBit((int)Flags, value, 9); }
        }

        /// <summary>
        /// Gets or sets an unknown flag value. 0 unless texture data type
        /// </summary>
        public bool TextureDataFlag
        {
            get { return BitUtils.GetBit(Flags, 10); }
            set { Flags = (ushort)BitUtils.SetBit((int)Flags, value, 10); }
        }

        public uint RealBlockIndex = 0;

        /// <summary>
        /// The block index which determines what data block to use right after the table block.
        /// </summary>
        public virtual uint BlockIndex
        {
            get { return (uint)BitUtils.GetBits(Flags, 12, 3); }
            set { Flags = (ushort)BitUtils.SetBits((int)Flags, (int)value, 12, 3); }
        }

        /// <summary>
        /// Determines if the chunk uses child chunks or raw data.
        /// </summary>
        public virtual bool HasChildren
        {
            get { return BitUtils.GetBit(Flags, 15); }
            set { Flags = (ushort)BitUtils.SetBit((int)Flags, value, 15); }
        }

        [XmlIgnore]
        public IChunkTable Table;

        public ChunkEntry() { }

        public ChunkEntry(IChunkTable table) { Table = table; }

        public ChunkEntry(IChunkTable table, ChunkDataType type, ushort flag, bool hasChildren = false)
        {
            Table = table;
            this.Flags = flag;
            this.HasChildren = hasChildren;
            this.Data = new MemoryStream();
        }

        public ChunkEntry(IChunkTable table, ChunkDataType type, bool hasChildren)
        {
            Table = table;
            this.ChunkType = type;
            this.Data = new MemoryStream();
            this.Flags = 4609; //00000010 00000001 to start
            this.TextureDataFlag = false;
            this.UnknownValue1 = true;

            if (type == ChunkDataType.TextureHeader)
                this.TextureDataFlag = false;

            this.HasChildren = hasChildren;
            switch (type)
            {
                case ChunkDataType.TextureData:
                case ChunkDataType.ModelTransform:
                case ChunkDataType.MeshBuffers:
                    AlignBytes = true;
                    this.Flags = 4865;
                    break;
            }
        }

        public virtual void Export(string filePath)
        {
            //readable json export
            ChunkFileEntry.FileExport file = new ChunkFileEntry.FileExport();
            file.FileType = this.ChunkType.ToString();

            List<ChunkFileEntry.SectionExport> ExportChildren(List<ChunkEntry> chunks)
            {
                List<ChunkFileEntry.SectionExport> list = new List<ChunkFileEntry.SectionExport>();

                foreach (var c in chunks)
                {
                    ChunkFileEntry.SectionExport section = new ChunkFileEntry.SectionExport();
                    section.Type = c.ChunkType.ToString();
                    section.SubSections.AddRange(ExportChildren(c.Children));
                    if (c.Data != null)
                        section.Data = c.Data.ToArray();

                    list.Add(section);
                }
                return list;
            };
            file.Sections.AddRange(ExportChildren(this.Children));

            File.WriteAllText(filePath + ".json", JsonConvert.SerializeObject(file, Formatting.Indented,
                new JsonSerializerSettings()
                {

                }));
        }

        /// <summary>
        /// Debugging bit flags
        /// </summary>
        public string FlagBits
        {
            get { return Convert.ToString(Flags, 2).PadLeft(16, '0'); }
            set { }
        }

        [XmlIgnore]
        public ChunkEntry Parent;

        /// <summary>
        /// The children of the chunk if "HasChildren" is used.
        /// </summary>
        [XmlIgnore]
        public List<ChunkEntry> Children = new List<ChunkEntry>();

        public ChunkEntry AddChild(ChunkDataType type, bool hasChildren = false)
        {
            var chunk = new ChunkEntry(Table, type, hasChildren);
            this.Children.Add(chunk);
            chunk.Parent = this;
            return chunk;
        }
        public virtual ChunkEntry AddChild(ChunkDataType type, ushort flags)
        {
            var chunk = new ChunkEntry(Table, type, false);
            chunk.Flags = flags;
            this.Children.Add(chunk);
            chunk.Parent = this;
            return chunk;
        }

        public ChunkEntry AddChild(ChunkEntry chunk)
        {
            this.Children.Add(chunk);
            chunk.Parent = this;
            return chunk;
        }

        /// <summary>
        /// Reads a given structure from the start of the data stream.
        /// </summary>
        public T ReadStruct<T>()
        {
            using (var reader = new FileReader(Data, true))
            {
                return reader.ReadStruct<T>();
            }
        }

        /// <summary>
        /// Reads multiple structures from the start of the data stream.
        /// </summary>
        public List<T> ReadStructs<T>(uint count)
        {
            using (var reader = new FileReader(Data, true))
            {
                return reader.ReadMultipleStructs<T>(count);
            }
        }

        public byte[] ReadBytes(uint count)
        {
            using (var reader = new FileReader(Data, true))
            {
                return reader.ReadBytes((int)count);
            }
        }

        public ushort[] ReadUShortList(uint count)
        {
            ushort[] values = new ushort[(int)count];
            using (var reader = new FileReader(Data, true))
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = reader.ReadUInt16();
            }
            return values;
        }

        public uint[] ReadUint32s(uint count)
        {
            uint[] values = new uint[(int)count];
            using (var reader = new FileReader(Data, true))
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = reader.ReadUInt32();
            }
            return values;
        }

        public Vector3[] ReadVector3List(uint count)
        {
            Vector3[] values = new Vector3[(int)count];
            using (var reader = new FileReader(Data, true))
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = new Vector3(
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            }
            return values;
        }

        public Vector4[] ReadVector4List(uint count)
        {
            Vector4[] values = new Vector4[(int)count];
            using (var reader = new FileReader(Data, true))
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = new Vector4(
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            }
            return values;
        }

        /// <summary>
        /// Writes a given structure from the start of the data stream.
        /// </summary>
        public void WriteStructs<T>(List<T> values)
        {
            var mem = new MemoryStream();
            using (var writer = new FileWriter(mem, true))
            {
                foreach (var item in values)
                    writer.WriteStruct<T>(item);
            }
            Data = mem;
        }

        /// <summary>
        /// Writes a given structure from the start of the data stream.
        /// </summary>
        public void WriteStruct<T>(T value)
        {
            var mem = new MemoryStream();
            using (var writer = new FileWriter(mem, true))
            {
                writer.WriteStruct<T>(value);
            }
            Data = mem;
        }

        /// <summary>
        /// Reads a list of primitive types from the start of the data stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<T> ReadPrimitive<T>(uint count)
        {
            T[] instace = new T[count];
            using (var reader = new FileReader(Data, true))
            {
                for (int i = 0; i < count; i++)
                {
                    object value = null;
                    if (typeof(T) == typeof(uint)) value = reader.ReadUInt32();
                    else if (typeof(T) == typeof(int)) value = reader.ReadInt32();
                    else if (typeof(T) == typeof(short)) value = reader.ReadInt16();
                    else if (typeof(T) == typeof(ushort)) value = reader.ReadUInt16();
                    else if (typeof(T) == typeof(float)) value = reader.ReadSingle();
                    else if (typeof(T) == typeof(bool)) value = reader.ReadBoolean();
                    else if (typeof(T) == typeof(sbyte)) value = reader.ReadSByte();
                    else if (typeof(T) == typeof(byte)) value = reader.ReadByte();
                    else
                        throw new Exception("Unsupported primitive type! " + typeof(T));

                    instace[i] = (T)value;
                }
            }
            return instace.ToList();
        }

         /// <summary>
        /// Reads a list of primitive types from the start of the data stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T ReadPrimitive<T>()
        {
            using (var reader = new FileReader(Data, true))
            {
                object value = null;
                if (typeof(T) == typeof(uint)) value = reader.ReadUInt32();
                else if (typeof(T) == typeof(int)) value = reader.ReadInt32();
                else if (typeof(T) == typeof(short)) value = reader.ReadInt16();
                else if (typeof(T) == typeof(ushort)) value = reader.ReadUInt16();
                else if (typeof(T) == typeof(float)) value = reader.ReadSingle();
                else if (typeof(T) == typeof(bool)) value = reader.ReadBoolean();
                else if (typeof(T) == typeof(sbyte)) value = reader.ReadSByte();
                else if (typeof(T) == typeof(byte)) value = reader.ReadByte();
                else
                    throw new Exception("Unsupported primitive type! " + typeof(T));

                return (T)value;
            }
        }

        /// <summary>
        /// Gets the child chunk from a given type. 
        /// Returns null if not present.
        /// </summary>
        public ChunkEntry GetChildChunk(ChunkDataType type)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i].ChunkType == type)
                    return Children[i];
            }
            return null;
        }

        /// <summary>
        /// Gets the child chunk from a given type. 
        /// Returns null if not present.
        /// </summary>
        public List<ChunkEntry> GetChildChunks(ChunkDataType type)
        {
            List<ChunkEntry> chunks = new List<ChunkEntry>();
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i].ChunkType == type)
                    chunks.Add(Children[i]);
            }
            return chunks;
        }
    }
}
