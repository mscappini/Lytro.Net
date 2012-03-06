using System;
using System.IO;
using System.Text;
using Lytro.Sections;

namespace Lytro
{
    public class LytroFile
    {
        private const int SHA1_LENGTH = 45;
        private const int MAGIC_LENGTH = 12;
        private const int BLANK_LENGTH = 35;
        private static readonly byte[] Magic = { 0x89, 0x4C, 0x46, 0x50, 0x0D, 0x0A, 0x1A, 0x0A };

        public string File { get; private set; }
        public string Filename { get; private set; }

        public TableOfContentsSection TableOfContents { get; private set; }
        public LookUpTableSection LookUpTable { get; private set; }
        public SectionCollection<RawImageSection> RawImageSections { get; private set; }
        public SectionCollection<TextSection> TextSections { get; private set; }
        public SectionCollection<JpegSection> JpegSections { get; private set; }

        public LytroFile(string file)
        {
            this.RawImageSections = new SectionCollection<RawImageSection>();
            this.TextSections = new SectionCollection<TextSection>();
            this.JpegSections = new SectionCollection<JpegSection>();

            this.File = file;
            this.Filename = Path.GetFileNameWithoutExtension(this.File);
        }

        public void Load()
        {
            using (BinaryReader br = new BinaryReader(System.IO.File.OpenRead(this.File)))
            {
                EnsureLFP(br);
                ParseSections(br);
            }
        }

        public void Export()
        {
            Export(string.Empty);
        }

        public void Export(string path)
        {
            path = path ?? string.Empty;

            if (this.TableOfContents != null)
            {
                string file = path + "\\" + string.Format("{0}_{1}.txt", this.Filename, this.TableOfContents.Name);
                this.TableOfContents.Export(file);
            }

            if (this.LookUpTable != null)
            {
                string file = path + "\\" + string.Format("{0}_{1}.txt", this.Filename, this.LookUpTable.Name);
                this.LookUpTable.Export(file);
            }

            for (int i = 0; i < this.RawImageSections.Count; i++)
            {
                RawImageSection section = this.RawImageSections[i];
                string file = path + "\\" + string.Format("{0}_{1}{2}.raw", this.Filename, section.Name, i);
                section.Export(file);
            }

            for (int i = 0; i < this.TextSections.Count; i++)
            {
                TextSection section = this.TextSections[i];
                string file = path + "\\" + string.Format("{0}_{1}{2}.txt", this.Filename, section.Name, i);
                section.Export(file);
            }

            for (int i = 0; i < this.JpegSections.Count; i++)
            {
                JpegSection section = this.JpegSections[i];
                string file = path + "\\" + string.Format("{0}_{1}{2}.jpg", this.Filename, section.Name, i);
                section.Export(file);
            }
        }

        private void EnsureLFP(BinaryReader br)
        {
            byte[] magicCheck = br.ReadBytes(MAGIC_LENGTH);
            for (int i = 0; i < Magic.Length; i++)
            {
                if (magicCheck[i] != Magic[i])
                {
                    throw new InvalidDataException("Not an LFP file");
                }
            }

            // Move past the first four 0's
            br.ReadBytes(4);
        }

        private void ParseSections(BinaryReader br)
        {
            // Assume the first section is always the table of contents
            ParseTableOfContents(br);

            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                // Parse the next section
                SectionData section = ParseSectionData(br);

                // Identify what type of section this is and build a section type for it
                IdentifySection(section);
            }
        }

        private void ParseTableOfContents(BinaryReader br)
        {
            SectionData sectionData = new SectionData();
            FillSectionData(br, sectionData);
            this.TableOfContents = new TableOfContentsSection(this, sectionData);
        }

        private void FillSectionData(BinaryReader br, SectionData sectionData)
        {
            br.ReadBytes(MAGIC_LENGTH);

            // the length is stored as a big endian unsigned 32 bit int
            uint ulen = br.ReadUInt32();
            int len = (int)SwapInt(ulen);

            // copy SHA1
            sectionData.SHA1 = Encoding.ASCII.GetString(br.ReadBytes(SHA1_LENGTH));

            // move past the 35 byte empty space
            br.ReadBytes(BLANK_LENGTH);

            // Copy the data
            sectionData.Data = br.ReadBytes(len);

            // There may be some null region between sections
            while (br.PeekChar() == '\0') br.ReadByte();
        }

        private SectionData ParseSectionData(BinaryReader br)
        {
            SectionData sectionData = new SectionData();
            FillSectionData(br, sectionData);
            return sectionData;
        }

        private Section IdentifySection(SectionData sectionData)
        {
            // Try to figure out if the data represents an image, text, raw data, etc
            Section section;

            if (sectionData.Data.Length == 1600)
            {
                // Hard coded to assume that the 20x20 LUT is 1600 bytes
                section = new LookUpTableSection(this, sectionData);
                this.LookUpTable = (LookUpTableSection)section;
            }
            else if (IsJPEG(sectionData))
            {
                // Check for the magic bytes to see if its a jpg
                section = new JpegSection(this, sectionData);
                this.JpegSections.Add((JpegSection)section);
            }
            else
            {
                string name = FindName(sectionData);

                if (name == "imageRef")
                {
                    // Assume a raw image
                    section = new RawImageSection(this, sectionData);
                    this.RawImageSections.Add((RawImageSection)section);
                }
                else
                {
                    // Assume anything that isn't called imageRef is plain text
                    section = new TextSection(this, sectionData);
                    this.TextSections.Add((TextSection)section);
                }

                section.Name = name;
            }

            return section;
        }

        private string FindName(SectionData sectionData)
        {
            // Find the sha1 in the table of contents
            int quotecount = 0;
            int sha1Ix = this.TableOfContents.Content.IndexOf(sectionData.SHA1);
            if (sha1Ix > -1)
            {
                // Move backwards to the corresponding name
                while (quotecount < 3 && (sha1Ix-- > 0))
                {
                    if (this.TableOfContents.Content[sha1Ix] == '"')
                    {
                        quotecount++;
                    }
                }

                // Read the name if we can
                if (quotecount == 3)
                {
                    int afterFirstQuote = sha1Ix + 1;
                    return this.TableOfContents.Content.Substring(
                        afterFirstQuote,
                        this.TableOfContents.Content.IndexOf('"', afterFirstQuote) - afterFirstQuote
                    );
                }
            }

            return "unknown";
        }

        private bool IsJPEG(SectionData sectionData)
        {
            byte[] jpeg = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
            for (int i = 0; i < jpeg.Length; i++)
            {
                if (sectionData.Data[i] != jpeg[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Swaps the byte order of an <see cref="Int32"/>.
        /// </summary>
        /// <param name="value"><see cref="Int32"/> to swap the bytes of.</param>
        /// <returns>Byte order swapped <see cref="Int32"/>.</returns>
        private static int SwapInt(uint value)
        {
            uint uvalue = (uint)value;

            uint swapped =
                 ((uvalue & 0x000000FF) << 24) |
                 ((uvalue & 0x0000FF00) << 8) |
                 ((uvalue & 0x00FF0000) >> 8) |
                 ((uvalue & 0xFF000000) >> 24);

            return (int)swapped;
        }
    }
}