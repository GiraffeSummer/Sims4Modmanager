﻿using System;
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

    public partial class Form1 : Form
    {
        public static Data data;
        public static string illegalCharacters = "\"M\"\\a/ry/ h**ad:>> a\\/:*?\"| li*tt|le|| la\"mb.?";
        public string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Electronic Arts\\The Sims 4\\Mods\\";
        string dataPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Electronic Arts\\The Sims 4\\Mod_Manager\\" + "data.xml";
        string AllFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Electronic Arts\\The Sims 4\\Mod_Manager\\files\\";

        Category noCategory = new Category("None");

        public List<string> VisualCategories;
        public Form1()
        {
            //.package
            InitializeComponent();

            panel1.BorderStyle = BorderStyle.FixedSingle;

            modEnabledFilter.Items.AddRange(new string[] { "Ignore", "Enabled", "Disabled" });
            modEnabledFilter.SelectedIndex = 0;

            progressBar1.Visible = false;
            progressBar1.Value = 100;

            VisualCategories = new List<string>();

            if (!Directory.Exists(AllFilesPath)) Directory.CreateDirectory(AllFilesPath);

            string[] files = Directory.GetFiles(path, "*.package");
            for (int i = 0; i < files.Length; i++)
            {
                Console.WriteLine(files[i]);
            }

            if (File.Exists(dataPath))
            {
                data = new Data().ReadData<Data>(dataPath);
                for (int i = 0; i < data.categories.Count; i++)
                {
                    VisualCategories.Add(data.categories[i].name);
                }
                this.Text = data.user;
            }
            else
            {
                inputBox input = new inputBox("Enter your name");
                input.ShowDialog();
                if (input.DialogResult == DialogResult.OK)
                {
                    data = new Data(input.text);
                    data.openDirectory = path;
                    data.categories.AddRange(new Category[] {
                        new Category("Build Mode"),
                        new Category("Buy Mode"),
                        new Category("CAS(create a sim)"),
                        new Category("Ect.") });
                    data.Save(dataPath);
                    this.Text = data.user;
                    for (int i = 0; i < data.categories.Count; i++)
                    {
                        VisualCategories.Add(data.categories[i].name);
                    }
                }
                else this.Close();
            }
            filterBox.Items.Add(noCategory.name);
            filterBox.SelectedIndex = 0;
            filterBox.Items.AddRange(VisualCategories.ToArray());

            LoadMods();
        }

        void LoadMods()
        {
            if (filterBox.SelectedIndex < 0 || filterBox.SelectedIndex < 0) return;
            string filter = searchBox.Text;
            listBox1.Items.Clear();
            Category category = new Category(filterBox.Items[filterBox.SelectedIndex].ToString());
            string modEnabled = modEnabledFilter.Items[modEnabledFilter.SelectedIndex].ToString();
            Console.WriteLine(modEnabled);
            if (category.name == noCategory.name) category = null;
            List<Mod> filteredMods = data.mods.FindAll(m =>
            {
                if (category == null)
                    return m.name.ToLower().Contains(filter.ToLower()) && (modEnabled == "Ignore" || m.enabled == (modEnabled == "Enabled"));
                else
                {
                    return (m.name.ToLower().Contains(filter.ToLower()) && (m.category.name == category.name) && (modEnabled == "Ignore" || m.enabled == (modEnabled == "Enabled")));
                }
            });
            for (int i = 0; i < filteredMods.Count; i++)
            {
                string name = filteredMods[i].name;
                listBox1.Items.Add(name);

                // if (data.mods[i].enabled) checkedListBox1.SetItemChecked(i, true);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Timer timer = new Timer();
            NewMod m = new NewMod(VisualCategories.ToArray());
            m.extractPath = AllFilesPath;
            m.ShowDialog();
            Functions f = new Functions();

            if (m.DialogResult == DialogResult.OK)
            {
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                progressBar1.Maximum = m.mod.files.Count;                
                
                for (int i = 0; i < m.mod.files.Count; i++)
                {
                    string fileName = Path.GetFileName(m.mod.files[i]);

                    string packPath = m.mod.files[i];
                    Console.WriteLine(packPath);
                    while (f.IsFileLocked(new FileInfo(packPath)))
                    {
                        File.Copy(packPath, AllFilesPath + fileName, true);
                        File.Copy(packPath, path + fileName, true);
                    }
                    m.mod.files[i] = AllFilesPath + fileName;
                    progressBar1.Value = i;
                }
                data.mods.Add(m.mod);
                data.Save(dataPath);
                listBox1.Items.Add(m.mod.name);

                timer.Interval = 1000;
                timer.Start();

                // Hook up the Elapsed event for the timer.
                timer.Tick += delegate (object se, EventArgs ea)
                {
                    progressBar1.Visible = false;
                    timer.Enabled = false;
                    timer.Dispose();
                };

                //checkedListBox1.SetItemChecked(indexMax, true);
            }
        }


        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetModInfo();
        }

        public void GetModInfo()
        {
            modFileBox.Items.Clear();
            if (listBox1.SelectedIndex == -1) return;
            modInfoBox.Text = "";
            Mod mod = data.mods.Find(m => m.name == listBox1.Items[listBox1.SelectedIndex].ToString());

            for (int i = 0; i < mod.files.Count; i++)
            {
                modFileBox.Items.Add(Path.GetFileName(mod.files[i]));
            }
            modInfoBox.AppendText($"Mod name: {mod.name}\r\n{(mod.enabled ? "Enabled" : "Disabled")}\r\nCategory: {mod.category.name}\r\nFiles: {mod.files.Count}");
            modInfoBox.Select(0, 8);
            modInfoBox.SelectionFont = new Font(modInfoBox.Font, FontStyle.Bold);
            modInfoBox.Select(modInfoBox.Text.IndexOf("Category:"), 8);
            modInfoBox.SelectionFont = new Font(modInfoBox.Font, FontStyle.Bold);
            modInfoBox.Select(modInfoBox.Text.IndexOf("Files:"), 5);
            modInfoBox.SelectionFont = new Font(modInfoBox.Font, FontStyle.Bold);
            if (modInfoBox.Text.IndexOf("Enabled") >= 0)
            {
                modInfoBox.Select(modInfoBox.Text.IndexOf("Enabled"), 7);
                modInfoBox.SelectionFont = new Font(modInfoBox.Font, FontStyle.Bold);
            }
            if (modInfoBox.Text.IndexOf("Disabled") >= 0)
            {
                modInfoBox.Select(modInfoBox.Text.IndexOf("Disabled"), 8);
                modInfoBox.SelectionFont = new Font(modInfoBox.Font, FontStyle.Bold);
            }
        }


        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mod mod = data.mods.Find(m => m.name == listBox1.Items[listBox1.SelectedIndex].ToString());
            data.mods.Remove(mod);
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            data.Save(dataPath);
            modInfoBox.Text = "";
            MessageBox.Show("Deleted mod");
            for (int i = 0; i < mod.files.Count; i++)
            {
                string fileName = Path.GetFileName(mod.files[i]);
                File.Delete(path + fileName);
            }
        }

        private void modMenu_Opening(object sender, CancelEventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
            {
                e.Cancel = true;
                return;
            }
            deleteToolStripMenuItem.Enabled = (listBox1.SelectedIndex > -1);
            Mod mod = data.mods.Find(m => m.name == listBox1.Items[listBox1.SelectedIndex].ToString());
            uninstallToolStripMenuItem.Text = (mod.enabled) ? "Disable" : "Enable";
        }

        private void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mod mod = data.mods.Find(m => m.name == listBox1.Items[listBox1.SelectedIndex].ToString());

            mod.enabled = !mod.enabled;

            for (int i = 0; i < mod.files.Count; i++)
            {

                string fileName = Path.GetFileName(mod.files[i]);
                if (mod.enabled)
                {
                    File.Copy(AllFilesPath + fileName, path + fileName, true);
                }
                else
                {
                    File.Delete(path + fileName);
                }
            }
            data.Save(dataPath);
            GetModInfo();
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            LoadMods();
        }

        private void clearSearch_Click(object sender, EventArgs e)
        {
            filterBox.SelectedIndex = 0;
            searchBox.Text = "";
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mod mod = data.mods.Find(mo => mo.name == listBox1.Items[listBox1.SelectedIndex].ToString());
            string oldName = mod.name;
            NewMod m = new NewMod(VisualCategories.ToArray(), true, mod);
            m.extractPath = AllFilesPath;
            m.ShowDialog();

            if (m.DialogResult == DialogResult.OK)
            {
                data.mods.Remove(mod);
                listBox1.Items.Remove(oldName);

                data.mods.Add(m.mod);
                listBox1.Items.Add(m.mod.name);
                data.Save(dataPath);
            }
            GetModInfo();
        }
    }
}