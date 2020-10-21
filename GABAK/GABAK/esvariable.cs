//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace GABAK
{
    /// <summary>
    /// Esvariable class holds the solution
    /// It has set and get methods to set and get variables for a solution
    /// </summary>
    class esvariable
    {
        private int numberofparameters;
        private double[] x;//x vector records the current position of variables
        private double cost;//cost value of each variable
        private double[] u;//upper limit for each variable
        private double[] l;//lower limit for each variable
        private warehouse wh;//warehouse object used for calculation of the cost
        private string designclass;//design class for a warehouse
        private bool infeasibledesign;//true if the design/solution is infeasible
        
        /// <summary>
        /// Constructor for a solution it automatically calculates the feasibility of the solution
        /// If it is feasible, it calculates the cost, if it is infeasible it sets the infeasibledesign variable to true
        /// </summary>
        /// <param name="p_numberofparameters">Total number of parameters for a solution</param>
        /// <param name="p_x">Solution array</param>
        /// <param name="p_l">Lower bound array</param>
        /// <param name="p_u">Upper bound array</param>
        /// <param name="p_wh">Warehouse object that is used for compututation for solution variables</param>
        /// <param name="p_o">Number of orders being calculated, this number can be lower than total number of orders (this is used for sampling)</param>
        /// <param name="p_computing">0 is Parallel, 1 is Distributed, 2 is Single Thread Computing</param>
        /// <param name="p_optimal">True use Concorde, False use LKH</param>
        /// <param name="p_socketservers">List of servers for distributed computing</param>
        /// <param name="p_designclass">Search All design classes or a specific design class</param>
        public esvariable(int p_numberofparameters, double[] p_x, double[] p_l, double[] p_u, warehouse p_wh, int p_o, int p_computing, bool p_optimal, socketservers p_socketservers, string p_designclass)
        {
            numberofparameters = p_numberofparameters;
            designclass = p_designclass;
            x = new double[numberofparameters];
            u = new double[p_u.Count()];
            l = new double[p_l.Count()];
            u = p_u;
            l = p_l;
            bool warehousefit = false;
            bool finalize = false;
            infeasibledesign = false;
            setx(p_x);
            wh = new warehouse();
            wh.aspectratio = x[0];
            wh.setArea(p_wh.area);
            wh.usevisibilitygraph = p_wh.usevisibilitygraph;
            wh.pickersize = p_wh.pickersize;
            
            //Adjust warehouse area until warehouse is fit (minimum number of empty locations with given aspect ratio)
            int increased = 0;
            int decreased = 0;
            do
            {
                wh.resetNetwork();
                //Set warehouse aspect ratio
                wh.aspectratio = x[0];
                //Set warehouse cross aisle width
                wh.crossaislewidth = p_wh.crossaislewidth;
                //Set warehouse picking aisle width
                wh.pickingaislewidth = p_wh.pickingaislewidth;
                //Set warehouse picking location width
                wh.pickinglocationwidth = p_wh.pickinglocationwidth;
                //Set warehouse picking location depth
                wh.pickinglocationdepth = p_wh.pickinglocationdepth;
                //Set SKUs for the warehouse object
                wh.setSKUs(p_wh.getSKUs());
                //Set average number of picks per pick tour
                wh.avgTourLength = p_wh.avgTourLength;
                
                if (designclass != "All")//Perform search on a specific design class
                {
                    double[] angle = { 180 * x[8], 180 * x[9], 180 * x[10], 180 * x[11], 180 * x[12], 180 * x[13] };
                    double[] adjuster = { x[14], x[15], x[16], x[17], x[18], x[19] };
                    double[] pickadjuster = { x[20], x[21], x[22], x[23], x[24], x[25] };
                    double[] ext = { x[1], x[2], x[3], x[4], x[5], x[6] };
                    double[] intx = { x[5] };
                    double[] inty = { x[6] };
                    double[] pd = { x[7] };
                    double aspectratio = x[0];
                    
                    switch (designclass)
                    {
                        case "0-0-0":
                            if (!wh.create000Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, wh.crossaislewidth, wh.pickingaislewidth, wh.pickinglocationwidth, wh.pickinglocationdepth))
                            {
                                cost = Double.MaxValue;//This means infeasible design
                                infeasibledesign = true;
                                return;
                            }
                            break;
                        case "2-0-1":
                            if (!wh.create201Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, wh.crossaislewidth, wh.pickingaislewidth, wh.pickinglocationwidth, wh.pickinglocationdepth))
                            {
                                cost = Double.MaxValue;
                                infeasibledesign = true;
                                return;
                            }
                            break;
                        case "3-0-2":
                            if (!wh.create302Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, wh.crossaislewidth, wh.pickingaislewidth, wh.pickinglocationwidth, wh.pickinglocationdepth))
                            {
                                cost = Double.MaxValue;
                                infeasibledesign = true;
                                return;
                            }
                            break;
                        case "3-0-3":
                            if (!wh.create303Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, wh.crossaislewidth, wh.pickingaislewidth, wh.pickinglocationwidth, wh.pickinglocationdepth))
                            {
                                cost = Double.MaxValue;
                                infeasibledesign = true;
                                return;
                            }
                            break;
                        case "3-1-3":
                            if (!wh.create313Warehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, wh.crossaislewidth, wh.pickingaislewidth, wh.pickinglocationwidth, wh.pickinglocationdepth))
                            {
                                cost = Double.MaxValue;
                                infeasibledesign = true;
                                return;
                            }
                            break;
                        case "4-0-2":
                            if (!wh.create402Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, wh.crossaislewidth, wh.pickingaislewidth, wh.pickinglocationwidth, wh.pickinglocationdepth))
                            {
                                cost = Double.MaxValue;
                                infeasibledesign = true;
                                return;
                            }
                            break;
                        case "4-0-4":
                            if (!wh.create404Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, wh.crossaislewidth, wh.pickingaislewidth, wh.pickinglocationwidth, wh.pickinglocationdepth))
                            {
                                cost = Double.MaxValue;
                                infeasibledesign = true;
                                return;
                            }
                            break;
                        case "4-0-5":
                            if (!wh.create405Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, wh.crossaislewidth, wh.pickingaislewidth, wh.pickinglocationwidth, wh.pickinglocationdepth))
                            {
                                cost = Double.MaxValue;
                                infeasibledesign = true;
                                return;
                            }
                            break;
                        case "4-1-4":
                            if (!wh.create414Warehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, wh.crossaislewidth, wh.pickingaislewidth, wh.pickinglocationwidth, wh.pickinglocationdepth))
                            {
                                cost = Double.MaxValue;
                                infeasibledesign = true;
                                return;
                            }
                            break;
                        case "4-2-5":
                            if (!wh.create425Warehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, wh.crossaislewidth, wh.pickingaislewidth, wh.pickinglocationwidth, wh.pickinglocationdepth))
                            {
                                cost = Double.MaxValue;
                                infeasibledesign = true;
                                return;
                            }
                            break;
                        case "6-0-3":
                            if (!wh.create603Warehouse(angle, adjuster, pickadjuster, ext, pd, aspectratio, wh.crossaislewidth, wh.pickingaislewidth, wh.pickinglocationwidth, wh.pickinglocationdepth))
                            {
                                cost = Double.MaxValue;
                                infeasibledesign = true;
                                return;
                            }
                            break;
                    }
                }
                else//Perform search on all design classes
                {
                    double[] angle = { 180 * x[8], 180 * x[9], 180 * x[10], 180 * x[11], 180 * x[12], 180 * x[13], 180 * x[14], 180 * x[15] };
                    double[] adjuster = { x[16], x[17], x[18], x[19], x[20], x[21], x[22], x[23] };
                    double[] pickadjuster = { x[24], x[25], x[26], x[27], x[28], x[29], x[30], x[31] };
                    double[] ext = { x[1], x[2], x[3], x[4] };
                    double[] intx = { x[5] };
                    double[] inty = { x[6] };
                    double[] pd = { x[7] };
                    double aspectratio = x[0];

                    bool[] connections = new bool[this.numberofparameters - l.Count()];
                    int m = 0;
                    for (int j = l.Count(); j < this.numberofparameters; j++)
                    {
                        int indexencodingprobability = j - (this.numberofparameters - l.Count());
                        if (x[j] == 0)//If that cross aisle exists then select that connection correct
                        {
                            connections[m] = false;
                        }
                        else
                        {
                            connections[m] = true;
                        }
                        m++;
                    }
                    
                    if (!wh.createWarehouse(angle, adjuster, pickadjuster, ext, intx, inty, pd, aspectratio, wh.crossaislewidth, wh.pickingaislewidth, wh.pickinglocationwidth, wh.pickinglocationdepth, connections))
                    {
                        cost = Double.MaxValue;
                        infeasibledesign = true;
                        return;
                    }
                }
                
                //Check warehouse size here before doing any other calculations and if size is not fit then adjust it approprately
                int totalstoragelocations = wh.totalNumberOfLocations();
                if (increased > 0 && decreased > 0)//No exact number of locations but this is the smallest it can get then we stop
                {
                    if (wh.getSKUs().Count > totalstoragelocations)//This check is necessary because last iteration it could have been decreased
                    {
                        wh.setArea(wh.area / options.warehouseadjustmentfactor);//Increase area
                        increased++;
                        finalize = true;
                    }
                    else if (wh.getSKUs().Count < totalstoragelocations || finalize == true)
                    {
                        warehousefit = true;
                        break;
                    }
                }
                if (wh.getSKUs().Count > totalstoragelocations)
                {
                    wh.setArea(wh.area / options.warehouseadjustmentfactor);//Increase area
                    increased++;
                }
                else if (wh.getSKUs().Count < totalstoragelocations)
                {
                    wh.setArea(wh.area * options.warehouseadjustmentfactor);//Decrease area
                    decreased++;
                }
                else if (wh.getSKUs().Count == totalstoragelocations)
                {
                    warehousefit = true;
                }
            } while (!warehousefit);
            
            if (!wh.usevisibilitygraph)//aisle centers method
            {
                wh.createImportantNodeShortestDistances();
                wh.locationShortestDistance();
            }
            else//visibility graph method
            {
                wh.createPolygonsandGraphNodes();
                wh.createVisibilityGraph();
                wh.fillGraphNodeDistances();
            }
            wh.pdTotalDistances();
            wh.totalDistances();
            wh.rankLocations(wh.avgTourLength);
            wh.colorOverall();
            //Use turnover based allocation method
            allocation al = new allocation(0);
            //If allocateSKUs return -1 then there is insufficient warehouse space, the design is infeasible, return positive infinite as a cost (death penalty)
            if (al.allocateSKUs(wh.getSKUs(), wh, 0) < 0) { cost = Double.MaxValue; infeasibledesign = true; return; };
            //Otherwise continue the calculations
            wh.setOrders(p_wh.getOrders());
            if (!wh.usevisibilitygraph)//Aisle centers method
            {
                if (p_computing == 0)//Parallel computing
                {
                    var sums = new ConcurrentBag<double>();
                    Parallel.For(0, p_o, k =>
                    {
                        warehouse tmpwh = wh;
                        List<order> tmporders = wh.getOrders();
                        routing rt = new routing();
                        double tourcost = 0;
                        bool LKHdoneonce = false;
                        while (tourcost == 0)
                        {
                            if (p_optimal || LKHdoneonce)
                            {
                                tourcost = rt.tspOptSteiner(tmpwh, tmporders[k], k);
                                if (tourcost == 0) break;//This means that tour cost is really zero (sometimes LKH returns 0 even though it should not)
                            }
                            else
                            {
                                tourcost = rt.tspLKHSteiner(tmpwh, tmporders[k], k);
                                LKHdoneonce = true;
                            }
                        }
                        sums.Add(tourcost);
                    });
                    cost = sums.Sum() / p_o;
                }
                else if(p_computing == 1)//Distributed computing
                {
                    warehouse tmpwh = wh;
                    routing rt = new routing();
                    cost = rt.tspOptNetSteiner(tmpwh, p_o, p_socketservers, 0);
                }
            }
            else//Visibility graph method
            {
                if (p_computing == 0)//Parallel computing
                {
                    var sums = new ConcurrentBag<double>();
                    Parallel.For(0, p_o, k =>
                    {
                        warehouse tmpwh = wh;
                        List<order> tmporders = wh.getOrders();
                        routing rt = new routing();
                        double tourcost = 0;
                        bool LKHdoneonce = false;
                        while (tourcost == 0)
                        {
                            if (p_optimal || LKHdoneonce)
                            {
                                tourcost = rt.tspOptVisibility(tmpwh, tmporders[k], k);
                                if (tourcost == 0) break;//This means that tour cost is really zero (sometimes LKH returns 0 even though it should not)
                            }
                            else
                            {
                                tourcost = rt.tspLKHVisibility(tmpwh, tmporders[k], k);
                                LKHdoneonce = true;
                            }
                        }
                        sums.Add(tourcost);
                    });
                    cost = sums.Sum() / p_o;
                }
                else if(p_computing == 1)
                {
                    warehouse tmpwh = wh;
                    routing rt = new routing();
                    cost = rt.tspOptNetVisibility(tmpwh, p_o, p_socketservers, 0);
                }
            }
        }

        /// <summary>
        /// Sets X variable and performs repairs if x is out of bounds
        /// </summary>
        /// <param name="p_x">X array that holds the solution</param>
        private void setx(double[] p_x)
        {
            for (int i = 0; i < numberofparameters; i++)
            {
                if (i < l.Count() && p_x[i] < l[i])//If x is lower than its lower bound
                {
                    x[i] = l[i] + 0.1 * (u[i] - l[i]);//Repair operation moves to 10 percent of the range
                }
                else if (i < u.Count() && p_x[i] > u[i])//If x is greater than its upper bound
                {
                    x[i] = u[i] - 0.1 * (u[i] - l[i]);//Repair operation moves to 90 percent of the range
                }
                else//Else x is inside the range so it sets x as p_x
                {
                    x[i] = p_x[i];
                }
            }
        }

        /// <summary>
        /// Get x solution
        /// </summary>
        /// <returns>X array</returns>
        public double[] getx()
        {
            return x;
        }

        /// <summary>
        /// Get cost of the solution without NFT
        /// </summary>
        /// <returns>Cost of the solution without NFT</returns>
        public double getCost()
        {
            return cost;
        }

        /// <summary>
        /// Get cost of the solution with our implementation of Near Feasibility Threshold
        /// </summary>
        /// <param name="p_i">Iteration number</param>
        /// <param name="p_nft0">Initial nft</param>
        /// <param name="p_nftlambda">NFT lambda</param>
        /// <returns></returns>
        public double getCost(int p_i, double p_nft0, double p_nftlambda)
        {
            //Calculate the difference between total number of storage locations and number of SKUs
            double totalviolations = wh.totalNumberOfLocations() - wh.getSKUs().Count;
            //Calculate NFT based on the NFT Formula by Coit (1996) paper
            double nft = p_nft0 / (1.0 + p_nftlambda * p_i);
            //We use the modified version of penalty function calculation by Tasgetiren (2006) "A multi-populated differential evolution algorithm for solving constrained optimization problem"
            //We set the severity parameter as 2
            double penalty = Math.Pow(totalviolations / nft, 2);
            //Add cost to the penalty function
            return cost + penalty;
        }

        /// <summary>
        /// Get warehouse object for the solution
        /// </summary>
        /// <returns>Warehouse object</returns>
        public warehouse getWarehouse()
        {
            return wh;
        }

        /// <summary>
        /// Returns true if solution is feasible, false otherwise
        /// </summary>
        /// <returns>Returns feasibility of the solution</returns>
        public bool isInfeasible()
        {
            return infeasibledesign;
        }

        /// <summary>
        /// Uniformly distributed random number generator with parameter mean and sigma
        /// rnd parameter is used to create different random numbers with different seeds
        /// </summary>
        /// <param name="minval">Lower bound</param>
        /// <param name="maxval">Upper bound</param>
        /// <param name="rnd">Random number object</param>
        /// <returns>A uniform random number between lower and upper bounds</returns>
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
    }
}
