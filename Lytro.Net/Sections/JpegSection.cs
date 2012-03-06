using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;

namespace Lytro.Sections
{
    public class JpegSection : Section
    {
        internal JpegSection(LytroFile file, SectionData sectionData)
            : base(file, sectionData)
        {
            this.SectionType = SectionTypes.LFP_JPEG;
            this.Name = "image";
        }

        public Bitmap GetBitmap()
        {
            return new Bitmap(new MemoryStream(base.Data));
        }
    }
}