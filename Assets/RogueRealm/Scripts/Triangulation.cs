using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Triangulation 
{
    public static List<Path> DelaunayTriangulation(List<Node> nodes)
    {
        List<Path> paths = new List<Path>();
        
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            Path cur = new Path();
            cur.left = nodes[i];
            cur.right = nodes[i + 1];
            paths.Add(cur);
        }
        
        
        return paths;
    }
}
