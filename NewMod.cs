using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using SharpCompress.Archives.Rar;
using SharpCompress.Readers.Rar;
using SharpCompress.Common;
using SharpCompress.IO;

namespace Sims_Mod_manager
{
    public partial class NewMod : Form
    {
        public bool Busy = false;
        string[] ForbinnenExtentions = new string[] { ".ico", ".exe", ".ini", ".txt", ".url", ".jpg" };
        public Mod mod;
        public string extractPath = "";
        bool editMode;
        Functions func = new Functions();
        public NewMod(string[] categories, bool _editMode = false, Mod _mod = null)
        {
            editMode = _editMode;
            InitializeComponent();
            this.MaximizeBox = false;
            progressBar1.Visible = false;
            this.Text = "Select Mod";
            comboBox1.Items.AddRange(categories);

            if (_editMode)
            {
                if (_mod == null) throw new SyntaxErrorException("Mod parameter was NULL");
                mod = _mod;
                button3.Visible = false;
                button3.Enabled = false;

                this.Text = mod.name;
                textBox1.Text = mod.name;
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(mod.category.name);

                for (int i = 0; i < mod.files.Count; i++)
                {
                    listBox1.Items.Add(Path.GetFileName(mod.files[i]));
                }
            }
        }

        public NewMod(string path, string[] categories)
        {
            //from url
            InitializeComponent();
            this.MaximizeBox = false;
            progressBar1.Visible = false;
            this.Text = "Select Mod";
            extractPath = Path.GetDirectoryName(path);
            comboBox1.Items.AddRange(categories);

            button3.Visible = false;
            button3.Enabled = false;

            AddMod(path);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Busy)
            {
                MessageBox.Show("Busy loading mods.");
                return;
            }
            mod.name = textBox1.Text;
            mod.category = new Category(comboBox1.Items[comboBox1.SelectedIndex].ToString());
            Console.WriteLine("MOd ADDER:" + mod.files.Count);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public void AddMod(string path)
        {
            mod = HandleModFiles(path);
            string fileName = Path.GetFileName(path);
            this.Text = fileName;
            textBox1.Text = fileName;
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(mod.category.name);
        }

        public Mod HandleModFiles(string modpath)
        {
            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
            mod = new Mod(Path.GetFileName(modpath));
            mod.category = new Category(comboBox1.Items[comboBox1.SelectedIndex].ToString());
            this.textBox1.Text = Path.GetFileName(modpath).Replace(Path.GetExtension(modpath), "");
            this.Text = Path.GetFileName(modpath) + " (" + Path.GetExtension(modpath) + ")";
            Busy = true;
            button2.Enabled = false;

            // string filePath = openFileDialog.FileName;
            string fileName = Path.GetFileName(modpath);
            string extention = Path.GetExtension(modpath);
            string folderpath = Path.GetDirectoryName(modpath);
            Form1.data.openDirectory = folderpath;

            if (extention == ".zip" || extention == ".7z")
            {
                progressBar1.Visible = true;
                //read files in zip an process them
                using (ZipArchive archive = ZipFile.Open(modpath, ZipArchiveMode.Read))
                {
                    progressBar1.Maximum = archive.Entries.Count;
                    foreach (ZipArchiveEntry item in archive.Entries)
                    {
                        if (!String.IsNullOrEmpty(item.Name))//check if not folder
                        {
                            string itemName = item.FullName;
                            if (itemName.Contains('/')) itemName = itemName.Split('/')[1];

                            string ext = Path.GetExtension(itemName);
                            if (ForbinnenExtentions.Contains(ext)) continue;

                            item.ExtractToFile(extractPath + "\\" + itemName, true);
                            listBox1.Items.Add(item);
                            mod.files.Add(extractPath + "\\" + itemName);
                            progressBar1.Value = archive.Entries.IndexOf(item);
                        }
                    }
                }

                progressBar1.Visible = false;
            }
            else if (extention == ".rar")
            {
                int fileAmount = 0;
                // SharpCompress.Readers.Rar;
                using (RarReader reader = RarReader.Open(File.OpenRead(modpath)))
                {
                    while (reader.MoveToNextEntry())
                    { if (!reader.Entry.IsDirectory) fileAmount++; }
                }
                progressBar1.Maximum = fileAmount;
                using (RarReader reader = RarReader.Open(File.OpenRead(modpath)))
                {
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            using (EntryStream entryStream = reader.OpenEntryStream())
                            {
                                string file = Path.GetFileName(reader.Entry.Key);
                                if (null != file)
                                {
                                    string destinationFileName = Path.Combine(extractPath + "\\", file);
                                    using (FileStream fs = File.OpenWrite(destinationFileName))
                                    {
                                        string ext = Path.GetExtension(file);
                                        if (ForbinnenExtentions.Contains(ext)) continue;
                                        func.TransferTo(reader, entryStream, fs);
                                        listBox1.Items.Add(file);
                                        mod.files.Add(extractPath + "\\" + file);
                                        //  progressBar1.Value++;
                                    }
                                }
                            }
                        }
                    }
                }

            }
            else
            {
                mod.files.Add(modpath);
                listBox1.Items.Add(Path.GetFileName(modpath));
            }

