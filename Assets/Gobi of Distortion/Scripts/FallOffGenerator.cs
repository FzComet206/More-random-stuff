using UnityEngine;

public static class FallOffGenerator 
{
    public static float[,] GenerateFallOffMap(int size)
    {
        float[,] map = new float[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                // -1 to 1
                float x = i / (float) size * 2 - 1;
                float y = j / (float) size * 2 - 1;
                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                // hard coded fall off 
                if (value > 0.9f)
                {
                    // somehow this had to be 2 instead of 1
                    value = 1f;
                }
                else
                {
                    value = 0f;
                }
                
                map[i, j] = value;
            } 
        }

        return map;
    }
}
