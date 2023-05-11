using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkData
{

    //CHUNK DATA
    public BlockType[] blocks;
    public int chunkSize = 16;
    public int chunkHeight = 100;
    public WorldGenerator worldReference;
    public Vector3Int worldPosition;

    public bool modifiedByThePlayer = false;

    public ChunkData(int chunkSize, int chunkHeight, WorldGenerator world, Vector3Int worldPosition)
    {
        this.chunkHeight = chunkHeight;
        this.chunkSize = chunkSize;
        this.worldReference = world;
        this.worldPosition = worldPosition;
        blocks = new BlockType[chunkSize * chunkHeight * chunkSize];
    }
}
