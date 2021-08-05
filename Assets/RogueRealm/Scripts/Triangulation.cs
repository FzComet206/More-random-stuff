using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Triangulation 
{
    public static List<Path> DelaunayTriangulation(List<Node> nodes)
    {
        List<Path> paths = new List<Path>();

        Path t1 = new Path();
        Path t2 = new Path();
        Path t3 = new Path();

        t1.left = nodes[0];
        t1.right = nodes[1];
        
        t2.left = nodes[3];
        t2.right = nodes[4];
        
        t3.left = nodes[1];
        t3.right = nodes[10];
        
        paths.Add(t1);
        paths.Add(t2);
        paths.Add(t3);
        
        return paths;
    }
}
