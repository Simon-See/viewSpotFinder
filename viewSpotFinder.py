import sys
import json
import math


# https://www.geeksforgeeks.org/union-find-algorithm-set-2-union-by-rank/
def find(parent, i):
    if parent[i] == -1:
        return i

    return find(parent, parent[i])


def union(parent, x, y):
    xset = find(parent, x)
    yset = find(parent, y)
    if xset == y or yset == x:
        return
    parent[xset] = yset


def getAllTheMaxima(jsonData, amountOfViewSpots):
    # save all the local Maxima
    allLocalMaxima = []

    # save the max height and The Elements who have the max height for each node
    maxHeight = [(-math.inf, [])] * len(jsonData['nodes'])

    for i in range(len(jsonData['elements'])):

        # get the nodes of the cur Element
        curElementNodes = jsonData['elements'][i]['nodes']

        # get The Height
        curHeight = jsonData['values'][i]['value']

        areWeLocalMaxima = True

        # for each node check if the max height can be topped
        for curNode in curElementNodes:

            (nodesMaxHeight, arr) = maxHeight[curNode]
            # cur Height is bigger -> new curHeight
            if nodesMaxHeight < curHeight:

                # delete every ArrayElement from the allLocalMaxima list
                for el in arr:
                    if el in allLocalMaxima:
                        allLocalMaxima.remove(el)

                # change the nodesMaxHeight and make a new arr
                maxHeight[curNode] = (curHeight, [i])

            # they are the same -> add the element to the arr
            elif nodesMaxHeight == curHeight:
                arr = arr.append(i)  # TODO CHECK IF THIS WORKS
            else:
                areWeLocalMaxima = False

        # if we are local Maxima -> add to list
        if areWeLocalMaxima:
            allLocalMaxima.append(i)

    # Union Find in order to only output one element when they have exactly the same value

    parent = [-1] * len(jsonData['elements'])

    for [a, arr] in maxHeight:
        if len(arr) > 1:

            # get an element that is a local max or has a root that is a max
            par = arr[0]
            for el in arr:
                if el in allLocalMaxima or find(parent, el) in allLocalMaxima:
                    par = el
                    break

            # union everything
            for el in arr:
                if el != par:
                    union(parent, el, par)

    # only those who are have a -1 or themselves as parent can be maxima:
    for el in allLocalMaxima:
        if not (el == parent[el] or parent[el] == -1):
            allLocalMaxima.remove(el)

    # sort the viewSpots by index
    allLocalMaxima.sort()
    counter = 0
    results = []

    # get the first n indices that are localMaxima
    for viewSpotID in allLocalMaxima:
        if counter >= amountOfViewSpots:
            break
        results.append(jsonData['values'][viewSpotID])
        counter += 1

    # sort the maxima by value
    results.sort(key=lambda s: s['value'], reverse=True)

    # create the Output
    answer = "[\n"
    for el in results:
        answer += "\t" + str(el) + ",\n"

    answer = answer[:len(answer) - 2]
    answer += "\n]"
    answer = answer.replace("'", "")

    print(answer)


file = open(sys.argv[1])
allJsonData = json.load(file)
getAllTheMaxima(allJsonData, int(sys.argv[2]))

file.close()
