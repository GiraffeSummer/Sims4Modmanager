using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Sims_Mod_manager
{
    public partial class EditCategories : Form
    {

        public string dataPath = "";
        public EditCategories()
        {
            InitializeComponent();

            LoadCategories();
        }

        public Category SelectedCategory()
        {
            Category cat = Form1.data.categories.Find(mo => mo.name == listBox1.Items[listBox1.SelectedIndex].ToString());
            return cat;
        }

        void LoadCategories()
        {
            listBox1.Items.Clear();
            Form1.data.categories.ForEach(x => { listBox1.Items.Add(x.name); });
        }

        private void doneBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            inputBox input = new inputBox("Enter new Category name:");
            input.ShowDialog();
            if (input.DialogResult == DialogResult.OK)
            {
                Category category = new Category(input.text);
                if (Form1.data.categories.Exists(c => c.name.ToLower() == input.text.ToLower()))
                {
                    MessageBox.Show("This category already exists");
                    return;
                }
                Form1.data.categories.Add(category);
                Form1.data.Save(dataPath);

                LoadCategories();
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            Category category = SelectedCategory();

            if (Form1.BaseCategories.Contains(category))
            {
                MessageBox.Show("You cannot edit this category.");
                return;
            }

            inputBox input = new inputBox($"Enter new name for [{category.name}]:");
            input.ShowDialog();
            if (input.DialogResult == DialogResult.OK)
            {
                if (Form1.data.categories.Exists(c => c.name.ToLower() == input.text.ToLower()))
                {
                    MessageBox.Show("This name already exists");
                    return;
                }

                Category item2 = Form1.data.categories.Where(i => i.name == category.name).First();
                int index = Form1.data.categories.IndexOf(item2);

                if (index != -1)
                    Form1.data.categories[index] = new Category(input.text);

                Form1.data.mods.ForEach(x => { Console.WriteLine(x.category.name); if (x.category.name == item2.name) x.category = Form1.data.categories[index];  });

                Form1.data.Save(dataPath);

                LoadCategories();
            }
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            Category category = SelectedCategory();

            if (Form1.BaseCategories.Contains(category))
            {
                MessageBox.Show("You cannot delete this category.");
                return;
            }

            bool accept = MessageBox.Show($"Are you sure you want to delete category: [{category.name}]?", "Are you sure?", MessageBoxButtons.YesNo) == DialogResult.Yes;
            if (accept)
            {
                Category newCat = new Category("Ect.");
                Form1.data.mods.ForEach(x => { Console.WriteLine(x.category.name); if (x.category.name == category.name) x.category = newCat; });

                Form1.data.categories.Remove(category);
                Form1.data.Save(dataPath);

                LoadCategories();
            }
        }
    }
}