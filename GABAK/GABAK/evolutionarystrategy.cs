//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GABAK
{
    class evolutionarystrategy
    {
        private int seed = 0;
        private int M = 5;
        private int lambda = 10;
        private int iteration = 10;
        private double sigma = 0.5;
        private double successrule = 0.2;
        private esvariable[] parents;
        private int size = 10;
        private Random[] rnd;
        private Random[] rnd1;
        private warehouse wh;
        private int numberofsuccessfulchildrencreated;
        private int numberofchildrencreated;
        private int counter;
        private int generalsuccess;
        private int generalfail;
        private int numberoforders;
        private int numberofgaps;
        private int numberofsuccessfulchildrencreatedperiteration = 0;
        private double[] u;
        private double[] l;
        private bool allsearch = true;
        private int computing = 0;
        private bool optimal = false;
        private socketservers mysocketservers;
        private string designclass;
        private bool usenft = false;
        private double nft0;
        private double nftlambda;

        /// <summary>
        /// Constructor of ES for non-NFT
        /// </summary>
        /// <param name="p_seed">Seed number</param>
        /// <param name="p_M">Number of parents</param>
        /// <param name="p_lambda">Number of children</param>
        /// <param name="p_size">Number of variables in a solution, this number is different between allsearch and specific design search</param>
        /// <param name="p_iteration">Maximum number of iterations</param>
        /// <param name="p_sigma">Sigma value for ES</param>
        /// <param name="p_counter">How often we should update sigma value</param>
        /// <param name="p_successrule">Success rule threshold, this is usually 0.2 (i.e., if 20 percent of the children are more successfuly than their parents then decrease sigma)</param>
        /// <param name="p_l">Lower bound array</param>
        /// <param name="p_u">Upper bound array</param>
        /// <param name="p_wh">Warehouse object</param>
        /// <param name="p_designclass">Design class selection (All or specific design class)</param>
        /// <param name="p_allsearch">Use all of the orders or use dynamicly increasing number of orders</param>
        /// <param name="p_computing">0 is Parallel, 1 is Distributed, 2 is Single Thread Computing</param>
        /// <param name="p_optimal">True use Concorde, False use LKH</param>
        public evolutionarystrategy(int p_seed, int p_M, int p_lambda, int p_size, int p_iteration, double p_sigma, int p_counter, double p_successrule, double[] p_l, double[] p_u, warehouse p_wh, string p_designclass, bool p_allsearch, int p_computing, bool p_optimal)
        {
            this.seed = p_seed;
            this.M = p_M;
            this.lambda = p_lambda;
            this.iteration = p_iteration;
            this.sigma = p_sigma;
            this.counter = p_counter;
            this.successrule = p_successrule;
            this.size = p_size;
            this.u = p_u;
            this.l = p_l;
            this.rnd = new Random[this.M];
            this.rnd1 = new Random[this.lambda];
            this.wh = p_wh;
            this.designclass = p_designclass;
            this.allsearch = p_allsearch;
            this.computing = p_computing;
            this.optimal = p_optimal;
            if (computing == 1)
            {
                mysocketservers = new socketservers();
                mysocketservers.checkAvailableConcordeServers();
            }
            //Use individual random numbers for each M and lambda
            for (int i = 0; i < this.M; i++)
            {
                this.rnd[i] = new Random(this.seed + i * 1000);
            }
            for (int i = 0; i < this.lambda; i++)
            {
                this.rnd1[i] = new Random(this.seed + this.M * 1000 + i * 1000);
            }
        }

        /// <summary>
        /// Constructor of ES for NFT
        /// </summary>
        /// <param name="p_seed">Seed number</param>
        /// <param name="p_M">Number of parents</param>
        /// <param name="p_lambda">Number of children</param>
        /// <param name="p_size">Number of variables in a solution, this number is different between allsearch and specific design search</param>
        /// <param name="p_iteration">Maximum number of iterations</param>
        /// <param name="p_sigma">Sigma value for ES</param>
        /// <param name="p_counter">How often we should update sigma value</param>
        /// <param name="p_successrule">Success rule threshold, this is usually 0.2 (i.e., if 20 percent of the children are more successfuly than their parents then decrease sigma)</param>
        /// <param name="p_l">Lower bound array</param>
        /// <param name="p_u">Upper bound array</param>
        /// <param name="p_wh">Warehouse object</param>
        /// <param name="p_designclass">Design class selection (All or specific design class)</param>
        /// <param name="p_allsearch">Use all of the orders or use dynamicly increasing number of orders</param>
        /// <param name="p_computing">0 is Parallel, 1 is Distributed, 2 is Single Thread Computing</param>
        /// <param name="p_optimal">True use Concorde, False use LKH</param>
        /// <param name="p_nft0">NFT Initial</param>
        /// <param name="p_nftlambda">Lambda for NFT</param>
        public evolutionarystrategy(int p_seed, int p_M, int p_lambda, int p_size, int p_iteration, double p_sigma, int p_counter, double p_successrule, double[] p_l, double[] p_u, warehouse p_wh, string p_designclass, bool p_allsearch, int p_computing, bool p_optimal, double p_nft0, double p_nftlambda)
        {
            this.seed = p_seed;
            this.M = p_M;
            this.lambda = p_lambda;
            this.iteration = p_iteration;
            this.sigma = p_sigma;
            this.counter = p_counter;
            this.successrule = p_successrule;
            this.size = p_size;
            this.u = p_u;
            this.l = p_l;
            this.rnd = new Random[this.M];
            this.rnd1 = new Random[this.lambda];
            this.wh = p_wh;
            this.designclass = p_designclass;
            this.allsearch = p_allsearch;
            this.computing = p_computing;
            this.optimal = p_optimal;
            this.usenft = true;
            this.nft0 = p_nft0;
            this.nftlambda = p_nftlambda;
            if (computing == 1)
            {
                mysocketservers = new socketservers();
                mysocketservers.checkAvailableConcordeServers();
            }
            //Use individual random numbers for each M and lambda
            for (int i = 0; i < this.M; i++)
            {
                this.rnd[i] = new Random(this.seed + i * 1000);
            }
            for (int i = 0; i < this.lambda; i++)
            {
                this.rnd1[i] = new Random(this.seed + this.M * 1000 + i * 1000);
            }
        }

        /// <summary>
        /// Creates initial population for ES
        /// </summary>
        public void createInitialParentPopulation()
        {
            parents = new esvariable[M];

            for (int i = 0; i < this.M; i++)
            {
                double[] x = new double[this.size];

                if (designclass != "All")
                {
                    //Randomly create all the parameters between their lower and upper limits
                    for (int j = 0; j < this.size; j++)
                    {
                        if (l[j] != u[j])
                            x[j] = uniformrandom(l[j], u[j], rnd[i]);
                        else
                            x[j] = l[j];
                    }
                }
                else//New encoding
                {
                    for (int j = 0; j < l.Count(); j++)
                    {
                        if (l[j] != u[j])
                            x[j] = uniformrandom(l[j], u[j], rnd[i]);
                        else
                            x[j] = l[j];
                    }
                    //This part is for connection instances part of the encoding
                    for (int j = l.Count(); j < this.size; j++)
                    {
                        int indexencodingprobability = j - (this.size - l.Count());
                        if (x[indexencodingprobability] == 1) x[j] = 1;
                        else if (x[indexencodingprobability] == 0) x[j] = 0;
                        else
                        {
                            if (uniformrandom(0, 1, rnd1[i]) < x[indexencodingprobability])//If random number is lower than the probability of creating that aisle than instance is 0
                            {
                                x[j] = 0;
                            }
                            else
                            {
                                x[j] = 1;
                            }
                        }
                    }
                }
                numberofgaps = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(iteration) / Convert.ToDouble(this.counter)));
                if (allsearch)
                {
                    numberoforders = wh.getOrders().Count();
                }
                else
                {
                    numberoforders = Convert.ToInt32(1 / Convert.ToDouble(numberofgaps) * wh.getOrders().Count());
                }
                //string logfile = "log.csv";
                esvariable p = new esvariable(size, x, l, u, wh, numberoforders, computing, optimal, mysocketservers, designclass);
                //writeLog(logfile, p);
                if (p.isInfeasible() == false)
                {
                    parents[i] = p;//add to population
                }
                else
                {
                    i--;//Do this iteration again
                }
            }
            if (!usenft)//Without Near Feasibility Threshold
            {
                parents = sortPopulation(parents);
            }
            else//With Near Feasibility Threshold
            {
                parents = sortPopulation(parents, 0);
            }
        }

        /// <summary>
        /// This runs the iterations for the ES
        /// </summary>
        /// <param name="p_pb">Used for accessing progress bar in main form</param>
        /// <param name="p_tb">Used for accessing textbox in main form</param>
        /// <param name="p_form1">Used for accesing main form objects</param>
        /// <param name="p_wh">Warehouse object used for accesing main form warehouse object</param>
        /// <param name="p_problemfolder">Problemfolder is null when batch optimization is not used, otherwise it uses the location of the folder for batch optimization</param>
        public void Solve(System.Windows.Forms.ProgressBar p_pb, System.Windows.Forms.TextBox p_tb, MainForm p_form1, warehouse p_wh, string p_problemfolder)
        {
            string searchtype = "";
            if(allsearch)
            {
                searchtype = "AllSearch";
            }
            else
            {
                searchtype = "DynamicSearch";
            }
            string experimentdirectoryorg = p_problemfolder + "\\ES-" + M.ToString() + "-" + lambda.ToString() + "-" + seed.ToString() + "-" + iteration.ToString() + "-" + sigma.ToString().Replace(".", string.Empty) + "-" + counter.ToString() + "-" + searchtype + "-" + designclass.Replace("-", string.Empty);
            int k = 0;
            string experimentdirectory = experimentdirectoryorg;
            while (System.IO.Directory.Exists(experimentdirectory))
            {
                if(k != 0)//in first folder creation don't put index to the end
                {
                    experimentdirectory = experimentdirectoryorg + "-" + k.ToString();
                }
                k++;
            }

            System.IO.Directory.CreateDirectory(experimentdirectory);
            
            
            p_pb.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            p_pb.Maximum = 100;
            p_pb.Value = 0;
            double previouscost = double.MaxValue;
            csvexport myexcel = new csvexport();
            //Save initial population as first row in excel file
            myexcel.addRow();
            if (designclass != "All")
            {
                myexcel["Cost"] = getCost().ToString("R");
                myexcel["Locs"] = getBestWarehouse().totalNumberOfLocations().ToString();
                myexcel["SR"] = "None";
                myexcel["Width"] = getBestWarehouse().getWidth().ToString("R");
                myexcel["Depth"] = getBestWarehouse().getDepth().ToString("R");
                myexcel["Area"] = getBestWarehouse().area.ToString("R");
                myexcel["Ratio"] = getBest().getx()[0].ToString("R");
                myexcel["E1"] = getBest().getx()[1].ToString("R");
                myexcel["E2"] = getBest().getx()[2].ToString("R");
                myexcel["E3"] = getBest().getx()[3].ToString("R");
                myexcel["E4"] = getBest().getx()[4].ToString("R");
                myexcel["I1X"] = getBest().getx()[5].ToString("R");
                myexcel["I1Y"] = getBest().getx()[6].ToString("R");
                myexcel["PD"] = getBest().getx()[7].ToString();
                myexcel["Angle1"] = (180 * getBest().getx()[8]).ToString("R");
                myexcel["Angle2"] = (180 * getBest().getx()[9]).ToString("R");
                myexcel["Angle3"] = (180 * getBest().getx()[10]).ToString("R");
                myexcel["Angle4"] = (180 * getBest().getx()[11]).ToString("R");
                myexcel["Angle5"] = (180 * getBest().getx()[12]).ToString("R");
                myexcel["Angle6"] = (180 * getBest().getx()[13]).ToString("R");
                myexcel["Adjuster1"] = getBest().getx()[14].ToString("R");
                myexcel["Adjuster2"] = getBest().getx()[15].ToString("R");
                myexcel["Adjuster3"] = getBest().getx()[16].ToString("R");
                myexcel["Adjuster4"] = getBest().getx()[17].ToString("R");
                myexcel["Adjuster5"] = getBest().getx()[18].ToString("R");
                myexcel["Adjuster6"] = getBest().getx()[19].ToString("R");
                myexcel["PickAdjuster1"] = getBest().getx()[20].ToString("R");
                myexcel["PickAdjuster2"] = getBest().getx()[21].ToString("R");
                myexcel["PickAdjuster3"] = getBest().getx()[22].ToString("R");
                myexcel["PickAdjuster4"] = getBest().getx()[23].ToString("R");
                myexcel["PickAdjuster5"] = getBest().getx()[24].ToString("R");
                myexcel["PickAdjuster6"] = getBest().getx()[25].ToString("R");
            }
            else//New encoding
            {
                myexcel["Cost"] = getCost().ToString();
                myexcel["Locs"] = getBestWarehouse().totalNumberOfLocations().ToString();
                myexcel["SR"] = "None";
                myexcel["Width"] = getBestWarehouse().getWidth().ToString();
                myexcel["Depth"] = getBestWarehouse().getDepth().ToString();
                myexcel["Area"] = getBestWarehouse().area.ToString("R");
                myexcel["Ratio"] = getBest().getx()[0].ToString("R");
                myexcel["E1"] = getBest().getx()[1].ToString("R");
                myexcel["E2"] = getBest().getx()[2].ToString("R");
                myexcel["E3"] = getBest().getx()[3].ToString("R");
                myexcel["E4"] = getBest().getx()[4].ToString("R");
                myexcel["I1X"] = getBest().getx()[5].ToString("R");
                myexcel["I1Y"] = getBest().getx()[6].ToString("R");
                myexcel["PD"] = getBest().getx()[7].ToString("R");
                myexcel["Angle1"] = (180 * getBest().getx()[8]).ToString("R");
                myexcel["Angle2"] = (180 * getBest().getx()[9]).ToString("R");
                myexcel["Angle3"] = (180 * getBest().getx()[10]).ToString("R");
                myexcel["Angle4"] = (180 * getBest().getx()[11]).ToString("R");
                myexcel["Angle5"] = (180 * getBest().getx()[12]).ToString("R");
                myexcel["Angle6"] = (180 * getBest().getx()[13]).ToString("R");
                myexcel["Angle7"] = (180 * getBest().getx()[14]).ToString("R");
                myexcel["Angle8"] = (180 * getBest().getx()[15]).ToString("R");
                myexcel["Adjuster1"] = getBest().getx()[16].ToString("R");
                myexcel["Adjuster2"] = getBest().getx()[17].ToString("R");
                myexcel["Adjuster3"] = getBest().getx()[18].ToString("R");
                myexcel["Adjuster4"] = getBest().getx()[19].ToString("R");
                myexcel["Adjuster5"] = getBest().getx()[20].ToString("R");
                myexcel["Adjuster6"] = getBest().getx()[21].ToString("R");
                myexcel["Adjuster7"] = getBest().getx()[22].ToString("R");
                myexcel["Adjuster8"] = getBest().getx()[23].ToString("R");
                myexcel["PickAdjuster1"] = getBest().getx()[24].ToString("R");
                myexcel["PickAdjuster2"] = getBest().getx()[25].ToString("R");
                myexcel["PickAdjuster3"] = getBest().getx()[26].ToString("R");
                myexcel["PickAdjuster4"] = getBest().getx()[27].ToString("R");
                myexcel["PickAdjuster5"] = getBest().getx()[28].ToString("R");
                myexcel["PickAdjuster6"] = getBest().getx()[29].ToString("R");
                myexcel["PickAdjuster7"] = getBest().getx()[30].ToString("R");
                myexcel["PickAdjuster8"] = getBest().getx()[31].ToString("R");
                myexcel["PC12"] = getBest().getx()[32].ToString("R");
                myexcel["PC13"] = getBest().getx()[33].ToString("R");
                myexcel["PC14"] = getBest().getx()[34].ToString("R");
                myexcel["PC15"] = getBest().getx()[35].ToString("R");
                myexcel["PC23"] = getBest().getx()[36].ToString("R");
                myexcel["PC24"] = getBest().getx()[37].ToString("R");
                myexcel["PC25"] = getBest().getx()[38].ToString("R");
                myexcel["PC34"] = getBest().getx()[39].ToString("R");
                myexcel["PC35"] = getBest().getx()[40].ToString("R");
                myexcel["PC45"] = getBest().getx()[41].ToString("R");
                myexcel["C12"] = getBest().getx()[42].ToString();
                myexcel["C13"] = getBest().getx()[43].ToString();
                myexcel["C14"] = getBest().getx()[44].ToString();
                myexcel["C15"] = getBest().getx()[45].ToString();
                myexcel["C23"] = getBest().getx()[46].ToString();
                myexcel["C24"] = getBest().getx()[47].ToString();
                myexcel["C25"] = getBest().getx()[48].ToString();
                myexcel["C34"] = getBest().getx()[49].ToString();
                myexcel["C35"] = getBest().getx()[50].ToString();
                myexcel["C45"] = getBest().getx()[51].ToString();
            }
            myexcel.exportToFile(experimentdirectory+"\\experiment.csv");
            //Print the first layout
            previouscost = getCost();
            p_wh.resetNetwork();
            //Reset IDs
            edge.nextID = 0;
            region.nextID = 0;
            node.nextID = 0;
            if (designclass != "All")
            {
                double[] angle = { 180 * getBest().getx()[8], 180 * getBest().getx()[9], 180 * getBest().getx()[10], 180 * getBest().getx()[11], 180 * getBest().getx()[12], 180 * getBest().getx()[13] };
                double[] adjuster = { getBest().getx()[14], getBest().getx()[15], getBest().getx()[16], getBest().getx()[17], getBest().getx()[18], getBest().getx()[19] };
                double[] pickadjuster = { getBest().getx()[20], getBest().getx()[21], getBest().getx()[22], getBest().getx()[23], getBest().getx()[24], getBest().getx()[25] };
                double[] ext = { getBest().getx()[1], getBest().getx()[2], getBest().getx()[3], getBest().getx()[4] };
                double[] intx = { getBest().getx()[5] };
                double[] inty = { getBest().getx()[6] };
                double[] pd = { getBest().getx()[7] };
                double aspectratio = getBest().getx()[0];
                p_wh.aspectratio = aspectratio;
                p_wh.setArea(getBestWarehouse().area);
                switch (designclass)
                {
                    case "0-0-0":
                        if (!p_wh.create000Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                        {
                            return;
                        }
                        break;
                    case "2-0-1":
                        if (!p_wh.create201Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                        {
                            return;
                        }
                        break;
                    case "3-0-2":
                        if (!p_wh.create302Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                        {
                            return;
                        }
                        break;
                    case "3-0-3":
                        if (!p_wh.create303Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                        {
                            return;
                        }
                        break;
                    case "3-1-3":
                        if (!p_wh.create313Warehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                        {
                            return;
                        }
                        break;
                    case "4-0-2":
                        if (!p_wh.create402Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                        {
                            return;
                        }
                        break;
                    case "4-0-4":
                        if (!p_wh.create404Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                        {
                            return;
                        }
                        break;
                    case "4-0-5":
                        if (!p_wh.create405Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                        {
                            return;
                        }
                        break;
                    case "4-1-4":
                        if (!p_wh.create414Warehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                        {
                            return;
                        }
                        break;
                    case "4-2-5":
                        if (!p_wh.create425Warehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                        {
                            return;
                        }
                        break;
                    case "6-0-3":
                        if (!p_wh.create603Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                        {
                            return;
                        }
                        break;
                }
            }
            else
            {
                double[] angle = { 180 * getBest().getx()[8], 180 * getBest().getx()[9], 180 * getBest().getx()[10], 180 * getBest().getx()[11], 180 * getBest().getx()[12], 180 * getBest().getx()[13], 180 * getBest().getx()[14], 180 * getBest().getx()[15] };
                double[] adjuster = { getBest().getx()[16], getBest().getx()[17], getBest().getx()[18], getBest().getx()[19], getBest().getx()[20], getBest().getx()[21], getBest().getx()[22], getBest().getx()[23]};
                double[] pickadjuster = { getBest().getx()[24], getBest().getx()[25], getBest().getx()[26], getBest().getx()[27], getBest().getx()[28], getBest().getx()[29], getBest().getx()[30], getBest().getx()[31] };
                double[] ext = { getBest().getx()[1], getBest().getx()[2], getBest().getx()[3], getBest().getx()[4] };
                double[] intx = { getBest().getx()[5] };
                double[] inty = { getBest().getx()[6] };
                double[] pd = { getBest().getx()[7] };
                double aspectratio = getBest().getx()[0];
                p_wh.aspectratio = aspectratio;
                p_wh.setArea(getBestWarehouse().area);

                bool[] connections = new bool[this.size - l.Count()];
                int i = 0;
                for (int j = l.Count(); j < this.size; j++)
                {
                    int indexencodingprobability = j - (this.size - l.Count());
                    if (getBest().getx()[j] == 0)//If that cross aisle exists then select that connection correct
                    {
                        connections[i] = false;
                    }
                    else
                    {
                        connections[i] = true;
                    }
                    i++;
                }

                if (!p_wh.createWarehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth, connections))
                {
                    return;
                }
            }

            if (!p_wh.usevisibilitygraph)//Aisle centers method
            {
                p_wh.createImportantNodeShortestDistances();
                p_wh.locationShortestDistance();//Calculate Shortest Path Distances for Locations
            }
            else//Visibility graph method
            {
                p_wh.createPolygonsandGraphNodes();
                p_wh.createVisibilityGraph();
                p_wh.fillGraphNodeDistances();
            }
            //Calculate Shortest Path Distances to PD
            p_wh.pdTotalDistances();
            p_wh.totalDistances();
            p_wh.rankLocations(Convert.ToDouble(p_wh.avgTourLength));
            p_wh.colorOverall();

            p_form1.drawCompleteWarehouse(p_wh.getWidth(), p_wh.getDepth());
            p_form1.saveBitmap(experimentdirectory + "\\0.bmp");
            //End print the first layout
            int counter1 = 0;
            int counter2 = 1;
            generalsuccess = 0;
            generalfail = 0;
            //This costarray is used to check for the last 50 iterations if there is improvement less than %0.5 then makes it early quit the for loop
            double[] costarray = new double[iteration];
            for (int i = 0; i < iteration; i++)
            {

                if (counter1 == this.counter)
                {
                    counter2++;
                    //Increase the step size if success rate is greater than 0.2 (or whatever is successrule) and decrease the step size otherwise
                    if ((double)numberofsuccessfulchildrencreated / (double)numberofchildrencreated > successrule)
                    {
                        sigma = sigma / 0.85;
                        generalsuccess++;
                    }
                    else
                    {
                        sigma = sigma * 0.85;
                        generalfail++;
                    }
                    counter1 = 0;
                    numberofsuccessfulchildrencreated = 0;
                    numberofchildrencreated = 0;
                    if (allsearch)
                    {
                        numberoforders = wh.getOrders().Count();
                    }
                    else
                    {
                        numberoforders = Convert.ToInt32(Convert.ToDouble(counter2) / Convert.ToDouble(numberofgaps) * wh.getOrders().Count());
                    }
                }
                counter1++;
                DateTime start = DateTime.Now;
                string logfile = experimentdirectory + "\\log.csv";
                evolution(i, logfile);
                double successrateperiteration = Convert.ToDouble(numberofsuccessfulchildrencreatedperiteration) / Convert.ToDouble(lambda);
                numberofsuccessfulchildrencreatedperiteration = 0;//Resetsucessperiteration
                TimeSpan elapsed = DateTime.Now - start;
                p_pb.Value = Convert.ToInt32(Convert.ToDouble(i + 1) / Convert.ToDouble(iteration) * 100);
                p_tb.AppendText("Iteration" + (i + 1).ToString() + ": " + getCost().ToString("0.000") + " Time: " + elapsed.ToString() +  " SR:" + successrateperiteration.ToString("0.000") + "\n");
                double currentcost = double.MaxValue;
                if (!usenft)
                {
                    currentcost = getCost();
                }
                else
                {
                    currentcost = getCost(i, nft0, nftlambda);
                }
                myexcel = new csvexport();
                myexcel.addRow();
                if (designclass != "All")
                {
                    costarray[i] = currentcost;
                    myexcel["Cost"] = getCost().ToString();
                    myexcel["Locs"] = getBestWarehouse().totalNumberOfLocations().ToString();
                    myexcel["SR"] = successrateperiteration.ToString();
                    myexcel["Width"] = getBestWarehouse().getWidth().ToString();
                    myexcel["Depth"] = getBestWarehouse().getDepth().ToString();
                    myexcel["Area"] = getBestWarehouse().area.ToString("R");
                    myexcel["Ratio"] = getBest().getx()[0].ToString("R");
                    myexcel["E1"] = getBest().getx()[1].ToString("R");
                    myexcel["E2"] = getBest().getx()[2].ToString("R");
                    myexcel["E3"] = getBest().getx()[3].ToString("R");
                    myexcel["E4"] = getBest().getx()[4].ToString("R");
                    myexcel["I1X"] = getBest().getx()[5].ToString("R");
                    myexcel["I1Y"] = getBest().getx()[6].ToString("R");
                    myexcel["PD"] = getBest().getx()[7].ToString("R");
                    myexcel["Angle1"] = (180 * getBest().getx()[8]).ToString("R");
                    myexcel["Angle2"] = (180 * getBest().getx()[9]).ToString("R");
                    myexcel["Angle3"] = (180 * getBest().getx()[10]).ToString("R");
                    myexcel["Angle4"] = (180 * getBest().getx()[11]).ToString("R");
                    myexcel["Angle5"] = (180 * getBest().getx()[12]).ToString("R");
                    myexcel["Angle6"] = (180 * getBest().getx()[13]).ToString("R");
                    myexcel["Adjuster1"] = getBest().getx()[14].ToString("R");
                    myexcel["Adjuster2"] = getBest().getx()[15].ToString("R");
                    myexcel["Adjuster3"] = getBest().getx()[16].ToString("R");
                    myexcel["Adjuster4"] = getBest().getx()[17].ToString("R");
                    myexcel["Adjuster5"] = getBest().getx()[18].ToString("R");
                    myexcel["Adjuster6"] = getBest().getx()[19].ToString("R");
                    myexcel["PickAdjuster1"] = getBest().getx()[20].ToString("R");
                    myexcel["PickAdjuster2"] = getBest().getx()[21].ToString("R");
                    myexcel["PickAdjuster3"] = getBest().getx()[22].ToString("R");
                    myexcel["PickAdjuster4"] = getBest().getx()[23].ToString("R");
                    myexcel["PickAdjuster5"] = getBest().getx()[24].ToString("R");
                    myexcel["PickAdjuster6"] = getBest().getx()[25].ToString("R");
                }
                else//New encoding
                {
                    costarray[i] = currentcost;
                    myexcel["Cost"] = getCost().ToString();
                    myexcel["Locs"] = getBestWarehouse().totalNumberOfLocations().ToString();
                    myexcel["SR"] = successrateperiteration.ToString("R");
                    myexcel["Width"] = getBestWarehouse().getWidth().ToString("R");
                    myexcel["Depth"] = getBestWarehouse().getDepth().ToString("R");
                    myexcel["Area"] = getBestWarehouse().area.ToString("R");
                    myexcel["Ratio"] = getBest().getx()[0].ToString("R");
                    myexcel["E1"] = getBest().getx()[1].ToString("R");
                    myexcel["E2"] = getBest().getx()[2].ToString("R");
                    myexcel["E3"] = getBest().getx()[3].ToString("R");
                    myexcel["E4"] = getBest().getx()[4].ToString("R");
                    myexcel["I1X"] = getBest().getx()[5].ToString("R");
                    myexcel["I1Y"] = getBest().getx()[6].ToString("R");
                    myexcel["PD"] = getBest().getx()[7].ToString("R");
                    myexcel["Angle1"] = (180 * getBest().getx()[8]).ToString("R");
                    myexcel["Angle2"] = (180 * getBest().getx()[9]).ToString("R");
                    myexcel["Angle3"] = (180 * getBest().getx()[10]).ToString("R");
                    myexcel["Angle4"] = (180 * getBest().getx()[11]).ToString("R");
                    myexcel["Angle5"] = (180 * getBest().getx()[12]).ToString("R");
                    myexcel["Angle6"] = (180 * getBest().getx()[13]).ToString("R");
                    myexcel["Angle7"] = (180 * getBest().getx()[14]).ToString("R");
                    myexcel["Angle8"] = (180 * getBest().getx()[15]).ToString("R");
                    myexcel["Adjuster1"] = getBest().getx()[16].ToString("R");
                    myexcel["Adjuster2"] = getBest().getx()[17].ToString("R");
                    myexcel["Adjuster3"] = getBest().getx()[18].ToString("R");
                    myexcel["Adjuster4"] = getBest().getx()[19].ToString("R");
                    myexcel["Adjuster5"] = getBest().getx()[20].ToString("R");
                    myexcel["Adjuster6"] = getBest().getx()[21].ToString("R");
                    myexcel["Adjuster7"] = getBest().getx()[22].ToString("R");
                    myexcel["Adjuster8"] = getBest().getx()[23].ToString("R");
                    myexcel["PickAdjuster1"] = getBest().getx()[24].ToString("R");
                    myexcel["PickAdjuster2"] = getBest().getx()[25].ToString("R");
                    myexcel["PickAdjuster3"] = getBest().getx()[26].ToString("R");
                    myexcel["PickAdjuster4"] = getBest().getx()[27].ToString("R");
                    myexcel["PickAdjuster5"] = getBest().getx()[28].ToString("R");
                    myexcel["PickAdjuster6"] = getBest().getx()[29].ToString("R");
                    myexcel["PickAdjuster7"] = getBest().getx()[30].ToString("R");
                    myexcel["PickAdjuster8"] = getBest().getx()[31].ToString("R");
                    myexcel["PC12"] = getBest().getx()[32].ToString("R");
                    myexcel["PC13"] = getBest().getx()[33].ToString("R");
                    myexcel["PC14"] = getBest().getx()[34].ToString("R");
                    myexcel["PC15"] = getBest().getx()[35].ToString("R");
                    myexcel["PC23"] = getBest().getx()[36].ToString("R");
                    myexcel["PC24"] = getBest().getx()[37].ToString("R");
                    myexcel["PC25"] = getBest().getx()[38].ToString("R");
                    myexcel["PC34"] = getBest().getx()[39].ToString("R");
                    myexcel["PC35"] = getBest().getx()[40].ToString("R");
                    myexcel["PC45"] = getBest().getx()[41].ToString("R");
                    myexcel["C12"] = getBest().getx()[42].ToString();
                    myexcel["C13"] = getBest().getx()[43].ToString();
                    myexcel["C14"] = getBest().getx()[44].ToString();
                    myexcel["C15"] = getBest().getx()[45].ToString();
                    myexcel["C23"] = getBest().getx()[46].ToString();
                    myexcel["C24"] = getBest().getx()[47].ToString();
                    myexcel["C25"] = getBest().getx()[48].ToString();
                    myexcel["C34"] = getBest().getx()[49].ToString();
                    myexcel["C35"] = getBest().getx()[50].ToString();
                    myexcel["C45"] = getBest().getx()[51].ToString();
                }
                myexcel.appendToFile(experimentdirectory + "\\experiment.csv");
                
                //There is a new best warehouse so print the layout to the screen
                if (currentcost < previouscost)
                {
                    previouscost = currentcost;
                    p_wh.resetNetwork();
                    //Reset IDs
                    edge.nextID = 0;
                    region.nextID = 0;
                    node.nextID = 0;
                    if (designclass != "All")
                    {
                        double[] angle = { 180 * getBest().getx()[8], 180 * getBest().getx()[9], 180 * getBest().getx()[10], 180 * getBest().getx()[11], 180 * getBest().getx()[12], 180 * getBest().getx()[13] };
                        double[] adjuster = { getBest().getx()[14], getBest().getx()[15], getBest().getx()[16], getBest().getx()[17], getBest().getx()[18], getBest().getx()[19] };
                        double[] pickadjuster = { getBest().getx()[20], getBest().getx()[21], getBest().getx()[22], getBest().getx()[23], getBest().getx()[24], getBest().getx()[25] };
                        double[] ext = { getBest().getx()[1], getBest().getx()[2], getBest().getx()[3], getBest().getx()[4] };
                        double[] intx = { getBest().getx()[5] };
                        double[] inty = { getBest().getx()[6] };
                        double[] pd = { getBest().getx()[7] };
                        double aspectratio = getBest().getx()[0];
                        p_wh.aspectratio = aspectratio;
                        p_wh.setArea(getBestWarehouse().area);
                        switch (designclass)
                        {
                            case "0-0-0":
                                if (!p_wh.create000Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                                {
                                    return;
                                }
                                break;
                            case "2-0-1":
                                if (!p_wh.create201Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                                {
                                    return;
                                }
                                break;
                            case "3-0-2":
                                if (!p_wh.create302Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                                {
                                    return;
                                }
                                break;
                            case "3-0-3":
                                if (!p_wh.create303Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                                {
                                    return;
                                }
                                break;
                            case "3-1-3":
                                if (!p_wh.create313Warehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                                {
                                    return;
                                }
                                break;
                            case "4-0-2":
                                if (!p_wh.create402Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                                {
                                    return;
                                }
                                break;
                            case "4-0-4":
                                if (!p_wh.create404Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                                {
                                    return;
                                }
                                break;
                            case "4-0-5":
                                if (!p_wh.create405Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                                {
                                    return;
                                }
                                break;
                            case "4-1-4":
                                if (!p_wh.create414Warehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                                {
                                    return;
                                }
                                break;
                            case "4-2-5":
                                if (!p_wh.create425Warehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                                {
                                    return;
                                }
                                break;
                            case "6-0-3":
                                if (!p_wh.create603Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth))
                                {
                                    return;
                                }
                                break;
                        }
                    }
                    else
                    {
                        double[] angle = { 180 * getBest().getx()[8], 180 * getBest().getx()[9], 180 * getBest().getx()[10], 180 * getBest().getx()[11], 180 * getBest().getx()[12], 180 * getBest().getx()[13], 180 * getBest().getx()[14], 180 * getBest().getx()[15] };
                        double[] adjuster = { getBest().getx()[16], getBest().getx()[17], getBest().getx()[18], getBest().getx()[19], getBest().getx()[20], getBest().getx()[21], getBest().getx()[22], getBest().getx()[23] };
                        double[] pickadjuster = { getBest().getx()[24], getBest().getx()[25], getBest().getx()[26], getBest().getx()[27], getBest().getx()[28], getBest().getx()[29], getBest().getx()[30], getBest().getx()[31] };
                        double[] ext = { getBest().getx()[1], getBest().getx()[2], getBest().getx()[3], getBest().getx()[4] };
                        double[] intx = { getBest().getx()[5] };
                        double[] inty = { getBest().getx()[6] };
                        double[] pd = { getBest().getx()[7] };
                        double aspectratio = getBest().getx()[0];
                        p_wh.aspectratio = aspectratio;
                        p_wh.setArea(getBestWarehouse().area);

                        bool[] connections = new bool[this.size - l.Count()];
                        int m = 0;
                        for (int j = l.Count(); j < this.size; j++)
                        {
                            int indexencodingprobability = j - (this.size - l.Count());
                            if (getBest().getx()[j] == 0)//If that cross aisle exists then select that connection correct
                            {
                                connections[m] = false;
                            }
                            else
                            {
                                connections[m] = true;
                            }
                            m++;
                        }

                        if (!p_wh.createWarehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, p_wh.crossaislewidth, p_wh.pickingaislewidth, p_wh.pickinglocationwidth, p_wh.pickinglocationdepth, connections))
                        {
                            return;
                        }
                    }

                    if (!p_wh.usevisibilitygraph)//Aisle centers method
                    {
                        p_wh.createImportantNodeShortestDistances();
                        //Calculate Shortest Path Distances for Locations
                        p_wh.locationShortestDistance();
                    }
                    else//Visibility graph method
                    {
                        p_wh.createPolygonsandGraphNodes();
                        p_wh.createVisibilityGraph();
                        p_wh.fillGraphNodeDistances();
                    }
                    //Calculate Shortest Path Distances to PD
                    p_wh.pdTotalDistances();
                    p_wh.totalDistances();
                    p_wh.rankLocations(Convert.ToDouble(p_wh.avgTourLength));
                    p_wh.colorOverall();

                    p_form1.drawCompleteWarehouse(p_wh.getWidth(), p_wh.getDepth());
                    p_form1.saveBitmap(experimentdirectory + "\\" + (i + 1).ToString() + ".bmp");
                }//end of printing if there is an improvement
                //Perform early termination if there is no significant (more than 0.5 percent) improvement for the last 100 iterations
                if (i >= 100 && ((costarray[i - 100] - costarray[i])/costarray[i-100]) < 0.005)
                {
                    break;
                }
            }//end of iteration
        }

        //Single sigma version
        private void evolution(int p_i, string logfile)
        {
            int[] parent = new int[lambda];
            int allpopulationsize = lambda + M;//lambda + M strategy
            esvariable[] tmppopulation = new esvariable[allpopulationsize];

            //Copy parents to the tmppopulation
            if (!allsearch)
            {
                for (int i = 0; i < M; i++)
                {
                    esvariable prt = new esvariable(size, parents[i].getx(), l, u, wh, numberoforders, computing, optimal, mysocketservers, designclass);//re-evaluate parents since numberoforders might have changed
                    tmppopulation[i] = prt;
                };
            }
            else
            {
                for (int i = 0; i < M; i++)
                {
                    tmppopulation[i] = parents[i];
                };
            }
            //Create lambda number of children from their parents
            for (int i = 0; i < lambda; i++)
            {
                //bool infeasibledesign = true;
                int index = M + i;
                parent[i] = Convert.ToInt32(i * M / lambda);//Select each parent in a deterministic way

                double[] x = new double[size];
                esvariable child = null;
                if(designclass != "All")//Fixed design class
                {
                    for (int k = 0; k < l.Count(); k++)
                    {
                        x[k] = parents[parent[i]].getx()[k] + normalDist(0, sigma, rnd1[i]);//Single sigma is used
                    }
                    child = new esvariable(size, x, l, u, wh, numberoforders, computing, optimal, mysocketservers, designclass);
                    //writeLog(logfile, child);
                }
                else//All design classes (new encoding)
                {
                    //Create this part only p
                    for (int k = 0; k < l.Count(); k++)
                    {
                        x[k] = parents[parent[i]].getx()[k] + normalDist(0, sigma, rnd1[i]);//Single sigma is used
                    }
                    setx(x);//Check if they are out of bounds and correct them, this has to be done in here before creating aisles randomly
                    for (int k = l.Count(); k < size; k++)//It won't enter this loop unless it is new encoding
                    {
                        int indexencodingprobability = k - (this.size - l.Count());
                        if (x[indexencodingprobability] == 1) x[k] = 1;
                        else if (x[indexencodingprobability] == 0) x[k] = 0;
                        else
                        {
                            if (uniformrandom(0, 1, rnd1[i]) < x[indexencodingprobability])//If random number is lower than the probability of creating that aisle than instance is 0
                            {
                                x[k] = 0;
                            }
                            else
                            {
                                x[k] = 1;
                            }
                        }
                        
                    }
                    
                    child = new esvariable(size, x, l, u, wh, numberoforders, computing, optimal, mysocketservers, designclass);
                    //writeLog(logfile, child);
                }
                tmppopulation[index] = child;
                if (!usenft)//Do not use Near Feasibility Threshold
                {
                    //If created child is better than parent then increase counter
                    if (child.getCost() < parents[parent[i]].getCost())
                    {
                        numberofsuccessfulchildrencreated++;
                        numberofsuccessfulchildrencreatedperiteration++;
                    }
                }
                else//Use Near Feasibility Threshold
                {
                    //If created child is better than parent then increase counter
                    if (child.getCost(p_i + 1, nft0, nftlambda) < parents[parent[i]].getCost(p_i + 1, nft0, nftlambda))
                    {
                        numberofsuccessfulchildrencreated++;
                        numberofsuccessfulchildrencreatedperiteration++;
                    }
                }
                numberofchildrencreated++;
            }
            if (!usenft)
            {
                tmppopulation = sortPopulation(tmppopulation);//Sort lambda + M
            }
            else
            {
                tmppopulation = sortPopulation(tmppopulation, p_i);
            }
            for (int i = 0; i < M; i++)//Select best M individuals as new parents
            {
                parents[i] = tmppopulation[i];
            }
        }

        /// <summary>
        /// Sort population (without NFT) to eliminate bottom Mu from Mu + Lambda total population (Mu + Lambda strategy)
        /// </summary>
        /// <param name="notSortedPopulation">List of not sorted population</param>
        /// <returns>Sorted population</returns>
        private esvariable[] sortPopulation(esvariable[] notSortedPopulation)
        {
            Array.Sort(notSortedPopulation, delegate(esvariable p1, esvariable p2) { return p1.getCost().CompareTo(p2.getCost()); });
            return notSortedPopulation;
        }

        /// <summary>
        /// Sort population (with NFT) to eliminate bottom Mu from Mu + Lambda total population (Mu + Lambda strategy)
        /// </summary>
        /// <param name="notSortedPopulation">List of not sorted population</param>
        /// <param name="p_i">Iteration number</param>
        /// <returns>Sorted population</returns>
        private esvariable[] sortPopulation(esvariable[] notSortedPopulation, int p_i)
        {
            Array.Sort(notSortedPopulation, delegate (esvariable p1, esvariable p2) { return p1.getCost(p_i, nft0, nftlambda).CompareTo(p2.getCost(p_i, nft0, nftlambda)); });
            return notSortedPopulation;
        }

        /// <summary>
        /// Return cost of best solution in population
        /// </summary>
        /// <returns>Cost of best solution</returns>
        public double getCost()
        {
            return parents[0].getCost();
        }
        /// <summary>
        /// Return cost of best solution in population with nft penalty included
        /// </summary>
        /// <param name="p_i">Iteration number</param>
        /// <param name="p_nft0">Initial NFT</param>
        /// <param name="p_nftlambda">NFT Lambda</param>
        /// <returns>Cost of best solution</returns>
        public double getCost(int p_i, double p_nft0, double p_nftlambda)
        {
            return parents[0].getCost(p_i, p_nft0, p_nftlambda);
        }
        /// <summary>
        /// Returns the best warehouse in population
        /// </summary>
        /// <returns>Best warehouse in population</returns>
        public warehouse getBestWarehouse()
        {
            return parents[0].getWarehouse();
        }

        /// <summary>
        /// Returns current sigma
        /// </summary>
        /// <returns>Current sigma</returns>
        public double getSigma()
        {
            return sigma;
        }

        /// <summary>
        /// Returns the number of sigma increases
        /// </summary>
        /// <returns>Number of sigma increases</returns>
        public double getGeneralSuccess()
        {
            return generalsuccess;
        }

        /// <summary>
        /// Returns the number of sigma decreases
        /// </summary>
        /// <returns>Number of sigma decreases</returns>
        public double getGeneralFail()
        {
            return generalfail;
        }

        /// <summary>
        /// Returns the best solution in population
        /// </summary>
        /// <returns>Best solution in population</returns>
        public esvariable getBest()
        {
            return parents[0];
        }

        /// <summary>
        /// Sets X variable and performs repairs if x is out of bounds
        /// </summary>
        /// <param name="p_x">X array that holds the solution</param>
        private void setx(double[] p_x)
        {
            for (int i = 0; i < l.Count(); i++)
            {
                options.totalattempts++;
                if (i < l.Count() && p_x[i] < l[i])
                {
                    options.lowerattempts++;
                    p_x[i] = l[i] + 0.1 * (u[i] - l[i]);
                }
                else if (i < u.Count() && p_x[i] > u[i])
                {
                    options.upperattempts++;
                    p_x[i] = u[i] - 0.1 * (u[i] - l[i]);
                }
                else
                {
                    p_x[i] = p_x[i];
                }
            }
        }
        
        /// <summary>
        /// Normally distributed random number generator with parameter mean and sigma
        /// rnd parameter is used to create different random numbers with different seeds
        /// </summary>
        /// <param name="mean">Mean</param>
        /// <param name="sigma">Sigma</param>
        /// <param name="rnd">Random number object</param>
        /// <returns>A normally distributed random number</returns>
        private double normalDist(double mean, double sigma, Random rnd)
        {
            int size = 12;
            int precision = 1000000;
            double Z = 0;
            double Q = 0;
            double[] U = new double[size];
            for (int i = 0; i < size; i++)
            {
                U[i] = (double)rnd.Next(precision) / precision;
            }
            for (int i = 0; i < size; i++)
            {
                Z = Z + U[i];
            }
            Z = Z - size / 2;
            Q = mean + sigma * Z;
            return Q;
        }

        /// <summary>
        /// Uniformly distributed random number generator with parameter minval and maxval
        /// rnd parameter is used to create different random numbers with different seeds
        /// </summary>
        /// <param name="minval">Lower bound</param>
        /// <param name="maxval">Upper bound</param>
        /// <param name="rnd">Random number object</param>
        /// <returns>A uniformly distributed random number</returns>
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

        /// <summary>
        /// Write each solution (each child) to a log file
        /// </summary>
        /// <param name="p_logfile">Location of the log file</param>
        /// <param name="p_esvar">ES solution</param>
        private void writeLog(string p_logfile, esvariable p_esvar)
        {
            csvexport myexcel = new csvexport();
            myexcel.addRow();
            if (designclass != "All")
            {
                myexcel["Cost"] = "";
                myexcel["Locs"] = "";
                myexcel["SR"] = "";
                myexcel["Width"] = p_esvar.getWarehouse().getWidth().ToString();
                myexcel["Depth"] = p_esvar.getWarehouse().getDepth().ToString();
                myexcel["Area"] = p_esvar.getWarehouse().area.ToString();
                myexcel["Ratio"] = p_esvar.getx()[0].ToString();
                myexcel["E1"] = p_esvar.getx()[1].ToString();
                myexcel["E2"] = p_esvar.getx()[2].ToString();
                myexcel["E3"] = p_esvar.getx()[3].ToString();
                myexcel["E4"] = p_esvar.getx()[4].ToString();
                myexcel["I1X"] = p_esvar.getx()[5].ToString();
                myexcel["I1Y"] = p_esvar.getx()[6].ToString();
                myexcel["PD"] = p_esvar.getx()[7].ToString();
                myexcel["Angle1"] = (180 * p_esvar.getx()[8]).ToString();
                myexcel["Angle2"] = (180 * p_esvar.getx()[9]).ToString();
                myexcel["Angle3"] = (180 * p_esvar.getx()[10]).ToString();
                myexcel["Angle4"] = (180 * p_esvar.getx()[11]).ToString();
                myexcel["Angle5"] = (180 * p_esvar.getx()[12]).ToString();
                myexcel["Angle6"] = (180 * p_esvar.getx()[13]).ToString();
                myexcel["Adjuster1"] = p_esvar.getx()[14].ToString();
                myexcel["Adjuster2"] = p_esvar.getx()[15].ToString();
                myexcel["Adjuster3"] = p_esvar.getx()[16].ToString();
                myexcel["Adjuster4"] = p_esvar.getx()[17].ToString();
                myexcel["Adjuster5"] = p_esvar.getx()[18].ToString();
                myexcel["Adjuster6"] = p_esvar.getx()[19].ToString();
                myexcel["PickAdjuster1"] = p_esvar.getx()[20].ToString();
                myexcel["PickAdjuster2"] = p_esvar.getx()[21].ToString();
                myexcel["PickAdjuster3"] = p_esvar.getx()[22].ToString();
                myexcel["PickAdjuster4"] = p_esvar.getx()[23].ToString();
                myexcel["PickAdjuster5"] = p_esvar.getx()[24].ToString();
                myexcel["PickAdjuster6"] = p_esvar.getx()[25].ToString();
                myexcel.appendToFile(p_logfile);
            }
            else
            {
                myexcel["Cost"] = "";
                myexcel["Locs"] = "";
                myexcel["SR"] = "";
                myexcel["Width"] = p_esvar.getWarehouse().getWidth().ToString();
                myexcel["Depth"] = p_esvar.getWarehouse().getDepth().ToString();
                myexcel["Area"] = p_esvar.getWarehouse().area.ToString();
                myexcel["Ratio"] = p_esvar.getx()[0].ToString();
                myexcel["E1"] = p_esvar.getx()[1].ToString();
                myexcel["E2"] = p_esvar.getx()[2].ToString();
                myexcel["E3"] = p_esvar.getx()[3].ToString();
                myexcel["E4"] = p_esvar.getx()[4].ToString();
                myexcel["I1X"] = p_esvar.getx()[5].ToString();
                myexcel["I1Y"] = p_esvar.getx()[6].ToString();
                myexcel["PD"] = p_esvar.getx()[7].ToString();
                myexcel["Angle1"] = (180 * p_esvar.getx()[8]).ToString();
                myexcel["Angle2"] = (180 * p_esvar.getx()[9]).ToString();
                myexcel["Angle3"] = (180 * p_esvar.getx()[10]).ToString();
                myexcel["Angle4"] = (180 * p_esvar.getx()[11]).ToString();
                myexcel["Angle5"] = (180 * p_esvar.getx()[12]).ToString();
                myexcel["Angle6"] = (180 * p_esvar.getx()[13]).ToString();
                myexcel["Angle7"] = (180 * p_esvar.getx()[14]).ToString();
                myexcel["Angle8"] = (180 * p_esvar.getx()[15]).ToString();
                myexcel["Adjuster1"] = p_esvar.getx()[16].ToString();
                myexcel["Adjuster2"] = p_esvar.getx()[17].ToString();
                myexcel["Adjuster3"] = p_esvar.getx()[18].ToString();
                myexcel["Adjuster4"] = p_esvar.getx()[19].ToString();
                myexcel["Adjuster5"] = p_esvar.getx()[20].ToString();
                myexcel["Adjuster6"] = p_esvar.getx()[21].ToString();
                myexcel["Adjuster7"] = p_esvar.getx()[22].ToString();
                myexcel["Adjuster8"] = p_esvar.getx()[23].ToString();
                myexcel["PickAdjuster1"] = p_esvar.getx()[24].ToString();
                myexcel["PickAdjuster2"] = p_esvar.getx()[25].ToString();
                myexcel["PickAdjuster3"] = p_esvar.getx()[26].ToString();
                myexcel["PickAdjuster4"] = p_esvar.getx()[27].ToString();
                myexcel["PickAdjuster5"] = p_esvar.getx()[28].ToString();
                myexcel["PickAdjuster6"] = p_esvar.getx()[29].ToString();
                myexcel["PickAdjuster7"] = p_esvar.getx()[30].ToString();
                myexcel["PickAdjuster8"] = p_esvar.getx()[31].ToString();
                myexcel["PC12"] = p_esvar.getx()[32].ToString();
                myexcel["PC13"] = p_esvar.getx()[33].ToString();
                myexcel["PC14"] = p_esvar.getx()[34].ToString();
                myexcel["PC15"] = p_esvar.getx()[35].ToString();
                myexcel["PC23"] = p_esvar.getx()[36].ToString();
                myexcel["PC24"] = p_esvar.getx()[37].ToString();
                myexcel["PC25"] = p_esvar.getx()[38].ToString();
                myexcel["PC34"] = p_esvar.getx()[39].ToString();
                myexcel["PC35"] = p_esvar.getx()[40].ToString();
                myexcel["PC45"] = p_esvar.getx()[41].ToString();
                myexcel["C12"] = p_esvar.getx()[42].ToString();
                myexcel["C13"] = p_esvar.getx()[43].ToString();
                myexcel["C14"] = p_esvar.getx()[44].ToString();
                myexcel["C15"] = p_esvar.getx()[45].ToString();
                myexcel["C23"] = p_esvar.getx()[46].ToString();
                myexcel["C24"] = p_esvar.getx()[47].ToString();
                myexcel["C25"] = p_esvar.getx()[48].ToString();
                myexcel["C34"] = p_esvar.getx()[49].ToString();
                myexcel["C35"] = p_esvar.getx()[50].ToString();
                myexcel["C45"] = p_esvar.getx()[51].ToString();
                myexcel.appendToFile(p_logfile);
            }
        }
    }
}
