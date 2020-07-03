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

namespace Sims_Mod_manager
{
    public partial class NewMod : Form
    {
        public bool Busy = false;
        string[] ForbinnenExtentions = new string[] { ".ico", ".exe", ".ini", ".txt", ".url" };
        public Mod mod;
        public string extractPath = "";
        bool editMode;
        public NewMod(string[] categories, bool _editMode = false, Mod _mod = null)
        {
            editMode = _editMode;
            InitializeComponent();
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

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            if (!editMode)
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Multiselect = true;
                    openFileDialog.InitialDirectory = Form1.data.openDirectory;
                    openFileDialog.Filter = "Mod files (*.zip,*.package;*.t4script)|*.zip;*.package|Whatever (testing) (*.*)|*.*";
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
                            
                            if (extention == ".zip" || extention == ".7z" || extention == ".rar")
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
                                            //Console.WriteLine(extractPath + itemName);
                                            item.ExtractToFile(extractPath + itemName, true);
                                            listBox1.Items.Add(item);
                                            mod.files.Add(extractPath + itemName);
                                            progressBar1.Value = archive.Entries.IndexOf(item);
                                        }
                                    }
                                }                                
                                
                                progressBar1.Visible = false;
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
