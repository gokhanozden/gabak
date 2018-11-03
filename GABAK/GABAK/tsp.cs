//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GABAK
{
    class tsp
    {
        private int N;
        private double[,] A;
        private string filename;

        public tsp(int p_N, string p_filename)
        {
            N = p_N;
            filename = p_filename;
            A = new double[N,N];
        }

        public void addPoint()
        {

        }

        public void generatePAR()
        {
            int x = visualmath.Choose(4, 2);
            string s = "PROBLEM_FILE = " + filename + ".tsp\n";
            s = s + "MOVE_TYPE = 5\n";
            s = s + "PATCHING_C = 3\n";
            s = s + "PATCHING_A = 2\n";
            s = s + "RUNS = 1\n";
            s = s + "TOUR_FILE = " + filename + ".tour\n";
            System.IO.File.WriteAllText(filename + ".par", s);
        }

        public void generateTSPLIB()
        {
            int x = visualmath.Choose(4, 2);
            string s = "";
            s = s + "NAME: " + filename + "\n";
            s = s + "TYPE: TSP\n";
            s = s + "DIMENSION: " + N.ToString() + "\n";
            s = s + "EDGE_WEIGHT_TYPE: EXPLICIT\n";
            s = s + "EDGE_WEIGHT_FORMAT: LOWER_DIAG_ROW\n";
            System.IO.File.WriteAllText(filename+".tsp", s);
        }
    }
}
