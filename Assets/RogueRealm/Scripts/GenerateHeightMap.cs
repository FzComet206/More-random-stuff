using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.Timeline;

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
    public static float[,] GetHeightMap(int width, int height, MapGeneratorTwo.Area[] areas, float depth)
    {
        System.Random random = new System.Random();
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
            List<Vector2> possibleLocations = new List<Vector2>();

            // hard coded grid distance so that the nodes dont overlaps with a radius
            int gridDistance = 40;
            
            //for (int i = start + gridDistance; i < curr - gridDistance; i+= gridDistance)
            //{
                //for (int j = gridDistance; j < height - gridDistance; j+= gridDistance)
                //{
                    //possibleLocations.Add(new Vector2(i, j));
                //}
            //}

            for (int i = start + gridDistance; i < curr - gridDistance; i++)
            {
                for (int j = gridDistance; j < height - gridDistance; j++)
                {
                    possibleLocations.Add(new Vector2(i, j));
                }
            }
            
            // select from current area
            var selectedLocations = possibleLocations 
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
                    minRadius = 18,
                    maxRadius = 30
                };

                nodeList.Add(node);
            }
            
            start = curr;
            curr += parts;
        }

        // set dungeon radius
        foreach (var node in nodeList)
        {
            // init a node
            int rad = random.Next(node.minRadius, node.maxRadius);
            node.positionPixels = new Vector3[(2*rad+1) * (2*rad+1)];

            int positionPixelIndex = 0;
            
            for (int i = -rad; i < rad + 1; i++)
            {
                for (int j = -rad; j < rad + 1; j++)
                {
                    // x and y coordinate
                    int x = (int) node.position.x + i;
                    int y = (int) node.position.y + j;
                    
                    // also store the position pixels
                    node.positionPixels[positionPixelIndex] = new Vector3(x, depth, y);
                    positionPixelIndex++;

                    // edge cases
                    if (x < width && y < height && x >= 0 && y >= 0)
                    {
                        heightMap[x, y] = -depth;
                    }
                }
            }
        }
        
        // this nodelist is key to graph

        // put those nodes and their distance to each other into a graph
        // figure out minimal spanning tree from the graph and get the paths
        // add a few paths from the original graph to make few loops
        // represent the path in the matrix withs heights and fatten them
        // fatten the paths and the nodes (rooms)
        // apply some cellular automata or perlin worms

        return heightMap;
    }
}
