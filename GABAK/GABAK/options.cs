//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GABAK
{
    static class options
    {
        //Big and small number needed for some calculations
        private static double big = Double.MaxValue;
        private static double small = Double.MinValue;
        //Smallest double number closest to zero
        private static double epsilon = 0.000001;
        //Edge types
        public static int tempedge = 0;
        public static int corneredge = 1;
        public static int regionedge = 2;
        public static int pickingaisleedge = 3;
        public static int pickingaisleregionedge = 4;
        public static int locationedge = 5;
        public static int pdedge = 6;
        public static int interiorregionedge = 7;
        public static int visibilitygraphedge = 8;

        //Node types
        public static int tempnode = 0;
        public static int cornernode = 1;
        public static int exteriornode = 2;
        public static int interiornode = 3;
        public static int pickingaislenode = 4;
        public static int locationnode = 5;
        public static int pdnode = 6;
        public static int intersectionnode = 7;
        public static int polygonnode = 8;

        //Corner node locations
        public static int topleft = 0;
        public static int topright = 1;
        public static int bottomright = 2;
        public static int bottomleft = 3;

        //Corner edges
        public static int top = 0;
        public static int right = 1;
        public static int bottom = 2;
        public static int left = 3;

        //Number of coloring
        public static int numbercolors = 100;

        public static int lowerattempts = 0;
        public static int upperattempts = 0;
        public static int totalattempts = 0;

        /// <summary>
        /// Returns largest negative double number
        /// </summary>
        /// /// <returns>Largest negative double number</returns>
        public static double getSmall()
        {
            return small;
        }
        /// <summary>
        /// Returns largest positive double number
        /// </summary>
        /// <returns>Largest positive double number</returns>
        public static double getBig()
        {
            return big;
        }
        /// <summary>
        /// Returns smallest positive double number
        /// </summary>
        /// <returns>Smallest positive double number</returns>
        public static double getEpsilon()
        {
            return epsilon;
        }
        //Network
        //Buffer Size
        public const int bufferSize = 1024;//1024 byte size of receive buffer size
        //Allocation
        public static double allowableemptyspacepercentage = 0.1;//This number should be between 0 and 1, percentage of empty locations compared to total SKUs, based on Celik's paper, maximum absolute difference 7.2%
        public static double warehouseadjustmentfactor = 0.999;//This number should be between 0 and 1, close to 1 means minor adjustments
        //Process ID
        public static int processid = -1;
    }
}
