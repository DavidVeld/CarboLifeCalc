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
    public class CarboCrocTemplateList : GH_Component
    {
        public CarboCrocTemplateList()
: base("CarboCrocTemplateSelector", "CarboCrocTemplateSelector", "Ricght click on node to select an available Carbo Life Templates for input into the CarboCrocTemplateSetter node", "CarboCroc", "Template")
        {

        }

        // Your selectable list of strings
        private List<string> _options = new List<string>();
        private List<string> _path = new List<string>();
        int _lastListCount = 0; // Track last list count to detect changes  

        private int _selectedIndex = 0;

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{99DD35B8-12B0-4EAA-A9B8-62CA03DDAC59}");
            }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Template Path", "Path", "Returns the material template path", GH_ParamAccess.item);//9
            pManager.Register_StringParam("Template Name", "Name", "Returns the material template name", GH_ParamAccess.item);//9
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            IDictionary<string,string> templates = PathUtils.getTemplateFiles();

            try
            {
                if (templates != null && templates.Count > 0)
                {
                    if (_lastListCount != templates.Count)
                    {
                        _lastListCount = templates.Count;
                        _options.Clear(); // Clear previous options

                        foreach (var template in templates)
                        {
                            _options.Add(template.Key);
                            _selectedIndex = 0; // Reset selection on new list
                        }
                    }

                }

                if (templates.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No template date loaded.");
                    return;
                }

                if (_options.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No template date loaded.");
                    return;
                }

                // Clamp index if needed
                if (_selectedIndex >= _options.Count)
                    _selectedIndex = 0;
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error loading templates: " + ex.Message);
                return;
            }

            //retreive the template
            string name = _options[_selectedIndex];
            string path = "";

            if (templates.TryGetValue(name, out path))

                if (File.Exists(path))
                {
                    path = path;
                }
                else
                {
                    path = "";
                    MessageBox.Show("The Selected Template could not be found");
                }

            DA.SetData(0, path);
            DA.SetData(1, name);

        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            ToolStripMenuItem parentItem = GH_DocumentObject.Menu_AppendItem(menu, "Select Option");
            ToolStripDropDown dropdownMenu = parentItem.DropDown;

            // Add Items from list:
            for (int i = 0; i < _options.Count; i++)
            {
                int index = i;
                var item = new ToolStripMenuItem(_options[i])
                {
                    Checked = (index == _selectedIndex)
                };

                item.Click += (sender, e) =>
                {
                    _selectedIndex = index;
                    ExpireSolution(true);
                };

                dropdownMenu.Items.Add(item);
            }




        }

        // Optional: Persist selected index (save/restore)
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetInt32("SelectedIndex", _selectedIndex);
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            _selectedIndex = reader.GetInt32("SelectedIndex");
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

    public class ListSelectionFormT : Form
    {
        private ListBox _listBox;
        private Button _okButton;
        public int SelectedIndex { get; private set; }

        public ListSelectionFormT(List<string> options, int currentIndex)
        {
            this.Text = "Select an Option";
            this.Size = new Size(300, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            _listBox = new ListBox()
            {
                Dock = DockStyle.Top,
                Height = 300
            };

            foreach (var option in options)
                _listBox.Items.Add(option);

            _listBox.SelectedIndex = currentIndex;

            _okButton = new Button()
            {
                Text = "OK",
                Dock = DockStyle.Bottom
            };

            _okButton.Click += (sender, e) =>
            {
                if (_listBox.SelectedIndex >= 0)
                {
                    SelectedIndex = _listBox.SelectedIndex;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };

            this.Controls.Add(_listBox);
            this.Controls.Add(_okButton);
        }
    }
}
