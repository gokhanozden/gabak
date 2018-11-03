//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GABAK
{
    public class node
    {
        /// <summary>
        /// Id used for unique identification of nodes
        /// </summary>
        public static int nextID;
        public int id { get; private set; }

        /// <summary>
        /// X coordinate
        /// </summary>
        private double x;
        
        /// <summary>
        /// Y coordinate
        /// </summary>
        private double y;

        private double location;//this is for exterior node

        private double locationX;//these are for interior nodes
        private double locationY;//these are for interior nodes
        
        /// <summary>
        ///Type of the node corner node, exterior node, etc. 
        /// </summary>
        public int type;

        public bool scanned = false;//Used for important nodes shortest path
        public bool scannedlocation = false;//Used for location nodes shortest path (might use)

        public int indeximportant;//Used for important nodes shortest path
        public int indexlocation;//Used for location nodes shortest path
        public int indexgraphnode;//Used for visibility graph
        public int indexpolygon;//Used for checking whether two points are in the same polygon (for visibility graph)
        public int indexregion;//Used for checking whether two points are in the same region (for visibility graph)
        public int indexpickingaisle;//Used for checking whether two points are in the same picking aisle (for visibility graph)
        public int indexpolygonlocation;//Used for checking which polygon nodes need to be connected (for visibility graph)
        public int indexpolygonleft;//Used for smart polygon checking for pick locations
        public int indexpolygonright;//Used for smart polygon checking for pick locations

        /// <summary>
        /// This variable is used in region finding algorithm
        /// </summary>
        public node previousnode;

        public double totaldistance;//Used for storing sum of travel distances to each pick location

        public double pddistance;//Used for storing travel distance to P&D point

        public int color;//Used for coloring storage and pick locations
        //One to Many Total Distance Normalized
        public double onetomanynormalizeddist;
        //Many to Many Total Distance Normalized
        public double manytomanynormalizeddist;
        public double overallranking;//Used for new turnover based storage

        public storagelocation s1;//Used for first storage location for pick location nodes
        public storagelocation s2;//Used for second storage location for pick location nodes

        private edge pickingaisleedge;//Used for nodes that lies on picking aisles (so we can get this node's picking aisle)
        private edge crossaisleedge;//Used for nodes that lies on cross aisles (so we can get this node's cross aisle)

        public List<edge> edges { get; private set; }//Each node might be connected to multiple edges, this will help us to build graph network for aisle centers method
        public List<edge> graphedges { get; private set; }//Each node might be connected to multipe edges, this will help us to build graph network for visibility graph method

        /// <summary>
        /// Default constructor of node object
        /// </summary>
        public node(int p_type)
        {
            id = System.Threading.Interlocked.Increment(ref nextID);
            edges = new List<edge>();
            graphedges = new List<edge>();
            x = 0;
            y = 0;
            type = p_type;
            totaldistance = Double.MaxValue;
            pddistance = Double.MaxValue;
            color = 0;
            indeximportant = -1;
            indexlocation = -1;
        }

        /// <summary>
        /// Constructor of node object
        /// </summary>
        /// <param name="p_x">X coordinate</param>
        /// <param name="p_y">Y coordinate</param>
        /// <param name="p_type">Node type</param>
        public node(double p_x, double p_y, int p_type)
        {
            id = System.Threading.Interlocked.Increment(ref nextID);
            edges = new List<edge>();
            graphedges = new List<edge>();
            x = p_x;
            y = p_y;
            type = p_type;
            totaldistance = Double.MaxValue;
            pddistance = Double.MaxValue;
            color = 0;
        }

        /// <summary>
        /// Set x coordinate of node object
        /// </summary>
        /// <param name="p_x">X coordinate</param>
        public void setX(double p_x)
        {
            x = p_x;
        }

        /// <summary>
        /// Set y coordinate of node object
        /// </summary>
        /// <param name="p_x">Y coordinate</param>
        public void setY(double p_y)
        {
            y = p_y;
        }

        /// <summary>
        /// Set location of exterior node object
        /// </summary>
        /// <param name="p_location">Location</param>
        public void setLocation(double p_location)
        {
            location = p_location;
        }

        /// <summary>
        /// Set X location of interior node object
        /// </summary>
        /// <param name="p_locationX">X location</param>
        public void setLocationX(double p_locationX)
        {
            locationX = p_locationX;
        }

        /// <summary>
        /// Set Y location of interior node object
        /// </summary>
        /// <param name="p_locationX">X location</param>
        public void setLocationY(double p_locationY)
        {
            locationY = p_locationY;
        }

        /// <summary>
        /// Set picking aisle edge for a node
        /// </summary>
        /// <param name="p_edge">Picking aisle</param>
        public void setPickingAisleEdge(edge p_edge)
        {
            pickingaisleedge = p_edge;
        }

        /// <summary>
        /// Set cross aisle edge for a node
        /// </summary>
        /// <param name="p_edge">Cross aisle</param>
        public void setCrossAisleEdge(edge p_edge)
        {
            crossaisleedge = p_edge;
        }

        /// <summary>
        /// Get location of exterior node object
        /// </summary>
        public double getLocation()
        {
            return location;
        }

        /// <summary>
        /// Get x coordinate of node object
        /// </summary>
        /// <param name="p_x">X coordinate</param>
        public double getX()
        {
            return x;
        }

        /// <summary>
        /// Get y coordinate of node object
        /// </summary>
        /// <param name="p_x">Y coordinate</param>
        public double getY()
        {
            return y;
        }

        /// <summary>
        /// Get picking aisle for a node
        /// </summary>
        /// <returns>Picking aisle</returns>
        public edge getPickingAisleEdge()
        {
            return pickingaisleedge;
        }

        /// <summary>
        /// Get cross aisle for a node
        /// </summary>
        /// <returns>Cross aisle</returns>
        public edge getCrossAisleEdge()
        {
            return crossaisleedge;
        }

        /// <summary>
        /// Connects this node to another node and returns the connection edge
        /// </summary>
        /// <param name="p_node">Connecting node</param>
        /// <param name="p_type">Type of connection (edge type)</param>
        /// <returns>Created edge after connection</returns>
        public edge connect(node p_node, int p_type)
        {
            edge temp = isconnected(p_node);

            if (temp == null || temp.type != p_type)
            {
                temp = new edge(this, p_node, p_type);
                this.edges.Add(temp);
                p_node.edges.Add(temp);
            }
            return temp;
        }

        /// <summary>
        /// Disconnects this node from another node
        /// </summary>
        /// <param name="p_node">Disconnecting node</param>
        public void disconnect(node p_node)
        {
            edge temp = isconnected(p_node);
            this.edges.Remove(temp);
            p_node.edges.Remove(temp);
        }

        /// <summary>
        /// Checks if a node is connected to this node
        /// </summary>
        /// <param name="p_node">Possible connecting node</param>
        /// <returns>Returns null if not connected, otherwise returns connection edge</returns>
        public edge isconnected(node p_node)
        {
            foreach (edge e in edges)
            {
                if ((e.getStart() == this && e.getEnd() == p_node) || (e.getEnd() == this && e.getStart() == p_node))
                {
                    return e;
                }
            }
            return null;
        }

        /// <summary>
        /// Connects this node to another node with visibility graph edge connection and returns visibility graph edge
        /// </summary>
        /// <param name="p_node">Connecting node</param>
        /// <returns>Visibility graph edge</returns>
        public edge connectvisibilitygraph(node p_node)
        {
            edge temp = isconnectedVisibilityGraph(p_node);
            if(temp == null)
            {
                temp = new edge(this, p_node, options.visibilitygraphedge);
                this.graphedges.Add(temp);
                p_node.graphedges.Add(temp);
            }
            return temp;
        }

        /// <summary>
        /// Disconnects this node from another node for visibility graph
        /// </summary>
        /// <param name="p_node">Disconnecting node</param>
        public void disconnectvisibilitygraph(node p_node)
        {
            edge temp = isconnectedVisibilityGraph(p_node);
            this.graphedges.Remove(temp);
            p_node.graphedges.Remove(temp);
        }

        /// <summary>
        /// Checks if a node is connected to this node via visibility graph edge
        /// </summary>
        /// <param name="p_node">Possible connecting node via visibility graph edge</param>
        /// <returns>Returns null if not connected, otherwise returns connection edge</returns>
        public edge isconnectedVisibilityGraph(node p_node)
        {
            if (graphedges.Count == 0) return null;
            foreach (edge e in graphedges)
            {
                if ((e.getStart() == this && e.getEnd() == p_node) || (e.getEnd() == this && e.getStart() == p_node))
                {
                    return e;
                }
            }
            return null;
        }
    }
}
