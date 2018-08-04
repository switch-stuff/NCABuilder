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

        public static byte ContentType;
        public static byte KeyIdx;
        public static byte KeyGeneration;

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void TextBox2_MouseClick(object sender, MouseEventArgs e)
        {
            Text = null;
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

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            Directory.CreateDirectory($@"{folderBrowserDialog1.SelectedPath}/RomFS/");
            Directory.CreateDirectory($@"{folderBrowserDialog1.SelectedPath}/ExeFS/");
            Directory.CreateDirectory($@"{folderBrowserDialog1.SelectedPath}/Logo/");
            ListDirectory(treeView1, $@"{folderBrowserDialog1.SelectedPath}/RomFS/");
            Process.Start($@"{folderBrowserDialog1.SelectedPath}/RomFS/");
            timer1.Start();
        }

        private void NCABuild()
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
            var RomFS = RomFSConstructor.MakeRomFS("test.romfs");

            uint Entry1Offset = 0xC00;
            byte[] Entry1 = Entry(Entry1Offset, (uint)RomFS.Item2.LongLength);

            byte[] Key = CryptoInitialisers.GenerateRandomKey(0x10);
            byte[] Keys = KeyArea(Utils.StringToBytes("XXXX"), Key);
            byte[] Head = Header(Utils.Pad(0x100), Utils.Pad(0x100), NCA3, NCAType_Digital, ContentType, CryptoType_Updated, KeyIdx, 0xC00 + (ulong)RomFS.Item2.LongLength, 0x0100AA00AAAA000, 0xFFFFFFFF, KeyGeneration, Utils.Pad(0x10), Entry1, Utils.Pad(0x10), Utils.Pad(0x10), Utils.Pad(0x10), CryptoInitialisers.GenSHA256Hash(RomFS.Item1), Utils.Pad(0x20), Utils.Pad(0x20), Utils.Pad(0x20), Keys);
            byte[] Final = NCAHeader(Head, RomFS.Item1, Utils.Pad(0x200), Utils.Pad(0x200), Utils.Pad(0x200));

            byte[] CryptRom = CryptoInitialisers.AES_CTR(Key, Utils.Pad(0xC).Concat(BitConverter.GetBytes(Entry1Offset >> 4).Reverse()).ToArray(), RomFS.Item2);

            byte[] NCAn = NCA(CryptoInitialisers.AES_XTS(HeaderKey1, HeaderKey2, 0x200, Final, 0), ref CryptRom);
            File.WriteAllBytes("test.nca", NCAn);
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox3.SelectedValue)
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
            switch (KeyGen.SelectedValue)
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
            switch (KeyIndex.SelectedValue)
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
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            ListDirectory(treeView1, $@"{folderBrowserDialog1.SelectedPath}/");
            treeView1.ExpandAll();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            ListDirectory(treeView1, $@"{folderBrowserDialog1.SelectedPath}/");
            treeView1.ExpandAll();
        }
    }
}