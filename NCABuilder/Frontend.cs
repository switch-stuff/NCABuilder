using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using static NCABuilder.Structs;

namespace NCABuilder
{
    public partial class Frontend : Form
    {
        public Frontend()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void TextBox2_MouseClick(object sender, EventArgs e)
        {
            OnClick(e);
                textBox2.Text = null;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
        }

        private void ListDirectory(TreeView treeView, string path)
        {
            treeView.Nodes.Clear();
            var rootDirectoryInfo = new DirectoryInfo(path);
            treeView.Nodes.Add(CreateDirectoryNode(rootDirectoryInfo));
        }

        private static TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeNode(directoryInfo.Name);
            foreach (var directory in directoryInfo.GetDirectories())
                directoryNode.Nodes.Add(CreateDirectoryNode(directory));
            foreach (var file in directoryInfo.GetFiles())
                directoryNode.Nodes.Add(new TreeNode(file.Name));
            return directoryNode;
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
        }

        public static byte type;

        private void Button1_Click_1(object sender, EventArgs e)
        {
            if (type == 1)
            {
                folderBrowserDialog1.ShowDialog();
                Directory.CreateDirectory($@"{folderBrowserDialog1.SelectedPath}/RomFS/");
                Directory.CreateDirectory($@"{folderBrowserDialog1.SelectedPath}/ExeFS/");
                Directory.CreateDirectory($@"{folderBrowserDialog1.SelectedPath}/Logo/");
                ListDirectory(treeView1, $@"{folderBrowserDialog1.SelectedPath}/");
                treeView1.ExpandAll();
                Process.Start($@"{folderBrowserDialog1.SelectedPath}/");
                timer1.Start();
                tabControl1.SelectTab(1);
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
            else if (type == 2)
            {
                folderBrowserDialog1.ShowDialog();
                Directory.CreateDirectory($@"{folderBrowserDialog1.SelectedPath}/RomFS/");
                Directory.CreateDirectory($@"{folderBrowserDialog1.SelectedPath}/ExeFS/");
                ListDirectory(treeView1, $@"{folderBrowserDialog1.SelectedPath}/");
                treeView1.ExpandAll();
                Process.Start($@"{folderBrowserDialog1.SelectedPath}/");
                timer1.Start();
                tabControl1.SelectTab(1);
            }
            else if (type == 3)
            {
                folderBrowserDialog1.ShowDialog();
                Directory.CreateDirectory($@"{folderBrowserDialog1.SelectedPath}/ExeFS/");
                ListDirectory(treeView1, $@"{folderBrowserDialog1.SelectedPath}/");
                treeView1.ExpandAll();
                Process.Start($@"{folderBrowserDialog1.SelectedPath}/");
                timer1.Start();
                tabControl1.SelectTab(1);
            }
            else if (type == 4)
            {
                folderBrowserDialog1.ShowDialog();
                Directory.CreateDirectory($@"{folderBrowserDialog1.SelectedPath}/RomFS/");
                ListDirectory(treeView1, $@"{folderBrowserDialog1.SelectedPath}/");
                treeView1.ExpandAll();
                Process.Start($@"{folderBrowserDialog1.SelectedPath}/");
                timer1.Start();
                tabControl1.SelectTab(1);
            }
            else if (type == 5)
            {
                folderBrowserDialog1.ShowDialog();
                Directory.CreateDirectory($@"{folderBrowserDialog1.SelectedPath}/PFS0/");
                ListDirectory(treeView1, $@"{folderBrowserDialog1.SelectedPath}/");
                treeView1.ExpandAll();
                Process.Start($@"{folderBrowserDialog1.SelectedPath}/");
                timer1.Start();
                tabControl1.SelectTab(1);
            }
        }

        /*private void NCABuild()
        {
            byte[] HeaderKey1 = new byte[0x10];
            byte[] HeaderKey2 = new byte[0x10];
            foreach (string line in File.ReadLines(@"keys.txt"))
            {
                if (line.Contains("header_key"))
                {
                    HeaderKey1 = Utils.StringToBytes(line.Replace(" ", "").Replace("=", "").Substring(11, 32));
                    HeaderKey2 = Utils.StringToBytes(line.Replace(" ", "").Replace("=", "").Substring(43, 32));
                }
            }
            var RomFS = RomFSConstructor.MakeRomFS("test.romfs", 0);

            uint Entry1Offset = 0xC00;
            byte Entry1Id = 0;
            byte[] Entry1 = Entry(Entry1Offset, (uint)RomFS.Item2.LongLength);

            byte[] Key = CryptoInitialisers.GenerateRandomKey(0x10);
            byte[] Keys = KeyArea(Utils.StringToBytes("XXXX"), Key);
            byte[] Head = Header(Utils.Pad(0x100), Utils.Pad(0x100), NCA3, NCAType_Digital, ContentType, CryptoType_Updated, KeyIdx, 0xC00 + (ulong)RomFS.Item2.LongLength, 0x0100AA00AAAA000, 0xFFFFFFFF, KeyGeneration, Utils.Pad(0x10), Entry1, Utils.Pad(0x10), Utils.Pad(0x10), Utils.Pad(0x10), CryptoInitialisers.GenSHA256Hash(RomFS.Item1), Utils.Pad(0x20), Utils.Pad(0x20), Utils.Pad(0x20), Keys);
            byte[] Final = NCAHeader(Head, RomFS.Item1, Utils.Pad(0x200), Utils.Pad(0x200), Utils.Pad(0x200));

            byte[] CryptRom = CryptoInitialisers.AES_CTR(Key, BitConverter.GetBytes((uint)Entry1Id).Concat(Utils.Pad(0x8).Concat(BitConverter.GetBytes(Entry1Offset >> 4)).Reverse()).ToArray(), RomFS.Item2);

            byte[] NCAn = NCA(CryptoInitialisers.AES_XTS(HeaderKey1, HeaderKey2, 0x200, Final, 0), ref CryptRom);
            File.WriteAllBytes("test.nca", NCAn);
        }*/

        /*private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox3.SelectedItem)
            {
                case "Program":
                    ContentType = ContentType_Program;
                    break;
                case "Meta":
                    ContentType = ContentType_Meta;
                    break;
                case "Control":
                    ContentType = ContentType_Control;
                    break;
                case "Manual":
                    ContentType = ContentType_Manual;
                    break;
                case "Data":
                    ContentType = ContentType_Data;
                    break;
                case "AOC":
                    ContentType = ContentType_AOC;
                    break;
                default:
                    break;
            }
        }

        private void KeyGen_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (KeyGen.SelectedItem)
            {
                case "1 (1.0.0-2.3.0)":
                    KeyGeneration = KeyGeneration_Firmware100_230;
                    break;
                case "2 (3.0.0)":
                    KeyGeneration = KeyGeneration_Firmware300;
                    break;
                case "3 (3.0.1-3.0.2)":
                    KeyGeneration = KeyGeneration_Firmware301_302;
                    break;
                case "4 (4.0.0-4.1.0)":
                    KeyGeneration = KeyGeneration_Firmware400_410;
                    break;
                case "5 (5.0.0-Now)":
                    KeyGeneration = KeyGeneration_Firmware500;
                    break;
                default:
                    break;
            }
        }

        private void KeyIndex_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (KeyIndex.SelectedItem)
            {
                case "Application":
                    KeyIdx = KeyIndex_Application;
                    break;
                case "Ocean":
                    KeyIdx = KeyIndex_Ocean;
                    break;
                case "System":
                    KeyIdx = KeyIndex_System;
                    break;
                default:
                    break;
            }
        }*/

        private void Timer1_Tick(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            ListDirectory(treeView1, $@"{folderBrowserDialog1.SelectedPath}/");
            treeView1.ExpandAll();
        }

        private void Button2_Click_1(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            ListDirectory(treeView1, $@"{folderBrowserDialog1.SelectedPath}/");
            treeView1.ExpandAll();
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox5.SelectedItem)
            {
                case "ExeFS, RomFS, PFS0 (Logo)":
                    type = 1;
                    break;
                case "ExeFS, RomFS":
                    type = 2;
                    break;
                case "ExeFS":
                    type = 3;
                    break;
                case "RomFS":
                    type = 4;
                    break;
                case "PFS0":
                    type = 5;
                    break;
                default:
                    break;
            }
            button1.Enabled = true;
        }

        private void textBox2_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            PresetDefines.Standard_Application(ContentType_Program, KeyIndex_Application, KeyGeneration_Firmware500, folderBrowserDialog1.SelectedPath, BitConverter.ToUInt64(Utils.StringToBytes(textBox2.Text).Reverse().ToArray(), 0), 0xFFFFFFFF, textBox3, textBox4);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}