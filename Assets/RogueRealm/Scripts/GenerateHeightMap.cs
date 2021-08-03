using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Node
{
    public string name;
    public Vector2 position;
    public int areaIndex;
    public Node[] neighbours;
    public Path[] paths;
    public Vector3[] positionPixels;
    public int minRadius;
    public int maxRadius;
}
public class Path
{
    public string name;
    public string type;
    public Node left;
    public Node right;
    public Vector3[] positionPixels;
}

public class Graph
{
    
}

public class GenerateHeightMap
{
    public static float[,] GetHeightMap(int width, int height, MapGeneratorTwo.Area[] areas)
    {
        System.Random random = new System.Random(174115261);
        List<Node> nodeList = new List<Node>();
        
        float[,] heightMap = new float[width,height];

        int parts = width / areas.Length;
        int curr = parts;
        int start = 0;
        
        for (int c = 0; c < areas.Length; c++)
        {
            // choose the amount of nodes
            int num = areas[c].numberOfNodes;
            
            // get current areas of the iteration
            List<Vector2> currLocations = new List<Vector2>();
            for (int i = start; i < curr; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    currLocations.Add(new Vector2(i, j));
                }
            }

            // select from current area
            var selectedLocations = currLocations
                .OrderBy(x => random.Next())
                .Take(num);
            
            // init node object and assign area
            foreach (var loc in selectedLocations)
            {
                Node node = new Node()
                {
                    name = "xd", 
                    neighbours = null, 
                    paths = null, 
                    position = loc,
                    areaIndex = c,
                    minRadius = 8,
                    maxRadius = 25
                };

                nodeList.Add(node);
            }
            
            start = curr;
            curr += parts;
        }

        // set dungeon radius
        foreach (var node in nodeList)
        {
            int rad = random.Next(node.minRadius, node.maxRadius);
            for (int i = -rad; i < rad + 1; i++)
            {
                for (int j = -rad; j < rad + 1; j++)
                {
                    int x = (int) node.position.x + i;
                    int y = (int) node.position.y + j;

                    if (x < width && y < height && x >= 0 && y >= 0)
                    {
                        heightMap[x, y] = -10f;
                    }
                }
            }
        }
        
        // this nodelist is key to graph

        // pick positions for nodes
        // put those nodes and their distance to each other into a Gabriel graph
        // figure out minimal spanning tree from the graph and get the paths
        // add a few paths from the original graph to make few loops
        // represent the path in the matrix withs 1s through A-Star
        // fatten the paths and the nodes (rooms)
        // apply some cellular automata or perlin worms

        return heightMap;
    }
}
