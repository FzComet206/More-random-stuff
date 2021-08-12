using UnityEngine;
using UnityEngine.Rendering;

public class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float [,] heightMap, float heightScale, bool useFlatShading)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        int vertexIndex = 0;
        
        MeshData meshData = new MeshData(width, height, useFlatShading);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                    // this sets the vertices for mesh
                    meshData.vertices[vertexIndex] = new Vector3(x, heightMap[x, y] * heightScale, y);
                    // uv tells each vertex where it locates with respect to rest vertices (0 to 1)
                    meshData.uvs[vertexIndex] = new Vector2(x / (float) width, y / (float) height);

                    // ignore right and bottom vertices for the map
                    if (x < width - 1 && y < height - 1)
                    {
                        // vertices to first triangles (i --> i + width + 1 --> i + width)
                        // vertices to second triangles (i + width + 1 --> i --> i + 1)
                        // switch the b and c order means inverting the normals here
                        meshData.AddTriangles(vertexIndex + width + 1, vertexIndex + 1, vertexIndex);
                        meshData.AddTriangles(vertexIndex, vertexIndex + width, vertexIndex + width + 1);
                    }
                    
                    vertexIndex++;
            }
        }
        
        meshData.ProcessMesh();
        return meshData;
    }
}

public class MeshData
{
    // vertices
    public Vector3[] vertices;
    
    // 3 vertices for a triangle, 6 for a square
    public int[] triangles;
    
    // one uv for each vertex
    public Vector2[] uvs;

    // keep track of vertices to triangles reference
    private int triangleIndex;

    private bool useFlatShading;
    
    // constructor
    public MeshData(int meshWidth, int meshHeight, bool useFlatShading)
    {
        this.useFlatShading = useFlatShading;
        vertices = new Vector3[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        uvs = new Vector2[meshWidth * meshHeight];
    }

    public void AddTriangles(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    void FlatShading()
    {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            // i dont fking understand this
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVertices;
        uvs = flatShadedUvs;
    }

    public void ProcessMesh()
    {
        if (useFlatShading)
        {
            FlatShading();
        }
    }
    public Mesh CreateMesh()
    {
        // build mesh
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.Optimize();
        return mesh;
    }
}
