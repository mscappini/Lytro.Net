using System;
using System.IO;

namespace Lytro
{
    public class Section
    {
        private SectionData sectionData;

        public Section()
        { }

        internal Section(LytroFile file, SectionData sectionData)
        {
            this.LytroFile = file;
            this.sectionData = sectionData;
        }

        public virtual void Export(string path)
        {
            File.WriteAllBytes(path, this.Data);
        }

        public LytroFile LytroFile { get; private set; }
        public SectionTypes SectionType { get; internal set; }
        public string Name { get; internal set; }
        public string SHA1 { get { return this.sectionData.SHA1; } }
        public byte[] Data { get { return this.sectionData.Data; } }
    }
}