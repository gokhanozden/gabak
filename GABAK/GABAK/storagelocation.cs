//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GABAK
{
    public class storagelocation
    {
        private node picklocation;
        public double X1;//Top left
        public double X2;//Top right
        public double X3;//Bottom left
        public double X4;//Bottom right
        public double Y1;//Top left
        public double Y2;//Top right
        public double Y3;//Bottom left
        public double Y4;//Bottom right
        private List<sku> skus;//List of skus stored in this storage location
        private int capacity;
        public storagelocation(node p_picklocation, double p_X1, double p_Y1, double p_X2, double p_Y2, double p_X3, double p_Y3, double p_X4, double p_Y4, int p_capacity)
        {
            picklocation = p_picklocation;
            X1 = p_X1;
            Y1 = p_Y1;
            X2 = p_X2;
            Y2 = p_Y2;
            X3 = p_X3;
            Y3 = p_Y3;
            X4 = p_X4;
            Y4 = p_Y4;
            capacity = p_capacity;
            skus = new List<sku>();
        }

        public bool addSKU(sku p_sku)
        {
            if (skus.Count < capacity)
            {
                skus.Add(p_sku);
                return true;
            }
            return false;
        }

        public int getCapacity()
        {
            return capacity;
        }
    }
}
