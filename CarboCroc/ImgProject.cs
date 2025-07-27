using System;
using System.Drawing;
using Grasshopper.Kernel;

public class ImageOutputComponent : GH_Component
{
    public ImageOutputComponent()
      : base("Image Output", "ImageOut",
          "Outputs a Bitmap image",
          "Utils", "Graphics")
    { }

    public override Guid ComponentGuid => new Guid("12345678-1234-1234-1234-1234567890AB");

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        // No inputs in this example
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddGenericParameter("Image", "Img", "Bitmap image output", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        Bitmap bmp = new Bitmap(100, 100);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.Red);
        }

        DA.SetData(0, bmp);
    }
}