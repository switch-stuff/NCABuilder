using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

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

        byte KeyGeneration;
        byte KeyIdx;
        byte ContentType;
        ulong TitleID;

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
                tabControl1.SelectTab(1);
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
            else if (type == 3)
            {
                folderBrowserDialog1.ShowDialog();
                Directory.CreateDirectory($@"{folderBrowserDialog1.SelectedPath}/RomFS/");
                ListDirectory(treeView1, $@"{folderBrowserDialog1.SelectedPath}/");
                treeView1.ExpandAll();
                Process.Start($@"{folderBrowserDialog1.SelectedPath}/");
                tabControl1.SelectTab(1);
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
            else if (type == 4)
            {
                folderBrowserDialog1.ShowDialog();
                Directory.CreateDirectory($@"{folderBrowserDialog1.SelectedPath}/ExeFS/");
                ListDirectory(treeView1, $@"{folderBrowserDialog1.SelectedPath}/");
                treeView1.ExpandAll();
                Process.Start($@"{folderBrowserDialog1.SelectedPath}/");
                tabControl1.SelectTab(1);
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
            else if (type == 5)
            {
                folderBrowserDialog1.ShowDialog();
                Directory.CreateDirectory($@"{folderBrowserDialog1.SelectedPath}/PFS0/");
                ListDirectory(treeView1, $@"{folderBrowserDialog1.SelectedPath}/");
                treeView1.ExpandAll();
                Process.Start($@"{folderBrowserDialog1.SelectedPath}/");
                tabControl1.SelectTab(1);
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
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
                case "ExeFS + RomFS + PFS0 (Logo)":
                    type = 1;
                    break;
                case "ExeFS + RomFS":
                    type = 2;
                    break;
                case "RomFS Only":
                    type = 3;
                    break;
                case "ExeFS Only":
                    type = 4;
                    break;
                case "PFS0 Only":
                    type = 5;
                    break;
                default:
                    break;
            }
            button1.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (KeyGen.Text != "" && KeyIndex.Text != "" && comboBox3.Text != "")
            {
                string[] Keys = { };
                string HeaderKey = null;
                string KeyAreaKey = null;
                string Index = KeyIndex.SelectedItem.ToString().ToLower();
                string Generation = KeyGen.SelectedItem.ToString().Substring(0, 2);
                try
                {
                    Keys = File.ReadAllLines("keys.txt");
                }
                catch (Exception)
                {
                    MessageBox.Show("Error: keys.txt not present in directory.");
                }
                try
                {
                    HeaderKey = Keys.FirstOrDefault(T => T.StartsWith("header_key")).Split(Convert.ToChar("="))[1].Trim();
                }
                catch (Exception)
                {
                    MessageBox.Show("Error: Header key not present in your keys file.");
                }
                try
                {
                    KeyAreaKey = Keys.FirstOrDefault(T => T.StartsWith($"key_area_key_{Index}_{Generation}")).Split(Convert.ToChar("="))[1].Trim();
                }
                catch
                {
                    MessageBox.Show($"Error: Key area key {Index} {Generation} not present in your keys file.");
                }
                //if (type == 1)
                //    PresetDefines.Standard_Application(ContentType, KeyIdx, KeyGeneration, folderBrowserDialog1.SelectedPath, TitleID, 0x05040000, HeaderKey, KeyAreaKey);
                /*else*/ if (type == 2)
                    PresetDefines.ExeFS_RomFS(ContentType, KeyIdx, KeyGeneration, folderBrowserDialog1.SelectedPath, TitleID, 0x05040000, HeaderKey, KeyAreaKey);
                else if (type == 3)
                    PresetDefines.RomFS(ContentType, KeyIdx, KeyGeneration, folderBrowserDialog1.SelectedPath, TitleID, 0x05040000, HeaderKey, KeyAreaKey);
                else if (type == 4)
                    PresetDefines.ExeFS(ContentType, KeyIdx, KeyGeneration, folderBrowserDialog1.SelectedPath, TitleID, 0x05040000, HeaderKey, KeyAreaKey);
                else if (type == 5)
                    PresetDefines.PFS(ContentType, KeyIdx, KeyGeneration, folderBrowserDialog1.SelectedPath, TitleID, 0x05040000, HeaderKey, KeyAreaKey);
            }
            else
            {
                MessageBox.Show("Error: Please ensure you have made a selection for all relevant options.");
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!ulong.TryParse(textBox2.Text, System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.CurrentInfo, out TitleID) && textBox2.Text != String.Empty)
            {
                textBox2.Text = textBox2.Text.Remove(textBox2.Text.Length - 1, 1);
                textBox2.SelectionStart = textBox2.Text.Length;
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void deleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                File.Delete($"{folderBrowserDialog1.SelectedPath}/{treeView1.SelectedNode.Parent.Text}/{treeView1.SelectedNode.Text}");
                treeView1.Nodes.Clear();
                ListDirectory(treeView1, $@"{folderBrowserDialog1.SelectedPath}/");
                treeView1.ExpandAll();
            }
            catch (Exception)
            {
                MessageBox.Show("Please choose a file to delete, not a directory.");
            }
        }

        private void KeyGen_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            switch (KeyGen.SelectedItem)
            {
                case "00 (1.0.0-2.3.0)":
                    KeyGeneration = 0;
                    break;
                case "01 (3.0.0)":
                    KeyGeneration = 0;
                    break;
                case "02 (3.0.1-3.0.2)":
                    KeyGeneration = 3;
                    break;
                case "03 (4.0.0-4.1.0)":
                    KeyGeneration = 4;
                    break;
                case "04 (5.0.0-Now)":
                    KeyGeneration = 5;
                    break;
                default:
                    break;
            }
        }

        private void comboBox3_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            switch (comboBox3.SelectedItem)
            {
                case "Program":
                    ContentType = 0;
                    break;
                case "Meta":
                    ContentType = 1;
                    break;
                case "Control":
                    ContentType = 2;
                    break;
                case "Manual":
                    ContentType = 3;
                    break;
                case "Data":
                    ContentType = 4;
                    break;
                case "AOC":
                    ContentType = 5;
                    break;
                default:
                    break;
            }
        }

        private void KeyIndex_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            switch (KeyIndex.SelectedItem)
            {
                case "Application":
                    KeyIdx = 0;
                    break;
                case "Ocean":
                    KeyIdx = 1;
                    break;
                case "System":
                    KeyIdx = 2;
                    break;
                default:
                    break;
            }
        }
    }
}