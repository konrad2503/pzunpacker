using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Diagnostics;

namespace PZDepack
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //OpenPackFile(@"C:\Program Files (x86)\Steam\steamapps\common\ProjectZomboid\media\texturepacks\IconsMoveables.pack");
            //if (1 < 2) return;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Pack-Files (*.pack)|*.pack";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (ofd.FileName != null && File.Exists(ofd.FileName))
                {
                    OpenPackFile(ofd.FileName);
                }
            }
        }
        private void OpenPackFile(string filename)
        {
            listView1.Items.Clear();
            using (FileStream fileStream = File.OpenRead(filename))
            {
                byte[] png = new byte[] { 137, 80, 78, 71 }; // PNG Header Bytes [ 20 89 50 4e 47 0d 0a 1a ] // 89504e47
                byte[] fileBytes = new byte[fileStream.Length];
                fileStream.Read(fileBytes, 0, (int)fileStream.Length);
                int index = -1;
                List<PngFile> PngFiles = new List<PngFile>();
                int pngIndex = -1;
                while (index++ < fileBytes.Length-1)
                {
                    byte currentByte = fileBytes[index];
                    if (index < fileBytes.Length - 6)
                    {
                        byte[] passingBytes = new byte[]{
                            currentByte,
                            fileBytes[index + 1],
                            fileBytes[index + 2],
                            fileBytes[index + 3]
                        };
                        //‰PNG
                        string hexSum = String.Format("{0} {1} {2} {3}", passingBytes[0].ToString("X"), passingBytes[1].ToString("X"), passingBytes[2].ToString("X"), passingBytes[3].ToString("X"));
                        if (hexSum.Contains("89 50 4E 47"))
                        {
                            Console.Write(hexSum + " = " + Encoding.UTF8.GetString(passingBytes) + " / ");
                            if (pngIndex != -1)
                            {
                                PngFile pngFail = new PngFile();
                                pngFail.start = pngIndex;
                                pngFail.length = Math.Abs(index - pngIndex);
                                PngFiles.Add(pngFail);
                                Application.DoEvents();
                                Console.WriteLine("new PngFile(start={0},length={1});", pngFail.start, pngFail.length);
                            }
                            pngIndex = index;
                        }
                        //if (currentByte == 20 && fileBytes[index + 1] == 89 && fileBytes[index + 2] == 50 && fileBytes[index + 3] == 78)
                        //{
                        //    Console.WriteLine("PNG");
                        //}
                    }
                    //Console.WriteLine("{0:D2} = {1}", index, Encoding.UTF8.GetString(new byte[] { currentByte }));
                }
                PngFile pngFailEnd = new PngFile();
                pngFailEnd.start = pngIndex;
                pngFailEnd.length = Math.Abs(fileBytes.Length - pngIndex);
                PngFiles.Add(pngFailEnd);
                Console.WriteLine("new PngFile(start={0},length={1});", pngFailEnd.start, pngFailEnd.length);

                Application.DoEvents();
                Console.WriteLine("Processing PNG Files");
                int Ii = 0;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(fileBytes, 0, fileBytes.Length);
                    foreach (PngFile PNGFile in PngFiles)
                    {
                        memoryStream.Seek(PNGFile.start, SeekOrigin.Begin);
                        List<byte> buff = new List<byte>();
                        
                        while (memoryStream.Position != PNGFile.start + PNGFile.length)
                        {
                            byte b = (byte)memoryStream.ReadByte();
                            buff.Add(b);
                        }
                        Application.DoEvents();
                        ListViewItem lvi = new ListViewItem();
                        lvi.Text = String.Format("{0}.png", Ii++);
                        lvi.Tag = buff.ToArray();
                        lvi.SubItems.Add(buff.ToArray().Length / 1024 + " KB");
                        lvi.SubItems.Add("PNG");
                        listView1.Items.Add(lvi);
                        buff.Clear();
                        Application.DoEvents();
                    }
                }
            }
        }
        public struct PngFile
        {
            public int start;
            public int length;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                contextMenuStrip1.Show((ListView)sender, new Point(0, 0));
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG-Images(*.png)|*.png";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllBytes(sfd.FileName, (byte[])listView1.SelectedItems[0].Tag);
            }
        }

        private void previewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(Application.StartupPath,listView1.SelectedItems[0].Text);
            File.WriteAllBytes(path, (byte[])listView1.SelectedItems[0].Tag);
            Process.Start(path);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.MyDocuments;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (ListViewItem lvi in listView1.Items)
                {
                    string path = Path.Combine(fbd.SelectedPath, lvi.Text);
                    File.WriteAllBytes(path, (byte[])lvi.Tag);
                }
            }
        }
    }
    public static class ByteHelper
    {
        [System.Diagnostics.Contracts.Pure]
        public static byte[] Subbytes(this byte[] a, int index)
        {
            byte[] b = new byte[a.Length-index];
            Array.Copy(a, index, b, 0, b.Length);
            return a;
        }
    }
}
