using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace LelandsLand
{
    public class TestStitcher : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public TestStitcher()
          : base("Test Stitcher", "Test Stitcher",
              "Stiches two given images together",
              "Leland's Land", "Imaging")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("img a", "img a", "path to an image", GH_ParamAccess.item);
            pManager.AddTextParameter("img b", "img b", "path to an image", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("path out", "path out", "result of the stitching", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string bmpAPath = null;
            string bmpBPath = null;

            if (!DA.GetData(0, ref bmpAPath)) { return; }
            if (!DA.GetData(0, ref bmpBPath)) { return; }

            if (bmpAPath == null | bmpAPath.Length == 0 ) { return; }
            if (bmpBPath == null | bmpBPath.Length == 0) { return; }


         
            var bmpA = (Bitmap)Image.FromFile(bmpAPath, true);
            var bmpB = (Bitmap)Image.FromFile(bmpBPath, true);
           

            var stitcher = new StitcherClass(bmpA, bmpB, true);
            Bitmap complete = stitcher.CompleteImage;

            var dir = System.IO.Directory.GetParent(bmpAPath);

            // Write out the complete bitmap
            string outputFileName = dir + "_Stitched" + ".jpg";
            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream fs = new FileStream(outputFileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    complete.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }

            //stitcher.Dispose();

            DA.SetData(0, outputFileName);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                return LelandsLand.Properties.Resources.wipIcon;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("EB528A28-57DF-435D-AEC0-DCB06F99A449"); }
        }
    }
}
