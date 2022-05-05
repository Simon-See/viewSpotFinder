package com.company;

import org.json.simple.JSONArray;
import org.json.simple.JSONObject;
import org.json.simple.parser.*;

import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Comparator;

public class Main {

    private static JSONArray nodes;
    private static JSONArray elements;
    private static JSONArray values;


    public static void main(String[] args) throws IOException, ParseException {

        JSONObject jsonData = (JSONObject) new JSONParser().parse(new FileReader(args[0]));

        nodes = (JSONArray) jsonData.get("nodes");
        elements = (JSONArray) jsonData.get("elements");
        values = (JSONArray) jsonData.get("values");

        int numViewSpots = Integer.parseInt(args[1]);
        

        boolean[] maxima = getAllTheMaxima();

        int amountOfMaximaFound = 0;

        ArrayList<JSONObject> viewSpots = new ArrayList<>();

        for (int i = 0; amountOfMaximaFound < numViewSpots && i < maxima.length; i++) {
            if (maxima[i]) {
                amountOfMaximaFound++;
                viewSpots.add((JSONObject) values.get(i));

            }
        }
        viewSpots.sort(Comparator.comparingDouble(o -> (-(double) o.get("value"))));


        StringBuilder str = new StringBuilder();
        str.append("[");
        viewSpots.forEach(el -> str.append("\n\t").append(el).append(","));
        str.delete(str.length() - 1, str.length());
        str.append("\n]");

        System.out.println(str.toString().replace("\"", "").replace(",value", ", value").replace(":", ": "));
    }


    private static boolean[] getAllTheMaxima() {

        //save in an bool array weather the elements are currently a local maxima
        boolean[] lokalMaxima = new boolean[elements.size()];

        //save the max height for each node
        double[] maxHeight = new double[nodes.size()];
        Arrays.fill(maxHeight, Double.NEGATIVE_INFINITY);

        //each node gets a list of elements who have the max height
        ArrayList<Integer>[] maxElements = new ArrayList[nodes.size()];

        for (int i = 0; i < maxElements.length; i++) {
            maxElements[i] = new ArrayList<>();
        }

        for (int i = 0; i < elements.size(); i++) {

            //get the nodes of the current Element
            JSONArray curElementNodes = (JSONArray) ((JSONObject) elements.get(i)).get("nodes");

            //get the height of the current Element
            double curHeight = (double) ((JSONObject) values.get(i)).get("value");

            //for each node check if the max height can be topped
            boolean itIsLocalMaxima = true;

            for (int j = 0; j < curElementNodes.size(); j++) {

                Long nodeIndexLong = (Long) curElementNodes.get(j);
                int nodeIndex = nodeIndexLong.intValue();
                //cur Height is bigger -> new curHeitght
                if (maxHeight[nodeIndex] < curHeight) {

                    maxHeight[nodeIndex] = curHeight;

                    //the old maxima isnt a maxima no more -> bool value must be set to false
                    maxElements[nodeIndex].forEach(index -> lokalMaxima[index] = false);
                    maxElements[nodeIndex].clear();
                    //add the CurrentIndex as the new Max height
                    maxElements[nodeIndex].add(i);
                }
                //same -> put it in the list of maxima
                else if (maxHeight[((Long) curElementNodes.get(j)).intValue()] == curHeight) {
                    maxElements[nodeIndex].add(i);
                }
                //not bigger -> it is not a maxima -> make the bool value false
                else {
                    itIsLocalMaxima = false;
                }
            }
            lokalMaxima[i] = itIsLocalMaxima;
        }

        // In the case when two or more neighboring elements have exactly the same value, only one of the elements should be reported as a view spot.
        // -> if there are 2 local Max adjacent only one should be a max -> use Union find

        int[] parent = new int[elements.size()];
        Arrays.fill(parent, -1);

        for (int i = 0; i < nodes.size(); i++) {
            //if there are multiple elements next to each other with the same height -> union
            if (maxElements[i].size() > 1) {

                //make someone parent who is a local max or where the root is a max
                int par = maxElements[i].stream().filter(el -> lokalMaxima[el] || lokalMaxima[find(parent, el)]).findFirst().orElse(maxElements[i].get(0));
                for (int j = 0; j < maxElements[i].size(); j++) {

                    if (maxElements[i].get(j) != par) {
                        union(parent, maxElements[i].get(j), par);
                    }
                }
            }
        }


        //only those who are a root (those who have a -1 or a root to themselves) can be local maxima -> if there are multiple besides each other only one will be printed
        for (int i = 0; i < parent.length; i++) {
            lokalMaxima[i] = lokalMaxima[i] && (parent[i] == -1 || parent[i] == i);
        }


        return lokalMaxima;
    }

    //https://www.geeksforgeeks.org/union-find-algorithm-set-2-union-by-rank/
    static int find(int[] parent, int i) {
        if (parent[i] == -1)
            return i;
        return find(parent, parent[i]);
    }

    static void union(int[] parent, int x, int y) {
        int xset = find(parent, x);
        int yset = find(parent, y);

        if (xset == y || yset == x) {
            return;
        }

        parent[xset] = yset;
    }

}




