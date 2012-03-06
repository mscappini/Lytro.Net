using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Lytro.Sections;

namespace Lytro.Windows
{
    public partial class MainForm : Form
    {
        private LytroFile lfp;

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Lytro Files (*.lfp)|*.lfp|All Files (*.*)|*.*";
                if (ofd.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    if (File.Exists(ofd.FileName))
                    {
                        try
                        {
                            lfp = new LytroFile(ofd.FileName);
                            lfp.Load();
                        }
                        catch (Exception ex)
                        {
                            ShowError("Could not load Lytro file:" + Environment.NewLine + ex.Message);
                            return;
                        }

                        ComboItem[] items = new ComboItem[lfp.JpegSections.Count];
                        for (int i = 0; i < items.Length; i++)
                        {
                            items[i] = new ComboItem(string.Format("{0}_{1}", lfp.Filename, i), i);
                        }
                        cbImages.DataSource = items;
                        cbImages.DisplayMember = "Text";
                        cbImages.ValueMember = "Value";
                    }
                }
            }
        }

        private void cbImages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbImages.SelectedIndex > -1)
            {
                pbPic.Image = this.lfp.JpegSections[((int)cbImages.SelectedValue)].GetBitmap();
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (cbImages.Items.Count > 0)
            {
                int idx = (cbImages.SelectedIndex - 1) % cbImages.Items.Count;
                if (idx == -1) idx = cbImages.Items.Count - 1;
                cbImages.SelectedIndex = idx;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (cbImages.Items.Count > 0)
            {
                cbImages.SelectedIndex = (cbImages.SelectedIndex + 1) % cbImages.Items.Count;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (cbImages.SelectedIndex > -1)
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    string[] filters =
                    {
                        "PNG Files (*.png)|*.png",
                        "JPEG Files (*.jpg)|*.jpg",
                        "Bitmap Files (*.bmp)|*.bmp",
                        "TIFF Files (*.tiff)|*.tiff"
                    };

                    sfd.FileName = cbImages.Text + ".png";
                    sfd.Filter = string.Join("|", filters);

                    if (sfd.ShowDialog(this) == DialogResult.OK)
                    {
                        JpegSection jpegSection = lfp.JpegSections[cbImages.SelectedIndex];
                        using (Bitmap bmp = jpegSection.GetBitmap()) // Always properly dispose of bitmap
                        {
                            int filterIx = sfd.FilterIndex;

                            ImageFormat format;
                            switch (filterIx)
                            {
                                case 1:
                                    bmp.Save(sfd.FileName, ImageFormat.Png);
                                    break;
                                case 2:
                                    bmp.SaveJpeg(sfd.FileName, 100);
                                    break;
                                case 3:
                                    bmp.Save(sfd.FileName, ImageFormat.Bmp);
                                    break;
                                case 4:
                                    bmp.Save(sfd.FileName, ImageFormat.Tiff);
                                    break;
                                default:
                                    format = ImageFormat.Png;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private class ComboItem
        {
            public ComboItem(string text, int value)
            {
                this.Text = text;
                this.Value = value;
            }

            public string Text { get; private set; }
            public int Value { get; private set; }
        }
    }
}