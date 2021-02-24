using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CarboLifeAPI.Data
{
    public class A1A3Collection
    {
        public List<A1A3List> a1a3List { get; set; }

        public A1A3Collection()
        {
            a1a3List = new List<A1A3List>();
        }

        /// <summary>
        /// Reads the csv files from the local path.
        /// </summary>
        public void LoadAll()
        {
            string myPath = Utils.getAssemblyPath() + "\\data\\A1A3Tables\\";

            List<string> DatabaseFiles = Directory.EnumerateFiles(myPath, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".csv")).ToList();

            //List<A1A3List> result = new List<A1A3List>();

            //Find Profilelist;
            foreach (string path in DatabaseFiles)
            {
                A1A3List list = new A1A3List();

                if (File.Exists(path))
                {
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    list.Name = fileName;
                    list.Path = path;


                    DataTable a1a3Table = Utils.LoadCSV(path);
                    foreach (DataRow dr in a1a3Table.Rows)
                    {
                        A1A3Element newElement = new A1A3Element();

                        int id = Convert.ToInt16(dr[0]);

                        string name = dr[1].ToString();
                        string description = dr[2].ToString();
                        string category = dr[3].ToString();

                        double density = Utils.ConvertMeToDouble(dr[4].ToString());
                        double ECI_A1A3 = Utils.ConvertMeToDouble(dr[5].ToString());


                        newElement.Id = id;
                        newElement.Name = name;
                        newElement.Description = description;
                        newElement.Density = density;
                        newElement.Category = category;
                        newElement.ECI_A1A3 = ECI_A1A3;

                        newElement.Group = fileName;

                        //Add new element to the list;
                        list.Add(newElement);
                    }
                }
                else
                {
                    MessageBox.Show("File: " + path + " could not be found, make sure you have the Eol list located in indicated folder");
                }

                if (list.Elements.Count > 0)
                {
                    a1a3List.Add(list);
                }
            }

            //a1a3List = result;

        }



        public A1A3Element FindElement(string group, string name)
        {
            A1A3Element result = new A1A3Element();

            foreach (A1A3List list in a1a3List)
            {
                if (list.Name == group)
                {
                    foreach (A1A3Element element in list.Elements)
                    {
                        if (name == element.Name)
                            result = element;
                    }
                }
            }

            return result;
        }

        public List<string> GetGroupNames()
        {
            List<string> result = new List<string>();

            foreach (A1A3List list in a1a3List)
            {
                result.Add(list.Name);
            }

            return result;
        }

        public List<string> GetCategoryList(string group)
        {
            List<string> result = new List<string>();

            foreach (A1A3List list in a1a3List)
            {
                if (list.Name == group)
                {
                    //Found the group:
                    foreach (A1A3Element element in list.Elements)
                    {
                        string categoryName = element.Category;
                        bool uniqueCategory = true;

                        foreach (string mc in result)
                        {
                            if (mc == categoryName)
                            {
                                uniqueCategory = false;
                            }
                        }

                        if (uniqueCategory == true)
                        {
                            result.Add(categoryName);
                        }

                    }
                }
            }

            result.Sort();
            return result;
        }

        /*
              private void LoadCategories()
        {
            cbb_Categories.Items.Clear();

            List<string> result = new List<string>();

            foreach (A1A3Element a1a3element in ECITables)
            {
                bool uniqueCategory = true;


            }
            result.Sort();
            return result;
        }
         */



    }

    public class A1A3List
    {
        public List<A1A3Element> Elements { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        public A1A3List()
        {
            Elements = new List<A1A3Element>();
            Name = "";
            Path = "";
        }

        internal void Add(A1A3Element newElement)
        {
            if (Elements != null)
            {
                if (newElement != null)
                    this.Elements.Add(newElement);
            }
        }
    }
    
    [Serializable]
    public class A1A3Element
    {
        public int Id { get; set; }
        public string Group { get; set; }
        public string Category { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public double Density { get; set; }
        public double ECI_A1A3 { get; set; }

        public A1A3Element()
        {
            Id = 0;
            Name = "";
            Category = "";
            Description = "";

            Density = 0;
            ECI_A1A3 = 0;
        }

        public void Calculate()
        {
            //
        }
    }
}
