using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace LelandsLand
{
    public class Shooter : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Shooter()
          : base("Shooter", "Shoot",
              "Shoots the viewport camera",
              "Leland's Land", "Imaging")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("shoot!", "shoot!", "connect a button to shoot", GH_ParamAccess.item);
            pManager.AddPointParameter("location", "loc", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("resolution", "res", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("lens length", "lens", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("path", "path", "location of shot images", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool enabled = false;
            Point3d loc = new Point3d(0,0,0);
            int res = 10;
            double lens = 35;


            if (!DA.GetData(0, ref enabled)) { return; }
            if (!DA.GetData(1, ref loc)) { return; }
            if (!DA.GetData(2, ref res)) { return; }
            if (!DA.GetData(3, ref lens)) { return; }
            if (!enabled) { return; }

            var path = ShootScene(loc, res, lens);

            DA.SetData(0, path);
        }

        private string ShootScene(Point3d loc, int res, double lens)
        {
            // Generate list of point targets in circle around basepoint
            var circ = new Rhino.Geometry.Circle(loc, 1);
            var circCast = circ.ToNurbsCurve();
            var ptParams = circCast.DivideByCount(res, false);

            // Get the active vpt
            var av = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView;

            // Set viewport props
            av.ActiveViewport.Camera35mmLensLength = lens;

            // Create temp directory
            var appdpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var myPath = appdpath + @"\aviary\";
            System.IO.Directory.CreateDirectory(myPath);

            // Clear existing in temp directory
            System.IO.DirectoryInfo di = new DirectoryInfo(myPath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }


            int nameIterator = 0;

            foreach (var par in ptParams)
            {
                // Convert par to target point
                var tar = circCast.PointAt(par);

                // Set the camera
                av.ActiveViewport.SetCameraLocations(tar, loc);

                // Capture the active viewport
                using (Bitmap map = av.CaptureToBitmap(new Size(800, 600)))
                {
                    string outputFileName = myPath + "_" + nameIterator.ToString() + ".jpg";
                    using (MemoryStream memory = new MemoryStream())
                    {
                        using (FileStream fs = new FileStream(outputFileName, FileMode.Create, FileAccess.ReadWrite))
                        {
                            map.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
                            byte[] bytes = memory.ToArray();
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                nameIterator++;
            }
            return myPath;
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
            get { return new Guid("81B34547-BDF0-43D9-A762-E4CBD96B68C0"); }
        }
    }
}
