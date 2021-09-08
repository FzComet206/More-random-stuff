using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator2
{
    public static SquareGrid squareGrid;

    public static void GenerateMesh(int[,] map, float squareSize)
    {
        squareGrid = new SquareGrid(map, squareSize);
        return;
    }
    
    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize)
        {
            
        }
    }
    
    public class Square
    {
        public ControlNode topLeft, topRight, bottomLeft, bottomRight;
        public Node centerTop, centerRight, centerLeft, centerBottom;

        public Square(
            ControlNode _topLeft,
            ControlNode _topRight,
            ControlNode _bottomRight, 
            ControlNode _bottomLeft
        )
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomLeft = _bottomLeft;
            bottomRight = _bottomRight;

            centerTop = topLeft.right;
            centerRight = bottomRight.above;
            centerBottom = bottomLeft.right;
            centerLeft = bottomLeft.above;
        }
    }
    
    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }

    public class ControlNode: Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _pos, bool _active, float _squareSize) : base(_pos)
        {
            active = _active;
            above = new Node(position + Vector3.forward * _squareSize);
            right = new Node(position + Vector3.right * _squareSize);
        }
    }
}
