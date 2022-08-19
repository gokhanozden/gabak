//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System.Collections.Generic;
using System.Linq;

namespace GABAK
{
    /// <summary>
    /// Holds orders as an object
    /// </summary>
    internal class order
    {
        private int orderID;
        private List<sku> skus;

        /// <summary>
        /// Constructor for order class
        /// </summary>
        /// <param name="p_orderid">Order ID</param>
        public order(int p_orderid)
        {
            orderID = p_orderid;
            skus = new List<sku>();
        }

        /// <summary>
        /// Returns order ID
        /// </summary>
        /// <returns>Order ID</returns>
        public int getOrderID()
        {
            return orderID;
        }

        /// <summary>
        /// Returns number of items in an order
        /// </summary>
        /// <returns>Number of items in an order</returns>
        public int getOrderSize()
        {
            return skus.Count();
        }

        /// <summary>
        /// Returns the list of items in an order
        /// </summary>
        /// <returns>List of items in an order</returns>
        public List<sku> getOrderSkus()
        {
            return skus;
        }

        /// <summary>
        /// Adds items to an order
        /// </summary>
        /// <param name="p_sku">SKU</param>
        public void addSKU(sku p_sku)
        {
            skus.Add(p_sku);
        }
        /// <summary>
        /// Returns the minimum x coordinate among the coordinates of SKU locations. Used for calculating the area of smallest rectangle that covers only the pick locations.
        /// </summary>
        /// <returns></returns>
        public double returnMinX()
        {
            double minX = double.MaxValue;
            for(int i = 0; i < getOrderSize(); i++)
            {
                if(minX > skus[i].location.getX())
                {
                    minX = skus[i].location.getX();
                }
            }
            return minX;
        }
        /// <summary>
        /// Returns the minimum y coordinate among the coordinates of SKU locations. Used for calculating the area of smallest rectangle that covers only the pick locations.
        /// </summary>
        /// <returns></returns>
        public double returnMinY()
        {
            double minY = double.MaxValue;
            for (int i = 0; i < getOrderSize(); i++)
            {
                if (minY > skus[i].location.getY())
                {
                    minY = skus[i].location.getX();
                }
            }
            return minY;
        }
        /// <summary>
        /// Returns the maximum x coordinate among the coordinates of SKU locations. Used for calculating the area of smallest rectangle that covers only the pick locations.
        /// </summary>
        /// <returns></returns>
        public double returnMaxX()
        {
            double maxX = double.MinValue;
            for (int i = 0; i < getOrderSize(); i++)
            {
                if (maxX < skus[i].location.getX())
                {
                    maxX = skus[i].location.getX();
                }
            }
            return maxX;
        }
        /// <summary>
        /// Returns the minimum y coordinate among the coordinates of SKU locations. Used for calculating the area of smallest rectangle that covers only the pick locations.
        /// </summary>
        /// <returns></returns>
        public double returnMaxY()
        {
            double maxY = double.MinValue;
            for (int i = 0; i < getOrderSize(); i++)
            {
                if (maxY < skus[i].location.getY())
                {
                    maxY = skus[i].location.getX();
                }
            }
            return maxY;
        }
    }
}