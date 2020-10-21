using System.IO;
using System.Text;

namespace GABAK
{
    internal class svg
    {
        private StringBuilder content;

        /// <summary>
        /// Initializes the svg object with width and height parameters
        /// </summary>
        /// <param name="width">Width of the SVG</param>
        /// <param name="height">Height of the SVG</param>
        public svg(int width, int height)
        {
            content = new StringBuilder();
            content.AppendLine("<?xml version=\"1.0\"?>");
            content.AppendLine("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">");
            content.AppendLine("<svg xmlns = \"http://www.w3.org/2000/svg\" version = \"1.1\" width = \"" + width.ToString() + "\" height = \"" + height.ToString() + "\">");
        }

        /// <summary>
        /// Add svg line
        /// </summary>
        /// <param name="x1">X1</param>
        /// <param name="y1">Y1</param>
        /// <param name="x2">X2</param>
        /// <param name="y2">Y2</param>
        /// <param name="lineWidth">Width of the line</param>
        /// <param name="lineColor">Color of the line</param>
        public void addLine(float x1, float y1, float x2, float y2, float lineWidth, System.Drawing.Color lineColor)
        {
            content.AppendLine("<line style = \"stroke:rgb(" + lineColor.R.ToString() + "," + lineColor.G.ToString() + "," + lineColor.B.ToString() + ")\" stroke-width = \"" + lineWidth.ToString() + "\" x1 = \"" + x1.ToString() + "\" y1 = \"" + y1.ToString() + "\" x2 = \"" + x2.ToString() + "\" y2 = \"" + y2.ToString() + "\" />\"");
        }

        public void addDashLine(float x1, float y1, float x2, float y2, float lineWidth, System.Drawing.Color lineColor)
        {
            content.AppendLine("<line style = \"stroke:rgb(" + lineColor.R.ToString() + "," + lineColor.G.ToString() + "," + lineColor.B.ToString() + ")\" stroke-width = \"" + lineWidth.ToString() + "\" x1 = \"" + x1.ToString() + "\" y1 = \"" + y1.ToString() + "\" x2 = \"" + x2.ToString() + "\" y2 = \"" + y2.ToString() + "\" stroke-dasharray=\"5 2\" />\"");
        }

        public void addCircle(float x1, float y1, float r, float lineWidth, System.Drawing.Color lineColor)
        {
            content.AppendLine("<circle fill=\"White\" style = \"stroke:rgb(" + lineColor.R.ToString() + "," + lineColor.G.ToString() + "," + lineColor.B.ToString() + ")\" stroke-width = \"" + lineWidth.ToString() + "\" cx = \"" + x1.ToString() + "\" cy = \"" + y1.ToString() + "\" r = \"" + r.ToString() + "\" />\"");
        }

        /// <summary>
        /// This must be used when adding content to svg is finished
        /// </summary>
        public void save(FileStream fs)
        {
            content.AppendLine("</svg>");
            fs.Write(Encoding.ASCII.GetBytes(content.ToString()), 0, Encoding.ASCII.GetBytes(content.ToString()).Length);
        }
    }
}