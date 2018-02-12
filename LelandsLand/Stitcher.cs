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
    public class Stitcher : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Stitcher()
          : base("Stitcher", "Stitch",
              "Stiches a pano together",
              "Leland's Land", "Imaging")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("path", "path", "path to folder with bitmaps", GH_ParamAccess.item);
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
            string bmpDirPath = null;
            // Defence
            //if (!DA.GetData(0, ref data)) { return; }
            //if (data == null) { return; }
            //if (data.Length == 0) { return; }
            bmpDirPath = @"C:\Users\Leland Jobson\AppData\Roaming\aviary";

            var myFiles = new DirectoryInfo(bmpDirPath).GetFiles()
                                                  .OrderBy(f => f.LastWriteTime)
                                                  .ToList();

            List<string> myFileNames = new List<string>();

            foreach(var fi in myFiles)
            {
                myFileNames.Add(fi.FullName);
            }


            Bitmap memBit = null;
            string memName = "";

            for(int i = 1; i < myFiles.Count; i++)
            {
                if( memBit == null)
                {
                    memName = myFiles[i - 1].FullName;
                    memBit = (Bitmap)Image.FromFile(memName, true);
                }

                Bitmap image2 = (Bitmap)Image.FromFile(myFiles[i].FullName, true);
                try
                {
                    var stitcher = new StitcherClass(memBit, image2);
                    memBit = stitcher.CompleteImage;
                    string img2Name = myFiles[i].FullName;

                    // Write out the progress bitmap
                    string progressFileName = memName + "_StitchedTo_" + img2Name + ".jpg";
                    using (MemoryStream memory = new MemoryStream())
                    {
                        using (FileStream fs = new FileStream(progressFileName, FileMode.Create, FileAccess.ReadWrite))
                        {
                            memBit.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
                            byte[] bytes = memory.ToArray();
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }

                    // Update the progress file name string
                    memName = myFiles[i].FullName;
                    image2.Dispose();
                }
                catch (Exception e)
                {
                    // Stop stitching and return however far you got.
                    image2.Dispose();
                    continue;
                }

            }

            // Write out the complete bitmap
            string outputFileName = bmpDirPath + "_Stitched" +  ".jpg";
            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream fs = new FileStream(outputFileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    memBit.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }

            memBit.Dispose();

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
            get { return new Guid("4F60884B-39C8-4185-A190-84E5EDC38B0F"); }
        }
    }
}
