using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = System.Random;

public class MapGeneratorTwo : MonoBehaviour
{
    public enum RoomType { Square, Cellular }
    public enum HallType { Straight, Slope }
    public enum ConnectionType { Msp, MspRandom, GridRandom }
    public enum MeshType { HeightMap, MarchingSquare }
    
    [System.Serializable]
    public struct MapStructure
    {
        public RoomType roomType;
        public HallType hallType;
        public ConnectionType connectionType;
        public MeshType meshType;
    }
    
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
        public MapStructure map;
        public MapGenerator.TerrainType[] regions;
    }

    // serialize fields
    public MapOptions2 options;
    
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
        
        if (options.map.meshType == MeshType.HeightMap)
        {
            display.DrawMeshMap(MeshGenerator.GenerateTerrainMesh(
                noiseMapCache, 
                options.depth,
                true));
            return;
        }

        if (options.map.meshType == MeshType.MarchingSquare)
        {
            int height = noiseMapCache.GetLength(0);
            int width = noiseMapCache.GetLength(1);
            int[,] marchingSquareMap = new int[height, width];
            
            for (int i = 0; i < height;i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (noiseMapCache[i, j] > 0.1)
                    {
                        marchingSquareMap[i, j] = 1;
                    }
                    else
                    {
                        marchingSquareMap[i, j] = 0;
                    }
                }
            }
            
            // display.DrawMeshMap(MeshGenerator2.GenerateMesh(
            //    marchingSquareMap,
            //    1));
        }
    }

    public void DrawTextureMap()
    {
        ActualMapDisplay display = FindObjectOfType<ActualMapDisplay>();
        display.DrawTextureMap(
            TextureGenerator.TextureFromColourMap(
                noiseMapCache,
                options.regions,
                options.mapWidth,
                options.mapHeight
            ));
    }
}