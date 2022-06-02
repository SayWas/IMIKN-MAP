using IMIKN_MAP.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMIKN_MAP.Services
{
    class Graph
    {
        public Dot[] graph { get; set; }
        public Graph(string[] temp)
        {
            List<Dot> allDots = new List<Dot>();
            for (int i = 0; i < temp.Length; i += 4)
            {
                //allDots.Add(new Dot(temp[i], float.Parse(temp[i + 1]), float.Parse(temp[i + 2]), temp[i + 3].Split(',')));
            }
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
                    }
                }
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

        private double DistBetween(Dot a, Dot b) { return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2)); }
    }
}
