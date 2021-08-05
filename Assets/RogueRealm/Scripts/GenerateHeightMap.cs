using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.Timeline;
using UnityEngine.XR;

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
    public static float[,] GetHeightMap(int width, int height, MapGeneratorTwo.Area[] areas, float depth, int seed)
    {
        System.Random random = new System.Random(seed.GetHashCode());
        List<Node> nodeList = new List<Node>();
        
        float[,] heightMap = new float[width, height];

        int parts = width / areas.Length;
        int curr = parts;
        int start = 0;
        
        for (int c = 0; c < areas.Length; c++)
        {
            int num = areas[c].numberOfCaves;
            
            // get current areas of the iteration
            List<Vector2> possibleLocations = new List<Vector2>();

            // hard coded grid distance so that the nodes dont overlaps with a radius
            int gridDistance = 30;

            for (int i = start + gridDistance; i < curr - gridDistance; i += gridDistance)
            {
                for (int j = gridDistance; j < height - gridDistance; j += gridDistance)
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
                    minRadius = 40,
                    maxRadius = 40
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
                        heightMap[x, y] = depth;
                    }
                }
            }
        }

        List<Path> graph = Triangulation.DelaunayTriangulation(nodeList);

        heightMap = DrawPath(heightMap, graph, depth);
        
        // this nodelist is key to graph
        // put those nodes and their distance to each other into a graph
        // figure out minimal spanning tree from the graph and get the paths
        // add a few paths from the original graph to make few loops
        // represent the path in the matrix withs heights and fatten them
        // fatten the paths and the nodes (rooms)
        // apply some cellular automata or perlin worms

        return heightMap;
    }

    private static float[,] DrawPath(float[,] heightMap, List<Path> paths, float depth)
    {
        foreach (var path in paths)
        {
            Vector2 start = path.left.position;
            Vector2 end = path.right.position;

            int startX = (int) start.x;
            int startY = (int) start.y;

            int endX = (int) end.x;
            int endY = (int) end.y;

            if (startX < endX)
            {
                for (int i = startX; i < endX; i++)
                {
                    heightMap[i, startY] = depth;
                }
            }
            else
            {
                for (int i = startX; i > endX; i--)
                {
                    heightMap[i, startY] = depth;
                }
            }

            if (startY < endY)
            {
                for (int i = startY; i < endY; i++)
                {
                    heightMap[endX, i] = depth;
                }
            }
            else
            {
                for (int i = startY; i > endY; i--)
                {
                    heightMap[endX, i] = depth;
                }
            }
        }
        
        return heightMap;
    }
}
