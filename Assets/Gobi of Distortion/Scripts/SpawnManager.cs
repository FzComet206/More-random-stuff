using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public static class SpawnManager
{
    // this entire class blow contains countless potential shit code
    // please proceed with caution !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public static List<int[]>[] SpawnEntities(int[][,] areaMAp, MapGenerator.Entities[] spawnPool, int seed)
    {

        int[,] occupiedMap = new int[areaMAp[0].GetLength(0), areaMAp[0].GetLength(1)];
            
        // return this list of obj and locations
        List<int[]>[] locationMap = new List<int[]>[spawnPool.Length];

        for (int i = 0; i < spawnPool.Length; i++)
        {
            List<int[]> possibleLocations = new List<int[]>();
            
            // get current entity reference from struct
            MapGenerator.Entities currEntity = spawnPool[i];
            
            // get area map matrix and process it
            int[,] curr = areaMAp[currEntity.area];

            // check for special options !!!!!!!!!!!!!!!!!!!
            if (currEntity.specialOptions != MapGenerator.Special.Nope)
            {
                // if this is Spawn or Entrance, which means only one spawn location
                // append to location map with unique method then break loop
                if (currEntity.specialOptions == MapGenerator.Special.Spawn)
                {
                    int[] coord = ProcessSpawn(curr);
                    
                    // add occupied map
                    if (currEntity.clearRadius != 0)
                    {
                        int r = currEntity.clearRadius;
                        
                        // iterate over the radius square
                        for (int k = -r; k < r; k++)
                        {
                            for (int l = -r; l < r; l++)
                            {
                                occupiedMap[coord[0] + k, coord[1] + l] = 1;
                            }
                        }
                    }
                    
                    locationMap[i] = new List<int[]>() {coord}; 
                    continue;
                }
                if (spawnPool[i].specialOptions == MapGenerator.Special.Entrance)
                {
                    int[] coord = ProcessEntrance(curr);
                    
                    // repeating code from above
                    if (currEntity.clearRadius != 0)
                    {
                        int r = currEntity.clearRadius;
                        
                        // iterate over the radius square
                        for (int k = -r; k < r; k++)
                        {
                            for (int l = -r; l < r; l++)
                            {
                                occupiedMap[coord[0] + k, coord[1] + l] = 1;
                            }
                        }
                    }
                    
                    locationMap[i] = new List<int[]>() {coord}; 
                    continue;
                }
                
                // if this is the other three options, then apply algorithms
                // to filter out the viable spawn locations
                curr = ApplyLeetCodeAlgorithms(curr,currEntity.specialOptions);
            }

            // Add ones to a array meaning possible spawn points
            for (int j = 0; j < curr.GetLength(0); j++)
            {
                for (int k = 0; k < curr.GetLength(0); k++)
                {
                    // if the area matrix is one, then ok
                    // here add checks the global matrix to see
                    // if the space is occupied by other clear radius

                    if (curr[j, k] != 0 && occupiedMap[j, k] != 1)
                    {
                        if (currEntity.clearRadius != 0)
                        {
                            int r = currEntity.clearRadius;
                            for (int m = -r; m < r; m++)
                            {
                                for (int n = -r; n < r; n++)
                                {
                                    try
                                    {
                                        // if radius is too large that causes index error
                                        // just ignore
                                        occupiedMap[j + m, k + n] = 1;
                                    }
                                    catch (Exception e) {}
                                }
                            }
                        }
                        
                        possibleLocations.Add(new int[2] {j, k});
                    }
                }
            }

            Random random = new Random(seed);
            // lol nah fuck you 
            var selectedLocs = possibleLocations
                .OrderBy(x => random.Next())
                .Take(spawnPool[i].amount);

            List<int[]> finalizedLocs = new List<int[]>();
            foreach (var loc in selectedLocs)
            {
                finalizedLocs.Add(loc);
            }

            locationMap[i] = finalizedLocs;
        }

        return locationMap;
    }

    private static int[,] ApplyLeetCodeAlgorithms(int[,] matrix, MapGenerator.Special options)
    {
        switch (options)
        {
            case MapGenerator.Special.Edge:
                return ProcessEdge(matrix);

            case MapGenerator.Special.Inner:
                return ProcessInner(matrix);

            case MapGenerator.Special.Center:
                return ProcessCenter(matrix);

            default:
                return matrix;
        }
    }

    public static int[,] ProcessEdge(int[,] matrix)
    {
        int[,] zeroOne = ZeroOneDistance(matrix);
        int m = zeroOne.GetLength(0);
        int n = zeroOne.GetLength(1);
        
        // because this is Edge, leave only the cells that is 1 distance from the 0 cells
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (zeroOne[i, j] == 1)
                {
                    zeroOne[i, j] = 1;
                }
                else
                {
                    zeroOne[i, j] = 0;
                }
            }
        }
        // this returns the matrix representing only the edges of a given area
        return zeroOne;
    }

    public static int[,] ProcessInner(int[,] matrix)
    {
        int[,] zeroOne = ZeroOneDistance(matrix);
        int m = zeroOne.GetLength(0);
        int n = zeroOne.GetLength(1);
        
        // because this is Inner, leave only the cells that is more than 8 distance from the 0 cell
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (zeroOne[i, j] < 5)
                {
                    zeroOne[i, j] = 0;
                }
                else
                {
                    zeroOne[i, j] = 1;
                }
            }
        }
        
        // this returns the matrix representing only the inner part of a given area
        return zeroOne;
    }

    public static int[,] ProcessCenter(int[,] matrix)
    {
        int[,] zeroOne = ZeroOneDistance(matrix);
        int m = zeroOne.GetLength(0);
        int n = zeroOne.GetLength(1);
        
        // because this is Center, get the largest number in a matrix representing center of an area
        // then leave only the cells that is at most 3 less that the largest number of an area

        // get largest
        int largest = 0;
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                largest = Mathf.Max(largest, zeroOne[i, j]);
            }
        }

        // iterate again to set values to only 1 and 0
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                // if the area is 1/3 away from center then nope
                if (zeroOne[i, j] < largest - (int)(largest * 0.3))
                {
                    zeroOne[i, j] = 0;
                }
                else
                {
                    zeroOne[i, j] = 1;
                }
            }
        }
        
        return zeroOne;
    }

    // ProcessSpawn and ProcessEntrance returns int[] instead of vector
    // in order to be consistent with the whole spawn map
    public static int[] ProcessSpawn(int[,] matrix)
    {
        // this function returns only one coordinate
        // select an point from the first 1/4 of the map
        int[,] zeroOne = ZeroOneDistance(matrix);
        int cutoff = (zeroOne.GetLength(0) / 4);

        List<int[]> valid = new List<int[]>();
        
        for (int i = 0; i < cutoff; i++)
        {
            for (int j = 0; j < zeroOne.GetLength(1); j++)
            {
                // the positon is valid if it't not too near the edge
                // to prevent random enclosed spawn points
                if (zeroOne[i, j] >= 10)
                {
                    valid.Add(new int[]{i, j});
                }
            }
        }
     
        // return the random location
        Random random = new Random();
        return valid[random.Next(0, valid.Count)];
    }

    public static int[] ProcessEntrance(int[,] matrix)
    {
        // this function returns only one coordinate
        // select most center point from the last 1/4 of the map
        int[,] zeroOne = ZeroOneDistance(matrix);

        int length = zeroOne.GetLength(0);
        int cutoff = length - (length / 4);

        List<int[]> valid = new List<int[]>();
        
        // iterate from 3/4 to end
        
        var largestCoord = new int[2] {0, 0};
        int largest = 0;
        for (int i = cutoff ; i < length; i++)
        {
            for (int j = 0; j < zeroOne.GetLength(1); j++)
            {
                if (zeroOne[i, j] > largest)
                {
                    largest = zeroOne[i, j];
                    largestCoord[0] = i;
                    largestCoord[1] = j;
                }
            }
        }

        return largestCoord;
    }

    public static int[,] ZeroOneDistance(int[,] matrix)
    {
        // 01 matrix algoithms from leetcode
        
        var m = matrix.GetLength(0);
        var n = matrix.GetLength(1);

        int[,] zeroOne = (int[,]) matrix.Clone();

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (zeroOne[i, j] == 1)
                {
                    int top;
                    int left;
                    
                    // warning, dont mod below lines where it says i +- 1 or j +- 1
                    // idk why this works but this works
                    top = (i != 0 ? zeroOne[i - 1, j] : 0);
                    left = (j != 0 ? zeroOne[i, j - 1] : 0);

                    zeroOne[i, j] = Mathf.Min(top, left) + 1;
                }
            }
        }

        for (int i = m-1; i >= 0; i--)
        {
            for (int j = n-1; j >= 0; j--)
            {
                int bottom;
                int right;

                bottom = (i < m - 1 ? zeroOne[i + 1, j] : 0);
                right = (j < n - 1 ? zeroOne[i, j + 1] : 0);

                zeroOne[i, j] = Mathf.Min(zeroOne[i, j], bottom + 1, right + 1);
            }
        }

        // this returns a matrix with each cell an integer representing the distance of that cell
        // from the nearest zero cell
        
        return zeroOne;
    }
}
