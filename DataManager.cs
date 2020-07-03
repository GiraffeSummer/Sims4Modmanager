using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sims_Mod_manager
{

    [Serializable]
    public class Data
    {
        public string user;
        public string openDirectory;
        public List<Category> categories;
        public List<Mod> mods;

        public Data()
        {
            user = "";
            categories = new List<Category>();
            mods = new List<Mod>();
        }
        public Data(string u)
        {
            user = u;
            categories = new List<Category>();
            mods = new List<Mod>();
        }

        public void Save(string path)
        {
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(Data));

            FileStream file = File.Create(path);

            writer.Serialize(file, this);
            file.Close();
        }

        public void SaveData<T>(T obj, string path)
        {
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(T));


            FileStream file = File.Create(path);

            writer.Serialize(file, obj);
            file.Close();
        }
        public T ReadData<T>(string path)
        {
            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(T));
            StreamReader file = new StreamReader(path);
            T data = (T)reader.Deserialize(file);
            file.Close();
            return data;
        }
    }

    [Serializable]
    public class Category
    {
        public string name;
        public Category() { name = ""; }
        public Category(string n)
        {
            name = n;
        }
    }

    [Serializable]
    public class Mod
    {
        public string name;
        public Category category;
        public bool enabled;
        public List<string> files;

        public Mod() { name = ""; enabled = true; }
        public Mod(string n)
        {
            name = n;
            files = new List<string>();
            enabled = true;
        }

    }
    public partial class Functions
    {
        public virtual bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
    }
}
