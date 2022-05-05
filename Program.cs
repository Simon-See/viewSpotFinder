using System.IO;
using System;
using System;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace ViewPlatformConsoleApplication
{
    public class Program
    {
        public static void Main(String[] args)
        {
            StreamReader r = new StreamReader(args[1]);
            string jsonString = r.ReadToEnd();
            var jsonData = JObject.Parse(jsonString);
            bool[] maxima = GetAllTheMaxima(jsonData);


            var numViewSpots = int.Parse(args[2]);
            var amountOfMaximaFound = 0;
            var viewSpots = new List<JToken>();
            for (int i = 0; amountOfMaximaFound < numViewSpots && i < maxima.Length; i++)
            {
                if (maxima[i])
                {
                    amountOfMaximaFound++;
                    viewSpots.Add(jsonData["values"][i]);
                }
            }

            // viewSpots.sort(Comparator.comparingDouble(o -> (-(double) o.get("value"))));
            viewSpots.Sort( (a,b) =>   comparingDouble(((double)a["value"] ), ((double)b["value"])));
            
            var str = new StringBuilder();
            str.Append("[");
            viewSpots.ForEach((el) => str.Append("\n\t").Append("{element_id: ").Append(el["element_id"]).Append(", value: ").Append(el["value"]).Append("},"));
            str.Remove(str.Length - 1, 1);
            str.Append("\n]");
            Console.WriteLine(string.Join(", ", str));
        }

        private static bool[] GetAllTheMaxima(JObject jsonData)
        {
            // save in an bool array weather the elements are currently a local maxima
            bool[] lokalMaxima = new bool[jsonData["elements"].Count()];
            


            // save the max height for each node
            double[] maxHeight = new double[jsonData["nodes"].Count()];

            for (int i = 0; i < maxHeight.Length; i++)
            {
                maxHeight[i] = Double.NegativeInfinity;
            }

            // each node gets a list of elements who have the max height
            List<int>[] maxElements = new List<int>[jsonData["nodes"].Count()];
            for (int i = 0; i < maxElements.Length; i++)
            {
                maxElements[i] = new List<int>();
            }

            for (int i = 0; i < jsonData["elements"].Count(); i++)
            {
                // get the nodes of the current Element
                var curElementNodes = jsonData["elements"][i]["nodes"];
                // get the height of the current Element
                var curHeight = (double)(jsonData["values"][i]["value"]);
                // for each node check if the max height can be topped
                var itIsLocalMaxima = true;
                for (int j = 0; j < curElementNodes.Count(); j++)
                {
                    var nodeIndex = (int) curElementNodes[j];

                    // cur Height is bigger -> new curHeitght
                    if (maxHeight[nodeIndex] < curHeight)
                    {
                        maxHeight[nodeIndex] = curHeight;
                        // the old maxima isnt a maxima no more -> bool value must be set to false
                        maxElements[nodeIndex].ForEach((index) => lokalMaxima[index] = false);
                        maxElements[nodeIndex].Clear();
                        // add the CurrentIndex as the new Max height
                        maxElements[nodeIndex].Add(i);
                    }
                    else if (maxHeight[nodeIndex] == curHeight)
                    {
                        maxElements[nodeIndex].Add(i);
                    }
                    else
                    {
                        itIsLocalMaxima = false;
                    }
                }

                lokalMaxima[i] = itIsLocalMaxima;
            }


            

            // In the case when two or more neighboring elements have exactly the same value, only one of the elements should be reported as a view spot.
            // -> if there are 2 local Max adjacent only one should be a max -> use Union find
            int[] parent = new int[jsonData["elements"].Count()];
            for (int i = 0; i < parent.Length; i++)
            {
                parent[i] = -1;
            }

            for (int i = 0; i < jsonData["nodes"].Count(); i++)
            {
                // if there are multiple elements next to each other with the same height -> union
                if (maxElements[i].Count > 1)
                {
                    // make someone parent who is a local max or where the root is a max
                    var par = maxElements[i][0];

                    for (int k = 0; k < maxElements[i].Count; k++)
                    {
                        if (lokalMaxima[maxElements[i][k]] || lokalMaxima[find(parent, maxElements[i][k])])
                        {
                            par = maxElements[i][k];
                            break;
                        }
                    }

                    for (int j = 0; j < maxElements[i].Count; j++)
                    {
                        if (maxElements[i][j] != par)
                        {
                            union(parent, maxElements[i][j], par);
                        }
                    }
                }
            }

            // only those who are a root (those who have a -1 or a root to themselfes) can be local maxima -> if there are multiple besides each other only one will be printed
            for (int i = 0; i < parent.Length; i++)
            {
                lokalMaxima[i] = lokalMaxima[i] && (parent[i] == -1 || parent[i] == i);
            }

            return lokalMaxima;
        }

        // https://www.geeksforgeeks.org/union-find-algorithm-set-2-union-by-rank/
        private static int find(int[] parent, int i)
        {
            if (parent[i] == -1)
            {
                return i;
            }

            return find(parent, parent[i]);
        }

        private static void union(int[] parent, int x, int y)
        {
            var xset = find(parent, x);
            var yset = find(parent, y);
            if (xset == y || yset == x)
            {
                return;
            }

            parent[xset] = yset;
        }

        private static int comparingDouble(double a, double b)
        {
            if (a > b)
                return -1;
            if (a < b)
                return 1;
            return 0;
        }
    }
}