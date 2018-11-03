using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using AviFile;

namespace GABAK
{
    public partial class ChartForm : Form
    {
        List<string[]> lines;//Used for importing designs for optimization
        FileInfo fileIn;
        public ChartForm()
        {
            InitializeComponent();
        }

        private double f(int i)
        {
            var f1 = 59894 - (8128 * i) + (262 * i * i) - (1.6 * i * i * i);
            return f1;
        }

        private static Bitmap MergeTwoImages(Image firstImage, Image secondImage, int outputImageWidth, int outputImageHeight)
        {
            if (firstImage == null)
            {
                throw new ArgumentNullException("firstImage");
            }

            if (secondImage == null)
            {
                throw new ArgumentNullException("secondImage");
            }

            Bitmap outputImage = new Bitmap(outputImageWidth, outputImageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                graphics.DrawImage(firstImage, new Rectangle(new Point(), firstImage.Size),
                    new Rectangle(new Point(), firstImage.Size), GraphicsUnit.Pixel);
                graphics.DrawImage(secondImage, new Rectangle(new Point(0, firstImage.Height + 1), secondImage.Size),
                    new Rectangle(new Point(), secondImage.Size), GraphicsUnit.Pixel);
            }

            return outputImage;
        }

        private void SplineChart(string filedirectory)
        {
            chart2.Series.Clear();
            var series1 = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Series1",
                Color = System.Drawing.Color.Green,
                IsVisibleInLegend = false,
                IsXValueIndexed = true,
                ChartType = SeriesChartType.Line
            };
            chart2.Series.Add(series1);

            //Set X and Y Axis Minimum and Maximum
            chart2.ChartAreas[0].AxisX.Minimum = 0;
            //chart2.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(lines.Count() - 1);
            chart2.ChartAreas[0].AxisX.Interval = 25;
            chart2.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chart2.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Number;
            chart2.ChartAreas[0].AxisX.IsStartedFromZero = true;
            chart2.ChartAreas[0].AxisY.Minimum = Math.Floor(Convert.ToDouble(lines[lines.Count()-1][0]));
            chart2.ChartAreas[0].AxisY.Maximum = Math.Ceiling(Convert.ToDouble(lines[1][0]));

            double cost = -1;
            double previouscost = double.MaxValue;
            int lasti = int.MinValue;
            List<Bitmap> designpics = new List<Bitmap>();
            List<Bitmap> chartpics = new List<Bitmap>();
            List<Bitmap> mergedpics = new List<Bitmap>();
            MemoryStream ms = new MemoryStream();
            int maxWidthdesign = -1;
            int maxHeightdesign = -1;
            //Create bmp pictures and save them for every iteration
            for (int i = 1; i < lines.Count(); i++)
            {
                cost = Convert.ToDouble(lines[i][0]);
                //There is a new design picture if this is true
                if (cost < previouscost)
                {
                    lasti = (i - 1);
                    designpics.Add(new Bitmap(filedirectory + "/" + (i - 1).ToString() + ".bmp"));
                    if (designpics[i - 1].Width > maxWidthdesign) maxWidthdesign = designpics[i - 1].Width;
                    if (designpics[i - 1].Height > maxHeightdesign) maxHeightdesign = designpics[i - 1].Height;
                    previouscost = cost;
                }
                else//No new picture use the old picture
                {
                    designpics.Add(new Bitmap(filedirectory + "/" + lasti.ToString() + ".bmp"));
                }
                series1.Points.AddXY(Convert.ToDouble(i-1), cost);
                chart2.Invalidate();
                //chart2.SaveImage(filedirectory + "/chart" + (i - 1).ToString() + ".bmp", System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Bmp);
                chart2.SaveImage(ms, ChartImageFormat.Bmp);
                chartpics.Add(new Bitmap(ms));
            }

            //Merge pictures
            for(int i = 0; i < lines.Count()-1; i++)
            {
                mergedpics.Add(MergeTwoImages(chartpics[i], designpics[i], maxWidthdesign > chart2.Width ? maxWidthdesign : chart2.Width, maxHeightdesign + chart2.Width + 1));
            }

            //Create avi video from images
            AviManager avimanager = new AviManager(filedirectory + "/video.avi", false);
            //add a new video stream and one frame to the new file
            VideoStream aviStream = avimanager.AddVideoStream(false, 10, mergedpics[0]);
            for (int i = 1; i < lines.Count()-1; i++)
            {
                //aviStream.AddFrame(chartpics[i]);
                aviStream.AddFrame(mergedpics[i]);
            }
            avimanager.Close();
        }

        private void buttonSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //Only show .csv files
            ofd.Filter = "Microsoft Office Excel Comma Separated Values File|*.csv";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                string fileName = ofd.FileName;
                fileIn = new FileInfo(fileName);
                StreamReader reader;
                try
                {
                    reader = fileIn.OpenText();
                }
                catch
                {
                    textBoxStatus.AppendText("File is Already Open, Close File\n"); return;
                }
                lines = new List<string[]>();
                string[] lineIn;//Do not read the first line (skip for header)
                while (!reader.EndOfStream)
                {
                    lineIn = reader.ReadLine().Split(',');
                    lines.Add(lineIn);
                }
                reader.Close();
            }
            textBoxStatus.AppendText("Importing done.");
            SplineChart(fileIn.Directory.FullName);
        }
    }
}
