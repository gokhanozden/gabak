//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GABAK
{
    /// <summary>
    /// You can implement under this class different routing methods such as aisle by aisle, S-Shape, Largest-gap, Combined
    /// currently we are only using LKH and Concorde to get optimal routing
    /// </summary>
    internal class routing
    {
        /// <summary>
        /// Default constructor for routing
        /// </summary>
        public routing()
        {
        }

        /// <summary>
        /// Aisle by aisle routing algorithm (implementation not finished), does not run correctly
        /// </summary>
        /// <param name="wh">Warehouse Object</param>
        /// <param name="locations">List of locations</param>
        /// <returns></returns>
        public double aislebyaisle(warehouse wh, List<node> locations)
        {
            //Find closest (to the pd point) location's region

            List<node> orderedlocations = new List<node>();
            for (int i = 0; i < wh.regions.Count; i++)
                for (int j = 0; j < wh.regions[i].pickingaisleedges.Count(); j++)
                    for (int k = 0; k < wh.regions[i].pickingaisleedges[j].getOnEdgeNodes().Count(); k++)
                        for (int l = 0; l < locations.Count(); l++)
                        {
                            if (locations[l].id == wh.regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].id)
                            {
                                orderedlocations.Add(locations[l]);
                            }
                        }
            double totaltourdistance = 0;
            for (int i = 0; i < orderedlocations.Count() - 1; i++)
            {
                totaltourdistance = totaltourdistance + wh.shortestPathDistanceTwoLocationsSteiner(orderedlocations[i], orderedlocations[i + 1]);
            }
            totaltourdistance = totaltourdistance + wh.shortestPathDistanceTwoLocationsSteiner(wh.pdnodes[0], orderedlocations[0]);
            totaltourdistance = totaltourdistance + wh.shortestPathDistanceTwoLocationsSteiner(wh.pdnodes[0], orderedlocations[orderedlocations.Count - 1]);
            return totaltourdistance;
        }

        /// <summary>
        /// Solves routing of an order in a warehouse by using aisle centers method and HeldKarp(Optimal)
        /// </summary>
        /// <param name="p_wh">Warehouse object</param>
        /// <param name="p_order">Order object</param>
        /// <param name="p_k">Index for order list</param>
        /// <returns></returns>
        public double tspHeldKarpSteiner(warehouse p_wh, order p_order, int p_k)
        {
            List<node> tourlocations = new List<node>();
            //LKH and Concorde only solves for integer distances, therefore we need to convert double distances into integer
            //m = 10 will keep only one decimal such as 10.1, m = 100 will keep two decimals such as 10.01
            //We kept it m = 10 because we think it gives enough precision for our results (up to 0.1 ft or 0.1 m)
            double m = 10;
            tourlocations.Add(p_wh.pdnodes[0]);//Each tour should have a PD point
            //Find the pick location of each SKU in the order in a warehouse
            //Do not double count the pick location if two SKUs are residing in the same location, this basically eliminates duplicate pick locations
            for (int i = 0; i < p_order.getOrderSize(); i++)
            {
                if (!tourlocations.Contains(p_order.getOrderSkus()[i].location))
                {
                    tourlocations.Add(p_order.getOrderSkus()[i].location);
                }
            }
            int dimension = tourlocations.Count;//Size of the TSP problem
            //If dimension is 2 or 3 then LKH or Concorde cannot solve the problem, we have to find optimal routes for these two dimensions manually inside our code
            if (dimension == 2)
            {
                return 2 * Convert.ToDouble(Convert.ToInt32(p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[1])) * m) / m;
            }
            if (dimension == 3)
            {
                double dist1 = p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[1], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[2], tourlocations[0]);
                double dist2 = p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[2], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[1], tourlocations[0]);

                if (dist1 < dist2)
                {
                    return Convert.ToDouble(Convert.ToInt32(dist1 * m)) / m;
                }
                else
                {
                    return Convert.ToDouble(Convert.ToInt32(dist2 * m)) / m;
                }
            }
            int[,] distanceMatrix = new int[dimension,dimension];
            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j < dimension; j++)
                {
                    distanceMatrix[i,j] = Convert.ToInt32(m * p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[i], tourlocations[j]));
                }
            }

            //Initalize the heldkarp object
            heldkarp heldkarpsolver = new heldkarp(distanceMatrix);
            return Convert.ToDouble(heldkarpsolver.SolveTSP()) / m;
        }

        /// <summary>
        /// Solves routing of an order in a warehouse by using visibility graph method and HeldKarp(Optimal)
        /// </summary>
        /// <param name="p_wh">Warehouse object</param>
        /// <param name="p_order">Order object</param>
        /// <param name="p_k">Index for order list</param>
        /// <returns></returns>
        public double tspHeldKarpVisibility(warehouse p_wh, order p_order, int p_k)
        {

            return 0;
        }

        private double heldKarpTourVisibilityGraphRecursive(node startNode, List<node> remainingNodes, warehouse p_wh)
        {
            if (remainingNodes.Count == 0)
            {
                return p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(p_wh.pdnodes[0], startNode);
            }
            else
            {
                double minValueFound = double.MaxValue;
                for (int i = 0; i < remainingNodes.Count; i++)
                {

                    node node = remainingNodes[i];
                    remainingNodes.Remove(node);
                    double currentCost = p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(startNode, remainingNodes[i]) + heldKarpTourVisibilityGraphRecursive(remainingNodes[i], remainingNodes, p_wh);
                    if (currentCost < minValueFound)
                    {
                        minValueFound = currentCost;
                    }
                    remainingNodes.Add(node);
                }
                return minValueFound;
            }
        }

        /// <summary>
        /// Solves routing of an order in a warehouse by using aisle centers method and 2OPT
        /// </summary>
        /// <param name="p_wh">Warehouse object</param>
        /// <param name="p_order">Order object</param>
        /// <param name="p_k">Index for order list</param>
        /// <returns></returns>
        public double tsp2OPTSteiner(warehouse p_wh, order p_order, int p_k)
        {
            List<node> tourlocations = new List<node>();
            List<node> temptourlocations = new List<node>();
            //LKH and Concorde only solves for integer distances, therefore we need to convert double distances into integer
            //m = 10 will keep only one decimal such as 10.1, m = 100 will keep two decimals such as 10.01
            //We kept it m = 10 because we think it gives enough precision for our results (up to 0.1 ft or 0.1 m)
            double m = 10;
            tourlocations.Add(p_wh.pdnodes[0]);//Each tour should have a PD point
            //Find the pick location of each SKU in the order in a warehouse
            //Do not double count the pick location if two SKUs are residing in the same location, this basically eliminates duplicate pick locations
            for (int i = 0; i < p_order.getOrderSize(); i++)
            {
                if (!tourlocations.Contains(p_order.getOrderSkus()[i].location))
                {
                    tourlocations.Add(p_order.getOrderSkus()[i].location);
                }
            }
            int dimension = tourlocations.Count;//Size of the TSP problem
            //If dimension is 2 or 3 then LKH or Concorde cannot solve the problem, we have to find optimal routes for these two dimensions manually inside our code
            if (dimension == 2)
            {
                return 2 * Convert.ToDouble(Convert.ToInt32(p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[1])) * m) / m;
            }
            if (dimension == 3)
            {
                double dist1 = p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[1], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[2], tourlocations[0]);
                double dist2 = p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[2], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[1], tourlocations[0]);

                if (dist1 < dist2)
                {
                    return Convert.ToDouble(Convert.ToInt32(dist1 * m)) / m;
                }
                else
                {
                    return Convert.ToDouble(Convert.ToInt32(dist2 * m)) / m;
                }
            }
            //The below code will only run when dimension > 3
            //Tour array can be used to keep the tour, currently we are only interested in tour cost for optimization purposes
            //Tours might be useful if we want to see tour details, currently we don't have an implementation for showing the actual tour
            int bestdistance = Convert.ToInt32(tourCostSteiner(p_wh, tourlocations));
            int initialbestdistance = bestdistance;
            int mini;
            int minj;
            do
            {
                bestdistance = Convert.ToInt32(tourCostSteiner(p_wh, tourlocations));
                initialbestdistance = bestdistance;
                mini = 0;
                minj = 0;
                for (int i = 1; i < dimension - 1; i++)
                {
                    for (int k = i + 1; k < dimension; k++)
                    {
                        temptourlocations = TwoOptSwap(i, k, tourlocations);
                        int distance = Convert.ToInt32(tourCostSteiner(p_wh, temptourlocations));
                        if (bestdistance > distance)
                        {
                            bestdistance = distance;
                            mini = i;
                            minj = k;
                        }
                    }
                }
                //apply mini/minj move
                node tempnode;
                tempnode = tourlocations[mini];
                tourlocations[mini] = tourlocations[minj];
                tourlocations[minj] = tempnode;
            }
            while (bestdistance - initialbestdistance < 0);
            return tourCostSteiner(p_wh, tourlocations) / m;
        }

        /// <summary>
        /// Solves routing of an order in a warehouse by using visibility graph method and 2OPT
        /// </summary>
        /// <param name="p_wh">Warehouse object</param>
        /// <param name="p_order">Order object</param>
        /// <param name="p_k">Index for order list</param>
        /// <returns></returns>
        public double tsp2OPTVisibility(warehouse p_wh, order p_order, int p_k)
        {
            List<node> tourlocations = new List<node>();
            List<node> temptourlocations = new List<node>();
            //LKH and Concorde only solves for integer distances, therefore we need to convert double distances into integer
            //m = 10 will keep only one decimal such as 10.1, m = 100 will keep two decimals such as 10.01
            //We kept it m = 10 because we think it gives enough precision for our results (up to 0.1 ft or 0.1 m)
            double m = 10;
            tourlocations.Add(p_wh.pdnodes[0]);//Each tour should have a PD point
            //Find the pick location of each SKU in the order in a warehouse
            //Do not double count the pick location if two SKUs are residing in the same location, this basically eliminates duplicate pick locations
            for (int i = 0; i < p_order.getOrderSize(); i++)
            {
                if (!tourlocations.Contains(p_order.getOrderSkus()[i].location))
                {
                    tourlocations.Add(p_order.getOrderSkus()[i].location);
                }
            }
            int dimension = tourlocations.Count;//Size of the TSP problem
            //If dimension is 2 or 3 then LKH or Concorde cannot solve the problem, we have to find optimal routes for these two dimensions manually inside our code
            if (dimension == 2)
            {
                return 2 * Convert.ToDouble(Convert.ToInt32(p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[0], tourlocations[1])) * m) / m;
            }
            if (dimension == 3)
            {
                double dist1 = p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[0], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[1], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[2], tourlocations[0]);
                double dist2 = p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[0], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[2], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[1], tourlocations[0]);

                if (dist1 < dist2)
                {
                    return Convert.ToDouble(Convert.ToInt32(dist1 * m)) / m;
                }
                else
                {
                    return Convert.ToDouble(Convert.ToInt32(dist2 * m)) / m;
                }
            }
            //The below code will only run when dimension > 3
            //Tour array can be used to keep the tour, currently we are only interested in tour cost for optimization purposes
            //Tours might be useful if we want to see tour details, currently we don't have an implementation for showing the actual tour
            double bestdistance = tourCostVisibility(p_wh, tourlocations) / m;
            double initialbestdistance = bestdistance;
            int mini;
            int minj;
            do
            {
                bestdistance = tourCostVisibility(p_wh, tourlocations) / m;
                initialbestdistance = bestdistance;
                mini = 0;
                minj = 0;
                for (int i = 1; i < dimension - 1; i++)
                {
                    for (int k = i + 1; k < dimension; k++)
                    {
                        temptourlocations = TwoOptSwap(i, k, tourlocations);
                        double distance = tourCostVisibility(p_wh, temptourlocations) / m;
                        if (bestdistance > distance)
                        {
                            bestdistance = distance;
                            mini = i;
                            minj = k;
                        }
                    }
                }
                //apply mini/minj move
                node tempnode;
                tempnode = tourlocations[mini];
                tourlocations[mini] = tourlocations[minj];
                tourlocations[minj] = tempnode;
            }
            while (bestdistance - initialbestdistance < 0);
            return tourCostVisibility(p_wh, tourlocations) / m;
        }

        private List<node> TwoOptSwap(int p_i, int p_k, List<node> p_tourlocations)
        {
            List<node> newtourlocations = new List<node>();
            // 1. take route[0] to route[i-1] and add them in order to new_route
            for (int c = 0; c <= p_i - 1; c++)
            {
                newtourlocations.Add(p_tourlocations[c]);
            }

            // 2. take route[i] to route[k] and add them in reverse order to new_route
            int dec = 0;
            for (int c = p_i; c <= p_k; c++)
            {
                newtourlocations.Add(p_tourlocations[p_k - dec]);
                dec++;
            }

            // 3. take route[k+1] to end and add them in order to new_route
            for (int c = p_k + 1; c < p_tourlocations.Count(); c++)
            {
                newtourlocations.Add(p_tourlocations[c]);
            }
            return newtourlocations;
        }


        /// <summary>
        /// Returns the tour cost when using aisle centers method
        /// </summary>
        /// <param name="p_wh">Warehouse object</param>
        /// <param name="p_tour">Tour object</param>
        /// <returns>Travel distance</returns>
        private int tourCostSteiner(warehouse p_wh, List<node> p_tour)
        {
            double m = 10;
            int tourcost = 0;
            for (int i = 0; i < p_tour.Count - 1; i++)
            {
                tourcost += Convert.ToInt32(m * p_wh.shortestPathDistanceTwoLocationsSteiner(p_tour[i], p_tour[i + 1]));
            }
            tourcost += Convert.ToInt32(m * p_wh.shortestPathDistanceTwoLocationsSteiner(p_tour[p_tour.Count - 1], p_tour[0]));//returning back to starting city
            return tourcost;
        }

        /// <summary>
        /// Returns the tour cost when using visibility graph method
        /// </summary>
        /// <param name="p_wh">Warehouse object</param>
        /// <param name="p_tour">Tour object</param>
        /// <returns>Travel distance</returns>
        private double tourCostVisibility(warehouse p_wh, List<node> p_tour)
        {
            //LKH and Concorde only solves for integer distances, therefore we need to convert double distances into integer
            //m = 10 will keep only one decimal such as 10.1, m = 100 will keep two decimals such as 10.01
            //We kept it m = 10 because we think it gives enough precision for our results (up to 0.1 ft or 0.1 m)
            double m = 10;
            double tourcost = 0;
            for (int i = 0; i < p_tour.Count - 1; i++)
            {
                tourcost += m * p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(p_tour[i], p_tour[i + 1]);
            }
            tourcost += m * p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(p_tour[p_tour.Count - 1], p_tour[0]);//returning back to starting city
            return tourcost;
        }

        /// <summary>
        /// Solves routing of an order in a warehouse by using aisle centers method and LKH
        /// </summary>
        /// <param name="p_wh">Warehouse object</param>
        /// <param name="p_order">Order object</param>
        /// <param name="p_k">Index for order list</param>
        /// <returns></returns>
        public double tspLKHSteiner(warehouse p_wh, order p_order, int p_k)
        {
            List<node> tourlocations = new List<node>();
            //LKH and Concorde only solves for integer distances, therefore we need to convert double distances into integer
            //m = 10 will keep only one decimal such as 10.1, m = 100 will keep two decimals such as 10.01
            //We kept it m = 10 because we think it gives enough precision for our results (up to 0.1 ft or 0.1 m)
            double m = 10;
            tourlocations.Add(p_wh.pdnodes[0]);//Each tour should have a PD point
            //Find the pick location of each SKU in the order in a warehouse
            //Do not double count the pick location if two SKUs are residing in the same location, this basically eliminates duplicate pick locations
            for (int i = 0; i < p_order.getOrderSize(); i++)
            {
                if (!tourlocations.Contains(p_order.getOrderSkus()[i].location))
                {
                    tourlocations.Add(p_order.getOrderSkus()[i].location);
                }
            }
            int dimension = tourlocations.Count;//Size of the TSP problem
            //If dimension is 2 or 3 then LKH or Concorde cannot solve the problem, we have to find optimal routes for these two dimensions manually inside our code
            if (dimension == 2)
            {
                return 2 * Convert.ToDouble(Convert.ToInt32(p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[1])) * m) / m;
            }
            if (dimension == 3)
            {
                double dist1 = p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[1], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[2], tourlocations[0]);
                double dist2 = p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[2], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[1], tourlocations[0]);

                if (dist1 < dist2)
                {
                    return Convert.ToDouble(Convert.ToInt32(dist1 * m)) / m;
                }
                else
                {
                    return Convert.ToDouble(Convert.ToInt32(dist2 * m)) / m;
                }
            }
            //The below code will only run when dimension > 3
            //Tour array can be used to keep the tour, currently we are only interested in tour cost for optimization purposes
            //Tours might be useful if we want to see tour details, currently we don't have an implementation for showing the actual tour
            int[] tour = new int[dimension];
            //For [dimension X dimension] sized distance matrix, we are only interested in lower diagonal part to create our TSP file
            int mysize = (dimension * (dimension + 1)) / 2;

            double tourcost = 0;//Initialize tourcost as zero
            int[] cost = new int[mysize];//Initialize cost array, this will keep the lower diagonal of distance matrix in an array format
            int c = 0;

            //
            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    cost[c] = Convert.ToInt32(m * p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[i], tourlocations[j]));
                    c++;
                }
            }

            System.IO.File.Delete(@"C:\gabak\tsp\" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp");
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\gabak\tsp\" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp", true))
            {
                string[] lines = { "NAME: whtsp", "TYPE: TSP", "DIMENSION: " + dimension.ToString(), "EDGE_WEIGHT_TYPE: EXPLICIT", "EDGE_WEIGHT_FORMAT: LOWER_DIAG_ROW", "EDGE_WEIGHT_SECTION" };
                for (int i = 0; i < lines.Count(); i++)
                {
                    file.WriteLine(lines[i]);
                }
                for (int i = 0; i < cost.Count(); i++)
                {
                    file.WriteLine(cost[i].ToString());
                }
                file.Write("EOF");
                file.Close();
            }

            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("C:\\gabak\\lkh.exe");
                psi.Arguments = @"C:\gabak\tsp\" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp";
                psi.CreateNoWindow = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

                System.Diagnostics.Process cmdexe = new System.Diagnostics.Process();
                cmdexe.StartInfo = psi;
                List<string> myoutput = new List<string>();

                cmdexe.Start();
                string output = cmdexe.StandardOutput.ReadToEnd();
                cmdexe.WaitForExit();
                while (!cmdexe.HasExited)
                {
                }
                string output1 = output.Substring(output.IndexOf("Cost.min = "));
                string output2 = output1.Substring(0, output1.IndexOf(","));
                string output3 = output2.Substring(11);
                tourcost = Convert.ToDouble(output3);
                cmdexe.Dispose();//This will stop memory leak (hopefully)
                System.IO.File.Delete(@"C:\gabak\tsp\" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp");
            }
            catch (Exception e)
            {
            }
            return tourcost / m;
        }

        public double tspLKHVisibility(warehouse p_wh, order p_order, int p_k)
        {
            List<node> tourlocations = new List<node>();
            double m = 10;
            bool writefile = true;
            tourlocations.Add(p_wh.pdnodes[0]);
            for (int i = 0; i < p_order.getOrderSize(); i++)
            {
                if (!tourlocations.Contains(p_order.getOrderSkus()[i].location))
                {
                    tourlocations.Add(p_order.getOrderSkus()[i].location);
                }
            }
            int dimension = tourlocations.Count;
            if (dimension == 2)
            {
                return 2 * Convert.ToDouble(Convert.ToInt32(p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[0], tourlocations[1])) * m) / m;
            }
            if (dimension == 3)
            {
                double dist1 = p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[0], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[1], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[2], tourlocations[0]);
                double dist2 = p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[0], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[2], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[1], tourlocations[0]);

                if (dist1 < dist2)
                {
                    return Convert.ToDouble(Convert.ToInt32(dist1 * m)) / m;
                }
                else
                {
                    return Convert.ToDouble(Convert.ToInt32(dist2 * m)) / m;
                }
            }
            int[] tour = new int[dimension];
            int mysize = (dimension * (dimension + 1)) / 2;

            double tourcost = 0;
            int[] cost = new int[mysize];
            int c = 0;

            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    cost[c] = Convert.ToInt32(m * p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[i], tourlocations[j]));
                    c++;
                }
            }

            if (writefile)
            {
                System.IO.File.Delete(@"C:\gabak\tsp\" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp");
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\gabak\tsp\" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp", true))
                {
                    string[] lines = { "NAME: whtsp", "TYPE: TSP", "DIMENSION: " + dimension.ToString(), "EDGE_WEIGHT_TYPE: EXPLICIT", "EDGE_WEIGHT_FORMAT: LOWER_DIAG_ROW", "EDGE_WEIGHT_SECTION" };
                    for (int i = 0; i < lines.Count(); i++)
                    {
                        file.WriteLine(lines[i]);
                    }
                    for (int i = 0; i < cost.Count(); i++)
                    {
                        file.WriteLine(cost[i].ToString());
                    }
                    file.Write("EOF");
                    file.Close();
                }
            }

            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("C:\\gabak\\lkh.exe");
                psi.Arguments = @"C:\gabak\tsp\" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp";
                psi.CreateNoWindow = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

                System.Diagnostics.Process cmdexe = new System.Diagnostics.Process();
                cmdexe.StartInfo = psi;
                List<string> myoutput = new List<string>();

                cmdexe.Start();
                string output = cmdexe.StandardOutput.ReadToEnd();
                cmdexe.WaitForExit();
                while (!cmdexe.HasExited)
                {
                }
                string output1 = output.Substring(output.IndexOf("Cost.min = "));
                string output2 = output1.Substring(0, output1.IndexOf(","));
                string output3 = output2.Substring(11);
                tourcost = Convert.ToDouble(output3);
                cmdexe.Dispose();//This will stop memory leak (hopefully)
                System.IO.File.Delete(@"C:\gabak\tsp\" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp");
            }
            catch (Exception e)
            {
            }
            return tourcost / m;
        }

        /// <summary>
        /// Solve with Distributed Computing LKH Steiner
        /// </summary>
        /// <param name="p_wh"></param>
        /// <param name="samplesize"></param>
        /// <param name="p_socketservers"></param>
        /// <param name="schedulemode">0 Order, 1 LPT</param>
        /// <returns></returns>
        public double tspLKHNetSteiner(warehouse p_wh, int samplesize, socketservers p_socketservers, int schedulemode)
        {
            var sums = new ConcurrentBag<double>();
            var unsolved = new ConcurrentBag<order>();
            double m = 10;
            if (samplesize > 0)
            {
                Parallel.For(0, samplesize, k =>
                    {
                        order p_order = p_wh.getOrders()[k];
                        List<node> tourlocations = new List<node>();

                        tourlocations.Add(p_wh.pdnodes[0]);
                        for (int i = 0; i < p_order.getOrderSize(); i++)
                        {
                            if (!tourlocations.Contains(p_order.getOrderSkus()[i].location))
                            {
                                tourlocations.Add(p_order.getOrderSkus()[i].location);
                            }
                        }
                        int dimension = tourlocations.Count;
                        if (dimension == 2)
                        {
                            sums.Add(2 * Convert.ToDouble(Convert.ToInt32(p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[1])) * m) / m);
                        }
                        if (dimension == 3)
                        {
                            double dist1 = p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[1], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[2], tourlocations[0]);
                            double dist2 = p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[2], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[1], tourlocations[0]);

                            if (dist1 < dist2)
                            {
                                sums.Add(Convert.ToDouble(Convert.ToInt32(dist1 * m)) / m);
                            }
                            else
                            {
                                sums.Add(Convert.ToDouble(Convert.ToInt32(dist2 * m)) / m);
                            }
                        }
                        int[] tour = new int[dimension];
                        int mysize = (dimension * (dimension + 1)) / 2;

                        int[] cost = new int[mysize];
                        int c = 0;

                        for (int i = 0; i < dimension; i++)
                        {
                            for (int j = 0; j <= i; j++)
                            {
                                cost[c] = Convert.ToInt32(m * p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[i], tourlocations[j]));
                                c++;
                            }
                        }
                        if (dimension > 3)
                        {
                            System.IO.File.Delete(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + p_order.getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");

                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + p_order.getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp", true))
                            {
                                string[] lines = { "NAME: whtsp", "TYPE: TSP", "DIMENSION: " + dimension.ToString(), "EDGE_WEIGHT_TYPE: EXPLICIT", "EDGE_WEIGHT_FORMAT: LOWER_DIAG_ROW", "EDGE_WEIGHT_SECTION" };
                                for (int i = 0; i < lines.Count(); i++)
                                {
                                    file.WriteLine(lines[i]);
                                }
                                for (int i = 0; i < cost.Count(); i++)
                                {
                                    file.WriteLine(cost[i].ToString());
                                }
                                file.Write("EOF");
                                file.Close();
                            }
                            unsolved.Add(p_order);
                        }
                    });
                //If it comes here then some of our orders needs to be solved with LKH tsp solver
                int numberavailableservers = p_socketservers.getAvialableServers().Count;

                if (schedulemode == 0)
                {
                    List<order> tempunsolved = unsolved.OrderByDescending(cc => cc.getOrderID()).ToList();
                    int n = 0;
                    int l = 0;
                    for (int k = 0; k < tempunsolved.Count; k++)
                    {
                        int serverindex = 0;
                        if (numberavailableservers > 1)
                        {
                            if (n < p_socketservers.getAvialableServers()[l].getShare())
                            {
                                n++;
                            }
                            else
                            {
                                n = 0;
                                l++;
                                l = l % numberavailableservers;
                            }
                            serverindex = l;
                        }
                        p_socketservers.getAvialableServers()[serverindex].addFile(options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp", @"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                        System.IO.File.Delete(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                    }
                }
                else
                {
                    List<order> tempunsolved = unsolved.OrderByDescending(cc => cc.getOrderSize()).ToList();
                    for (int k = 0; k < tempunsolved.Count; k++)
                    {
                        //Find the server with earliest finish time
                        int minload = int.MaxValue;
                        int minindex = -1;
                        for (int i = 0; i < p_socketservers.getAvialableServers().Count; i++)
                        {
                            int currentload = p_socketservers.getAvialableServers()[i].getLoad();
                            if (minload > currentload)
                            {
                                minload = currentload;
                                minindex = i;
                            }
                        }
                        //Add load to minload server
                        p_socketservers.getAvialableServers()[minindex].addLoad(tempunsolved.ElementAt(k).getOrderSize());
                        p_socketservers.getAvialableServers()[minindex].addFile(options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp", @"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                        System.IO.File.Delete(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                    }
                }
                //After file addition to each socket then send these files and get the results from the servers

                Parallel.For(0, numberavailableservers, k =>
                //for(int k = 0; k < numberavailableservers; k++)
                {
                    string serverresponseraw = p_socketservers.getAvialableServers()[k].returnSolution(false);
                    string[] solutions = serverresponseraw.Split(',');
                    for (int i = 0; i < solutions.Count(); i++)
                    {
                        sums.Add(int.Parse(solutions[i]) / m);
                    }
                });
                return sums.Sum() / samplesize;
            }
            return -1;
        }

        public double tspLKHNetVisibility(warehouse p_wh, int samplesize, socketservers p_socketservers, int schedulemode)
        {
            var sums = new ConcurrentBag<double>();
            var unsolved = new ConcurrentBag<order>();
            double m = 10;
            if (samplesize > 0)
            {
                Parallel.For(0, samplesize, k =>
                {
                    order p_order = p_wh.getOrders()[k];
                    List<node> tourlocations = new List<node>();

                    tourlocations.Add(p_wh.pdnodes[0]);
                    for (int i = 0; i < p_order.getOrderSize(); i++)
                    {
                        if (!tourlocations.Contains(p_order.getOrderSkus()[i].location))
                        {
                            tourlocations.Add(p_order.getOrderSkus()[i].location);
                        }
                    }
                    int dimension = tourlocations.Count;
                    if (dimension == 2)
                    {
                        sums.Add(2 * Convert.ToDouble(Convert.ToInt32(p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[0], tourlocations[1])) * m) / m);
                    }
                    if (dimension == 3)
                    {
                        double dist1 = p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[0], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[1], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[2], tourlocations[0]);
                        double dist2 = p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[0], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[2], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[1], tourlocations[0]);

                        if (dist1 < dist2)
                        {
                            sums.Add(Convert.ToDouble(Convert.ToInt32(dist1 * m)) / m);
                        }
                        else
                        {
                            sums.Add(Convert.ToDouble(Convert.ToInt32(dist2 * m)) / m);
                        }
                    }
                    int[] tour = new int[dimension];
                    int mysize = (dimension * (dimension + 1)) / 2;

                    int[] cost = new int[mysize];
                    int c = 0;

                    for (int i = 0; i < dimension; i++)
                    {
                        for (int j = 0; j <= i; j++)
                        {
                            cost[c] = Convert.ToInt32(m * p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[i], tourlocations[j]));
                            c++;
                        }
                    }
                    if (dimension > 3)
                    {
                        System.IO.File.Delete(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + p_order.getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + p_order.getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp", true))
                        {
                            string[] lines = { "NAME: whtsp", "TYPE: TSP", "DIMENSION: " + dimension.ToString(), "EDGE_WEIGHT_TYPE: EXPLICIT", "EDGE_WEIGHT_FORMAT: LOWER_DIAG_ROW", "EDGE_WEIGHT_SECTION" };
                            for (int i = 0; i < lines.Count(); i++)
                            {
                                file.WriteLine(lines[i]);
                            }
                            for (int i = 0; i < cost.Count(); i++)
                            {
                                file.WriteLine(cost[i].ToString());
                            }
                            file.Write("EOF");
                            file.Close();
                        }
                        unsolved.Add(p_order);
                    }
                });
                //If it comes here then some of our orders needs to be solved with LKH tsp solver
                int numberavailableservers = p_socketservers.getAvialableServers().Count;

                if (schedulemode == 0)
                {
                    List<order> tempunsolved = unsolved.OrderByDescending(cc => cc.getOrderID()).ToList();
                    int n = 0;
                    int l = 0;
                    for (int k = 0; k < tempunsolved.Count; k++)
                    {
                        int serverindex = 0;
                        if (numberavailableservers > 1)
                        {
                            if (n < p_socketservers.getAvialableServers()[l].getShare())
                            {
                                n++;
                            }
                            else
                            {
                                n = 0;
                                l++;
                                l = l % numberavailableservers;
                            }
                            serverindex = l;
                        }
                        p_socketservers.getAvialableServers()[serverindex].addFile(options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp", @"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                        System.IO.File.Delete(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                    }
                }
                else
                {
                    List<order> tempunsolved = unsolved.OrderByDescending(cc => cc.getOrderSize()).ToList();
                    for (int k = 0; k < tempunsolved.Count; k++)
                    {
                        //Find the server with earliest finish time
                        int minload = int.MaxValue;
                        int minindex = -1;
                        for (int i = 0; i < p_socketservers.getAvialableServers().Count; i++)
                        {
                            int currentload = p_socketservers.getAvialableServers()[i].getLoad();
                            if (minload > currentload)
                            {
                                minload = currentload;
                                minindex = i;
                            }
                        }
                        //Add load to minload server
                        p_socketservers.getAvialableServers()[minindex].addLoad(tempunsolved.ElementAt(k).getOrderSize());
                        p_socketservers.getAvialableServers()[minindex].addFile(options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp", @"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                        System.IO.File.Delete(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                    }
                }
                //After file addition to each socket then send these files and get the results from the servers

                Parallel.For(0, numberavailableservers, k =>
                //for(int k = 0; k < numberavailableservers; k++)
                {
                    string serverresponseraw = p_socketservers.getAvialableServers()[k].returnSolution(false);
                    string[] solutions = serverresponseraw.Split(',');
                    for (int i = 0; i < solutions.Count(); i++)
                    {
                        sums.Add(int.Parse(solutions[i]) / m);
                    }
                });
                return sums.Sum() / samplesize;
            }
            return -1;
        }

        /// <summary>
        /// Solve optimally on local computer with aisle centers method
        /// </summary>
        /// <param name="p_wh">Warehouse Object</param>
        /// <param name="p_order">Order Object</param>
        /// <param name="p_k">Order Number</param>
        /// <returns>Optimal Tour Cost</returns>
        public double tspOptSteiner(warehouse p_wh, order p_order, int p_k)
        {
            List<node> tourlocations = new List<node>();
            double m = 10;//Decimal precision
            bool writefile = true;
            tourlocations.Add(p_wh.pdnodes[0]);
            for (int i = 0; i < p_order.getOrderSize(); i++)
            {
                if (!tourlocations.Contains(p_order.getOrderSkus()[i].location))
                {
                    tourlocations.Add(p_order.getOrderSkus()[i].location);
                }
            }
            int dimension = tourlocations.Count;
            if (dimension == 2)
            {
                return 2 * Convert.ToDouble(Convert.ToInt32(p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[1])) * m) / m;
            }
            if (dimension == 3)
            {
                double dist1 = p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[1], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[2], tourlocations[0]);
                double dist2 = p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[2], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[1], tourlocations[0]);

                if (dist1 < dist2)
                {
                    return Convert.ToDouble(Convert.ToInt32(dist1 * m)) / m;
                }
                else
                {
                    return Convert.ToDouble(Convert.ToInt32(dist2 * m)) / m;
                }
            }
            int[] tour = new int[dimension];
            int mysize = (dimension * (dimension + 1)) / 2;

            double tourcost = 0;
            int[] cost = new int[mysize];
            int c = 0;

            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    cost[c] = Convert.ToInt32(m * p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[i], tourlocations[j]));
                    c++;
                }
            }

            if (writefile)
            {
                System.IO.File.Delete(@"C:\gabak\tsp\" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp");
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\gabak\tsp\" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp", true))
                {
                    string[] lines = { "NAME: whtsp", "TYPE: TSP", "DIMENSION: " + dimension.ToString(), "EDGE_WEIGHT_TYPE: EXPLICIT", "EDGE_WEIGHT_FORMAT: LOWER_DIAG_ROW", "EDGE_WEIGHT_SECTION" };
                    for (int i = 0; i < lines.Count(); i++)
                    {
                        file.WriteLine(lines[i]);
                    }
                    for (int i = 0; i < cost.Count(); i++)
                    {
                        file.WriteLine(cost[i].ToString());
                    }
                    file.Write("EOF");
                    file.Close();
                }
            }

            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("C:\\gabak\\concorde.exe");
                psi.Arguments = " -x /cygdrive/c/gabak/tsp/" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp";
                psi.CreateNoWindow = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

                System.Diagnostics.Process cmdexe = new System.Diagnostics.Process();
                cmdexe.StartInfo = psi;
                cmdexe.Start();
                string output = cmdexe.StandardOutput.ReadToEnd();
                cmdexe.WaitForExit();
                while (!cmdexe.HasExited)
                {
                }
                string output1 = output.Substring(output.IndexOf("Optimal Solution:") + 18);
                string output2 = output1.Substring(0, output1.IndexOf("\n"));
                tourcost = Convert.ToDouble(output2);
                cmdexe.Dispose();//This will stop memory leak (hopefully)
                System.IO.File.Delete(@"C:\gabak\tsp\" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp");
                System.IO.File.Delete(@"" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".sol");
                System.IO.File.Delete(@"" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".res");
                System.IO.File.Delete(@"O" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".res");
            }
            catch (Exception e)
            {
            }
            return tourcost / m;
        }

        /// <summary>
        /// Solve optimally on local computer with visibility graph method
        /// </summary>
        /// <param name="p_wh">Warehouse Object</param>
        /// <param name="p_order">Order Object</param>
        /// <param name="p_k">Order Number</param>
        /// <returns>Optimal Tour Cost</returns>
        public double tspOptVisibility(warehouse p_wh, order p_order, int p_k)
        {
            List<node> tourlocations = new List<node>();
            double m = 10;//Decimal precision
            bool writefile = true;
            tourlocations.Add(p_wh.pdnodes[0]);
            for (int i = 0; i < p_order.getOrderSize(); i++)
            {
                if (!tourlocations.Contains(p_order.getOrderSkus()[i].location))
                {
                    tourlocations.Add(p_order.getOrderSkus()[i].location);
                }
            }
            int dimension = tourlocations.Count;
            if (dimension == 2)
            {
                return 2 * Convert.ToDouble(Convert.ToInt32(p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[0], tourlocations[1])) * m) / m;
            }
            if (dimension == 3)
            {
                double dist1 = p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[0], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[1], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[2], tourlocations[0]);
                double dist2 = p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[0], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[2], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[1], tourlocations[0]);

                if (dist1 < dist2)
                {
                    return Convert.ToDouble(Convert.ToInt32(dist1 * m)) / m;
                }
                else
                {
                    return Convert.ToDouble(Convert.ToInt32(dist2 * m)) / m;
                }
            }
            int[] tour = new int[dimension];
            int mysize = (dimension * (dimension + 1)) / 2;

            double tourcost = 0;
            int[] cost = new int[mysize];
            int c = 0;

            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    cost[c] = Convert.ToInt32(m * p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[i], tourlocations[j]));
                    c++;
                }
            }

            if (writefile)
            {
                System.IO.File.Delete(@"C:\gabak\tsp\" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp");
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\gabak\tsp\" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp", true))
                {
                    string[] lines = { "NAME: whtsp", "TYPE: TSP", "DIMENSION: " + dimension.ToString(), "EDGE_WEIGHT_TYPE: EXPLICIT", "EDGE_WEIGHT_FORMAT: LOWER_DIAG_ROW", "EDGE_WEIGHT_SECTION" };
                    for (int i = 0; i < lines.Count(); i++)
                    {
                        file.WriteLine(lines[i]);
                    }
                    for (int i = 0; i < cost.Count(); i++)
                    {
                        file.WriteLine(cost[i].ToString());
                    }
                    file.Write("EOF");
                    file.Close();
                }
            }

            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("C:\\gabak\\concorde.exe");
                psi.Arguments = " -x /cygdrive/c/gabak/tsp/" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp";
                psi.CreateNoWindow = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

                System.Diagnostics.Process cmdexe = new System.Diagnostics.Process();
                cmdexe.StartInfo = psi;
                cmdexe.Start();
                string output = cmdexe.StandardOutput.ReadToEnd();
                cmdexe.WaitForExit();
                while (!cmdexe.HasExited)
                {
                }
                string output1 = output.Substring(output.IndexOf("Optimal Solution:") + 18);
                string output2 = output1.Substring(0, output1.IndexOf("\n"));
                tourcost = Convert.ToDouble(output2);
                cmdexe.Dispose();//This will stop memory leak (hopefully)
                System.IO.File.Delete(@"C:\gabak\tsp\" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".tsp");
                System.IO.File.Delete(@"" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".sol");
                System.IO.File.Delete(@"" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".res");
                System.IO.File.Delete(@"O" + options.processid.ToString() + "-" + p_k.ToString() + "-" + p_wh.id.ToString() + ".res");
            }
            catch (Exception e)
            {
            }
            return tourcost / m;
        }

        /// <summary>
        /// Solve optimally using distributed computing and aisle centers method
        /// </summary>
        /// <param name="p_wh">Warehouse Object</param>
        /// <param name="samplesize">Number of Orders to Be Solved</param>
        /// <param name="p_socketservers">List of servers</param>
        /// <returns>Returns average tour cost of overall orders in sample</returns>
        public double tspOptNetSteiner(warehouse p_wh, int samplesize, socketservers p_socketservers, int schedulemode)
        {
            var sums = new ConcurrentBag<double>();
            var unsolved = new ConcurrentBag<order>();
            double m = 10;
            if (samplesize > 0)
            {
                Parallel.For(0, samplesize, k =>
                {
                    order p_order = p_wh.getOrders()[k];
                    List<node> tourlocations = new List<node>();

                    tourlocations.Add(p_wh.pdnodes[0]);
                    for (int i = 0; i < p_order.getOrderSize(); i++)
                    {
                        if (!tourlocations.Contains(p_order.getOrderSkus()[i].location))
                        {
                            tourlocations.Add(p_order.getOrderSkus()[i].location);
                        }
                    }
                    int dimension = tourlocations.Count;
                    if (dimension == 2)
                    {
                        sums.Add(2 * Convert.ToDouble(Convert.ToInt32(p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[1])) * m) / m);
                    }
                    if (dimension == 3)
                    {
                        double dist1 = p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[1], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[2], tourlocations[0]);
                        double dist2 = p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[0], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[2], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[1], tourlocations[0]);

                        if (dist1 < dist2)
                        {
                            sums.Add(Convert.ToDouble(Convert.ToInt32(dist1 * m)) / m);
                        }
                        else
                        {
                            sums.Add(Convert.ToDouble(Convert.ToInt32(dist2 * m)) / m);
                        }
                    }
                    int[] tour = new int[dimension];
                    int mysize = (dimension * (dimension + 1)) / 2;

                    int[] cost = new int[mysize];
                    int c = 0;

                    for (int i = 0; i < dimension; i++)
                    {
                        for (int j = 0; j <= i; j++)
                        {
                            cost[c] = Convert.ToInt32(m * p_wh.shortestPathDistanceTwoLocationsSteiner(tourlocations[i], tourlocations[j]));
                            c++;
                        }
                    }
                    if (dimension > 3)
                    {
                        System.IO.File.Delete(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + p_order.getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + p_order.getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp", true))
                        {
                            string[] lines = { "NAME: whtsp", "TYPE: TSP", "DIMENSION: " + dimension.ToString(), "EDGE_WEIGHT_TYPE: EXPLICIT", "EDGE_WEIGHT_FORMAT: LOWER_DIAG_ROW", "EDGE_WEIGHT_SECTION" };
                            for (int i = 0; i < lines.Count(); i++)
                            {
                                file.WriteLine(lines[i]);
                            }
                            for (int i = 0; i < cost.Count(); i++)
                            {
                                file.WriteLine(cost[i].ToString());
                            }
                            file.Write("EOF");
                            file.Close();
                        }
                        unsolved.Add(p_order);
                    }
                });
                //If it comes here then some of our orders needs to be solved with concorde tsp solver

                int numberavailableservers = p_socketservers.getAvialableServers().Count;

                if (schedulemode == 0)
                {
                    List<order> tempunsolved = unsolved.OrderByDescending(cc => cc.getOrderID()).ToList();
                    int n = 0;
                    int l = 0;
                    for (int k = 0; k < tempunsolved.Count; k++)
                    {
                        int serverindex = 0;
                        if (numberavailableservers > 1)
                        {
                            if (n < p_socketservers.getAvialableServers()[l].getShare())
                            {
                                n++;
                            }
                            else
                            {
                                n = 0;
                                l++;
                                l = l % numberavailableservers;
                            }
                            serverindex = l;
                        }
                        p_socketservers.getAvialableServers()[serverindex].addFile(options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp", @"C:\concorde\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                        System.IO.File.Delete(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                    }
                }
                else
                {
                    List<order> tempunsolved = unsolved.OrderByDescending(cc => cc.getOrderSize()).ToList();
                    for (int k = 0; k < tempunsolved.Count; k++)
                    {
                        //Find the server with earliest finish time
                        int minload = int.MaxValue;
                        int minindex = -1;
                        for (int i = 0; i < p_socketservers.getAvialableServers().Count; i++)
                        {
                            int currentload = p_socketservers.getAvialableServers()[i].getLoad();
                            if (minload > currentload)
                            {
                                minload = currentload;
                                minindex = i;
                            }
                        }
                        //Add load to minload server
                        p_socketservers.getAvialableServers()[minindex].addLoad(tempunsolved.ElementAt(k).getOrderSize());
                        p_socketservers.getAvialableServers()[minindex].addFile(options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp", @"C:\concorde\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                        System.IO.File.Delete(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                    }
                }
                //After file addition to each socket then send these files and get the results from the servers

                Parallel.For(0, numberavailableservers, k =>
                //for(int k = 0; k < numberavailableservers; k++)
                {
                    string serverresponseraw = p_socketservers.getAvialableServers()[k].returnSolution(true);
                    string[] solutions = serverresponseraw.Split(',');
                    for (int i = 0; i < solutions.Count(); i++)
                    {
                        sums.Add(int.Parse(solutions[i]) / m);
                    }
                });
                return sums.Sum() / samplesize;
            }
            else//if samplesize is 0 then do not do anything
            {
                return -1;
            }
        }

        /// <summary>
        /// Solve optimally using distributed computing and visibility graph method
        /// </summary>
        /// <param name="p_wh">Warehouse Object</param>
        /// <param name="samplesize">Number of Orders to Be Solved</param>
        /// <param name="p_socketservers">List of servers</param>
        /// <returns>Returns average tour cost of overall orders in sample</returns>
        public double tspOptNetVisibility(warehouse p_wh, int samplesize, socketservers p_socketservers, int schedulemode)
        {
            var sums = new ConcurrentBag<double>();
            var unsolved = new ConcurrentBag<order>();
            double m = 10;
            if (samplesize > 0)
            {
                Parallel.For(0, samplesize, k =>
                {
                    order p_order = p_wh.getOrders()[k];
                    List<node> tourlocations = new List<node>();

                    tourlocations.Add(p_wh.pdnodes[0]);
                    for (int i = 0; i < p_order.getOrderSize(); i++)
                    {
                        if (!tourlocations.Contains(p_order.getOrderSkus()[i].location))
                        {
                            tourlocations.Add(p_order.getOrderSkus()[i].location);
                        }
                    }
                    int dimension = tourlocations.Count;
                    if (dimension == 2)
                    {
                        sums.Add(2 * Convert.ToDouble(Convert.ToInt32(p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[0], tourlocations[1])) * m) / m);
                    }
                    if (dimension == 3)
                    {
                        double dist1 = p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[0], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[1], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[2], tourlocations[0]);
                        double dist2 = p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[0], tourlocations[2]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[2], tourlocations[1]) + p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[1], tourlocations[0]);

                        if (dist1 < dist2)
                        {
                            sums.Add(Convert.ToDouble(Convert.ToInt32(dist1 * m)) / m);
                        }
                        else
                        {
                            sums.Add(Convert.ToDouble(Convert.ToInt32(dist2 * m)) / m);
                        }
                    }
                    int[] tour = new int[dimension];
                    int mysize = (dimension * (dimension + 1)) / 2;

                    int[] cost = new int[mysize];
                    int c = 0;

                    for (int i = 0; i < dimension; i++)
                    {
                        for (int j = 0; j <= i; j++)
                        {
                            cost[c] = Convert.ToInt32(m * p_wh.shortestPathDistanceTwoLocationsVisibilityGraph(tourlocations[i], tourlocations[j]));
                            c++;
                        }
                    }
                    if (dimension > 3)
                    {
                        System.IO.File.Delete(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + p_order.getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + p_order.getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp", true))
                        {
                            string[] lines = { "NAME: whtsp", "TYPE: TSP", "DIMENSION: " + dimension.ToString(), "EDGE_WEIGHT_TYPE: EXPLICIT", "EDGE_WEIGHT_FORMAT: LOWER_DIAG_ROW", "EDGE_WEIGHT_SECTION" };
                            for (int i = 0; i < lines.Count(); i++)
                            {
                                file.WriteLine(lines[i]);
                            }
                            for (int i = 0; i < cost.Count(); i++)
                            {
                                file.WriteLine(cost[i].ToString());
                            }
                            file.Write("EOF");
                            file.Close();
                        }
                        unsolved.Add(p_order);
                    }
                });
                //If it comes here then some of our orders needs to be solved with concorde tsp solver

                int numberavailableservers = p_socketservers.getAvialableServers().Count;

                if (schedulemode == 0)
                {
                    List<order> tempunsolved = unsolved.OrderByDescending(cc => cc.getOrderID()).ToList();
                    int n = 0;
                    int l = 0;
                    for (int k = 0; k < tempunsolved.Count; k++)
                    {
                        int serverindex = 0;
                        if (numberavailableservers > 1)
                        {
                            if (n < p_socketservers.getAvialableServers()[l].getShare())
                            {
                                n++;
                            }
                            else
                            {
                                n = 0;
                                l++;
                                l = l % numberavailableservers;
                            }
                            serverindex = l;
                        }
                        p_socketservers.getAvialableServers()[serverindex].addFile(options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp", @"C:\concorde\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).ToString() + "-" + p_wh.id.ToString() + ".tsp");
                        System.IO.File.Delete(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                    }
                }
                else
                {
                    List<order> tempunsolved = unsolved.OrderByDescending(cc => cc.getOrderSize()).ToList();
                    for (int k = 0; k < tempunsolved.Count; k++)
                    {
                        //Find the server with earliest finish time
                        int minload = int.MaxValue;
                        int minindex = -1;
                        for (int i = 0; i < p_socketservers.getAvialableServers().Count; i++)
                        {
                            int currentload = p_socketservers.getAvialableServers()[i].getLoad();
                            if (minload > currentload)
                            {
                                minload = currentload;
                                minindex = i;
                            }
                        }
                        //Add load to minload server
                        p_socketservers.getAvialableServers()[minindex].addLoad(tempunsolved.ElementAt(k).getOrderSize());
                        p_socketservers.getAvialableServers()[minindex].addFile(options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp", @"C:\concorde\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                        System.IO.File.Delete(@"C:\gabak\sendfiles\" + options.processid.ToString() + "-" + tempunsolved.ElementAt(k).getOrderID().ToString() + "-" + p_wh.id.ToString() + ".tsp");
                    }
                }
                //After file addition to each socket then send these files and get the results from the servers

                Parallel.For(0, numberavailableservers, k =>
                //for(int k = 0; k < numberavailableservers; k++)
                {
                    string serverresponseraw = p_socketservers.getAvialableServers()[k].returnSolution(true);
                    string[] solutions = serverresponseraw.Split(',');
                    for (int i = 0; i < solutions.Count(); i++)
                    {
                        sums.Add(int.Parse(solutions[i]) / m);
                    }
                });
                return sums.Sum() / samplesize;
            }
            else//if samplesize is 0 then do not do anything
            {
                return -1;
            }
        }
    }
}