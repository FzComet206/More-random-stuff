using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateHeightMap
{
    private struct Node
    {
        private string name;
        private string type;
        private Vector2 position;
        private int areaIndex;
        private Node[] neighbours;
        private Path[] paths;
        private Vector3[] positionPixels;
    }
    private struct Path
    {
        private string name;
        private string type;
        private Node[] nodes;
        private Vector3[] positionPixels;
    }
    public static float[,] GetHeightMap(int width, int height)
    {
        float[,] heightMap = new float[width,height];
        
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
