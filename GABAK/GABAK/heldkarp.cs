using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GABAK
{
    public class heldkarp
    {
        // Number of cities
        private int N;

        // A large number to represent infinity
        private int INF = int.MaxValue / 2;

        // Distance matrix (distance between cities)
        private int[,] dist;

        // DP table to store the minimum cost for each subset of cities
        private int[,] dp;

        // Constructor to initialize the distance matrix and number of cities
        public heldkarp(int[,] distanceMatrix)
        {
            this.dist = distanceMatrix;
            this.N = dist.GetLength(0);  // Number of cities is the size of the distance matrix
        }

        // Function to solve TSP using the Held-Karp dynamic programming algorithm
        public int SolveTSP()
        {
            // Number of subsets of cities is 2^N (using bitmask to represent subsets)
            dp = new int[1 << N, N];

            // Fill DP table with a large value (indicating unvisited states)
            for (int i = 0; i < (1 << N); i++)
                for (int j = 0; j < N; j++)
                    dp[i, j] = INF;

            // Base case: starting at city 0, cost is 0
            dp[1, 0] = 0;

            // Iterate over all subsets of cities represented by bitmasks
            for (int mask = 1; mask < (1 << N); mask++)
            {
                for (int u = 0; u < N; u++)
                {
                    // If city u is not part of the current subset, skip it
                    if ((mask & (1 << u)) == 0) continue;

                    // Iterate over all possible cities v to transition to
                    for (int v = 0; v < N; v++)
                    {
                        // Skip if city v is already in the subset (already visited)
                        if ((mask & (1 << v)) != 0) continue;

                        // Transition from city u to city v
                        int newMask = mask | (1 << v);
                        dp[newMask, v] = Math.Min(dp[newMask, v], dp[mask, u] + dist[u, v]);
                    }
                }
            }

            // Find the minimum cost to return to the starting city (city 0) after visiting all cities
            int result = INF;
            for (int i = 1; i < N; i++)
            {
                result = Math.Min(result, dp[(1 << N) - 1, i] + dist[i, 0]);
            }

            return result;
        }
    }
}
