using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using System.Drawing;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace CarboCroc
{
    public class CarboMaterialSelector : GH_Component
    {
        public CarboMaterialSelector()
: base("CarboMaterialSelect", "CarboMaterialSelect", "Select a material from a template", "CarboCroc", "Data")
        {

        }
        private List<string> _options = new List<string>();
        private int _selectedIndex = 0;
        private string _lastLoadedPath = "";
        private bool hasbeenloaded = false;

        public override Guid ComponentGuid => new Guid("3F6C110B-8F0B-4683-95A6-1A0AC112DE5C");


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            //pManager.AddTextParameter("Template Path", "Path", "Path to a cxml or csv file, empty path will load the default or globally set template", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Selected", "S", "Selected item from the CSV list", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            /*
            string path = "";

            if (!DA.GetData(0, ref path) || string.IsNullOrWhiteSpace(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Please provide a valid cxml or csv file path.");
                return;
            }

            if (!File.Exists(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File not found.");
                return;
            }
            */
            string path = CarboCrocUtils.getSetTemplatePath("");

            // Only reload if file path changed or file has been modified
            if (hasbeenloaded == false)
            {
                _lastLoadedPath = path;
                _options = LoadTemplate(path);
                _selectedIndex = 0; // Reset selection on new list
                hasbeenloaded = true;
            }

            if (_options.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No data loaded from file.");
                return;
            }

            // Clamp index if needed
            if (_selectedIndex >= _options.Count)
                _selectedIndex = 0;

            DA.SetData(0, _options[_selectedIndex]);
        }

        private List<string> LoadTemplate(string path)
        {
            var lines = new List<string>();
            try
            {

                CarboProject CP = new CarboProject(path);
                CarboDatabase DB = CP.CarboDatabase;

                lines = new List<string>();

                foreach (CarboMaterial CM in DB.CarboMaterialList)
                {
                    lines.Add(CM.Name);
                    // DA.SetData(1, CM.Name);
                }
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error reading file: " + ex.Message);
            }

            return lines;
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            GH_DocumentObject.Menu_AppendItem(menu, "[Click here to select]", (sender, e) =>
            {
                if (_options == null || _options.Count == 0)
                {
                    MessageBox.Show("No valid options available. Check that your file path input is connected and valid.", "file Not Loaded", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var dialog = new ListSelectionForm(_options, _selectedIndex))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _selectedIndex = dialog.SelectedIndex;
                        ExpireSolution(true); // Recompute
                    }
                }
            });
        }

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetInt32("SelectedIndex", _selectedIndex);
            writer.SetString("LastPath", _lastLoadedPath);
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            _selectedIndex = reader.GetInt32("SelectedIndex");
            _lastLoadedPath = reader.GetString("LastPath");
            return base.Read(reader);
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return CarboCroc.Properties.Resources.List;
            }
        }

    }
    public class ListSelectionForm : Form
    {
        private ListBox _listBox;
        private TextBox _searchBox;
        private Button _okButton;
        private List<string> _fullOptions;

        public int SelectedIndex { get; private set; }

        public ListSelectionForm(List<string> options, int currentIndex)
        {
            this.Text = "Select an Option";
            this.Size = new Size(300, 450);
            this.StartPosition = FormStartPosition.CenterParent;

            _fullOptions = new List<string>(options);

            _searchBox = new TextBox()
            {
                Dock = DockStyle.Top,
                PlaceholderText = "Search...",
                Margin = new Padding(5),
            };
            _searchBox.TextChanged += SearchBox_TextChanged;

            _listBox = new ListBox()
            {
                Dock = DockStyle.Fill
            };

            _okButton = new Button()
            {
                Text = "OK",
                Dock = DockStyle.Bottom,
                Height = 30
            };

            _okButton.Click += (sender, e) =>
            {
                if (_listBox.SelectedIndex >= 0)
                {
                    string selected = _listBox.SelectedItem.ToString();
                    SelectedIndex = _fullOptions.IndexOf(selected); // Map back to original list
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };

            this.Controls.Add(_listBox);
            this.Controls.Add(_okButton);
            this.Controls.Add(_searchBox);

            PopulateListBox(_fullOptions, currentIndex);
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            string filter = _searchBox.Text.ToLowerInvariant();
            var filtered = _fullOptions.Where(s => s.ToLowerInvariant().Contains(filter)).ToList();
            PopulateListBox(filtered, 0);
        }
    
        private void PopulateListBox(List<string> items, int selectIndex)
        {
            _listBox.BeginUpdate();
            _listBox.Items.Clear();

            foreach (var item in items)
                _listBox.Items.Add(item);

            if (_listBox.Items.Count > 0)
                _listBox.SelectedIndex = Math.Min(selectIndex, _listBox.Items.Count - 1);

            _listBox.EndUpdate();
        }
    }
}
