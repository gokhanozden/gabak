//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GABAK
{
    /// <summary>
    /// Distance class is used in Djikstra's Shortest Path Algorithm calculations
    /// </summary>
    public class distance
    {
        public node node1;
        public node node2;
        public double dist = 0;
        public distance(node p_node1, node p_node2, double p_dist)
        {
            node1 = p_node1;
            node2 = p_node2;
            dist = p_dist;
        }
    }
}
