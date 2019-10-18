using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeUI.UI
{
    public class TransportationList : List<Vehicle>
    {
        public TransportationList()
        {
            this.Add(new Vehicle { Name = "Car - Diesel", CarboNew = 12000, MaxDistance = 100000, VolumePerTransport = 5, ECPerkm = 0.177 });
            this.Add(new Vehicle { Name = "Car - Petrol", CarboNew = 12000, MaxDistance = 100000, VolumePerTransport = 5, ECPerkm = 0.185 });
            this.Add(new Vehicle { Name = "Car - Electric", CarboNew = 12000, MaxDistance = 100000, VolumePerTransport = 5, ECPerkm = 0.000 });

            this.Add(new Vehicle { Name = "Van", CarboNew = 12000, MaxDistance = 100000, VolumePerTransport = 5, ECPerkm = 0.155 });
            this.Add(new Vehicle { Name = "Truck", CarboNew = 24000, MaxDistance = 100000, VolumePerTransport = 5, ECPerkm = 0.200 });
            this.Add(new Vehicle { Name = "Concrete Truck", CarboNew = 24000, MaxDistance = 100000, VolumePerTransport = 6.1, ECPerkm = 0.200 });
            this.Add(new Vehicle { Name = "Boat", CarboNew = 240000, MaxDistance = 100000, VolumePerTransport = 5, ECPerkm = 1.000 });
            this.Add(new Vehicle { Name = "Plane", CarboNew = 240000, MaxDistance = 100000, VolumePerTransport = 5, ECPerkm = 1.000 });
            this.Add(new Vehicle { Name = "Bike", CarboNew = 100, MaxDistance = 100000, VolumePerTransport = 5, ECPerkm = 0.000 });
            this.Add(new Vehicle { Name = "Train", CarboNew = 100, MaxDistance = 100000, VolumePerTransport = 5, ECPerkm = 0.046 });

        }
    }

    public class Vehicle
    {
        public string Name;
        public double CarboNew;
        public double MaxDistance;
        public double VolumePerTransport;
        public double ECPerkm;
    }
}
