/* This file is part of ALO (Advanced Layout Optimizer)
 * (c) Copyright 2013-2015 by Sabahattin Gökhan Özden, gokhan.ozden@yahoo.com
 * All rights reserved.
 * Copying, distributing, using this code is prohibited.
 * Contact the author for licensing options.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Media;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace GABAK
{
    public partial class MainForm : Form
    {
        warehouse mywh;
        System.Drawing.Graphics graphicsObj;
        System.Drawing.Bitmap myBitmap;
        Pen mypen;
        List<order> myorders = new List<order>();
        List<sku> myskus = new List<sku>();
        double[] CDF;
        allocation al;
        double avgTourLength = 1;
        float m = 1;//Magnify factor
        bool solving = false;//Used for aspect ratio warehouse width auto conversion on key change if it is set to false then only it works on key focus
        string problemfolder = "";
        List<string[]> lines;//Used for importing designs

        public MainForm()
        {
            InitializeComponent();
            comboBoxDesignClass.SelectedIndex = 0;
            comboBoxComputing.SelectedIndex = 0;
            comboBoxGenerate.SelectedIndex = 0;
            comboBoxNetSchedule.SelectedIndex = 0;
            options.processid = Process.GetCurrentProcess().Id;
        }


        /// <summary>
        /// Button that creates warehouse and draws warehouse on the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCreate_Click(object sender, EventArgs e)
        {
            if (comboBoxDesignClass.SelectedItem.ToString() != "All")
            {
                DateTime start = DateTime.Now;
                //Set mywh object to null
                mywh = null;
                //Set Magnify factor
                m = (float)Convert.ToDouble(textBoxMagnify.Text);
                //Reset IDs
                edge.nextID = 0;
                region.nextID = 0;
                node.nextID = 0;
                //Create a new warehouse object
                mywh = new warehouse();
                //Set warehouse aspectratio
                mywh.aspectratio = Convert.ToDouble(textBoxAspectRatio.Text);
                //Set warehouse area
                mywh.setArea(Convert.ToDouble(textBoxArea.Text));
                //Set warehouse cross aisle width
                mywh.crossaislewidth = Convert.ToDouble(textBoxCrossAisleWidth.Text);
                //Set warehouse picking aisle width
                mywh.pickingaislewidth = Convert.ToDouble(textBoxPickingAisleWidth.Text);
                //Set warehouse picking location width
                mywh.pickinglocationwidth = Convert.ToDouble(textBoxLWidth.Text);
                //Set warehouse picking location depth
                mywh.pickinglocationdepth = Convert.ToDouble(textBoxLDepth.Text);
                //Set size of picker used for visibility graph
                mywh.pickersize = Convert.ToDouble(textBoxPickerSize.Text);
                //Set which graph will be used Steiner or Visibility Graph
                mywh.usevisibilitygraph = checkBoxVisibilityGraph.Checked;
                //Set number of colors
                options.numbercolors = Convert.ToInt32(textBoxNumberColors.Text);

                DateTime start1 = DateTime.Now;
                //Create Warehouse in the background
                //mywh.createSingleCrossAisleWarehouse(Convert.ToDouble(textBoxAngle1.Text), Convert.ToDouble(textBoxAngle2.Text), Convert.ToDouble(textBoxAdjuster1.Text), Convert.ToDouble(textBoxAdjuster2.Text), mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth, Convert.ToDouble(textBoxE1.Text), Convert.ToDouble(textBoxE2.Text), Convert.ToDouble(textBoxpd.Text));
                double[] angle = { Convert.ToDouble(textBoxAngle1.Text), Convert.ToDouble(textBoxAngle2.Text), Convert.ToDouble(textBoxAngle3.Text), Convert.ToDouble(textBoxAngle4.Text), Convert.ToDouble(textBoxAngle5.Text), Convert.ToDouble(textBoxAngle6.Text), Convert.ToDouble(textBoxAngle7.Text), Convert.ToDouble(textBoxAngle8.Text) };
                double[] adjuster = { Convert.ToDouble(textBoxAdjuster1.Text), Convert.ToDouble(textBoxAdjuster2.Text), Convert.ToDouble(textBoxAdjuster3.Text), Convert.ToDouble(textBoxAdjuster4.Text), Convert.ToDouble(textBoxAdjuster5.Text), Convert.ToDouble(textBoxAdjuster6.Text), Convert.ToDouble(textBoxAdjuster7.Text), Convert.ToDouble(textBoxAdjuster8.Text) };
                double[] pickadjuster = { Convert.ToDouble(textBoxPickAdjuster1.Text), Convert.ToDouble(textBoxPickAdjuster2.Text), Convert.ToDouble(textBoxPickAdjuster3.Text), Convert.ToDouble(textBoxPickAdjuster4.Text), Convert.ToDouble(textBoxPickAdjuster5.Text), Convert.ToDouble(textBoxPickAdjuster6.Text), Convert.ToDouble(textBoxPickAdjuster7.Text), Convert.ToDouble(textBoxPickAdjuster8.Text) };
                double[] ext = { Convert.ToDouble(textBoxE1.Text), Convert.ToDouble(textBoxE2.Text), Convert.ToDouble(textBoxE3.Text), Convert.ToDouble(textBoxE4.Text) };
                double[] intx = { Convert.ToDouble(textBoxI1X.Text) };
                double[] inty = { Convert.ToDouble(textBoxI1Y.Text) };
                double[] pd = { Convert.ToDouble(textBoxpd.Text) };
                double aspectratio = Convert.ToDouble(textBoxAspectRatio.Text);

                //Adjust warehouse area until warehouse is fit
                int increased = 0;
                int decreased = 0;
                bool warehousefit = false;
                bool finalize = false;
                bool resize = checkBoxResize.Checked;
                if (resize) mywh.setArea(40000);//Set to a fixed point so it can find the best area from the same starting point
                do
                {
                    mywh.resetNetwork();
                    mywh.setSKUs(myskus);
                    switch (comboBoxDesignClass.SelectedItem.ToString())
                    {
                        case "0-0-0":
                            if (!mywh.create000Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                            {
                                textBoxNeighbors.AppendText("Invalid design parameters\n");
                                return;
                            }
                            break;
                        case "2-0-1":
                            if (!mywh.create201Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                            {
                                textBoxNeighbors.AppendText("Invalid design parameters\n");
                                return;
                            }
                            break;
                        case "3-0-2":
                            if (!mywh.create302Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                            {
                                textBoxNeighbors.AppendText("Invalid design parameters\n");
                                return;
                            }
                            break;
                        case "3-0-3":
                            if (!mywh.create303Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                            {
                                textBoxNeighbors.AppendText("Invalid design parameters\n");
                                return;
                            }
                            break;
                        case "3-1-3":
                            if (!mywh.create313Warehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                            {
                                textBoxNeighbors.AppendText("Invalid design parameters\n");
                                return;
                            }
                            break;
                        case "4-0-2":
                            if (!mywh.create402Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                            {
                                textBoxNeighbors.AppendText("Invalid design parameters\n");
                                return;
                            }
                            break;
                        case "4-0-4":
                            if (!mywh.create404Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                            {
                                textBoxNeighbors.AppendText("Invalid design parameters\n");
                                return;
                            }
                            break;
                        case "4-0-5":
                            if (!mywh.create405Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                            {
                                textBoxNeighbors.AppendText("Invalid design parameters\n");
                                return;
                            }
                            break;
                        case "4-1-4":
                            if (!mywh.create414Warehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                            {
                                textBoxNeighbors.AppendText("Invalid design parameters\n");
                                return;
                            }
                            break;
                        case "4-2-5":
                            if (!mywh.create425Warehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                            {
                                textBoxNeighbors.AppendText("Invalid design parameters\n");
                                return;
                            }
                            break;
                        case "6-0-3":
                            if (!mywh.create603Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                            {
                                textBoxNeighbors.AppendText("Invalid design parameters\n");
                                return;
                            }
                            break;
                    }
                    if (!resize) break;
                    //Check warehouse size here before doing any other calculations and if size is not fit then adjust it approprately
                    int totalstoragelocations = mywh.totalNumberOfLocations();
                    if (increased > 0 && decreased > 0)//No exact number of locations but this is the smallest it can get then we stop
                    {
                        if (mywh.getSKUs().Count > totalstoragelocations)//This check is necessary because last iteration it could have been decreased
                        {
                            mywh.setArea(mywh.area / options.warehouseadjustmentfactor);//Increase area
                            increased++;
                            finalize = true;
                        }
                        else if (mywh.getSKUs().Count < totalstoragelocations || finalize == true)
                        {
                            warehousefit = true;
                            break;
                        }
                    }
                    if (mywh.getSKUs().Count > totalstoragelocations)
                    {
                        mywh.setArea(mywh.area / options.warehouseadjustmentfactor);//Increase area
                        increased++;
                    }
                    else if (mywh.getSKUs().Count < totalstoragelocations)
                    {
                        mywh.setArea(mywh.area * options.warehouseadjustmentfactor);//Decrease area
                        decreased++;
                    }
                    else if (mywh.getSKUs().Count == totalstoragelocations)
                    {
                        warehousefit = true;
                    }
                } while (!warehousefit);

                if (mywh.getSKUs().Count > mywh.totalNumberOfLocations())
                {
                    textBoxNeighbors.AppendText("Insufficient space\n");
                    labelTotalLocations.Text = "Total Locations: " + mywh.totalNumberOfLocations().ToString();
                    return;
                }
                //Set final area
                textBoxArea.Text = mywh.area.ToString("R");
                //Set width of the warehouse
                textBoxWHWidth.Text = mywh.getWidth().ToString();
                //Set depth of the warehouse
                textBoxWHDepth.Text = mywh.getDepth().ToString();
                //Set panel's width with consideration of margins of edge crossaisles
                panelDrawing.Width = Convert.ToInt32(mywh.getWidth() * m);
                //Set panel's height with consideration of margins of edge crossaisles
                panelDrawing.Height = Convert.ToInt32(mywh.getDepth() * m);
                myBitmap = new Bitmap(this.panelDrawing.Width, this.panelDrawing.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                //graphicsObj = this.panelDrawing.CreateGraphics();
                //Create graphics object for drawing lines and shapes in panel
                graphicsObj = Graphics.FromImage(myBitmap);
                graphicsObj.Clear(Color.White);
                //Create pen object for width and color of shapess
                mypen = new Pen(System.Drawing.Color.Blue, 1);
                //Set alignment to center so aisles are correctly aligned
                mypen.Alignment = PenAlignment.Center;


                TimeSpan elapsed1 = DateTime.Now - start1;
                DateTime start2 = DateTime.Now;
                //Create importantnodes shortest distances
                if (!mywh.usevisibilitygraph) mywh.createImportantNodeShortestDistances();
                TimeSpan elapsed2 = DateTime.Now - start2;
                DateTime start3 = DateTime.Now;
                TimeSpan elapsed9 = DateTime.Now - start2;
                TimeSpan elapsed10 = DateTime.Now - start2;
                TimeSpan elapsed11 = DateTime.Now - start2;
                //Calculate Shortest Path Distances for Locations
                if (!mywh.usevisibilitygraph) mywh.locationShortestDistance();
                else
                {
                    DateTime start9 = DateTime.Now;
                    mywh.createPolygonsandGraphNodes();
                    elapsed9 = DateTime.Now - start9;
                    DateTime start10 = DateTime.Now;
                    mywh.createVisibilityGraph();
                    elapsed10 = DateTime.Now - start10;
                    DateTime start11 = DateTime.Now;
                    mywh.fillGraphNodeDistances();
                    elapsed11 = DateTime.Now - start11;
                }
                //Calculate Shortest Path Distances to PD
                mywh.pdTotalDistances();
                TimeSpan elapsed3 = DateTime.Now - start3;
                DateTime start4 = DateTime.Now;
                //Fulfill Total Distances to Each Locations
                mywh.totalDistances();
                TimeSpan elapsed4 = DateTime.Now - start4;
                DateTime start5 = DateTime.Now;
                //mywh.colorManytoMany();
                //mywh.colorOnetoMany();
                mywh.rankLocations(Convert.ToDouble(textBoxAvgOrderSize.Text));
                mywh.colorOverall();
                TimeSpan elapsed5 = DateTime.Now - start5;
                DateTime start6 = DateTime.Now;
                //Draw created warehouse to panel
                drawWarehouse();
                TimeSpan elapsed6 = DateTime.Now - start6;
                labelTotalLocations.Text = "Total Locations: " + mywh.totalNumberOfLocations().ToString();
                DateTime start7 = DateTime.Now;
                al = new allocation(Convert.ToInt32(textBoxAllocationSeed.Text));
                int allocationmethod = 0;
                if (checkBoxStraightAllocation.Checked) allocationmethod = 1;
                int warehouseareafit = al.allocateSKUs(mywh.getSKUs(), mywh, allocationmethod);
                TimeSpan elapsed7 = DateTime.Now - start7;
                if (warehouseareafit < 0) { textBoxNeighbors.AppendText("Insufficient space\n"); return; }
                else if (warehouseareafit > 0) { textBoxNeighbors.AppendText("Too many empty locations\n"); return; };

                //al.randomized(myskus);
                mywh.setOrders(this.myorders);
                //mywh.randomizeOrders();
                double totalcost = 0;
                int samplesize = Convert.ToInt32(textBoxSampleSize.Text);
                DateTime start8 = DateTime.Now;
                //If you want to solve it with center aisles
                if (!mywh.usevisibilitygraph)
                {
                    //If you want to solve TSPs using parallel computing
                    if (comboBoxComputing.SelectedIndex == 0)
                    {
                        var sums = new ConcurrentBag<double>();
                        var sums2 = new ConcurrentBag<int>();
                        var sums3 = new ConcurrentBag<int>();
                        var sums4 = new ConcurrentBag<string>();
                        bool TSPConcorde = false;
                        bool ExportCheck = false;
                        if (checkBoxTSP.Checked) TSPConcorde = true;
                        if (checkBoxExport.Checked) ExportCheck = true;
                        Parallel.For(0, samplesize, k =>
                        //for (int k = 0; k < samplesize; k++)
                        {
                            warehouse tmpwh = mywh;
                            List<order> tmporders = mywh.getOrders();
                            routing rt = new routing();
                            //totalcost += rt.tsp(tmpwh, tmporders[k]);
                            double tourcost = 0;
                            bool LKHDONEonce = false;//This is used when LKH sometimes stucks because of an error so we let Concorde to solve it
                            DateTime start15 = DateTime.Now;
                            while (tourcost == 0)
                            {
                                if (TSPConcorde || LKHDONEonce)
                                {
                                    tourcost = rt.tspOptSteiner(tmpwh, tmporders[k], k);
                                    if (tourcost == 0) break;//This means that tour cost is really zero
                                }
                                else
                                {
                                    tourcost = rt.tspLKHSteiner(tmpwh, tmporders[k], k);
                                    LKHDONEonce = true;
                                }
                            }
                            sums.Add(tourcost);
                            if (ExportCheck)
                            {
                                sums2.Add(tmporders[k].getOrderSize());
                                sums3.Add(tmporders[k].getOrderID());
                                TimeSpan elapsed15 = DateTime.Now - start15;
                                sums4.Add(elapsed15.TotalSeconds.ToString());
                            }
                        });

                        //Excel csv export (optional)
                        if (checkBoxExport.Checked)
                        {
                            csvexport myexcel = new csvexport();
                            for (int i = 0; i < sums.Count; i++)
                            {
                                myexcel.addRow();
                                myexcel["tourcost"] = sums.ElementAt(i).ToString();
                                myexcel["toursize"] = sums2.ElementAt(i).ToString();
                                myexcel["orderid"] = sums3.ElementAt(i).ToString();
                                myexcel["time"] = sums4.ElementAt(i).ToString();
                            }
                            myexcel.exportToFile("export.csv");
                        }
                        totalcost = sums.Sum() / samplesize;
                    }
                    //If you want to solve it using distributed computing
                    else if (comboBoxComputing.SelectedIndex == 1)
                    {
                        bool TSPConcorde = false;
                        if (checkBoxTSP.Checked) TSPConcorde = true;
                        if (TSPConcorde)
                        {
                            socketservers mysocketservers = new socketservers();
                            mysocketservers.checkAvailableConcordeServers();
                            routing rt = new routing();
                            totalcost = rt.tspOptNetSteiner(mywh, samplesize, mysocketservers, comboBoxNetSchedule.SelectedIndex);
                        }
                        else
                        {
                            socketservers mysocketservers = new socketservers();
                            mysocketservers.checkAvailableConcordeServers();
                            routing rt = new routing();
                            totalcost = rt.tspLKHNetSteiner(mywh, samplesize, mysocketservers, comboBoxNetSchedule.SelectedIndex);
                        }
                    }
                    // If you want to solve it using single thread computing
                    else
                    {
                        List<double> sum = new List<double>();
                        List<int> sum2 = new List<int>();
                        List<int> sum3 = new List<int>();
                        List<string> sum4 = new List<string>();
                        bool TSPConcorde = false;
                        bool ExportCheck = false;
                        if (checkBoxTSP.Checked) TSPConcorde = true;
                        if (checkBoxExport.Checked) ExportCheck = true;
                        //Parallel.For(0, samplesize, k =>
                        for (int k = 0; k < samplesize; k++)
                        {
                            warehouse tmpwh = mywh;
                            List<order> tmporders = mywh.getOrders();
                            routing rt = new routing();
                            double tourcost = 0;
                            bool LKHdoneonce = false;
                            DateTime start15 = DateTime.Now;
                            while (tourcost == 0)
                            {
                                if (TSPConcorde || LKHdoneonce)
                                {
                                    tourcost = rt.tspOptSteiner(tmpwh, tmporders[k], k);
                                    if (tourcost == 0) break;//This means that tour cost is really zero
                                }
                                else
                                {
                                    tourcost = rt.tspLKHSteiner(tmpwh, tmporders[k], k);
                                    LKHdoneonce = true;
                                }
                            }
                            sum.Add(tourcost);
                            if (ExportCheck)
                            {
                                sum2.Add(tmporders[k].getOrderSize());
                                sum3.Add(tmporders[k].getOrderID());
                                TimeSpan elapsed15 = DateTime.Now - start15;
                                sum4.Add(elapsed15.TotalSeconds.ToString());
                            }
                        }

                        //Excel csv export (optional)
                        if (checkBoxExport.Checked)
                        {
                            csvexport myexcel = new csvexport();
                            for (int i = 0; i < sum.Count; i++)
                            {
                                myexcel.addRow();
                                myexcel["tourcost"] = sum.ElementAt(i).ToString();
                                myexcel["toursize"] = sum2.ElementAt(i).ToString();
                                myexcel["orderid"] = sum3.ElementAt(i).ToString();
                                myexcel["time"] = sum4.ElementAt(i).ToString();
                            }
                            myexcel.exportToFile("export.csv");
                        }
                        totalcost = sum.Sum() / samplesize;
                    }
                }
                else//using visibility graph
                {
                    //If you want to solve TSPs using parallel computing
                    if (comboBoxComputing.SelectedIndex == 0)
                    {
                        var sums = new ConcurrentBag<double>();
                        var sums2 = new ConcurrentBag<int>();
                        var sums3 = new ConcurrentBag<int>();
                        var sums4 = new ConcurrentBag<string>();
                        bool TSPConcorde = false;
                        bool ExportCheck = false;
                        if (checkBoxTSP.Checked) TSPConcorde = true;
                        if (checkBoxExport.Checked) ExportCheck = true;
                        Parallel.For(0, samplesize, k =>
                        //for (int k = 0; k < samplesize; k++)
                        {
                            warehouse tmpwh = mywh;
                            List<order> tmporders = mywh.getOrders();
                            routing rt = new routing();
                            //totalcost += rt.tsp(tmpwh, tmporders[k]);
                            double tourcost = 0;
                            bool LKHdoneonce = false;
                            DateTime start15 = DateTime.Now;
                            while (tourcost == 0)
                            {
                                if (TSPConcorde || LKHdoneonce)
                                {
                                    tourcost = rt.tspOptVisibility(tmpwh, tmporders[k], k);
                                    if (tourcost == 0) break;//This means that tour cost is really zero
                                }
                                else
                                {
                                    tourcost = rt.tspLKHVisibility(tmpwh, tmporders[k], k);
                                    LKHdoneonce = true;
                                }
                            }
                            sums.Add(tourcost);
                            if (ExportCheck)
                            {
                                sums2.Add(tmporders[k].getOrderSize());
                                sums3.Add(tmporders[k].getOrderID());
                                TimeSpan elapsed15 = DateTime.Now - start15;
                                sums4.Add(elapsed15.TotalSeconds.ToString());
                            }
                        });

                        //Excel csv export (optional)
                        if (checkBoxExport.Checked)
                        {
                            csvexport myexcel = new csvexport();
                            for (int i = 0; i < sums.Count; i++)
                            {
                                myexcel.addRow();
                                myexcel["tourcost"] = sums.ElementAt(i).ToString();
                                myexcel["toursize"] = sums2.ElementAt(i).ToString();
                                myexcel["orderid"] = sums3.ElementAt(i).ToString();
                                myexcel["time"] = sums4.ElementAt(i).ToString();
                            }
                            myexcel.exportToFile("export.csv");
                        }
                        totalcost = sums.Sum() / samplesize;
                    }
                    //If you want to solve it using distributed computing
                    else if(comboBoxComputing.SelectedIndex == 1)
                    {
                        bool TSPConcorde = false;
                        if (checkBoxTSP.Checked) TSPConcorde = true;
                        if (TSPConcorde)
                        {
                            socketservers mysocketservers = new socketservers();
                            mysocketservers.checkAvailableConcordeServers();
                            routing rt = new routing();
                            totalcost = rt.tspOptNetVisibility(mywh, samplesize, mysocketservers, comboBoxNetSchedule.SelectedIndex);
                        }
                        else
                        {
                            socketservers mysocketservers = new socketservers();
                            mysocketservers.checkAvailableConcordeServers();
                            routing rt = new routing();
                            totalcost = rt.tspLKHNetVisibility(mywh, samplesize, mysocketservers, comboBoxNetSchedule.SelectedIndex);
                        }
                    }
                    //If you want to solve TSPs using single thread
                    else
                    {
                        var sums = new ConcurrentBag<double>();
                        var sums2 = new ConcurrentBag<int>();
                        var sums3 = new ConcurrentBag<int>();
                        var sums4 = new ConcurrentBag<string>();
                        bool TSPConcorde = false;
                        bool ExportCheck = false;
                        if (checkBoxTSP.Checked) TSPConcorde = true;
                        if (checkBoxExport.Checked) ExportCheck = true;
                        //Parallel.For(0, samplesize, k =>
                        for (int k = 0; k < samplesize; k++)
                        {
                            warehouse tmpwh = mywh;
                            List<order> tmporders = mywh.getOrders();
                            routing rt = new routing();
                            //totalcost += rt.tsp(tmpwh, tmporders[k]);
                            double tourcost = 0;
                            bool LKHdoneonce = false;
                            DateTime start15 = DateTime.Now;
                            while (tourcost == 0)
                            {
                                if (TSPConcorde || LKHdoneonce)
                                {
                                    tourcost = rt.tspOptVisibility(tmpwh, tmporders[k], k);
                                    if (tourcost == 0) break;//This means that tour cost is really zero
                                }
                                else
                                {
                                    tourcost = rt.tspLKHVisibility(tmpwh, tmporders[k], k);
                                    LKHdoneonce = true;
                                }
                            }
                            sums.Add(tourcost);
                            if (ExportCheck)
                            {
                                sums2.Add(tmporders[k].getOrderSize());
                                sums3.Add(tmporders[k].getOrderID());
                                TimeSpan elapsed15 = DateTime.Now - start15;
                                sums4.Add(elapsed15.TotalSeconds.ToString());
                            }
                        };

                        //Excel csv export (optional)
                        if (checkBoxExport.Checked)
                        {
                            csvexport myexcel = new csvexport();
                            for (int i = 0; i < sums.Count; i++)
                            {
                                myexcel.addRow();
                                myexcel["tourcost"] = sums.ElementAt(i).ToString();
                                myexcel["toursize"] = sums2.ElementAt(i).ToString();
                                myexcel["orderid"] = sums3.ElementAt(i).ToString();
                                myexcel["time"] = sums4.ElementAt(i).ToString();
                            }
                            myexcel.exportToFile("export.csv");
                        }
                        totalcost = sums.Sum() / samplesize;
                    }
                }
                TimeSpan elapsed8 = DateTime.Now - start8;

                labelDistanceOutput.Text = "Average Distance: " + totalcost.ToString("0.0");
                //labelDistanceOutput.Text = "Average Distance: " + mywh.averageTotalDistancePerLocation().ToString();
                //labelDistanceOutput.Text = "Average Distance: " + mywh.averageDistancetoPDPerLocation().ToString();
                panelDrawing.BackgroundImage = myBitmap;
                //Delete everything in the panel and refresh
                panelDrawing.Refresh();
                //graphicsObj.Dispose();
                textBoxRegions.Text = "";
                for (int i = 0; i < mywh.regionedges.Count; i++)
                {
                    textBoxRegions.AppendText(
                        visualmath.calculateAngle(mywh.regionedges[i].getStart().getX(), mywh.regionedges[i].getStart().getY(), mywh.regionedges[i].getEnd().getX(), mywh.regionedges[i].getEnd().getY()).ToString() + "\t" +
                        mywh.regionedges[i].id.ToString() + "\t" +
                        mywh.regionedges[i].getStart().id.ToString() + "\t" +
                        mywh.regionedges[i].getStart().getX().ToString() + "\t" +
                        mywh.regionedges[i].getStart().getY().ToString() + "\t" +
                        mywh.regionedges[i].getEnd().id.ToString() + "\t" +
                        mywh.regionedges[i].getEnd().getX().ToString() + "\t" +
                        mywh.regionedges[i].getEnd().getY().ToString() + "\n");
                }
                textBoxNeighbors.Text = "";
                for (int i = 0; i < mywh.regions.Count; i++)
                {
                    textBoxNeighbors.AppendText("Region" + mywh.regions[i].getRegionID().ToString() + ":\t");
                    for (int j = 0; j < mywh.regions[i].regionedges.Count; j++)
                    {
                        textBoxNeighbors.AppendText(mywh.regions[i].regionedges[j].id.ToString() + " ");
                    }
                    textBoxNeighbors.AppendText("\n");
                }
                TimeSpan elapsed = DateTime.Now - start;
                textBoxNeighbors.AppendText("Create Single:" + elapsed1.ToString() + "\n");
                textBoxNeighbors.AppendText("Important Node Shortest Dist:" + elapsed2.ToString() + "\n");
                textBoxNeighbors.AppendText("Shortest Path Distances to Locations:" + elapsed3.ToString() + "\n");
                textBoxNeighbors.AppendText("Shortest Path Distances Create:" + elapsed4.ToString() + "\n");
                textBoxNeighbors.AppendText("Coloring:" + elapsed5.ToString() + "\n");
                textBoxNeighbors.AppendText("Drawing:" + elapsed6.ToString() + "\n");
                textBoxNeighbors.AppendText("Allocation:" + elapsed7.ToString() + "\n");
                textBoxNeighbors.AppendText("TSP:" + elapsed8.ToString() + "\n");
                textBoxNeighbors.AppendText("createPolygonsandGraphNodes" + elapsed9.ToString() + "\n");
                if(mywh.usevisibilitygraph) textBoxNeighbors.AppendText("graphNodes: " + mywh.graphnodes.Count().ToString() + "\n");
                textBoxNeighbors.AppendText("createVisibilityGraph:" + elapsed10.ToString() + "\n");
                textBoxNeighbors.AppendText("fillGraphNodeDistances:" + elapsed11.ToString() + "\n");
                textBoxNeighbors.AppendText("Total time:" + elapsed.ToString() + "\n");
                textBoxNeighbors.AppendText("Total time (ms):" + elapsed.TotalMilliseconds.ToString() + "\n");

                //csvexport myexcel1 = new csvexport();
                //for (int i = 0; i < mywh.visibilitygraphdistances.GetLength(0); i++)
                //{
                //    for (int j = 0; j < mywh.visibilitygraphdistances.GetLength(1); j++)
                //    {

                //        myexcel1.addRow();
                //        myexcel1["distance"] = mywh.visibilitygraphdistances[i, j].ToString();

                //    }
                //}
                //myexcel1.exportToFile("visibilitygraphdistancescreate.csv");
                //csvexport myexcel1 = new csvexport();
                //for (int i = 0; i < mywh.graphnodes.Length; i++)
                //{
                //    myexcel1.addRow();
                //    myexcel1["X"] = mywh.graphnodes[i].getX().ToString();
                //    myexcel1["Y"] = mywh.graphnodes[i].getY().ToString();
                //}
                //myexcel1.exportToFile("XYGraphNodes-create.csv");
            }
            else//This is general warehouse
            {
                DateTime start = DateTime.Now;
                //Set mywh object to null
                mywh = null;
                //Set Magnify factor
                m = (float)Convert.ToDouble(textBoxMagnify.Text);
                //Reset IDs
                edge.nextID = 0;
                region.nextID = 0;
                node.nextID = 0;
                //Create a new warehouse object
                mywh = new warehouse();
                //Set warehouse aspectratio
                mywh.aspectratio = Convert.ToDouble(textBoxAspectRatio.Text);
                //Set warehouse area
                mywh.setArea(Convert.ToDouble(textBoxArea.Text));
                //Set warehouse cross aisle width
                mywh.crossaislewidth = Convert.ToDouble(textBoxCrossAisleWidth.Text);
                //Set warehouse picking aisle width
                mywh.pickingaislewidth = Convert.ToDouble(textBoxPickingAisleWidth.Text);
                //Set warehouse picking location width
                mywh.pickinglocationwidth = Convert.ToDouble(textBoxLWidth.Text);
                //Set warehouse picking location depth
                mywh.pickinglocationdepth = Convert.ToDouble(textBoxLDepth.Text);
                //Set size of picker used for visibility graph
                mywh.pickersize = Convert.ToDouble(textBoxPickerSize.Text);
                //Set which graph will be used Steiner or Visibility Graph
                mywh.usevisibilitygraph = checkBoxVisibilityGraph.Checked;
                //Set average pick list size (this is used for best location calculation
                mywh.avgTourLength = Convert.ToDouble(textBoxAvgOrderSize.Text);

                //Set number of colors
                options.numbercolors = Convert.ToInt32(textBoxNumberColors.Text);
                DateTime start1 = DateTime.Now;
                //Create Warehouse in the background
                //mywh.createSingleCrossAisleWarehouse(Convert.ToDouble(textBoxAngle1.Text), Convert.ToDouble(textBoxAngle2.Text), Convert.ToDouble(textBoxAdjuster1.Text), Convert.ToDouble(textBoxAdjuster2.Text), mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth, Convert.ToDouble(textBoxE1.Text), Convert.ToDouble(textBoxE2.Text), Convert.ToDouble(textBoxpd.Text));
                double[] angle = { Convert.ToDouble(textBoxAngle1.Text), Convert.ToDouble(textBoxAngle2.Text), Convert.ToDouble(textBoxAngle3.Text), Convert.ToDouble(textBoxAngle4.Text), Convert.ToDouble(textBoxAngle5.Text), Convert.ToDouble(textBoxAngle6.Text), Convert.ToDouble(textBoxAngle7.Text), Convert.ToDouble(textBoxAngle8.Text) };
                double[] adjuster = { Convert.ToDouble(textBoxAdjuster1.Text), Convert.ToDouble(textBoxAdjuster2.Text), Convert.ToDouble(textBoxAdjuster3.Text), Convert.ToDouble(textBoxAdjuster4.Text), Convert.ToDouble(textBoxAdjuster5.Text), Convert.ToDouble(textBoxAdjuster6.Text), Convert.ToDouble(textBoxAdjuster7.Text), Convert.ToDouble(textBoxAdjuster8.Text) };
                double[] pickadjuster = { Convert.ToDouble(textBoxPickAdjuster1.Text), Convert.ToDouble(textBoxPickAdjuster2.Text), Convert.ToDouble(textBoxPickAdjuster3.Text), Convert.ToDouble(textBoxPickAdjuster4.Text), Convert.ToDouble(textBoxPickAdjuster5.Text), Convert.ToDouble(textBoxPickAdjuster6.Text), Convert.ToDouble(textBoxPickAdjuster7.Text), Convert.ToDouble(textBoxPickAdjuster8.Text) };
                double[] ext = { Convert.ToDouble(textBoxE1.Text), Convert.ToDouble(textBoxE2.Text), Convert.ToDouble(textBoxE3.Text), Convert.ToDouble(textBoxE4.Text) };
                double[] intx = { Convert.ToDouble(textBoxI1X.Text) };
                double[] inty = { Convert.ToDouble(textBoxI1Y.Text) };
                double[] pd = { Convert.ToDouble(textBoxpd.Text) };
                double aspectratio = Convert.ToDouble(textBoxAspectRatio.Text);

                bool[] connections = new bool[10];

                connections[0] = checkBoxC12.Checked;
                connections[1] = checkBoxC13.Checked;
                connections[2] = checkBoxC14.Checked;
                connections[3] = checkBoxC15.Checked;
                connections[4] = checkBoxC23.Checked;
                connections[5] = checkBoxC24.Checked;
                connections[6] = checkBoxC25.Checked;
                connections[7] = checkBoxC34.Checked;
                connections[8] = checkBoxC35.Checked;
                connections[9] = checkBoxC45.Checked;

                //Adjust warehouse area until warehouse is fit
                int increased = 0;
                int decreased = 0;
                bool warehousefit = false;
                bool finalize = false;
                bool resize = checkBoxResize.Checked;
                if (resize) mywh.setArea(40000);//Set to a fixed point so it can find the best area from the same starting point
                do
                {
                    mywh.resetNetwork();
                    mywh.setSKUs(myskus);
                    if (!mywh.createWarehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth, connections))
                    {
                        textBoxNeighbors.AppendText("Invalid design parameters\n");
                        for (int i = 0; i < mywh.problematicconnections.Count(); i++)
                        {
                            textBoxNeighbors.AppendText(mywh.problematicconnections[i].ToString() + " ");
                        }
                        textBoxNeighbors.AppendText("\n");
                        return;
                    }
                    if (!resize) break;
                    //Check warehouse size here before doing any other calculations and if size is not fit then adjust it approprately
                    int totalstoragelocations = mywh.totalNumberOfLocations();
                    if (increased > 0 && decreased > 0)//No exact number of locations but this is the smallest it can get then we stop
                    {
                        if (mywh.getSKUs().Count > totalstoragelocations)//This check is necessary because last iteration it could have been decreased
                        {
                            mywh.setArea(mywh.area / options.warehouseadjustmentfactor);//Increase area
                            increased++;
                            finalize = true;
                        }
                        else if (mywh.getSKUs().Count < totalstoragelocations || finalize == true)
                        {
                            warehousefit = true;
                            break;
                        }
                    }
                    if (mywh.getSKUs().Count > totalstoragelocations)
                    {
                        mywh.setArea(mywh.area / options.warehouseadjustmentfactor);//Increase area
                        increased++;
                    }
                    else if (mywh.getSKUs().Count < totalstoragelocations)
                    {
                        mywh.setArea(mywh.area * options.warehouseadjustmentfactor);//Decrease area
                        decreased++;
                    }
                    else if (mywh.getSKUs().Count == totalstoragelocations)
                    {
                        warehousefit = true;
                    }
                } while (!warehousefit);
                if(mywh.getSKUs().Count > mywh.totalNumberOfLocations())
                {
                    textBoxNeighbors.AppendText("Insufficient space\n");
                    labelTotalLocations.Text = "Total Locations: " + mywh.totalNumberOfLocations().ToString();
                    return;
                }
                //Set final area
                textBoxArea.Text = mywh.area.ToString("R");//R is for making sure the double is converted to string exactly without losing precision
                //Set width of the warehouse
                textBoxWHWidth.Text = mywh.getWidth().ToString();
                //Set depth of the warehouse
                textBoxWHDepth.Text = mywh.getDepth().ToString();
                //Set panel's width with consideration of margins of edge crossaisles
                panelDrawing.Width = Convert.ToInt32(mywh.getWidth() * m);
                //Set panel's height with consideration of margins of edge crossaisles
                panelDrawing.Height = Convert.ToInt32(mywh.getDepth() * m);
                myBitmap = new Bitmap(this.panelDrawing.Width, this.panelDrawing.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                //graphicsObj = this.panelDrawing.CreateGraphics();
                //Create graphics object for drawing lines and shapes in panel
                graphicsObj = Graphics.FromImage(myBitmap);
                graphicsObj.Clear(Color.White);
                //Create pen object for width and color of shapess
                mypen = new Pen(System.Drawing.Color.Blue, 1);
                //Set alignment to center so aisles are correctly aligned
                mypen.Alignment = PenAlignment.Center;

                TimeSpan elapsed1 = DateTime.Now - start1;
                DateTime start2 = DateTime.Now;
                //Create importantnodes shortest distances
                if (!mywh.usevisibilitygraph) mywh.createImportantNodeShortestDistances();
                TimeSpan elapsed2 = DateTime.Now - start2;
                DateTime start3 = DateTime.Now;
                TimeSpan elapsed9 = DateTime.Now - start2;
                TimeSpan elapsed10 = DateTime.Now - start2;
                TimeSpan elapsed11 = DateTime.Now - start2;
                //Calculate Shortest Path Distances for Locations
                if (!mywh.usevisibilitygraph) mywh.locationShortestDistance();
                else
                {
                    DateTime start9 = DateTime.Now;
                    mywh.createPolygonsandGraphNodes();
                    elapsed9 = DateTime.Now - start9;
                    DateTime start10 = DateTime.Now;
                    mywh.createVisibilityGraph();
                    elapsed10 = DateTime.Now - start10;
                    DateTime start11 = DateTime.Now;
                    mywh.fillGraphNodeDistances();
                    elapsed11 = DateTime.Now - start11;
                }
                //Calculate Shortest Path Distances to PD
                mywh.pdTotalDistances();
                TimeSpan elapsed3 = DateTime.Now - start3;
                DateTime start4 = DateTime.Now;
                //Fulfill Total Distances to Each Locations
                mywh.totalDistances();
                TimeSpan elapsed4 = DateTime.Now - start4;
                DateTime start5 = DateTime.Now;
                //mywh.colorManytoMany();
                //mywh.colorOnetoMany();
                mywh.rankLocations(Convert.ToDouble(textBoxAvgOrderSize.Text));
                mywh.colorOverall();
                TimeSpan elapsed5 = DateTime.Now - start5;
                DateTime start6 = DateTime.Now;
                //Draw created warehouse to panel
                this.drawWarehouse();
                TimeSpan elapsed6 = DateTime.Now - start5;
                labelTotalLocations.Text = "Total Locations: " + mywh.totalNumberOfLocations().ToString();
                DateTime start7 = DateTime.Now;
                int allocationmethod = 0;
                if (checkBoxStraightAllocation.Checked) allocationmethod = 1;
                al = new allocation(Convert.ToInt32(textBoxAllocationSeed.Text));
                int warehouseareafit = al.allocateSKUs(mywh.getSKUs(), mywh, allocationmethod);
                TimeSpan elapsed7 = DateTime.Now - start6;
                if (warehouseareafit < 0) { textBoxNeighbors.AppendText("Insufficient space\n"); return; }
                else if (warehouseareafit > 0) { textBoxNeighbors.AppendText("Too many empty locations\n"); return; };

                //al.randomized(myskus);
                mywh.setOrders(this.myorders);
                //mywh.randomizeOrders();
                double totalcost = 0;
                int samplesize = Convert.ToInt32(textBoxSampleSize.Text);
                //If you want to solve it on this computer
                DateTime start8 = DateTime.Now;
                if (!mywh.usevisibilitygraph)
                {
                    if (comboBoxComputing.SelectedIndex == 0)
                    {
                        var sums = new ConcurrentBag<double>();
                        var sums2 = new ConcurrentBag<int>();
                        var sums3 = new ConcurrentBag<int>();
                        bool TSPConcorde = false;
                        if (checkBoxTSP.Checked) TSPConcorde = true;

                        Parallel.For(0, samplesize, k =>
                        //for (int k = 0; k < samplesize; k++)
                        {
                            warehouse tmpwh = mywh;
                            List<order> tmporders = mywh.getOrders();
                            routing rt = new routing();
                            //totalcost += rt.tsp(tmpwh, tmporders[k]);
                            double tourcost = 0;
                            bool LKHdoneonce = false;
                            while (tourcost == 0)
                            {
                                if (TSPConcorde || LKHdoneonce)
                                {
                                    tourcost = rt.tspOptSteiner(tmpwh, tmporders[k], k);
                                    if (tourcost == 0) break;//This means that tour cost is really zero
                                }
                                else
                                {
                                    tourcost = rt.tspLKHSteiner(tmpwh, tmporders[k], k);
                                    LKHdoneonce = true;
                                }
                            }
                            sums.Add(tourcost);
                            sums2.Add(tmporders[k].getOrderSize());
                            sums3.Add(tmporders[k].getOrderID());
                        });

                        //Excel csv export (optional)
                        if (checkBoxExport.Checked)
                        {
                            csvexport myexcel = new csvexport();
                            for (int i = 0; i < sums.Count; i++)
                            {
                                myexcel.addRow();
                                myexcel["tourcost"] = sums.ElementAt(i).ToString();
                                myexcel["toursize"] = sums2.ElementAt(i).ToString();
                                myexcel["orderid"] = sums3.ElementAt(i).ToString();
                            }
                            myexcel.exportToFile("export.csv");
                        }
                        totalcost = sums.Sum() / samplesize;
                    }
                    //If you want to solve it using distributed computing
                    else if (comboBoxComputing.SelectedIndex == 1)
                    {
                        bool TSPConcorde = false;
                        if (checkBoxTSP.Checked) TSPConcorde = true;
                        if (TSPConcorde)
                        {
                            socketservers mysocketservers = new socketservers();
                            mysocketservers.checkAvailableConcordeServers();
                            routing rt = new routing();
                            totalcost = rt.tspOptNetSteiner(mywh, samplesize, mysocketservers, comboBoxNetSchedule.SelectedIndex);
                        }
                        else
                        {
                            socketservers mysocketservers = new socketservers();
                            mysocketservers.checkAvailableConcordeServers();
                            routing rt = new routing();
                            totalcost = rt.tspLKHNetSteiner(mywh, samplesize, mysocketservers, comboBoxNetSchedule.SelectedIndex);
                        }
                    }
                    else
                    {
                        var sums = new ConcurrentBag<double>();
                        var sums2 = new ConcurrentBag<int>();
                        var sums3 = new ConcurrentBag<int>();
                        bool TSPConcorde = false;
                        if (checkBoxTSP.Checked) TSPConcorde = true;

                        //Parallel.For(0, samplesize, k =>
                        for (int k = 0; k < samplesize; k++)
                        {
                            warehouse tmpwh = mywh;
                            List<order> tmporders = mywh.getOrders();
                            routing rt = new routing();
                            //totalcost += rt.tsp(tmpwh, tmporders[k]);
                            double tourcost = 0;
                            bool LKHdoneonce = false;
                            while (tourcost == 0)
                            {
                                if (TSPConcorde || LKHdoneonce)
                                {
                                    tourcost = rt.tspOptSteiner(tmpwh, tmporders[k], k);
                                    if (tourcost == 0) break;//This means that tour cost is really zero
                                }
                                else
                                {
                                    tourcost = rt.tspLKHSteiner(tmpwh, tmporders[k], k);
                                    LKHdoneonce = true;
                                }
                            }
                            sums.Add(tourcost);
                            sums2.Add(tmporders[k].getOrderSize());
                            sums3.Add(tmporders[k].getOrderID());
                        };

                        //Excel csv export (optional)
                        if (checkBoxExport.Checked)
                        {
                            csvexport myexcel = new csvexport();
                            for (int i = 0; i < sums.Count; i++)
                            {
                                myexcel.addRow();
                                myexcel["tourcost"] = sums.ElementAt(i).ToString();
                                myexcel["toursize"] = sums2.ElementAt(i).ToString();
                                myexcel["orderid"] = sums3.ElementAt(i).ToString();
                            }
                            myexcel.exportToFile("export.csv");
                        }
                        totalcost = sums.Sum() / samplesize;
                    }
                }
                else
                {
                    if (comboBoxComputing.SelectedIndex == 0)
                    {
                        var sums = new ConcurrentBag<double>();
                        var sums2 = new ConcurrentBag<int>();
                        var sums3 = new ConcurrentBag<int>();
                        bool TSPConcorde = false;
                        if (checkBoxTSP.Checked) TSPConcorde = true;

                        Parallel.For(0, samplesize, k =>
                        //for (int k = 0; k < samplesize; k++)
                        {
                            warehouse tmpwh = mywh;
                            List<order> tmporders = mywh.getOrders();
                            routing rt = new routing();
                            //totalcost += rt.tsp(tmpwh, tmporders[k]);
                            double tourcost = 0;
                            bool LKHdoneonce = false;
                            while (tourcost == 0)
                            {
                                if (TSPConcorde || LKHdoneonce)
                                {
                                    tourcost = rt.tspOptVisibility(tmpwh, tmporders[k], k);
                                    if (tourcost == 0) break;//This means that tour cost is really zero
                                }
                                else
                                {
                                    tourcost = rt.tspLKHVisibility(tmpwh, tmporders[k], k);
                                    LKHdoneonce = true;
                                }
                            }
                            sums.Add(tourcost);
                            sums2.Add(tmporders[k].getOrderSize());
                            sums3.Add(tmporders[k].getOrderID());
                        });

                        //Excel csv export (optional)
                        if (checkBoxExport.Checked)
                        {
                            csvexport myexcel = new csvexport();
                            for (int i = 0; i < sums.Count; i++)
                            {
                                myexcel.addRow();
                                myexcel["tourcost"] = sums.ElementAt(i).ToString();
                                myexcel["toursize"] = sums2.ElementAt(i).ToString();
                                myexcel["orderid"] = sums3.ElementAt(i).ToString();
                            }
                            myexcel.exportToFile("export.csv");
                        }
                        totalcost = sums.Sum() / samplesize;
                    }
                    //If you want to solve it using distributed computing
                    else if (comboBoxComputing.SelectedIndex == 1)
                    {
                        bool TSPConcorde = false;
                        if (checkBoxTSP.Checked) TSPConcorde = true;
                        if (TSPConcorde)
                        {
                            socketservers mysocketservers = new socketservers();
                            mysocketservers.checkAvailableConcordeServers();
                            routing rt = new routing();
                            totalcost = rt.tspOptNetVisibility(mywh, samplesize, mysocketservers, comboBoxNetSchedule.SelectedIndex);
                        }
                        else
                        {
                            socketservers mysocketservers = new socketservers();
                            mysocketservers.checkAvailableConcordeServers();
                            routing rt = new routing();
                            totalcost = rt.tspLKHNetVisibility(mywh, samplesize, mysocketservers, comboBoxNetSchedule.SelectedIndex);
                        }
                    }
                    else
                    {
                        var sums = new ConcurrentBag<double>();
                        var sums2 = new ConcurrentBag<int>();
                        var sums3 = new ConcurrentBag<int>();
                        bool TSPConcorde = false;
                        if (checkBoxTSP.Checked) TSPConcorde = true;

                        //Parallel.For(0, samplesize, k =>
                        for (int k = 0; k < samplesize; k++)
                        {
                            warehouse tmpwh = mywh;
                            List<order> tmporders = mywh.getOrders();
                            routing rt = new routing();
                            //totalcost += rt.tsp(tmpwh, tmporders[k]);
                            double tourcost = 0;
                            bool LKHdoneonce = false;
                            while (tourcost == 0)
                            {
                                if (TSPConcorde || LKHdoneonce)
                                {
                                    tourcost = rt.tspOptVisibility(tmpwh, tmporders[k], k);
                                    if (tourcost == 0) break;//This means that tour cost is really zero
                                }
                                else
                                {
                                    tourcost = rt.tspLKHVisibility(tmpwh, tmporders[k], k);
                                    LKHdoneonce = true;
                                }
                            }
                            sums.Add(tourcost);
                            sums2.Add(tmporders[k].getOrderSize());
                            sums3.Add(tmporders[k].getOrderID());
                        };

                        //Excel csv export (optional)
                        if (checkBoxExport.Checked)
                        {
                            csvexport myexcel = new csvexport();
                            for (int i = 0; i < sums.Count; i++)
                            {
                                myexcel.addRow();
                                myexcel["tourcost"] = sums.ElementAt(i).ToString();
                                myexcel["toursize"] = sums2.ElementAt(i).ToString();
                                myexcel["orderid"] = sums3.ElementAt(i).ToString();
                            }
                            myexcel.exportToFile("export.csv");
                        }
                        totalcost = sums.Sum() / samplesize;
                    }
                }
                TimeSpan elapsed8 = DateTime.Now - start7;

                labelDistanceOutput.Text = "Average Distance: " + totalcost.ToString("0.0");
                //labelDistanceOutput.Text = "Average Distance: " + mywh.averageTotalDistancePerLocation().ToString();
                //labelDistanceOutput.Text = "Average Distance: " + mywh.averageDistancetoPDPerLocation().ToString();
                panelDrawing.BackgroundImage = myBitmap;
                //Delete everything in the panel and refresh
                panelDrawing.Refresh();
                //graphicsObj.Dispose();
                textBoxRegions.Text = "";
                for (int i = 0; i < mywh.regionedges.Count; i++)
                {
                    textBoxRegions.AppendText(
                        visualmath.calculateAngle(mywh.regionedges[i].getStart().getX(), mywh.regionedges[i].getStart().getY(), mywh.regionedges[i].getEnd().getX(), mywh.regionedges[i].getEnd().getY()).ToString() + "\t" +
                        mywh.regionedges[i].id.ToString() + "\t" +
                        mywh.regionedges[i].getStart().id.ToString() + "\t" +
                        mywh.regionedges[i].getStart().getX().ToString() + "\t" +
                        mywh.regionedges[i].getStart().getY().ToString() + "\t" +
                        mywh.regionedges[i].getEnd().id.ToString() + "\t" +
                        mywh.regionedges[i].getEnd().getX().ToString() + "\t" +
                        mywh.regionedges[i].getEnd().getY().ToString() + "\n");
                }
                textBoxNeighbors.Text = "";
                for (int i = 0; i < mywh.regions.Count; i++)
                {
                    textBoxNeighbors.AppendText("Region" + mywh.regions[i].getRegionID().ToString() + ":\t");
                    for (int j = 0; j < mywh.regions[i].regionedges.Count; j++)
                    {
                        textBoxNeighbors.AppendText(mywh.regions[i].regionedges[j].id.ToString() + " ");
                    }
                    textBoxNeighbors.AppendText("\n");
                }
                TimeSpan elapsed = DateTime.Now - start;
                textBoxNeighbors.AppendText("Create Single:" + elapsed1.ToString() + "\n");
                textBoxNeighbors.AppendText("Important Node Shortest Dist:" + elapsed2.ToString() + "\n");
                textBoxNeighbors.AppendText("Shortest Path Distances to Locations:" + elapsed3.ToString() + "\n");
                textBoxNeighbors.AppendText("Shortest Path Distances Create:" + elapsed4.ToString() + "\n");
                textBoxNeighbors.AppendText("Coloring:" + elapsed5.ToString() + "\n");
                textBoxNeighbors.AppendText("Drawing:" + elapsed6.ToString() + "\n");
                textBoxNeighbors.AppendText("Allocation:" + elapsed7.ToString() + "\n");
                textBoxNeighbors.AppendText("TSP:" + elapsed8.ToString() + "\n");
                textBoxNeighbors.AppendText("createPolygonsandGraphNodes" + elapsed9.ToString() + "\n");
                if(mywh.usevisibilitygraph) textBoxNeighbors.AppendText("graphNodes: " + mywh.graphnodes.Count().ToString() + "\n");
                textBoxNeighbors.AppendText("createVisibilityGraph:" + elapsed10.ToString() + "\n");
                textBoxNeighbors.AppendText("fillGraphNodeDistances:" + elapsed11.ToString() + "\n");
                textBoxNeighbors.AppendText("Total time:" + elapsed.ToString() + "\n");
                textBoxNeighbors.AppendText("Total time (ms):" + elapsed.TotalMilliseconds.ToString() + "\n");
            }
        }

        /// <summary>
        /// Draws a warehouse to the form
        /// </summary>
        private void drawWarehouse()
        {
            //this.drawRegionEdges();
            this.drawPickingAisles();
            this.drawPDPoint();
            //this.drawConnectivity();
            //this.drawPolygons();
        }

        /// <summary>
        /// Draws cross aisles to the form
        /// </summary>
        private void drawRegionEdges()
        {
            for (int i = 0; i < mywh.regionedges.Count; i++)
            {
                mypen.Color = System.Drawing.Color.Black;
                mypen.Width = 2 * m;
                graphicsObj.DrawEllipse(mypen, ((float)mywh.regionedges[i].getStart().getX() - 1) * m, ((float)mywh.regionedges[i].getStart().getY() - 1) * m, m, m);
                graphicsObj.DrawEllipse(mypen, ((float)mywh.regionedges[i].getEnd().getX() - 1) * m, ((float)mywh.regionedges[i].getEnd().getY() - 1) * m, m, m);
                mypen.Color = System.Drawing.Color.Gray;
                mypen.Width = (float)mywh.regionedges[i].width * m;
                graphicsObj.DrawLine(mypen, (float)mywh.regionedges[i].getStart().getX() * m, (float)mywh.regionedges[i].getStart().getY() * m, (float)mywh.regionedges[i].getEnd().getX() * m, (float)mywh.regionedges[i].getEnd().getY() * m);
                //mypen.Color = System.Drawing.Color.Black;
                //mypen.Width = m;
                //graphicsObj.DrawLine(mypen, (float)mywh.regionedges[i].getStart().getX() * m, (float)mywh.regionedges[i].getStart().getY() * m, (float)mywh.regionedges[i].getEnd().getX() * m, (float)mywh.regionedges[i].getEnd().getY() * m);
            }
        }

        private void drawPDPoint()
        {
            for (int i = 0; i < mywh.pdnodes.Count; i++)
            {
                mypen.Color = System.Drawing.Color.Black;
                mypen.Width = 2 * m;
                graphicsObj.DrawEllipse(mypen, ((float)mywh.pdnodes[i].getX() - 1) * m, ((float)mywh.pdnodes[i].getY() - 1) * m, m, m);
                //using (System.IO.StreamWriter file =
                //new System.IO.StreamWriter(@"C:\concorde\PDCoordinates.txt", true))
                //{
                //    file.WriteLine(mywh.pdnodes[i].getX().ToString() + "\t" + mywh.pdnodes[i].getY().ToString());
                //}
            }
        }

        private void drawConnectivity()
        {
            int mycounter = 0;
            if (mywh.connectivity == null) return;
            for (int i = 0; i < mywh.connectivity.GetLength(0); i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (mywh.connectivity[i, j])
                    {
                        mypen.Color = System.Drawing.Color.Black;
                        mypen.Width = (float)m/10;
                        graphicsObj.DrawLine(mypen, (float)mywh.graphnodes[i].getX() * m, (float)mywh.graphnodes[i].getY() * m, (float)mywh.graphnodes[j].getX() * m, (float)mywh.graphnodes[j].getY() * m);
                        mycounter++;
                    }
                }
            }
            int finalcount = mycounter;
        }

        /// <summary>
        /// Draws picking aisles to the form
        /// </summary>
        private void drawPickingAisles()
        {
            for (int i = 0; i < mywh.regions.Count; i++)
            {
                for (int j = 0; j < mywh.regions[i].pickingaisleedges.Count; j++)
                {
                    //mypen.Color = System.Drawing.Color.LightGray;
                    //mypen.Width = (float)mywh.regions[i].pickingaisleedges[j].width * m;
                    //graphicsObj.DrawLine(mypen, (float)mywh.regions[i].pickingaisleedges[j].getStart().getX() * m, (float)mywh.regions[i].pickingaisleedges[j].getStart().getY() * m, (float)mywh.regions[i].pickingaisleedges[j].getEnd().getX() * m, (float)mywh.regions[i].pickingaisleedges[j].getEnd().getY() * m);
                    //mypen.Color = System.Drawing.Color.Black;
                    //mypen.Width = 2 * m;
                    //graphicsObj.DrawEllipse(mypen, ((float)mywh.regions[i].pickingaisleedges[j].getStart().getX() - 1) * m, ((float)mywh.regions[i].pickingaisleedges[j].getStart().getY() - 1) * m, m, m);
                    //graphicsObj.DrawEllipse(mypen, ((float)mywh.regions[i].pickingaisleedges[j].getEnd().getX() - 1) * m, ((float)mywh.regions[i].pickingaisleedges[j].getEnd().getY() - 1) * m, m, m);
                    //mypen.Color = System.Drawing.Color.Black;
                    //mypen.Width = m;
                    //graphicsObj.DrawLine(mypen, (float)mywh.regions[i].pickingaisleedges[j].getStart().getX() * m, (float)mywh.regions[i].pickingaisleedges[j].getStart().getY() * m, (float)mywh.regions[i].pickingaisleedges[j].getEnd().getX() * m, (float)mywh.regions[i].pickingaisleedges[j].getEnd().getY() * m);
                    drawPickingAisleLocations(mywh.regions[i].pickingaisleedges[j]);
                }
            }
        }

        /// <summary>
        /// Draws picking aisle locations for given picking aisle
        /// </summary>
        /// <param name="p_pickingaisle">Picking Aisle</param>
        private void drawPickingAisleLocations(edge p_pickingaisleedge)
        {   
            for (int i = 0; i < p_pickingaisleedge.getOnEdgeNodes().Count; i++)
            {
                int wavelength = 700 - Convert.ToInt32((Convert.ToDouble(p_pickingaisleedge.getOnEdgeNodes()[i].color + 1) / Convert.ToDouble(options.numbercolors)) * 320);
                mypen.Color = getColorFromWaveLength(wavelength);
                //mypen.Width = 2 * m;
                //mypen.Alignment = PenAlignment.Center;
                //graphicsObj.DrawEllipse(mypen, (float)p_pickingaisleedge.getOnEdgeNodes()[i].getX() * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].getY() * m, m, m);
                //pickingaislecoordinates.Add(p_pickingaisleedge.getOnEdgeNodes()[i].getX().ToString() + "\t" + p_pickingaisleedge.getOnEdgeNodes()[i].getY().ToString());
                //using (System.IO.StreamWriter file =
                //new System.IO.StreamWriter(@"C:\concorde\LocationCoordinates.txt", true))
                //{
                //    file.WriteLine(p_pickingaisleedge.getOnEdgeNodes()[i].getX().ToString() + "\t" + p_pickingaisleedge.getOnEdgeNodes()[i].getY().ToString());
                //}

                mypen.Width = 1 * m;
                if (p_pickingaisleedge.getOnEdgeNodes()[i].s1 != null)
                {
                    graphicsObj.DrawLine(mypen, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.X1 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.Y1 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.X2 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.Y2 * m);
                    graphicsObj.DrawLine(mypen, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.X2 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.Y2 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.X4 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.Y4 * m);
                    graphicsObj.DrawLine(mypen, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.X4 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.Y4 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.X3 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.Y3 * m);
                    graphicsObj.DrawLine(mypen, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.X3 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.Y3 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.X1 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s1.Y1 * m);
                }
                if (p_pickingaisleedge.getOnEdgeNodes()[i].s2 != null)
                {
                    graphicsObj.DrawLine(mypen, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.X1 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.Y1 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.X2 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.Y2 * m);
                    graphicsObj.DrawLine(mypen, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.X2 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.Y2 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.X4 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.Y4 * m);
                    graphicsObj.DrawLine(mypen, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.X4 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.Y4 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.X3 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.Y3 * m);
                    graphicsObj.DrawLine(mypen, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.X3 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.Y3 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.X1 * m, (float)p_pickingaisleedge.getOnEdgeNodes()[i].s2.Y1 * m);
                }
            }
        }

        private void drawPolygons()
        {
            //List<string> polygoncoordinates = new List<string>();
            if (mywh.connectivity == null) return;
            //Do dash pattern for polygons
            float[] dashValues = { 5, 2};
            mypen.DashPattern = dashValues;
            mypen.Color = System.Drawing.Color.Black;
            mypen.Width = m;
            for (int i = 0; i < mywh.polygons.Length; i++)
            {
                for (int j = 0; j < mywh.polygons[i].vectors.Count - 1; j++)
                {
                    graphicsObj.DrawLine(mypen, (float)mywh.polygons[i].vectors[j].X * m, (float)mywh.polygons[i].vectors[j].Y * m, (float)mywh.polygons[i].vectors[j + 1].X * m, (float)mywh.polygons[i].vectors[j + 1].Y * m);
                    //polygoncoordinates.Add(mywh.polygons[i].vectors[j].X.ToString() + "\t" + mywh.polygons[i].vectors[j].Y.ToString());
                }
                graphicsObj.DrawLine(mypen, (float)mywh.polygons[i].vectors[mywh.polygons[i].vectors.Count - 1].X * m, (float)mywh.polygons[i].vectors[mywh.polygons[i].vectors.Count - 1].Y * m, (float)mywh.polygons[i].vectors[0].X * m, (float)mywh.polygons[i].vectors[0].Y * m);
                //polygoncoordinates.Add(mywh.polygons[i].vectors[mywh.polygons[i].vectors.Count - 1].X.ToString() + "\t" + mywh.polygons[i].vectors[mywh.polygons[i].vectors.Count - 1].Y.ToString());
            }
            //System.IO.File.WriteAllLines(@"C:\concorde\PolygonCoordinates.txt", polygoncoordinates);
        }

        private Color getColorFromWaveLength(int Wavelength)
        {
            double Gamma = 1.00;
            int IntensityMax = 255;
            double Blue;
            double Green;
            double Red;
            double Factor;
            if (Wavelength >= 350 && Wavelength <= 439)
            {
                Red = -(Wavelength - 440d) / (440d - 350d);
                Green = 0.0;
                Blue = 1.0;
            }
            else if (Wavelength >= 440 && Wavelength <= 489)
            {
                Red = 0.0;
                Green = (Wavelength - 440d) / (490d - 440d);
                Blue = 1.0;
            }
            else if (Wavelength >= 490 && Wavelength <= 509)
            {
                Red = 0.0;
                Green = 1.0;
                Blue = -(Wavelength - 510d) / (510d - 490d);
            }
            else if (Wavelength >= 510 && Wavelength <= 579)
            {
                Red = (Wavelength - 510d) / (580d - 510d);
                Green = 1.0;
                Blue = 0.0;
            }
            else if (Wavelength >= 580 && Wavelength <= 644)
            {
                Red = 1.0;
                Green = -(Wavelength - 645d) / (645d - 580d);
                Blue = 0.0;
            }
            else if (Wavelength >= 645 && Wavelength <= 780)
            {
                Red = 1.0;
                Green = 0.0;
                Blue = 0.0;
            }
            else
            {
                Red = 0.0;
                Green = 0.0;
                Blue = 0.0;
            }
            if (Wavelength >= 350 && Wavelength <= 419)
            {
                Factor = 0.3 + 0.7 * (Wavelength - 350d) / (420d - 350d);
            }
            else if (Wavelength >= 420 && Wavelength <= 700)
            {
                Factor = 1.0;
            }
            else if (Wavelength >= 701 && Wavelength <= 780)
            {
                Factor = 0.3 + 0.7 * (780d - Wavelength) / (780d - 700d);
            }
            else
            {
                Factor = 0.0;
            }
            int R = this.factorAdjust(Red, Factor, IntensityMax, Gamma);
            int G = this.factorAdjust(Green, Factor, IntensityMax, Gamma);
            int B = this.factorAdjust(Blue, Factor, IntensityMax, Gamma);
            return Color.FromArgb(R, G, B);
        }

        private int factorAdjust(double Color, double Factor, int IntensityMax, double Gamma)
        {
            if (Color == 0.0)
            {
                return 0;
            }
            else
            {
                return (int)Math.Round(IntensityMax * Math.Pow(Color * Factor, Gamma));
            }
        }

        /// <summary>
        /// Resets warehouse graphics
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonReset_Click(object sender, EventArgs e)
        {
            panelDrawing.Width = Convert.ToInt32(Convert.ToDouble(textBoxWHWidth.Text) * Convert.ToDouble(textBoxMagnify.Text));
            panelDrawing.Height = Convert.ToInt32(Convert.ToDouble(textBoxWHDepth.Text) * Convert.ToDouble(textBoxMagnify.Text));
            graphicsObj = Graphics.FromImage(myBitmap);
            graphicsObj.Clear(Color.White);
            panelDrawing.Refresh();
            graphicsObj.Dispose();
            labelTotalLocations.Text = "Total Locations:";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown) return;

            // Confirm user wants to close
            switch (MessageBox.Show(this, "Are you sure you want to exit?", "Closing", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                case DialogResult.No:
                    e.Cancel = true;
                    break;
                default:
                    break;
            }
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(myBitmap, 0, 0);
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printBitmap();
        }

        private void printBitmap()
        {
            printDialog1.ShowDialog();
            printDocument1.PrinterSettings = printDialog1.PrinterSettings;
            printDocument1.Print();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Png Image (.png)|*.png|Jpeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.ShowDialog();
            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                System.IO.FileStream fs =
                   (System.IO.FileStream)saveFileDialog1.OpenFile();
                // Saves the Image in the appropriate ImageFormat based upon the
                // File type selected in the dialog box.
                // NOTE that the FilterIndex property is one-based.
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        myBitmap.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 2:
                        myBitmap.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Bmp);
                        break;

                    case 3:
                        myBitmap.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                }

                fs.Close();
            }
            
        }

        public void saveBitmap(string fname)
        {
            myBitmap.Save(fname, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        private void panelDrawing_MouseClick(object sender, MouseEventArgs e)
        {
            Point screenPos = System.Windows.Forms.Cursor.Position;
            MessageBox.Show(this.panelDrawing.PointToClient(screenPos).ToString());
        }

        private void exporttocsv()
        {
            csvexport myexcel = new csvexport();
            for (int i = 0; i < mywh.regions.Count; i++)
            {
                for (int j = 0; j < mywh.regions[i].pickingaisleedges.Count; j++)
                {
                    for (int k = 0; k < mywh.regions[i].pickingaisleedges[j].getOnEdgeNodes().Count; k++)
                    {
                        myexcel.addRow();
                        myexcel["x"] = mywh.regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getX().ToString();
                        myexcel["y"] = mywh.regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getY().ToString();
                        myexcel["total distance"] = mywh.regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].totaldistance.ToString();
                    }
                }
            }
            myexcel.exportToFile("export.csv");
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exporttocsv();
        }

        public void drawCompleteWarehouse(double p_width, double p_depth)
        {
            //Set panel's width with consideration of margins of edge crossaisles
            panelDrawing.Width = Convert.ToInt32(p_width * m);
            //Set panel's height with consideration of margins of edge crossaisles
            panelDrawing.Height = Convert.ToInt32(p_depth * m);
            myBitmap = new Bitmap(this.panelDrawing.Width, this.panelDrawing.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            graphicsObj = Graphics.FromImage(myBitmap);
            graphicsObj.Clear(Color.White);
            //Create pen object for width and color of shapess
            mypen = new Pen(System.Drawing.Color.Blue, m);
            //Set alignment to center so aisles are correctly aligned
            mypen.Alignment = PenAlignment.Center;
            //Draw created warehouse to panel
            this.drawWarehouse();
            panelDrawing.BackgroundImage = myBitmap;
            //Delete everything in the panel and refresh
            panelDrawing.Refresh();
            graphicsObj.Dispose();
        }

        private void buttonSolveES_Click(object sender, EventArgs e)
        {
            solveES();
        }

        private void solveES()
        {
            //Used for auto conversion part if set to true then autoconversion works different way if set to false then it needs focus on text
            solving = true;
            //Set mywh object to null
            mywh = null;
            //Set Magnify factor
            m = (float)Convert.ToDouble(textBoxMagnify.Text);
            //Reset IDs
            edge.nextID = 0;
            region.nextID = 0;
            node.nextID = 0;
            //Create a new warehouse object
            mywh = new warehouse();
            //Set warehouse area
            mywh.area = 60000.0;//We set it this way so it will always produce the same result with the same seed number in ES
            //Set warehouse cross aisle width
            mywh.crossaislewidth = Convert.ToDouble(textBoxCrossAisleWidth.Text);
            //Set warehouse picking aisle width
            mywh.pickingaislewidth = Convert.ToDouble(textBoxPickingAisleWidth.Text);
            //Set warehouse picking location width
            mywh.pickinglocationwidth = Convert.ToDouble(textBoxLWidth.Text);
            //Set warehouse picking location depth
            mywh.pickinglocationdepth = Convert.ToDouble(textBoxLDepth.Text);
            //Set size of picker used for visibility graph
            mywh.pickersize = Convert.ToDouble(textBoxPickerSize.Text);
            //Set which graph will be used Steiner or Visibility Graph
            mywh.usevisibilitygraph = checkBoxVisibilityGraph.Checked;
            mywh.setSKUs(myskus);
            mywh.setOrders(myorders);
            mywh.avgTourLength = Convert.ToDouble(textBoxAvgOrderSize.Text);
            //mywh.randomizeOrders();
            //Set number of colors
            options.numbercolors = Convert.ToInt32(textBoxNumberColors.Text);

            int M, lambda, seed, iteration, counter;
            double sigma, successrule;
            M = Convert.ToInt32(textBoxMu.Text);
            lambda = Convert.ToInt32(textBoxLambda.Text);
            seed = Convert.ToInt32(textBoxESSeed.Text);
            iteration = Convert.ToInt32(textBoxESIteration.Text);
            sigma = Convert.ToDouble(textBoxSigma.Text);
            counter = Convert.ToInt32(textBoxESCounter.Text);
            successrule = Convert.ToDouble(textBoxESSuccessRule.Text);

            options.lowerattempts = 0;
            options.upperattempts = 0;
            options.totalattempts = 0;

            if (comboBoxDesignClass.SelectedItem.ToString() != "All")
            {
                double[] l = {   Convert.ToDouble(textBoxAspectRatioL.Text),
                             Convert.ToDouble(textBoxE1L.Text),
                             Convert.ToDouble(textBoxE2L.Text),
                             Convert.ToDouble(textBoxE3L.Text),
                             Convert.ToDouble(textBoxE4L.Text),
                             Convert.ToDouble(textBoxI1XL.Text),
                             Convert.ToDouble(textBoxI1YL.Text),
                             Convert.ToDouble(textBoxPDL.Text),
                             Convert.ToDouble(textBoxAngle1L.Text),
                             Convert.ToDouble(textBoxAngle2L.Text),
                             Convert.ToDouble(textBoxAngle3L.Text),
                             Convert.ToDouble(textBoxAngle4L.Text),
                             Convert.ToDouble(textBoxAngle5L.Text),
                             Convert.ToDouble(textBoxAngle6L.Text),
                             Convert.ToDouble(textBoxAdjuster1L.Text),
                             Convert.ToDouble(textBoxAdjuster2L.Text),
                             Convert.ToDouble(textBoxAdjuster3L.Text),
                             Convert.ToDouble(textBoxAdjuster4L.Text),
                             Convert.ToDouble(textBoxAdjuster5L.Text),
                             Convert.ToDouble(textBoxAdjuster6L.Text),
                             Convert.ToDouble(textBoxPickAdjuster1L.Text),
                             Convert.ToDouble(textBoxPickAdjuster2L.Text),
                             Convert.ToDouble(textBoxPickAdjuster3L.Text),
                             Convert.ToDouble(textBoxPickAdjuster4L.Text),
                             Convert.ToDouble(textBoxPickAdjuster5L.Text),
                             Convert.ToDouble(textBoxPickAdjuster6L.Text)
                         };
                double[] u = {   Convert.ToDouble(textBoxAspectRatioU.Text),
                             Convert.ToDouble(textBoxE1U.Text),
                             Convert.ToDouble(textBoxE2U.Text),
                             Convert.ToDouble(textBoxE3U.Text),
                             Convert.ToDouble(textBoxE4U.Text),
                             Convert.ToDouble(textBoxI1XU.Text),
                             Convert.ToDouble(textBoxI1YU.Text),
                             Convert.ToDouble(textBoxPDU.Text),
                             Convert.ToDouble(textBoxAngle1U.Text),
                             Convert.ToDouble(textBoxAngle2U.Text),
                             Convert.ToDouble(textBoxAngle3U.Text),
                             Convert.ToDouble(textBoxAngle4U.Text),
                             Convert.ToDouble(textBoxAngle5U.Text),
                             Convert.ToDouble(textBoxAngle6U.Text),
                             Convert.ToDouble(textBoxAdjuster1U.Text),
                             Convert.ToDouble(textBoxAdjuster2U.Text),
                             Convert.ToDouble(textBoxAdjuster3U.Text),
                             Convert.ToDouble(textBoxAdjuster4U.Text),
                             Convert.ToDouble(textBoxAdjuster5U.Text),
                             Convert.ToDouble(textBoxAdjuster6U.Text),
                             Convert.ToDouble(textBoxPickAdjuster1U.Text),
                             Convert.ToDouble(textBoxPickAdjuster2U.Text),
                             Convert.ToDouble(textBoxPickAdjuster3U.Text),
                             Convert.ToDouble(textBoxPickAdjuster4U.Text),
                             Convert.ToDouble(textBoxPickAdjuster5U.Text),
                             Convert.ToDouble(textBoxPickAdjuster6U.Text)
                         };

                DateTime start = DateTime.Now;
                bool allsearch = false;
                if (checkBoxAllSearch.Checked)
                {
                    allsearch = true;
                }
                int distcomp = comboBoxComputing.SelectedIndex;

                bool optimal = false;
                if (checkBoxTSP.Checked)
                {
                    optimal = true;
                }
                evolutionarystrategy es;
                if (!checkBoxNFT.Checked)
                {
                    es = new evolutionarystrategy(seed, M, lambda, u.Count(), iteration, sigma, counter, successrule, l, u, mywh, comboBoxDesignClass.SelectedItem.ToString(), allsearch, distcomp, optimal);
                }
                else
                {
                    double nft0 = Convert.ToDouble(textBoxNFT0.Text);
                    double nftlambda = Convert.ToDouble(textBoxNFTLambda.Text);
                    es = new evolutionarystrategy(seed, M, lambda, u.Count(), iteration, sigma, counter, successrule, l, u, mywh, comboBoxDesignClass.SelectedItem.ToString(), allsearch, distcomp, optimal, nft0, nftlambda);
                }
                es.createInitialParentPopulation();
                es.Solve(this.progressBarAlg, this.textBoxNeighbors, this, this.mywh, this.problemfolder);
                TimeSpan elapsed = DateTime.Now - start;
                double[] x = new double[u.Count()];
                x = es.getBest().getx();
                //csvexport myexcel = new csvexport();
                //for (int i = 0; i < es.getBestWarehouse().visibilitygraphdistances.GetLength(0); i++)
                //{
                //    for(int j = 0; j < es.getBestWarehouse().visibilitygraphdistances.GetLength(1); j++)
                //    {

                //            myexcel.addRow();
                //            myexcel["distance"] = es.getBestWarehouse().visibilitygraphdistances[i,j].ToString();

                //    }
                //}
                //myexcel.exportToFile("visibilitygraphdistances.csv");
                //csvexport myexcel = new csvexport();
                //for (int i = 0; i < es.getBestWarehouse().graphnodes.Length; i++)
                //{
                //    myexcel.addRow();
                //    myexcel["X"] = es.getBestWarehouse().graphnodes[i].getX().ToString();
                //    myexcel["Y"] = es.getBestWarehouse().graphnodes[i].getY().ToString();
                //}
                //myexcel.exportToFile("XYGraphNodes.csv");
                mywh.resetNetwork();
                //Reset IDs
                edge.nextID = 0;
                region.nextID = 0;
                node.nextID = 0;
                double[] angle = { 180 * x[8], 180 * x[9], 180 * x[10], 180 * x[11], 180 * x[12], 180 * x[13] };
                double[] adjuster = { x[14], x[15], x[16], x[17], x[18], x[19] };
                double[] pickadjuster = { x[20], x[21], x[22], x[23], x[24], x[25] };
                double[] ext = { x[1], x[2], x[3], x[4] };
                double[] intx = { x[5] };
                double[] inty = { x[6] };
                double[] pd = { x[7] };
                double aspectratio = x[0];
                mywh.aspectratio = x[0];
                mywh.setArea(es.getBestWarehouse().area);

                switch (comboBoxDesignClass.SelectedItem.ToString())
                {
                    case "0-0-0":
                        if (!mywh.create000Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                        {
                            textBoxNeighbors.AppendText("Invalid design parameters\n");
                            return;
                        }
                        break;
                    case "2-0-1":
                        if (!mywh.create201Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                        {
                            textBoxNeighbors.AppendText("Invalid design parameters\n");
                            return;
                        }
                        break;
                    case "3-0-2":
                        if (!mywh.create302Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                        {
                            textBoxNeighbors.AppendText("Invalid design parameters\n");
                            return;
                        }
                        break;
                    case "3-0-3":
                        if (!mywh.create303Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                        {
                            textBoxNeighbors.AppendText("Invalid design parameters\n");
                            return;
                        }
                        break;
                    case "3-1-3":
                        if (!mywh.create313Warehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                        {
                            textBoxNeighbors.AppendText("Invalid design parameters\n");
                            return;
                        }
                        break;
                    case "4-0-2":
                        if (!mywh.create402Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                        {
                            textBoxNeighbors.AppendText("Invalid design parameters\n");
                            return;
                        }
                        break;
                    case "4-0-4":
                        if (!mywh.create404Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                        {
                            textBoxNeighbors.AppendText("Invalid design parameters\n");
                            return;
                        }
                        break;
                    case "4-0-5":
                        if (!mywh.create405Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                        {
                            textBoxNeighbors.AppendText("Invalid design parameters\n");
                            return;
                        }
                        break;
                    case "4-1-4":
                        if (!mywh.create414Warehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                        {
                            textBoxNeighbors.AppendText("Invalid design parameters\n");
                            return;
                        }
                        break;
                    case "4-2-5":
                        if (!mywh.create425Warehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                        {
                            textBoxNeighbors.AppendText("Invalid design parameters\n");
                            return;
                        }
                        break;
                    case "6-0-3":
                        if (!mywh.create603Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth))
                        {
                            textBoxNeighbors.AppendText("Invalid design parameters\n");
                            return;
                        }
                        break;
                }

                if (!mywh.usevisibilitygraph) mywh.createImportantNodeShortestDistances();
                //Calculate Shortest Path Distances for Locations
                if (!mywh.usevisibilitygraph) mywh.locationShortestDistance();
                else
                {
                    mywh.createPolygonsandGraphNodes();
                    mywh.createVisibilityGraph();
                    mywh.fillGraphNodeDistances();
                }
                //Calculate Shortest Path Distances to PD
                mywh.pdTotalDistances();
                mywh.totalDistances();
                //mywh.colorManytoMany();
                //mywh.colorOnetoMany();
                mywh.rankLocations(Convert.ToDouble(textBoxAvgOrderSize.Text));
                mywh.colorOverall();

                textBoxArea.Text = mywh.area.ToString("R");
                textBoxAspectRatio.Text = (x[0]).ToString("R");
                textBoxE1.Text = (x[1]).ToString("R");
                textBoxE2.Text = (x[2]).ToString("R");
                textBoxE3.Text = (x[3]).ToString("R");
                textBoxE4.Text = (x[4]).ToString("R");
                textBoxI1X.Text = (x[5]).ToString("R");
                textBoxI1Y.Text = (x[6]).ToString("R");
                textBoxpd.Text = (x[7]).ToString();
                textBoxAngle1.Text = (x[8] * 180).ToString("R");
                textBoxAngle2.Text = (x[9] * 180).ToString("R");
                textBoxAngle3.Text = (x[10] * 180).ToString("R");
                textBoxAngle4.Text = (x[11] * 180).ToString("R");
                textBoxAngle5.Text = (x[12] * 180).ToString("R");
                textBoxAngle6.Text = (x[13] * 180).ToString("R");
                textBoxAdjuster1.Text = (x[14]).ToString("R");
                textBoxAdjuster2.Text = (x[15]).ToString("R");
                textBoxAdjuster3.Text = (x[16]).ToString("R");
                textBoxAdjuster4.Text = (x[17]).ToString("R");
                textBoxAdjuster5.Text = (x[18]).ToString("R");
                textBoxAdjuster6.Text = (x[19]).ToString("R");
                textBoxPickAdjuster1.Text = (x[20]).ToString("R");
                textBoxPickAdjuster2.Text = (x[21]).ToString("R");
                textBoxPickAdjuster3.Text = (x[22]).ToString("R");
                textBoxPickAdjuster4.Text = (x[23]).ToString("R");
                textBoxPickAdjuster5.Text = (x[24]).ToString("R");
                textBoxPickAdjuster6.Text = (x[25]).ToString("R");

                myBitmap = new Bitmap(Convert.ToInt32(mywh.getWidth() * m), Convert.ToInt32(mywh.getDepth() * m), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                //graphicsObj = this.panelDrawing.CreateGraphics();
                //Create graphics object for drawing lines and shapes in panel
                //
                graphicsObj = Graphics.FromImage(myBitmap);
                graphicsObj.Clear(Color.White);
                //Create pen object for width and color of shapess
                mypen = new Pen(System.Drawing.Color.Blue, m);
                //Set alignment to center so aisles are correctly aligned
                mypen.Alignment = PenAlignment.Center;
                //Draw created warehouse to panel
                this.drawWarehouse();
                labelTotalLocations.Text = "Total Locations: " + mywh.totalNumberOfLocations().ToString();
                //labelDistanceOutput.Text = "Average Distance: " + mywh.averageTotalDistancePerLocation().ToString();
                labelDistanceOutput.Text = "Average Distance: " + es.getCost().ToString("0.0");
                //labelDistanceOutput.Text = "Average Distance: " + mywh.averageDistancetoPDPerLocation().ToString();
                panelDrawing.BackgroundImage = myBitmap;
                //Set panel's width with consideration of margins of edge crossaisles
                panelDrawing.Width = Convert.ToInt32(mywh.getWidth() * m);
                //Set panel's height with consideration of margins of edge crossaisles
                panelDrawing.Height = Convert.ToInt32(mywh.getDepth() * m);
                //Delete everything in the panel and refresh
                panelDrawing.Refresh();
                graphicsObj.Dispose();
                textBoxRegions.Text = "";
                for (int i = 0; i < mywh.regionedges.Count; i++)
                {
                    textBoxRegions.AppendText(
                        mywh.regionedges[i].id.ToString() + "\t" +
                        mywh.regionedges[i].getStart().id.ToString() + "\t" +
                        mywh.regionedges[i].getStart().getX().ToString() + "\t" +
                        mywh.regionedges[i].getStart().getY().ToString() + "\t" +
                        mywh.regionedges[i].getEnd().id.ToString() + "\t" +
                        mywh.regionedges[i].getEnd().getX().ToString() + "\t" +
                        mywh.regionedges[i].getEnd().getY().ToString() + "\n");
                }
                //textBoxNeighbors.Text = "";
                for (int i = 0; i < mywh.regions.Count; i++)
                {
                    textBoxNeighbors.AppendText("Region" + mywh.regions[i].getRegionID().ToString() + ":\t");
                    for (int j = 0; j < mywh.regions[i].regionedges.Count; j++)
                    {
                        textBoxNeighbors.AppendText(mywh.regions[i].regionedges[j].id.ToString() + " ");
                    }
                    textBoxNeighbors.AppendText("\n");
                }
                textBoxNeighbors.AppendText("Sigma:" + es.getSigma().ToString() + "\n");
                textBoxNeighbors.AppendText("Success:" + es.getGeneralSuccess().ToString() + "\n");
                textBoxNeighbors.AppendText("Fail:" + es.getGeneralFail().ToString() + "\n");
                textBoxNeighbors.AppendText("Total time:" + elapsed.ToString() + "\n");
                textBoxNeighbors.AppendText("Lower Attempts:" + options.lowerattempts + "\n");
                textBoxNeighbors.AppendText("Upper Attempts:" + options.upperattempts + "\n");
                textBoxNeighbors.AppendText("Total Attempts:" + options.totalattempts + "\n");
                SystemSounds.Asterisk.Play();
            }
            else//New encoding
            {
                double[] l = {   Convert.ToDouble(textBoxAspectRatioL.Text),
                             Convert.ToDouble(textBoxE1L.Text),
                             Convert.ToDouble(textBoxE2L.Text),
                             Convert.ToDouble(textBoxE3L.Text),
                             Convert.ToDouble(textBoxE4L.Text),
                             Convert.ToDouble(textBoxI1XL.Text),
                             Convert.ToDouble(textBoxI1YL.Text),
                             Convert.ToDouble(textBoxPDL.Text),
                             Convert.ToDouble(textBoxAngle1L.Text),
                             Convert.ToDouble(textBoxAngle2L.Text),
                             Convert.ToDouble(textBoxAngle3L.Text),
                             Convert.ToDouble(textBoxAngle4L.Text),
                             Convert.ToDouble(textBoxAngle5L.Text),
                             Convert.ToDouble(textBoxAngle6L.Text),
                             Convert.ToDouble(textBoxAngle7L.Text),
                             Convert.ToDouble(textBoxAngle8L.Text),
                             Convert.ToDouble(textBoxAdjuster1L.Text),
                             Convert.ToDouble(textBoxAdjuster2L.Text),
                             Convert.ToDouble(textBoxAdjuster3L.Text),
                             Convert.ToDouble(textBoxAdjuster4L.Text),
                             Convert.ToDouble(textBoxAdjuster5L.Text),
                             Convert.ToDouble(textBoxAdjuster6L.Text),
                             Convert.ToDouble(textBoxAdjuster7L.Text),
                             Convert.ToDouble(textBoxAdjuster8L.Text),
                             Convert.ToDouble(textBoxPickAdjuster1L.Text),
                             Convert.ToDouble(textBoxPickAdjuster2L.Text),
                             Convert.ToDouble(textBoxPickAdjuster3L.Text),
                             Convert.ToDouble(textBoxPickAdjuster4L.Text),
                             Convert.ToDouble(textBoxPickAdjuster5L.Text),
                             Convert.ToDouble(textBoxPickAdjuster6L.Text),
                             Convert.ToDouble(textBoxPickAdjuster7L.Text),
                             Convert.ToDouble(textBoxPickAdjuster8L.Text),
                             Convert.ToDouble(textBoxC12L.Text),
                             Convert.ToDouble(textBoxC13L.Text),
                             Convert.ToDouble(textBoxC14L.Text),
                             Convert.ToDouble(textBoxC15L.Text),
                             Convert.ToDouble(textBoxC23L.Text),
                             Convert.ToDouble(textBoxC24L.Text),
                             Convert.ToDouble(textBoxC25L.Text),
                             Convert.ToDouble(textBoxC34L.Text),
                             Convert.ToDouble(textBoxC35L.Text),
                             Convert.ToDouble(textBoxC45L.Text)
                         };
                double[] u = {   Convert.ToDouble(textBoxAspectRatioU.Text),
                             Convert.ToDouble(textBoxE1U.Text),
                             Convert.ToDouble(textBoxE2U.Text),
                             Convert.ToDouble(textBoxE3U.Text),
                             Convert.ToDouble(textBoxE4U.Text),
                             Convert.ToDouble(textBoxI1XU.Text),
                             Convert.ToDouble(textBoxI1YU.Text),
                             Convert.ToDouble(textBoxPDU.Text),
                             Convert.ToDouble(textBoxAngle1U.Text),
                             Convert.ToDouble(textBoxAngle2U.Text),
                             Convert.ToDouble(textBoxAngle3U.Text),
                             Convert.ToDouble(textBoxAngle4U.Text),
                             Convert.ToDouble(textBoxAngle5U.Text),
                             Convert.ToDouble(textBoxAngle6U.Text),
                             Convert.ToDouble(textBoxAngle7U.Text),
                             Convert.ToDouble(textBoxAngle8U.Text),
                             Convert.ToDouble(textBoxAdjuster1U.Text),
                             Convert.ToDouble(textBoxAdjuster2U.Text),
                             Convert.ToDouble(textBoxAdjuster3U.Text),
                             Convert.ToDouble(textBoxAdjuster4U.Text),
                             Convert.ToDouble(textBoxAdjuster5U.Text),
                             Convert.ToDouble(textBoxAdjuster6U.Text),
                             Convert.ToDouble(textBoxAdjuster7U.Text),
                             Convert.ToDouble(textBoxAdjuster8U.Text),
                             Convert.ToDouble(textBoxPickAdjuster1U.Text),
                             Convert.ToDouble(textBoxPickAdjuster2U.Text),
                             Convert.ToDouble(textBoxPickAdjuster3U.Text),
                             Convert.ToDouble(textBoxPickAdjuster4U.Text),
                             Convert.ToDouble(textBoxPickAdjuster5U.Text),
                             Convert.ToDouble(textBoxPickAdjuster6U.Text),
                             Convert.ToDouble(textBoxPickAdjuster7U.Text),
                             Convert.ToDouble(textBoxPickAdjuster8U.Text),
                             Convert.ToDouble(textBoxC12U.Text),
                             Convert.ToDouble(textBoxC13U.Text),
                             Convert.ToDouble(textBoxC14U.Text),
                             Convert.ToDouble(textBoxC15U.Text),
                             Convert.ToDouble(textBoxC23U.Text),
                             Convert.ToDouble(textBoxC24U.Text),
                             Convert.ToDouble(textBoxC25U.Text),
                             Convert.ToDouble(textBoxC34U.Text),
                             Convert.ToDouble(textBoxC35U.Text),
                             Convert.ToDouble(textBoxC45U.Text)
                         };

                DateTime start = DateTime.Now;
                bool allsearch = false;
                if (checkBoxAllSearch.Checked)
                {
                    allsearch = true;
                }
                int distcomp = comboBoxComputing.SelectedIndex;

                bool optimal = false;
                if (checkBoxTSP.Checked)
                {
                    optimal = true;
                }

                int numberencparams = 52;//Total number of encoding variables in new encoding

                evolutionarystrategy es;
                if (!checkBoxNFT.Checked)
                {
                    es = new evolutionarystrategy(seed, M, lambda, numberencparams, iteration, sigma, counter, successrule, l, u, mywh, comboBoxDesignClass.SelectedItem.ToString(), allsearch, distcomp, optimal);
                }
                else
                {
                    double nft0 = Convert.ToDouble(textBoxNFT0.Text);
                    double nftlambda = Convert.ToDouble(textBoxNFTLambda.Text);
                    es = new evolutionarystrategy(seed, M, lambda, numberencparams, iteration, sigma, counter, successrule, l, u, mywh, comboBoxDesignClass.SelectedItem.ToString(), allsearch, distcomp, optimal, nft0, nftlambda);
                }
                es.createInitialParentPopulation();
                es.Solve(this.progressBarAlg, this.textBoxNeighbors, this, this.mywh, this.problemfolder);
                TimeSpan elapsed = DateTime.Now - start;
                double[] x = new double[u.Count()];
                x = es.getBest().getx();
                //csvexport myexcel = new csvexport();
                //for (int i = 0; i < es.getBestWarehouse().visibilitygraphdistances.GetLength(0); i++)
                //{
                //    for(int j = 0; j < es.getBestWarehouse().visibilitygraphdistances.GetLength(1); j++)
                //    {

                //            myexcel.addRow();
                //            myexcel["distance"] = es.getBestWarehouse().visibilitygraphdistances[i,j].ToString();

                //    }
                //}
                //myexcel.exportToFile("visibilitygraphdistances.csv");
                //csvexport myexcel = new csvexport();
                //for (int i = 0; i < es.getBestWarehouse().graphnodes.Length; i++)
                //{
                //    myexcel.addRow();
                //    myexcel["X"] = es.getBestWarehouse().graphnodes[i].getX().ToString();
                //    myexcel["Y"] = es.getBestWarehouse().graphnodes[i].getY().ToString();
                //}
                //myexcel.exportToFile("XYGraphNodes.csv");
                mywh.resetNetwork();
                //Reset IDs
                edge.nextID = 0;
                region.nextID = 0;
                node.nextID = 0;
                double[] angle = { 180 * x[8], 180 * x[9], 180 * x[10], 180 * x[11], 180 * x[12], 180 * x[13], 180 * x[14], 180 * x[15] };
                double[] adjuster = { x[16], x[17], x[18], x[19], x[20], x[21], x[22], x[23] };
                double[] pickadjuster = { x[24], x[25], x[26], x[27], x[28], x[29], x[30], x[31] };
                double[] ext = { x[1], x[2], x[3], x[4] };
                double[] intx = { x[5] };
                double[] inty = { x[6] };
                double[] pd = { x[7] };
                double aspectratio = x[0];
                mywh.aspectratio = x[0];
                mywh.setArea(es.getBestWarehouse().area);

                bool[] connections = new bool[numberencparams - l.Count()];
                int ii = 0;
                for (int j = l.Count(); j < numberencparams; j++)
                {
                    int indexencodingprobability = j - (numberencparams - l.Count());
                    if (x[j] == 0)//If that cross aisle exists then select that connection correct
                    {
                        connections[ii] = false;
                    }
                    else
                    {
                        connections[ii] = true;
                    }
                    ii++;
                }

                if (!mywh.createWarehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, mywh.crossaislewidth, mywh.pickingaislewidth, mywh.pickinglocationwidth, mywh.pickinglocationdepth, connections))
                {
                    textBoxNeighbors.AppendText("Invalid design parameters\n");
                    return;
                }

                if (!mywh.usevisibilitygraph) mywh.createImportantNodeShortestDistances();
                //Calculate Shortest Path Distances for Locations
                if (!mywh.usevisibilitygraph) mywh.locationShortestDistance();
                else
                {
                    mywh.createPolygonsandGraphNodes();
                    mywh.createVisibilityGraph();
                    mywh.fillGraphNodeDistances();
                }
                //Calculate Shortest Path Distances to PD
                mywh.pdTotalDistances();
                mywh.totalDistances();
                mywh.rankLocations(Convert.ToDouble(textBoxAvgOrderSize.Text));
                mywh.colorOverall();

                textBoxArea.Text = mywh.area.ToString("R");
                textBoxAspectRatio.Text = (x[0]).ToString("R");
                textBoxE1.Text = (x[1]).ToString("R");
                textBoxE2.Text = (x[2]).ToString("R");
                textBoxE3.Text = (x[3]).ToString("R");
                textBoxE4.Text = (x[4]).ToString("R");
                textBoxI1X.Text = (x[5]).ToString("R");
                textBoxI1Y.Text = (x[6]).ToString("R");
                textBoxpd.Text = (x[7]).ToString();
                textBoxAngle1.Text = (x[8] * 180).ToString("R");
                textBoxAngle2.Text = (x[9] * 180).ToString("R");
                textBoxAngle3.Text = (x[10] * 180).ToString("R");
                textBoxAngle4.Text = (x[11] * 180).ToString("R");
                textBoxAngle5.Text = (x[12] * 180).ToString("R");
                textBoxAngle6.Text = (x[13] * 180).ToString("R");
                textBoxAngle7.Text = (x[14] * 180).ToString("R");
                textBoxAngle8.Text = (x[15] * 180).ToString("R");
                textBoxAdjuster1.Text = (x[16]).ToString("R");
                textBoxAdjuster2.Text = (x[17]).ToString("R");
                textBoxAdjuster3.Text = (x[18]).ToString("R");
                textBoxAdjuster4.Text = (x[19]).ToString("R");
                textBoxAdjuster5.Text = (x[20]).ToString("R");
                textBoxAdjuster6.Text = (x[21]).ToString("R");
                textBoxAdjuster7.Text = (x[22]).ToString("R");
                textBoxAdjuster8.Text = (x[23]).ToString("R");
                textBoxPickAdjuster1.Text = (x[24]).ToString("R");
                textBoxPickAdjuster2.Text = (x[25]).ToString("R");
                textBoxPickAdjuster3.Text = (x[26]).ToString("R");
                textBoxPickAdjuster4.Text = (x[27]).ToString("R");
                textBoxPickAdjuster5.Text = (x[28]).ToString("R");
                textBoxPickAdjuster6.Text = (x[29]).ToString("R");
                textBoxPickAdjuster7.Text = (x[30]).ToString("R");
                textBoxPickAdjuster8.Text = (x[31]).ToString("R");
                if (x[42] == 0) checkBoxC12.Checked = false;
                else checkBoxC12.Checked = true;
                if (x[43] == 0) checkBoxC13.Checked = false;
                else checkBoxC13.Checked = true;
                if (x[44] == 0) checkBoxC14.Checked = false;
                else checkBoxC14.Checked = true;
                if (x[45] == 0) checkBoxC15.Checked = false;
                else checkBoxC15.Checked = true;
                if (x[46] == 0) checkBoxC23.Checked = false;
                else checkBoxC23.Checked = true;
                if (x[47] == 0) checkBoxC24.Checked = false;
                else checkBoxC24.Checked = true;
                if (x[48] == 0) checkBoxC25.Checked = false;
                else checkBoxC25.Checked = true;
                if (x[49] == 0) checkBoxC34.Checked = false;
                else checkBoxC34.Checked = true;
                if (x[50] == 0) checkBoxC35.Checked = false;
                else checkBoxC35.Checked = true;
                if (x[51] == 0) checkBoxC45.Checked = false;
                else checkBoxC45.Checked = true;
                myBitmap = new Bitmap(Convert.ToInt32(mywh.getWidth() * m), Convert.ToInt32(mywh.getDepth() * m), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                //graphicsObj = this.panelDrawing.CreateGraphics();
                //Create graphics object for drawing lines and shapes in panel
                //
                graphicsObj = Graphics.FromImage(myBitmap);
                graphicsObj.Clear(Color.White);
                //Create pen object for width and color of shapess
                mypen = new Pen(System.Drawing.Color.Blue, m);
                //Set alignment to center so aisles are correctly aligned
                mypen.Alignment = PenAlignment.Center;
                //Draw created warehouse to panel
                this.drawWarehouse();
                labelTotalLocations.Text = "Total Locations: " + mywh.totalNumberOfLocations().ToString();
                //labelDistanceOutput.Text = "Average Distance: " + mywh.averageTotalDistancePerLocation().ToString();
                labelDistanceOutput.Text = "Average Distance: " + es.getCost().ToString("0.0");
                //labelDistanceOutput.Text = "Average Distance: " + mywh.averageDistancetoPDPerLocation().ToString();
                panelDrawing.BackgroundImage = myBitmap;
                //Set panel's width with consideration of margins of edge crossaisles
                panelDrawing.Width = Convert.ToInt32(mywh.getWidth() * m);
                //Set panel's height with consideration of margins of edge crossaisles
                panelDrawing.Height = Convert.ToInt32(mywh.getDepth() * m);
                //Delete everything in the panel and refresh
                panelDrawing.Refresh();
                graphicsObj.Dispose();
                textBoxRegions.Text = "";
                for (int i = 0; i < mywh.regionedges.Count; i++)
                {
                    textBoxRegions.AppendText(
                        mywh.regionedges[i].id.ToString() + "\t" +
                        mywh.regionedges[i].getStart().id.ToString() + "\t" +
                        mywh.regionedges[i].getStart().getX().ToString() + "\t" +
                        mywh.regionedges[i].getStart().getY().ToString() + "\t" +
                        mywh.regionedges[i].getEnd().id.ToString() + "\t" +
                        mywh.regionedges[i].getEnd().getX().ToString() + "\t" +
                        mywh.regionedges[i].getEnd().getY().ToString() + "\n");
                }
                //textBoxNeighbors.Text = "";
                for (int i = 0; i < mywh.regions.Count; i++)
                {
                    textBoxNeighbors.AppendText("Region" + mywh.regions[i].getRegionID().ToString() + ":\t");
                    for (int j = 0; j < mywh.regions[i].regionedges.Count; j++)
                    {
                        textBoxNeighbors.AppendText(mywh.regions[i].regionedges[j].id.ToString() + " ");
                    }
                    textBoxNeighbors.AppendText("\n");
                }
                textBoxNeighbors.AppendText("Sigma:" + es.getSigma().ToString() + "\n");
                textBoxNeighbors.AppendText("Success:" + es.getGeneralSuccess().ToString() + "\n");
                textBoxNeighbors.AppendText("Fail:" + es.getGeneralFail().ToString() + "\n");
                textBoxNeighbors.AppendText("Total time:" + elapsed.ToString() + "\n");
                textBoxNeighbors.AppendText("Lower Attempts:" + options.lowerattempts + "\n");
                textBoxNeighbors.AppendText("Upper Attempts:" + options.upperattempts + "\n");
                textBoxNeighbors.AppendText("Total Attempts:" + options.totalattempts + "\n");
                SystemSounds.Asterisk.Play();
            }
        }

        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            options.numbercolors = Convert.ToInt32(textBoxNumberColors.Text);
            m = Convert.ToInt32(textBoxMagnify.Text);
            panelDrawing.Width = Convert.ToInt32(mywh.getWidth() * m);
            panelDrawing.Height = Convert.ToInt32(mywh.getDepth() * m);
            myBitmap = new Bitmap(this.panelDrawing.Width, this.panelDrawing.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            graphicsObj = Graphics.FromImage(myBitmap);
            graphicsObj.Clear(Color.White);
            mywh.rankLocations(Convert.ToDouble(textBoxAvgOrderSize.Text));
            mywh.colorOverall();
            this.drawWarehouse();
            panelDrawing.BackgroundImage = myBitmap;
            panelDrawing.Refresh();
        }

        private void buttonFindLocation_Click(object sender, EventArgs e)
        {
            int orderindex = Convert.ToInt32(textBoxOrderIndex.Text);
            if (orderindex < mywh.getOrders().Count)
            {
                pathCost(orderindex);
            }
            else
            {
                textBoxNeighbors.AppendText("Order Index is bigger than total number of orders\n");
            }
        }

        private void pathCost(int orderindex)
        {
            graphicsObj = Graphics.FromImage(myBitmap);
            graphicsObj.Clear(Color.White);
            this.drawWarehouse();

            for (int i = 0; i < mywh.getOrders()[orderindex].getOrderSize(); i++)
            {
                colorNode(mywh.getOrders()[orderindex].getOrderSkus()[i].location);
            }

            if(checkBoxExport.Checked)
            {
                List<node> tourlocations = new List<node>();
                tourlocations.Add(mywh.pdnodes[0]);
                for (int i = 0; i < mywh.getOrders()[orderindex].getOrderSize(); i++)
                {
                    if (!tourlocations.Contains(mywh.getOrders()[orderindex].getOrderSkus()[i].location))
                    {
                        tourlocations.Add(mywh.getOrders()[orderindex].getOrderSkus()[i].location);
                    }
                }

                int dimension = tourlocations.Count;
                int[,] cost = new int[dimension, dimension];

                for (int i = 0; i < dimension; i++)
                {
                    for(int j = 0; j < dimension; j++)
                    {
                        cost[i, j] = Convert.ToInt32(10*mywh.shortestPathDistanceTwoLocations(tourlocations[i], tourlocations[j]));
                    }
                }

                //csvexport myexcel = new csvexport();
                //for (int i = 0; i < dimension; i++)
                //{
                //    myexcel.addRow();
                //    myexcel["DM"] = i.ToString();
                //    for (int j = 0; j < dimension; j++)
                //    {
                //        myexcel[j.ToString()] = (Convert.ToDouble(cost[i, j]) / 10).ToString();
                //    }
                //}
                //System.IO.File.Delete("distancematrix.csv");
                //myexcel.exportToFile("distancematrix.csv");
            }
            panelDrawing.Refresh();
            routing rt = new routing();
            DateTime start = DateTime.Now;
            if (!mywh.usevisibilitygraph)
            {
                textBoxTSP.Text = rt.tspOptSteiner(mywh, mywh.getOrders()[orderindex], orderindex).ToString();
            }
            else
            {
                textBoxTSP.Text = rt.tspOptVisibility(mywh, mywh.getOrders()[orderindex], orderindex).ToString();
            }
            TimeSpan elapsed = DateTime.Now - start;
            textBoxNeighbors.AppendText("TSP Total time:" + elapsed.ToString() + "\n");
        }

        private void colorNode(node p_location)
        {
            mypen.Color = System.Drawing.Color.Black;
            mypen.Width = 3;
            graphicsObj.DrawEllipse(mypen, (float)p_location.getX() * m, (float)p_location.getY() * m, 2 * m, 2 * m);
        }
                            
        /// <summary>
        /// This button imports all orders as well as SKUs from a single csv file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonImport_Click(object sender, EventArgs e)
        {
            import();
        }

        /// <summary>
        /// Method for importing all orders as well as SKUs from a single csv file
        /// </summary>
        private void import()
        {
            myskus.Clear();
            myorders.Clear();
            int maxorderid = 0;
            int maxordersize = 0;
            //Browse for file
            OpenFileDialog ofd = new OpenFileDialog();
            //Only show .csv files
            ofd.Filter = "Microsoft Office Excel Comma Separated Values File|*.csv";
            DialogResult result = ofd.ShowDialog();
            //If the user selects a valid file 
            if (result == DialogResult.OK)
            {
                string fileName = ofd.FileName;
                FileInfo fileIn = new FileInfo(fileName);

                StreamReader reader = fileIn.OpenText();
                string[] lineIn;

                int previousorderid = 0;

                while (!reader.EndOfStream)
                {
                    lineIn = reader.ReadLine().Split(',');
                    //Validate values here
                    int orderid;
                    int.TryParse(lineIn[0], out orderid);
                    //Then continue to add current orderid the skus
                    if (orderid == previousorderid)
                    {
                        int skuID;
                        int.TryParse(lineIn[1], out skuID);
                        //Check if you have that SKU in your SKU list if not then add to the list
                        sku tmpsku = myskus.Find(item => item.ID == skuID);
                        if (tmpsku == null)
                        {
                            tmpsku = new sku(skuID);
                            myskus.Add(tmpsku);
                        }
                        //Add this sku to the current order
                        myorders[myorders.Count() - 1].addSKU(tmpsku);
                        //Increase frequency of this sku
                        tmpsku.addFrequency();
                    }
                    //Then create a new order and add the sku to the new order
                    else if(orderid != previousorderid)
                    {
                        //Order finished so check the size
                        if (previousorderid != 0)
                        {
                            if (maxordersize < myorders[myorders.Count()-1].getOrderSize())
                            {
                                maxorderid = myorders.Count() - 1;
                                maxordersize = myorders[myorders.Count() - 1].getOrderSize();
                            }
                        }
                        //Change previousorderid
                        previousorderid = orderid;
                        //Create a new order
                        order tmporder = new order(orderid);
                        myorders.Add(tmporder);
                        int skuID;
                        int.TryParse(lineIn[1], out skuID);
                        //Check if you have that SKU in your SKU list if not then add to the list
                        sku tmpsku = myskus.Find(item => item.ID == skuID);
                        if (tmpsku == null)
                        {
                            tmpsku = new sku(skuID);
                            myskus.Add(tmpsku);
                        }
                        //Add this sku to the current order
                        myorders[myorders.Count() - 1].addSKU(tmpsku);
                        //Increase frequency of this sku
                        tmpsku.addFrequency();
                    }
                }
                reader.Close();
                //Calculate Average Tour Length
                double totalnumberofpicks = 0;
                for (int i = 0; i < myorders.Count(); i++)
                {
                    totalnumberofpicks = totalnumberofpicks + myorders[i].getOrderSize();
                }
                avgTourLength = totalnumberofpicks / myorders.Count();

                labelTotalSKUS.Text = "# SKU: " + myskus.Count().ToString();
                labelTotalOrders.Text = "# Orders: " + myorders.Count().ToString();
                labelMaxOrder.Text = "MaxOrderID: " + maxorderid.ToString();
                labelMaxOrderSize.Text = "Max Order #: " + maxordersize.ToString();
                textBoxAvgOrderSize.Text = avgTourLength.ToString("#.##");
                textBoxSampleSize.Text = myorders.Count().ToString();
            }
        }

        /// <summary>
        /// This method is used if you import SKUs and Orders in two different files, this one imports SKUs and their pick probabilities
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonImportSKUs_Click(object sender, EventArgs e)
        {
            //If you want to generate orders based on Bender (1981)'s model
            if (comboBoxGenerate.SelectedIndex == 1)
            {
                myskus.Clear();
                double xPercentofItems = Convert.ToDouble(textBoxDemandSkewness20.Text) / 100;
                double yPercentofTotalDemand = Convert.ToDouble(textBoxDemandSkewness80.Text) / 100;
                if (xPercentofItems < yPercentofTotalDemand)
                {
                    double shapeFactor = (xPercentofItems - yPercentofTotalDemand * xPercentofItems) / (yPercentofTotalDemand - xPercentofItems);
                    int totalNumberSKUs = Convert.ToInt32(textBoxNumberSKUs.Text);
                    if (totalNumberSKUs > 0)
                    {
                        CDF = new double[totalNumberSKUs];
                        CDF[0] = ((1.0 + shapeFactor) * (1.0 / totalNumberSKUs)) / (shapeFactor + 1.0 / totalNumberSKUs);
                        //Create first SKU
                        sku tmpsku = new sku(1);
                        tmpsku.pickprobability = CDF[0] - 0;
                        myskus.Add(tmpsku);
                        for (int i = 1; i < totalNumberSKUs; i++)
                        {
                            CDF[i] = ((1.0 + shapeFactor) * ((double)(i + 1) / totalNumberSKUs)) / (shapeFactor + (double)(i + 1) / totalNumberSKUs);
                            tmpsku = new sku(i + 1);//id starts from 1 instead of 0
                            tmpsku.pickprobability = CDF[i] - CDF[i - 1];
                            myskus.Add(tmpsku);
                        }
                    }
                    labelTotalSKUS.Text = "# SKU: " + myskus.Count().ToString();
                }
                else
                {
                    textBoxNeighbors.AppendText("xPercent cannot be greater than or equal to yPercent\n");
                }
            }
            //If you want to generate all dual command combinations (all possible orders with 2 picks)
            else if (comboBoxGenerate.SelectedIndex == 3)
            {
                myskus.Clear();
                int totalNumberSKUs = Convert.ToInt32(textBoxNumberSKUs.Text);
                if (totalNumberSKUs > 0)
                {
                    sku tmpsku;
                    double pickprobability = 1 / totalNumberSKUs;//Each item are being picked with equal probability (uniform demand)
                    for (int i = 0; i < totalNumberSKUs; i++)
                    {
                        tmpsku = new sku(i + 1);//id starts from 1 instead of 0
                        tmpsku.pickprobability = pickprobability;
                        myskus.Add(tmpsku);
                    }
                }
                labelTotalSKUS.Text = "# SKU: " + myskus.Count().ToString();
            }
            //If you want to import a csv file that has SKUs and their pick probabilities
            else
            {
                myskus.Clear();
                //Browse for file
                OpenFileDialog ofd = new OpenFileDialog();
                //Only show .csv files
                ofd.Filter = "Microsoft Office Excel Comma Separated Values File|*.csv";
                DialogResult result = ofd.ShowDialog();
                //If the user selects a valid file 
                if (result == DialogResult.OK)
                {
                    string fileName = ofd.FileName;
                    FileInfo fileIn = new FileInfo(fileName);

                    StreamReader reader = fileIn.OpenText();
                    string[] lineIn;
                    while (!reader.EndOfStream)
                    {
                        lineIn = reader.ReadLine().Split(',');
                        //Validate values here
                        int sku;
                        int.TryParse(lineIn[0], out sku);
                        double pickprobability;
                        double.TryParse(lineIn[1], out pickprobability);
                        sku tmpsku = new sku(sku);
                        tmpsku.pickprobability = pickprobability;
                        myskus.Add(tmpsku);
                    }
                }
                labelTotalSKUS.Text = "# SKU: " + myskus.Count().ToString();
            }
        }
        
        private void importOptimizationRuns()
        {
            //Reset Progress Bar Batch
            progressBarBatch.Value = 0;
            //Browse for file
            OpenFileDialog ofd = new OpenFileDialog();
            //Only show .csv files
            ofd.Filter = "Microsoft Office Excel Comma Separated Values File|*.csv";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                string fileName = ofd.FileName;
                FileInfo fileIn = new FileInfo(fileName);

                List<string[]> lines = new List<string[]>();
                List<string> cost = new List<string>();
                List<string> locations = new List<string>();
                List<string> TSPtime = new List<string>();
                List<string> WW = new List<string>();
                List<string> WD = new List<string>();
                List<string> WA = new List<string>();
                StreamReader reader;
                try
                {
                    reader = fileIn.OpenText();
                }
                catch
                {
                    textBoxNeighbors.AppendText("File is Already Open, Close File\n"); return;
                }

                string rootfolder = Path.GetDirectoryName(fileName);

                string[] lineIn;//Do not read the first line (skip for header)

                while (!reader.EndOfStream)
                {
                    lineIn = reader.ReadLine().Split(',');
                    lines.Add(lineIn);
                    cost.Add("N/A");
                    locations.Add("N/A");
                    TSPtime.Add("N/A");
                    WW.Add("N/A");
                    WD.Add("N/A");
                    WA.Add("N/A");
                }
                reader.Close();
                var exceptions = new ConcurrentQueue<Exception>();
                for (int b = 1; b < lines.Count(); b++)
                {
                    try
                    {
                        //Create a new warehouse object
                        mywh = new warehouse();
                        //SKU Generation
                        myskus = new List<sku>();
                        textBoxDemandSkewness20.Text = lines[b][19];
                        textBoxDemandSkewness80.Text = lines[b][20];
                        double xPercentofItems = Convert.ToDouble(lines[b][19]) / 100;
                        double yPercentofTotalDemand = Convert.ToDouble(lines[b][20]) / 10 / 10;
                        if (xPercentofItems < yPercentofTotalDemand)
                        {
                            double shapeFactor = (xPercentofItems - yPercentofTotalDemand * xPercentofItems) / (yPercentofTotalDemand - xPercentofItems);
                            int totalNumberSKUs = Convert.ToInt32(lines[b][21]);
                            textBoxNumberSKUs.Text = lines[b][21];
                            if (totalNumberSKUs > 0)
                            {
                                CDF = new double[totalNumberSKUs];
                                CDF[0] = ((1.0 + shapeFactor) * (1.0 / totalNumberSKUs)) / (shapeFactor + 1.0 / totalNumberSKUs);
                                //Create first SKU
                                sku tmpsku = new sku(1);
                                tmpsku.pickprobability = CDF[0] - 0;
                                myskus.Add(tmpsku);
                                for (int i = 1; i < totalNumberSKUs; i++)
                                {
                                    CDF[i] = ((1.0 + shapeFactor) * ((double)(i + 1) / totalNumberSKUs)) / (shapeFactor + (double)(i + 1) / totalNumberSKUs);
                                    tmpsku = new sku(i + 1);//id starts from 1 instead of 0
                                    tmpsku.pickprobability = CDF[i] - CDF[i - 1];
                                    myskus.Add(tmpsku);
                                }
                            }
                        }
                        else
                        {
                            cost[b] = "X > Y";//This means SKU error
                        }
                        if (cost[b] != "X > Y")
                        {
                            //Order Generation
                            myorders = new List<order>();
                            textBoxNumberOrders.Text = lines[b][22];
                            int numberoforderlists = Convert.ToInt32(lines[b][22]);
                            textBoxOrderSize.Text = lines[b][23];
                            int ordersize = Convert.ToInt32(lines[b][23]);
                            textBoxOrderGenRandSeed.Text = lines[b][24];
                            Random myrand = new Random(Convert.ToInt32(lines[b][24]));
                            for (int i = 0; i < numberoforderlists; i++)
                            {
                                order tmporder = new order(i);
                                for (int j = 0; j < ordersize; j++)
                                {
                                    double myprob = uniformrandom(0, 1, myrand);
                                    for (int k = 0; k < myskus.Count(); k++)
                                    {
                                        if (myprob < CDF[k])
                                        {
                                            tmporder.addSKU(myskus[k]);
                                            break;
                                        }
                                    }
                                }
                                myorders.Add(tmporder);
                            }

                            string foldernamesize = "Size " + lines[b][21];
                            if(!System.IO.Directory.Exists(rootfolder + "\\" + foldernamesize))
                            {
                                System.IO.Directory.CreateDirectory(rootfolder + "\\" + foldernamesize);
                            }

                            string foldernameskewness = lines[b][19] + "-" + lines[b][20];
                            if (!System.IO.Directory.Exists(rootfolder + "\\" + foldernamesize + "\\" + foldernameskewness))
                            {
                                System.IO.Directory.CreateDirectory(rootfolder + "\\" + foldernamesize + "\\" + foldernameskewness);
                            }

                            string foldernamepicksize = "Pick Size " + lines[b][23];
                            if (!System.IO.Directory.Exists(rootfolder + "\\" + foldernamesize + "\\" + foldernameskewness + "\\" + foldernamepicksize))
                            {
                                System.IO.Directory.CreateDirectory(rootfolder + "\\" + foldernamesize + "\\" + foldernameskewness + "\\" + foldernamepicksize);
                            }

                            string foldernamegenseed = "Seed " + lines[b][24];
                            if (!System.IO.Directory.Exists(rootfolder + "\\" + foldernamesize + "\\" + foldernameskewness + "\\" + foldernamepicksize + "\\" + foldernamegenseed))
                            {
                                System.IO.Directory.CreateDirectory(rootfolder + "\\" + foldernamesize + "\\" + foldernameskewness + "\\" + foldernamepicksize + "\\" + foldernamegenseed);
                            }
                            problemfolder = rootfolder + "\\" + foldernamesize + "\\" + foldernameskewness + "\\" + foldernamepicksize + "\\" + foldernamegenseed;
                            //Calculate Average Tour Length
                            double totalnumberofpicks = 0;
                            for (int i = 0; i < myorders.Count(); i++)
                            {
                                totalnumberofpicks = totalnumberofpicks + myorders[i].getOrderSize();
                            }
                            double avgTourLength = totalnumberofpicks / myorders.Count();
                            textBoxAvgOrderSize.Text = avgTourLength.ToString();

                            textBoxCrossAisleWidth.Text = lines[b][27];
                            textBoxPickingAisleWidth.Text = lines[b][28];
                            textBoxLWidth.Text = lines[b][25];
                            textBoxLDepth.Text = lines[b][26];
                            textBoxPickerSize.Text = lines[b][18];
                            if(lines[b][17] == "1")
                            {
                                checkBoxVisibilityGraph.Checked = true;
                            }
                            else
                            {
                                checkBoxVisibilityGraph.Checked = false;
                            }

                            textBoxMu.Text = lines[b][4];
                            textBoxLambda.Text = lines[b][5];
                            textBoxESSeed.Text = lines[b][6];
                            textBoxESIteration.Text = lines[b][7];
                            textBoxSigma.Text = lines[b][8];
                            textBoxESCounter.Text = lines[b][9];
                            textBoxESSuccessRule.Text = lines[b][10];
                            comboBoxDesignClass.SelectedIndex = comboBoxDesignClass.FindString(lines[b][3]);
                            //Lower Bounds
                            textBoxE1L.Text = lines[b][34];
                            textBoxE2L.Text = lines[b][36];
                            textBoxE3L.Text = lines[b][38];
                            textBoxE4L.Text = lines[b][40];
                            textBoxI1XL.Text = lines[b][42];
                            textBoxI1YL.Text = lines[b][44];
                            textBoxPDL.Text = lines[b][46];
                            textBoxAngle1L.Text = lines[b][48];
                            textBoxAngle2L.Text = lines[b][50];
                            textBoxAngle3L.Text = lines[b][52];
                            textBoxAngle4L.Text = lines[b][54];
                            textBoxAngle5L.Text = lines[b][56];
                            textBoxAngle6L.Text = lines[b][58];
                            textBoxAngle7L.Text = lines[b][60];
                            textBoxAngle8L.Text = lines[b][62];
                            textBoxAdjuster1L.Text = lines[b][64];
                            textBoxAdjuster2L.Text = lines[b][66];
                            textBoxAdjuster3L.Text = lines[b][68];
                            textBoxAdjuster4L.Text = lines[b][70];
                            textBoxAdjuster5L.Text = lines[b][72];
                            textBoxAdjuster6L.Text = lines[b][74];
                            textBoxAdjuster7L.Text = lines[b][76];
                            textBoxAdjuster8L.Text = lines[b][78];
                            textBoxPickAdjuster1L.Text = lines[b][80];
                            textBoxPickAdjuster2L.Text = lines[b][82];
                            textBoxPickAdjuster3L.Text = lines[b][84];
                            textBoxPickAdjuster4L.Text = lines[b][86];
                            textBoxPickAdjuster5L.Text = lines[b][88];
                            textBoxPickAdjuster6L.Text = lines[b][90];
                            textBoxPickAdjuster7L.Text = lines[b][92];
                            textBoxPickAdjuster8L.Text = lines[b][94];
                            textBoxC12L.Text = lines[b][96];
                            textBoxC13L.Text = lines[b][98];
                            textBoxC14L.Text = lines[b][100];
                            textBoxC15L.Text = lines[b][102];
                            textBoxC23L.Text = lines[b][104];
                            textBoxC24L.Text = lines[b][106];
                            textBoxC25L.Text = lines[b][108];
                            textBoxC34L.Text = lines[b][110];
                            textBoxC35L.Text = lines[b][112];
                            textBoxC45L.Text = lines[b][114];

                            textBoxE1U.Text = lines[b][35];
                            textBoxE2U.Text = lines[b][37];
                            textBoxE3U.Text = lines[b][39];
                            textBoxE4U.Text = lines[b][41];
                            textBoxI1XU.Text = lines[b][43];
                            textBoxI1YU.Text = lines[b][45];
                            textBoxPDU.Text = lines[b][47];
                            textBoxAngle1U.Text = lines[b][49];
                            textBoxAngle2U.Text = lines[b][51];
                            textBoxAngle3U.Text = lines[b][53];
                            textBoxAngle4U.Text = lines[b][55];
                            textBoxAngle5U.Text = lines[b][57];
                            textBoxAngle6U.Text = lines[b][59];
                            textBoxAngle7U.Text = lines[b][61];
                            textBoxAngle8U.Text = lines[b][63];
                            textBoxAdjuster1U.Text = lines[b][65];
                            textBoxAdjuster2U.Text = lines[b][67];
                            textBoxAdjuster3U.Text = lines[b][69];
                            textBoxAdjuster4U.Text = lines[b][71];
                            textBoxAdjuster5U.Text = lines[b][73];
                            textBoxAdjuster6U.Text = lines[b][75];
                            textBoxAdjuster7U.Text = lines[b][77];
                            textBoxAdjuster8U.Text = lines[b][79];
                            textBoxPickAdjuster1U.Text = lines[b][81];
                            textBoxPickAdjuster2U.Text = lines[b][83];
                            textBoxPickAdjuster3U.Text = lines[b][85];
                            textBoxPickAdjuster4U.Text = lines[b][87];
                            textBoxPickAdjuster5U.Text = lines[b][89];
                            textBoxPickAdjuster6U.Text = lines[b][91];
                            textBoxPickAdjuster7U.Text = lines[b][93];
                            textBoxPickAdjuster8U.Text = lines[b][95];
                            textBoxC12U.Text = lines[b][97];
                            textBoxC13U.Text = lines[b][99];
                            textBoxC14U.Text = lines[b][101];
                            textBoxC15U.Text = lines[b][103];
                            textBoxC23U.Text = lines[b][105];
                            textBoxC24U.Text = lines[b][107];
                            textBoxC25U.Text = lines[b][109];
                            textBoxC34U.Text = lines[b][111];
                            textBoxC35U.Text = lines[b][113];
                            textBoxC45U.Text = lines[b][115];

                            if(lines[b][15] == "1")
                            {
                                checkBoxAllSearch.Checked = true;
                            }
                            else
                            {
                                checkBoxAllSearch.Checked = false;
                            }

                            comboBoxComputing.SelectedIndex = Convert.ToInt32(lines[b][11]);

                            if (lines[b][16] == "1")
                            {
                                checkBoxTSP.Checked = true;
                            }
                            else
                            {
                                checkBoxTSP.Checked = false;
                            }

                            if (lines[b][12] == "1")
                            {
                                checkBoxNFT.Checked = true;
                            }
                            else
                            {
                                checkBoxNFT.Checked = false;
                            }

                            textBoxNFT0.Text = lines[b][13];
                            textBoxNFTLambda.Text = lines[b][14];

                            solveES();
                        }
                    }
                    catch (Exception e)
                    {
                        exceptions.Enqueue(e);
                    }
                    textBoxNeighbors.AppendText("Run: " + b.ToString() + "/" + (lines.Count() - 1).ToString() + "\n");
                    progressBarBatch.Value = Convert.ToInt32(Convert.ToDouble(b) / Convert.ToDouble(lines.Count() - 1) * 100);

                }
            }
        }

        /// <summary>
        /// There is a possibility of an infinite loop in here
        /// </summary>
        private void importDesigns()
        {
            //Reset progress bar
            this.progressBarAlg.Value = 0;
            //Browse for file
            OpenFileDialog ofd = new OpenFileDialog();
            //Only show .csv files
            ofd.Filter = "Microsoft Office Excel Comma Separated Values File|*.csv";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                string fileName = ofd.FileName;
                FileInfo fileIn = new FileInfo(fileName);

                List<string[]> lines = new List<string[]>();
                List<string> cost = new List<string>();
                List<string> locations = new List<string>();
                List<string> TSPtime = new List<string>();
                List<string> WW = new List<string>();
                List<string> WD = new List<string>();
                List<string> WA = new List<string>();
                StreamReader reader;
                try
                {
                    reader = fileIn.OpenText();
                }
                catch
                {
                    textBoxNeighbors.AppendText("File is Already Open, Close File\n"); return;
                }

                
                string[] lineIn;//Do not read the first line (skip for header)
                while (!reader.EndOfStream)
                {
                    lineIn = reader.ReadLine().Split(',');
                    lines.Add(lineIn);
                    cost.Add("N/A");
                    locations.Add("N/A");
                    TSPtime.Add("N/A");
                    WW.Add("N/A");
                    WD.Add("N/A");
                    WA.Add("N/A");
                }
                reader.Close();
                var exceptions = new ConcurrentQueue<Exception>();
                DateTime start1 = DateTime.Now;
                //Parallel.For(1, lines.Count(), m =>
                for (int m = 1; m < lines.Count(); m++)
                {
                    try
                    {
                        //Create a new warehouse object
                        warehouse mylocalwh = new warehouse();
                        //SKU Generation
                        List<sku> mylocalskus = new List<sku>();
                        double xPercentofItems = Convert.ToDouble(lines[m][10]) / 100;
                        double yPercentofTotalDemand = Convert.ToDouble(lines[m][11]) / 10 / 10;
                        if (xPercentofItems < yPercentofTotalDemand)
                        {
                            double shapeFactor = (xPercentofItems - yPercentofTotalDemand * xPercentofItems) / (yPercentofTotalDemand - xPercentofItems);
                            int totalNumberSKUs = Convert.ToInt32(lines[m][12]);
                            if (totalNumberSKUs > 0)
                            {
                                CDF = new double[totalNumberSKUs];
                                CDF[0] = ((1.0 + shapeFactor) * (1.0 / totalNumberSKUs)) / (shapeFactor + 1.0 / totalNumberSKUs);
                                //Create first SKU
                                sku tmpsku = new sku(1);
                                tmpsku.pickprobability = CDF[0] - 0;
                                mylocalskus.Add(tmpsku);
                                for (int i = 1; i < totalNumberSKUs; i++)
                                {
                                    CDF[i] = ((1.0 + shapeFactor) * ((double)(i + 1) / totalNumberSKUs)) / (shapeFactor + (double)(i + 1) / totalNumberSKUs);
                                    tmpsku = new sku(i + 1);//id starts from 1 instead of 0
                                    tmpsku.pickprobability = CDF[i] - CDF[i - 1];
                                    mylocalskus.Add(tmpsku);
                                }
                            }
                        }
                        else
                        {
                            cost[m] = "X > Y";//This means SKU error
                        }
                        if (cost[m] != "X > Y")
                        {
                            //Order Generation
                            List<order> mylocalorders = new List<order>();
                            int numberoforderlists = Convert.ToInt32(lines[m][13]);
                            int ordersize = Convert.ToInt32(lines[m][14]);
                            Random myrand = new Random(Convert.ToInt32(lines[m][15]));
                            for (int i = 0; i < numberoforderlists; i++)
                            {
                                order tmporder = new order(i);
                                for (int j = 0; j < ordersize; j++)
                                {
                                    double myprob = uniformrandom(0, 1, myrand);
                                    for (int k = 0; k < mylocalskus.Count(); k++)
                                    {
                                        if (myprob < CDF[k])
                                        {
                                            tmporder.addSKU(mylocalskus[k]);
                                            break;
                                        }
                                    }
                                }
                                mylocalorders.Add(tmporder);
                            }

                            //Calculate Average Tour Length
                            double totalnumberofpicks = 0;
                            for (int i = 0; i < mylocalorders.Count(); i++)
                            {
                                totalnumberofpicks = totalnumberofpicks + mylocalorders[i].getOrderSize();
                            }
                            double avgTourLength = totalnumberofpicks / mylocalorders.Count();

                            //Set warehouse aspectratio
                            mylocalwh.aspectratio = Convert.ToDouble(lines[m][23]);
                            //Set warehouse area
                            mylocalwh.setArea(Convert.ToDouble(lines[m][22]));
                            //Set warehouse cross aisle width
                            mylocalwh.crossaislewidth = Convert.ToDouble(lines[m][18]);
                            //Set warehouse picking aisle width
                            mylocalwh.pickingaislewidth = Convert.ToDouble(lines[m][19]);
                            //Set warehouse picking location width
                            mylocalwh.pickinglocationwidth = Convert.ToDouble(lines[m][16]);
                            //Set warehouse picking location depth
                            mylocalwh.pickinglocationdepth = Convert.ToDouble(lines[m][17]);
                            //Set size of picker used for visibility graph
                            mylocalwh.pickersize = Convert.ToDouble(lines[m][9]);
                            //Set which graph will be used Steiner or Visibility Graph
                            if (lines[m][7] == "1") mylocalwh.usevisibilitygraph = true; else mylocalwh.usevisibilitygraph = false;
                            if (lines[m][8] == "1") checkBoxStraightAllocation.Checked = true; else checkBoxStraightAllocation.Checked = false;
                            double[] angle = { Convert.ToDouble(lines[m][31]), Convert.ToDouble(lines[m][32]), Convert.ToDouble(lines[m][33]), Convert.ToDouble(lines[m][34]), Convert.ToDouble(lines[m][35]), Convert.ToDouble(lines[m][36]), Convert.ToDouble(lines[m][37]), Convert.ToDouble(lines[m][38]) };
                            double[] adjuster = { Convert.ToDouble(lines[m][39]), Convert.ToDouble(lines[m][40]), Convert.ToDouble(lines[m][41]), Convert.ToDouble(lines[m][42]), Convert.ToDouble(lines[m][43]), Convert.ToDouble(lines[m][44]), Convert.ToDouble(lines[m][45]), Convert.ToDouble(lines[m][46]) };
                            double[] pickadjuster = { Convert.ToDouble(lines[m][47]), Convert.ToDouble(lines[m][48]), Convert.ToDouble(lines[m][49]), Convert.ToDouble(lines[m][50]), Convert.ToDouble(lines[m][51]), Convert.ToDouble(lines[m][52]), Convert.ToDouble(lines[m][53]), Convert.ToDouble(lines[m][54]) };
                            double[] ext = { Convert.ToDouble(lines[m][24]), Convert.ToDouble(lines[m][25]), Convert.ToDouble(lines[m][26]), Convert.ToDouble(lines[m][27]) };
                            double[] intx = { Convert.ToDouble(lines[m][28]) };
                            double[] inty = { Convert.ToDouble(lines[m][29]) };
                            double[] pd = { Convert.ToDouble(lines[m][30]) };
                            double aspectratio = Convert.ToDouble(lines[m][23]);

                            bool[] connections = new bool[10];

                            if (lines[m][55] == "1") connections[0] = true; else connections[0] = false;
                            if (lines[m][56] == "1") connections[1] = true; else connections[1] = false;
                            if (lines[m][57] == "1") connections[2] = true; else connections[2] = false;
                            if (lines[m][58] == "1") connections[3] = true; else connections[3] = false;
                            if (lines[m][59] == "1") connections[4] = true; else connections[4] = false;
                            if (lines[m][60] == "1") connections[5] = true; else connections[5] = false;
                            if (lines[m][61] == "1") connections[6] = true; else connections[6] = false;
                            if (lines[m][62] == "1") connections[7] = true; else connections[7] = false;
                            if (lines[m][63] == "1") connections[8] = true; else connections[8] = false;
                            if (lines[m][64] == "1") connections[9] = true; else connections[9] = false;

                            //Adjust warehouse area until warehouse is inside the location boundaries
                            //This is good for comparison of optimal and traditional layouts for their storage location equality
                            int increased = 0;
                            int decreased = 0;
                            bool warehousefit = false;
                            bool finalize = false;
                            int resize = Convert.ToInt32(lines[m][3]);
                            int desloc = Convert.ToInt32(lines[m][4]);
                            if (resize > 0) mylocalwh.setArea(40000);

                            do
                            {
                                mylocalwh.resetNetwork();
                                mylocalwh.setSKUs(mylocalskus);
                                if (!mylocalwh.createWarehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, mylocalwh.crossaislewidth, mylocalwh.pickingaislewidth, mylocalwh.pickinglocationwidth, mylocalwh.pickinglocationdepth, connections))
                                {
                                    cost[m] = "Inv";
                                }
                                if (resize == 0) break;
                                //Check warehouse size here before doing any other calculations and if size is not fit then adjust it approprately
                                int totalstoragelocations = mylocalwh.totalNumberOfLocations();

                                if (resize == 1)//Smallest fit
                                {
                                    if (increased > 0 && decreased > 0)//No exact number of locations but this is the closest it can get to smallest it can fit
                                    {
                                        if (mylocalwh.getSKUs().Count > totalstoragelocations)//This check is necessary because last iteration it could have been decreased
                                        {
                                            mylocalwh.setArea(mylocalwh.area / options.warehouseadjustmentfactor);//Increase area
                                            increased++;
                                            finalize = true;
                                        }
                                        else if (mylocalwh.getSKUs().Count < totalstoragelocations || finalize == true)
                                        {
                                            warehousefit = true;
                                            break;
                                        }
                                    }
                                    if (mylocalwh.getSKUs().Count > totalstoragelocations)
                                    {
                                        mylocalwh.setArea(mylocalwh.area / options.warehouseadjustmentfactor);//Increase area
                                        increased++;
                                    }
                                    else if (mylocalwh.getSKUs().Count < totalstoragelocations)
                                    {
                                        mylocalwh.setArea(mylocalwh.area * options.warehouseadjustmentfactor);//Decrease area
                                        decreased++;
                                    }
                                    else if (mylocalwh.getSKUs().Count == totalstoragelocations)
                                    {
                                        warehousefit = true;
                                    }
                                }
                                else if (resize == 2)//Desired number of locations
                                {
                                    if (increased > 0 && decreased > 0)//No exact number of locations but this is closest it can get to the desired number of locations
                                    {
                                        if(desloc > totalstoragelocations)//This check is necessary because last iteration it could have been decreased
                                        {
                                            mylocalwh.setArea(mylocalwh.area / options.warehouseadjustmentfactor);//Increase area
                                            increased++;
                                            finalize = true;
                                        }
                                        else if (desloc < totalstoragelocations || finalize == true)
                                        {
                                            warehousefit = true;
                                            break;
                                        }
                                    }
                                    if (desloc > totalstoragelocations)
                                    {
                                        mylocalwh.setArea(mylocalwh.area / options.warehouseadjustmentfactor);//Increase area
                                        increased++;
                                    }
                                    else if (desloc < totalstoragelocations)
                                    {
                                        mylocalwh.setArea(mylocalwh.area * options.warehouseadjustmentfactor);//Decrease area
                                        decreased++;
                                    }
                                    else if(desloc == totalstoragelocations)
                                    {
                                        warehousefit = true;
                                    }
                                }
                            } while (!warehousefit);
                            
                            if (mylocalwh.totalNumberOfLocations() >= mylocalskus.Count())
                            {
                                mylocalwh.setSKUs(mylocalskus);
                            }
                            else
                            {
                                cost[m] = "Ins";
                            }

                            if (cost[m] != "Inv" || cost[m] != "Ins")
                            {
                                int computing = Convert.ToInt32(lines[m][5]);
                                //Create importantnodes shortest distances
                                if (!mylocalwh.usevisibilitygraph) mylocalwh.createImportantNodeShortestDistances();
                                //Calculate Shortest Path Distances for Locations
                                if (!mylocalwh.usevisibilitygraph) mylocalwh.locationShortestDistance();
                                else
                                {
                                    mylocalwh.createPolygonsandGraphNodes();
                                    mylocalwh.createVisibilityGraph();
                                    mylocalwh.fillGraphNodeDistances();
                                }
                                //Calculate Shortest Path Distances to PD
                                mylocalwh.pdTotalDistances();
                                //Fulfill Total Distances to Each Locations
                                mylocalwh.totalDistances();
                                //mywh.colorManytoMany();
                                //mywh.colorOnetoMany();
                                mylocalwh.rankLocations(avgTourLength);

                                //locations[m] = mywh.totalNumberOfLocations().ToString();
                                allocation myal = new allocation(0);
                                int allocationmethod = 0;
                                if (checkBoxStraightAllocation.Checked) allocationmethod = 1;
                                int warehouseareafit = myal.allocateSKUs(mylocalwh.getSKUs(), mylocalwh, allocationmethod);
                                locations[m] = mylocalwh.totalNumberOfLocations().ToString();
                                WW[m] = mylocalwh.getWidth().ToString("R");
                                WD[m] = mylocalwh.getDepth().ToString("R");
                                WA[m] = mylocalwh.area.ToString("R");
                                mylocalwh.setOrders(mylocalorders);
                                //mywh.randomizeOrders();
                                double totalcost = 0;
                                int samplesize = mylocalorders.Count();
                                DateTime start2 = DateTime.Now;
                                //If you want to solve it using center aisles
                                if (!mylocalwh.usevisibilitygraph)
                                {
                                    //If you want to solve TSPs using parallel computing
                                    if (computing == 0)
                                    {
                                        var sums = new ConcurrentBag<double>();
                                        bool TSPConcorde = false;
                                        if (lines[m][6] == "1") TSPConcorde = true;

                                        Parallel.For(0, samplesize, k =>
                                        //for (int k = 0; k < samplesize; k++)
                                        {
                                            warehouse tmpwh = mylocalwh;
                                            List<order> tmporders = mylocalwh.getOrders();
                                            routing rt = new routing();
                                            //totalcost += rt.tsp(tmpwh, tmporders[k]);
                                            double tourcost = 0;
                                            bool LKHdoneonce = false;
                                            while (tourcost == 0)
                                            {
                                                if (TSPConcorde || LKHdoneonce)
                                                {
                                                    tourcost = rt.tspOptSteiner(tmpwh, tmporders[k], k);
                                                    if (tourcost == 0) break;//This means that tour cost is really zero
                                                }
                                                else
                                                {
                                                    tourcost = rt.tspLKHSteiner(tmpwh, tmporders[k], k);
                                                    LKHdoneonce = true;
                                                }
                                            }
                                            sums.Add(tourcost);
                                        });

                                        totalcost = sums.Sum() / samplesize;
                                    }
                                    //If you want to solve it using distributed computing
                                    else if (computing == 1)
                                    {
                                        bool TSPConcorde = false;
                                        if (lines[m][6] == "1") TSPConcorde = true;
                                        if (TSPConcorde)
                                        {
                                            socketservers mysocketservers = new socketservers();
                                            mysocketservers.checkAvailableConcordeServers();
                                            routing rt = new routing();
                                            totalcost = rt.tspOptNetSteiner(mylocalwh, samplesize, mysocketservers, comboBoxNetSchedule.SelectedIndex);
                                        }
                                        else
                                        {
                                            socketservers mysocketservers = new socketservers();
                                            mysocketservers.checkAvailableConcordeServers();
                                            routing rt = new routing();
                                            totalcost = rt.tspLKHNetSteiner(mylocalwh, samplesize, mysocketservers, comboBoxNetSchedule.SelectedIndex);
                                        }
                                    }
                                    //Single thread computing
                                    else if (computing == 2)
                                    {
                                        var sums = new ConcurrentBag<double>();
                                        bool TSPConcorde = false;
                                        if (lines[m][6] == "1") TSPConcorde = true;

                                        //Parallel.For(0, samplesize, k =>
                                        for (int k = 0; k < samplesize; k++)
                                        {
                                            warehouse tmpwh = mylocalwh;
                                            List<order> tmporders = mylocalwh.getOrders();
                                            routing rt = new routing();
                                            //totalcost += rt.tsp(tmpwh, tmporders[k]);
                                            double tourcost = 0;
                                            bool LKHdoneonce = false;
                                            while (tourcost == 0)
                                            {
                                                if (TSPConcorde || LKHdoneonce)
                                                {
                                                    tourcost = rt.tspOptSteiner(tmpwh, tmporders[k], k);
                                                    if (tourcost == 0) break;//This means that tour cost is really zero
                                                }
                                                else
                                                {
                                                    tourcost = rt.tspLKHSteiner(tmpwh, tmporders[k], k);
                                                    LKHdoneonce = true;
                                                }
                                            }
                                            sums.Add(tourcost);
                                        };

                                        totalcost = sums.Sum() / samplesize;
                                    }
                                }
                                //using visibility graph
                                else
                                {
                                    //If you want to solve TSPs using parallel computing
                                    if (computing == 0)
                                    {
                                        var sums = new ConcurrentBag<double>();
                                        bool TSPConcorde = false;
                                        if (lines[m][6] == "1") TSPConcorde = true;

                                        Parallel.For(0, samplesize, k =>
                                        //for (int k = 0; k < samplesize; k++)
                                        {
                                            warehouse tmpwh = mylocalwh;
                                            List<order> tmporders = mylocalwh.getOrders();
                                            routing rt = new routing();
                                            //totalcost += rt.tsp(tmpwh, tmporders[k]);
                                            double tourcost = 0;
                                            bool LKHdoneonce = false;
                                            while (tourcost == 0)
                                            {
                                                if (TSPConcorde || LKHdoneonce)
                                                {
                                                    tourcost = rt.tspOptVisibility(tmpwh, tmporders[k], k);
                                                    if (tourcost == 0) break;//This means that tour cost is really zero
                                                }
                                                else
                                                {
                                                    tourcost = rt.tspLKHVisibility(tmpwh, tmporders[k], k);
                                                    LKHdoneonce = true;
                                                }
                                            }
                                            sums.Add(tourcost);
                                        });

                                        totalcost = sums.Sum() / samplesize;
                                    }
                                    //If you want to solve TSPs using distributed computing
                                    else if (computing == 1)
                                    {
                                        bool TSPConcorde = false;
                                        if (lines[m][6] == "1") TSPConcorde = true;
                                        if (TSPConcorde)
                                        {
                                            socketservers mysocketservers = new socketservers();
                                            mysocketservers.checkAvailableConcordeServers();
                                            routing rt = new routing();
                                            totalcost = rt.tspOptNetVisibility(mylocalwh, samplesize, mysocketservers, comboBoxNetSchedule.SelectedIndex);
                                        }
                                        else
                                        {
                                            socketservers mysocketservers = new socketservers();
                                            mysocketservers.checkAvailableConcordeServers();
                                            routing rt = new routing();
                                            totalcost = rt.tspLKHNetVisibility(mylocalwh, samplesize, mysocketservers, comboBoxNetSchedule.SelectedIndex);
                                        }
                                    }
                                    //Single thread computing
                                    else if (computing == 2)
                                    {
                                        var sums = new ConcurrentBag<double>();
                                        bool TSPConcorde = false;
                                        if (lines[m][6] == "1") TSPConcorde = true;

                                        //Parallel.For(0, samplesize, k =>
                                        for (int k = 0; k < samplesize; k++)
                                        {
                                            warehouse tmpwh = mylocalwh;
                                            List<order> tmporders = mylocalwh.getOrders();
                                            routing rt = new routing();
                                            //totalcost += rt.tsp(tmpwh, tmporders[k]);
                                            double tourcost = 0;
                                            bool LKHdoneonce = false;
                                            while (tourcost == 0)
                                            {
                                                if (TSPConcorde || LKHdoneonce)
                                                {
                                                    tourcost = rt.tspOptVisibility(tmpwh, tmporders[k], k);
                                                    if (tourcost == 0) break;//This means that tour cost is really zero
                                                }
                                                else
                                                {
                                                    tourcost = rt.tspLKHVisibility(tmpwh, tmporders[k], k);
                                                    LKHdoneonce = true;
                                                }
                                            }
                                            sums.Add(tourcost);
                                        };

                                        totalcost = sums.Sum() / samplesize;
                                    }
                                }
                                TimeSpan elapsed2 = DateTime.Now - start2;
                                TSPtime[m] = elapsed2.TotalSeconds.ToString("R");
                                cost[m] = totalcost.ToString("R");
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        exceptions.Enqueue(e);
                    }
                    textBoxNeighbors.AppendText("Run: " + m.ToString() + "/" + (lines.Count()-1).ToString() + "\n");
                    this.progressBarAlg.Value = Convert.ToInt32(Convert.ToDouble(m) / Convert.ToDouble(lines.Count()-1) * 100);
                };//End of parallel for

                TimeSpan elapsed1 = DateTime.Now - start1;
                textBoxNeighbors.AppendText("Batch Run Time: " + elapsed1.ToString() + "\n");
                // Throw the exceptions here after the loop completes.
                if (exceptions.Count > 0) throw new AggregateException(exceptions);

                File.Delete(fileName);
                csvexport myexcel = new csvexport();
                //Skip first row since it is header
                for (int i = 1; i < lines.Count(); i++)
                {
                    myexcel.addRow();
                    myexcel[lines[0][0]] = cost[i];
                    myexcel[lines[0][1]] = locations[i];
                    myexcel[lines[0][2]] = TSPtime[i];
                    if (Convert.ToInt32(lines[i][3]) == 0)//No resize so keep everything after there the same
                    {
                        for (int j = 3; j < 65; j++)
                        {
                            myexcel[lines[0][j]] = lines[i][j];
                        }
                    }
                    else
                    {
                        for (int j = 3; j < 20; j++)
                        {
                            myexcel[lines[0][j]] = lines[i][j];
                        }
                        myexcel[lines[0][20]] = WW[i];
                        myexcel[lines[0][21]] = WD[i];
                        myexcel[lines[0][22]] = WA[i];
                        for (int j = 23; j < 65; j++)
                        {
                            myexcel[lines[0][j]] = lines[i][j];
                        }
                    }
                    
                }
                myexcel.exportToFile(fileName);
            }
        }

        private void buttonDoE_Click(object sender, EventArgs e)
        {
            importDesigns();
        }

        /// <summary>
        /// This method is used if you import SKUs and Orders in two different files, this one imports orders and SKUs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonImportOrders_Click(object sender, EventArgs e)
        {
            if (comboBoxGenerate.SelectedIndex == 1)
            {
                myorders.Clear();
                int numberoforderlists = Convert.ToInt32(textBoxNumberOrders.Text);
                int ordersize = Convert.ToInt32(textBoxOrderSize.Text);
                Random myrand = new Random(Convert.ToInt32(textBoxOrderGenRandSeed.Text));
                for(int i = 0; i < numberoforderlists; i++)
                {
                    order tmporder = new order(i);
                    for(int j = 0; j < ordersize; j++)
                    {
                        double myprob = uniformrandom(0, 1, myrand);
                        for (int k = 0; k < myskus.Count; k++)
                        {
                            if (myprob < CDF[k])
                            {
                                tmporder.addSKU(myskus[k]);
                                break;
                            }
                        }
                    }
                    myorders.Add(tmporder);
                }
                //Calculate Average Tour Length
                double totalnumberofpicks = 0;
                for (int i = 0; i < myorders.Count(); i++)
                {
                    totalnumberofpicks = totalnumberofpicks + myorders[i].getOrderSize();
                }
                avgTourLength = totalnumberofpicks / myorders.Count();

                labelTotalOrders.Text = "# Orders: " + myorders.Count().ToString();
                labelMaxOrder.Text = "MaxOrderID: 0";
                labelMaxOrderSize.Text = "Max Order #: " + ordersize.ToString();
                textBoxAvgOrderSize.Text = avgTourLength.ToString("#.##");
                textBoxSampleSize.Text = myorders.Count().ToString();
            }
            else if (comboBoxGenerate.SelectedIndex == 3)//All dual command combinations
            {
                myorders.Clear();
                int numberofSKUs = Convert.ToInt32(textBoxNumberSKUs.Text);
                int orderNumberCounter = 0;
                order tmporder;
                for (int i = 0; i < numberofSKUs; i++)
                {
                    for(int j = i + 1; j < numberofSKUs; j++)
                    {
                        tmporder = new order(orderNumberCounter);
                        tmporder.addSKU(myskus[i]);//First item
                        tmporder.addSKU(myskus[j]);//Second item
                        myorders.Add(tmporder);
                    }
                }
                labelTotalOrders.Text = "# Orders: " + myorders.Count().ToString();
                labelMaxOrder.Text = "MaxOrderID: 0";
                labelMaxOrderSize.Text = "Max Order #: 0";
                textBoxAvgOrderSize.Text = "2";
                textBoxSampleSize.Text = myorders.Count().ToString();
            }
            else
            {
                myorders.Clear();
                int maxorderid = 0;
                int maxordersize = 0;
                //Browse for file
                OpenFileDialog ofd = new OpenFileDialog();
                //Only show .csv files
                ofd.Filter = "Microsoft Office Excel Comma Separated Values File|*.csv";
                DialogResult result = ofd.ShowDialog();
                //If the user selects a valid file 
                if (result == DialogResult.OK)
                {
                    string fileName = ofd.FileName;
                    problemfolder = Path.GetDirectoryName(fileName);
                    FileInfo fileIn = new FileInfo(fileName);
                    StreamReader reader = null;
                    try
                    {
                        reader = fileIn.OpenText();
                    }
                    catch
                    {
                        textBoxNeighbors.AppendText("File is used by another program\n");
                    }
                    string[] lineIn;

                    int previousorderid = 0;

                    while (!reader.EndOfStream)
                    {
                        lineIn = reader.ReadLine().Split(',');
                        //Validate values here
                        int orderid;
                        int.TryParse(lineIn[0], out orderid);
                        //Then continue to add current orderid the skus
                        if (orderid == previousorderid)
                        {
                            int skuID;
                            int.TryParse(lineIn[1], out skuID);
                            //Check if you have that SKU in your SKU list if not then add to the list
                            sku tmpsku = myskus.Find(item => item.ID == skuID);
                            if (tmpsku == null)
                            {
                                tmpsku = new sku(skuID);
                                myskus.Add(tmpsku);
                            }
                            //Add this sku to the current order
                            myorders[myorders.Count() - 1].addSKU(tmpsku);
                            //Increase frequency of this sku
                            tmpsku.addFrequency();
                        }
                        //Then create a new order and add the sku to the new order
                        else if (orderid != previousorderid)
                        {
                            //Order finished so check the size
                            if (previousorderid != 0)
                            {
                                if (maxordersize < myorders[myorders.Count() - 1].getOrderSize())
                                {
                                    maxorderid = myorders.Count() - 1;
                                    maxordersize = myorders[myorders.Count() - 1].getOrderSize();
                                }
                            }
                            //Change previousorderid
                            previousorderid = orderid;
                            //Create a new order
                            order tmporder = new order(orderid);
                            myorders.Add(tmporder);
                            int skuID;
                            int.TryParse(lineIn[1], out skuID);
                            //Check if you have that SKU in your SKU list if not then add to the list
                            sku tmpsku = myskus.Find(item => item.ID == skuID);
                            if (tmpsku == null)
                            {
                                tmpsku = new sku(skuID);
                                myskus.Add(tmpsku);
                            }
                            //Add this sku to the current order
                            myorders[myorders.Count() - 1].addSKU(tmpsku);
                            //Increase frequency of this sku
                            tmpsku.addFrequency();
                        }
                    }
                    reader.Close();

                    //Calculate Average Tour Length
                    double totalnumberofpicks = 0;
                    for (int i = 0; i < myorders.Count(); i++)
                    {
                        totalnumberofpicks = totalnumberofpicks + myorders[i].getOrderSize();
                    }
                    avgTourLength = totalnumberofpicks / myorders.Count();

                    labelTotalOrders.Text = "# Orders: " + myorders.Count().ToString();
                    labelMaxOrder.Text = "MaxOrderID: " + maxorderid.ToString();
                    labelMaxOrderSize.Text = "Max Order #: " + maxordersize.ToString();
                    textBoxAvgOrderSize.Text = avgTourLength.ToString("#.##");
                    textBoxSampleSize.Text = myorders.Count().ToString();
                }
            }
        }
        //Start of Textboxchanged parts
        private void textBoxWHWidth_TextChanged(object sender, EventArgs e)
        {
            if (textBoxWHWidth.ContainsFocus)
            {
                if (textBoxWHWidth.Text != "")
                {
                    if (Convert.ToDouble(textBoxWHWidth.Text) > 0)
                    {
                        textBoxArea.Text = (Convert.ToDouble(textBoxWHWidth.Text) * Convert.ToDouble(textBoxWHDepth.Text)).ToString();
                        textBoxAspectRatio.Text = (Convert.ToDouble(textBoxWHDepth.Text) / Convert.ToDouble(textBoxWHWidth.Text)).ToString();
                    }
                    else if (Convert.ToDouble(textBoxWHWidth.Text) <= 0)
                    {
                        MessageBox.Show("Please enter width value greater than 0");
                    }
                }
            }
        }

        private void textBoxWHDepth_TextChanged(object sender, EventArgs e)
        {
            if (textBoxWHDepth.ContainsFocus)
            {
                if (textBoxWHDepth.Text != "")
                {
                    if (Convert.ToDouble(textBoxWHDepth.Text) > 0)
                    {
                        textBoxArea.Text = (Convert.ToDouble(textBoxWHWidth.Text) * Convert.ToDouble(textBoxWHDepth.Text)).ToString();
                        textBoxAspectRatio.Text = (Convert.ToDouble(textBoxWHDepth.Text) / Convert.ToDouble(textBoxWHWidth.Text)).ToString();
                    }
                    else if (Convert.ToDouble(textBoxWHDepth.Text) <= 0)
                    {
                        MessageBox.Show("Please enter depth value greater than 0");
                    }
                }
            }
        }

        private void textBoxArea_TextChanged(object sender, EventArgs e)
        {
            if (textBoxArea.ContainsFocus)
            {
                if (textBoxArea.Text != "")
                {
                    if (Convert.ToDouble(textBoxArea.Text) > 0)
                    {
                        textBoxWHDepth.Text = Math.Sqrt(Convert.ToDouble(textBoxArea.Text) * Convert.ToDouble(textBoxAspectRatio.Text)).ToString();
                        textBoxWHWidth.Text = Math.Sqrt(Convert.ToDouble(textBoxArea.Text) / Convert.ToDouble(textBoxAspectRatio.Text)).ToString();
                    }
                    else if (Convert.ToDouble(textBoxArea.Text) <= 0)
                    {
                        MessageBox.Show("Please enter area value greater than 0");
                    }
                }
            }
        }

        private void textBoxAspectRatio_TextChanged(object sender, EventArgs e)
        {
            if (!solving)
            {
                if (textBoxAspectRatio.ContainsFocus)
                {
                    if (textBoxAspectRatio.Text != "")
                    {
                        if (Convert.ToDouble(textBoxAspectRatio.Text) > 0)
                        {
                            textBoxWHDepth.Text = Math.Sqrt(Convert.ToDouble(textBoxArea.Text) * Convert.ToDouble(textBoxAspectRatio.Text)).ToString();
                            textBoxWHWidth.Text = Math.Sqrt(Convert.ToDouble(textBoxArea.Text) / Convert.ToDouble(textBoxAspectRatio.Text)).ToString();
                        }
                        else if (Convert.ToDouble(textBoxArea.Text) <= 0)
                        {
                            MessageBox.Show("Please enter aspect ratio value greater than 0");
                        }
                    }
                }
            }
            if (solving)
            {
                if (textBoxAspectRatio.Text != "")
                {
                    if (Convert.ToDouble(textBoxAspectRatio.Text) > 0)
                    {
                        textBoxWHDepth.Text = Math.Sqrt(Convert.ToDouble(textBoxArea.Text) * Convert.ToDouble(textBoxAspectRatio.Text)).ToString();
                        textBoxWHWidth.Text = Math.Sqrt(Convert.ToDouble(textBoxArea.Text) / Convert.ToDouble(textBoxAspectRatio.Text)).ToString();
                    }
                    else if (Convert.ToDouble(textBoxArea.Text) <= 0)
                    {
                        MessageBox.Show("Please enter aspect ratio value greater than 0");
                    }
                }
                solving = false;
            }
        }
        //End of textboxchanged parts
        private void checkNumeric(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void aboutAdvancedLayoutOptimizerV10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm aboutform = new AboutForm();
            aboutform.ShowDialog();
        }

        private void comboBoxDesignClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBoxDesignClass.SelectedItem.ToString())
            {
                case "0-0-0":
                    textBoxE1.ReadOnly = true;
                    textBoxE2.ReadOnly = true;
                    textBoxE3.ReadOnly = true;
                    textBoxE4.ReadOnly = true;

                    textBoxAngle2.ReadOnly = true;
                    textBoxAngle3.ReadOnly = true;
                    textBoxAngle4.ReadOnly = true;
                    textBoxAngle5.ReadOnly = true;
                    textBoxAngle6.ReadOnly = true;
                    textBoxAngle7.ReadOnly = true;
                    textBoxAngle8.ReadOnly = true;

                    textBoxAdjuster2.ReadOnly = true;
                    textBoxAdjuster3.ReadOnly = true;
                    textBoxAdjuster4.ReadOnly = true;
                    textBoxAdjuster5.ReadOnly = true;
                    textBoxAdjuster6.ReadOnly = true;
                    textBoxAdjuster7.ReadOnly = true;
                    textBoxAdjuster8.ReadOnly = true;

                    textBoxPickAdjuster2.ReadOnly = true;
                    textBoxPickAdjuster3.ReadOnly = true;
                    textBoxPickAdjuster4.ReadOnly = true;
                    textBoxPickAdjuster5.ReadOnly = true;
                    textBoxPickAdjuster6.ReadOnly = true;
                    textBoxPickAdjuster7.ReadOnly = true;
                    textBoxPickAdjuster8.ReadOnly = true;

                    textBoxI1X.ReadOnly = true;
                    textBoxI1Y.ReadOnly = true;
                    break;
                case "2-0-1":
                    textBoxE1.ReadOnly = false;
                    textBoxE2.ReadOnly = false;
                    textBoxE3.ReadOnly = true;
                    textBoxE4.ReadOnly = true;

                    textBoxAngle2.ReadOnly = false;
                    textBoxAngle3.ReadOnly = true;
                    textBoxAngle4.ReadOnly = true;
                    textBoxAngle5.ReadOnly = true;
                    textBoxAngle6.ReadOnly = true;
                    textBoxAngle7.ReadOnly = true;
                    textBoxAngle8.ReadOnly = true;

                    textBoxAdjuster2.ReadOnly = false;
                    textBoxAdjuster3.ReadOnly = true;
                    textBoxAdjuster4.ReadOnly = true;
                    textBoxAdjuster5.ReadOnly = true;
                    textBoxAdjuster6.ReadOnly = true;
                    textBoxAdjuster7.ReadOnly = true;
                    textBoxAdjuster8.ReadOnly = true;

                    textBoxPickAdjuster2.ReadOnly = false;
                    textBoxPickAdjuster3.ReadOnly = true;
                    textBoxPickAdjuster4.ReadOnly = true;
                    textBoxPickAdjuster5.ReadOnly = true;
                    textBoxPickAdjuster6.ReadOnly = true;
                    textBoxPickAdjuster7.ReadOnly = true;
                    textBoxPickAdjuster8.ReadOnly = true;

                    textBoxI1X.ReadOnly = true;
                    textBoxI1Y.ReadOnly = true;
                    break;
                case "3-0-2":
                    textBoxE1.ReadOnly = false;
                    textBoxE2.ReadOnly = false;
                    textBoxE3.ReadOnly = false;
                    textBoxE4.ReadOnly = true;

                    textBoxAngle2.ReadOnly = false;
                    textBoxAngle3.ReadOnly = false;
                    textBoxAngle4.ReadOnly = true;
                    textBoxAngle5.ReadOnly = true;
                    textBoxAngle6.ReadOnly = true;
                    textBoxAngle7.ReadOnly = true;
                    textBoxAngle8.ReadOnly = true;

                    textBoxAdjuster2.ReadOnly = false;
                    textBoxAdjuster3.ReadOnly = false;
                    textBoxAdjuster4.ReadOnly = true;
                    textBoxAdjuster5.ReadOnly = true;
                    textBoxAdjuster6.ReadOnly = true;
                    textBoxAdjuster7.ReadOnly = true;
                    textBoxAdjuster8.ReadOnly = true;

                    textBoxPickAdjuster2.ReadOnly = false;
                    textBoxPickAdjuster3.ReadOnly = false;
                    textBoxPickAdjuster4.ReadOnly = true;
                    textBoxPickAdjuster5.ReadOnly = true;
                    textBoxPickAdjuster6.ReadOnly = true;
                    textBoxPickAdjuster7.ReadOnly = true;
                    textBoxPickAdjuster8.ReadOnly = true;

                    textBoxI1X.ReadOnly = true;
                    textBoxI1Y.ReadOnly = true;
                    break;
                case "3-0-3":
                    textBoxE1.ReadOnly = false;
                    textBoxE2.ReadOnly = false;
                    textBoxE3.ReadOnly = false;
                    textBoxE4.ReadOnly = true;

                    textBoxAngle2.ReadOnly = false;
                    textBoxAngle3.ReadOnly = false;
                    textBoxAngle4.ReadOnly = false;
                    textBoxAngle5.ReadOnly = true;
                    textBoxAngle6.ReadOnly = true;
                    textBoxAngle7.ReadOnly = true;
                    textBoxAngle8.ReadOnly = true;

                    textBoxAdjuster2.ReadOnly = false;
                    textBoxAdjuster3.ReadOnly = false;
                    textBoxAdjuster4.ReadOnly = false;
                    textBoxAdjuster5.ReadOnly = true;
                    textBoxAdjuster6.ReadOnly = true;
                    textBoxAdjuster7.ReadOnly = true;
                    textBoxAdjuster8.ReadOnly = true;

                    textBoxPickAdjuster2.ReadOnly = false;
                    textBoxPickAdjuster3.ReadOnly = false;
                    textBoxPickAdjuster4.ReadOnly = false;
                    textBoxPickAdjuster5.ReadOnly = true;
                    textBoxPickAdjuster6.ReadOnly = true;
                    textBoxPickAdjuster7.ReadOnly = true;
                    textBoxPickAdjuster8.ReadOnly = true;

                    textBoxI1X.ReadOnly = true;
                    textBoxI1Y.ReadOnly = true;
                    break;
                case "3-1-3":
                    textBoxE1.ReadOnly = false;
                    textBoxE2.ReadOnly = false;
                    textBoxE3.ReadOnly = false;
                    textBoxE4.ReadOnly = true;

                    textBoxAngle2.ReadOnly = false;
                    textBoxAngle3.ReadOnly = false;
                    textBoxAngle4.ReadOnly = true;
                    textBoxAngle5.ReadOnly = true;
                    textBoxAngle6.ReadOnly = true;
                    textBoxAngle7.ReadOnly = true;
                    textBoxAngle8.ReadOnly = true;

                    textBoxAdjuster2.ReadOnly = false;
                    textBoxAdjuster3.ReadOnly = false;
                    textBoxAdjuster4.ReadOnly = true;
                    textBoxAdjuster5.ReadOnly = true;
                    textBoxAdjuster6.ReadOnly = true;
                    textBoxAdjuster7.ReadOnly = true;
                    textBoxAdjuster8.ReadOnly = true;

                    textBoxPickAdjuster2.ReadOnly = false;
                    textBoxPickAdjuster3.ReadOnly = false;
                    textBoxPickAdjuster4.ReadOnly = true;
                    textBoxPickAdjuster5.ReadOnly = true;
                    textBoxPickAdjuster6.ReadOnly = true;
                    textBoxPickAdjuster7.ReadOnly = true;
                    textBoxPickAdjuster8.ReadOnly = true;

                    textBoxI1X.ReadOnly = false;
                    textBoxI1Y.ReadOnly = false;
                    break;
                case "4-0-2":
                    textBoxE1.ReadOnly = false;
                    textBoxE2.ReadOnly = false;
                    textBoxE3.ReadOnly = false;
                    textBoxE4.ReadOnly = false;

                    textBoxAngle2.ReadOnly = false;
                    textBoxAngle3.ReadOnly = false;
                    textBoxAngle4.ReadOnly = true;
                    textBoxAngle5.ReadOnly = true;
                    textBoxAngle6.ReadOnly = true;
                    textBoxAngle7.ReadOnly = true;
                    textBoxAngle8.ReadOnly = true;

                    textBoxAdjuster2.ReadOnly = false;
                    textBoxAdjuster3.ReadOnly = false;
                    textBoxAdjuster4.ReadOnly = true;
                    textBoxAdjuster5.ReadOnly = true;
                    textBoxAdjuster6.ReadOnly = true;
                    textBoxAdjuster7.ReadOnly = true;
                    textBoxAdjuster8.ReadOnly = true;

                    textBoxPickAdjuster2.ReadOnly = false;
                    textBoxPickAdjuster3.ReadOnly = false;
                    textBoxPickAdjuster4.ReadOnly = true;
                    textBoxPickAdjuster5.ReadOnly = true;
                    textBoxPickAdjuster6.ReadOnly = true;
                    textBoxPickAdjuster7.ReadOnly = true;
                    textBoxPickAdjuster8.ReadOnly = true;

                    textBoxI1X.ReadOnly = true;
                    textBoxI1Y.ReadOnly = true;
                    break;
                case "4-0-4":
                    textBoxE1.ReadOnly = false;
                    textBoxE2.ReadOnly = false;
                    textBoxE3.ReadOnly = false;
                    textBoxE4.ReadOnly = false;

                    textBoxAngle2.ReadOnly = false;
                    textBoxAngle3.ReadOnly = false;
                    textBoxAngle4.ReadOnly = false;
                    textBoxAngle5.ReadOnly = false;
                    textBoxAngle6.ReadOnly = true;
                    textBoxAngle7.ReadOnly = true;
                    textBoxAngle8.ReadOnly = true;

                    textBoxAdjuster2.ReadOnly = false;
                    textBoxAdjuster3.ReadOnly = false;
                    textBoxAdjuster4.ReadOnly = false;
                    textBoxAdjuster5.ReadOnly = false;
                    textBoxAdjuster6.ReadOnly = true;
                    textBoxAdjuster7.ReadOnly = true;
                    textBoxAdjuster8.ReadOnly = true;

                    textBoxPickAdjuster2.ReadOnly = false;
                    textBoxPickAdjuster3.ReadOnly = false;
                    textBoxPickAdjuster4.ReadOnly = false;
                    textBoxPickAdjuster5.ReadOnly = false;
                    textBoxPickAdjuster6.ReadOnly = true;
                    textBoxPickAdjuster7.ReadOnly = true;
                    textBoxPickAdjuster8.ReadOnly = true;

                    textBoxI1X.ReadOnly = true;
                    textBoxI1Y.ReadOnly = true;
                    break;
                case "4-0-5":
                    textBoxE1.ReadOnly = false;
                    textBoxE2.ReadOnly = false;
                    textBoxE3.ReadOnly = false;
                    textBoxE4.ReadOnly = false;

                    textBoxAngle2.ReadOnly = false;
                    textBoxAngle3.ReadOnly = false;
                    textBoxAngle4.ReadOnly = false;
                    textBoxAngle5.ReadOnly = false;
                    textBoxAngle6.ReadOnly = false;
                    textBoxAngle7.ReadOnly = true;
                    textBoxAngle8.ReadOnly = true;

                    textBoxAdjuster2.ReadOnly = false;
                    textBoxAdjuster3.ReadOnly = false;
                    textBoxAdjuster4.ReadOnly = false;
                    textBoxAdjuster5.ReadOnly = false;
                    textBoxAdjuster6.ReadOnly = false;
                    textBoxAdjuster7.ReadOnly = true;
                    textBoxAdjuster8.ReadOnly = true;

                    textBoxPickAdjuster2.ReadOnly = false;
                    textBoxPickAdjuster3.ReadOnly = false;
                    textBoxPickAdjuster4.ReadOnly = false;
                    textBoxPickAdjuster5.ReadOnly = false;
                    textBoxPickAdjuster6.ReadOnly = false;
                    textBoxPickAdjuster7.ReadOnly = true;
                    textBoxPickAdjuster8.ReadOnly = true;

                    textBoxI1X.ReadOnly = true;
                    textBoxI1Y.ReadOnly = true;
                    break;
                case "4-1-4":
                    textBoxE1.ReadOnly = false;
                    textBoxE2.ReadOnly = false;
                    textBoxE3.ReadOnly = false;
                    textBoxE4.ReadOnly = false;

                    textBoxAngle2.ReadOnly = false;
                    textBoxAngle3.ReadOnly = false;
                    textBoxAngle4.ReadOnly = false;
                    textBoxAngle5.ReadOnly = true;
                    textBoxAngle6.ReadOnly = true;
                    textBoxAngle7.ReadOnly = true;
                    textBoxAngle8.ReadOnly = true;

                    textBoxAdjuster2.ReadOnly = false;
                    textBoxAdjuster3.ReadOnly = false;
                    textBoxAdjuster4.ReadOnly = false;
                    textBoxAdjuster5.ReadOnly = true;
                    textBoxAdjuster6.ReadOnly = true;
                    textBoxAdjuster7.ReadOnly = true;
                    textBoxAdjuster8.ReadOnly = true;

                    textBoxPickAdjuster2.ReadOnly = false;
                    textBoxPickAdjuster3.ReadOnly = false;
                    textBoxPickAdjuster4.ReadOnly = false;
                    textBoxPickAdjuster5.ReadOnly = true;
                    textBoxPickAdjuster6.ReadOnly = true;
                    textBoxPickAdjuster7.ReadOnly = true;
                    textBoxPickAdjuster8.ReadOnly = true;

                    textBoxI1X.ReadOnly = false;
                    textBoxI1Y.ReadOnly = false;
                    break;
                case "All":
                    textBoxE1.ReadOnly = false;
                    textBoxE2.ReadOnly = false;
                    textBoxE3.ReadOnly = false;
                    textBoxE4.ReadOnly = false;

                    textBoxAngle1.ReadOnly = false;
                    textBoxAngle2.ReadOnly = false;
                    textBoxAngle3.ReadOnly = false;
                    textBoxAngle4.ReadOnly = false;
                    textBoxAngle5.ReadOnly = false;
                    textBoxAngle6.ReadOnly = false;
                    textBoxAngle7.ReadOnly = false;
                    textBoxAngle8.ReadOnly = false;

                    textBoxAdjuster1.ReadOnly = false;
                    textBoxAdjuster2.ReadOnly = false;
                    textBoxAdjuster3.ReadOnly = false;
                    textBoxAdjuster4.ReadOnly = false;
                    textBoxAdjuster5.ReadOnly = false;
                    textBoxAdjuster6.ReadOnly = false;
                    textBoxAdjuster7.ReadOnly = false;
                    textBoxAdjuster8.ReadOnly = false;

                    textBoxPickAdjuster1.ReadOnly = false;
                    textBoxPickAdjuster2.ReadOnly = false;
                    textBoxPickAdjuster3.ReadOnly = false;
                    textBoxPickAdjuster4.ReadOnly = false;
                    textBoxPickAdjuster5.ReadOnly = false;
                    textBoxPickAdjuster6.ReadOnly = false;
                    textBoxPickAdjuster7.ReadOnly = false;
                    textBoxPickAdjuster8.ReadOnly = false;

                    textBoxI1X.ReadOnly = false;
                    textBoxI1Y.ReadOnly = false;
                    break;
            }
        }

        private void buttonTwoLocationDistance_Click(object sender, EventArgs e)
        {
            if (mywh == null) return;
            else
            {
                int loc1 = Convert.ToInt32(textBoxLoc1.Text);
                int loc2 = Convert.ToInt32(textBoxLoc2.Text);

                graphicsObj = Graphics.FromImage(myBitmap);
                graphicsObj.Clear(Color.White);
                this.drawWarehouse();
                if (mywh.usevisibilitygraph)
                {
                    colorNode(mywh.graphnodes.ElementAt(loc1));
                    colorNode(mywh.graphnodes.ElementAt(loc2));

                    panelDrawing.Refresh();

                    textBoxTSP.Text = mywh.shortestPathDistanceTwoLocationsVisibilityGraph(mywh.graphnodes.ElementAt(loc1), mywh.graphnodes.ElementAt(loc2)).ToString();
                }
                else
                {
                    colorNode(mywh.locationnodes.ElementAt(loc1));
                    colorNode(mywh.locationnodes.ElementAt(loc2));

                    panelDrawing.Refresh();

                    textBoxTSP.Text = mywh.shortestPathDistanceTwoLocations(mywh.locationnodes.ElementAt(loc1), mywh.locationnodes.ElementAt(loc2)).ToString();
                }
            }
        }

        //Uniformly distributed random number generator with parameter mean and sigma
        //rnd parameter is used to create different random numbers with different seeds
        private double uniformrandom(double minval, double maxval, Random rnd)
        {
            int precision = 100000;
            double tmp, randomnum, rn1;
            if (minval > maxval)//if minval is greater than maxval then swap
            {
                tmp = minval;
                minval = maxval;
                maxval = tmp;
            }
            rn1 = rnd.Next(precision);
            randomnum = (minval + (rn1 / precision) * (maxval - minval));
            return randomnum;
        }

        private void textBoxPickerSize_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToDouble(textBoxCrossAisleWidth.Text) / (Math.Sqrt(2)) <= Convert.ToDouble(textBoxPickerSize.Text))
                {
                    textBoxNeighbors.AppendText("Picker size should be to less than \"Cross Aisle Width / SQRT(2)\" = " + (Convert.ToDouble(textBoxCrossAisleWidth.Text) / (Math.Sqrt(2))).ToString() + " because of cross aisle width buffer zone collapse possibility\n");
                    textBoxNeighbors.AppendText("Picker size is changed to nearest floor integer\n");
                    textBoxPickerSize.Text = (Math.Floor(Convert.ToDouble(textBoxCrossAisleWidth.Text) / (Math.Sqrt(2)))).ToString();
                }
                if (Convert.ToDouble(textBoxPickingAisleWidth.Text) <= Convert.ToDouble(textBoxPickerSize.Text))
                {
                    textBoxNeighbors.AppendText("Picker size should be less than \"Pick Aisle Width\" = " + textBoxPickingAisleWidth.Text + " because of pick aisle width buffer zone collapse possibility\n Picker size is changed to nearest floor integer\n");
                    textBoxNeighbors.AppendText("Picker size is changed to nearest floor integer\n");
                    textBoxPickerSize.Text = (Math.Floor(Convert.ToDouble(textBoxPickingAisleWidth.Text) - options.getEpsilon())).ToString();
                }
            }
            catch
            {

            }
          
        }

        private void textBoxCrossAisleWidth_Leave(object sender, EventArgs e)
        {
            if (Convert.ToDouble(textBoxCrossAisleWidth.Text) / (2 * Math.Sqrt(2)) < Convert.ToDouble(textBoxPickerSize.Text))
            {
                textBoxPickerSize.Text = (Convert.ToDouble(textBoxPickerSize.Text) * (2 * Math.Sqrt(2))).ToString();
            }
        }

        private void buttonGetDesign_Click(object sender, EventArgs e)
        {
            if(lines != null)
            {
                if (lines[0].Length == 65)
                {
                    int i = Convert.ToInt32(textBoxDesignNumber.Text);
                    textBoxDemandSkewness20.Text = lines[i][10];
                    textBoxDemandSkewness80.Text = lines[i][11];
                    textBoxNumberSKUs.Text = lines[i][12];
                    textBoxNumberOrders.Text = lines[i][13];
                    textBoxOrderSize.Text = lines[i][14];
                    textBoxOrderGenRandSeed.Text = lines[i][15];
                    textBoxWHWidth.Text = lines[i][20];
                    textBoxWHDepth.Text = lines[i][21];
                    textBoxArea.Text = lines[i][22];
                    textBoxAspectRatio.Text = lines[i][23];
                    textBoxE1.Text = lines[i][24];
                    textBoxE2.Text = lines[i][25];
                    textBoxE3.Text = lines[i][26];
                    textBoxE4.Text = lines[i][27];
                    textBoxI1X.Text = lines[i][28];
                    textBoxI1Y.Text = lines[i][29];
                    textBoxpd.Text = lines[i][30];
                    textBoxAngle1.Text = lines[i][31];
                    textBoxAngle2.Text = lines[i][32];
                    textBoxAngle3.Text = lines[i][33];
                    textBoxAngle4.Text = lines[i][34];
                    textBoxAngle5.Text = lines[i][35];
                    textBoxAngle6.Text = lines[i][36];
                    textBoxAngle7.Text = lines[i][37];
                    textBoxAngle8.Text = lines[i][38];
                    textBoxAdjuster1.Text = lines[i][39];
                    textBoxAdjuster2.Text = lines[i][40];
                    textBoxAdjuster3.Text = lines[i][41];
                    textBoxAdjuster4.Text = lines[i][42];
                    textBoxAdjuster5.Text = lines[i][43];
                    textBoxAdjuster6.Text = lines[i][44];
                    textBoxAdjuster7.Text = lines[i][45];
                    textBoxAdjuster8.Text = lines[i][46];
                    textBoxPickAdjuster1.Text = lines[i][47];
                    textBoxPickAdjuster2.Text = lines[i][48];
                    textBoxPickAdjuster3.Text = lines[i][49];
                    textBoxPickAdjuster4.Text = lines[i][50];
                    textBoxPickAdjuster5.Text = lines[i][51];
                    textBoxPickAdjuster6.Text = lines[i][52];
                    textBoxPickAdjuster7.Text = lines[i][53];
                    textBoxPickAdjuster8.Text = lines[i][54];
                    if (lines[i][7] == "1") checkBoxVisibilityGraph.Checked = true; else checkBoxVisibilityGraph.Checked = false;
                    if (lines[i][8] == "1") checkBoxStraightAllocation.Checked = true; else checkBoxStraightAllocation.Checked = false;
                    if (lines[i][55] == "1") checkBoxC12.Checked = true; else checkBoxC12.Checked = false;
                    if (lines[i][56] == "1") checkBoxC13.Checked = true; else checkBoxC13.Checked = false;
                    if (lines[i][57] == "1") checkBoxC14.Checked = true; else checkBoxC14.Checked = false;
                    if (lines[i][58] == "1") checkBoxC15.Checked = true; else checkBoxC15.Checked = false;
                    if (lines[i][59] == "1") checkBoxC23.Checked = true; else checkBoxC23.Checked = false;
                    if (lines[i][60] == "1") checkBoxC24.Checked = true; else checkBoxC24.Checked = false;
                    if (lines[i][61] == "1") checkBoxC25.Checked = true; else checkBoxC25.Checked = false;
                    if (lines[i][62] == "1") checkBoxC34.Checked = true; else checkBoxC34.Checked = false;
                    if (lines[i][63] == "1") checkBoxC35.Checked = true; else checkBoxC35.Checked = false;
                    if (lines[i][64] == "1") checkBoxC45.Checked = true; else checkBoxC45.Checked = false;
                }
                else
                {
                    int i = Convert.ToInt32(textBoxDesignNumber.Text);
                    textBoxWHWidth.Text = lines[i][3];
                    textBoxWHDepth.Text = lines[i][4];
                    textBoxArea.Text = lines[i][5];
                    textBoxAspectRatio.Text = lines[i][6];
                    textBoxE1.Text = lines[i][7];
                    textBoxE2.Text = lines[i][8];
                    textBoxE3.Text = lines[i][9];
                    textBoxE4.Text = lines[i][10];
                    textBoxI1X.Text = lines[i][11];
                    textBoxI1Y.Text = lines[i][12];
                    textBoxpd.Text = lines[i][13];
                    textBoxAngle1.Text = lines[i][14];
                    textBoxAngle2.Text = lines[i][15];
                    textBoxAngle3.Text = lines[i][16];
                    textBoxAngle4.Text = lines[i][17];
                    textBoxAngle5.Text = lines[i][18];
                    textBoxAngle6.Text = lines[i][19];
                    textBoxAngle7.Text = lines[i][20];
                    textBoxAngle8.Text = lines[i][21];
                    textBoxAdjuster1.Text = lines[i][22];
                    textBoxAdjuster2.Text = lines[i][23];
                    textBoxAdjuster3.Text = lines[i][24];
                    textBoxAdjuster4.Text = lines[i][25];
                    textBoxAdjuster5.Text = lines[i][26];
                    textBoxAdjuster6.Text = lines[i][27];
                    textBoxAdjuster7.Text = lines[i][28];
                    textBoxAdjuster8.Text = lines[i][29];
                    textBoxPickAdjuster1.Text = lines[i][30];
                    textBoxPickAdjuster2.Text = lines[i][31];
                    textBoxPickAdjuster3.Text = lines[i][32];
                    textBoxPickAdjuster4.Text = lines[i][33];
                    textBoxPickAdjuster5.Text = lines[i][34];
                    textBoxPickAdjuster6.Text = lines[i][35];
                    textBoxPickAdjuster7.Text = lines[i][36];
                    textBoxPickAdjuster8.Text = lines[i][37];
                    if (lines[i][48] == "1") checkBoxC12.Checked = true; else checkBoxC12.Checked = false;
                    if (lines[i][49] == "1") checkBoxC13.Checked = true; else checkBoxC13.Checked = false;
                    if (lines[i][50] == "1") checkBoxC14.Checked = true; else checkBoxC14.Checked = false;
                    if (lines[i][51] == "1") checkBoxC15.Checked = true; else checkBoxC15.Checked = false;
                    if (lines[i][52] == "1") checkBoxC23.Checked = true; else checkBoxC23.Checked = false;
                    if (lines[i][53] == "1") checkBoxC24.Checked = true; else checkBoxC24.Checked = false;
                    if (lines[i][54] == "1") checkBoxC25.Checked = true; else checkBoxC25.Checked = false;
                    if (lines[i][55] == "1") checkBoxC34.Checked = true; else checkBoxC34.Checked = false;
                    if (lines[i][56] == "1") checkBoxC35.Checked = true; else checkBoxC35.Checked = false;
                    if (lines[i][57] == "1") checkBoxC45.Checked = true; else checkBoxC45.Checked = false;
                }
            }
        }

        private void buttonImportDesign_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //Only show .csv files
            ofd.Filter = "Microsoft Office Excel Comma Separated Values File|*.csv";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                string fileName = ofd.FileName;
                FileInfo fileIn = new FileInfo(fileName);
                StreamReader reader;
                try
                {
                    reader = fileIn.OpenText();
                }
                catch
                {
                    textBoxNeighbors.AppendText("File is Already Open, Close File\n"); return;
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
            textBoxNeighbors.AppendText("Importing done.");
        }

        private void buttonBatchSearch_Click(object sender, EventArgs e)
        {
            importOptimizationRuns();
        }

        private void newChartFormMenu_Click(object sender, EventArgs e)
        {
            ChartForm chartform = new ChartForm();
            chartform.ShowDialog();
        }

        private void checkBoxNFT_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBoxNFT.Checked)
            {
                textBoxNFT0.ReadOnly = false;
                textBoxNFTLambda.ReadOnly = false;
            }
            else
            {
                textBoxNFT0.ReadOnly = true;
                textBoxNFTLambda.ReadOnly = true;
            }
        }

        private void checkBoxVisibilityGraph_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBoxVisibilityGraph.Checked)
            {
                textBoxPickerSize.ReadOnly = false;
            }
            else
            {
                textBoxPickerSize.ReadOnly = true;
            }
        }
    }
}