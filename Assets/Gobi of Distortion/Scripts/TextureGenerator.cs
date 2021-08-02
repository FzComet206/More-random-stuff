using UnityEngine;

public static class TextureGenerator 
{
    public static Texture2D TextureFromColourMap(float[,] noiseMap, MapGenerator.TerrainType[] regions ,int mapWidth, int mapHeight)
    {
        Color[] colourMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
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
        
        Texture2D texture = new Texture2D(mapWidth, mapHeight);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }
}
