using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Toolbox.Core.IO;

namespace NextLevelLibrary
{
    /// <summary>
    /// A chunk that represents a file that stores a unique hash and identifier in the header.
    /// These will store either raw data or sub chunks depending on the sub data flag.
    /// </summary>
    public class ChunkFileEntry : ChunkEntry
    {
        /// <summary>
        /// The magic identifier for the file.
        /// </summary>
        public uint Magic { get; set; }

        /// <summary>
        /// The hash of the file name.
        /// </summary>
        public uint Hash { get; set; }

        /// <summary>
        /// A flag with an unknown purpose.
        /// </summary>
        public ushort FileFlag { get; set; }

        /// <summary>
        /// The header size for file magic/hash
        /// </summary>
        public uint FileHeaderSize;

        /// <summary>
        /// The header offset for file magic/hash
        /// </summary>
        public uint FileHeaderOffset;

        /// <summary>
        /// The file type regarding the chunk contents.
        /// </summary>
        [XmlIgnore]
        public ChunkFileType FileType;

        /// <summary>
        /// The raw data of the file header.
        /// </summary>
        [XmlIgnore]
        public Stream FileHeader;

        public ChunkEntry DataChild;

        public void Save() { }

        public override void Export(string filePath)
        {
            if (!this.HasChildren)
            {
                //export raw data
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (var writer = new FileWriter(fs))
                {
                    Data.CopyTo(writer.BaseStream);
                }
                return;
            }
            else
            {
                //readable json export
                FileExport file = new FileExport();
                file.FileType = this.FileType.ToString();
                file.Magic = this.Magic;
                file.Hash = this.Hash;

                List<SectionExport> ExportChildren(List<ChunkEntry> chunks)
                {
                    List<SectionExport> list = new List<SectionExport>();

                    foreach (var c in chunks)
                    {
                        SectionExport section = new SectionExport();
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
                return;
            }
        }

        public ChunkFileEntry() { }

        public ChunkFileEntry(IChunkTable table) : base(table)
        {
        }

        class TypeFlags
        {
            public Dictionary<string, uint> Types = new Dictionary<string, uint>();
            public Dictionary<uint, uint> MagicTypes = new Dictionary<uint, uint>();
        }

        public class FileExport
        {
            public List<SectionExport> Sections = new List<SectionExport>();

            public uint Magic;
            public uint Hash;
            public string FileType;

            public byte[] Data { get; set; }
            public string DataAsText { get; set; }
        }

        public class SectionExport
        {
            public string Type {  get; set; }
            public byte[] Data { get; set; }

            public List<SectionExport> SubSections = new List<SectionExport>();
        }
    }
}
