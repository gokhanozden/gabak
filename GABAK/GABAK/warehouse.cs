//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace GABAK
{

    class warehouse
    {
        /// <summary>
        /// Id used for unique identification of warehouses
        /// </summary>
        public static int nextID;
        public int id { get; private set; }

        /// <summary>
        /// Width of warehouse
        /// </summary>
        private double width;

        /// <summary>
        /// Depth Ratio of warehouse
        /// </summary>
        private double depth;

        public double area;

        public double aspectratio;

        public double pickersize;

        /// <summary>
        /// A list that has all cross aisles of warehouse object
        /// </summary>
        public List<edge> corneredges;

        /// <summary>
        /// A list that has all cross aisles of warehouse object
        /// </summary>
        public List<edge> regionedges;

        /// <summary>
        /// A list that has all pick and deposit point edges of warehouse object
        /// </summary>
        public List<edge> pdedges;
        /// <summary>
        /// A list that has 4 corner nodes of warehouse object
        /// </summary>
        public List<node> cornernodes;

        /// <summary>
        /// A list that has exterior nodes of warehouse object
        /// </summary>
        public List<node> exteriornodes;

        /// <summary>
        /// A list that has interior nodes of warehouse object
        /// </summary>
        public List<node> interiornodes;

        /// <summary>
        /// A list that has pd nodes of warehouse object
        /// </summary>
        public List<node> pdnodes;

        /// <summary>
        /// A list that has adjuster parameters of warehouse
        /// </summary>
        public List<double> adjuster;

        /// <summary>
        /// A list that has pickadjuster parameters of warehouse
        /// </summary>
        public List<double> pickadjuster;

        /// <summary>
        /// A list that has important nodes of warehouse object
        /// </summary>
        private List<node> importantnodes;

        /// <summary>
        /// An array (for thread-safety) that has all storage location centers and PD point
        /// </summary>
        public node[] locationnodes;

        /// <summary>
        /// An array (for thread-safety) that has all storage location centers, PD, and polygon vertices used in visibility graph calculations
        /// </summary>
        public node[] graphnodes;
        /// <summary>
        /// An array (for thread-safety) that stores all the polygons which are used in collision calculations in visibility graph
        /// </summary>
        public polygon[] polygons;

        /// <summary>
        /// A list that has all regions of warehouse object
        /// </summary>
        public List<region> regions;
        /// <summary>
        /// A list that has all the edges that connects picking aisle beginning and ending points on region edges
        /// </summary>
        private List<edge> pickingaisleregionedges;

        public List<distance> distances;
        private double[,] importantnodedistances;
        public double[,] visibilitygraphdistances;
        public double[,] locationnodedistances;
        public bool[,] connectivity;
        public bool[] problematicconnections;
        public List<distance> onetomanytotaldistances;
        public List<distance> manytomanytotaldistances;
        public List<distance> overallranking;

        private List<order> myorders;
        private List<sku> myskus;

        public double crossaislewidth;
        public double pickingaislewidth;
        public double pickinglocationdepth;
        public double pickinglocationwidth;

        public double avgTourLength;
        public TimeSpan elapsed1;
        public TimeSpan elapsed2;

        public bool usevisibilitygraph;

        public double manytomanytotaldistancessum;
        /// <summary>
        /// Default constructor of warehouse object
        /// </summary>
        public warehouse()
        {
            id = System.Threading.Interlocked.Increment(ref nextID);
            corneredges = new List<edge>();
            regionedges = new List<edge>();
            pickingaisleregionedges = new List<edge>();
            pdedges = new List<edge>();
            cornernodes = new List<node>();
            exteriornodes = new List<node>();
            interiornodes = new List<node>();
            pdnodes = new List<node>();
            adjuster = new List<double>();
            pickadjuster = new List<double>();
            importantnodes = new List<node>();
            regions = new List<region>();
            width = 400;
            depth = 200;
            area = 80000;
            pickersize = 2;
            aspectratio = 0.5;
            avgTourLength = 1;
            usevisibilitygraph = false;
            distances = new List<distance>();
            onetomanytotaldistances = new List<distance>();
            manytomanytotaldistances = new List<distance>();
            overallranking = new List<distance>();
            myorders = new List<order>();
            myskus = new List<sku>();
        }

        public void resetNetwork()
        {
            corneredges.Clear();
            cornernodes.Clear();
            regionedges.Clear();
            pickingaisleregionedges.Clear();
            pdedges.Clear(); ;
            exteriornodes.Clear();
            interiornodes.Clear();
            importantnodes.Clear();
            adjuster.Clear();
            pickadjuster.Clear();
            regions.Clear();
            distances.Clear();
            if (importantnodedistances != null)
            {
                Array.Clear(importantnodedistances, 0, importantnodedistances.Length);
            }
            if (locationnodedistances != null)
            {
                Array.Clear(locationnodedistances, 0, locationnodedistances.Length);
            }
            if( visibilitygraphdistances != null)
            {
                Array.Clear(visibilitygraphdistances, 0, visibilitygraphdistances.Length);
            }
            onetomanytotaldistances.Clear();
            manytomanytotaldistances.Clear();
            pdnodes.Clear();
            overallranking.Clear();
        }

        /// <summary>
        /// Creates 4 corner edges of the warehouse
        /// </summary>
        private void createCornerEdges()
        {
            addCornerNode(options.topleft);
            addCornerNode(options.topright);
            addCornerNode(options.bottomright);
            addCornerNode(options.bottomleft);
            //Top Cross Aisle
            addCornerEdge(0);
            //Right Cross Aisle
            addCornerEdge(1);
            //Bottom Cross Aisle
            addCornerEdge(2);
            //Left Cross Aisle
            addCornerEdge(3);
        }

        private void addCornerNode(int position)
        {
            //If corner nodes are 4 then do not try to add another one.
            if (cornernodes.Count > 4) return;
            //Calculate the margin for edge cross aisle
            double margin = crossaislewidth / 2;
            double tmpX = 0, tmpY = 0;
            if (position == 0)
            {
                tmpX = 0 + margin;
                tmpY = 0 + margin;
            }
            if (position == 1)
            {
                tmpX = this.width - margin;
                tmpY = 0 + margin;
            }
            if (position == 2)
            {
                tmpX = this.width - margin;
                tmpY = this.depth - margin;
            }
            if (position == 3)
            {
                tmpX = 0 + margin;
                tmpY = this.depth - margin;
            }
            node tmp = new node(tmpX, tmpY, options.cornernode);
            cornernodes.Add(tmp);
            importantnodes.Add(tmp);
        }

        private void addCornerEdge(int position)
        {
            if (position == 0)
            {
                connect(cornernodes[options.topleft], cornernodes[options.topright]);
            }
            if (position == 1)
            {
                connect(cornernodes[options.topright], cornernodes[options.bottomright]);
            }
            if (position == 2)
            {
                connect(cornernodes[options.bottomright], cornernodes[options.bottomleft]);
            }
            if (position == 3)
            {
                connect(cornernodes[options.bottomleft], cornernodes[options.topleft]);
            }
        }

        private edge connect(node p_node1, node p_node2)
        {
            edge tempedge = null;
            //Corner edge connections
            if (p_node1.type == options.cornernode && p_node2.type == options.cornernode)
            {
                tempedge = p_node1.connect(p_node2, options.regionedge);
                corneredges.Add(tempedge);
                tempedge.setWidth(this.crossaislewidth);
                regionedges.Add(tempedge);
            }
            //Exteriornode and cornernode connections
            if ((p_node1.type == options.cornernode && p_node2.type == options.exteriornode) || (p_node1.type == options.exteriornode && p_node2.type == options.cornernode))
            {
                tempedge = p_node1.connect(p_node2, options.regionedge);
                tempedge.setWidth(this.crossaislewidth);
                regionedges.Add(tempedge);
            }
            //Exteriornode and exteriornode connections
            if (p_node1.type == options.exteriornode && p_node2.type == options.exteriornode)
            {
                tempedge = p_node1.connect(p_node2, options.regionedge);
                tempedge.setWidth(this.crossaislewidth);
                regionedges.Add(tempedge);
            }
            //Interiornode and exteriornode connections
            if ((p_node1.type == options.interiornode && p_node2.type == options.exteriornode) || (p_node1.type == options.exteriornode && p_node2.type == options.interiornode))
            {
                tempedge = p_node1.connect(p_node2, options.regionedge);
                tempedge.setWidth(this.crossaislewidth);
                regionedges.Add(tempedge);
            }
            //Interiornode and interiornode connections
            if (p_node1.type == options.interiornode && p_node2.type == options.interiornode)
            {
                tempedge = p_node1.connect(p_node2, options.regionedge);
                tempedge.setWidth(this.crossaislewidth);
                regionedges.Add(tempedge);
            }
            //Pickingaislenode and cornernode connections
            if ((p_node1.type == options.cornernode && p_node2.type == options.pickingaislenode) || (p_node1.type == options.pickingaislenode && p_node2.type == options.cornernode))
            {
                addPickingAisleRegionEdge(p_node1, p_node2);
            }
            //Pickingaislenode and exteriornode connections
            if ((p_node1.type == options.exteriornode && p_node2.type == options.pickingaislenode) || (p_node1.type == options.pickingaislenode && p_node2.type == options.exteriornode))
            {
                addPickingAisleRegionEdge(p_node1, p_node2);
            }
            //Pickingaislenode and pickingaislenode connections, two different picking aisles
            if (p_node1.type == options.pickingaislenode && p_node2.type == options.pickingaislenode)
            {
                addPickingAisleRegionEdge(p_node1, p_node2);
            }
            //pdnode and exteriornode connections
            if ((p_node1.type == options.exteriornode && p_node2.type == options.pdnode) || (p_node1.type == options.pdnode && p_node2.type == options.exteriornode))
            {
                addPDEdge(p_node1, p_node2);
            }
            //pdnode and cornernode connections
            if ((p_node1.type == options.cornernode && p_node2.type == options.pdnode) || (p_node1.type == options.pdnode && p_node2.type == options.cornernode))
            {
                addPDEdge(p_node1, p_node2);
            }
            //pdnode and pickingaislenode connections
            if ((p_node1.type == options.pickingaislenode && p_node2.type == options.pdnode) || (p_node1.type == options.pdnode && p_node2.type == options.pickingaislenode))
            {
                addPDEdge(p_node1, p_node2);
            }
            return tempedge;
        }

        /// <summary>
        /// This function is used for general warehouse creation
        /// </summary>
        /// <param name="p_node1"></param>
        /// <param name="p_node2"></param>
        /// <param name="p_connectionk"></param>
        /// <returns></returns>
        private edge connect(node p_node1, node p_node2, int p_connectionk)
        {
            edge tempedge = null;
            //Corner edge connections
            if (p_node1.type == options.cornernode && p_node2.type == options.cornernode)
            {
                tempedge = p_node1.connect(p_node2, options.regionedge);
                tempedge.connectionk = p_connectionk;
                corneredges.Add(tempedge);
                tempedge.setWidth(this.crossaislewidth);
                regionedges.Add(tempedge);
            }
            //Exteriornode and cornernode connections
            if ((p_node1.type == options.cornernode && p_node2.type == options.exteriornode) || (p_node1.type == options.exteriornode && p_node2.type == options.cornernode))
            {
                tempedge = p_node1.connect(p_node2, options.regionedge);
                tempedge.connectionk = p_connectionk;
                tempedge.setWidth(this.crossaislewidth);
                regionedges.Add(tempedge);
            }
            //Exteriornode and exteriornode connections
            if (p_node1.type == options.exteriornode && p_node2.type == options.exteriornode)
            {
                tempedge = p_node1.connect(p_node2, options.regionedge);
                tempedge.connectionk = p_connectionk;
                tempedge.setWidth(this.crossaislewidth);
                regionedges.Add(tempedge);
            }
            //Interiornode and exteriornode connections
            if ((p_node1.type == options.interiornode && p_node2.type == options.exteriornode) || (p_node1.type == options.exteriornode && p_node2.type == options.interiornode))
            {
                tempedge = p_node1.connect(p_node2, options.regionedge);
                tempedge.connectionk = p_connectionk;
                tempedge.setWidth(this.crossaislewidth);
                regionedges.Add(tempedge);
            }
            //Interiornode and interiornode connections
            if (p_node1.type == options.interiornode && p_node2.type == options.interiornode)
            {
                tempedge = p_node1.connect(p_node2, options.regionedge);
                tempedge.connectionk = p_connectionk;
                tempedge.setWidth(this.crossaislewidth);
                regionedges.Add(tempedge);
            }
            //Pickingaislenode and cornernode connections
            if ((p_node1.type == options.cornernode && p_node2.type == options.pickingaislenode) || (p_node1.type == options.pickingaislenode && p_node2.type == options.cornernode))
            {
                addPickingAisleRegionEdge(p_node1, p_node2, p_connectionk);
            }
            //Pickingaislenode and exteriornode connections
            if ((p_node1.type == options.exteriornode && p_node2.type == options.pickingaislenode) || (p_node1.type == options.pickingaislenode && p_node2.type == options.exteriornode))
            {
                addPickingAisleRegionEdge(p_node1, p_node2, p_connectionk);
            }
            //Pickingaislenode and pickingaislenode connections, two different picking aisles
            if (p_node1.type == options.pickingaislenode && p_node2.type == options.pickingaislenode)
            {
                addPickingAisleRegionEdge(p_node1, p_node2, p_connectionk);
            }
            //pdnode and exteriornode connections
            if ((p_node1.type == options.exteriornode && p_node2.type == options.pdnode) || (p_node1.type == options.pdnode && p_node2.type == options.exteriornode))
            {
                addPDEdge(p_node1, p_node2, p_connectionk);
            }
            //pdnode and cornernode connections
            if ((p_node1.type == options.cornernode && p_node2.type == options.pdnode) || (p_node1.type == options.pdnode && p_node2.type == options.cornernode))
            {
                addPDEdge(p_node1, p_node2, p_connectionk);
            }
            //pdnode and pickingaislenode connections
            if ((p_node1.type == options.pickingaislenode && p_node2.type == options.pdnode) || (p_node1.type == options.pdnode && p_node2.type == options.pickingaislenode))
            {
                addPDEdge(p_node1, p_node2, p_connectionk);
            }
            return tempedge;
        }

        private void disconnect(node p_node1, node p_node2)
        {
            if (p_node1.type == options.cornernode && p_node2.type == options.cornernode)
            {
                for (int i = 0; i < regionedges.Count; i++)
                {
                    if ((regionedges[i].getStart() == p_node1 && regionedges[i].getEnd() == p_node2) || (regionedges[i].getStart() == p_node2 && regionedges[i].getEnd() == p_node1))
                    {
                        regionedges.RemoveAt(i);
                        p_node1.disconnect(p_node2);
                    }
                }
            }
            if ((p_node1.type == options.cornernode && p_node2.type == options.exteriornode) || (p_node1.type == options.exteriornode && p_node2.type == options.cornernode))
            {
                for (int i = 0; i < regionedges.Count; i++)
                {
                    if ((regionedges[i].getStart() == p_node1 && regionedges[i].getEnd() == p_node2) || (regionedges[i].getStart() == p_node2 && regionedges[i].getEnd() == p_node1))
                    {
                        regionedges.RemoveAt(i);
                        p_node1.disconnect(p_node2);
                    }
                }
            }
            if (p_node1.type == options.exteriornode && p_node2.type == options.exteriornode)
            {
                for (int i = 0; i < regionedges.Count; i++)
                {
                    if ((regionedges[i].getStart() == p_node1 && regionedges[i].getEnd() == p_node2) || (regionedges[i].getStart() == p_node2 && regionedges[i].getEnd() == p_node1))
                    {
                        regionedges.RemoveAt(i);
                        p_node1.disconnect(p_node2);
                    }
                }
            }
        }

        private void addPickingAisleRegionEdge(node p_start, node p_end)
        {
            //Connect two picking aisle nodes, different picking aisle
            edge tempedge = p_start.connect(p_end, options.pickingaisleregionedge);
            //Add this edge to region picking aisle edges
            pickingaisleregionedges.Add(tempedge);
        }

        private void addPickingAisleRegionEdge(node p_start, node p_end, int p_connectionk)
        {
            //Connect two picking aisle nodes, different picking aisle
            edge tempedge = p_start.connect(p_end, options.pickingaisleregionedge);
            tempedge.connectionk = p_connectionk;
            //Add this edge to region picking aisle edges
            pickingaisleregionedges.Add(tempedge);
        }

        private void addPDEdge(node p_start, node p_end)
        {
            //Connect two picking aisle nodes, different picking aisle
            edge tempedge = p_start.connect(p_end, options.pdedge);
            //Add this edge to region picking aisle edges
            pickingaisleregionedges.Add(tempedge);
        }

        private void addPDEdge(node p_start, node p_end, int p_connectionk)
        {
            //Connect two picking aisle nodes, different picking aisle
            edge tempedge = p_start.connect(p_end, options.pdedge);
            tempedge.connectionk = p_connectionk;
            //Add this edge to region picking aisle edges
            pickingaisleregionedges.Add(tempedge);
        }

        private void addExteriorNode(double p_location)
        {
            double tempX;
            double tempY;
            int tmpl = -1;
            int tmpr = -1;
            double tmplocation;

            node e = new node(options.exteriornode);

            if (p_location == 0)
            {
                e.setY(cornernodes[options.topleft].getY());
                tempX = Math.Abs(cornernodes[options.topright].getX() - cornernodes[options.topleft].getX()) * ((p_location - 0) / 0.25) + cornernodes[options.topleft].getX();
                e.setX(tempX);
                e.setLocation(p_location);
                importantnodes.Remove(cornernodes[options.topleft]);
                importantnodes.Add(e);

                //if there is an exterior node on the left find the closest on the left
                tmpl = -1;
                tmplocation = Double.MinValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() >= 0.75 && exteriornodes[i].getLocation() < 1)
                    {
                        if (tmplocation < exteriornodes[i].getLocation())
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpl = i;
                        }
                    }
                }

                //if there is an exterior node on the right find the closest on the right
                tmpr = -1;
                tmplocation = Double.MaxValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() > 0 && exteriornodes[i].getLocation() <= 0.25)
                    {
                        if (tmplocation > exteriornodes[i].getLocation())
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpr = i;
                        }
                    }
                }

                //there are exterior nodes on the left and on the right on the same corner edge 
                if (tmpl > -1 && tmpr > -1)
                {
                    //disconnect left exterior node with the topleft corner node
                    disconnect(exteriornodes[tmpl], cornernodes[options.topleft]);
                    //disconnect right exterior node with the topleft corner node
                    disconnect(exteriornodes[tmpr], cornernodes[options.topleft]);
                    //connect with the left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect with the right exterior node
                    connect(exteriornodes[tmpr], e);
                }

                //there is an exterior node on the left but not on the right
                if (tmpl > -1 && tmpr == -1)
                {
                    //disconnect left exterior node with the topleft corner node
                    disconnect(exteriornodes[tmpl], cornernodes[options.topleft]);
                    //disconnect topleft corner node and topright corner node
                    disconnect(cornernodes[options.topleft], cornernodes[options.topright]);
                    //connect left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect to the topright corner node
                    connect(cornernodes[options.topright], e);
                }

                //there is an exterior node on the right but not on the left
                if (tmpl == -1 && tmpr > -1)
                {
                    //disconnect the right exterior node with the topleft corner node
                    disconnect(exteriornodes[tmpr], cornernodes[options.topleft]);
                    //disconnect bottomleft corner node and topleft corner node
                    disconnect(cornernodes[options.bottomleft], cornernodes[options.topleft]);
                    //connect right exterior node
                    connect(exteriornodes[tmpr], e);
                    //connect to the bottomleft corner node
                    connect(cornernodes[options.bottomleft], e);
                }

                //there are no exterior nodes on the left corner edge or on the right corner edge
                if (tmpl == -1 && tmpr == -1)
                {
                    //disconnect topleft corner node and topright corner node
                    disconnect(cornernodes[options.topleft], cornernodes[options.topright]);
                    //disconnect bottomleft corner node and topleft corner node
                    disconnect(cornernodes[options.bottomleft], cornernodes[options.topleft]);
                    //connect bottomleft corner node
                    connect(cornernodes[options.bottomleft], e);
                    //connect topright corner node
                    connect(cornernodes[options.topright], e);
                }
            }

            if (p_location > 0 && p_location < 0.25)
            {
                e.setY(cornernodes[options.topleft].getY());
                tempX = Math.Abs(cornernodes[options.topright].getX() - cornernodes[options.topleft].getX()) * ((p_location - 0) / 0.25) + cornernodes[options.topleft].getX();
                e.setX(tempX);
                e.setLocation(p_location);
                importantnodes.Add(e);

                //if there is an exterior node on the left find the closest on the left
                tmpl = -1;
                tmplocation = Double.MinValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() >= 0 && exteriornodes[i].getLocation() <= 0.25)
                    {
                        if (exteriornodes[i].getLocation() < e.getLocation() && tmplocation < exteriornodes[i].getLocation())
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpl = i;
                        }
                    }
                }

                //if there is an exterior node on the right find the closest on the right
                tmpr = -1;
                tmplocation = Double.MaxValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() >= 0 && exteriornodes[i].getLocation() <= 0.25)
                    {
                        if (exteriornodes[i].getLocation() > e.getLocation() && tmplocation > exteriornodes[i].getLocation())
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpr = i;
                        }
                    }
                }

                //there are exterior nodes on the left and on the right on the same corner edge 
                if (tmpl > -1 && tmpr > -1)
                {
                    //disconnect the two connected exterior nodes on the left and on the right
                    disconnect(exteriornodes[tmpl], exteriornodes[tmpr]);
                    //connect with the left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect with the right exterior node
                    connect(exteriornodes[tmpr], e);
                }

                //there is an exterior node on the left but not on the right
                if (tmpl > -1 && tmpr == -1)
                {
                    //disconnect left exterior node with the topright corner node
                    disconnect(exteriornodes[tmpl], cornernodes[options.topright]);
                    //connect left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect to the topright corner node
                    connect(cornernodes[options.topright], e);
                }

                //there is an exterior node on the right but not on the left
                if (tmpl == -1 && tmpr > -1)
                {
                    //disconnect the right exterior node with the topleft corner node
                    disconnect(exteriornodes[tmpr], cornernodes[options.topleft]);
                    //connect right exterior node
                    connect(exteriornodes[tmpr], e);
                    //connect to the topleft corner node
                    connect(cornernodes[options.topleft], e);
                }

                //there are no exterior nodes on this corner edge
                if (tmpl == -1 && tmpr == -1)
                {
                    //disconnect topleft corner node and topright corner node
                    disconnect(cornernodes[options.topleft], cornernodes[options.topright]);
                    //connect topleft corner node
                    connect(cornernodes[options.topleft], e);
                    //connect topright corner node
                    connect(cornernodes[options.topright], e);
                }
            }

            if (p_location == 0.25)
            {
                e.setX(cornernodes[options.topright].getX());
                tempY = Math.Abs(cornernodes[options.bottomright].getY() - cornernodes[options.topright].getY()) * ((p_location - 0.25) / 0.25) + cornernodes[options.topright].getY();
                e.setY(tempY);
                e.setLocation(p_location);
                importantnodes.Remove(cornernodes[options.topright]);
                importantnodes.Add(e);

                //if there is an exterior node on the left find the closest on the left
                tmpl = -1;
                tmplocation = Double.MinValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() >= 0 && exteriornodes[i].getLocation() < 0.25)
                    {
                        if (exteriornodes[i].getLocation() > tmplocation)
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpl = i;
                        }
                    }
                }

                //if there is an exterior node on the right find the closest on the right
                tmpr = -1;
                tmplocation = Double.MaxValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() > 0.25 && exteriornodes[i].getLocation() <= 0.5)
                    {
                        if (exteriornodes[i].getLocation() < tmplocation)
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpr = i;
                        }
                    }
                }

                //there are exterior nodes on the left and on the right 
                if (tmpl > -1 && tmpr > -1)
                {
                    //disconnect left exterior node with the topright corner node
                    disconnect(exteriornodes[tmpl], cornernodes[options.topright]);
                    //disconnect right exterior node with the topright corner node
                    disconnect(exteriornodes[tmpr], cornernodes[options.topright]);
                    //connect with the left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect with the right exterior node
                    connect(exteriornodes[tmpr], e);
                }

                //there is an exterior node on the left but not on the right
                if (tmpl > -1 && tmpr == -1)
                {
                    //disconnect left exterior node with the topright corner node
                    disconnect(exteriornodes[tmpl], cornernodes[options.topright]);
                    //disconnect topright corner node and bottomright corner node
                    disconnect(cornernodes[options.topright], cornernodes[options.bottomright]);
                    //connect left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect to the bottomright corner node
                    connect(cornernodes[options.bottomright], e);
                }

                //there is an exterior node on the right but not on the left
                if (tmpl == -1 && tmpr > -1)
                {
                    //disconnect the right exterior node with the topright corner node
                    disconnect(exteriornodes[tmpr], cornernodes[options.topright]);
                    //disconnect topleft corner node and topright corner node
                    disconnect(cornernodes[options.topleft], cornernodes[options.topright]);
                    //connect right exterior node
                    connect(exteriornodes[tmpr], e);
                    //connect to the bottomleft corner node
                    connect(cornernodes[options.topleft], e);
                }

                //there are no exterior nodes on the left corner edge or on the right corner edge
                if (tmpl == -1 && tmpr == -1)
                {
                    //disconnect topleft corner node and topright corner node
                    disconnect(cornernodes[options.topleft], cornernodes[options.topright]);
                    //disconnect bottomright corner node and topright corner node
                    disconnect(cornernodes[options.bottomright], cornernodes[options.topright]);
                    //connect topleft corner node
                    connect(cornernodes[options.topleft], e);
                    //connect topright corner node
                    connect(cornernodes[options.bottomright], e);
                }
            }

            if (p_location > 0.25 && p_location < 0.5)
            {
                e.setX(cornernodes[options.topright].getX());
                tempY = Math.Abs(cornernodes[options.bottomright].getY() - cornernodes[options.topright].getY()) * ((p_location - 0.25) / 0.25) + cornernodes[options.topright].getY();
                e.setY(tempY);
                e.setLocation(p_location);
                importantnodes.Add(e);

                //if there is an exterior node on the left find the closest on the left
                tmpl = -1;
                tmplocation = Double.MinValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() >= 0.25 && exteriornodes[i].getLocation() < 0.5)
                    {
                        if (exteriornodes[i].getLocation() < e.getLocation() && exteriornodes[i].getLocation() > tmplocation)
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpl = i;
                        }
                    }
                }

                //if there is an exterior node on the right find the closest on the right
                tmpr = -1;
                tmplocation = Double.MaxValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() > 0.25 && exteriornodes[i].getLocation() <= 0.5)
                    {
                        if (exteriornodes[i].getLocation() > e.getLocation() && exteriornodes[i].getLocation() < tmplocation)
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpr = i;
                        }
                    }
                }

                //there are exterior nodes on the left and on the right on the same corner edge 
                if (tmpl > -1 && tmpr > -1)
                {
                    //disconnect the two connected exterior nodes on the left and on the right
                    disconnect(exteriornodes[tmpl], exteriornodes[tmpr]);
                    //connect with the left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect with the right exterior node
                    connect(exteriornodes[tmpr], e);
                }

                //there is an exterior node on the left but not on the right
                if (tmpl > -1 && tmpr == -1)
                {
                    //disconnect left exterior node with the bottomright corner node
                    disconnect(exteriornodes[tmpl], cornernodes[options.bottomright]);
                    //connect left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect to the bottomright corner node
                    connect(cornernodes[options.bottomright], e);
                }

                //there is an exterior node on the right but not on the left
                if (tmpl == -1 && tmpr > -1)
                {
                    //disconnect the right exterior node with the topright corner node
                    disconnect(exteriornodes[tmpr], cornernodes[options.topright]);
                    //connect right exterior node
                    connect(exteriornodes[tmpr], e);
                    //connect to the topright corner node
                    connect(cornernodes[options.topright], e);
                }

                //there are no exterior nodes on this corner edge
                if (tmpl == -1 && tmpr == -1)
                {
                    //disconnect topleft corner node and topright corner node
                    disconnect(cornernodes[options.topright], cornernodes[options.bottomright]);
                    //connect topright corner node
                    connect(cornernodes[options.topright], e);
                    //connect bottomright corner node
                    connect(cornernodes[options.bottomright], e);
                }
            }

            if (p_location == 0.5)
            {
                e.setY(cornernodes[options.bottomright].getY());
                tempX = cornernodes[options.bottomright].getX() - Math.Abs(cornernodes[options.bottomleft].getX() - cornernodes[options.bottomright].getX()) * ((p_location - 0.5) / 0.25);
                e.setX(tempX);
                e.setLocation(p_location);
                importantnodes.Remove(cornernodes[options.bottomright]);
                importantnodes.Add(e);

                //if there is an exterior node on the left find the closest on the left
                tmpl = -1;
                tmplocation = Double.MinValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() >= 0.25 && exteriornodes[i].getLocation() < 0.5)
                    {
                        if (exteriornodes[i].getLocation() > tmplocation)
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpl = i;
                        }
                    }
                }

                //if there is an exterior node on the right find the closest on the right
                tmpr = -1;
                tmplocation = Double.MaxValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() > 0.5 && exteriornodes[i].getLocation() <= 0.75)
                    {
                        if (exteriornodes[i].getLocation() < tmplocation)
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpr = i;
                        }
                    }
                }

                //there are exterior nodes on the left and on the right 
                if (tmpl > -1 && tmpr > -1)
                {
                    //disconnect left exterior node with the bottomright corner node
                    disconnect(exteriornodes[tmpl], cornernodes[options.bottomright]);
                    //disconnect right exterior node with the bottomright corner node
                    disconnect(exteriornodes[tmpr], cornernodes[options.bottomright]);
                    //connect with the left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect with the right exterior node
                    connect(exteriornodes[tmpr], e);
                }

                //there is an exterior node on the left but not on the right
                if (tmpl > -1 && tmpr == -1)
                {
                    //disconnect left exterior node with the bottomright corner node
                    disconnect(exteriornodes[tmpl], cornernodes[options.bottomright]);
                    //disconnect bottomleft corner node and bottomright corner node
                    disconnect(cornernodes[options.bottomleft], cornernodes[options.bottomright]);
                    //connect left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect to the bottomleft corner node
                    connect(cornernodes[options.bottomleft], e);
                }

                //there is an exterior node on the right but not on the left
                if (tmpl == -1 && tmpr > -1)
                {
                    //disconnect the right exterior node with the bottomright corner node
                    disconnect(exteriornodes[tmpr], cornernodes[options.bottomright]);
                    //disconnect topright corner node and bottomright corner node
                    disconnect(cornernodes[options.topright], cornernodes[options.bottomright]);
                    //connect right exterior node
                    connect(exteriornodes[tmpr], e);
                    //connect to the bottomleft corner node
                    connect(cornernodes[options.topright], e);
                }

                //there are no exterior nodes on the left corner edge or on the right corner edge
                if (tmpl == -1 && tmpr == -1)
                {
                    //disconnect topright corner node and bottomright corner node
                    disconnect(cornernodes[options.topright], cornernodes[options.bottomright]);
                    //disconnect bottomright corner node and bottomleft corner node
                    disconnect(cornernodes[options.bottomright], cornernodes[options.bottomleft]);
                    //connect topleft corner node
                    connect(cornernodes[options.topright], e);
                    //connect topright corner node
                    connect(cornernodes[options.bottomleft], e);
                }
            }

            if (p_location > 0.5 && p_location < 0.75)
            {
                e.setY(cornernodes[options.bottomright].getY());
                tempX = cornernodes[options.bottomright].getX() - Math.Abs(cornernodes[options.bottomleft].getX() - cornernodes[options.bottomright].getX()) * ((p_location - 0.5) / 0.25);
                e.setX(tempX);
                e.setLocation(p_location);
                importantnodes.Add(e);

                //if there is an exterior node on the left find the closest on the left
                tmpl = -1;
                tmplocation = Double.MinValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() >= 0.5 && exteriornodes[i].getLocation() < 0.75)
                    {
                        if (exteriornodes[i].getLocation() < e.getLocation() && exteriornodes[i].getLocation() > tmplocation)
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpl = i;
                        }
                    }
                }

                //if there is an exterior node on the right find the closest on the right
                tmpr = -1;
                tmplocation = Double.MaxValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() > 0.5 && exteriornodes[i].getLocation() <= 0.75)
                    {
                        if (exteriornodes[i].getLocation() > e.getLocation() && exteriornodes[i].getLocation() < tmplocation)
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpr = i;
                        }
                    }
                }

                //there are exterior nodes on the left and on the right on the same corner edge 
                if (tmpl > -1 && tmpr > -1)
                {
                    //disconnect the two connected exterior nodes on the left and on the right
                    disconnect(exteriornodes[tmpl], exteriornodes[tmpr]);
                    //connect with the left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect with the right exterior node
                    connect(exteriornodes[tmpr], e);
                }

                //there is an exterior node on the left but not on the right
                if (tmpl > -1 && tmpr == -1)
                {
                    //disconnect left exterior node with the bottomleft corner node
                    disconnect(exteriornodes[tmpl], cornernodes[options.bottomleft]);
                    //connect left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect to the bottomleft corner node
                    connect(cornernodes[options.bottomleft], e);
                }

                //there is an exterior node on the right but not on the left
                if (tmpl == -1 && tmpr > -1)
                {
                    //disconnect the right exterior node with the bottomright corner node
                    disconnect(exteriornodes[tmpr], cornernodes[options.bottomright]);
                    //connect right exterior node
                    connect(exteriornodes[tmpr], e);
                    //connect to the topright corner node
                    connect(cornernodes[options.bottomright], e);
                }

                //there are no exterior nodes on this corner edge
                if (tmpl == -1 && tmpr == -1)
                {
                    //disconnect topleft corner node and topright corner node
                    disconnect(cornernodes[options.bottomright], cornernodes[options.bottomleft]);
                    //connect bottomright corner node
                    connect(cornernodes[options.bottomright], e);
                    //connect bottomleft corner node
                    connect(cornernodes[options.bottomleft], e);
                }
            }

            if (p_location == 0.75)
            {
                e.setX(cornernodes[options.bottomleft].getX());
                tempY = cornernodes[options.bottomleft].getY() - Math.Abs(cornernodes[options.topleft].getY() - cornernodes[options.bottomleft].getY()) * ((p_location - 0.75) / 0.25);
                e.setY(tempY);
                e.setLocation(p_location);
                importantnodes.Remove(cornernodes[options.bottomleft]);
                importantnodes.Add(e);

                //if there is an exterior node on the left find the closest on the left
                tmpl = -1;
                tmplocation = Double.MinValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() >= 0.5 && exteriornodes[i].getLocation() < 0.75)
                    {
                        if (exteriornodes[i].getLocation() > tmplocation)
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpl = i;
                        }
                    }
                }

                //if there is an exterior node on the right find the closest on the right
                tmpr = -1;
                tmplocation = Double.MaxValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() > 0.75 && exteriornodes[i].getLocation() < 1)
                    {
                        if (exteriornodes[i].getLocation() < tmplocation)
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpr = i;
                        }
                    }
                }
                //Special case if closest on the right is on the top left corner
                if (tmpr == -1)
                {
                    for (int i = 0; i < exteriornodes.Count; i++)
                    {

                        if (exteriornodes[i].getLocation() == 0)
                        {
                            tmpr = i;
                        }
                    }
                }

                //there are exterior nodes on the left and on the right 
                if (tmpl > -1 && tmpr > -1)
                {
                    //disconnect left exterior node with the bottomleft corner node
                    disconnect(exteriornodes[tmpl], cornernodes[options.bottomleft]);
                    //disconnect right exterior node with the bottomleft corner node
                    disconnect(exteriornodes[tmpr], cornernodes[options.bottomleft]);
                    //connect with the left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect with the right exterior node
                    connect(exteriornodes[tmpr], e);
                }

                //there is an exterior node on the left but not on the right
                if (tmpl > -1 && tmpr == -1)
                {
                    //disconnect left exterior node with the bottomleft corner node
                    disconnect(exteriornodes[tmpl], cornernodes[options.bottomleft]);
                    //disconnect bottomleft corner node and topleft corner node
                    disconnect(cornernodes[options.bottomleft], cornernodes[options.topleft]);
                    //connect left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect to the topleft corner node
                    connect(cornernodes[options.topleft], e);
                }

                //there is an exterior node on the right but not on the left
                if (tmpl == -1 && tmpr > -1)
                {
                    //disconnect the right exterior node with the bottomleft corner node
                    disconnect(exteriornodes[tmpr], cornernodes[options.bottomleft]);
                    //disconnect bottomright corner node and bottomleft corner node
                    disconnect(cornernodes[options.bottomright], cornernodes[options.bottomleft]);
                    //connect right exterior node
                    connect(exteriornodes[tmpr], e);
                    //connect to the bottomright corner node
                    connect(cornernodes[options.bottomright], e);
                }

                //there are no exterior nodes on the left corner edge or on the right corner edge
                if (tmpl == -1 && tmpr == -1)
                {
                    //disconnect topleft corner node and bottomleft corner node
                    disconnect(cornernodes[options.topleft], cornernodes[options.bottomleft]);
                    //disconnect bottomright corner node and bottomleft corner node
                    disconnect(cornernodes[options.bottomright], cornernodes[options.bottomleft]);
                    //connect topleft corner node
                    connect(cornernodes[options.topleft], e);
                    //connect bottomright corner node
                    connect(cornernodes[options.bottomright], e);
                }
            }

            if (p_location > 0.75 && p_location < 1)
            {
                e.setX(cornernodes[options.bottomleft].getX());
                tempY = cornernodes[options.bottomleft].getY() - Math.Abs(cornernodes[options.topleft].getY() - cornernodes[options.bottomleft].getY()) * ((p_location - 0.75) / 0.25);
                e.setY(tempY);
                e.setLocation(p_location);
                importantnodes.Add(e);

                //if there is an exterior node on the left find the closest on the left
                tmpl = -1;
                tmplocation = Double.MinValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() >= 0.75 && exteriornodes[i].getLocation() < 1)
                    {
                        if (exteriornodes[i].getLocation() < e.getLocation() && exteriornodes[i].getLocation() > tmplocation)
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpl = i;
                        }
                    }
                }

                //if there is an exterior node on the right find the closest on the right
                tmpr = -1;
                tmplocation = Double.MaxValue;
                for (int i = 0; i < exteriornodes.Count; i++)
                {
                    if (exteriornodes[i].getLocation() > 0.75 && exteriornodes[i].getLocation() < 1)
                    {
                        if (exteriornodes[i].getLocation() > e.getLocation() && exteriornodes[i].getLocation() < tmplocation)
                        {
                            tmplocation = exteriornodes[i].getLocation();
                            tmpr = i;
                        }
                    }
                }

                //Special case if closest on the right is on the top left corner
                if (tmpr == -1)
                {
                    for (int i = 0; i < exteriornodes.Count; i++)
                    {

                        if (exteriornodes[i].getLocation() == 0)
                        {
                            tmpr = i;
                        }
                    }
                }

                //there are exterior nodes on the left and on the right on the same corner edge 
                if (tmpl > -1 && tmpr > -1)
                {
                    //disconnect the two connected exterior nodes on the left and on the right
                    disconnect(exteriornodes[tmpl], exteriornodes[tmpr]);
                    //connect with the left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect with the right exterior node
                    connect(exteriornodes[tmpr], e);
                }

                //there is an exterior node on the left but not on the right
                if (tmpl > -1 && tmpr == -1)
                {
                    //disconnect left exterior node with the topleft corner node
                    disconnect(exteriornodes[tmpl], cornernodes[options.topleft]);
                    //connect left exterior node
                    connect(exteriornodes[tmpl], e);
                    //connect to the topleft corner node
                    connect(cornernodes[options.topleft], e);
                }

                //there is an exterior node on the right but not on the left
                if (tmpl == -1 && tmpr > -1)
                {
                    //disconnect the right exterior node with the bottomleft corner node
                    disconnect(exteriornodes[tmpr], cornernodes[options.bottomleft]);
                    //connect right exterior node
                    connect(exteriornodes[tmpr], e);
                    //connect to the bottomleft corner node
                    connect(cornernodes[options.bottomleft], e);
                }

                //there are no exterior nodes on this corner edge
                if (tmpl == -1 && tmpr == -1)
                {
                    //disconnect bottomleft corner node and topright corner node
                    disconnect(cornernodes[options.bottomleft], cornernodes[options.topleft]);
                    //connect bottomleft corner node
                    connect(cornernodes[options.bottomleft], e);
                    //connect topleft corner node
                    connect(cornernodes[options.topleft], e);
                }
            }
            exteriornodes.Add(e);
        }

        private void addInteriorNode(double p_locationx, double p_locationy)
        {
            node interior = new node(options.interiornode);
            double rangeX = cornernodes[options.topright].getX() - cornernodes[options.topleft].getX();
            double rangeY = cornernodes[options.bottomright].getY() - cornernodes[options.topright].getY();
            interior.setX(cornernodes[options.topleft].getX() + rangeX * p_locationx);
            interior.setY(cornernodes[options.topright].getY() + rangeY * p_locationy);
            interior.setLocationX(p_locationx);
            interior.setLocationY(p_locationy);
            importantnodes.Add(interior);
            interiornodes.Add(interior);
            //Perform connections in warehouse creations such as createwarehouse313, 414, etc
        }

        /// <summary>
        /// Checks if two exterior nodes are on the same edge
        /// </summary>
        /// <param name="e1">First Exterior Node</param>
        /// <param name="e2">Second Exterior Node</param>
        /// <returns>True if on the same edge</returns>
        private bool checkExteriorNodesOnSameEdge(double e1, double e2)
        {
            if (e1 >= 0 && e1 <= 0.25 && e2 >= 0 && e2 <= 0.25)
            {
                return true;
            }
            else if (e1 >= 0.25 && e1 <= 0.5 && e2 >= 0.25 && e2 <= 0.5)
            {
                return true;
            }
            else if (e1 >= 0.5 && e1 <= 0.75 && e2 >= 0.5 && e2 <= 0.75)
            {
                return true;
            }
            else if (e1 >= 0.75 && e1 < 1 && e2 >= 0.75 && e2 < 1)
            {
                return true;
            }
            else if ((e1 >= 0.75 && e2 == 0) || (e2 >= 0.75 && e1 == 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if intersect
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="e2"></param>
        /// <param name="e3"></param>
        /// <param name="e4"></param>
        /// <returns></returns>
        private bool checkCrossAislesIntersectExteriorOnly(double e1, double e2, double e3, double e4)
        {
            if (e1 < e2)
            {
                if ((e3 < e2) && (e3 > e1))//Case when e3 is between e1-e2
                {
                    if ((e4 < e1) || (e4 > e2))
                    {
                        return true;
                    }
                }
                if ((e4 < e2) && (e4 > e1))//Case when e4 is between e1-e2
                {
                    if ((e3 < e1) || (e3 > e2))
                    {
                        return true;
                    }
                }
            }
            if (e2 < e1)
            {
                if ((e3 < e1) && (e3 > e2))//Case when e3 is between e2-e1
                {
                    if ((e4 < e2) || (e4 > e1))//Check if e4 is in correct position
                    {
                        return true;
                    }
                }
                if ((e4 < e1) && (e4 > e2))//Case when e4 is between e2-e1
                {
                    if ((e3 < e2) || (e3 > e1))//Check if e3 is in correct position
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Used for cross aisles with both interior and exterior node
        /// </summary>
        /// <param name="p_edge1"></param>
        /// <param name="p_edge2"></param>
        /// <returns>Returns false when intersects</returns>
        private bool checkCrossAislesIntersectExteriorInterior(edge p_edge1, edge p_edge2)
        {
            if(p_edge1.getEnd() == p_edge2.getStart() || p_edge1.getEnd() == p_edge2.getEnd() ||
                p_edge1.getStart() == p_edge2.getStart() || p_edge1.getStart() == p_edge2.getEnd())
            {
                return true;//This means two edges have one common node
            }
            else if (p_edge1.calculateIntersect(p_edge2).getX() == -1000000)
            {
                //Does not intersect
                return true;
            }
            else
            {
                //Does intersect
                return false;
            }
        }

        public bool createWarehouse(double[] p_angle, double[] p_adjuster, double[] p_pickadjuster, double[] p_e, double[] p_ix, double[] p_iy, double[] p_pd, double p_aspectratio, double p_crossaislewidth, double p_pickingaislewidth, double p_locationwidth, double p_locationdepth, bool[] p_connections)
        {
            this.width = Convert.ToDouble(Math.Sqrt(this.area / aspectratio));
            this.depth = Convert.ToDouble(Math.Sqrt(this.area * aspectratio));
            //Create Edge Cross Aisles
            createCornerEdges();

            double[] ext = new double[p_e.Count()];
            problematicconnections = new bool[p_connections.Count()];
            for (int i = 0; i < p_e.Count(); i++)
            {
                if (p_e[i] == 1)
                {
                    p_e[i] = 0;
                }
                ext[i] = p_e[i];
            }

            //Some fix for region finding algorithm
            //If two exterior nodes are on the same point then the algorithm does not work
            //So we move away the points as small as possible but still does not make affect on cost function
            for (int i = 0; i < ext.Count(); i++)
            {
                for (int j = i + 1; j < ext.Count(); j++)
                {
                    if (j != i)
                    {
                        if (ext[i] == ext[j])
                        {
                            ext[i] = ext[i] + 0.000000001 * (i + 1) * (j + 1);
                        }
                    }
                }
            }

            int k = 0;
            int[] connectioncount = new int[ext.Count() + p_ix.Count()];

            for (int i = 0; i < ext.Count() + p_ix.Count() - 1; i++)
            {
                for (int j = i + 1; j < ext.Count() + p_ix.Count(); j++)
                {
                    if(p_connections[k])
                    {
                        connectioncount[i]++;
                        connectioncount[j]++;
                    }
                    k++;
                }
            }

            //Reset all problematic connections to false
            for (int i = 0; i < problematicconnections.Count(); i++)
            {
                problematicconnections[i] = false;
            }

            for(int i = ext.Count(); i < ext.Count() + p_ix.Count(); i++)
            {
                if (connectioncount[i] == 1)
                {
                    k = 0;
                    for (int l = 0; l < ext.Count() + p_ix.Count() - 1; l++)
                    {
                        for (int j = l + 1; j < ext.Count() + p_ix.Count(); j++)
                        {
                            if (p_connections[k] && (l == i || j == i))
                            {
                                problematicconnections[k] = true;
                                return false;
                            }
                            k++;
                        }
                    }
                }
            }

            for (int i = 0; i < ext.Count() + p_ix.Count(); i++)
            {
                if(connectioncount[i] > 0 && i < ext.Count())//There is a connection for this exteriornode
                {
                    addExteriorNode(ext[i]);
                }
                else if(connectioncount[i] > 0 && i >= ext.Count())//There is a connection for this interiornode
                {
                    addInteriorNode(p_ix[i - ext.Count()], p_iy[i - ext.Count()]);
                }
            }

            k = 0;
            for (int i = 0; i < ext.Count() + p_ix.Count() - 1; i++)
            {
                for(int j = i + 1; j < ext.Count() + p_ix.Count(); j++)
                {
                    if(p_connections[k])
                    {
                        if(i < ext.Count())//then i is exteriornode
                        {
                            if( j < ext.Count())//then j is exteriornode
                            {
                                if (checkExteriorNodesOnSameEdge(ext[i], ext[j]))//Check if design is invalid
                                {
                                    problematicconnections[k] = true;
                                    return false;
                                }
                                else
                                {
                                    int a = -1;
                                    int b = -1;
                                    for(int l = 0; l < exteriornodes.Count(); l++)
                                    {
                                        if (ext[i] == exteriornodes[l].getLocation())
                                            a = l;
                                        if (ext[j] == exteriornodes[l].getLocation())
                                            b = l;
                                    }
                                    connect(exteriornodes[a], exteriornodes[b], k);
                                }
                                
                            }
                            else//then j is interiornode
                            {
                                if (p_connections[k])
                                {
                                    int a = -1;
                                    for (int l = 0; l < exteriornodes.Count(); l++)
                                    {
                                        if (ext[i] == exteriornodes[l].getLocation())
                                            a = l;
                                    }
                                    connect(exteriornodes[a], interiornodes[j - ext.Count()], k);
                                }
                            }
                        }
                        else//then i is interior node
                        {
                            if (j < ext.Count())//then j is exteriornode
                            {
                                if (p_connections[k])
                                connect(interiornodes[i - ext.Count()], exteriornodes[j], k);
                            }
                            else//then j is interiornode
                            {
                                if (p_connections[k])
                                connect(interiornodes[i - ext.Count()], interiornodes[j - ext.Count()], k);
                            }
                        }
                    }
                    k++;
                }
            }

            if (checkRegionEdgesIntersect(p_connections))
            {
                fillRegion(findRegions(p_crossaislewidth, p_pickingaislewidth, p_locationwidth, p_locationdepth), p_angle, p_adjuster, p_pickadjuster);
            }
            else//The design is invalid, return false for infeasible design
            {
                return false;
            }

            //Add picking aisle start and end nodes of all regions to important nodes for shortest path algorithm
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    importantnodes.Add(regions[i].pickingaisleedges[j].getStart());
                    importantnodes.Add(regions[i].pickingaisleedges[j].getEnd());
                }
            }
            //Add P&D point as an exterior node
            //This is a quick solution I need to code a better way of adding PD point
            addPickandDepositNode(p_pd[0]);
            return true;
        }

        /// <summary>
        /// Checks whether any cross aisle segments cross each other
        /// </summary>
        /// <returns></returns>
        private bool checkRegionEdgesIntersect(bool[] p_connections)
        {
            for(int i = 0; i < regionedges.Count(); i++)
            {
                for(int j = 0; j < i; j++)
                {
                    if(!checkCrossAislesIntersectExteriorInterior(regionedges[i], regionedges[j]))//Two segments crosses then return false
                    {
                        problematicconnections[regionedges[i].connectionk] = true;
                        problematicconnections[regionedges[j].connectionk] = true;
                        return false;
                    }
                }
            }
            return true;
        }

        public bool create000Warehouse(double[] p_angle, double[] p_adjuster, double[] p_pickadjuster, double[] p_e, double[] p_pd, double aspectratio, double p_crossaislewidth, double p_pickingaislewidth, double p_locationwidth, double p_locationdepth)
        {
            this.width = Convert.ToDouble(Math.Sqrt(this.area / aspectratio));
            this.depth = Convert.ToDouble(Math.Sqrt(this.area * aspectratio));
            this.aspectratio = aspectratio;
            //Create Edge Cross Aisles
            createCornerEdges();

            //Find regions
            findRegions(p_angle, p_adjuster, p_pickadjuster, p_crossaislewidth, p_pickingaislewidth, p_locationwidth, p_locationdepth, "0-0-0");
            //Add picking aisle start and end nodes of all regions to important nodes for shortest path algorithm
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    importantnodes.Add(regions[i].pickingaisleedges[j].getStart());
                    importantnodes.Add(regions[i].pickingaisleedges[j].getEnd());
                }
            }
            //Add P&D point as an exterior node
            //This is a quick solution I need to code a better way of adding PD point
            addPickandDepositNode(p_pd[0]);
            return true;
        }

        public bool create201Warehouse(double[] p_angle, double[] p_adjuster, double[] p_pickadjuster, double[] p_e, double[] p_pd, double aspectratio, double p_crossaislewidth, double p_pickingaislewidth, double p_locationwidth, double p_locationdepth)
        {
            this.width = Convert.ToDouble(Math.Sqrt(this.area / aspectratio));
            this.depth = Convert.ToDouble(Math.Sqrt(this.area * aspectratio));
            this.aspectratio = aspectratio;
            //Create Edge Cross Aisles
            createCornerEdges();
            double[] ext = new double[p_e.Count()];
            for (int i = 0; i < p_e.Count(); i++)
            {
                if (p_e[i] == 1)
                {
                    p_e[i] = 0;
                }
                ext[i] = p_e[i];
            }

            //Some fix for region finding algorithm
            //If two exterior nodes are on the same point then the algorithm does not work
            //So we move away the points as small as possible but still does not make affect on cost function
            for (int i = 0; i < ext.Count(); i++)
            {
                for (int j = i + 1; j < ext.Count(); j++)
                {
                    if (j != i)
                    {
                        if (ext[i] == ext[j])
                        {
                            ext[i] = ext[i] + 0.000000001 * (i + 1) * (j + 1);
                        }
                    }
                }
            }

            //Check if design is invalid
            if (checkExteriorNodesOnSameEdge(ext[0], ext[1]))
            {
                return false;
            }
            //If it passes from these tests then design is valid start creating it

            addExteriorNode(ext[0]);
            addExteriorNode(ext[1]);
            connect(exteriornodes[0], exteriornodes[1]);//Assuming the design is valid

            //Find regions
            findRegions(p_angle, p_adjuster, p_pickadjuster, p_crossaislewidth, p_pickingaislewidth, p_locationwidth, p_locationdepth, "2-0-1");
            //Add picking aisle start and end nodes of all regions to important nodes for shortest path algorithm
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    importantnodes.Add(regions[i].pickingaisleedges[j].getStart());
                    importantnodes.Add(regions[i].pickingaisleedges[j].getEnd());
                }
            }
            //Add P&D point as an exterior node
            //This is a quick solution I need to code a better way of adding PD point
            addPickandDepositNode(p_pd[0]);
            return true;
        }

        /// <summary>
        /// Create a 303 Design Class
        /// </summary>
        /// <param name="p_angle"></param>
        /// <param name="p_adjuster"></param>
        /// <param name="p_pickadjuster"></param>
        /// <param name="p_e"></param>
        /// <param name="p_pd"></param>
        /// <param name="aspectratio"></param>
        /// <param name="p_crossaislewidth"></param>
        /// <param name="p_pickingaislewidth"></param>
        /// <param name="p_locationwidth"></param>
        /// <param name="p_locationdepth"></param>
        public bool create302Warehouse(double[] p_angle, double[] p_adjuster, double[] p_pickadjuster, double[] p_e, double[] p_pd, double aspectratio, double p_crossaislewidth, double p_pickingaislewidth, double p_locationwidth, double p_locationdepth)
        {
            this.width = Convert.ToDouble(Math.Sqrt(this.area / aspectratio));
            this.depth = Convert.ToDouble(Math.Sqrt(this.area * aspectratio));
            this.aspectratio = aspectratio;
            //Create Edge Cross Aisles
            createCornerEdges();
            double[] ext = new double[p_e.Count()];
            for (int i = 0; i < p_e.Count(); i++)
            {
                if (p_e[i] == 1)
                {
                    p_e[i] = 0;
                }
                ext[i] = p_e[i];
            }

            //Some fix for region finding algorithm
            //If two exterior nodes are on the same point then the algorithm does not work
            //So we move away the points as small as possible but still does not make affect on cost function
            for (int i = 0; i < ext.Count(); i++)
            {
                for (int j = i + 1; j < ext.Count(); j++)
                {
                    if (j != i)
                    {
                        if (ext[i] == ext[j])
                        {
                            ext[i] = ext[i] + 0.000000001 * (i + 1) * (j + 1);
                        }
                    }
                }
            }

            //Check if design is invalid
            if (checkExteriorNodesOnSameEdge(ext[0], ext[2]))
            {
                return false;
            }
            if (checkExteriorNodesOnSameEdge(ext[1], ext[2]))
            {
                return false;
            }
            //If it passes from these tests then design is valid start creating it

            addExteriorNode(ext[0]);
            addExteriorNode(ext[1]);
            addExteriorNode(ext[2]);
            connect(exteriornodes[0], exteriornodes[2]);
            connect(exteriornodes[1], exteriornodes[2]);


            //Find regions
            findRegions(p_angle, p_adjuster, p_pickadjuster, p_crossaislewidth, p_pickingaislewidth, p_locationwidth, p_locationdepth, "3-0-2");
            //Add picking aisle start and end nodes of all regions to important nodes for shortest path algorithm
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    importantnodes.Add(regions[i].pickingaisleedges[j].getStart());
                    importantnodes.Add(regions[i].pickingaisleedges[j].getEnd());
                }
            }
            //Add P&D point as an exterior node
            //This is a quick solution I need to code a better way of adding PD point
            addPickandDepositNode(p_pd[0]);
            return true;
        }

        /// <summary>
        /// Create a 303 Warehouse
        /// </summary>
        /// <param name="p_angle"></param>
        /// <param name="p_adjuster"></param>
        /// <param name="p_pickadjuster"></param>
        /// <param name="p_e"></param>
        /// <param name="p_pd"></param>
        /// <param name="aspectratio"></param>
        /// <param name="p_crossaislewidth"></param>
        /// <param name="p_pickingaislewidth"></param>
        /// <param name="p_locationwidth"></param>
        /// <param name="p_locationdepth"></param>
        public bool create303Warehouse(double[] p_angle, double[] p_adjuster, double[] p_pickadjuster, double[] p_e, double[] p_pd, double aspectratio, double p_crossaislewidth, double p_pickingaislewidth, double p_locationwidth, double p_locationdepth)
        {
            this.width = Convert.ToDouble(Math.Sqrt(this.area / aspectratio));
            this.depth = Convert.ToDouble(Math.Sqrt(this.area * aspectratio));
            this.aspectratio = aspectratio;
            //Create Edge Cross Aisles
            createCornerEdges();
            double[] ext = new double[p_e.Count()];
            for (int i = 0; i < p_e.Count(); i++)
            {
                if (p_e[i] == 1)
                {
                    p_e[i] = 0;
                }
                ext[i] = p_e[i];
            }

            //Some fix for region finding algorithm
            //If two exterior nodes are on the same point then the algorithm does not work
            //So we move away the points as small as possible but still does not make affect on cost function
            for (int i = 0; i < ext.Count(); i++)
            {
                for (int j = i + 1; j < ext.Count(); j++)
                {
                    if (j != i)
                    {
                        if (ext[i] == ext[j])
                        {
                            ext[i] = ext[i] + 0.000000001 * (i + 1) * (j + 1);
                        }
                    }
                }
            }

            //Check if design is invalid
            if (checkExteriorNodesOnSameEdge(ext[0], ext[2]))
            {
                return false;
            }
            if (checkExteriorNodesOnSameEdge(ext[1], ext[2]))
            {
                return false;
            }
            if (checkExteriorNodesOnSameEdge(ext[0], ext[1]))
            {
                return false;
            }
            //If it passes from these tests then design is valid start creating it

            addExteriorNode(ext[0]);
            addExteriorNode(ext[1]);
            addExteriorNode(ext[2]);
            connect(exteriornodes[0], exteriornodes[2]);
            connect(exteriornodes[1], exteriornodes[2]);
            connect(exteriornodes[0], exteriornodes[1]);

            //Find regions
            findRegions(p_angle, p_adjuster, p_pickadjuster, p_crossaislewidth, p_pickingaislewidth, p_locationwidth, p_locationdepth, "3-0-3");
            //Add picking aisle start and end nodes of all regions to important nodes for shortest path algorithm
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    importantnodes.Add(regions[i].pickingaisleedges[j].getStart());
                    importantnodes.Add(regions[i].pickingaisleedges[j].getEnd());
                }
            }
            //Add P&D point as an exterior node
            //This is a quick solution I need to code a better way of adding PD point
            addPickandDepositNode(p_pd[0]);
            return true;
        }

        /// <summary>
        /// Create a 313 Warehouse
        /// </summary>
        /// <param name="p_angle"></param>
        /// <param name="p_adjuster"></param>
        /// <param name="p_pickadjuster"></param>
        /// <param name="p_e"></param>
        /// <param name="p_ix"></param>
        /// <param name="p_iy"></param>
        /// <param name="p_pd"></param>
        /// <param name="aspectratio"></param>
        /// <param name="p_crossaislewidth"></param>
        /// <param name="p_pickingaislewidth"></param>
        /// <param name="p_locationwidth"></param>
        /// <param name="p_locationdepth"></param>
        public bool create313Warehouse(double[] p_angle, double[] p_adjuster, double[] p_pickadjuster, double[] p_e, double[] p_ix, double[] p_iy, double[] p_pd, double aspectratio, double p_crossaislewidth, double p_pickingaislewidth, double p_locationwidth, double p_locationdepth)
        {
            this.width = Convert.ToDouble(Math.Sqrt(this.area / aspectratio));
            this.depth = Convert.ToDouble(Math.Sqrt(this.area * aspectratio));
            this.aspectratio = aspectratio;
            //Create Edge Cross Aisles
            createCornerEdges();
            double[] ext = new double[p_e.Count()];
            double[] ix = new double[p_ix.Count()];
            double[] iy = new double[p_iy.Count()];
            for (int i = 0; i < p_e.Count(); i++)
            {
                if (p_e[i] == 1)
                {
                    p_e[i] = 0;
                }
                ext[i] = p_e[i];
            }

            for (int i = 0; i < p_ix.Count(); i++)
            {
                ix[i] = p_ix[i];
            }
            for (int i = 0; i < p_iy.Count(); i++)
            {
                iy[i] = p_iy[i];
            }
            //Some fix for region finding algorithm
            //If two exterior nodes are on the same point then the algorithm does not work
            //So we move away the points as small as possible but still does not make affect on cost function
            for (int i = 0; i < ext.Count(); i++)
            {
                for (int j = i + 1; j < ext.Count(); j++)
                {
                    if (j != i)
                    {
                        if (ext[i] == ext[j])
                        {
                            ext[i] = ext[i] + 0.000000001 * (i + 1) * (j + 1);
                        }
                    }
                }
            }

            //Check if the design is invalid
            if (ix[0] <= 0 || ix[0] >= 1 || iy[0] <= 0 || iy[0] >= 1)
            {
                return false;
            }

            addExteriorNode(ext[0]);
            addExteriorNode(ext[1]);
            addExteriorNode(ext[2]);
            addInteriorNode(ix[0], iy[0]);

            connect(exteriornodes[0], interiornodes[0]);
            connect(exteriornodes[1], interiornodes[0]);
            connect(exteriornodes[2], interiornodes[0]);

            //Find regions
            findRegions(p_angle, p_adjuster, p_pickadjuster, p_crossaislewidth, p_pickingaislewidth, p_locationwidth, p_locationdepth, "3-1-3");
            //Add picking aisle start and end nodes of all regions to important nodes for shortest path algorithm
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    importantnodes.Add(regions[i].pickingaisleedges[j].getStart());
                    importantnodes.Add(regions[i].pickingaisleedges[j].getEnd());
                }
            }
            //Add P&D point as an exterior node
            //This is a quick solution I need to code a better way of adding PD point
            addPickandDepositNode(p_pd[0]);
            return true;
        }

        /// <summary>
        /// Create a 402 Design Class
        /// </summary>
        /// <param name="p_angle"></param>
        /// <param name="p_adjuster"></param>
        /// <param name="p_pickadjuster"></param>
        /// <param name="p_e"></param>
        /// <param name="p_pd"></param>
        /// <param name="aspectratio"></param>
        /// <param name="p_crossaislewidth"></param>
        /// <param name="p_pickingaislewidth"></param>
        /// <param name="p_locationwidth"></param>
        /// <param name="p_locationdepth"></param>
        public bool create402Warehouse(double[] p_angle, double[] p_adjuster, double[] p_pickadjuster, double[] p_e, double[] p_pd, double aspectratio, double p_crossaislewidth, double p_pickingaislewidth, double p_locationwidth, double p_locationdepth)
        {
            this.width = Convert.ToDouble(Math.Sqrt(this.area / aspectratio));
            this.depth = Convert.ToDouble(Math.Sqrt(this.area * aspectratio));
            this.aspectratio = aspectratio;
            //Create Edge Cross Aisles
            createCornerEdges();
            double[] ext = new double[p_e.Count()];
            for (int i = 0; i < p_e.Count(); i++)
            {
                if (p_e[i] == 1)
                {
                    p_e[i] = 0;
                }
                ext[i] = p_e[i];
            }

            //Some fix for region finding algorithm
            //If two exterior nodes are on the same point then the algorithm does not work
            //So we move away the points as small as possible but still does not make affect on cost function
            for (int i = 0; i < ext.Count(); i++)
            {
                for (int j = i + 1; j < ext.Count(); j++)
                {
                    if (j != i)
                    {
                        if (ext[i] == ext[j])
                        {
                            ext[i] = ext[i] + 0.000000001 * (i + 1) * (j + 1);
                        }
                    }
                }
            }

            //Check if design is invalid
            if (checkExteriorNodesOnSameEdge(ext[0], ext[1]))
            {
                return false;
            }
            if (checkExteriorNodesOnSameEdge(ext[2], ext[3]))
            {
                return false;
            }
            //Check the order of exterior nodes for non-crossing cross aisles
            if (checkCrossAislesIntersectExteriorOnly(ext[0], ext[1], ext[2], ext[3]))
            {
                return false; //they intersect
            }
            //If it passes from these tests then design is valid start creating it

            addExteriorNode(ext[0]);
            addExteriorNode(ext[1]);
            addExteriorNode(ext[2]);
            addExteriorNode(ext[3]);

            connect(exteriornodes[0], exteriornodes[1]);
            connect(exteriornodes[2], exteriornodes[3]);

            //Find regions
            findRegions(p_angle, p_adjuster, p_pickadjuster, p_crossaislewidth, p_pickingaislewidth, p_locationwidth, p_locationdepth, "4-0-2");
            //Add picking aisle start and end nodes of all regions to important nodes for shortest path algorithm
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    importantnodes.Add(regions[i].pickingaisleedges[j].getStart());
                    importantnodes.Add(regions[i].pickingaisleedges[j].getEnd());
                }
            }
            //Add P&D point as an exterior node
            //This is a quick solution I need to code a better way of adding PD point
            addPickandDepositNode(p_pd[0]);
            return true;
        }

        /// <summary>
        /// Create a 404 Design Class
        /// </summary>
        /// <param name="p_angle"></param>
        /// <param name="p_adjuster"></param>
        /// <param name="p_pickadjuster"></param>
        /// <param name="p_e"></param>
        /// <param name="p_pd"></param>
        /// <param name="aspectratio"></param>
        /// <param name="p_crossaislewidth"></param>
        /// <param name="p_pickingaislewidth"></param>
        /// <param name="p_locationwidth"></param>
        /// <param name="p_locationdepth"></param>
        public bool create404Warehouse(double[] p_angle, double[] p_adjuster, double[] p_pickadjuster, double[] p_e, double[] p_pd, double aspectratio, double p_crossaislewidth, double p_pickingaislewidth, double p_locationwidth, double p_locationdepth)
        {
            this.width = Convert.ToDouble(Math.Sqrt(this.area / aspectratio));
            this.depth = Convert.ToDouble(Math.Sqrt(this.area * aspectratio));
            this.aspectratio = aspectratio;
            //Create Edge Cross Aisles
            createCornerEdges();
            double[] ext = new double[p_e.Count()];
            for (int i = 0; i < p_e.Count(); i++)
            {
                if (p_e[i] == 1)
                {
                    p_e[i] = 0;
                }
                ext[i] = p_e[i];
            }

            //Some fix for region finding algorithm
            //If two exterior nodes are on the same point then the algorithm does not work
            //So we move away the points as small as possible but still does not make affect on cost function
            for (int i = 0; i < ext.Count(); i++)
            {
                for (int j = i + 1; j < ext.Count(); j++)
                {
                    if (j != i)
                    {
                        if (ext[i] == ext[j])
                        {
                            ext[i] = ext[i] + 0.000000001 * (i + 1) * (j + 1);
                        }
                    }
                }
            }

            //Check if design is invalid
            //Check none of the exterior nodes are not on the same edge
            if (checkExteriorNodesOnSameEdge(ext[0], ext[1])
                || checkExteriorNodesOnSameEdge(ext[0], ext[2])
                || checkExteriorNodesOnSameEdge(ext[0], ext[3])
                || checkExteriorNodesOnSameEdge(ext[1], ext[2])
                || checkExteriorNodesOnSameEdge(ext[1], ext[3])
                || checkExteriorNodesOnSameEdge(ext[2], ext[3])
                )
            {
                return false;
            }
            //Check the order of exterior nodes for non-crossing cross aisles
            if (checkCrossAislesIntersectExteriorOnly(ext[0], ext[1], ext[2], ext[3]))
            {
                return false; //they intersect
            }
            if (checkCrossAislesIntersectExteriorOnly(ext[1], ext[2], ext[3], ext[0]))
            {
                return false; //they intersect
            }
            if (checkCrossAislesIntersectExteriorOnly(ext[2], ext[3], ext[0], ext[1]))
            {
                return false; //they intersect
            }
            if (checkCrossAislesIntersectExteriorOnly(ext[3], ext[0], ext[1], ext[2]))
            {
                return false; //they intersect
            }
            //If it passes from these tests then design is valid start creating it

            addExteriorNode(ext[0]);
            addExteriorNode(ext[1]);
            addExteriorNode(ext[2]);
            addExteriorNode(ext[3]);

            connect(exteriornodes[0], exteriornodes[1]);
            connect(exteriornodes[1], exteriornodes[2]);
            connect(exteriornodes[2], exteriornodes[3]);
            connect(exteriornodes[3], exteriornodes[0]);

            //Find regions
            findRegions(p_angle, p_adjuster, p_pickadjuster, p_crossaislewidth, p_pickingaislewidth, p_locationwidth, p_locationdepth, "4-0-4");
            //Add picking aisle start and end nodes of all regions to important nodes for shortest path algorithm
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    importantnodes.Add(regions[i].pickingaisleedges[j].getStart());
                    importantnodes.Add(regions[i].pickingaisleedges[j].getEnd());
                }
            }
            //Add P&D point as an exterior node
            //This is a quick solution I need to code a better way of adding PD point
            addPickandDepositNode(p_pd[0]);
            return true;
        }

        public bool create405Warehouse(double[] p_angle, double[] p_adjuster, double[] p_pickadjuster, double[] p_e, double[] p_pd, double aspectratio, double p_crossaislewidth, double p_pickingaislewidth, double p_locationwidth, double p_locationdepth)
        {
            this.width = Convert.ToDouble(Math.Sqrt(this.area / aspectratio));
            this.depth = Convert.ToDouble(Math.Sqrt(this.area * aspectratio));
            this.aspectratio = aspectratio;
            //Create Edge Cross Aisles
            createCornerEdges();
            double[] ext = new double[p_e.Count()];
            for (int i = 0; i < p_e.Count(); i++)
            {
                if (p_e[i] == 1)
                {
                    p_e[i] = 0;
                }
                ext[i] = p_e[i];
            }

            //Some fix for region finding algorithm
            //If two exterior nodes are on the same point then the algorithm does not work
            //So we move away the points as small as possible but still does not make affect on cost function
            for (int i = 0; i < ext.Count(); i++)
            {
                for (int j = i + 1; j < ext.Count(); j++)
                {
                    if (j != i)
                    {
                        if (ext[i] == ext[j])
                        {
                            ext[i] = ext[i] + 0.000000001 * (i + 1) * (j + 1);
                        }
                    }
                }
            }

            //Check if design is invalid
            //Check none of the exterior nodes are not on the same edge
            if (checkExteriorNodesOnSameEdge(ext[0], ext[1])
                || checkExteriorNodesOnSameEdge(ext[0], ext[2])
                || checkExteriorNodesOnSameEdge(ext[0], ext[3])
                || checkExteriorNodesOnSameEdge(ext[1], ext[2])
                || checkExteriorNodesOnSameEdge(ext[1], ext[3])
                || checkExteriorNodesOnSameEdge(ext[2], ext[3])
                )
            {
                return false;
            }
            //Check the order of exterior nodes for non-crossing cross aisles
            if (checkCrossAislesIntersectExteriorOnly(ext[0], ext[1], ext[2], ext[3]))
            {
                return false; //they intersect
            }
            if (checkCrossAislesIntersectExteriorOnly(ext[1], ext[2], ext[3], ext[0]))
            {
                return false; //they intersect
            }
            if (checkCrossAislesIntersectExteriorOnly(ext[2], ext[3], ext[0], ext[1]))
            {
                return false; //they intersect
            }
            if (checkCrossAislesIntersectExteriorOnly(ext[3], ext[0], ext[1], ext[2]))
            {
                return false; //they intersect
            }
            //If it passes from these tests then design is valid start creating it

            addExteriorNode(ext[0]);
            addExteriorNode(ext[1]);
            addExteriorNode(ext[2]);
            addExteriorNode(ext[3]);
            if (!checkExteriorNodesOnSameEdge(ext[0], ext[1]))
            {
                connect(exteriornodes[0], exteriornodes[1]);
            }
            if (!checkExteriorNodesOnSameEdge(ext[1], ext[2]))
            {
                connect(exteriornodes[1], exteriornodes[2]);
            }
            if (!checkExteriorNodesOnSameEdge(ext[2], ext[3]))
            {
                connect(exteriornodes[2], exteriornodes[3]);
            }
            if (!checkExteriorNodesOnSameEdge(ext[3], ext[0]))
            {
                connect(exteriornodes[3], exteriornodes[0]);
            }
            if (!checkExteriorNodesOnSameEdge(ext[0], ext[2]))
            {
                connect(exteriornodes[0], exteriornodes[2]);
            }

            //Find regions
            findRegions(p_angle, p_adjuster, p_pickadjuster, p_crossaislewidth, p_pickingaislewidth, p_locationwidth, p_locationdepth, "4-0-5");
            //Add picking aisle start and end nodes of all regions to important nodes for shortest path algorithm
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    importantnodes.Add(regions[i].pickingaisleedges[j].getStart());
                    importantnodes.Add(regions[i].pickingaisleedges[j].getEnd());
                }
            }
            //Add P&D point as an exterior node
            //This is a quick solution I need to code a better way of adding PD point
            addPickandDepositNode(p_pd[0]);
            return true;
        }

        public bool create414Warehouse(double[] p_angle, double[] p_adjuster, double[] p_pickadjuster, double[] p_e, double[] p_ix, double[] p_iy, double[] p_pd, double aspectratio, double p_crossaislewidth, double p_pickingaislewidth, double p_locationwidth, double p_locationdepth)
        {
            this.width = Convert.ToDouble(Math.Sqrt(this.area / aspectratio));
            this.depth = Convert.ToDouble(Math.Sqrt(this.area * aspectratio));
            this.aspectratio = aspectratio;
            //Create Edge Cross Aisles
            createCornerEdges();
            double[] ext = new double[p_e.Count()];
            double[] ix = new double[p_ix.Count()];
            double[] iy = new double[p_iy.Count()];
            for (int i = 0; i < p_e.Count(); i++)
            {
                if (p_e[i] == 1)
                {
                    p_e[i] = 0;
                }
                ext[i] = p_e[i];
            }

            for (int i = 0; i < p_ix.Count(); i++)
            {
                ix[i] = p_ix[i];
            }
            for (int i = 0; i < p_iy.Count(); i++)
            {
                iy[i] = p_iy[i];
            }
            //Some fix for region finding algorithm
            //If two exterior nodes are on the same point then the algorithm does not work
            //So we move away the points as small as possible but still does not make affect on cost function
            for (int i = 0; i < ext.Count(); i++)
            {
                for (int j = i + 1; j < ext.Count(); j++)
                {
                    if (j != i)
                    {
                        if (ext[i] == ext[j])
                        {
                            ext[i] = ext[i] + 0.000000001 * (i + 1) * (j + 1);
                        }
                    }
                }
            }

            //Check if the design is invalid
            if (ix[0] <= 0 || ix[0] >= 1 || iy[0] <= 0 || iy[0] >= 1)
            {
                return false;
            }

            addExteriorNode(ext[0]);
            addExteriorNode(ext[1]);
            addExteriorNode(ext[2]);
            addExteriorNode(ext[3]);
            addInteriorNode(ix[0], iy[0]);

            connect(exteriornodes[0], interiornodes[0]);
            connect(exteriornodes[1], interiornodes[0]);
            connect(exteriornodes[2], interiornodes[0]);
            connect(exteriornodes[3], interiornodes[0]);

            //Find regions
            findRegions(p_angle, p_adjuster, p_pickadjuster, p_crossaislewidth, p_pickingaislewidth, p_locationwidth, p_locationdepth, "4-1-4");
            //Add picking aisle start and end nodes of all regions to important nodes for shortest path algorithm
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    importantnodes.Add(regions[i].pickingaisleedges[j].getStart());
                    importantnodes.Add(regions[i].pickingaisleedges[j].getEnd());
                }
            }
            //Add P&D point as an exterior node
            //This is a quick solution I need to code a better way of adding PD point
            addPickandDepositNode(p_pd[0]);
            return true;
        }

        public bool create425Warehouse(double[] p_angle, double[] p_adjuster, double[] p_pickadjuster, double[] p_e, double[] p_ix, double[] p_iy, double[] p_pd, double aspectratio, double p_crossaislewidth, double p_pickingaislewidth, double p_locationwidth, double p_locationdepth)
        {
            this.width = Convert.ToDouble(Math.Sqrt(this.area / aspectratio));
            this.depth = Convert.ToDouble(Math.Sqrt(this.area * aspectratio));
            this.aspectratio = aspectratio;
            //Create Edge Cross Aisles
            createCornerEdges();
            double[] ext = new double[p_e.Count()];
            double[] ix = new double[p_ix.Count()];
            double[] iy = new double[p_iy.Count()];
            for (int i = 0; i < p_e.Count(); i++)
            {
                if (p_e[i] == 1)
                {
                    p_e[i] = 0;
                }
                ext[i] = p_e[i];
            }

            for (int i = 0; i < p_ix.Count(); i++)
            {
                ix[i] = p_ix[i];
            }
            for (int i = 0; i < p_iy.Count(); i++)
            {
                iy[i] = p_iy[i];
            }
            //Some fix for region finding algorithm
            //If two exterior nodes are on the same point then the algorithm does not work
            //So we move away the points as small as possible but still does not make affect on cost function
            for (int i = 0; i < ext.Count(); i++)
            {
                for (int j = i + 1; j < ext.Count(); j++)
                {
                    if (j != i)
                    {
                        if (ext[i] == ext[j])
                        {
                            ext[i] = ext[i] + 0.000000001 * (i + 1) * (j + 1);
                        }
                    }
                }
            }

            //Check if the design is invalid
            if (ix[0] <= 0 || ix[0] >= 1 || iy[0] <= 0 || iy[0] >= 1)
            {
                return false;
            }
            if (ix[1] <= 0 || ix[1] >= 1 || iy[1] <= 0 || iy[1] >= 1)
            {
                return false;
            }


            addExteriorNode(ext[0]);
            addExteriorNode(ext[1]);
            addExteriorNode(ext[2]);
            addExteriorNode(ext[3]);
            addInteriorNode(ix[0], iy[0]);
            addInteriorNode(ix[1], iy[1]);

            edge edge1 = connect(exteriornodes[0], interiornodes[0]);
            edge edge2 = connect(exteriornodes[1], interiornodes[1]);
            edge edge3 = connect(exteriornodes[2], interiornodes[1]);
            edge edge4 = connect(exteriornodes[3], interiornodes[0]);
            edge edge5 = connect(interiornodes[0], interiornodes[1]);

            if (checkCrossAislesIntersectExteriorInterior(edge1, edge2))
            {
                return false;
            }
            if (checkCrossAislesIntersectExteriorInterior(edge1, edge3))
            {
                return false;
            }
            if (checkCrossAislesIntersectExteriorInterior(edge2, edge4))
            {
                return false;
            }
            if (checkCrossAislesIntersectExteriorInterior(edge3, edge4))
            {
                return false;
            }

            //Find regions
            findRegions(p_angle, p_adjuster, p_pickadjuster, p_crossaislewidth, p_pickingaislewidth, p_locationwidth, p_locationdepth, "4-2-5");
            //Add picking aisle start and end nodes of all regions to important nodes for shortest path algorithm
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    importantnodes.Add(regions[i].pickingaisleedges[j].getStart());
                    importantnodes.Add(regions[i].pickingaisleedges[j].getEnd());
                }
            }
            //Add P&D point as an exterior node
            //This is a quick solution I need to code a better way of adding PD point
            addPickandDepositNode(p_pd[0]);
            return true;
        }

        public bool create603Warehouse(double[] p_angle, double[] p_adjuster, double[] p_pickadjuster, double[] p_e, double[] p_pd, double aspectratio, double p_crossaislewidth, double p_pickingaislewidth, double p_locationwidth, double p_locationdepth)
        {
            this.width = Convert.ToDouble(Math.Sqrt(this.area / aspectratio));
            this.depth = Convert.ToDouble(Math.Sqrt(this.area * aspectratio));
            this.aspectratio = aspectratio;
            //Create Edge Cross Aisles
            createCornerEdges();
            double[] ext = new double[p_e.Count()];
            for (int i = 0; i < p_e.Count(); i++)
            {
                if (p_e[i] == 1)
                {
                    p_e[i] = 0;
                }
                ext[i] = p_e[i];
            }

            //Some fix for region finding algorithm
            //If two exterior nodes are on the same point then the algorithm does not work
            //So we move away the points as small as possible but still does not make affect on cost function
            for (int i = 0; i < ext.Count(); i++)
            {
                for (int j = i + 1; j < ext.Count(); j++)
                {
                    if (j != i)
                    {
                        if (ext[i] == ext[j])
                        {
                            ext[i] = ext[i] + 0.000000001 * (i + 1) * (j + 1);
                        }
                    }
                }
            }

            //Check if design is invalid
            if (checkExteriorNodesOnSameEdge(ext[0], ext[1]))
            {
                return false;
            }
            if (checkExteriorNodesOnSameEdge(ext[2], ext[3]))
            {
                return false;
            }
            if (checkExteriorNodesOnSameEdge(ext[4], ext[5]))
            {
                return false;
            }
            //Check the order of exterior nodes for non-crossing cross aisles
            if (checkCrossAislesIntersectExteriorOnly(ext[0], ext[1], ext[2], ext[3]))
            {
                return false; //they intersect
            }
            if (checkCrossAislesIntersectExteriorOnly(ext[0], ext[1], ext[4], ext[5]))
            {
                return false; //they intersect
            }
            if (checkCrossAislesIntersectExteriorOnly(ext[2], ext[3], ext[4], ext[5]))
            {
                return false; //they intersect
            }
            //If it passes from these tests then design is valid start creating it

            addExteriorNode(ext[0]);
            addExteriorNode(ext[1]);
            addExteriorNode(ext[2]);
            addExteriorNode(ext[3]);
            addExteriorNode(ext[4]);
            addExteriorNode(ext[5]);

            connect(exteriornodes[0], exteriornodes[1]);
            connect(exteriornodes[2], exteriornodes[3]);
            connect(exteriornodes[4], exteriornodes[5]);

            //Find regions
            findRegions(p_angle, p_adjuster, p_pickadjuster, p_crossaislewidth, p_pickingaislewidth, p_locationwidth, p_locationdepth, "6-0-3");
            //Add picking aisle start and end nodes of all regions to important nodes for shortest path algorithm
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    importantnodes.Add(regions[i].pickingaisleedges[j].getStart());
                    importantnodes.Add(regions[i].pickingaisleedges[j].getEnd());
                }
            }
            //Add P&D point as an exterior node
            //This is a quick solution I need to code a better way of adding PD point
            addPickandDepositNode(p_pd[0]);
            return true;
        }

        //This is generic find region without giving the design classes
        public List<region> findRegions(double p_crossaislewidth, double p_pickingaislewidth, double p_locationwidth, double p_locationdepth)
        {
            List<edge> edgepositive = new List<edge>();
            List<edge> edgenegative = new List<edge>();
            for (int i = 0; i < regionedges.Count; i++)
            {
                edgepositive.Add(regionedges[i]);
                edge tempedge1 = new edge(regionedges[i].getEnd(), regionedges[i].getStart(), options.regionedge);
                edgenegative.Add(tempedge1);
                regionedges[i].findEdgeRegion();//Used to calculate if storage location is inside a cross aisle
            }

            List<edge> tempedge = new List<edge>();
            List<region> tempregionlist = new List<region>();
            for (int i = 0; i < regionedges.Count; i++)
            {
                tempedge.Add(edgepositive[i]);
            }
            for (int i = 0; i < regionedges.Count; i++)
            {
                tempedge.Add(edgenegative[i]);
            }
            int position = 0;

            while (tempedge.Count > 0)
            {
                edge currentedge = tempedge[position];
                bool rightmostturn = true;
                bool nextturn = true;
                for (int i = 0; i < regionedges.Count; i++)
                {
                    if (isNegateEdge(currentedge, regionedges[i]))
                    {
                        currentedge = regionedges[i];
                        rightmostturn = false;
                        break;
                    }
                }

                region tempregion = new region(0, p_crossaislewidth, p_pickingaislewidth, p_locationwidth, p_locationdepth, 0, 0);
                edge startingedge = currentedge;

                do
                {
                    //add region edge to the region
                    tempregion.addRegionEdge(currentedge);
                    //Remove current edge from the non visited list
                    for (int i = 0; i < tempedge.Count; i++)
                    {
                        if (rightmostturn == true)
                        {
                            if (tempedge[i].id == currentedge.id)
                            {
                                tempedge.RemoveAt(i);
                                break;
                            }
                        }
                        if (rightmostturn == false)
                        {
                            if (isNegateEdge(tempedge[i], currentedge))
                            {
                                tempedge.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    //decide rightmost edge direction
                    if (currentedge.rightmost(rightmostturn).getEnd() == currentedge.getEnd() || currentedge.rightmost(rightmostturn).getEnd() == currentedge.getStart())
                    {
                        nextturn = false;
                    }
                    else
                    {
                        nextturn = true;
                    }
                    currentedge = currentedge.rightmost(rightmostturn);
                    rightmostturn = nextturn;
                } while (currentedge != startingedge);
                tempregionlist.Add(tempregion);
            }
            //Remove the outer region by finding the largest area (which will be the outer region)
            //Region finding also returns outer region which is an extra region we do not need
            //We can use this outer region's edges to name other regions in clockwise manner
            double maxregionarea = 0;
            int removeregionposition = -1;
            for (int i = 0; i < tempregionlist.Count; i++)
            {
                double currentregionarea = tempregionlist[i].getArea();
                if (currentregionarea > maxregionarea)
                {
                    removeregionposition = i;
                    maxregionarea = currentregionarea;
                }
            }
            tempregionlist.RemoveAt(removeregionposition);//Remove the ones that are connected to outside region
            return tempregionlist;
        }

        private void fillRegion(List<region> tempregionlist, double[] p_angle, double[] p_adjuster, double[] p_pickadjuster)
        {
            for (int i = 0; i < tempregionlist.Count; i++)
            {
                tempregionlist[i].setRegionAngle(p_angle[i]);
                tempregionlist[i].setHorizontalAdjuster(p_adjuster[i]);
                tempregionlist[i].setVerticalAdjuster(p_pickadjuster[i]);
                tempregionlist[i].fill();
                addRegion(tempregionlist[i]);
            }

            //Connect each picking aisle node on the same region edge with next one and
            //if they are the first and last connect them with region edge beginning and end
            for (int i = 0; i < regionedges.Count; i++)
            {
                //if there is only one picking aisle node on this region edge
                //connect both end and beginning of region edge to this picking aisle node
                if (regionedges[i].getOnEdgeNodes().Count == 1)
                {
                    connect(regionedges[i].getStart(), regionedges[i].getOnEdgeNodes()[0]);
                    connect(regionedges[i].getEnd(), regionedges[i].getOnEdgeNodes()[0]);
                }
                //if there are two picking aisle nodes on this region edge
                //this is also the special case
                if (regionedges[i].getOnEdgeNodes().Count == 2)
                {
                    if (visualmath.distance(regionedges[i].getOnEdgeNodes()[0].getX(), regionedges[i].getOnEdgeNodes()[0].getY(),
                                regionedges[i].getStart().getX(), regionedges[i].getStart().getY()) <
                                visualmath.distance(regionedges[i].getOnEdgeNodes()[1].getX(), regionedges[i].getOnEdgeNodes()[1].getY(),
                                regionedges[i].getStart().getX(), regionedges[i].getStart().getY()))
                    {
                        connect(regionedges[i].getStart(), regionedges[i].getOnEdgeNodes()[0]);
                        connect(regionedges[i].getOnEdgeNodes()[0], regionedges[i].getOnEdgeNodes()[1]);
                        connect(regionedges[i].getEnd(), regionedges[i].getOnEdgeNodes()[1]);
                    }
                    else
                    {
                        connect(regionedges[i].getStart(), regionedges[i].getOnEdgeNodes()[1]);
                        connect(regionedges[i].getOnEdgeNodes()[0], regionedges[i].getOnEdgeNodes()[1]);
                        connect(regionedges[i].getEnd(), regionedges[i].getOnEdgeNodes()[0]);
                    }
                }
                if (regionedges[i].getOnEdgeNodes().Count > 2)
                {
                    List<node> unsettlednodes = new List<node>();
                    List<node> settlednodes = new List<node>();
                    double tempdist;
                    for (int j = 0; j < regionedges[i].getOnEdgeNodes().Count; j++)
                    {
                        unsettlednodes.Add(regionedges[i].getOnEdgeNodes()[j]);
                    }
                    int c = 0;
                    while (unsettlednodes.Count > 0)
                    {
                        tempdist = Double.MaxValue;
                        for (int j = 0; j < unsettlednodes.Count; j++)
                        {
                            double tempdist1 = visualmath.distance(unsettlednodes[j].getX(), unsettlednodes[j].getY(), regionedges[i].getStart().getX(), regionedges[i].getStart().getY());

                            if (tempdist1 < tempdist)
                            {
                                tempdist = tempdist1;
                                c = j;
                            }
                        }
                        settlednodes.Add(unsettlednodes[c]);
                        unsettlednodes.RemoveAt(c);
                    }
                    for (int j = 0; j < settlednodes.Count; j++)
                    {
                        //Check the first picking aisle node on that edge
                        if (j == 0)
                        {
                            if (visualmath.distance(settlednodes[j].getX(), settlednodes[j].getY(),
                                regionedges[i].getStart().getX(), regionedges[i].getStart().getY()) <
                                visualmath.distance(settlednodes[j].getX(), settlednodes[j].getY(),
                                regionedges[i].getEnd().getX(), regionedges[i].getEnd().getY()))
                            {
                                connect(regionedges[i].getStart(), settlednodes[j]);
                            }
                            else
                            {
                                connect(regionedges[i].getEnd(), settlednodes[j]);
                            }
                        }
                        //Connect in between picking aisle nodes
                        if (j > 0 && j < settlednodes.Count - 1)
                        {
                            connect(settlednodes[j], settlednodes[j - 1]);
                        }
                        //Check the last picking aisle node on that edge
                        if (j == settlednodes.Count - 1)
                        {
                            if (visualmath.distance(settlednodes[j].getX(), settlednodes[j].getY(),
                                regionedges[i].getStart().getX(), regionedges[i].getStart().getY()) <
                                visualmath.distance(settlednodes[j].getX(), settlednodes[j].getY(),
                                regionedges[i].getEnd().getX(), regionedges[i].getEnd().getY()))
                            {
                                connect(settlednodes[j], settlednodes[j - 1]);
                                connect(regionedges[i].getStart(), settlednodes[j]);
                            }
                            else
                            {
                                connect(settlednodes[j], settlednodes[j - 1]);
                                connect(regionedges[i].getEnd(), settlednodes[j]);
                            }
                        }
                    }
                }
            }
        }

        public void findRegions(double[] p_angle, double[] p_adjuster, double[] p_pickadjuster, double p_crossaislewidth, double p_pickingaislewidth, double p_locationwidth, double p_locationdepth, string p_designclass)
        {
            List<edge> edgepositive = new List<edge>();
            List<edge> edgenegative = new List<edge>();
            for (int i = 0; i < regionedges.Count; i++)
            {
                edgepositive.Add(regionedges[i]);
                edge tempedge1 = new edge(regionedges[i].getEnd(), regionedges[i].getStart(), options.regionedge);
                edgenegative.Add(tempedge1);
                regionedges[i].findEdgeRegion();//Used to calculate if storage location is inside a cross aisle
            }

            List<edge> tempedge = new List<edge>();
            List<region> tempregionlist = new List<region>();
            for (int i = 0; i < regionedges.Count; i++)
            {
                tempedge.Add(edgepositive[i]);
            }
            for (int i = 0; i < regionedges.Count; i++)
            {
                tempedge.Add(edgenegative[i]);
            }
            int position = 0;

            while (tempedge.Count > 0)
            {
                edge currentedge = tempedge[position];
                bool rightmostturn = true;
                bool nextturn = true;
                for (int i = 0; i < regionedges.Count; i++)
                {
                    if (isNegateEdge(currentedge, regionedges[i]))
                    {
                        currentedge = regionedges[i];
                        rightmostturn = false;
                        break;
                    }
                }

                region tempregion = new region(0, p_crossaislewidth, p_pickingaislewidth, p_locationwidth, p_locationdepth, 0, 0);
                edge startingedge = currentedge;

                do
                {
                    //add region edge to the region
                    tempregion.addRegionEdge(currentedge);
                    //Remove current edge from the non visited list
                    for (int i = 0; i < tempedge.Count; i++)
                    {
                        if (rightmostturn == true)
                        {
                            if (tempedge[i].id == currentedge.id)
                            {
                                tempedge.RemoveAt(i);
                                break;
                            }
                        }
                        if (rightmostturn == false)
                        {
                            if (isNegateEdge(tempedge[i], currentedge))
                            {
                                tempedge.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    //decide rightmost edge direction
                    if (currentedge.rightmost(rightmostturn).getEnd() == currentedge.getEnd() || currentedge.rightmost(rightmostturn).getEnd() == currentedge.getStart())
                    {
                        nextturn = false;
                    }
                    else
                    {
                        nextturn = true;
                    }
                    currentedge = currentedge.rightmost(rightmostturn);
                    rightmostturn = nextturn;
                } while (currentedge != startingedge);
                tempregionlist.Add(tempregion);
            }
            //Remove the outer region by finding the largest area (which will be the outer region)
            //Region finding also returns outer region which is an extra region we do not need
            //We can use this outer region's edges to name other regions in clockwise manner
            double maxregionarea = 0;
            int removeregionposition = -1;
            for (int i = 0; i < tempregionlist.Count; i++)
            {
                double currentregionarea = tempregionlist[i].getArea();
                if (currentregionarea > maxregionarea)
                {
                    removeregionposition = i;
                    maxregionarea = currentregionarea;
                }
            }

            List<edge> clockwiseedges = new List<edge>();

            //Find first clockwise edge
            int firstedgeindex = -1;
            for (int i = 0; i < tempregionlist[removeregionposition].regionedges.Count; i++)
            {
                if (tempregionlist[removeregionposition].regionedges[i].getStart().getX() == cornernodes[options.topleft].getX() && tempregionlist[removeregionposition].regionedges[i].getStart().getY() == cornernodes[options.topleft].getY() && (tempregionlist[removeregionposition].regionedges[i].getStart().getX() < tempregionlist[removeregionposition].regionedges[i].getEnd().getX()))
                {
                    firstedgeindex = i;
                    break;
                }
                if (tempregionlist[removeregionposition].regionedges[i].getEnd().getX() == cornernodes[options.topleft].getX() && tempregionlist[removeregionposition].regionedges[i].getEnd().getY() == cornernodes[options.topleft].getY() && (tempregionlist[removeregionposition].regionedges[i].getEnd().getX() < tempregionlist[removeregionposition].regionedges[i].getStart().getX()))
                {
                    firstedgeindex = i;
                    break;
                }
            }
            //Add all of them into clockwiseedges
            for (int i = 0; i < tempregionlist[removeregionposition].regionedges.Count; i++)
            {
                clockwiseedges.Add(tempregionlist[removeregionposition].regionedges[i]);
            }
            //Correct clockwiseedges order
            for (int i = 0; i <= firstedgeindex; i++)
            {
                clockwiseedges.Add(clockwiseedges[0]);
                clockwiseedges.RemoveAt(0);
            }


            for (int i = 0; i < tempregionlist[removeregionposition].regionedges.Count; i++)
            {
                clockwiseedges.Add(tempregionlist[removeregionposition].regionedges[i]);
            }
            tempregionlist.RemoveAt(removeregionposition);

            List<region> tempregionlist2 = new List<region>();//Used for actual ordering of regions that is connected to outside edges
            bool mybreak = false;
            //Set region angles, and then fill regions with picking aisles and locations
            for (int i = 0; i < clockwiseedges.Count; i++)
            {
                mybreak = false;
                for (int j = 0; j < tempregionlist.Count; j++)
                {
                    if (!tempregionlist2.Contains(tempregionlist[j]))
                    {
                        for (int k = 0; k < tempregionlist[j].regionedges.Count; k++)
                        {
                            if (clockwiseedges[i].id == tempregionlist[j].regionedges[k].id)
                            {
                                tempregionlist2.Add(tempregionlist[j]);
                                mybreak = true;
                                break;
                            }
                        }
                    }
                    if (mybreak == true) break;
                }
            }
            for (int i = 0; i < tempregionlist2.Count; i++)
            {
                tempregionlist2[i].setRegionAngle(p_angle[i]);
                tempregionlist2[i].setHorizontalAdjuster(p_adjuster[i]);
                tempregionlist2[i].setVerticalAdjuster(p_pickadjuster[i]);
                tempregionlist2[i].fill();
                addRegion(tempregionlist2[i]);
                tempregionlist.Remove(tempregionlist2[i]);//Remove the ones that are connected to outside region
            }
            //Check the design classes with interior regions
            switch (p_designclass)
            {
                case "3-0-3":
                    tempregionlist[0].setRegionAngle(p_angle[3]);
                    tempregionlist[0].setHorizontalAdjuster(p_adjuster[3]);
                    tempregionlist[0].setVerticalAdjuster(p_pickadjuster[3]);
                    tempregionlist[0].fill();
                    addRegion(tempregionlist[0]);
                    tempregionlist.Remove(tempregionlist[0]);
                    break;
                case "4-0-4":
                    tempregionlist[0].setRegionAngle(p_angle[4]);
                    tempregionlist[0].setHorizontalAdjuster(p_adjuster[4]);
                    tempregionlist[0].setVerticalAdjuster(p_pickadjuster[4]);
                    tempregionlist[0].fill();
                    addRegion(tempregionlist[0]);
                    tempregionlist.Remove(tempregionlist[0]);
                    break;
                case "4-0-5":
                    for (int i = 0; i < tempregionlist[0].regionedges.Count; i++)
                    {
                        if (tempregionlist[0].regionedges[i].getStart() == exteriornodes[1])//This means that that region has E2
                        {
                            tempregionlist[0].setRegionAngle(p_angle[4]);
                            tempregionlist[0].setHorizontalAdjuster(p_adjuster[4]);
                            tempregionlist[0].setVerticalAdjuster(p_pickadjuster[4]);
                            tempregionlist[0].fill();
                            addRegion(tempregionlist[0]);
                            tempregionlist.Remove(tempregionlist[0]);
                            break;
                        }
                    }
                    for (int i = 0; i < tempregionlist[0].regionedges.Count; i++)
                    {
                        if (tempregionlist[0].regionedges[i].getStart() == exteriornodes[3])//This means that that region has E4
                        {
                            tempregionlist[0].setRegionAngle(p_angle[5]);
                            tempregionlist[0].setHorizontalAdjuster(p_adjuster[5]);
                            tempregionlist[0].setVerticalAdjuster(p_pickadjuster[5]);
                            tempregionlist[0].fill();
                            addRegion(tempregionlist[0]);
                            tempregionlist.Remove(tempregionlist[0]);
                            break;
                        }
                    }
                    break;
            }


            //Connect each picking aisle node on the same region edge with next one and
            //if they are the first and last connect them with region edge beginning and end
            for (int i = 0; i < regionedges.Count; i++)
            {
                //if there is only one picking aisle node on this region edge
                //connect both end and beginning of region edge to this picking aisle node
                if (regionedges[i].getOnEdgeNodes().Count == 1)
                {
                    connect(regionedges[i].getStart(), regionedges[i].getOnEdgeNodes()[0]);
                    connect(regionedges[i].getEnd(), regionedges[i].getOnEdgeNodes()[0]);
                }
                //if there are two picking aisle nodes on this region edge
                //this is also the special case
                if (regionedges[i].getOnEdgeNodes().Count == 2)
                {
                    if (visualmath.distance(regionedges[i].getOnEdgeNodes()[0].getX(), regionedges[i].getOnEdgeNodes()[0].getY(),
                                regionedges[i].getStart().getX(), regionedges[i].getStart().getY()) <
                                visualmath.distance(regionedges[i].getOnEdgeNodes()[1].getX(), regionedges[i].getOnEdgeNodes()[1].getY(),
                                regionedges[i].getStart().getX(), regionedges[i].getStart().getY()))
                    {
                        connect(regionedges[i].getStart(), regionedges[i].getOnEdgeNodes()[0]);
                        connect(regionedges[i].getOnEdgeNodes()[0], regionedges[i].getOnEdgeNodes()[1]);
                        connect(regionedges[i].getEnd(), regionedges[i].getOnEdgeNodes()[1]);
                    }
                    else
                    {
                        connect(regionedges[i].getStart(), regionedges[i].getOnEdgeNodes()[1]);
                        connect(regionedges[i].getOnEdgeNodes()[0], regionedges[i].getOnEdgeNodes()[1]);
                        connect(regionedges[i].getEnd(), regionedges[i].getOnEdgeNodes()[0]);
                    }
                }
                if (regionedges[i].getOnEdgeNodes().Count > 2)
                {
                    List<node> unsettlednodes = new List<node>();
                    List<node> settlednodes = new List<node>();
                    double tempdist;
                    for (int j = 0; j < regionedges[i].getOnEdgeNodes().Count; j++)
                    {
                        unsettlednodes.Add(regionedges[i].getOnEdgeNodes()[j]);
                    }
                    int c = 0;
                    while (unsettlednodes.Count > 0)
                    {
                        tempdist = Double.MaxValue;
                        for (int j = 0; j < unsettlednodes.Count; j++)
                        {
                            double tempdist1 = visualmath.distance(unsettlednodes[j].getX(), unsettlednodes[j].getY(), regionedges[i].getStart().getX(), regionedges[i].getStart().getY());

                            if (tempdist1 < tempdist)
                            {
                                tempdist = tempdist1;
                                c = j;
                            }
                        }
                        settlednodes.Add(unsettlednodes[c]);
                        unsettlednodes.RemoveAt(c);
                    }
                    for (int j = 0; j < settlednodes.Count; j++)
                    {
                        //Check the first picking aisle node on that edge
                        if (j == 0)
                        {
                            if (visualmath.distance(settlednodes[j].getX(), settlednodes[j].getY(),
                                regionedges[i].getStart().getX(), regionedges[i].getStart().getY()) <
                                visualmath.distance(settlednodes[j].getX(), settlednodes[j].getY(),
                                regionedges[i].getEnd().getX(), regionedges[i].getEnd().getY()))
                            {
                                connect(regionedges[i].getStart(), settlednodes[j]);
                            }
                            else
                            {
                                connect(regionedges[i].getEnd(), settlednodes[j]);
                            }
                        }
                        //Connect in between picking aisle nodes
                        if (j > 0 && j < settlednodes.Count - 1)
                        {
                            connect(settlednodes[j], settlednodes[j - 1]);
                        }
                        //Check the last picking aisle node on that edge
                        if (j == settlednodes.Count - 1)
                        {
                            if (visualmath.distance(settlednodes[j].getX(), settlednodes[j].getY(),
                                regionedges[i].getStart().getX(), regionedges[i].getStart().getY()) <
                                visualmath.distance(settlednodes[j].getX(), settlednodes[j].getY(),
                                regionedges[i].getEnd().getX(), regionedges[i].getEnd().getY()))
                            {
                                connect(settlednodes[j], settlednodes[j - 1]);
                                connect(regionedges[i].getStart(), settlednodes[j]);
                            }
                            else
                            {
                                connect(settlednodes[j], settlednodes[j - 1]);
                                connect(regionedges[i].getEnd(), settlednodes[j]);
                            }
                        }
                    }
                }
            }
        }

        private void addPickandDepositNodetoGraph(double p_location)
        {
            node pd = new node(options.pdnode);
            double tempX;
            double tempY;

            if (p_location == 0)
            {
                pd.setY(cornernodes[options.topleft].getY());
                tempX = Math.Abs(cornernodes[options.topright].getX() - cornernodes[options.topleft].getX()) * ((p_location - 0) / 0.25) + cornernodes[options.topleft].getX();
                pd.setX(tempX);
                pd.setLocation(p_location);
            }
            if (p_location > 0 && p_location < 0.25)
            {
                pd.setY(cornernodes[options.topleft].getY());
                tempX = Math.Abs(cornernodes[options.topright].getX() - cornernodes[options.topleft].getX()) * ((p_location - 0) / 0.25) + cornernodes[options.topleft].getX();
                pd.setX(tempX);
                pd.setLocation(p_location);
            }
            if (p_location == 0.25)
            {
                pd.setX(cornernodes[options.topright].getX());
                tempY = Math.Abs(cornernodes[options.bottomright].getY() - cornernodes[options.topright].getY()) * ((p_location - 0.25) / 0.25) + cornernodes[options.topright].getY();
                pd.setY(tempY);
                pd.setLocation(p_location);
            }
            if (p_location > 0.25 && p_location < 0.5)
            {
                pd.setX(cornernodes[options.topright].getX());
                tempY = Math.Abs(cornernodes[options.bottomright].getY() - cornernodes[options.topright].getY()) * ((p_location - 0.25) / 0.25) + cornernodes[options.topright].getY();
                pd.setY(tempY);
                pd.setLocation(p_location);
            }
            if (p_location == 0.5)
            {
                pd.setY(cornernodes[options.bottomright].getY());
                tempX = cornernodes[options.bottomright].getX() - Math.Abs(cornernodes[options.bottomleft].getX() - cornernodes[options.bottomright].getX()) * ((p_location - 0.5) / 0.25);
                pd.setX(tempX);
                pd.setLocation(p_location);
            }
            if (p_location > 0.5 && p_location < 0.75)
            {
                pd.setY(cornernodes[options.bottomright].getY());
                tempX = cornernodes[options.bottomright].getX() - Math.Abs(cornernodes[options.bottomleft].getX() - cornernodes[options.bottomright].getX()) * ((p_location - 0.5) / 0.25);
                pd.setX(tempX);
                pd.setLocation(p_location);
            }
            if (p_location == 0.75)
            {
                pd.setX(cornernodes[options.bottomleft].getX());
                tempY = cornernodes[options.bottomleft].getY() - Math.Abs(cornernodes[options.topleft].getY() - cornernodes[options.bottomleft].getY()) * ((p_location - 0.75) / 0.25);
                pd.setY(tempY);
                pd.setLocation(p_location);
            }
            if (p_location > 0.75 && p_location < 1)
            {
                pd.setX(cornernodes[options.bottomleft].getX());
                tempY = cornernodes[options.bottomleft].getY() - Math.Abs(cornernodes[options.topleft].getY() - cornernodes[options.bottomleft].getY()) * ((p_location - 0.75) / 0.25);
                pd.setY(tempY);
                pd.setLocation(p_location);
            }
            if (p_location == 1)
            {
                pd.setX(cornernodes[options.bottomleft].getX());
                tempY = cornernodes[options.bottomleft].getY() - Math.Abs(cornernodes[options.topleft].getY() - cornernodes[options.bottomleft].getY()) * ((p_location - 0.75) / 0.25);
                pd.setY(tempY);
                pd.setLocation(p_location);
            }

        }

        private void addPickandDepositNode(double p_location)
        {
            node pd = new node(options.pdnode);
            double tempX;
            double tempY;

            if (p_location == 0)
            {
                pd.setY(cornernodes[options.topleft].getY());
                tempX = Math.Abs(cornernodes[options.topright].getX() - cornernodes[options.topleft].getX()) * ((p_location - 0) / 0.25) + cornernodes[options.topleft].getX();
                pd.setX(tempX);
                pd.setLocation(p_location);
            }
            if (p_location > 0 && p_location < 0.25)
            {
                pd.setY(cornernodes[options.topleft].getY());
                tempX = Math.Abs(cornernodes[options.topright].getX() - cornernodes[options.topleft].getX()) * ((p_location - 0) / 0.25) + cornernodes[options.topleft].getX();
                pd.setX(tempX);
                pd.setLocation(p_location);
            }
            if (p_location == 0.25)
            {
                pd.setX(cornernodes[options.topright].getX());
                tempY = Math.Abs(cornernodes[options.bottomright].getY() - cornernodes[options.topright].getY()) * ((p_location - 0.25) / 0.25) + cornernodes[options.topright].getY();
                pd.setY(tempY);
                pd.setLocation(p_location);
            }
            if (p_location > 0.25 && p_location < 0.5)
            {
                pd.setX(cornernodes[options.topright].getX());
                tempY = Math.Abs(cornernodes[options.bottomright].getY() - cornernodes[options.topright].getY()) * ((p_location - 0.25) / 0.25) + cornernodes[options.topright].getY();
                pd.setY(tempY);
                pd.setLocation(p_location);
            }
            if (p_location == 0.5)
            {
                pd.setY(cornernodes[options.bottomright].getY());
                tempX = cornernodes[options.bottomright].getX() - Math.Abs(cornernodes[options.bottomleft].getX() - cornernodes[options.bottomright].getX()) * ((p_location - 0.5) / 0.25);
                pd.setX(tempX);
                pd.setLocation(p_location);
            }
            if (p_location > 0.5 && p_location < 0.75)
            {
                pd.setY(cornernodes[options.bottomright].getY());
                tempX = cornernodes[options.bottomright].getX() - Math.Abs(cornernodes[options.bottomleft].getX() - cornernodes[options.bottomright].getX()) * ((p_location - 0.5) / 0.25);
                pd.setX(tempX);
                pd.setLocation(p_location);
            }
            if (p_location == 0.75)
            {
                pd.setX(cornernodes[options.bottomleft].getX());
                tempY = cornernodes[options.bottomleft].getY() - Math.Abs(cornernodes[options.topleft].getY() - cornernodes[options.bottomleft].getY()) * ((p_location - 0.75) / 0.25);
                pd.setY(tempY);
                pd.setLocation(p_location);
            }
            if (p_location > 0.75 && p_location < 1)
            {
                pd.setX(cornernodes[options.bottomleft].getX());
                tempY = cornernodes[options.bottomleft].getY() - Math.Abs(cornernodes[options.topleft].getY() - cornernodes[options.bottomleft].getY()) * ((p_location - 0.75) / 0.25);
                pd.setY(tempY);
                pd.setLocation(p_location);
            }
            if (p_location == 1)
            {
                pd.setX(cornernodes[options.bottomleft].getX());
                tempY = cornernodes[options.bottomleft].getY() - Math.Abs(cornernodes[options.topleft].getY() - cornernodes[options.bottomleft].getY()) * ((p_location - 0.75) / 0.25);
                pd.setY(tempY);
                pd.setLocation(p_location);
            }

            importantnodes.Add(pd);

            //Find the closest node on the region edges
            double tempdist = double.MaxValue;
            node closest1 = null;
            node closest2 = null;
            for (int i = 0; i < regionedges.Count; i++)
            {
                if (regionedges[i].isNodeOnEdge(pd))
                {
                    double tempdist3 = visualmath.distance(pd.getX(), pd.getY(), regionedges[i].getStart().getX(), regionedges[i].getStart().getY());
                    double tempdist4 = visualmath.distance(pd.getX(), pd.getY(), regionedges[i].getEnd().getX(), regionedges[i].getEnd().getY());

                    //Closest one is start
                    if (tempdist > tempdist3)
                    {
                        tempdist = tempdist3;
                        closest1 = regionedges[i].getStart();
                    }
                    //Closest one is end
                    if (tempdist > tempdist4)
                    {
                        tempdist = tempdist4;
                        closest1 = regionedges[i].getEnd();
                    }
                    //Closest one is in between
                    for (int j = 0; j < regionedges[i].getOnEdgeNodes().Count; j++)
                    {
                        double tempdist2 = visualmath.distance(pd.getX(), pd.getY(), regionedges[i].getOnEdgeNodes()[j].getX(), regionedges[i].getOnEdgeNodes()[j].getY());
                        if (tempdist > tempdist2)
                        {
                            tempdist = tempdist2;
                            closest1 = regionedges[i].getOnEdgeNodes()[j];
                        }
                    }
                }
            }
            //Find the second closest on the region edges
            double tempdist1 = double.MaxValue;

            for (int i = 0; i < regionedges.Count; i++)
            {
                if (regionedges[i].isNodeOnEdge(pd))
                {
                    double tempdist3 = visualmath.distance(pd.getX(), pd.getY(), regionedges[i].getStart().getX(), regionedges[i].getStart().getY());
                    double tempdist4 = visualmath.distance(pd.getX(), pd.getY(), regionedges[i].getEnd().getX(), regionedges[i].getEnd().getY());
                    //Closest is start
                    if (tempdist1 > tempdist3 && closest1 != regionedges[i].getStart())
                    {
                        tempdist1 = tempdist3;
                        closest2 = regionedges[i].getStart();
                    }
                    //Closest is end
                    if (tempdist1 > tempdist4 && closest1 != regionedges[i].getEnd())
                    {
                        tempdist1 = tempdist4;
                        closest2 = regionedges[i].getEnd();
                    }
                    //Closest is in between
                    for (int j = 0; j < regionedges[i].getOnEdgeNodes().Count; j++)
                    {
                        double tempdist2 = visualmath.distance(pd.getX(), pd.getY(), regionedges[i].getOnEdgeNodes()[j].getX(), regionedges[i].getOnEdgeNodes()[j].getY());
                        if (tempdist1 > tempdist2)
                        {
                            if (tempdist2 > tempdist)
                            {
                                tempdist1 = tempdist2;
                                closest2 = regionedges[i].getOnEdgeNodes()[j];
                            }
                        }
                    }
                }
            }

            if (tempdist == 0)
            {
                connect(pd, closest1);
            }
            else
            {
                connect(pd, closest1);
                connect(pd, closest2);
            }

            pdnodes.Add(pd);
        }

        private bool isSameEdge(edge p_edge1, edge p_edge2)
        {
            if (p_edge1.getStart().getX() == p_edge2.getStart().getX() &&
                p_edge1.getStart().getY() == p_edge2.getStart().getY() &&
                p_edge1.getEnd().getX() == p_edge2.getEnd().getX() &&
                p_edge1.getEnd().getY() == p_edge2.getEnd().getY())
            {
                return true;
            }
            else if (p_edge1.getStart().getX() == p_edge2.getEnd().getX() &&
                p_edge1.getStart().getY() == p_edge2.getEnd().getY() &&
                p_edge1.getEnd().getX() == p_edge2.getStart().getX() &&
                p_edge1.getEnd().getY() == p_edge2.getStart().getY())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool isNegateEdge(edge p_edge1, edge p_edge2)
        {
            if (p_edge1.getStart().getX() == p_edge2.getEnd().getX() &&
                p_edge1.getStart().getY() == p_edge2.getEnd().getY() &&
                p_edge1.getEnd().getX() == p_edge2.getStart().getX() &&
                p_edge1.getEnd().getY() == p_edge2.getStart().getY())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Calculates total number of storage locations in warehouse object
        /// </summary>
        /// <returns>Number of storage locations</returns>
        public int totalNumberOfLocations()
        {
            int count = 0;
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    for (int k = 0; k < regions[i].pickingaisleedges[j].getOnEdgeNodes().Count; k++)
                    {
                        if (regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s1 != null)
                        {
                            count = count + regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s1.getCapacity();
                        }
                        if (regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s2 != null)
                        {
                            count = count + regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s2.getCapacity();
                        }
                    }
                }
            }

            return count;
        }

        public int totalNumberOfAisles()
        {
            int count = 0;
            for (int i = 0; i < regions.Count; i++)
            {
                count = count + regions[i].pickingaisleedges.Count;
            }

            return count;
        }

        private void addCrossAisle(node p_start, node p_end)
        {
            //Add connection in node level
            edge e = p_start.connect(p_end, options.regionedge);
            e.setWidth(this.crossaislewidth);
            //Add cross aisle connection
            regionedges.Add(e);
        }

        /// <summary>
        /// Adds region to warehouse
        /// </summary>
        private void addRegion(region r)
        {
            regions.Add(r);
        }

        /// <summary>
        /// Returns true if a region is removed correctly
        /// </summary>
        /// <param name="p_regionID">Region ID of the region to be removed</param>
        /// <returns></returns>
        public bool removeRegion(int p_regionID)
        {
            for (int i = 0; i < regions.Count; i++)
            {
                if (regions[i].getRegionID() == p_regionID)
                {
                    regions.Remove(regions[i]);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns sum of shortest path distances from this location to other locations
        /// </summary>
        /// <param name="p_node">Source node</param>
        /// <returns></returns>
        public double totalDistance(node p_node)
        {
            double totalDist = 0;
            if (!usevisibilitygraph)
            {
                for (int i = 0; i < locationnodedistances.GetLength(0); i++)
                {
                    totalDist = totalDist + locationnodedistances[p_node.indexlocation, i];
                }
            }
            else
            {
                for (int i = 0; i < visibilitygraphdistances.GetLength(0); i++)
                {
                    totalDist = totalDist + visibilitygraphdistances[p_node.indexgraphnode, i];
                }
            }
            return totalDist;
        }

        /// <summary>
        /// Calculates total distances from each location to each location
        /// </summary>
        public void totalDistances()
        {
            int totallocations = totalNumberOfLocations();
            for (int i = 0; i < regions.Count; i++)
            {
                //for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                Parallel.For(0, regions[i].pickingaisleedges.Count, j =>
                {
                    //for (int k = 0; k < regions[i].pickingaisleedges[j].getOnEdgeNodes().Count; k++)
                    Parallel.For(0, regions[i].pickingaisleedges[j].getOnEdgeNodes().Count, k =>
                    {
                        double dist = totalDistance(regions[i].pickingaisleedges[j].getOnEdgeNodes()[k]);
                        distance tempdistance = new distance(regions[i].pickingaisleedges[j].getOnEdgeNodes()[k], null, dist);
                        //manytomanytotaldistances.Add(tempdistance);
                        lock (manytomanytotaldistances) { manytomanytotaldistances.Add(tempdistance); }
                        regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].totaldistance = dist;
                    });
                });
            }
        }

        /// <summary>
        /// Calculates distance to pd point from each location
        /// </summary>
        public void pdTotalDistances()
        {
            DateTime start = DateTime.Now;
            if (!usevisibilitygraph)
            {
                for (int i = 0; i < regions.Count; i++)
                {
                    for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                    {
                        for (int k = 0; k < regions[i].pickingaisleedges[j].getOnEdgeNodes().Count; k++)
                        {
                            double dist = locationnodedistances[locationnodedistances.GetLength(0) - 1, regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].indexlocation];
                            distance tempdistance = new distance(regions[i].pickingaisleedges[j].getOnEdgeNodes()[k], null, dist);
                            lock (onetomanytotaldistances) { onetomanytotaldistances.Add(tempdistance); }
                            regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].pddistance = dist;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < regions.Count; i++)
                {
                    for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                    {
                        for (int k = 0; k < regions[i].pickingaisleedges[j].getOnEdgeNodes().Count; k++)
                        {
                            double dist = visibilitygraphdistances[visibilitygraphdistances.GetLength(0) - 1, regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].indexgraphnode];
                            distance tempdistance = new distance(regions[i].pickingaisleedges[j].getOnEdgeNodes()[k], null, dist);
                            lock (onetomanytotaldistances) { onetomanytotaldistances.Add(tempdistance); }
                            regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].pddistance = dist;
                        }
                    }
                }
            }
            this.elapsed1 = DateTime.Now - start;
        }

        public void randomizeOrders()
        {
            Random rnd = new Random(0);
            myorders = myorders.OrderBy(item => rnd.Next(myorders.Count)).ToList();
        }

        public void rankLocations(double p_averageTourLength)
        {
            //Average Tour Length including pick and deposit point
            double a = p_averageTourLength + 1;
            //Weight For OnetoMany Problem
            double b = 1 - ((a - 2) / a);
            //Weight For ManytoMany Problem
            double c = ((a - 2) / a);
            //Sort one to many distances and many to many distances
            onetomanytotaldistances.Sort((x, y) => x.dist.CompareTo(y.dist));
            manytomanytotaldistances.Sort((x, y) => x.dist.CompareTo(y.dist));
            double min1 = onetomanytotaldistances[0].dist;
            double max1 = onetomanytotaldistances[onetomanytotaldistances.Count - 1].dist;
            double min2 = manytomanytotaldistances[0].dist;
            double max2 = manytomanytotaldistances[manytomanytotaldistances.Count - 1].dist;
            //Find onetomany normalized distance for each node
            for (int i = 0; i < onetomanytotaldistances.Count(); i++)
            {
                onetomanytotaldistances[i].node1.onetomanynormalizeddist = (onetomanytotaldistances[i].dist - min1) / (max1 - min1);
            }
            //Find manytomany normalized distance for each node
            for (int i = 0; i < manytomanytotaldistances.Count(); i++)
            {
                manytomanytotaldistances[i].node1.manytomanynormalizeddist = (manytomanytotaldistances[i].dist - min2) / (max2 - min2);
            }
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    for (int k = 0; k < regions[i].pickingaisleedges[j].getOnEdgeNodes().Count; k++)
                    {
                        regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].overallranking = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].onetomanynormalizeddist * b + regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].manytomanynormalizeddist * c;
                        distance tempdistance = new distance(regions[i].pickingaisleedges[j].getOnEdgeNodes()[k], null, regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].overallranking);
                        overallranking.Add(tempdistance);
                    }
                }
            }
            overallranking.Sort((x, y) => x.dist.CompareTo(y.dist));
        }

        public void colorOverall()
        {
            DateTime start = DateTime.Now;
            overallranking.Sort((x, y) => x.dist.CompareTo(y.dist));
            colorLocations();
            elapsed2 = DateTime.Now - start;
        }

        public void colorLocations()
        {
            double range = overallranking[overallranking.Count - 1].dist - overallranking[0].dist;

            double increment = range / options.numbercolors;
            int m = 0;
            double start = overallranking[0].dist;
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    for (int k = 0; k < regions[i].pickingaisleedges[j].getOnEdgeNodes().Count; k++)
                    {
                        for (m = 0; m < options.numbercolors; m++)
                        {
                            if (start + increment * m < regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].overallranking)
                            {
                                regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].color = m;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns shortest path distance between two important nodes
        /// </summary>
        /// <param name="p_node1">First Important Node</param>
        /// <param name="p_node2">Second Important Node</param>
        /// <returns></returns>
        private double shortestDistanceImportantNode(node p_node1, node p_node2)
        {
            return importantnodedistances[p_node1.indeximportant, p_node2.indeximportant];
        }

        public void locationShortestDistance()
        {
            int l = 0;//Used for location index
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    for (int k = 0; k < regions[i].pickingaisleedges[j].getOnEdgeNodes().Count; k++)
                    //Parallel.For(0, regions[i].pickingaisleedges[j].getOnEdgeNodes().Count, k =>
                    {
                        regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].indexlocation = l + k;//This incremental logic is used for thread safety
                    }
                    l = l + regions[i].pickingaisleedges[j].getOnEdgeNodes().Count;
                }
            }

            for (int i = 0; i < pdnodes.Count; i++)//Add pd nodes to location shortest distances (in this way we don't need to recalculate)
            {
                pdnodes[i].indexlocation = l;
                l++;
            }

            locationnodedistances = new double[l, l];//Create the size of the locationnodedistancesmatrix, l gives the total number of locations
            locationnodes = new node[l];//Create the size of the locationnodes, l gives the total number of locations

            int m = 0;
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    for (int k = 0; k < regions[i].pickingaisleedges[j].getOnEdgeNodes().Count; k++)
                    {
                        locationnodes[m] = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k];
                        m++;
                    }
                }
            }

            for (int i = 0; i < pdnodes.Count; i++)//Add pd nodes to location shortest distances (in this way we don't need to recalculate)
            {
                locationnodes[m] = pdnodes[i];
                m++;
            }

            for (int i = 0; i < regions.Count; i++)
            //Parallel.For(0, regions.Count, i=>
            {
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                //Parallel.For(0, regions[i].pickingaisleedges.Count, j =>
                {
                    //for (int k = 0; k < regions[i].pickingaisleedges[j].getOnEdgeNodes().Count; k++)
                    Parallel.For(0, regions[i].pickingaisleedges[j].getOnEdgeNodes().Count, k =>
                    {
                        double d1, d2;
                        d1 = visualmath.distance(regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getX(), regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getY(),
                            regions[i].pickingaisleedges[j].getStart().getX(), regions[i].pickingaisleedges[j].getStart().getY());
                        d2 = visualmath.distance(regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getX(), regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getY(),
                            regions[i].pickingaisleedges[j].getEnd().getX(), regions[i].pickingaisleedges[j].getEnd().getY());
                        for (int ii = 0; ii < regions.Count; ii++)
                        {
                            for (int jj = 0; jj < regions[ii].pickingaisleedges.Count; jj++)
                            {
                                double d13, d23, d14, d24;
                                d13 = shortestDistanceImportantNode(regions[i].pickingaisleedges[j].getStart(), regions[ii].pickingaisleedges[jj].getStart());
                                d23 = shortestDistanceImportantNode(regions[i].pickingaisleedges[j].getEnd(), regions[ii].pickingaisleedges[jj].getStart());
                                d14 = shortestDistanceImportantNode(regions[i].pickingaisleedges[j].getStart(), regions[ii].pickingaisleedges[jj].getEnd());
                                d24 = shortestDistanceImportantNode(regions[i].pickingaisleedges[j].getEnd(), regions[ii].pickingaisleedges[jj].getEnd());
                                //for (int kk = 0; kk < regions[ii].pickingaisleedges[jj].getOnEdgeNodes().Count; kk++)
                                Parallel.For(0, regions[ii].pickingaisleedges[jj].getOnEdgeNodes().Count, kk =>
                                {
                                    double shortestdist = Double.MaxValue;
                                    if (regions[ii].pickingaisleedges[jj].id != regions[i].pickingaisleedges[j].id)
                                    {
                                        double d3, d4;
                                        double dist1, dist2, dist3, dist4;
                                        d3 = visualmath.distance(regions[ii].pickingaisleedges[jj].getOnEdgeNodes()[kk].getX(), regions[ii].pickingaisleedges[jj].getOnEdgeNodes()[kk].getY(),
                                            regions[ii].pickingaisleedges[jj].getStart().getX(), regions[ii].pickingaisleedges[jj].getStart().getY());
                                        d4 = visualmath.distance(regions[ii].pickingaisleedges[jj].getOnEdgeNodes()[kk].getX(), regions[ii].pickingaisleedges[jj].getOnEdgeNodes()[kk].getY(),
                                            regions[ii].pickingaisleedges[jj].getEnd().getX(), regions[ii].pickingaisleedges[jj].getEnd().getY());

                                        dist1 = d13 + d1 + d3;
                                        dist2 = d23 + d2 + d3;
                                        dist3 = d14 + d1 + d4;
                                        dist4 = d24 + d2 + d4;

                                        if (shortestdist > dist1)
                                        {
                                            shortestdist = dist1;
                                        }
                                        if (shortestdist > dist2)
                                        {
                                            shortestdist = dist2;
                                        }
                                        if (shortestdist > dist3)
                                        {
                                            shortestdist = dist3;
                                        }
                                        if (shortestdist > dist4)
                                        {
                                            shortestdist = dist4;
                                        }
                                    }
                                    else//They are on the same picking aisle
                                    {
                                        shortestdist = visualmath.distance(regions[ii].pickingaisleedges[jj].getOnEdgeNodes()[kk].getX(), regions[ii].pickingaisleedges[jj].getOnEdgeNodes()[kk].getY(),
                                            regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getX(), regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getY());
                                    }

                                    locationnodedistances[regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].indexlocation, regions[ii].pickingaisleedges[jj].getOnEdgeNodes()[kk].indexlocation] = shortestdist;
                                });
                            }
                        }
                    });
                }
            }

            //Calculate shortest distance for PD locations
            for (int p = 0; p < pdnodes.Count; p++)
            {
                for (int i = 0; i < regions.Count; i++)
                {
                    for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                    {
                        //for (int k = 0; k < regions[i].pickingaisleedges[j].getOnEdgeNodes().Count; k++)
                        Parallel.For(0, regions[i].pickingaisleedges[j].getOnEdgeNodes().Count, k =>
                        {
                            double d1 = shortestDistanceImportantNode(regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getPickingAisleEdge().getStart(), pdnodes[p]);
                            double d2 = shortestDistanceImportantNode(regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getPickingAisleEdge().getEnd(), pdnodes[p]);
                            double d3 = visualmath.distance(regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getX(), regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getY(),
                                        regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getPickingAisleEdge().getStart().getX(), regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getPickingAisleEdge().getStart().getY());
                            double d4 = visualmath.distance(regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getX(), regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getY(),
                                regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getPickingAisleEdge().getEnd().getX(), regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].getPickingAisleEdge().getEnd().getY());
                            double dist1 = d1 + d3;
                            double dist2 = d2 + d4;
                            double shortestdist = Double.MaxValue;
                            if (shortestdist > dist1)
                            {
                                shortestdist = dist1;
                            }
                            if (shortestdist > dist2)
                            {
                                shortestdist = dist2;
                            }
                            locationnodedistances[pdnodes[p].indexlocation, regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].indexlocation] = shortestdist;
                            locationnodedistances[regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].indexlocation, pdnodes[p].indexlocation] = shortestdist;
                        });
                    }
                }
            }
        }

        public double shortestPathDistanceTwoLocations(node p_node1, node p_node2)
        {
            return locationnodedistances[p_node1.indexlocation, p_node2.indexlocation];
        }

        public double shortestPathDistanceTwoLocationsVisibilityGraph(node p_node1, node p_node2)
        {
            return visibilitygraphdistances[p_node1.indexgraphnode, p_node2.indexgraphnode];
        }

        //public double shortestPathDistanceTwoLocations(node p_node1, node p_node2)
        //{
        //    if (p_node2.type == options.pdnode && p_node1.type == options.locationnode)
        //    {
        //        node tempnode = p_node2;
        //        p_node2 = p_node1;
        //        p_node1 = tempnode;
        //    }
        //    if (p_node1.type == options.pdnode && p_node2.type == options.locationnode)
        //    {
        //        double d1 = shortestDistanceImportantNode(p_node2.getPickingAisleEdge().getStart(), p_node1);
        //        double d2 = shortestDistanceImportantNode(p_node2.getPickingAisleEdge().getEnd(), p_node1);
        //        double d3 = visualmath.distance(p_node2.getX(), p_node2.getY(),
        //                    p_node2.getPickingAisleEdge().getStart().getX(), p_node2.getPickingAisleEdge().getStart().getY());
        //        double d4 = visualmath.distance(p_node2.getX(), p_node2.getY(),
        //            p_node2.getPickingAisleEdge().getEnd().getX(), p_node2.getPickingAisleEdge().getEnd().getY());
        //        double dist1 = d1 + d3;
        //        double dist2 = d2 + d4;
        //        double dist = Double.MaxValue;
        //        if (dist > dist1)
        //        {
        //            dist = dist1;
        //        }
        //        if (dist > dist2)
        //        {
        //            dist = dist2;
        //        }
        //        return dist;
        //    }
        //    if (p_node1.type == options.locationnode && p_node2.type == options.locationnode)
        //    {
        //        if (p_node1.getPickingAisleEdge().id != p_node2.getPickingAisleEdge().id)
        //        {
        //            double d1, d2;
        //            d1 = visualmath.distance(p_node1.getX(), p_node1.getY(),
        //                p_node1.getPickingAisleEdge().getStart().getX(), p_node1.getPickingAisleEdge().getStart().getY());
        //            d2 = visualmath.distance(p_node1.getX(), p_node1.getY(),
        //                p_node1.getPickingAisleEdge().getEnd().getX(), p_node1.getPickingAisleEdge().getEnd().getY());
        //            double d13, d23, d14, d24;
        //            d13 = shortestDistanceImportantNode(p_node1.getPickingAisleEdge().getStart(), p_node2.getPickingAisleEdge().getStart());
        //            d23 = shortestDistanceImportantNode(p_node1.getPickingAisleEdge().getEnd(), p_node2.getPickingAisleEdge().getStart());
        //            d14 = shortestDistanceImportantNode(p_node1.getPickingAisleEdge().getStart(), p_node2.getPickingAisleEdge().getEnd());
        //            d24 = shortestDistanceImportantNode(p_node1.getPickingAisleEdge().getEnd(), p_node2.getPickingAisleEdge().getEnd());
        //            double d3, d4;
        //            double dist1, dist2, dist3, dist4;
        //            d3 = visualmath.distance(p_node2.getX(), p_node2.getY(),
        //                p_node2.getPickingAisleEdge().getStart().getX(), p_node2.getPickingAisleEdge().getStart().getY());
        //            d4 = visualmath.distance(p_node2.getX(), p_node2.getY(),
        //                p_node2.getPickingAisleEdge().getEnd().getX(), p_node2.getPickingAisleEdge().getEnd().getY());

        //            dist1 = d13 + d1 + d3;
        //            dist2 = d23 + d2 + d3;
        //            dist3 = d14 + d1 + d4;
        //            dist4 = d24 + d2 + d4;
        //            double shortestdist = Double.MaxValue;

        //            if (shortestdist > dist1)
        //            {
        //                shortestdist = dist1;
        //            }
        //            if (shortestdist > dist2)
        //            {
        //                shortestdist = dist2;
        //            }
        //            if (shortestdist > dist3)
        //            {
        //                shortestdist = dist3;
        //            }
        //            if (shortestdist > dist4)
        //            {
        //                shortestdist = dist4;
        //            }
        //            return shortestdist;
        //        }
        //        else
        //        {
        //            return visualmath.distance(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY());
        //        }
        //    }
        //    return 0;
        //}

        public void createImportantNodeShortestDistances()
        {
            importantnodedistances = new double[importantnodes.Count, importantnodes.Count];//Distances matrix
            //node[,] prev = new node[importantnodes.Count,importantnodes.Count];
            bool[,] scanned = new bool[importantnodes.Count, importantnodes.Count];//If item is scanned then this will be true, initial values are all false
            for (int i = 0; i < importantnodes.Count; i++)
            {
                importantnodes[i].indeximportant = i;
            }
            Parallel.For(0, importantnodes.Count, i =>
            //for (int i = 0; i < importantnodes.Count; i++)//For all-pair shortest path
            {
                importantnodedistances[i, i] = 0;         //Distance from source to source
                //prev[i, i] = null;      //Previous node in optimal path initialization
                priorityqueue Q = new priorityqueue();
                Q.Enqueue(new distance(importantnodes[i], null, 0));
                scanned[i, i] = true;   //Mark source node as scanned
                for (int j = 0; j < importantnodes.Count; j++)
                {
                    if (j != i)
                    {
                        importantnodedistances[i, j] = double.MaxValue;
                        //prev[i, j] = null;
                    }
                }
                while (Q.Count() > 0)
                {
                    distance ud = Q.Dequeue();//Find smallest distance in queue
                    node currentnode = ud.node1;
                    scanned[i, currentnode.indeximportant] = true;
                    double alt = double.MaxValue;
                    for (int l = 0; l < currentnode.edges.Count; l++)
                    {
                        if (currentnode.edges[l].getStart() == currentnode)
                        {
                            if (scanned[i, currentnode.edges[l].getEnd().indeximportant] == false)//This means item has not been scanned
                            {
                                alt = importantnodedistances[i, currentnode.indeximportant] + visualmath.distance(currentnode.getX(), currentnode.getY(),
                                    currentnode.edges[l].getEnd().getX(), currentnode.edges[l].getEnd().getY());
                                if (alt < importantnodedistances[i, currentnode.edges[l].getEnd().indeximportant])
                                {
                                    importantnodedistances[i, currentnode.edges[l].getEnd().indeximportant] = alt;//Record new distance
                                    //prev[i, currentnode.edges[l].getEnd().indeximportant] = currentnode;//Record predecessor
                                    Q.Enqueue(new distance(currentnode.edges[l].getEnd(), null, alt));
                                }
                            }
                        }
                        if (currentnode.edges[l].getEnd() == currentnode)
                        {
                            if (scanned[i, currentnode.edges[l].getStart().indeximportant] == false)//This means item has not been scanned
                            {
                                alt = importantnodedistances[i, currentnode.indeximportant] + visualmath.distance(currentnode.getX(), currentnode.getY(),
                                    currentnode.edges[l].getStart().getX(), currentnode.edges[l].getStart().getY());
                                if (alt < importantnodedistances[i, currentnode.edges[l].getStart().indeximportant])
                                {
                                    importantnodedistances[i, currentnode.edges[l].getStart().indeximportant] = alt;//Record new distance
                                    //prev[i, currentnode.edges[l].getStart().indeximportant] = currentnode;//Record predecessor
                                    Q.Enqueue(new distance(currentnode.edges[l].getStart(), null, alt));
                                }
                            }
                        }
                    }
                }
            });
        }

        ///// <summary>
        ///// Shortest Path Algorithm without Priority Queue
        ///// </summary>
        ///// <param name="p_node"></param>
        ///// <returns></returns>
        //public List<distance> shortestPath(node p_node)
        //{
        //    List<distance> tempdistances = new List<distance>();
        //    List<node> unsettlednodes = new List<node>();

        //    for (int i = 0; i < importantnodes.Count; i++)
        //    {
        //        if (importantnodes[i] == p_node)
        //        {
        //            tempdistances.Add(new distance(p_node, p_node, 0));
        //            unsettlednodes.Add(importantnodes[i]);
        //            unsettlednodes[unsettlednodes.Count - 1].previousnode = null;
        //        }
        //        else
        //        {
        //            tempdistances.Add(new distance(p_node, importantnodes[i], double.MaxValue));
        //            unsettlednodes.Add(importantnodes[i]);
        //            unsettlednodes[unsettlednodes.Count - 1].previousnode = null;
        //        }
        //    }

        //    while (unsettlednodes.Count > 0)
        //    {
        //        double minval = double.MaxValue;
        //        int c = -1;
        //        //find smallest distance in unsettled nodes
        //        for (int i = 0; i < unsettlednodes.Count; i++)
        //        {
        //            if (returnDistance(tempdistances, unsettlednodes[i].id).dist < minval)
        //            {
        //                minval = returnDistance(tempdistances, unsettlednodes[i].id).dist;
        //                c = i;
        //            }
        //        }

        //        int c1 = -1;
        //        for (int i = 0; i < tempdistances.Count; i++)
        //        {
        //            if (tempdistances[i].node2.id == unsettlednodes[c].id)
        //            {
        //                c1 = i;
        //            }
        //        }

        //        if (tempdistances[c1].dist == double.MaxValue)
        //        {
        //            break;
        //        }

        //        node currentnode = unsettlednodes[c];
        //        unsettlednodes.RemoveAt(c);
        //        double alt = Double.MaxValue;
        //        for (int i = 0; i < currentnode.edges.Count; i++)
        //        {
        //            if (currentnode.edges[i].getStart() == currentnode)
        //            {
        //                for (int j = 0; j < unsettlednodes.Count; j++)
        //                {
        //                    if (currentnode.edges[i].getEnd() == unsettlednodes[j])
        //                    {
        //                        alt = returnDistance(tempdistances, currentnode.id).dist +
        //                        visualmath.distance(currentnode.getX(), currentnode.getY(),
        //                        unsettlednodes[j].getX(), unsettlednodes[j].getY());
        //                    }
        //                }
        //                if (alt < returnDistance(tempdistances, currentnode.edges[i].getEnd().id).dist)
        //                {
        //                    returnDistance(tempdistances, currentnode.edges[i].getEnd().id).dist = alt;
        //                    currentnode.edges[i].getEnd().previousnode = currentnode;
        //                }
        //            }
        //            if (currentnode.edges[i].getEnd() == currentnode)
        //            {
        //                for (int j = 0; j < unsettlednodes.Count; j++)
        //                {
        //                    if (currentnode.edges[i].getStart() == unsettlednodes[j])
        //                    {
        //                        alt = returnDistance(tempdistances, currentnode.id).dist +
        //                        visualmath.distance(currentnode.getX(), currentnode.getY(),
        //                        unsettlednodes[j].getX(), unsettlednodes[j].getY());
        //                    }
        //                }
        //                if (alt < returnDistance(tempdistances, currentnode.edges[i].getStart().id).dist)
        //                {
        //                    returnDistance(tempdistances, currentnode.edges[i].getStart().id).dist = alt;
        //                    currentnode.edges[i].getStart().previousnode = currentnode;
        //                }
        //            }
        //        }
        //    }

        //    return tempdistances;
        //}

        /// <summary>
        /// This is also terrible implementation (this should be O(1)) instead of O(n)
        /// </summary>
        /// <param name="p_distances"></param>
        /// <param name="p_nodeid"></param>
        /// <returns></returns>
        private distance returnDistance(List<distance> p_distances, int p_nodeid)
        {
            for (int i = 0; i < p_distances.Count; i++)
            {
                if (p_distances[i].node2.id == p_nodeid)
                {
                    return p_distances[i];
                }
            }
            return null;
        }

        public double averageTotalDistancePerLocation()
        {
            double totalDist = 0;
            for (int i = 0; i < manytomanytotaldistances.Count; i++)
            {
                totalDist = totalDist + manytomanytotaldistances[i].dist;
            }
            double divider = Math.Pow(Convert.ToDouble(manytomanytotaldistances.Count), 2);
            return totalDist / divider;
        }

        public double averageDistancetoPDPerLocation()
        {
            double totalDist = 0;
            for (int i = 0; i < onetomanytotaldistances.Count; i++)
            {
                totalDist = totalDist + onetomanytotaldistances[i].dist;
            }
            double divider = Math.Pow(Convert.ToDouble(onetomanytotaldistances.Count), 1);
            return totalDist / divider;
        }

        public double getWidth()
        {
            return Math.Sqrt(area / aspectratio);
        }

        public double getDepth()
        {
            return Math.Sqrt(area * aspectratio);
        }

        public void setArea(double p_area)
        {
            this.area = p_area;
            this.width = getWidth();
            this.depth = getDepth();
        }

        public void createPolygonsandGraphNodes()
        {
            int countnodes = 0;//Used for counting how big grapnodes array should be
            int countpolygons = 0;
            List<List<int>> mylist = new List<List<int>>();
            List<List<int>> mylist2 = new List<List<int>>();
            double ff = 0.0000000001; //Fixfactor, used for bugs coming from precision error where l1, l3, r2, r4 could be inside the neighbor polygon so we move them away a little bit
            for (int i = 0; i < regions.Count; i++)
            {
                List<int> tmplist = new List<int>();
                List<int> tmplist2 = new List<int>();
                for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                {
                    tmplist.Add(countnodes);
                    tmplist2.Add(countpolygons);
                    bool isthereleftstorage = false;
                    bool isthererightstorage = false;
                    countnodes += regions[i].pickingaisleedges[j].getOnEdgeNodes().Count;

                    for (int k = 0; k < regions[i].pickingaisleedges[j].getOnEdgeNodes().Count; k++)
                    {
                        if (regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s1 != null)
                        {
                            isthereleftstorage = true;
                        }
                        if (regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s2 != null)
                        {
                            isthererightstorage = true;
                        }
                        if (isthereleftstorage && isthererightstorage) break;
                    }
                    if (isthereleftstorage)
                    {
                        countnodes = countnodes + 4;
                        countpolygons++;
                    }
                    if (isthererightstorage)
                    {
                        countnodes = countnodes + 4;
                        countpolygons++;
                    }
                }
                mylist.Add(tmplist);
                mylist2.Add(tmplist2);
            }

            countnodes += pdnodes.Count;

            graphnodes = new node[countnodes];
            polygons = new polygon[countpolygons];

            //for (int i = 0; i < regions.Count; i++)
            Parallel.For(0, regions.Count, i =>
            {
                //for (int j = 0; j < regions[i].pickingaisleedges.Count; j++)
                Parallel.For(0, regions[i].pickingaisleedges.Count, j =>
                {
                    int g = mylist[i][j];
                    int h = mylist2[i][j];
                    bool isthereleftstorage = false;
                    bool isthererightstorage = false;
                    
                    //Calculation of two polygons
                    double Xl1 = -1, Xl2 = -1, Xl3 = -1, Xl4 = -1;
                    double Yl1 = -1, Yl2 = -1, Yl3 = -1, Yl4 = -1;
                    double Xr1 = -1, Xr2 = -1, Xr3 = -1, Xr4 = -1;
                    double Yr1 = -1, Yr2 = -1, Yr3 = -1, Yr4 = -1;

                    //Find top two vertices of the left polygon of picking aisle
                    for (int k = 0; k < regions[i].pickingaisleedges[j].getOnEdgeNodes().Count; k++)
                    {
                        if (regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s1 != null)
                        {
                            Xl1 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s1.X1;
                            Yl1 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s1.Y1;
                            Xl2 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s1.X2;
                            Yl2 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s1.Y2;
                            isthereleftstorage = true;
                            break;
                        }
                    }
                    if (isthereleftstorage)
                    {
                        //Find bottom two vertices of the left polygon of picking aisle, start from other side
                        for (int k = regions[i].pickingaisleedges[j].getOnEdgeNodes().Count - 1; k >= 0; k--)
                        {
                            if (regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s1 != null)
                            {
                                Xl3 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s1.X3;
                                Yl3 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s1.Y3;
                                Xl4 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s1.X4;
                                Yl4 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s1.Y4;
                                isthereleftstorage = true;
                                break;
                            }
                        }
                    }

                    //Find top two vertices of the right polygon of picking aisle
                    for (int k = 0; k < regions[i].pickingaisleedges[j].getOnEdgeNodes().Count; k++)
                    {
                        if (regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s2 != null)
                        {
                            Xr1 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s2.X1;
                            Yr1 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s2.Y1;
                            Xr2 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s2.X2;
                            Yr2 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s2.Y2;
                            isthererightstorage = true;
                            break;
                        }
                    }
                    if (isthererightstorage)
                    {
                        //Find bottom two vertices of the right polygon of picking aisle, start from other side
                        for (int k = regions[i].pickingaisleedges[j].getOnEdgeNodes().Count - 1; k >= 0; k--)
                        {
                            if (regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s2 != null)
                            {
                                Xr3 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s2.X3;
                                Yr3 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s2.Y3;
                                Xr4 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s2.X4;
                                Yr4 = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k].s2.Y4;
                                isthererightstorage = true;
                                break;
                            }
                        }
                    }

                    if (isthereleftstorage)
                    {
                        polygon leftpoly = new polygon();
                        node l1, l2, l3, l4;
                        //Fix for precision error for polygons
                        Xl1 += ff * (Xl2 - Xl1);
                        Yl1 += ff * (Yl2 - Yl1);
                        Xl3 += ff * (Xl4 - Xl3);
                        Yl3 += ff * (Yl4 - Yl3);

                        //Increase size of polygons for picker size to have a buffer region
                        double Xl2l4 = Xl2 - Xl4;
                        double Yl2l4 = Yl2 - Yl4;
                        double Xl3l4 = Xl3 - Xl4;
                        double Yl3l4 = Yl3 - Yl4;
                        double Xl1l2 = Xl1 - Xl2;
                        double Yl1l2 = Yl1 - Yl2;
                        double Xl1l3 = Xl1 - Xl3;
                        double Yl1l3 = Yl1 - Yl3;
                        double dl2l4 = visualmath.distance(Xl2, Yl2, Xl4, Yl4);
                        double dl3l4 = visualmath.distance(Xl3, Yl3, Xl4, Yl4);
                        double dl1l2 = visualmath.distance(Xl1, Yl1, Xl2, Yl2);
                        double dl1l3 = visualmath.distance(Xl1, Yl1, Xl3, Yl3);
                        Xl1 += ((Xl1l3 * (pickersize / 2)) / dl1l3);
                        Yl1 += ((Yl1l3 * (pickersize / 2)) / dl1l3);
                        Xl2 += (((Xl2l4 * (pickersize / 2)) / dl2l4) - ((Xl1l2 * (pickersize / 2)) / dl1l2));
                        Yl2 += (((Yl2l4 * (pickersize / 2)) / dl2l4) - ((Yl1l2 * (pickersize / 2)) / dl1l2));
                        Xl3 -= ((Xl1l3 * (pickersize / 2)) / dl1l3);
                        Yl3 -= ((Yl1l3 * (pickersize / 2)) / dl1l3);
                        Xl4 -= (((Xl2l4 * (pickersize / 2)) / dl2l4) + ((Xl3l4 * (pickersize / 2)) / dl3l4));
                        Yl4 -= (((Yl2l4 * (pickersize / 2)) / dl2l4) + ((Yl3l4 * (pickersize / 2)) / dl3l4));

                        l1 = new node(Xl1, Yl1, options.polygonnode); l1.setPickingAisleEdge(regions[i].pickingaisleedges[j]);
                        l2 = new node(Xl2, Yl2, options.polygonnode); l2.setPickingAisleEdge(regions[i].pickingaisleedges[j]);
                        l3 = new node(Xl3, Yl3, options.polygonnode); l3.setPickingAisleEdge(regions[i].pickingaisleedges[j]);
                        l4 = new node(Xl4, Yl4, options.polygonnode); l4.setPickingAisleEdge(regions[i].pickingaisleedges[j]);
                        //Create polygon for finding intersections of storage locations with polygons
                        leftpoly.addVector(Xl1, Yl1);
                        leftpoly.addVector(Xl3, Yl3);
                        leftpoly.addVector(Xl4, Yl4);
                        leftpoly.addVector(Xl2, Yl2);

                        //These connections are done in serial so no problem since they are in the same thread
                        //l1.connectvisibilitygraph(l3);
                        l3.connectvisibilitygraph(l4);
                        l4.connectvisibilitygraph(l2);
                        l2.connectvisibilitygraph(l1);

                        l1.indexpolygon = h; l1.indexpolygonlocation = 11; l1.indexregion = regions[i].getRegionID(); l1.indexpickingaisle = regions[i].pickingaisleedges[j].id; graphnodes[g] = l1; g++;
                        l3.indexpolygon = h; l3.indexpolygonlocation = 13; l3.indexregion = regions[i].getRegionID(); l3.indexpickingaisle = regions[i].pickingaisleedges[j].id; graphnodes[g] = l3; g++;
                        l4.indexpolygon = h; l4.indexpolygonlocation = 14; l4.indexregion = regions[i].getRegionID(); l4.indexpickingaisle = regions[i].pickingaisleedges[j].id; graphnodes[g] = l4; g++;
                        l2.indexpolygon = h; l2.indexpolygonlocation = 12; l2.indexregion = regions[i].getRegionID(); l2.indexpickingaisle = regions[i].pickingaisleedges[j].id; graphnodes[g] = l2; g++;
                        polygons[h] = leftpoly; h++;
                    }
                    if (isthererightstorage)
                    {
                        polygon rightpoly = new polygon();
                        node r1, r2, r3, r4;
                        //Fix for precision error for polygons, polygons are entering inside each other and creating problems with visibility graph so this is a fix by shrinking the touching parts of the polygons
                        Xr2 += ff * (Xr1 - Xr2);
                        Yr2 += ff * (Yr1 - Yr2);
                        Xr4 += ff * (Xr3 - Xr4);
                        Yr4 += ff * (Yr3 - Yr4);

                        //Increase size of polygons for picker size to have a buffer region
                        double Xr2r4 = Xr2 - Xr4;
                        double Yr2r4 = Yr2 - Yr4;
                        double Xr3r4 = Xr3 - Xr4;
                        double Yr3r4 = Yr3 - Yr4;
                        double Xr1r2 = Xr1 - Xr2;
                        double Yr1r2 = Yr1 - Yr2;
                        double Xr1r3 = Xr1 - Xr3;
                        double Yr1r3 = Yr1 - Yr3;
                        double dr2r4 = visualmath.distance(Xr2, Yr2, Xr4, Yr4);
                        double dr3r4 = visualmath.distance(Xr3, Yr3, Xr4, Yr4);
                        double dr1r2 = visualmath.distance(Xr1, Yr1, Xr2, Yr2);
                        double dr1r3 = visualmath.distance(Xr1, Yr1, Xr3, Yr3);
                        Xr1 += (((Xr1r3 * (pickersize / 2) ) / dr1r3) + ((Xr1r2 * (pickersize / 2)) / dr1r2));
                        Yr1 += (((Yr1r3 * (pickersize / 2)) / dr1r3) + ((Yr1r2 * (pickersize / 2)) / dr1r2));
                        Xr2 += ((Xr2r4 * (pickersize / 2)) / dr2r4);
                        Yr2 += ((Yr2r4 * (pickersize / 2)) / dr2r4);
                        Xr3 -= (((Xr1r3 * (pickersize / 2)) / dr1r3) - ((Xr3r4 * (pickersize / 2)) / dr3r4));
                        Yr3 -= (((Yr1r3 * (pickersize / 2)) / dr1r3) - ((Yr3r4 * (pickersize / 2)) / dr3r4));
                        Xr4 -= ((Xr2r4 * (pickersize / 2)) / dr2r4);
                        Yr4 -= ((Yr2r4 * (pickersize / 2)) / dr2r4);

                        r1 = new node(Xr1, Yr1, options.polygonnode); r1.setPickingAisleEdge(regions[i].pickingaisleedges[j]);
                        r2 = new node(Xr2, Yr2, options.polygonnode); r2.setPickingAisleEdge(regions[i].pickingaisleedges[j]);
                        r3 = new node(Xr3, Yr3, options.polygonnode); r3.setPickingAisleEdge(regions[i].pickingaisleedges[j]);
                        r4 = new node(Xr4, Yr4, options.polygonnode); r4.setPickingAisleEdge(regions[i].pickingaisleedges[j]);
                        //Create polygon for finding intersections of storage locations with polygons
                        rightpoly.addVector(Xr2, Yr2);
                        rightpoly.addVector(Xr4, Yr4);
                        rightpoly.addVector(Xr3, Yr3);
                        rightpoly.addVector(Xr1, Yr1);

                        //These connections are done in serial so no problem since they are in the same thread
                        //r2.connectvisibilitygraph(r4);
                        r4.connectvisibilitygraph(r3);
                        r3.connectvisibilitygraph(r1);
                        r1.connectvisibilitygraph(r2);

                        r2.indexpolygon = h; r2.indexpolygonlocation = 22; r2.indexregion = regions[i].getRegionID(); r2.indexpickingaisle = regions[i].pickingaisleedges[j].id; graphnodes[g] = r2; g++;
                        r4.indexpolygon = h; r4.indexpolygonlocation = 24; r4.indexregion = regions[i].getRegionID(); r4.indexpickingaisle = regions[i].pickingaisleedges[j].id; graphnodes[g] = r4; g++;
                        r3.indexpolygon = h; r3.indexpolygonlocation = 23; r3.indexregion = regions[i].getRegionID(); r3.indexpickingaisle = regions[i].pickingaisleedges[j].id; graphnodes[g] = r3; g++;
                        r1.indexpolygon = h; r1.indexpolygonlocation = 21; r1.indexregion = regions[i].getRegionID(); r1.indexpickingaisle = regions[i].pickingaisleedges[j].id; graphnodes[g] = r1; g++;
                        polygons[h] = rightpoly; h++;
                    }
                    //Assign all location nodes to graphnodes array
                    for (int k = 0; k < regions[i].pickingaisleedges[j].getOnEdgeNodes().Count; k++)
                    {
                        graphnodes[g] = regions[i].pickingaisleedges[j].getOnEdgeNodes()[k];
                        if(isthereleftstorage && isthererightstorage)//Only right polygon
                        {
                            graphnodes[g].indexpolygonleft = h - 2;
                            graphnodes[g].indexpolygonright = h - 1;
                        }
                        else if(isthereleftstorage)//Only left polygon
                        {
                            graphnodes[g].indexpolygonleft = h - 1;
                            graphnodes[g].indexpolygonright = - 1;//No right polygon
                        }
                        else if(isthererightstorage)//Only right polygon
                        {
                            graphnodes[g].indexpolygonleft = - 1;//No left polygon
                            graphnodes[g].indexpolygonright = h - 1;
                        }
                        else
                        {
                            graphnodes[g].indexpolygonleft = -1;//No left polygon
                            graphnodes[g].indexpolygonright = -1;//No right polygon
                        }
                        g++;
                    }
                });
            });
            //Finally add PD point to graphnodes
            graphnodes[graphnodes.Length - 1] = pdnodes[0];
        }

        public void createVisibilityGraph()
        {
            //for (int i = 0; i < graphnodes.Count; i++)
            Parallel.For(0, graphnodes.Length, i =>
            {
                graphnodes[i].indexgraphnode = i;
            });
            connectivity = new bool[graphnodes.Length, graphnodes.Length];
            for(int i = 0; i < graphnodes.Length; i++)
            //Parallel.For(0, graphnodes.Length, i =>
            {
                //bool haspickingaisle =false;
                //if(graphnodes[i].getPickingAisleEdge() != null) haspickingaisle = true;
                for (int j = 0; j < i; j++)
                //Parallel.For(0, i, j =>
                {
                    //if (haspickingaisle && graphnodes[j].getPickingAisleEdge() != null && graphnodes[i].getPickingAisleEdge().id == graphnodes[j].getPickingAisleEdge().id)
                    //{
                    //    lock (this)
                    //    {
                    //        graphnodes[i].connectvisibilitygraph(graphnodes[j]);//They become neighbors which means their distance is euclidean distance in shortest path
                    //    }
                    //}
                    if (shouldConnect(graphnodes[i], graphnodes[j]))//Check if they should connect with polygons, returns true if it should be connected
                    {
                        connectivity[i, j] = true;
                    }
                    else
                    {
                        connectivity[i, j] = false;
                    }
                };
            };

            //csvexport myexcel = new csvexport();
            //for (int i = 0; i < connectivity.GetLength(0); i++)
            //{
            //    for (int j = 0; j < i; j++)
            //    {
            //        myexcel.addRow();
            //        myexcel["connectivity"] = connectivity[i, j].ToString();
            //        myexcel["X1"] = graphnodes[i].getX().ToString();
            //        myexcel["X2"] = graphnodes[j].getX().ToString();
            //        myexcel["Y1"] = graphnodes[i].getY().ToString();
            //        myexcel["Y2"] = graphnodes[j].getY().ToString();
            //    }
            //}
            //myexcel.exportToFile("connectivity-" + exteriornodes[0].getLocation().ToString() + ".csv");

            for (int i = 0; i < graphnodes.Length; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (connectivity[i, j])
                    {
                        graphnodes[i].connectvisibilitygraph(graphnodes[j]);//They become neighbors which means their distance is euclidean distance in shortest path
                    }
                }
            }
        }

        private void findPolygonNeighbors()
        {
            for(int i = 0; i < polygons.Length; i++)
            {
                for(int j = 0; j < i; j++)
                {
                    
                }
            }
        }

        /// <summary>
        /// Returns false when it does not intersect with any polygon, this code does not work when both nodes are polygon nodes
        /// </summary>
        /// <param name="p_node1"></param>
        /// <param name="p_node2"></param>
        /// <returns></returns>
        public bool doesIntersectwithPolygon(node p_node1, node p_node2)
        {
            const int polygonedgecount = 4;//This is fixed since they are all rectangle shaped
            //Smart check for locationnodes, first check polygons in that aisle because it will intersect with them first most likely
            if (p_node1.type == options.locationnode && p_node2.type == options.locationnode)
            {
                for (int k = 0; k < 4; k++)//There are four possible polygons to check
                {
                    int i = -1;
                    if (k == 0 && p_node1.indexpolygonleft != -1)//There is a left polygon for node1
                    {
                        i = p_node1.indexpolygonleft;
                    }
                    else if(k == 1 && p_node1.indexpolygonright != -1)//There is a right polygon for node1
                    {
                        i = p_node1.indexpolygonright;
                    }
                    else if (k == 2 && p_node2.indexpolygonleft != -1)//There is a left polygon for node2
                    {
                        i = p_node2.indexpolygonleft;
                    }
                    else if (k == 3 && p_node2.indexpolygonright != -1)//There is a right polygon for node2
                    {
                        i = p_node2.indexpolygonright;
                    }
                    if (i != -1)
                    {
                        for (int j = 0; j < polygonedgecount - 1; j++)
                        {
                            if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), polygons[i].vectors[j].X, polygons[i].vectors[j].Y, polygons[i].vectors[j + 1].X, polygons[i].vectors[j + 1].Y))
                                return true;
                        }
                        if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), polygons[i].vectors[polygonedgecount - 1].X, polygons[i].vectors[polygonedgecount - 1].Y, polygons[i].vectors[0].X, polygons[i].vectors[0].Y))
                            return true;
                    }
                }
            }
            else if(p_node1.type == options.locationnode && p_node2.type != options.locationnode)
            {
                for (int k = 0; k < 2; k++)//There are two possible polygons to check
                {
                    int i = -1;
                    if (k == 0 && p_node1.indexpolygonleft != -1)//There is a left polygon for node1
                    {
                        i = p_node1.indexpolygonleft;
                    }
                    else if (k == 1 && p_node1.indexpolygonright != -1)//There is a right polygon for node1
                    {
                        i = p_node1.indexpolygonright;
                    }
                    
                    if (i != -1)
                    {
                        for (int j = 0; j < polygonedgecount - 1; j++)
                        {
                            if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), polygons[i].vectors[j].X, polygons[i].vectors[j].Y, polygons[i].vectors[j + 1].X, polygons[i].vectors[j + 1].Y))
                                return true;
                        }
                        if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), polygons[i].vectors[polygonedgecount - 1].X, polygons[i].vectors[polygonedgecount - 1].Y, polygons[i].vectors[0].X, polygons[i].vectors[0].Y))
                            return true;
                    }
                }
            }
            else if (p_node1.type != options.locationnode && p_node2.type == options.locationnode)
            {
                for (int k = 0; k < 2; k++)//There are two possible polygons to check
                {
                    int i = -1;
                    if (k == 0 && p_node2.indexpolygonleft != -1)//There is a left polygon for node2
                    {
                        i = p_node2.indexpolygonleft;
                    }
                    else if (k == 1 && p_node2.indexpolygonright != -1)//There is a right polygon for node2
                    {
                        i = p_node2.indexpolygonright;
                    }

                    if (i != -1)
                    {
                        for (int j = 0; j < polygonedgecount - 1; j++)
                        {
                            if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), polygons[i].vectors[j].X, polygons[i].vectors[j].Y, polygons[i].vectors[j + 1].X, polygons[i].vectors[j + 1].Y))
                                return true;
                        }
                        if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), polygons[i].vectors[polygonedgecount - 1].X, polygons[i].vectors[polygonedgecount - 1].Y, polygons[i].vectors[0].X, polygons[i].vectors[0].Y))
                            return true;
                    }
                }
            }
            //End of smart check, if it does not return from here then check the other polygons

            for (int i = 0; i < polygons.Length; i++)
            {
                for (int j = 0; j < polygonedgecount - 1; j++)
                {
                    if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), polygons[i].vectors[j].X, polygons[i].vectors[j].Y, polygons[i].vectors[j + 1].X, polygons[i].vectors[j + 1].Y))
                        return true;
                }
                if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), polygons[i].vectors[polygonedgecount - 1].X, polygons[i].vectors[polygonedgecount - 1].Y, polygons[i].vectors[0].X, polygons[i].vectors[0].Y))
                    return true;
            }
            return false;//Does not intersect
        }

        /// <summary>
        /// Returns false when it does not intersect with any polygon, this code is for polygonnode to polygonnode checks only
        /// </summary>
        /// <param name="p_node1"></param>
        /// <param name="p_node2"></param>
        /// <returns></returns>
        public bool doesIntersectwithPolygonTwoPolygonNodes(node p_node1, node p_node2)
        {
            const int polygonedgecount = 4;//This is fixed since they are all rectangle shaped
            for (int i = 0; i < polygons.Length; i++)
            {
                for (int j = 0; j < polygonedgecount - 1; j++)
                {
                    if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), polygons[i].vectors[j].X, polygons[i].vectors[j].Y, polygons[i].vectors[j + 1].X, polygons[i].vectors[j + 1].Y))
                        return true;
                }
                if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), polygons[i].vectors[polygonedgecount - 1].X, polygons[i].vectors[polygonedgecount - 1].Y, polygons[i].vectors[0].X, polygons[i].vectors[0].Y))
                    return true;
            }
            //if(p_node1.type == options.polygonnode)
            //{
            //    if (isInsidePolygon(p_node1.getX(), p_node1.getY(), polygons[i]) || isInsidePolygon(p_node2.getX(), p_node2.getY(), polygons[i]))
            //    {
            //        return true;
            //    }
            //}
            for (int i = 0; i < polygons.Length; i++)
            {
                if(isInsidePolygon(p_node1.getX(), p_node1.getY(), polygons[i]) || isInsidePolygon(p_node2.getX(), p_node2.getY(), polygons[i]))
                {
                    return true;
                }
            }
            return false;//Does not intersect
        }

        /// <summary>
        /// Check if point is inside the polygon
        /// </summary>
        /// <param name="p_X"></param>
        /// <param name="p_Y"></param>
        /// <param name="p_polygon"></param>
        /// <returns></returns>
        private bool isInsidePolygon(double p_X, double p_Y, polygon p_polygon)//This is used for the precision error that creates wrong visibility connections because polygons are inside each other
        {
            const int polygonedgecount = 4;//This is fixed since they are all rectangle shaped
            bool isInside = false;
            for (int i = 0, j = polygonedgecount - 1; i < polygonedgecount; j = i++)
            {
                if (((p_polygon.vectors[i].Y > p_Y) != (p_polygon.vectors[j].Y > p_Y)) &&
                    (p_X < (p_polygon.vectors[j].X - p_polygon.vectors[i].X) * (p_Y - p_polygon.vectors[i].Y) /
                    (p_polygon.vectors[j].Y - p_polygon.vectors[i].Y) + p_polygon.vectors[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }



        /// <summary>
        /// This is another layer whether two nodes should be connected or not, returns false if should not be connected
        /// </summary>
        /// <param name="p_node1"></param>
        /// <param name="p_node2"></param>
        /// <returns></returns>
        public bool shouldConnect(node p_node1, node p_node2)
        {
            //If else if statements are going in sequence, the sequence of these statements are important
            if (p_node1.isconnectedVisibilityGraph(p_node2) != null)//This means that they are already connected at the creating polygons part so these nodes are a part of the polygon
            {
                return true; //They are already connected no need to connect them again
            }
            if (p_node1.type == options.locationnode && p_node2.type == options.locationnode && p_node1.getPickingAisleEdge().id == p_node2.getPickingAisleEdge().id)
            {
                return true; //They are in the same picking aisle so they should be connected
            }
            else if (p_node1.type == options.polygonnode && p_node2.type == options.polygonnode && p_node1.indexpolygon == p_node2.indexpolygon)//If they are both polygonnode and if they are in the same polygon then do not try to connect them
            {
                return false; //They are not connected and they are not supposed to be connected
            }
            else if (p_node1.type == options.polygonnode && p_node2.type == options.polygonnode && p_node1.indexregion == p_node2.indexregion)//If they are both polygonnode and they are in the same region then do some checks for special connections of polygons
            {
                //No check
                if((p_node1.indexpolygonlocation == 11 && p_node2.indexpolygonlocation == 22) || (p_node1.indexpolygonlocation == 22 && p_node2.indexpolygonlocation == 11))//l1-r2
                {
                    return false; //They are not supposed to be connected
                }
                else if ((p_node1.indexpolygonlocation == 13 && p_node2.indexpolygonlocation == 24) || (p_node1.indexpolygonlocation == 24 && p_node2.indexpolygonlocation == 13))//l3-r4
                {
                    return false; //They are not supposed to be connected
                }
                //l3-l4 line check
                else if ((p_node1.indexpolygonlocation == 13 && p_node2.indexpolygonlocation == 22))//l3-r2
                {
                    double xl1 = polygons[p_node1.indexpolygon].vectors[0].X;
                    double yl1 = polygons[p_node1.indexpolygon].vectors[0].Y;
                    double xl3 = polygons[p_node1.indexpolygon].vectors[1].X;
                    double yl3 = polygons[p_node1.indexpolygon].vectors[1].Y;
                    double xl4 = polygons[p_node1.indexpolygon].vectors[2].X;
                    double yl4 = polygons[p_node1.indexpolygon].vectors[2].Y;

                    if (visualmath.pointSide(xl3, yl3, xl4, yl4, xl1, yl1) == visualmath.pointSide(xl3, yl3, xl4, yl4, p_node2.getX(), p_node2.getY()))// No gap
                    {
                        return false;
                    }
                    else
                    {
                        return !doesIntersectwithPolygon(p_node1, p_node2);
                    }
                }
                else if ((p_node1.indexpolygonlocation == 22 && p_node2.indexpolygonlocation == 13))//r2-l3
                {
                    double xl1 = polygons[p_node2.indexpolygon].vectors[0].X;
                    double yl1 = polygons[p_node2.indexpolygon].vectors[0].Y;
                    double xl3 = polygons[p_node2.indexpolygon].vectors[1].X;
                    double yl3 = polygons[p_node2.indexpolygon].vectors[1].Y;
                    double xl4 = polygons[p_node2.indexpolygon].vectors[2].X;
                    double yl4 = polygons[p_node2.indexpolygon].vectors[2].Y;

                    if (visualmath.pointSide(xl3, yl3, xl4, yl4, xl1, yl1) == visualmath.pointSide(xl3, yl3, xl4, yl4, p_node1.getX(), p_node1.getY()))// No gap
                    {
                        return false;
                    }
                    else
                    {
                        return !doesIntersectwithPolygon(p_node1, p_node2);
                    }
                }
                else if ((p_node1.indexpolygonlocation == 13 && p_node2.indexpolygonlocation == 21))//l3-r1
                {
                    double xl1 = polygons[p_node1.indexpolygon].vectors[0].X;
                    double yl1 = polygons[p_node1.indexpolygon].vectors[0].Y;
                    double xl3 = polygons[p_node1.indexpolygon].vectors[1].X;
                    double yl3 = polygons[p_node1.indexpolygon].vectors[1].Y;
                    double xl4 = polygons[p_node1.indexpolygon].vectors[2].X;
                    double yl4 = polygons[p_node1.indexpolygon].vectors[2].Y;

                    if (visualmath.pointSide(xl3, yl3, xl4, yl4, xl1, yl1) == visualmath.pointSide(xl3, yl3, xl4, yl4, p_node2.getX(), p_node2.getY()))// No gap
                    {
                        return false;
                    }
                    else
                    {
                        return !doesIntersectwithPolygon(p_node1, p_node2);
                    }
                }
                else if ((p_node1.indexpolygonlocation == 21 && p_node2.indexpolygonlocation == 13))//r1-l3
                {
                    double xl1 = polygons[p_node2.indexpolygon].vectors[0].X;
                    double yl1 = polygons[p_node2.indexpolygon].vectors[0].Y;
                    double xl3 = polygons[p_node2.indexpolygon].vectors[1].X;
                    double yl3 = polygons[p_node2.indexpolygon].vectors[1].Y;
                    double xl4 = polygons[p_node2.indexpolygon].vectors[2].X;
                    double yl4 = polygons[p_node2.indexpolygon].vectors[2].Y;

                    if (visualmath.pointSide(xl3, yl3, xl4, yl4, xl1, yl1) == visualmath.pointSide(xl3, yl3, xl4, yl4, p_node1.getX(), p_node1.getY()))// No gap
                    {
                        return false;
                    }
                    else
                    {
                        return !doesIntersectwithPolygon(p_node1, p_node2);
                    }
                }
                else if ((p_node1.indexpolygonlocation == 14 && p_node2.indexpolygonlocation == 22))//l4-r2
                {
                    double xl1 = polygons[p_node1.indexpolygon].vectors[0].X;
                    double yl1 = polygons[p_node1.indexpolygon].vectors[0].Y;
                    double xl3 = polygons[p_node1.indexpolygon].vectors[1].X;
                    double yl3 = polygons[p_node1.indexpolygon].vectors[1].Y;
                    double xl4 = polygons[p_node1.indexpolygon].vectors[2].X;
                    double yl4 = polygons[p_node1.indexpolygon].vectors[2].Y;

                    if (visualmath.pointSide(xl3, yl3, xl4, yl4, xl1, yl1) == visualmath.pointSide(xl3, yl3, xl4, yl4, p_node2.getX(), p_node2.getY()))// No gap
                    {
                        return false;
                    }
                    else
                    {
                        return !doesIntersectwithPolygon(p_node1, p_node2);
                    }
                }
                else if ((p_node1.indexpolygonlocation == 22 && p_node2.indexpolygonlocation == 14))//r2-l4
                {
                    double xl1 = polygons[p_node2.indexpolygon].vectors[0].X;
                    double yl1 = polygons[p_node2.indexpolygon].vectors[0].Y;
                    double xl3 = polygons[p_node2.indexpolygon].vectors[1].X;
                    double yl3 = polygons[p_node2.indexpolygon].vectors[1].Y;
                    double xl4 = polygons[p_node2.indexpolygon].vectors[2].X;
                    double yl4 = polygons[p_node2.indexpolygon].vectors[2].Y;

                    if (visualmath.pointSide(xl3, yl3, xl4, yl4, xl1, yl1) == visualmath.pointSide(xl3, yl3, xl4, yl4, p_node1.getX(), p_node1.getY()))// No gap
                    {
                        return false;
                    }
                    else
                    {
                        return !doesIntersectwithPolygon(p_node1, p_node2);
                    }
                }
                //l1-l2 line check
                else if ((p_node1.indexpolygonlocation == 11 && p_node2.indexpolygonlocation == 24))//l1-r4
                {
                    double xl1 = polygons[p_node1.indexpolygon].vectors[0].X;
                    double yl1 = polygons[p_node1.indexpolygon].vectors[0].Y;
                    double xl2 = polygons[p_node1.indexpolygon].vectors[3].X;
                    double yl2 = polygons[p_node1.indexpolygon].vectors[3].Y;
                    double xl4 = polygons[p_node1.indexpolygon].vectors[2].X;
                    double yl4 = polygons[p_node1.indexpolygon].vectors[2].Y;

                    if (visualmath.pointSide(xl1, yl1, xl2, yl2, xl4, yl4) == visualmath.pointSide(xl1, yl1, xl2, yl2, p_node2.getX(), p_node2.getY()))// No gap
                    {
                        return false;
                    }
                    else
                    {
                        return !doesIntersectwithPolygon(p_node1, p_node2);
                    }
                }
                else if ((p_node1.indexpolygonlocation == 24 && p_node2.indexpolygonlocation == 11))//r4-l1
                {
                    double xl1 = polygons[p_node2.indexpolygon].vectors[0].X;
                    double yl1 = polygons[p_node2.indexpolygon].vectors[0].Y;
                    double xl2 = polygons[p_node2.indexpolygon].vectors[3].X;
                    double yl2 = polygons[p_node2.indexpolygon].vectors[3].Y;
                    double xl4 = polygons[p_node2.indexpolygon].vectors[2].X;
                    double yl4 = polygons[p_node2.indexpolygon].vectors[2].Y;

                    if (visualmath.pointSide(xl1, yl1, xl2, yl2, xl4, yl4) == visualmath.pointSide(xl1, yl1, xl2, yl2, p_node1.getX(), p_node1.getY()))// No gap
                    {
                        return false;
                    }
                    else
                    {
                        return !doesIntersectwithPolygon(p_node1, p_node2);
                    }
                }
                else if ((p_node1.indexpolygonlocation == 11 && p_node2.indexpolygonlocation == 23))//l1-r3
                {
                    double xl1 = polygons[p_node1.indexpolygon].vectors[0].X;
                    double yl1 = polygons[p_node1.indexpolygon].vectors[0].Y;
                    double xl2 = polygons[p_node1.indexpolygon].vectors[3].X;
                    double yl2 = polygons[p_node1.indexpolygon].vectors[3].Y;
                    double xl4 = polygons[p_node1.indexpolygon].vectors[2].X;
                    double yl4 = polygons[p_node1.indexpolygon].vectors[2].Y;

                    if (visualmath.pointSide(xl1, yl1, xl2, yl2, xl4, yl4) == visualmath.pointSide(xl1, yl1, xl2, yl2, p_node2.getX(), p_node2.getY()))// No gap
                    {
                        return false;
                    }
                    else
                    {
                        return !doesIntersectwithPolygon(p_node1, p_node2);
                    }
                }
                else if ((p_node1.indexpolygonlocation == 23 && p_node2.indexpolygonlocation == 11))//r3-l1
                {
                    double xl1 = polygons[p_node2.indexpolygon].vectors[0].X;
                    double yl1 = polygons[p_node2.indexpolygon].vectors[0].Y;
                    double xl2 = polygons[p_node2.indexpolygon].vectors[3].X;
                    double yl2 = polygons[p_node2.indexpolygon].vectors[3].Y;
                    double xl4 = polygons[p_node2.indexpolygon].vectors[2].X;
                    double yl4 = polygons[p_node2.indexpolygon].vectors[2].Y;

                    if (visualmath.pointSide(xl1, yl1, xl2, yl2, xl4, yl4) == visualmath.pointSide(xl1, yl1, xl2, yl2, p_node1.getX(), p_node1.getY()))// No gap
                    {
                        return false;
                    }
                    else
                    {
                        return !doesIntersectwithPolygon(p_node1, p_node2);
                    }
                }
                else if ((p_node1.indexpolygonlocation == 12 && p_node2.indexpolygonlocation == 24))//l2-r4
                {
                    double xl1 = polygons[p_node1.indexpolygon].vectors[0].X;
                    double yl1 = polygons[p_node1.indexpolygon].vectors[0].Y;
                    double xl2 = polygons[p_node1.indexpolygon].vectors[3].X;
                    double yl2 = polygons[p_node1.indexpolygon].vectors[3].Y;
                    double xl4 = polygons[p_node1.indexpolygon].vectors[2].X;
                    double yl4 = polygons[p_node1.indexpolygon].vectors[2].Y;

                    if (visualmath.pointSide(xl1, yl1, xl2, yl2, xl4, yl4) == visualmath.pointSide(xl1, yl1, xl2, yl2, p_node2.getX(), p_node2.getY()))// No gap
                    {
                        return false;
                    }
                    else
                    {
                        return !doesIntersectwithPolygon(p_node1, p_node2);
                    }
                }
                else if ((p_node1.indexpolygonlocation == 24 && p_node2.indexpolygonlocation == 12))//r4-l2
                {
                    double xl1 = polygons[p_node2.indexpolygon].vectors[0].X;
                    double yl1 = polygons[p_node2.indexpolygon].vectors[0].Y;
                    double xl2 = polygons[p_node2.indexpolygon].vectors[3].X;
                    double yl2 = polygons[p_node2.indexpolygon].vectors[3].Y;
                    double xl4 = polygons[p_node2.indexpolygon].vectors[2].X;
                    double yl4 = polygons[p_node2.indexpolygon].vectors[2].Y;

                    if (visualmath.pointSide(xl1, yl1, xl2, yl2, xl4, yl4) == visualmath.pointSide(xl1, yl1, xl2, yl2, p_node1.getX(), p_node1.getY()))// No gap
                    {
                        return false;
                    }
                    else
                    {
                        return !doesIntersectwithPolygon(p_node1, p_node2);
                    }
                }
                else
                {
                    return !doesIntersectwithPolygon(p_node1, p_node2);
                }
            }
            else if (p_node1.type == options.polygonnode && p_node2.type == options.polygonnode)//This means that they are in different regions but they are both polygonnodes
            {
                //Check if they go through invisible aisles that are between back to back polygons
                //We use a trick
                //For example for l1 we extend l3 l4 line by making l3 side a little bit longer so it will not be able to connect through that invisible aisle to a point in another region
                double X1 = -1;
                double X2 = -1;
                double Y1 = -1;
                double Y2 = -1;
                double ff = 0.000000001;//Extending factor used for extending l3 a little bit for checking l1 intersection through invisible aisle
                if ((p_node1.indexpolygonlocation == 11))
                {
                    X1 = polygons[p_node1.indexpolygon].vectors[1].X + ff * (polygons[p_node1.indexpolygon].vectors[1].X - polygons[p_node1.indexpolygon].vectors[2].X);//Extend l3 X
                    Y1 = polygons[p_node1.indexpolygon].vectors[1].Y + ff * (polygons[p_node1.indexpolygon].vectors[1].Y - polygons[p_node1.indexpolygon].vectors[2].Y);//Extend l3 Y
                    X2 = polygons[p_node1.indexpolygon].vectors[2].X; //l4 x
                    Y2 = polygons[p_node1.indexpolygon].vectors[2].Y; //l4 Y
                    if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), X1, Y1, X2, Y2))//If the line intersects with extended l3-l4 then it should not connect
                        return false;
                }
                else if ((p_node1.indexpolygonlocation == 13))
                {
                    X1 = polygons[p_node1.indexpolygon].vectors[0].X + ff * (polygons[p_node1.indexpolygon].vectors[0].X - polygons[p_node1.indexpolygon].vectors[3].X);//Extend l1 X
                    Y1 = polygons[p_node1.indexpolygon].vectors[0].Y + ff * (polygons[p_node1.indexpolygon].vectors[0].Y - polygons[p_node1.indexpolygon].vectors[3].Y);//Extend l1 Y
                    X2 = polygons[p_node1.indexpolygon].vectors[3].X; //l2 x
                    Y2 = polygons[p_node1.indexpolygon].vectors[3].Y; //l2 Y
                    if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), X1, Y1, X2, Y2))//If the line intersects with extended l1-l2 then it should not connect
                        return false;
                }
                else if ((p_node1.indexpolygonlocation == 22))
                {
                    X1 = polygons[p_node1.indexpolygon].vectors[1].X + ff * (polygons[p_node1.indexpolygon].vectors[1].X - polygons[p_node1.indexpolygon].vectors[2].X);//Extend r4 X
                    Y1 = polygons[p_node1.indexpolygon].vectors[1].Y + ff * (polygons[p_node1.indexpolygon].vectors[1].Y - polygons[p_node1.indexpolygon].vectors[2].Y);//Extend r4 Y
                    X2 = polygons[p_node1.indexpolygon].vectors[2].X; //r3 x
                    Y2 = polygons[p_node1.indexpolygon].vectors[2].Y; //r3 Y
                    if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), X1, Y1, X2, Y2))//If the line intersects with extended r3-r4 then it should not connect
                        return false;
                }
                else if ((p_node1.indexpolygonlocation == 24))
                {
                    X1 = polygons[p_node1.indexpolygon].vectors[0].X + ff * (polygons[p_node1.indexpolygon].vectors[0].X - polygons[p_node1.indexpolygon].vectors[3].X);//Extend r2 X
                    Y1 = polygons[p_node1.indexpolygon].vectors[0].Y + ff * (polygons[p_node1.indexpolygon].vectors[0].Y - polygons[p_node1.indexpolygon].vectors[3].Y);//Extend r2 Y
                    X2 = polygons[p_node1.indexpolygon].vectors[3].X; //r1 x
                    Y2 = polygons[p_node1.indexpolygon].vectors[3].Y; //r1 Y
                    if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), X1, Y1, X2, Y2))//If the line intersects with extended r1-r2 then it should not connect
                        return false;
                }
                if ((p_node2.indexpolygonlocation == 11))
                {
                    X1 = polygons[p_node2.indexpolygon].vectors[1].X + ff * (polygons[p_node2.indexpolygon].vectors[1].X - polygons[p_node2.indexpolygon].vectors[2].X);//Extend l3 X
                    Y1 = polygons[p_node2.indexpolygon].vectors[1].Y + ff * (polygons[p_node2.indexpolygon].vectors[1].Y - polygons[p_node2.indexpolygon].vectors[2].Y);//Extend l3 Y
                    X2 = polygons[p_node2.indexpolygon].vectors[2].X; //l4 x
                    Y2 = polygons[p_node2.indexpolygon].vectors[2].Y; //l4 Y
                    if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), X1, Y1, X2, Y2))//If the line intersects with extended l3-l4 then it should not connect
                        return false;
                }
                else if ((p_node2.indexpolygonlocation == 13))
                {
                    X1 = polygons[p_node2.indexpolygon].vectors[0].X + ff * (polygons[p_node2.indexpolygon].vectors[0].X - polygons[p_node2.indexpolygon].vectors[3].X);//Extend l1 X
                    Y1 = polygons[p_node2.indexpolygon].vectors[0].Y + ff * (polygons[p_node2.indexpolygon].vectors[0].Y - polygons[p_node2.indexpolygon].vectors[3].Y);//Extend l1 Y
                    X2 = polygons[p_node2.indexpolygon].vectors[3].X; //l2 x
                    Y2 = polygons[p_node2.indexpolygon].vectors[3].Y; //l2 Y
                    if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), X1, Y1, X2, Y2))//If the line intersects with extended l1-l2 then it should not connect
                        return false;
                }
                else if ((p_node2.indexpolygonlocation == 22))
                {
                    X1 = polygons[p_node2.indexpolygon].vectors[1].X + ff * (polygons[p_node2.indexpolygon].vectors[1].X - polygons[p_node2.indexpolygon].vectors[2].X);//Extend r4 X
                    Y1 = polygons[p_node2.indexpolygon].vectors[1].Y + ff * (polygons[p_node2.indexpolygon].vectors[1].Y - polygons[p_node2.indexpolygon].vectors[2].Y);//Extend r4 Y
                    X2 = polygons[p_node2.indexpolygon].vectors[2].X; //r3 x
                    Y2 = polygons[p_node2.indexpolygon].vectors[2].Y; //r3 Y
                    if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), X1, Y1, X2, Y2))//If the line intersects with extended r3-r4 then it should not connect
                        return false;
                }
                else if ((p_node2.indexpolygonlocation == 24))
                {
                    X1 = polygons[p_node2.indexpolygon].vectors[0].X + ff * (polygons[p_node2.indexpolygon].vectors[0].X - polygons[p_node2.indexpolygon].vectors[3].X);//Extend r2 X
                    Y1 = polygons[p_node2.indexpolygon].vectors[0].Y + ff * (polygons[p_node2.indexpolygon].vectors[0].Y - polygons[p_node2.indexpolygon].vectors[3].Y);//Extend r2 Y
                    X2 = polygons[p_node2.indexpolygon].vectors[3].X; //r1 x
                    Y2 = polygons[p_node2.indexpolygon].vectors[3].Y; //r1 Y
                    if (visualmath.lineSegmentCross(p_node1.getX(), p_node1.getY(), p_node2.getX(), p_node2.getY(), X1, Y1, X2, Y2))//If the line intersects with extended r1-r2 then it should not connect
                        return false;
                }
                return !doesIntersectwithPolygon(p_node1, p_node2);//If it comes to here then p_node1 and p_node2 are one of the l2,l4,r1,r3, then normal doesintersect function will work
            }
            else//This means they are both locationnodes
            {
                return !doesIntersectwithPolygon(p_node1, p_node2);//Return negate of does intersectwithpolygon since if it intersects with polygon we should return false for shouldConnect function
            }
        }

        public void fillGraphNodeDistances()
        {
            bool[,] scanned = new bool[graphnodes.Length, graphnodes.Length];//If item is scanned then this will be true, initial values are all false
            visibilitygraphdistances = new double[graphnodes.Length, graphnodes.Length];//Distance matrix for visibility graph
            Parallel.For(0, graphnodes.Length, i =>
            //for (int i = 0; i < graphnodes.Count; i++)//For all-pair shortest path
            {
                visibilitygraphdistances[i, i] = 0;         //Distance from source to source
                //prev[i, i] = null;      //Previous node in optimal path initialization
                priorityqueue Q = new priorityqueue();
                Q.Enqueue(new distance(graphnodes[i], null, 0));
                scanned[i, i] = true;   //Mark source node as scanned
                for (int j = 0; j < graphnodes.Length; j++)
                {
                    if (j != i)
                    {
                        visibilitygraphdistances[i, j] = double.MaxValue;
                        //prev[i, j] = null;
                    }
                }

                while (Q.Count() > 0)
                {
                    distance ud = Q.Dequeue();//Find smallest distance in queue
                    node currentnode = ud.node1;
                    scanned[i, currentnode.indexgraphnode] = true;
                    double alt = double.MaxValue;
                    for (int l = 0; l < currentnode.graphedges.Count; l++)
                    {
                        if (currentnode.graphedges[l].getStart() == currentnode)
                        {
                            if (scanned[i, currentnode.graphedges[l].getEnd().indexgraphnode] == false)//This means item has not been scanned
                            {
                                alt = visibilitygraphdistances[i, currentnode.indexgraphnode] +
                                    visualmath.distance(currentnode.getX(), currentnode.getY(), currentnode.graphedges[l].getEnd().getX(), currentnode.graphedges[l].getEnd().getY());
                                if (alt < visibilitygraphdistances[i, currentnode.graphedges[l].getEnd().indexgraphnode])
                                {
                                    visibilitygraphdistances[i, currentnode.graphedges[l].getEnd().indexgraphnode] = alt;//Record new distance
                                    //prev[i, currentnode.edges[l].getEnd().indeximportant] = currentnode;//Record predecessor
                                    Q.Enqueue(new distance(currentnode.graphedges[l].getEnd(), null, alt));
                                }
                            }
                        }
                        if (currentnode.graphedges[l].getEnd() == currentnode)
                        {
                            if (scanned[i, currentnode.graphedges[l].getStart().indexgraphnode] == false)//This means item has not been scanned
                            {
                                alt = visibilitygraphdistances[i, currentnode.indexgraphnode] +
                                    visualmath.distance(currentnode.getX(), currentnode.getY(), currentnode.graphedges[l].getStart().getX(), currentnode.graphedges[l].getStart().getY());
                                if (alt < visibilitygraphdistances[i, currentnode.graphedges[l].getStart().indexgraphnode])
                                {
                                    visibilitygraphdistances[i, currentnode.graphedges[l].getStart().indexgraphnode] = alt;//Record new distance
                                    //prev[i, currentnode.edges[l].getStart().indeximportant] = currentnode;//Record predecessor
                                    Q.Enqueue(new distance(currentnode.graphedges[l].getStart(), null, alt));
                                }
                            }
                        }
                    }
                }
            });
            //Fix some resolution issues with symmetricity
            Parallel.For(0, graphnodes.Length, i =>
            {
                for (int j = 0; j < i; j++)
                {
                    visibilitygraphdistances[j, i] = visibilitygraphdistances[i, j];
                }
            });
        }

        public void setSKUs(List<sku> p_skus)
        {
            myskus.Clear();
            for (int i = 0; i < p_skus.Count; i++)
            {
                sku tmpsku = new sku(p_skus[i].ID);
                tmpsku.pickprobability = p_skus[i].pickprobability;
                myskus.Add(tmpsku);
            }
        }

        public void setOrders(List<order> p_orders)
        {
            myorders.Clear();
            for (int i = 0; i < p_orders.Count; i++)
            {
                order tmporder = new order(p_orders[i].getOrderID());
                for (int j = 0; j < p_orders[i].getOrderSize(); j++)
                {
                    sku tmpsku = myskus.Find(item => item.ID == p_orders[i].getOrderSkus()[j].ID);
                    tmporder.addSKU(tmpsku);
                }
                myorders.Add(tmporder);
            }
        }

        public List<sku> getSKUs()
        {
            return myskus;
        }

        public List<order> getOrders()
        {
            return myorders;
        }
    }
}