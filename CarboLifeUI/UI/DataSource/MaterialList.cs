using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeUI.UI
{
    [Obsolete]
    public class MaterialList : List<String>
    {
        public MaterialList()
        {
            this.Add("Timber");
            this.Add("Metal");
            this.Add("Concrete");
            this.Add("Brick");
            this.Add("Other");
        }
    }
}
