//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GABAK
{
    static class visualmath
    {
        /// <summary>
        /// Rotates source coordinate around the base coordinate with an angle
        /// </summary>
        /// <param name="p_basecoordinate">Fixed Point</param>
        /// <param name="p_sourcecoordinate">Rotated Point</param>
        /// <param name="angle">Rotation angle in radian</param>
        /// <returns></returns>
        public static node rotate(node p_basecoordinate, node p_sourcecoordinate, double angle)
        {
            double px, py;
            double ox, oy;
            double nx, ny;
            px = p_sourcecoordinate.getX();
            py = p_sourcecoordinate.getY();
            ox = p_basecoordinate.getX();
            oy = p_basecoordinate.getY();
            nx = Math.Cos((degreeToRadian(angle))) * (px - ox) - Math.Sin(degreeToRadian(angle)) * (py - oy) + ox;
            ny = Math.Sin((degreeToRadian(angle))) * (px - ox) + Math.Cos(degreeToRadian(angle)) * (py - oy) + oy;
            p_sourcecoordinate.setX(nx);
            p_sourcecoordinate.setY(ny);
            return p_sourcecoordinate;
        }

        public static double degreeToRadian(double degree)
        {
            return (Math.PI * degree / 180.0);
        }

        public static double radianToDegree(double radian)
        {
            double degree = radian * (180.0 / Math.PI);
            if (degree < 0)
            {
                degree = degree + 360;
            }
            if (degree > 360)
            {
                degree = degree - 360;
            }
            return degree;
        }

        public static int Factorial(int x)
        {
            int fact = 1;
            if (x == 0)
            {
                return 1;
            }
            while (x >= 1)
            {
                fact *= x;
                x--;
            }
            return fact;
        }

        public static int Choose(int n, int r)
        {
            return (int)((double)Factorial(n)/(double)(Factorial(n-r) * (Factorial(r))));
        }

        public static double calculateAngle(double px1, double py1, double px2, double py2)
        {
            double angle = 0;
            angle = Math.Atan2(px1 - px2, py1 - py2) * (180 / Math.PI);
            return angle;
        }

        public static double distance(double px1, double py1, double px2, double py2)
        {
            double d1 = Math.Pow(px1 - px2, 2);
            double d2 = Math.Pow(py1 - py2, 2);
            double distance = Math.Sqrt(d1 + d2);
            return distance;
        }

        public static bool pointSide(double px1, double py1, double px2, double py2, double px3, double py3)
        {
            return (px2 - px1) * (py3 - py1) > (py2 - py1) * (px3 - px1);
        }

        public static double getDeterminant(double px1, double py1, double px2, double py2)
        {
            return px1 * py2 - px2 * py1;
        }

        //Used for checking whether two lines intersect or not
        public static bool lineSegmentCross(double p_X1, double p_Y1, double p_X2, double p_Y2, double p_X3, double p_Y3, double p_X4, double p_Y4)
        {
            //int precision = 12;//Used for rounding error problem when intersection of 90 degree pick aisles
            node tempcoordinate = new node(options.tempnode);
            double ua, ub;

            double x1 = p_X1;
            double x2 = p_X2;
            double x3 = p_X3;
            double x4 = p_X4;

            double y1 = p_Y1;
            double y2 = p_Y2;
            double y3 = p_Y3;
            double y4 = p_Y4;

            if((x1 == x3 && y1 == y3) || (x1 == x4 && y1 == y4) || (x2 == x3 && y2 == y3) || (x2 == x4 && y2 == y4))//This is a special case which needs to be checked (there was a bug before this with visibility graph
            {
                return false;
            }

            //Fixing the rounding error problem when picking aisles are 90 degrees
            //x1 = Math.Round(x1, precision);
            //x2 = Math.Round(x2, precision);
            //x3 = Math.Round(x3, precision);
            //x4 = Math.Round(x4, precision);
            //y1 = Math.Round(y1, precision);
            //y2 = Math.Round(y2, precision);
            //y3 = Math.Round(y3, precision);
            //y4 = Math.Round(y4, precision);

            double denom = ((y4 - y3) * (x2 - x1)) - ((x4 - x3) * (y2 - y1));

            if (denom == 0)
            {
                return false;
            }

            double noma = ((x4 - x3) * (y1 - y3)) - ((y4 - y3) * (x1 - x3));

            double nomb = ((x2 - x1) * (y1 - y3)) - ((y2 - y1) * (x1 - x3));

            ua = noma / denom;
            ub = nomb / denom;
            if ((ua < 1) && (ua > 0) && (ub < 1) && (ub > 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
