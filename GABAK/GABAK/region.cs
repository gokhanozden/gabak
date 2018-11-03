//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GABAK
{
    /// <summary>
    /// Region class holds a region information of a warehouse object, a warehouse can have multiple regions so we may have multiple of region objects per warehouse object
    /// </summary>
    public class region
    {
        public static int nextID;
        /// <summary>
        /// ID of the region
        /// </summary>
        public int regionID { get; private set; }

        /// <summary>
        /// Angle of the picking aisles in the region
        /// </summary>
        private double angle;

        private double crossaislewidth;
        private double pickingaislewidth;
        private double locationwidth;
        private double locationdepth;
        //Horizontal adjuster is used for shifting the picking aisles inside a region to left or right
        private double horizontaladjuster;
        //Vertical adjuster is used for shifting the pick locations up and down in a picking aisle, that's why we call it vertical
        private double verticaladjuster;
        
        public List<edge> regionedges;
        public List<edge> pickingaisleedges;
        public List<edge> pickingaisleregionedges;

        private List<node> interiorintersectnodes;
        private List<node> regionedgeintersectnodes;
        private List<edge> regioninterioredges;
        private List<node> regionnodes;

        /// <summary>
        /// Constructor for a region object
        /// </summary>
        /// <param name="p_angle">Picking aisle angle</param>
        /// <param name="p_crossaislewidth">Cross aisle width</param>
        /// <param name="p_pickingaislewidth">Picking aisle width</param>
        /// <param name="p_locationwidth">Storage location opening width</param>
        /// <param name="p_locationdepth">Storage location opening depth</param>
        /// <param name="p_horizontaladjuster">Horizontal adjuster to shift picking aisles</param>
        /// <param name="p_verticaladjuster">Vertical adjuster to shift pick locations up and down in a picking aisle</param>
        public region(double p_angle, double p_crossaislewidth, double p_pickingaislewidth, double p_locationwidth, double p_locationdepth, double p_horizontaladjuster, double p_verticaladjuster)
        {
            regionID = System.Threading.Interlocked.Increment(ref nextID);
            angle = p_angle % 180;
            horizontaladjuster = p_horizontaladjuster;
            verticaladjuster = p_verticaladjuster;
            crossaislewidth = p_crossaislewidth;
            pickingaislewidth = p_pickingaislewidth;
            locationwidth = p_locationwidth;
            locationdepth = p_locationdepth;
            regionedges = new List<edge>();
            pickingaisleedges = new List<edge>();
            pickingaisleregionedges = new List<edge>();
            interiorintersectnodes = new List<node>();
            regionedgeintersectnodes = new List<node>();
            regioninterioredges = new List<edge>();
            regionnodes = new List<node>();
        }

        /// <summary>
        /// Set the angle of picking aisles (the range is 180 degrees)
        /// </summary>
        /// <param name="p_angle">Angle of picking aisles</param>
        public void setRegionAngle(double p_angle)
        {
            angle = p_angle % 180;
        }

        /// <summary>
        /// Get the angle of picking aisles
        /// </summary>
        /// <returns>Angle of picking aisles</returns>
        public double getRegionAngle()
        {
            return angle;
        }

        /// <summary>
        /// Set horizontal adjuster parameter
        /// </summary>
        /// <param name="p_horizontaladjuster">Horizontal adjuster between 0 and 1</param>
        public void setHorizontalAdjuster(double p_horizontaladjuster)
        {
            horizontaladjuster = p_horizontaladjuster;
        }

        /// <summary>
        /// Set vertical adjuster parameter
        /// </summary>
        /// <param name="p_verticaladjuster">Vertical adjuster between 0 and 1</param>
        public void setVerticalAdjuster(double p_verticaladjuster)
        {
            verticaladjuster = p_verticaladjuster;
        }

        /// <summary>
        /// Get region ID
        /// </summary>
        /// <returns>Region ID</returns>
        public int getRegionID()
        {
            return regionID;
        }

        /// <summary>
        /// Add picking aisle to the region
        /// </summary>
        /// <param name="p_start">Beginning of pick aisle</param>
        /// <param name="p_end">Ending of pick aisle</param>
        /// <param name="p_referencenode1">First reference node</param>
        /// <param name="p_referencenode2">Second reference node</param>
        /// <returns>Returns true if at least one picking aisle is created, otherwise returns false</returns>
        public bool addPickingAisleEdge(node p_start, node p_end, node p_referencenode1, node p_referencenode2)
        {
            //Connect two picking aisle nodes, same picking aisle
            edge tempedge = p_start.connect(p_end, options.pickingaisleedge);
            //Set width of the picking aisle
            tempedge.setWidth(pickingaislewidth);
            //Set region of the picking aisle
            tempedge.setRegion(this);
            //if there is at least one storage location then create a picking aisle
            if (fulfillLocations(tempedge, p_referencenode1, p_referencenode2))
            {
                //Add this edge to region picking aisle edges
                pickingaisleedges.Add(tempedge);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds boundary region edge to the region object
        /// </summary>
        /// <param name="p_edge">Region edge</param>
        public void addRegionEdge(edge p_edge)
        {
            if (p_edge.type == options.regionedge)
            {
                regionedges.Add(p_edge);
            }
        }

        /// <summary>
        /// Copies edge object to another edge object
        /// </summary>
        /// <param name="p_edgeoriginal">Original edge</param>
        /// <param name="p_edgecopy">Copy edge</param>
        private void copyedge(edge p_edgeoriginal, edge p_edgecopy)
        {
            p_edgecopy.getStart().setX(p_edgeoriginal.getStart().getX());
            p_edgecopy.getStart().setY(p_edgeoriginal.getStart().getY());
            p_edgecopy.getEnd().setX(p_edgeoriginal.getEnd().getX());
            p_edgecopy.getEnd().setY(p_edgeoriginal.getEnd().getY());
        }

        /// <summary>
        /// Fill region with picking aisles, this method currently only works with one interior node (because of picking aisles that are split into two by another region)
        /// </summary>
        public void fill()
        {
            findRegionNodes();
            //base coordinate used for rotations in regionfill
            node basecoordinate = new node(0, 0, options.tempnode);
            //temporary region edges
            List<edge> tempregionedges = new List<edge>();
            //rotate all edges of the region by given angle
            for (int i = 0; i < regionedges.Count; i++)
            {
                edge tempedge = new edge();
                copyedge(regionedges[i], tempedge);
                tempregionedges.Add(tempedge);
                tempregionedges[i].setStart(visualmath.rotate(basecoordinate, tempregionedges[i].getStart(), angle));
                tempregionedges[i].setEnd(visualmath.rotate(basecoordinate, tempregionedges[i].getEnd(), angle));
            }

            //Used for rectangular creation
            double minx = options.getBig();
            double miny = options.getBig();
            double maxx = options.getSmall();
            double maxy = options.getSmall();

            //Find the minx, miny, maxx and maxy for a box that is extreme boundaries of the region
            for (int i = 0; i < regionedges.Count; i++)
            {
                if (tempregionedges[i].getStart().getX() > maxx)
                {
                    maxx = tempregionedges[i].getStart().getX();
                }
                if (tempregionedges[i].getStart().getY() > maxy)
                {
                    maxy = tempregionedges[i].getStart().getY();
                }
                if (tempregionedges[i].getEnd().getX() > maxx)
                {
                    maxx = tempregionedges[i].getEnd().getX();
                }
                if (tempregionedges[i].getEnd().getY() > maxy)
                {
                    maxy = tempregionedges[i].getEnd().getY();
                }

                if (tempregionedges[i].getStart().getX() < minx)
                {
                    minx = tempregionedges[i].getStart().getX();
                }
                if (tempregionedges[i].getStart().getY() < miny)
                {
                    miny = tempregionedges[i].getStart().getY();
                }
                if (tempregionedges[i].getEnd().getX() < minx)
                {
                    minx = tempregionedges[i].getEnd().getX();
                }
                if (tempregionedges[i].getEnd().getY() < miny)
                {
                    miny = tempregionedges[i].getEnd().getY();
                }
            }

            //Temporary edges used to add picking aisles to the region
            //infeasible ones are eliminated and not added as picking aisles
            List<edge> tempedges = new List<edge>();
            
            //calculate the gap between picking aisles
            double gap = (pickingaislewidth + 2 * locationdepth);
            int k = 0;
            //Add edges starting from minimum y point and increasing by gap up to maximum y point of the rotated region
            while (miny + k * gap + horizontaladjuster * gap <= maxy)
            {
                edge e1 = new edge(new node(minx, miny + k * gap + horizontaladjuster * gap, options.tempnode), new node(maxx, miny + k * gap + horizontaladjuster * gap, options.tempnode), options.tempedge);
                tempedges.Add(e1);
                k++;
            }

            //Rotate temp edges back
            for (int i = 0; i < tempedges.Count; i++)
            {
                tempedges[i].setStart(visualmath.rotate(basecoordinate, tempedges[i].getStart(), -angle));
                tempedges[i].setEnd(visualmath.rotate(basecoordinate, tempedges[i].getEnd(), -angle));
            }

            //Find edges that are intersecting two of the edges and add them as picking aisles of that region
            for (int i = 0; i < tempedges.Count; i++)
            {
                List<node> tmppoints = new List<node>();
                List<int> intersectedges = new List<int>();
                for (int j = 0; j < regionedges.Count; j++)
                {
                    node p = new node(0, 0, options.pickingaislenode);
                    p = regionedges[j].calculateIntersect(tempedges[i]);
                    //Check if that edge intersect with tempedge
                    if (p.getX() != -1000000)//-1000000 is a magic number for no intersection
                    {
                        tmppoints.Add(p);
                        tmppoints[tmppoints.Count - 1].setCrossAisleEdge(regionedges[j]);
                    }
                }
                //if there are exactly two intersections then add these two points as picking aisles to that region
                if (tmppoints.Count == 2)
                {
                    if (angle == 90)
                    {
                        tmppoints.Sort((x, y) => y.getY().CompareTo(x.getY()));
                    }
                    else
                    {
                        tmppoints.Sort((x, y) => y.getX().CompareTo(x.getX()));
                    }
                    tmppoints[0].type = options.pickingaislenode;
                    tmppoints[1].type = options.pickingaislenode;
                    //If there are storage locations on this picking aisle then add this picking aisle to the region
                    if (addPickingAisleEdge(tmppoints[0], tmppoints[1], tempedges[i].getStart(), tempedges[i].getEnd()))
                    {
                        tmppoints[0].getCrossAisleEdge().addOnEdgeNode(tmppoints[0]);
                        tmppoints[1].getCrossAisleEdge().addOnEdgeNode(tmppoints[1]);
                    }
                }
                //if there are exactly four intersections then add first two points as first picking aisle and second two points as second picking aisle
                if (tmppoints.Count == 4)
                {
                    if (angle == 90)
                    {
                        tmppoints.Sort((x, y) => y.getY().CompareTo(x.getY()));
                    }
                    else
                    {
                        tmppoints.Sort((x, y) => y.getX().CompareTo(x.getX()));
                    }
                    //First picking aisle
                    tmppoints[0].type = options.pickingaislenode;
                    tmppoints[1].type = options.pickingaislenode;
                    //If there are storage locations on this picking aisle then add this picking aisle to the region
                    if (addPickingAisleEdge(tmppoints[0], tmppoints[1], tempedges[i].getStart(), tempedges[i].getEnd()))
                    {
                        tmppoints[0].getCrossAisleEdge().addOnEdgeNode(tmppoints[0]);
                        tmppoints[1].getCrossAisleEdge().addOnEdgeNode(tmppoints[1]);
                    }
                    //Second picking aisle
                    tmppoints[2].type = options.pickingaislenode;
                    tmppoints[3].type = options.pickingaislenode;
                    //If there are storage locations on this picking aisle then add this picking aisle to the region
                    if (addPickingAisleEdge(tmppoints[2], tmppoints[3], tempedges[i].getStart(), tempedges[i].getEnd()))
                    {
                        tmppoints[2].getCrossAisleEdge().addOnEdgeNode(tmppoints[2]);
                        tmppoints[3].getCrossAisleEdge().addOnEdgeNode(tmppoints[3]);
                    }
                }
            }
        }

        /// <summary>
        /// Finds all region nodes that are part of region edges and adds them to regionnodes list, this list is used in isInsideRegion method 
        /// </summary>
        private void findRegionNodes()
        {
            bool direction = true;
            bool previousdirection = true;
            if (regionedges[0].getEnd() == regionedges[1].getStart())
            {
                regionnodes.Add(regionedges[0].getStart());
                previousdirection = true;
            }
            else if (regionedges[0].getEnd() == regionedges[1].getEnd())
            {
                regionnodes.Add(regionedges[0].getStart());
                previousdirection = true;
            }
            else if (regionedges[0].getStart() == regionedges[1].getStart())
            {
                regionnodes.Add(regionedges[0].getEnd());
                previousdirection = false;
            }
            else if (regionedges[0].getStart() == regionedges[1].getEnd())
            {
                regionnodes.Add(regionedges[0].getEnd());
                previousdirection = false;
            }
            for(int i = 1; i < regionedges.Count; i++)
            {
                if (previousdirection)
                {
                    if(regionedges[i - 1].getEnd() == regionedges[i].getStart())
                    {
                        regionnodes.Add(regionedges[i].getStart());
                        direction = true;
                    }
                    if (regionedges[i - 1].getEnd() == regionedges[i].getEnd())
                    {
                        regionnodes.Add(regionedges[i].getEnd());
                        direction = false;
                    }
                }
                else
                {
                    if (regionedges[i - 1].getStart() == regionedges[i].getStart())
                    {
                        regionnodes.Add(regionedges[i].getStart());
                        direction = true;
                    }
                    if (regionedges[i - 1].getStart() == regionedges[i].getEnd())
                    {
                        regionnodes.Add(regionedges[i].getEnd());
                        direction = false;
                    }
                }
                if (direction) previousdirection = true;
                else previousdirection = false;
            }
        }

        /// <summary>
        /// Checks if a coordinate is inside the region
        /// </summary>
        /// <param name="p_X">X coordinate</param>
        /// <param name="p_Y">Y coordinate</param>
        /// <returns>Returns true if it is inside, otherwise returns false</returns>
        private bool isInsideRegion(double p_X, double p_Y)
        {
            bool isInside = false;
            for (int i = 0, j = regionnodes.Count - 1; i < regionnodes.Count; j = i++)
            {
                if (((regionnodes[i].getY() > p_Y) != (regionnodes[j].getY() > p_Y)) &&
                    (p_X < (regionnodes[j].getX() - regionnodes[i].getX()) * (p_Y - regionnodes[i].getY()) /
                    (regionnodes[j].getY() - regionnodes[i].getY()) + regionnodes[i].getX()))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }

        /// <summary>
        /// Fulfills pick aisles with pick locations and storage locations, returns false if no storage location can be created (because aisle is too small)
        /// </summary>
        /// <param name="p_edge">Picking aisle</param>
        /// <param name="p_referencenode">Reference node 1 is used for aligning storage locations correctly between picking aisles</param>
        /// <param name="p_referencenode2">Reference node 2 is used for aligning storage locations correctly between picking aisles</param>
        /// <returns>Returns false if no storage locations are created in a pick aisle, otherwise returns true</returns>
        public bool fulfillLocations(edge p_edge, node p_referencenode1, node p_referencenode2)
        {
            double X;
            double Y;
            double Xl1, Xr1;
            double Xl2, Xr2;
            double Xl3, Xr3;
            double Xl4, Xr4;
            double Yl1, Yr1;
            double Yl2, Yr2;
            double Yl3, Yr3;
            double Yl4, Yr4;

            double Xs, Xe, Xr, Ys, Ye, Yr;
            
            if (p_edge.getStart().getY() > p_edge.getEnd().getY())
            {
                Xs = p_edge.getEnd().getX();
                Xe = p_edge.getStart().getX();
                Ys = p_edge.getEnd().getY();
                Ye = p_edge.getStart().getY();
            }
            else
            {
                Xs = p_edge.getStart().getX();
                Xe = p_edge.getEnd().getX();
                Ys = p_edge.getStart().getY();
                Ye = p_edge.getEnd().getY();
            }

            //Check which reference point is close to starting coordinate (Xs, Ys)
            if(p_referencenode1.getY() < p_referencenode2.getY())
            {
                Xr = p_referencenode1.getX();
                Yr = p_referencenode1.getY();
            }
            else
            {
                Xr = p_referencenode2.getX();
                Yr = p_referencenode2.getY();
            }

            double angle = p_edge.calculateAngle();
            double angle1 = (visualmath.radianToDegree(angle) - 90);
            angle = visualmath.degreeToRadian(angle1);
            double halfpickingaislewidth = pickingaislewidth / 2;

            double lengthofaisle = Math.Sqrt(Math.Pow(Xs - Xe, 2) + Math.Pow(Ys - Ye, 2));

            int numberoflocations = Convert.ToInt32(Math.Ceiling(lengthofaisle / locationwidth));
            double incx = (Xe - Xs) / (lengthofaisle / locationwidth);
            double incy = (Ye - Ys) / (lengthofaisle / locationwidth);

            //Find the starting point that is inside the picking aisle
            X = Xr;
            Y = Yr;
            if (Yr < Ys)
            {
                while (Y < Ys)
                {
                    X = X + incx;
                    Y = Y + incy;
                }
            }
            else
            {
                while (Y > Ys)
                {
                    X = X + incx;
                    Y = Y + incy;
                }
            }

            X = X + incx * verticaladjuster;
            Y = Y + incy * verticaladjuster;

            for (int i = 0; i < numberoflocations; i++)
            {
                storagelocation s1 = null;
                storagelocation s2 = null;
                node c = new node(X, Y, options.locationnode);
                //Check each corner of the storage location is inside the region in a smart way
                //If all the corners are inside then create a storage location for left face
                Xl1 = X - incx / 2 - (halfpickingaislewidth + locationdepth) * Math.Cos(angle);
                Yl1 = Y - incy / 2 - (halfpickingaislewidth + locationdepth) * Math.Sin(angle);

                if (!isOnCrossAisle(Xl1, Yl1) && isInsideRegion(Xl1, Yl1))
                {
                    Xl3 = X + incx / 2 - (halfpickingaislewidth + locationdepth) * Math.Cos(angle);
                    Yl3 = Y + incy / 2 - (halfpickingaislewidth + locationdepth) * Math.Sin(angle);
                    if (!isOnCrossAisle(Xl3, Yl3) && isInsideRegion(Xl3, Yl3))
                    {
                        Xl2 = X - incx / 2 - (halfpickingaislewidth) * Math.Cos(angle);
                        Yl2 = Y - incy / 2 - (halfpickingaislewidth) * Math.Sin(angle);
                        if (!isOnCrossAisle(Xl2, Yl2) && isInsideRegion(Xl2, Yl2))
                        {
                            Xl4 = X + incx / 2 - (halfpickingaislewidth) * Math.Cos(angle);
                            Yl4 = Y + incy / 2 - (halfpickingaislewidth) * Math.Sin(angle);
                            if (!isOnCrossAisle(Xl4, Yl4) && isInsideRegion(Xl4, Yl4))
                            {
                                s1 = new storagelocation(c, Xl1, Yl1, Xl2, Yl2, Xl3, Yl3, Xl4, Yl4, 1);
                            }
                        }
                    }
                }
                    
                Xr2 = X - incx / 2 + (halfpickingaislewidth + locationdepth) * Math.Cos(angle);
                Yr2 = Y - incy / 2 + (halfpickingaislewidth + locationdepth) * Math.Sin(angle);

                if (!isOnCrossAisle(Xr2, Yr2) && isInsideRegion(Xr2, Yr2))
                {
                    Xr4 = X + incx / 2 + (halfpickingaislewidth + locationdepth) * Math.Cos(angle);
                    Yr4 = Y + incy / 2 + (halfpickingaislewidth + locationdepth) * Math.Sin(angle);
                    if (!isOnCrossAisle(Xr4, Yr4) && isInsideRegion(Xr4, Yr4))
                    {
                        Xr1 = X - incx / 2 + (halfpickingaislewidth) * Math.Cos(angle);
                        Yr1 = Y - incy / 2 + (halfpickingaislewidth) * Math.Sin(angle);
                        if (!isOnCrossAisle(Xr1, Yr1) && isInsideRegion(Xr1, Yr1))
                        {
                            Xr3 = X + incx / 2 + (halfpickingaislewidth) * Math.Cos(angle);
                            Yr3 = Y + incy / 2 + (halfpickingaislewidth) * Math.Sin(angle);
                            if (!isOnCrossAisle(Xr3, Yr3) && isInsideRegion(Xr3, Yr3))
                            {
                                s2 = new storagelocation(c, Xr1, Yr1, Xr2, Yr2, Xr3, Yr3, Xr4, Yr4, 1);
                            }
                        }
                    }
                }
                    
                if(s1 != null)
                {
                    c.s1 = s1;
                }
                if (s2 != null)
                {
                    c.s2 = s2;
                }
                //Do not create a pick location if there is no storage location
                if (s1 != null || s2 != null)
                {
                    c.setPickingAisleEdge(p_edge);
                    p_edge.addOnEdgeNode(c);
                }
                X = X + incx;
                Y = Y + incy;
            }
            //Return false if there are no pick locations created (because aisle is too short), else return true
            if (p_edge.getOnEdgeNodes().Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Calculates the region area, this is used in region finding algorithm to remove the largest region (i.e., the whole warehouse) from the list of regions
        /// </summary>
        /// <returns>Area</returns>
        public double getArea()
        {
            List<node> tmpnodes = new List<node>();
            //Add all the nodes
            for (int i = 0; i < regionedges.Count; i++ )
            {
                if (!tmpnodes.Contains(regionedges[i].getStart()))
                {
                    tmpnodes.Add(regionedges[i].getStart());
                }
                if (!tmpnodes.Contains(regionedges[i].getEnd()))
                {
                    tmpnodes.Add(regionedges[i].getEnd());
                }
            }
            //If there are less than three nodes (highly unlikely) in a region, then the area is set to zero
            if(tmpnodes.Count < 3)
            {
                return 0;
            }
            //The code below calculates the area given its coordinates
            double area = visualmath.getDeterminant(tmpnodes[tmpnodes.Count - 1].getX(), tmpnodes[tmpnodes.Count - 1].getY(), tmpnodes[0].getX(), tmpnodes[0].getY());
            for (int i = 1; i < tmpnodes.Count; i++)
            {
                area += visualmath.getDeterminant(tmpnodes[i - 1].getX(), tmpnodes[i - 1].getY(), tmpnodes[i].getX(), tmpnodes[i].getY());
            }
            return Math.Abs(area / 2);
        }

        /// <summary>
        /// Checks if a point is on any cross aisles in region
        /// </summary>
        /// <param name="p_X">X axis</param>
        /// <param name="p_Y">Y axis</param>
        /// <returns>True if it is on cross aisle</returns>
        private bool isOnCrossAisle(double p_X, double p_Y)
        {
            for(int i = 0; i < regionedges.Count; i++)
            {
                if (regionedges[i].isInsideEdgeRegion(p_X, p_Y)) return true;
            }
            return false;
        }
    }
}
