//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden

namespace GABAK
{
    /// <summary>
    /// SKU class is used to store SKUs in a warehouse
    /// </summary>
    public class sku
    {
        public node location;//pick location of the SKU
        public int frequency;//frequency of SKU in orders
        public double pickprobability;//pick probability of SKU
        public int ID;//SKU ID

        public sku(int p_ID)
        {
            ID = p_ID;
            frequency = 0;
            pickprobability = 0;
        }

        public void addFrequency()
        {
            frequency++;
        }

        public void setLocation(node p_location)
        {
            location = p_location;
        }
    }
}