using UnityEngine;

public static class TextureGenerator 
{
    public static Texture2D TextureFromColourMap(float[,] noiseMap, MapGenerator.TerrainType[] regions ,int mapWidth)
    {
        Color[] colourMap = new Color[mapWidth * mapWidth];
        for (int y = 0; y < mapWidth; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float curHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (curHeight >= regions[i].height)
                    {
                        colourMap[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }
            }
        }
        
        Texture2D texture = new Texture2D(mapWidth, mapWidth);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }
}
