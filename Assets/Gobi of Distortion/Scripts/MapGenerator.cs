using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class MapGenerator : MonoBehaviour
{
    public int mapWidth;

    public float noiseScale;

    public int octaves;
    public float persistance;
    public float lacunarity;

    public float heightScale;
    public float offsetx = 0f;
    public float offsety = 0f;

    public float firstClampThreshold;
    public float secondClampThreshold;
    public float thirdClampThreshold;
    public float fourthClampThreshold;
    public float fifthClampThreshold;

    public int depth;

    public bool fallOffMap;
    public bool useFlatShading;
    public bool autoUpdate;

    public int layerIndex = 0;

    public TerrainType[] regions;

    // below are the settings for spawn
    public Entities[] spawnPool;

    [System.Serializable]
    public struct Entities
    {
        public GameObject obj;
        public int amount;
        public int area;
        public float spawnOffsetY;
        public Vector3 scale;
        public int clearRadius;
        public Special specialOptions;
    }
    public enum Special
    {
        Nope,
        Edge,
        Inner,
        Center,
        Spawn,
        Entrance
    } 
    
    // other
    public Transform spawnerPool;
    private int seed;
    
    // cache
    private float[,] noiseMapCache;
    private List<GameObject> objectPool;

    // struct giving the color correspond with height
    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }

    private void Start()
    {
        objectPool = new List<GameObject>();
        seed = new Random().Next(1, 1000000);
        DrawMesh();
        DrawTextureMap();
        SpawnObjects();
    }

    private float[,] GetNoiseMap()
    {
        float[,] noiseMap = NoiseMap.GenerateNoiseMap(
            mapWidth,
            noiseScale,
            octaves,
            persistance,
            lacunarity,
            heightScale,
            offsetx,
            offsety,
            firstClampThreshold,
            secondClampThreshold,
            thirdClampThreshold,
            fourthClampThreshold,
            fifthClampThreshold,
            seed,
            fallOffMap);

        noiseMapCache = noiseMap;
        return noiseMap;
    }

    public void DrawLayerMesh()
    {

        var layerMapInt = SpawnManager.ZeroOneDistance(NoiseMap.GetSpawnMap()[layerIndex]);

        float[,] layerMap = new float[layerMapInt.GetLength(0), layerMapInt.GetLength(1)];
        for (int i = 0; i < layerMapInt.GetLength(0); i++)
        {
            for (int j = 0; j < layerMap.GetLength(1); j++)
            {
                layerMap[i, j] = (float) layerMapInt[i, j];
            }
        }

        ActualMapDisplay display = FindObjectOfType<ActualMapDisplay>();
        display.DrawLayerMeshMap(MeshGenerator.GenerateTerrainMesh(layerMap, depth, useFlatShading));
    }

    public void DrawMesh()
    {
        GetNoiseMap();
        ActualMapDisplay display = FindObjectOfType<ActualMapDisplay>();
        display.DrawMeshMap(MeshGenerator.GenerateTerrainMesh(noiseMapCache, depth, useFlatShading));
    }

    public void DrawTextureMap()
    {
        ActualMapDisplay display = FindObjectOfType<ActualMapDisplay>();
        display.DrawTextureMap(
            TextureGenerator.TextureFromColourMap(
                noiseMapCache,
                regions,
                mapWidth
            ));
    }

    public void SpawnObjects()
    {
        var locations = SpawnManager.SpawnEntities(NoiseMap.GetSpawnMap(), spawnPool, seed);
        for (int i = 0; i < locations.Length; i++)
        {
            foreach (var location in locations[i])
            {
                // start generating objects, here check if the object have a clear radius
                // if it has, set the clear radius in a global matrix with two loops
                int x = location[0];
                int y = location[1];

                Vector3 loc = new Vector3(x, (noiseMapCache[x, y] * depth) + spawnPool[i].spawnOffsetY, y);

                GameObject obj = Instantiate(spawnPool[i].obj, loc, Quaternion.Euler(0, 0, 0));
                
                obj.transform.localScale = spawnPool[i].scale;
                obj.transform.parent = spawnerPool;
                objectPool.Add(obj);
            }
        }
    }

    public void DeleteObjs()
    {
        foreach (var obj in objectPool)
        {
            DestroyImmediate(obj);
        }
    }
}
    

