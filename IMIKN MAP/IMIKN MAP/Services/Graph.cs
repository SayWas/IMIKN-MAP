using IMIKN_MAP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMIKN_MAP.Services
{
    class Graph
    {
        public Dot Location { get; private set; }
        public Dot[] graph { get; set; }
        public Rib[] ribs { get; set; }
        public Graph(string temp)
        {
            List<Dot> allDots = new List<Dot>();
            var events = JArray.Parse(temp);
            foreach (var item in events)
                allDots.Add(new Dot((string)item["Id"], (double)item["X"], (double)item["Y"], (int)item["Floor"], item["LinkedPoints"].ToObject<string[]>(), (bool)item["IsStairs"]));
            graph = new Dot[allDots.Count];
            allDots.CopyTo(graph);
            for (int i = 0; i < graph.Length - 1; i++)
            {
                for (int k = i + 1; k < graph.Length; k++)
                {
                    foreach (var a in graph[i].LinkedId)
                    {
                        if (a == graph[k].Id)
                        {
                            graph[i].LinkedDots.Add(graph[k]);
                            graph[k].LinkedDots.Add(graph[i]);
                            break;
                        }
                    }
                }
            }

            List<Rib> allRibs = new List<Rib>();
            for (int i = 0; i < graph.Length; i++)
            {
                for (int j = i + 1; j < graph.Length; j++)
                {
                    if (graph[i].LinkedDots.Contains(graph[j]))
                    {
                        allRibs.Add(new Rib(graph[i], graph[j]));
                    }
                }
            }
            ribs = allRibs.ToArray();
        }
        public Dot[] GetPath(string from, string to)
        {
            int fromInd = 0, toInd = 0;
            for (int i = 0; i < graph.Length; i++)
            {
                if (graph[i].Id == from) { fromInd = i; }
                else if (graph[i].Id == to) { toInd = i; }
            }

            double[][] relMatr = new double[graph.Length][];
            for (int i = 0; i < graph.Length; i++)
            {
                relMatr[i] = new double[graph.Length];
                relMatr[i][i] = 0;
                for (int j = 0; j < graph.Length; j++)
                {
                    relMatr[i][j] = double.MaxValue;
                    if (graph[i].LinkedDots.Contains(graph[j]))
                    {
                        relMatr[i][j] = DistBetween(graph[i], graph[j]);
                        if (graph[i].IsStairs && graph[j].IsStairs) relMatr[i][j] += Math.Abs(graph[i].Floor - graph[j].Floor) * 25;
                    }
                }
            }

            if (Array.TrueForAll(relMatr[fromInd], val => val == double.MaxValue))
            {
                Console.WriteLine("Нет пути"); return new Dot[0];
            }

            return RecoverPath(fromInd, toInd, relMatr, Algoriphm(fromInd, graph.Length, relMatr), graph);
        }
        private double[] Algoriphm(int beg, int graphLeng, double[][] relMatr)
        {
            double temp, min;
            int minindex;

            int begin_index = beg;
            double[] minDist = new double[graphLeng];
            bool[] visited = new bool[graphLeng];

            for (int i = 0; i < graphLeng; i++)
            {
                minDist[i] = double.MaxValue;
                visited[i] = false;
            }
            minDist[begin_index] = 0;

            do
            {
                minindex = int.MaxValue;
                min = double.MaxValue;
                for (int i = 0; i < graphLeng; i++)
                {
                    if (!visited[i] && minDist[i] < min)
                    {
                        min = minDist[i];
                        minindex = i;
                    }
                }

                if (minindex != int.MaxValue)
                {
                    for (int i = 0; i < graphLeng; i++)
                    {
                        if (relMatr[minindex][i] > 0)
                        {
                            temp = min + relMatr[minindex][i];
                            if (temp < minDist[i])
                            {
                                minDist[i] = temp;
                            }
                        }
                    }
                    visited[minindex] = true;
                }
            } while (minindex < int.MaxValue);

            return minDist;
        }
        private Dot[] RecoverPath(int begin_index, int final_index, double[][] relMatr, double[] minDist, Dot[] graph)
        {
            List<int> ver = new List<int>();
            int end = final_index;
            ver.Add(final_index);
            int ind = 1;
            double weight = minDist[end];

            while (end != begin_index)
            {
                for (int i = 0; i < minDist.Length; i++)
                {
                    if (relMatr[i][end] != 0 && relMatr[i][end] != double.MaxValue)
                    {
                        double t = weight - relMatr[i][end];
                        if (t == minDist[i] || (t + 0.01 > minDist[i] && t - 0.01 < minDist[i]))
                        {
                            weight = t;
                            end = i;
                            ver.Add(i);
                            ind++;
                            if (end == begin_index) break;
                        }
                    }
                }
            }
            List<Dot> p = new List<Dot>();
            foreach (var g in ver)
            {
                p.Add(graph[g]);
            }
            p.Reverse();
            return p.ToArray();
        }
        public void AddDot(float x, float y, int floor)
        {
            var newgraph = graph.ToList();
            Rib closestRib = null;
            double minDist = double.MaxValue;

            if (!(Location is null))
            {
                newgraph.Remove(Location);
            }

            foreach (var r in ribs)
            {
                var a = DistToRib(x, y, r);
                if (floor == r.Floor && a < minDist)
                {
                    minDist = a;
                    closestRib = r;
                }
            }
            if (closestRib.Vertical)
            {
                if (y < closestRib.YLimit[1] && y > closestRib.YLimit[0])
                {
                    Location = new Dot("Мое местоположение", x - (float)minDist, y, floor, linkedDots: new List<Dot>() { closestRib.Dots[0], closestRib.Dots[1] });
                }
                else
                {
                    var temp = new Dot("Мое местоположение", x, y, floor, linkedDots: new List<Dot>() { closestRib.Dots[0], closestRib.Dots[1] });
                    var a = DistBetween(closestRib.Dots[0], temp);
                    var b = DistBetween(closestRib.Dots[1], temp);
                    if (a < b)
                    {
                        Location = new Dot("Мое местоположение", closestRib.Dots[0].X, closestRib.Dots[0].Y, floor, linkedDots: new List<Dot>() { closestRib.Dots[0], closestRib.Dots[1] });
                    }
                    else
                    {
                        Location = new Dot("Мое местоположение", closestRib.Dots[1].X, closestRib.Dots[1].Y, floor, linkedDots: new List<Dot>() { closestRib.Dots[0], closestRib.Dots[1] });
                    }
                }
            }
            else
            {
                if (y < closestRib.YLimit[1] && y > closestRib.YLimit[0] && x < closestRib.XLimit[1] && x > closestRib.XLimit[0])
                {
                    double K = -1 / closestRib.K;
                    double B = y - x * K;
                    float xrib = (float)((B - closestRib.B) / (closestRib.K - K));
                    float yrib = (float)(K * xrib + B);
                    Location = new Dot("Мое местоположение", xrib, yrib, floor, linkedDots: new List<Dot>() { closestRib.Dots[0], closestRib.Dots[1] });
                }
                else
                {
                    var temp = new Dot("Мое местоположение", x, y, floor, linkedDots: new List<Dot>() { closestRib.Dots[0], closestRib.Dots[1] });
                    var a = DistBetween(closestRib.Dots[0], temp);
                    var b = DistBetween(closestRib.Dots[1], temp);
                    if (a < b)
                    {
                        Location = new Dot("Мое местоположение", closestRib.Dots[0].X, closestRib.Dots[0].Y, floor, linkedDots: new List<Dot>() { closestRib.Dots[0], closestRib.Dots[1] });
                    }
                    else
                    {
                        Location = new Dot("Мое местоположение", closestRib.Dots[1].X, closestRib.Dots[1].Y, floor, linkedDots: new List<Dot>() { closestRib.Dots[0], closestRib.Dots[1] });
                    }
                }

            }
            newgraph.Add(Location);
            graph = newgraph.ToArray();
        }

        private static double DistBetween(Dot a, Dot b) { return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2)); }
        private static double DistToRib(float x, float y, Rib r)
        {
            if (r.Vertical)
            {
                if (y < r.YLimit[1] && y > r.YLimit[0])
                {
                    return x - r.B;
                }
                var a = DistBetween(new Dot("a", x, y, 1, new string[0]), r.Dots[0]);
                var b = DistBetween(new Dot("b", x, y, 1, new string[0]), r.Dots[1]);
                return a < b ? a : b;
            }
            else
            {
                if (y < r.YLimit[1] && y > r.YLimit[0] && x < r.XLimit[1] && x > r.XLimit[0])
                {
                    return Math.Abs(r.K * x - y + r.B) / Math.Sqrt(Math.Pow(r.K, 2) + 1);
                }
                var a = DistBetween(new Dot("a", x, y, 1, new string[0]), r.Dots[0]);
                var b = DistBetween(new Dot("b", x, y, 1, new string[0]), r.Dots[1]);
                return a < b ? a : b;
            }
        }
    }
}
