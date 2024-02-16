using System.IO;
using System.Text;

namespace GABAK
{
    internal class ardesign
    {
        private StringBuilder content;

        /// <summary>
        /// Initializes the xml object with width and height parameters
        /// </summary>
        /// <param name="width">Width of the warehouse</param>
        /// <param name="depth">Depth of the warehouse</param>
        public ardesign(double p_width, double p_depth, double p_storagelocationwidth, double p_storagelocationdepth)
        {
            content = new StringBuilder();
            content.AppendLine("<?xml version=\"1.0\"?>");
            content.AppendLine("<warehouse width = \"" + p_width.ToString() + "\" depth = \"" + p_depth.ToString() + "\" storagelocationwidth = \"" + p_storagelocationwidth.ToString() + "\" storagelocationdepth = \"" + p_storagelocationdepth.ToString() + "\">");
        }

        /// <summary>
        /// Open a Region in XML
        /// </summary>
        /// <param name="p_angle">Angle of the Region</param>
        public void addRegion(double p_angle)
        {
            content.AppendLine("<region angle = \"" + p_angle.ToString() + "\">");
        }

        /// <summary>
        /// Close a Region in XML
        /// </summary>
        public void closeRegion()
        {
            content.AppendLine("</region>");
        }

        public void addStorageLocation(double p_x, double p_y, bool p_flip)
        {
            content.AppendLine("<storagelocation x = \"" + p_x.ToString() + "\" y = \"" + p_y.ToString() + "\" flip = \"" + p_flip.ToString() + "\"" + "></storagelocation>");
        }

        public void addPickLists()
        {
            content.AppendLine("<picklists>");
        }

        public void closePickLists()
        {
            content.AppendLine("</picklists>");
        }

        public void addPickList(int p_picklistid)
        {
            content.AppendLine("<picklist id= \"" + p_picklistid.ToString() + "\">");
        }

        public void closePickList()
        {
            content.AppendLine("</picklist>");
        }

        public void addPickLocation(double p_x, double p_y)
        {
            content.AppendLine("<picklocation x = \"" + p_x.ToString() + "\" y = \"" + p_y.ToString() + "\"></picklocation>");
        }

        public void closeWarehouse()
        {
            content.AppendLine("</warehouse>");
        }

        /// <summary>
        /// This must be used when adding content to ard file is finished
        /// </summary>
        public void save(FileStream fs)
        {
            fs.Write(Encoding.ASCII.GetBytes(content.ToString()), 0, Encoding.ASCII.GetBytes(content.ToString()).Length);
        }
    }
}