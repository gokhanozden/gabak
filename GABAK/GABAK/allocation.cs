//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden

using System;
using System.Collections.Generic;
using System.Linq;

namespace GABAK
{
    /// <summary>
    /// Allocation class has multiple allocation algorithms used in warehouse class to allocate SKUs (products) to storage locations
    /// </summary>
    internal class allocation
    {
        private int seed = 0;//Used for random allocation
        private Random rnd;//Used for random allocation
        private List<node> locations;//Warehouse pick locations
        private List<sku> sortedskus;//A SKU list used in calculations for different allocation methods

        /// <summary>
        /// Constructor for allocation class
        /// </summary>
        /// <param name="p_seed"></param>
        public allocation(int p_seed)
        {
            seed = p_seed;
            rnd = new Random(seed);
            locations = new List<node>();
            sortedskus = new List<sku>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_skus">List of SKUs</param>
        /// <param name="p_wh">Warehouse object</param>
        /// <param name="method">Allocation method: 0 turnover, 1 straight, 2 random</param>
        /// <returns>Returns -1 if there is insufficent warehouse space, 0 if there is sufficent warehouse space</returns>
        public int allocateSKUs(List<sku> p_skus, warehouse p_wh, int method)
        {
            int totalstoragelocations = p_wh.totalNumberOfLocations();
            if (p_skus.Count > totalstoragelocations) return -1;//Return insufficient warehouse space
            //Add warehouse locations to locations list in allocation class
            for (int i = 0; i < p_wh.regions.Count; i++)
                for (int j = 0; j < p_wh.regions[i].pickingaisleedges.Count(); j++)
                    for (int k = 0; k < p_wh.regions[i].pickingaisleedges[j].getOnEdgeNodes().Count(); k++)
                    {
                        locations.Add(p_wh.regions[i].pickingaisleedges[j].getOnEdgeNodes()[k]);
                    }
            //Use a different sku list to sort the skus based on their frequency
            //We don't want to change the order of skus in the original list because this might affect the randomized allocation process later
            for (int i = 0; i < p_skus.Count; i++)
            {
                sortedskus.Add(p_skus[i]);
            }

            if (method == 0)//Turnover based storage allocation
            {
                sortedskus.Sort((x, y) => y.pickprobability.CompareTo(x.pickprobability));
                //Set sku locations based on their frequency to the best locations based on overall ranking

                int k = 0;
                for (int i = 0; i < sortedskus.Count; i++)
                {
                    bool added = false;
                    if (p_wh.overallranking[k].node1.s1 != null)
                    {
                        if (p_wh.overallranking[k].node1.s1.addSKU(sortedskus[i]))
                        {
                            sortedskus[i].location = p_wh.overallranking[k].node1;
                            added = true;
                        }
                    }
                    if (!added && p_wh.overallranking[k].node1.s2 != null)
                    {
                        if (p_wh.overallranking[k].node1.s2.addSKU(sortedskus[i]))
                        {
                            sortedskus[i].location = p_wh.overallranking[k].node1;
                            added = true;
                        }
                    }
                    if (!added)
                    {
                        k++;
                        i--;
                    }
                }
            }
            else if (method == 1) //Straight allocation (first SKU goes to first location based on location ID)
            {
                int k = 0;
                for (int i = 0; i < sortedskus.Count; i++)
                {
                    bool added = false;
                    if (locations[k].s1 != null)
                    {
                        if (locations[k].s1.addSKU(sortedskus[i]))
                        {
                            sortedskus[i].location = locations[k];
                            added = true;
                        }
                    }
                    if (!added && locations[k].s2 != null)
                    {
                        if (locations[k].s2.addSKU(sortedskus[i]))
                        {
                            sortedskus[i].location = locations[k];
                            added = true;
                        }
                    }
                    if (!added)
                    {
                        k++;
                        i--;
                    }
                }
            }
            else //Randomized allocation, each SKU goes to a random location
            {
                int n = 2;//Number of SKUs per pick location
                while (sortedskus.Count > 0)
                {
                    node tmplocation = locations[rnd.Next(0, locations.Count - 1)];
                    for (int i = 0; i < n; i++)
                    {
                        if (sortedskus.Count > 0)
                        {
                            sku tmpsku = sortedskus[rnd.Next(0, sortedskus.Count - 1)];
                            tmpsku.location = tmplocation;
                            sortedskus.Remove(tmpsku);
                        }
                    }
                    locations.Remove(tmplocation);
                }
            }
            return 0;
        }
    }
}