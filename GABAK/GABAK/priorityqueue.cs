//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System.Collections.Generic;
using System.Linq;

namespace GABAK
{
    /// <summary>
    /// Priority queue used for Djikstra's shortest path algorithm
    /// </summary>
    public class priorityqueue
    {
        private List<distance> data;//A list that keeps the distances

        /// <summary>
        /// Default constructor for priority queue
        /// </summary>
        public priorityqueue()
        {
            data = new List<distance>();
        }

        /// <summary>
        /// Add a distance object to priority queue
        /// </summary>
        /// <param name="item">Distance object</param>
        public void Enqueue(distance item)
        {
            data.Add(item);
            int ci = data.Count - 1;
            while (ci > 0)
            {
                int pi = (ci - 1) / 2;
                if (data[ci].dist.CompareTo(data[pi].dist) >= 0)
                    break;
                distance tmp = data[ci];
                data[ci] = data[pi];
                data[pi] = tmp;
                ci = pi;
            }
        }

        /// <summary>
        /// Remove a distance object from priority queue
        /// </summary>
        /// <returns>Distance object that is removed</returns>
        public distance Dequeue()
        {
            //Assumes pq isn't empty
            int li = data.Count - 1;
            distance frontItem = data[0];
            data[0] = data[li];
            data.RemoveAt(li);

            --li;
            int pi = 0;
            while (true)
            {
                int ci = pi * 2 + 1;
                if (ci > li) break;
                int rc = ci + 1;
                if (rc <= li && data[rc].dist.CompareTo(data[ci].dist) < 0)
                    ci = rc;
                if (data[pi].dist.CompareTo(data[ci].dist) <= 0) break;
                distance tmp = data[pi];
                data[pi] = data[ci];
                data[ci] = tmp;
                pi = ci;
            }
            return frontItem;
        }

        /// <summary>
        /// Returns the number of items in the queue
        /// </summary>
        /// <returns>Number of items in the queue</returns>
        public int Count()
        {
            return data.Count;
        }

        /// <summary>
        /// Returns true if a node exists in the queue
        /// </summary>
        /// <param name="item">Node</param>
        /// <returns>Returns true if a node exists in the queue, false otherwise</returns>
        public bool Contains(node item)
        {
            for (int i = 0; i < data.Count(); i++)
            {
                if (data[i].node1 == item) return true;
            }
            return false;
        }
    }
}