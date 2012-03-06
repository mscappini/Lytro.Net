using System.Text;

namespace Lytro.Sections
{
    public class TableOfContentsSection : Section
    {
        private string toc;

        internal TableOfContentsSection(LytroFile file, SectionData sectionData)
            : base(file, sectionData)
        {
            this.SectionType = SectionTypes.LFP_TEXT;
            this.Name = "table";
        }

        public string Content
        {
            get
            {
                if (toc == null)
                {
                    toc = Encoding.ASCII.GetString(base.Data);
                }
                return toc;
            }
        }
    }
}