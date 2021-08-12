using UnityEngine;

public static class NoiseMap
{
    private static int[][,] spawnMap;
    
    public static float[,] GenerateNoiseMap(MapGenerator.MapOptions1 options)
    {
        MapGenerator.NoiseMapOptions nm = options.noiseOptions;
        // Set seed
        System.Random prng = new System.Random(nm.seed);
        Vector2[] octavesOffset = new Vector2[nm.octaves];
        for (int i = 0; i < nm.octaves; i++)
        {
            // scrolling and random octaves
            float offsetXOct = prng.Next(-100000, 100000) + nm.offsetx;
            float offsetYOct = prng.Next(-100000, 100000) + nm.offsety;
            octavesOffset[i] = new Vector2(offsetXOct, offsetYOct);
        }
        
        // 5 different height so 5 list of coordinates
        // indices from 0 to 4 ==> from highest to lowest ground
        spawnMap = new int[6][,];
        for (int i = 0; i < spawnMap.Length; i++)
        {
            spawnMap[i] = new int[nm.mapWidth, nm.mapWidth];
        }

        float[,] noiseMap = new float[nm.mapWidth, nm.mapWidth];

        // clamp scale
        if (nm.scale <= 0)
        {
            nm.scale = 0.0001f;
        }
        
        
        // generate noise
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y < nm.mapWidth; y++)
        {
            for (int x = 0; x < nm.mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < nm.octaves; i++)
                {
                    // apply octaves sample and offsets with scale and freq
                    float sampleX = x / nm.scale * frequency + octavesOffset[i].x;
                    float sampleY = y / nm.scale * frequency + octavesOffset[i].y;
                    
                    // float sampleX = x / scale * frequency + offsetx;
                    // float sampleY = y / scale * frequency + offsety;

                    // perlinValue of range -1 to 1 sample
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    // noiseHeight is the key output
                    noiseHeight += perlinValue * amplitude;

                    // update frequence and amplitude with persistance and lacunarity
                    amplitude *= nm.persistance;
                    frequency *= nm.lacunarity;
                }

                // get the max and min noise height in order to normalize the noise map
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight * nm.heightScale;
            }
        }

        if (nm.fallOffMap)
        {
            // get fall off map matrix
            float[,] falloffMap = FallOffGenerator.GenerateFallOffMap(nm.mapWidth);
            
            for (int y = 0; y < nm.mapWidth; y++)
            {
                for (int x = 0; x < nm.mapWidth; x++)
                {
                    // inverseLerp returns 0 and 1
                    float normalizedPerlin = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                    
                    // apply fall off map
                    noiseMap[x, y] = Mathf.Clamp01(normalizedPerlin + falloffMap[x, y]);
                    
                    // modify perlin height based on rules
                    noiseMap[x, y] = ModifiedPerlinRules(
                        noiseMap[x,y],
                        nm.firstClampThreshold, 
                        nm.secondClampThreshold, 
                        nm.thirdClampThreshold,
                        nm.fourthClampThreshold,
                        nm.fifthClampThreshold,
                        x,
                        y);
                }
            }
        }
        else
        {
            // iterate again to normalize
            for (int y = 0; y < nm.mapWidth; y++)
            {
                for (int x = 0; x < nm.mapWidth; x++)
                {
                    // inverseLerp returns 0 and 1
                    float normalizedPerlin = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                    // modify perlin height based on rules
                    noiseMap[x, y] = ModifiedPerlinRules(
                        normalizedPerlin, 
                        nm.firstClampThreshold, 
                        nm.secondClampThreshold, 
                        nm.thirdClampThreshold,
                        nm.fourthClampThreshold,
                        nm.fifthClampThreshold,
                        x,
                        y);
                }
            }
        }
        
        return noiseMap;
    }

    static float ModifiedPerlinRules(float perlin, float first, float second, float third, float fourth, float fifth, int _x, int _y)
    {
        float modPerlin;
        if (perlin >= first)
        {
            // highest
            modPerlin = 1f;
        }
        else if (perlin >= second)
        {
            // cliff
            modPerlin = 0.9f;
            spawnMap[1][_x, _y] = 1;
        }
        else if (perlin >= third)
        {
            modPerlin = 0.5f;
            spawnMap[1][_x, _y] = 1;
        }
        else if (perlin >= (third - 0.06f))
        {
            // walkable
            modPerlin = 0.05f;
            spawnMap[0][_x, _y] = 1;
            spawnMap[2][_x, _y] = 1;
        }
        else if (perlin >= fourth)
        {
            // walkable
            modPerlin = 0f;
            spawnMap[0][_x, _y] = 1;
            spawnMap[3][_x, _y] = 1;
        }
        else
        {
            if (perlin >= fifth)
            {
                // walkable
                modPerlin = 0.05f;
                spawnMap[0][_x, _y] = 1;
                spawnMap[4][_x, _y] = 1;
            }
            else
            {
                // middle
                modPerlin = 0.5f;
                spawnMap[1][_x, _y] = 1;
                spawnMap[5][_x, _y] = 1;
            }
        }
        return modPerlin;
    }

    public static int[][,] GetSpawnMap()
    {
        return spawnMap;
    }
    
}
