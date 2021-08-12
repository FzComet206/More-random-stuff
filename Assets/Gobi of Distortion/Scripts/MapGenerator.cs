using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class MapGenerator : MonoBehaviour
{
    [System.Serializable]
    public struct NoiseMapOptions
    {
        public int mapWidth;
        public float scale;
        public int octaves;
        public float persistance;
        public float lacunarity;
        public float heightScale;
        public float offsetx;
        public float offsety;
        public float firstClampThreshold;
        public float secondClampThreshold;
        public float thirdClampThreshold;
        public float fourthClampThreshold;
        public float fifthClampThreshold;
        public int depth;
        public bool fallOffMap;
        public bool useFlatShading;
        public int seed;
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
    
    // struct giving the color correspond with height
    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }

    [System.Serializable]
    public struct MapOptions1
    {
        public NoiseMapOptions noiseOptions;
        public TerrainType[] regions;
        public Entities[] spawnPool;
    }

    public MapOptions1 options;
    
    // other
    private float[,] noiseMapCache;
    private List<GameObject> objectPool;
    public Transform spawnerPool;
    
    private void Start()
    {
        objectPool = new List<GameObject>();
        options.noiseOptions.seed = new Random().Next(1, 1000000);
        DrawMesh();
        DrawTextureMap();
        SpawnObjects();
    }

    private float[,] GetNoiseMap()
    {
        float[,] noiseMap = NoiseMap.GenerateNoiseMap(options);

        noiseMapCache = noiseMap;
        return noiseMap;
    }

    public void DrawMesh()
    {
        GetNoiseMap();
        ActualMapDisplay display = FindObjectOfType<ActualMapDisplay>();
        display.DrawMeshMap(MeshGenerator.GenerateTerrainMesh(
            noiseMapCache, 
            options.noiseOptions.depth, 
            options.noiseOptions.useFlatShading));
    }

    public void DrawTextureMap()
    {
        ActualMapDisplay display = FindObjectOfType<ActualMapDisplay>();
        display.DrawTextureMap(
            TextureGenerator.TextureFromColourMap(
                noiseMapCache,
                options.regions,
                options.noiseOptions.mapWidth,
                options.noiseOptions.mapWidth
            ));
    }

    public void SpawnObjects()
    {
        var locations = SpawnManager.SpawnEntities(NoiseMap.GetSpawnMap(), options.spawnPool, options.noiseOptions.seed);
        for (int i = 0; i < locations.Length; i++)
        {
            foreach (var location in locations[i])
            {
                // start generating objects, here check if the object have a clear radius
                // if it has, set the clear radius in a global matrix with two loops
                int x = location[0];
                int y = location[1];

                Vector3 loc = new Vector3(x, (noiseMapCache[x, y] * options.noiseOptions.depth) + options.spawnPool[i].spawnOffsetY, y);

                GameObject obj = Instantiate(options.spawnPool[i].obj, loc, Quaternion.Euler(0, 0, 0));
                
                obj.transform.localScale = options.spawnPool[i].scale;
                obj.transform.parent = spawnerPool;
                objectPool.Add(obj);
            }
        }
    }
}
    

