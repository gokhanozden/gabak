//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GABAK
{
    /// <summary>
    /// Holds orders as an object
    /// </summary>
    class order
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
    }
}
