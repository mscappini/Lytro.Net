using System;
using System.IO;

namespace Lytro.Sections
{
    public class RawImageSection : Section
    {
        internal RawImageSection(LytroFile file, SectionData sectionData)
            : base(file, sectionData)
        {
            this.SectionType = SectionTypes.LFP_RAW_IMAGE;
            this.Name = "raw";
        }

        public override void Export(string path)
        {
            byte[] convertedImage = GetConvertedImage();
            File.WriteAllBytes(path, convertedImage);
        }

        //static char *converted_image(const unsigned char *data, int *datalen, int len)
        //{
        //    int filelen = 4*len/3;
        //    const unsigned char *ptr = data;
        //    unsigned short *image = malloc(filelen*sizeof(short));
        //    unsigned short *start = image;

        //    if (!image) return NULL;
        //    // Turn the 12 bits per pixel packed array into 16 bits per pixel
        //    // to make it easier to import into other libraries
        //    while (ptr < data+len) {
        //        *image++ = (*ptr << 8) | (*(ptr+1) & 0xF0);
        //        *image++ = ((*(ptr+1) & 0x0F) << 12) | (*(ptr+2) << 4);

        //        ptr += 3;
        //    }

        //    *datalen = filelen;

        //    return (char *)start;
        //}
        private byte[] GetConvertedImage()
        {
            byte[] data = base.Data;

            int filelen = (4 * data.Length) / 3;

            // Turn the 12 bits per pixel packed array into 16 bits per pixel
            // to make it easier to import into other libraries
            ushort[] temp = new ushort[filelen];
            for (int ctr = 0, ptr = 0; ptr < data.Length; ptr += 3)
            {
                temp[ctr++] = (ushort)((data[ptr] << 8) | (data[ptr + 1] & 0xF0));
                temp[ctr++] = (ushort)(((data[ptr + 1] & 0x0F) << 12) | (data[ptr + 2] << 4));
            }

            // Pack the ushort bits into a byte array
            byte[] image = new byte[filelen];
            //for (int incursor = 0, outcursor = 0; incursor < data.Length; incursor += 2, outcursor += 3)
            //{
            //    image[outcursor + 0] = (byte)(((data[incursor + 0]) >> 4) & 0xFF);
            //    image[outcursor + 1] = (byte)((data[incursor + 0] & 0x0F) << 4 | ((data[incursor + 1] >> 8) & 0x0F));
            //    image[outcursor + 2] = (byte)(data[incursor + 1] & 0xFF);
            //}
            
            Buffer.BlockCopy(temp, 0, image, 0, temp.Length);

            return image;
        }
    }
}