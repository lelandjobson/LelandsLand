using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace LelandsLand
{
    public class LelandsLandInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "LelandsLand";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Tools by Leland Jobson";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("8fd2ba4d-e4d6-4484-820d-5f58d3590c47");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Leland Jobson";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "lelandjobson@gmail.com";
            }
        }
    }
}
