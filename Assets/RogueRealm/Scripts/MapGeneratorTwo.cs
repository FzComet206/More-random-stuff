using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = System.Random;

public class MapGeneratorTwo : MonoBehaviour
{
    [System.Serializable]
    public struct Area
    {
        public int numberOfCaves;
    }
    
    [System.Serializable]
    public struct MapOptions2
    {
        public int mapWidth;
        public int mapHeight;
        public int depth;
        public int seed;
        public Area[] areas;
    }

    // serialize fields
    public MapOptions2 options;
    public MapGenerator.TerrainType[] regions;
    
    // cache
    private float[,] noiseMapCache;
    
    private void Start()
    {
        DrawMesh();
        DrawTextureMap();
    }

    private float[,] GetNoiseMap()
    {
        float[,] heightMap =  GenerateHeightMap.GetHeightMap(options);
        noiseMapCache = heightMap;
        return heightMap;
    }

    public void DrawMesh()
    {
        GetNoiseMap();
        ActualMapDisplay display = FindObjectOfType<ActualMapDisplay>();
        display.DrawMeshMap(MeshGenerator.GenerateTerrainMesh(noiseMapCache, options.depth, true));
    }

    public void DrawTextureMap()
    {
        ActualMapDisplay display = FindObjectOfType<ActualMapDisplay>();
        display.DrawTextureMap(
            TextureGenerator.TextureFromColourMap(
                noiseMapCache,
                regions,
                options.mapWidth,
                options.mapHeight
            ));
    }
}