            button2.Enabled = true;
            Busy = false;
            return mod;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            if (!editMode)
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Multiselect = true;
                    openFileDialog.InitialDirectory = Form1.data.openDirectory;
                    openFileDialog.Filter = "Mod files (*.zip,*.package;*.t4script;*.rar;)|*.zip;*.package;*.t4script;*.rar|Whatever (testing) (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        //Get the path of specified file
                        comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
                        mod = new Mod(Path.GetFileName(openFileDialog.FileNames[0]));
                        mod.category = new Category(comboBox1.Items[comboBox1.SelectedIndex].ToString());
                        this.textBox1.Text = Path.GetFileName(openFileDialog.FileNames[0]).Replace(Path.GetExtension(openFileDialog.FileNames[0]), "");
                        this.Text = Path.GetFileName(openFileDialog.FileNames[0]) + " (" + Path.GetExtension(openFileDialog.FileNames[0]) + ")";
                        Busy = true;
                        button2.Enabled = false;
                        foreach (string filePath in openFileDialog.FileNames)
                        {
                            // string filePath = openFileDialog.FileName;
                            string fileName = Path.GetFileName(filePath);
                            string extention = Path.GetExtension(filePath);
                            string folderpath = Path.GetDirectoryName(filePath);
                            Form1.data.openDirectory = folderpath;

                            if (extention == ".zip" || extention == ".7z")
                            {
                                progressBar1.Visible = true;
                                //read files in zip an process them
                                using (ZipArchive archive = ZipFile.Open(filePath, ZipArchiveMode.Read))
                                {
                                    progressBar1.Maximum = archive.Entries.Count;
                                    foreach (ZipArchiveEntry item in archive.Entries)
                                    {
                                        if (!String.IsNullOrEmpty(item.Name))//check if not folder
                                        {
                                            string itemName = item.FullName;
                                            if (itemName.Contains('/')) itemName = itemName.Split('/')[1];

                                            string ext = Path.GetExtension(itemName);
                                            if (ForbinnenExtentions.Contains(ext)) continue;

                                            item.ExtractToFile(extractPath + itemName, true);
                                            listBox1.Items.Add(item);
                                            mod.files.Add(extractPath + itemName);
                                            progressBar1.Value = archive.Entries.IndexOf(item);
                                        }
                                    }
                                }

                                progressBar1.Visible = false;
                            }
                            else if (extention == ".rar")
                            {
                                int fileAmount = 0;
                                // SharpCompress.Readers.Rar;
                                using (RarReader reader = RarReader.Open(File.OpenRead(filePath)))
                                {
                                    while (reader.MoveToNextEntry())
                                    { if (!reader.Entry.IsDirectory) fileAmount++; }
                                }
                                progressBar1.Maximum = fileAmount;
                                using (RarReader reader = RarReader.Open(File.OpenRead(filePath)))
                                {
                                    while (reader.MoveToNextEntry())
                                    {
                                        if (!reader.Entry.IsDirectory)
                                        {
                                            using (EntryStream entryStream = reader.OpenEntryStream())
                                            {
                                                string file = Path.GetFileName(reader.Entry.Key);
                                                if (null != file)
                                                {
                                                    string destinationFileName = extractPath + file;
                                                    using (FileStream fs = File.OpenWrite(destinationFileName))
                                                    {
                                                        string ext = Path.GetExtension(file);
                                                        if (ForbinnenExtentions.Contains(ext)) continue;
                                                        func.TransferTo(reader, entryStream, fs);
                                                        listBox1.Items.Add(file);
                                                        mod.files.Add(extractPath + file);
                                                        //  progressBar1.Value++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                            else
                            {
                                mod.files.Add(filePath);
                                listBox1.Items.Add(Path.GetFileName(filePath));
                            }
                        }
                        button2.Enabled = true;
                        Busy = false;
                    }
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.Text = this.textBox1.Text;
        }
    }
}