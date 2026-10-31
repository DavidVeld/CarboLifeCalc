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

        /// <summary>
        /// What could not be loaded: a missing folder, an unreadable file, rows skipped. Empty
        /// when everything loaded. For the caller to show; loading itself never throws.
        /// </summary>
        public List<string> LoadErrors { get; private set; }

        public A1A3Collection()
        {
            a1a3List = new List<A1A3List>();
            LoadErrors = new List<string>();
        }

        /// <summary>
        /// Reads the csv files from the local path.
        /// </summary>
        public void LoadAll()
        {
            LoadFrom(Utils.getAssemblyPath() + "\\data\\A1A3Tables\\");
        }

        /// <summary>
        /// Reads every csv in the folder as one table. A bad file or row is skipped and listed in
        /// LoadErrors, so one hand edited file cannot stop the others loading.
        /// </summary>
        public void LoadFrom(string folder)
        {
            if (!Directory.Exists(folder))
            {
                LoadErrors.Add("The A1-A3 table folder could not be found: " + folder);
                return;
            }

            List<string> DatabaseFiles = Directory.EnumerateFiles(folder, "*.csv", SearchOption.AllDirectories)
                .OrderBy(s => s).ToList();

            foreach (string path in DatabaseFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(path);

                try
                {
                    A1A3List list = LoadFile(path, fileName);

                    if (list.Elements.Count > 0)
                        a1a3List.Add(list);
                }
                catch (Exception ex)
                {
                    LoadErrors.Add(fileName + ".csv could not be read: " + ex.Message);
                }
            }
        }

        private A1A3List LoadFile(string path, string fileName)
        {
            A1A3List list = new A1A3List();
            list.Name = fileName;
            list.Path = path;

            DataTable a1a3Table = Utils.LoadCSV(path);

            if (a1a3Table.Columns.Count < 6)
            {
                LoadErrors.Add(fileName + ".csv has " + a1a3Table.Columns.Count + " columns, it needs 6: Id, Name, Description, Category, Density, ECI_A1A3.");
                return list;
            }

            List<int> malformedRows = Utils.GetMalformedRows(a1a3Table);

            for (int i = 0; i < a1a3Table.Rows.Count; i++)
            {
                DataRow dr = a1a3Table.Rows[i];
                string name = dr[1].ToString().Trim();

                //Its fields are shifted, so its values would be read from the wrong columns.
                if (malformedRows.Contains(i))
                {
                    LoadErrors.Add(fileName + ".csv: skipped \"" + name + "\", it has more fields than the header. A comma inside a value needs the value in quotes.");
                    continue;
                }

                if (name == "")
                {
                    LoadErrors.Add(fileName + ".csv: skipped data row " + (i + 1) + ", it has no name.");
                    continue;
                }

                A1A3Element newElement = new A1A3Element();

                //Rounded, not converted: Convert.ToInt32 throws on an id too large for an int.
                double id = Math.Round(DataExportUtils.ReadCsvDouble(dr[0].ToString()));
                newElement.Id = (id > int.MaxValue || id < int.MinValue) ? 0 : (int)id;

                newElement.Name = name;
                newElement.Description = dr[2].ToString();
                newElement.Category = dr[3].ToString().Trim();
                newElement.Density = DataExportUtils.ReadCsvDouble(dr[4].ToString());
                newElement.ECI_A1A3 = DataExportUtils.ReadCsvDouble(dr[5].ToString());

                newElement.Group = fileName;

                //Add new element to the list;
                list.Add(newElement);
            }

            return list;
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
