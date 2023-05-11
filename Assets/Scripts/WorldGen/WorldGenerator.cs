using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;

public class WorldGenerator : MonoBehaviour
{

    void Start()
    {
        //GenerateTerrainLayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //protected void GenerateTerrainLayer()
    //{
    //    for (int z = 0; z < 50; z++)
    //    {
    //        for (int x = 0; x < 50; x++)
    //        {
    //            yPos = Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;
    //            Instantiate(sampleTerrain, new Vector3(x * 1, (int)yPos, z * 1), Quaternion.identity);

    //        }
    //    }
    //}
}

public enum BlockType
{
    DIRT,
    GRASS,
    STONE,
    SAND,
    WATER,
    LOG,
    LEAVES,
    AIR
}
