//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Collections.Generic;

namespace GABAK
{
    public class edge
    {
        public static int nextID;
        public int id { get; private set; }
        private node start;
        private node end;
        public double width;
        public int connectionk;//Used for matching edge with connection array
        public int type;

        /// <summary>
        /// This variable is used in region finding algorithm for directions of the edge
        /// </summary>
        public bool crossed1;

        /// <summary>
        /// This variable is used in region finding algorithm for directions of the edge
        /// </summary>
        public bool crossed2;

        private List<node> onedgenodes;
        private node[] edgecornernodes;

        private region region;

        public edge()
        {
            id = System.Threading.Interlocked.Increment(ref nextID);
            start = new node(options.tempnode);
            end = new node(options.tempnode);
            type = 0;
            crossed1 = false;
            crossed2 = false;
            onedgenodes = new List<node>();
            region = null;
        }

        public edge(node p_start, node p_end, int p_type)
        {
            id = System.Threading.Interlocked.Increment(ref nextID);
            start = p_start;
            end = p_end;
            type = p_type;
            crossed1 = false;
            crossed2 = false;
            onedgenodes = new List<node>();
            region = null;
        }

        public List<node> getOnEdgeNodes()
        {
            return onedgenodes;
        }

        public void setRegion(region p_region)
        {
            region = p_region;
        }

        public region getRegion()
        {
            return region;
        }

        public void cross(bool crossway)
        {
            if (crossway == true)
            {
                this.crossed1 = true;
            }
            else
            {
                this.crossed2 = true;
            }
        }

        public void uncross(bool crossway)
        {
            if (crossway == true)
            {
                this.crossed1 = false;
            }
            else
            {
                this.crossed2 = false;
            }
        }

        public bool isCrossed(bool crossway)
        {
            if (crossway == true)
            {
                return this.crossed1;
            }
            else
            {
                return this.crossed2;
            }
        }

        public edge rightmost(bool crossway)
        {
            double angle = 0;
            double degangle = options.getBig();
            double tempangle = options.getBig();
            int position = -1;
            if (crossway == true)
            {
                for (int i = 0; i < end.edges.Count; i++)
                {
                    //discard the end node's edge which is current edge
                    if (end.edges[i] != this)
                    {
                        //starting edge and end edge are same
                        if (end.edges[i].getStart() == end && end.edges[i].type == options.regionedge)
                        {
                            angle = Math.Atan2(start.getY() - end.getY(), start.getX() - end.getX()) - Math.Atan2(end.edges[i].getEnd().getY() - end.edges[i].getStart().getY(), end.edges[i].getEnd().getX() - end.edges[i].getStart().getX());
                            tempangle = visualmath.radianToDegree(angle);
                        }

                        if (end.edges[i].getEnd() == end && end.edges[i].type == options.regionedge)
                        {
                            angle = Math.Atan2(start.getY() - end.getY(), start.getX() - end.getX()) - Math.Atan2(end.edges[i].getStart().getY() - end.edges[i].getEnd().getY(), end.edges[i].getStart().getX() - end.edges[i].getEnd().getX());
                            tempangle = visualmath.radianToDegree(angle);
                        }

                        if ((tempangle <= degangle) && (end.edges[i].type == options.regionedge))
                        {
                            degangle = tempangle;
                            position = i;
                        }
                    }
                }
                return end.edges[position];
            }
            else
            {
                for (int i = 0; i < start.edges.Count; i++)
                {
                    if (start.edges[i] != this)
                    {
                        if (start.edges[i].getStart() == start && start.edges[i].type == options.regionedge)
                        {
                            angle = Math.Atan2(end.getY() - start.getY(), end.getX() - start.getX()) - Math.Atan2(start.edges[i].getEnd().getY() - start.edges[i].getStart().getY(), start.edges[i].getEnd().getX() - start.edges[i].getStart().getX());
                            tempangle = visualmath.radianToDegree(angle);
                        }
                        if (start.edges[i].getEnd() == start && start.edges[i].type == options.regionedge)
                        {
                            angle = Math.Atan2(end.getY() - start.getY(), end.getX() - start.getX()) - Math.Atan2(start.edges[i].getStart().getY() - start.edges[i].getEnd().getY(), start.edges[i].getStart().getX() - start.edges[i].getEnd().getX());
                            tempangle = visualmath.radianToDegree(angle);
                        }

                        if ((tempangle <= degangle) && (start.edges[i].type == options.regionedge))
                        {
                            degangle = tempangle;
                            position = i;
                        }
                    }
                }
                return start.edges[position];
            }
        }

