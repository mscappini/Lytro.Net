namespace Lytro.Sections
{
    public class TextSection : Section
    {
        public TextSection(LytroFile file, SectionData sectionData)
            : base(file, sectionData)
        {
            this.SectionType = SectionTypes.LFP_TEXT;
        }
    }
}