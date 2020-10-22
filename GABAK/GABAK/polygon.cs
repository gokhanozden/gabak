//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System.Collections.Generic;

namespace GABAK
{
    /// <summary>
    /// Polygon class is used to create polygons that are used in visibility graph
    /// </summary>
    internal class polygon
    {
        /// <summary>
        /// Each polygon has vectors
        /// </summary>
        public List<vector> vectors;

        public polygon()
        {
            vectors = new List<vector>();
        }

        /// <summary>
        /// Method to add a vector to a polygon object
        /// </summary>
        /// <param name="p_X">X coordinate</param>
        /// <param name="p_Y">Y coordinate</param>
        public void addVector(double p_X, double p_Y)
        {
            vector tempvector = new vector();
            tempvector.X = p_X;
            tempvector.Y = p_Y;
            vectors.Add(tempvector);
        }

        /// <summary>
        /// Method to check if a coordinate is inside or outside of a polygon
        /// </summary>
        /// <param name="p_X">X coordinate</param>
        /// <param name="p_Y">Y coordinate</param>
        /// <returns>Returns true if it is inside, false if it is outside</returns>
        private bool isInsidePolygon(double p_X, double p_Y)
        {
            bool isInside = false;
            for (int i = 0, j = vectors.Count - 1; i < vectors.Count; j = i++)
            {
                if (((vectors[i].Y > p_Y) != (vectors[j].Y > p_Y)) &&
                    (p_X < (vectors[j].X - vectors[i].X) * (p_Y - vectors[i].Y) /
                    (vectors[j].Y - vectors[i].Y) + vectors[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }
    }
}