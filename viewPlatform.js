class Main {
    //static #nodes = null;
    //static #elements = null;
    //static #values = null;

    static main() {
        
        let jsonData = require(process.argv[2])
        const numViewSpots = parseInt(process.argv[3]);
        let maxima = this.getAllTheMaxima(jsonData);
        let amountOfMaximaFound = 0;

        let viewSpots = [];
        for (let i = 0; amountOfMaximaFound < numViewSpots && i < maxima.length; i++) {
            if (maxima[i]) {
                amountOfMaximaFound++;
                viewSpots.push(jsonData['values'][i])
            }
        }
        viewSpots.sort((a, b) => b['value'] - a['value'])
        let str = "";
        str += "[";
        for (let i = 0; i < viewSpots.length; i++) {
            str += "\n\t" + (JSON.stringify(viewSpots[i])).replace("\"", "").replace(":", ": ").replace(",", ", ").replace("\"", "").replace("\"", "").replace("\"", "") + ",";
        }
        str = str.substring(0, str.length - 1);
        str += "\n]";
        console.log(str);
    }

    static getAllTheMaxima(jsonData) {
        // save in an bool array weather the elements are currently a local maxima
        let localMaxima = Array(jsonData['elements'].length).fill(false);
        // save the max height for each node
        let maxHeight = [...Array(jsonData['nodes'].length)].map(x => Number.NEGATIVE_INFINITY);
        // each node gets a list of elements who have the max height
        let maxElements = Array(jsonData['nodes'].length).fill([]);
        for (let i = 0; i < jsonData['elements'].length; i++) {
            // get the nodes of the current Element
            let curElementNodes = jsonData['elements'][i]['nodes'];
            // get the height of the current Element
            let curHeight = jsonData['values'][i]['value'];
            // for each node check if the max height can be topped
            let itIsLocalMaxima = true;

            for (let j = 0; j < curElementNodes.length; j++) {
                let nodeIndex = curElementNodes[j];
                // cur Height is bigger -> new curHeitght
                if (maxHeight[nodeIndex] < curHeight) {
                    maxHeight[nodeIndex] = curHeight;

                    // the old maxima isnt a maxima no more -> bool value must be set to false
                    maxElements[nodeIndex].forEach(index => localMaxima[index] = false);
                    maxElements[nodeIndex] = new Array(0);
                    // add the CurrentIndex as the new Max height
                    maxElements[nodeIndex].push(i);
                } else if (maxHeight[curElementNodes[j]] <= curHeight) {
                    maxElements[nodeIndex].push(i);
                } else {
                    itIsLocalMaxima = false;
                }
            }
            localMaxima[i] = itIsLocalMaxima;
        }

        // In the case when two or more neighboring elements have exactly the same value, only one of the elements should be reported as a view spot.
        // -> if there are 2 local Max adjacent only one should be a max -> use Union find
        let parent = Array(jsonData['elements'].length).fill(-1);
        for (let i = 0; i < jsonData['nodes'].length; i++) {
            // if there are multiple elements next to each other with the same height -> union
            if (maxElements[i].length > 1) {
                // make someone parent who is a local max or where the root is a max
                let par = maxElements[i][0];

                for (let j = 0; j < maxElements[i].length; j++) {
                    if (localMaxima[maxElements[i][j]] || localMaxima[this.find(parent, maxElements[i][j])]) {
                        par = maxElements[i][j];
                        break;
                    }
                }

                //union everything
                for (let j = 0; j < maxElements[i].length; j++) {
                    if (maxElements[i][j] !== par) {
                        this.union(parent, maxElements[i][j], par);
                    }
                }
            }
        }
        // only those who are a root (those who have a -1 or a root to themselves) can be local maxima -> if there are multiple besides each other only one will be printed
        for (let i = 0; i < parent.length; i++) {
            localMaxima[i] = localMaxima[i] && (parent[i] === -1 || parent[i] === i)
        }

        return localMaxima;
    }

// https://www.geeksforgeeks.org/union-find-algorithm-set-2-union-by-rank/
    static find(parent, i) {
        if (parent[i] == -1) {
            return i;
        }
        return this.find(parent, parent[i]);
    }

    static union(parent, x, y) {
        let xset = this.find(parent, x);
        let yset = this.find(parent, y);
        if (xset == y || yset == x) {
            return;
        }
        parent[xset] = yset;
    }
}

Main.main([]);