        /// <summary>
        /// Used for picking aisle intersection points calculation
        /// </summary>
        /// <param name="p_edge">Intersecting edge</param>
        /// <returns>Returns -1000000 if there is no intersection between the line segments, returns the intersection point if there is intersection</returns>
        public node calculateIntersect(edge p_edge)
        {
            double nointersection = -1000000;//A magic coordinate number used for no intersection
            node tempcoordinate = new node(options.tempnode);
            double ua, ub;

            double x1 = this.getStart().getX();
            double x2 = this.getEnd().getX();
            double x3 = p_edge.getStart().getX();
            double x4 = p_edge.getEnd().getX();

            double y1 = this.getStart().getY();
            double y2 = this.getEnd().getY();
            double y3 = p_edge.getStart().getY();
            double y4 = p_edge.getEnd().getY();

            //Fixing the rounding error problem when picking aisles are 90 degrees to cross aisles (or regionedges) using epsilon
            x3 = x3 + (options.getEpsilon()) * (x3 - x4);
            x4 = x4 - (options.getEpsilon()) * (x3 - x4);
            y3 = y3 + (options.getEpsilon()) * (y3 - y4);
            y4 = y4 - (options.getEpsilon()) * (y3 - y4);

            double denom = ((y4 - y3) * (x2 - x1)) - ((x4 - x3) * (y2 - y1));

            if (denom == 0)//Lines are parallel
            {
                tempcoordinate.setX(nointersection);
                tempcoordinate.setY(nointersection);
                return tempcoordinate;
            }

            double noma = ((x4 - x3) * (y1 - y3)) - ((y4 - y3) * (x1 - x3));

            double nomb = ((x2 - x1) * (y1 - y3)) - ((y2 - y1) * (x1 - x3));

            ua = noma / denom;
            ub = nomb / denom;
            if ((ua <= 1) && (ua >= 0) && (ub <= 1) && (ub >= 0))//Lines intersect within their line segments
            {
                tempcoordinate.setX(x1 + ua * (x2 - x1));
                tempcoordinate.setY(y1 + ua * (y2 - y1));
                return tempcoordinate;
            }
            else//Lines intersect but beyond their line segments
            {
                tempcoordinate.setX(nointersection);
                tempcoordinate.setY(nointersection);
                return tempcoordinate;
            }
        }

        /// <summary>
        /// Used for interior cross aisle intersection point calculation
        /// This is little different than the previous one, we assume the lines are infinitely long lines
        /// We get rid of the ua <= 1 type of checks
        /// </summary>
        /// <param name="p_edge">Intersecting edge</param>
        /// <returns>Returns -1000000 if lines are parallel otherwise returns the intersection point</returns>
        public node calculateIntersectInfinite(edge p_edge)
        {
            int precision = 10;//Used for rounding error problem when intersection of 90 degree pick aisles
            double nointersection = -1000000;//A magic coordinate number used for no intersection
            node tempcoordinate = new node(options.tempnode);
            double ua, ub;

            double x1 = this.getStart().getX();
            double x2 = this.getEnd().getX();
            double x3 = p_edge.getStart().getX();
            double x4 = p_edge.getEnd().getX();

            double y1 = this.getStart().getY();
            double y2 = this.getEnd().getY();
            double y3 = p_edge.getStart().getY();
            double y4 = p_edge.getEnd().getY();

            //Fixing the rounding error problem when picking aisles are 90 degrees
            x1 = Math.Round(x1, precision);
            x2 = Math.Round(x2, precision);
            x3 = Math.Round(x3, precision);
            x4 = Math.Round(x4, precision);
            y1 = Math.Round(y1, precision);
            y2 = Math.Round(y2, precision);
            y3 = Math.Round(y3, precision);
            y4 = Math.Round(y4, precision);

            double denom = ((y4 - y3) * (x2 - x1)) - ((x4 - x3) * (y2 - y1));

            if (denom == 0)
            {
                tempcoordinate.setX(nointersection);
                tempcoordinate.setY(nointersection);
                return tempcoordinate;
            }

            double noma = ((x4 - x3) * (y1 - y3)) - ((y4 - y3) * (x1 - x3));

            double nomb = ((x2 - x1) * (y1 - y3)) - ((y2 - y1) * (x1 - x3));

            ua = noma / denom;
            ub = nomb / denom;
            tempcoordinate.setX(x1 + ua * (x2 - x1));
            tempcoordinate.setY(y1 + ua * (y2 - y1));
            return tempcoordinate;
        }

        /// <summary>
        /// Returns the angle of the edge
        /// </summary>
        /// <returns>Angle of the edge</returns>
        public double calculateAngle()
        {
            double angle;
            angle = Math.Atan2(end.getY() - start.getY(), end.getX() - start.getX());
            return angle;
        }

