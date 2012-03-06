using System;
using System.IO;
using System.Text;

namespace Lytro.Sections
{
    public class LookUpTableSection : Section
    {
        internal LookUpTableSection(LytroFile file, SectionData sectionData)
            : base(file, sectionData)
        {
            this.SectionType = SectionTypes.LFP_DEPTH_LUT;
            this.Name = "depth";
        }

        public override void Export(string path)
        {
            string depthLUT = ParseLookups();
            File.WriteAllText(path, depthLUT);
        }

        private string ParseLookups()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < base.Data.Length / 4; i++)
            {
                float val = BitConverter.ToSingle(base.Data, i * 4);
                sb.AppendLine(val.ToString());
            }

            return sb.ToString();
        }
    }
}