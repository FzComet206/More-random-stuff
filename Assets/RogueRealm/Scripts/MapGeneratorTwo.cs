using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class MapGeneratorTwo : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public int depth;
    
    // cache
    private float[,] noiseMapCache;

    [System.Serializable]
    public struct Area
    {
        public int numberOfNodes;
        public float minDistance;
    }

    public Area[] areas;

    public MapGenerator.TerrainType[] regions;
    
    private void Start()
    {
        DrawMesh();
        DrawTextureMap();
    }

    private float[,] GetNoiseMap()
    {
        float[,] heightMap =  GenerateHeightMap.GetHeightMap(mapWidth, mapHeight);
        noiseMapCache = heightMap;
        return heightMap;
    }

    public void DrawMesh()
    {
        GetNoiseMap();
        ActualMapDisplay display = FindObjectOfType<ActualMapDisplay>();
        display.DrawMeshMap(MeshGenerator.GenerateTerrainMesh(noiseMapCache, depth, true));
    }

    public void DrawTextureMap()
    {
        ActualMapDisplay display = FindObjectOfType<ActualMapDisplay>();
        display.DrawTextureMap(
            TextureGenerator.TextureFromColourMap(
                noiseMapCache,
                regions,
                mapWidth,
                mapHeight
            ));
    }
}