using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;

namespace GABAK
{
    static class statistics
    {
        //public static double[] tInterval(double[] p_data, double p_confidencelevel)
        //{
        //    double[] MSCI = new double[6];//Mean and Confidence Interval
        //    int n = p_data.Length;
        //    double xbar = 0;
        //    double sum = 0;
        //    double sumOfSqrs = 0;
        //    double stdDeviation = 0;
        //    double p = (1 - p_confidencelevel);
        //    for (int i = 0; i < n; i++ )
        //    {
        //        sum += p_data[i];
        //    }
        //    xbar = sum / n;
        //    for (int i = 0; i < n; i++ )
        //    {
        //        sumOfSqrs += ((p_data[i] - xbar) * (p_data[i] - xbar));
        //    }
        //    stdDeviation = Math.Sqrt(sumOfSqrs / (n - 1));
        //    Chart chart1 = new Chart();
        //    double t = chart1.DataManipulator.Statistics.InverseTDistribution(p, n-1);
        //    double halfinterval = t * (stdDeviation / Math.Sqrt(n));
        //    MSCI[0] = xbar;
        //    MSCI[1] = stdDeviation;
        //    MSCI[2] = xbar - halfinterval;
        //    MSCI[3] = xbar + halfinterval;
        //    return MSCI;
        //}

        private static double Gauss(double z)
        {
            // input = z-value (-inf to +inf)
            // output = p under Standard Normal curve from -inf to z
            // e.g., if z= 0.0, function returns 0.5000
            // ACM Algorithm #209
            double y; // 209 scratch variable
            double p; // result. called 'z' in 209
            double w; // 209 scratch variable
            if (z == 0.0)
            {
                p = 0.0;
            }
            else
            {
                y = Math.Abs(z) / 2;
                if (y >= 3.0)
                {
                    p = 1.0;
                }
                else if (y < 1.0)
                {
                    w = y * y;
                    p = ((((((((0.000124818987 * w
                        - 0.001075204047) * w + 0.005198775019) * w
                        - 0.019198292004) * w + 0.059054035642) * w
                        - 0.151968751364) * w + 0.319152932694) * w
                        - 0.531923007300) * w + 0.797884560593) * y * 2.0;
                }
                else
                {
                    y = y - 2.0;
                    p = (((((((((((((-0.000045255659 * y
                      + 0.000152529290) * y - 0.000019538132) * y
                      - 0.000676904986) * y + 0.001390604284) * y
                      - 0.000794620820) * y - 0.002034254874) * y
                      + 0.006549791214) * y - 0.010557625006) * y
                      + 0.011630447319) * y - 0.009279453341) * y
                      + 0.005353579108) * y - 0.002141268741) * y
                      + 0.000535310849) * y + 0.999936657524;

                }
            }
            if ( z > 0.0)
            {
                return (p + 1.0) / 2;
            }
            else
            {
                return (1.0 - p) / 2;
            }
        }

        public static double Student(double t, double df)
        {
            // for large integer df or double df
            // adapted from ACM algorithm 395
            // returns right-tail p-value
            double n = df; // to sync with ACM parameter name
            double a, b, y;
            t = t * t;
            y = t / n;
            b = y + 1.0;
            if (y > 1.0E-6) y = Math.Log(b);
            a = n - 0.5;
            b = 48.0 * a * a;
            y = a * y;
            y = (((((-0.4 * y - 3.3) * y - 24.0) * y - 85.5) /
                (0.8 * y * y + 100.0 + b) + y + 3.0) / b + 1.0) *
                Math.Sqrt(y);
            return Gauss(-y);  // ACM algorithm 209
        }