        /// <summary>
        /// Checks if a node lies on an edge
        /// </summary>
        /// <param name="p_node">Node to be checked</param>
        /// <returns>Returns true if node is on the edge, returns false otherwise</returns>
        public bool isNodeOnEdge(node p_node)
        {
            double x = p_node.getX();
            double y = p_node.getY();
            double x1 = this.end.getX();
            double x2 = this.start.getX();
            double y1 = this.end.getY();
            double y2 = this.start.getY();

            if (((x >= x1 && x <= x2) || (x >= x2 && x <= x1)) && ((y >= y1 && y <= y2) || (y >= y2 && y <= y1)))
            {
                if ((x2 - x1) < 0.00001)
                {
                    return true;
                }
                double slope = (y2 - y1) / (x2 - x1);
                double Yintercept = -(slope * x1) + y1;
                if (y - (slope * x + Yintercept) < 0.00001)
                {
                    return true;
                }
            }
            return false;
        }

        //If the edge is a region edge then you can add picking aisle start and end points to this edge
        //If the edge is a picking aisle edge then you can add pick locations to this edge
        /// <summary>
        /// If the edge is a region edge then you can add picking aisle start and end points to this edge
        /// If the edge is a picking aisle edge then you can add pick locations to this edge
        /// </summary>
        /// <param name="p_node">Node to be added</param>
        public void addOnEdgeNode(node p_node)
        {
            onedgenodes.Add(p_node);
        }

        /// <summary>
        /// Returns the start node of the edge
        /// </summary>
        /// <returns>Start node</returns>
        public node getStart()
        {
            return start;
        }

        /// <summary>
        /// Returns the end node of the edge
        /// </summary>
        /// <returns>End node</returns>
        public node getEnd()
        {
            return end;
        }

        /// <summary>
        /// Sets the start node of the edge
        /// </summary>
        /// <param name="p_node">New start node</param>
        public void setStart(node p_node)
        {
            start = p_node;
        }

        /// <summary>
        /// Sets the end node of the edge
        /// </summary>
        /// <param name="p_node">New end node</param>
        public void setEnd(node p_node)
        {
            end = p_node;
        }

        /// <summary>
        /// Sets the width of the edge
        /// </summary>
        /// <param name="p_width">Edge width</param>
        public void setWidth(double p_width)
        {
            width = p_width;
        }

        /// <summary>
        /// Calculates the four corners of an edge, used in checking if a storage location is inside a cross aisle (region edge)
        /// </summary>
        public void findEdgeRegion()
        {
            node topleft, topright, bottomleft, bottomright;
            double Xtl, Xtr;
            double Xbl, Xbr;
            double Ytl, Ytr;
            double Ybl, Ybr;
            double angle = this.calculateAngle();
            double angle1 = (visualmath.radianToDegree(angle) - 90);
            double halfwidth = this.width / 2;
            angle = visualmath.degreeToRadian(angle1);
            Xtl = this.getStart().getX() - halfwidth * Math.Cos(angle);
            Xtr = this.getStart().getX() + halfwidth * Math.Cos(angle);
            Xbl = this.getEnd().getX() - halfwidth * Math.Cos(angle);
            Xbr = this.getEnd().getX() + halfwidth * Math.Cos(angle);

            Ytl = this.getStart().getY() - halfwidth * Math.Sin(angle);
            Ytr = this.getStart().getY() + halfwidth * Math.Sin(angle);
            Ybl = this.getEnd().getY() - halfwidth * Math.Sin(angle);
            Ybr = this.getEnd().getY() + halfwidth * Math.Sin(angle);

            topleft = new node(Xtl, Ytl, options.tempnode);
            topright = new node(Xtr, Ytr, options.tempnode);
            bottomleft = new node(Xbl, Ybl, options.tempnode);
            bottomright = new node(Xbr, Ybr, options.tempnode);

            edgecornernodes = new node[4];
            edgecornernodes[0] = topleft;
            edgecornernodes[1] = topright;
            edgecornernodes[2] = bottomright;
            edgecornernodes[3] = bottomleft;
        }

        /// <summary>
        /// Checks if a coordinate (one of the corners of a storage location) is inside the edge region (cross aisle region)
        /// Used for checking if a storage location is inside a cross aisle
        /// </summary>
        /// <param name="p_X">X coordinate</param>
        /// <param name="p_Y">Y coordinate</param>
        /// <returns></returns>
        public bool isInsideEdgeRegion(double p_X, double p_Y)
        {
            int n = 4;
            bool isInside = false;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((edgecornernodes[i].getY() > p_Y) != (edgecornernodes[j].getY() > p_Y)) &&
                    (p_X < (edgecornernodes[j].getX() - edgecornernodes[i].getX()) * (p_Y - edgecornernodes[i].getY()) /
                    (edgecornernodes[j].getY() - edgecornernodes[i].getY()) + edgecornernodes[i].getX()))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }
    }
}