        public static double StudentT(double t, double df)
        {
            // www.matrixlab-examples.com/t-distribution.html

            double x = 1;
            double y = 1;
            double s, r, z, j, k, l, a1, a2, a3, a4;
            t = t * t;

            // Computers using inverse for small T-values
            if(t < 1)
            {
                s = df;
                r = y;
                z = 1 / t;
            }
            else
            {
                s = y;
                r = df;
                z = t;
            }

            j = 2 / (9 * s);
            k = 2 / (9 * r);

            // Computes using approximation formulas
            l = Math.Abs((1 - k) * Math.Pow(z, (1 / 3)) - 1 + j) / Math.Sqrt(k * Math.Pow(z, (2 / 3)) + j);
 
            if (r < 4)
            {
                l = l * (1 + 0.08 * Math.Pow(l, 4) / Math.Pow(r, 3));
            }

            a1 = 0.196854;
            a2 = 0.115194;
            a3 = 0.000344;
            a4 = 0.019527;
            x = 0.25 / Math.Pow((1 + l * (a1 + l * (a2 + l * (a3 + l * a4)))), 4);
            x = Math.Floor(x * 10000 + 0.5) / 10000;

            // Adjusts if inverse was calculated
            if (t < 1)
            {
                x = 1 - x;
            }

            return x;
        }

        //double ibetainv(p, a, b)
        //{
        //var EPS = 1e-8,
        //    a1 = a - 1,
        //    b1 = b - 1,
        //    j = 0,
        //    lna, lnb, pp, t, u, err, x, al, h, w, afac;
        //if(p <= 0) return 0;
        //if(p >= 1) return 1;
        //if(a >= 1 && b >= 1) 
        //{
        //    pp = (p < 0.5) ? p : 1 - p;
        //    t = Math.sqrt(-2 * Math.log(pp));
        //    x = (2.30753 + t * 0.27061) / (1 + t* (0.99229 + t * 0.04481)) - t;
        //    if(p < 0.5) x = -x;
        //    al = (x * x - 3) / 6;
        //    h = 2 / (1 / (2 * a - 1)  + 1 / (2 * b - 1));
        //    w = (x * Math.sqrt(al + h) / h) - (1 / (2 * b - 1) - 1 / (2 * a - 1)) * (al + 5 / 6 - 2 / (3 * h));
        //    x = a / (a + b * Math.exp(2 * w));
        //} 
        //else
        //{
        //    lna = Math.log(a / (a + b));
        //    lnb = Math.log(b / (a + b));
        //    t = Math.exp(a * lna) / a;
        //    u = Math.exp(b * lnb) / b;
        //    w = t + u;
        //    if(p < t / w) x = Math.pow(a * w * p, 1 / a);
        //    else x = 1 - Math.pow(b * w * (1 - p), 1 / b);
        //}
        //afac = -gammaln(a) - gammaln(b) + gammaln(a + b);
        //for(int j = 0; j < 10; j++)
        //{
        //    if(x === 0 || x === 1) return x;
        //    err = ibeta(x, a, b) - p;
        //    t = Math.exp(a1 * Math.log(x) + b1 * Math.log(1 - x) + afac);
        //    u = err / t;
        //    x -= (t = u / (1 - 0.5 * Math.min(1, u * (a1 / x - b1 / (1 - x)))));
        //    if(x <= 0) x = 0.5 * (x + t);
        //    if(x >= 1) x = 0.5 * (x + t + 1);
        //    if(Math.abs(t) < EPS * x && j > 0) break;
        //}
        //return x;
        //}

        //double gammaln(double x)
        //{
        //var j = 0,
        //    cof = [
        //        76.18009172947146, -86.50532032941677, 24.01409824083091,
        //        -1.231739572450155, 0.1208650973866179e-2, -0.5395239384953e-5
        //    ],
        //    ser = 1.000000000190015,
        //    xx, y, tmp;
        //tmp = (y = xx = x) + 5.5;
        //tmp -= (xx + 0.5) * Math.log(tmp);
        //for (; j < 6; j++) ser += cof[j] / ++y;
        //return Math.log(2.5066282746310005 * ser / xx) - tmp;
        //}

    }
}
