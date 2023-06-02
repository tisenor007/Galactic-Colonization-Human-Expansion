using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Helped with world Gen: https://www.youtube.com/playlist?list=PLVsTSlfj0qsWEJ-5eMtXsYp03Y9yF1dEn

public class World : MonoBehaviour
{
    public int seed;
    public BiomeAttributes biome;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public Material transparentMaterial;

    public BlockType[] blockTypes;

    public float gravity = -9.8f;

    Chunk[,] chunks = new Chunk[VoxelData.worldSizeInChunks, VoxelData.worldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    List<Chunk> chunksToUpdate = new List<Chunk>();
    bool applyingModifications = false;

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    private void Awake()
    {
        foreach (BlockType blockType in blockTypes){ blockType.SetPresetVariables(); }
    }

    private void Start()
    {
        Random.InitState(seed);

        spawnPosition = new Vector3((VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f, VoxelData.chunkHeight - 50f, (VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {CheckViewDistance();}
        
        if (!applyingModifications) 
        { ApplyModifactions(); }

        if (chunksToCreate.Count > 0) 
        { CreateChunk(); }

        if (chunksToUpdate.Count > 0) 
        { UpdateChunks(); }

        if (chunksToDraw.Count > 0)
        {
            lock (chunksToDraw)
            {
                if (chunksToDraw.Peek().isEditable) { chunksToDraw.Dequeue().CreateMesh(); }
            }
        }
    }

    void GenerateWorld()
    {
        for (int x = (VoxelData.worldSizeInChunks / 2) - VoxelData.viewDistanceInChunks; x < (VoxelData.worldSizeInChunks / 2) + VoxelData.viewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.worldSizeInChunks / 2) - VoxelData.viewDistanceInChunks; z < (VoxelData.worldSizeInChunks / 2) + VoxelData.viewDistanceInChunks; z++)
            {
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x, z));
            }
        }

        player.position = spawnPosition;
    }

    void CreateChunk()
    {
        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        activeChunks.Add(c);
        chunks[c.x, c.z].Init();
    }

    void UpdateChunks()
    {
        bool updated = false;
        int index = 0;

        while (!updated && index < chunksToUpdate.Count - 1)
        {
            if (chunksToUpdate[index].isEditable)
            {
                chunksToUpdate[index].UpdateChunk();
                chunksToUpdate.RemoveAt(index);
                updated = true;
            }
            else
            {
                index++;
            }
        }
    }

    void ApplyModifactions()
    {
        applyingModifications = true;

        while (modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();
            //new line to solve error
            //Queue<VoxelMod> queue = new Queue<VoxelMod>();
            //queue = modifications.Dequeue();

            while (queue.Count > 0)
            {

                VoxelMod v = queue.Dequeue();

                ChunkCoord c = GetChunkCoordFromVector3(v.position);

                if (chunks[c.x, c.z] == null)
                {
                    chunks[c.x, c.z] = new Chunk(c, this, true);
                    activeChunks.Add(c);
                }

                chunks[c.x, c.z].modifications.Enqueue(v);

                if (!chunksToUpdate.Contains(chunks[c.x, c.z]))
                { chunksToUpdate.Add(chunks[c.x, c.z]); }
            }
        }

        applyingModifications = false;
    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);

        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);

        return chunks[x, z];
    }

    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int x = coord.x - VoxelData.viewDistanceInChunks; x < coord.x + VoxelData.viewDistanceInChunks; x++)
        {
            for (int z = coord.z - VoxelData.viewDistanceInChunks; z < coord.z + VoxelData.viewDistanceInChunks; z++)
            {
                if (isChunkInWorld(new ChunkCoord(x, z)))
                {
                    if (chunks[x, z] == null) 
                    { 
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false); 
                        chunksToCreate.Add(new ChunkCoord(x, z)); 
                    }
                    else if (!chunks[x, z].isActive) { chunks[x,z].isActive = true; }
                    activeChunks.Add(new ChunkCoord(x, z));
                }

                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                    {previouslyActiveChunks.RemoveAt(i);}
                }
            }
        }

        foreach (ChunkCoord c in previouslyActiveChunks)
        {
            chunks[c.x, c.z].isActive = false;
        }
    }

    public bool CheckForVoxel (Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!isChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.chunkHeight) { return false; }
        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable) 
        { return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isSolid; }

        return blockTypes[GetVoxel(pos)].isSolid;

    }

    public bool CheckIfVoxelTransparent(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!isChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.chunkHeight) { return false; }
        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
        { return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isTransparent; }

        return blockTypes[GetVoxel(pos)].isTransparent;

    }

    public byte GetByteFromID(Item.ID id)
    {
        for (int i = 0; i <= blockTypes.Length - 1; i++)
        {
            if (blockTypes[i].presetBlockData.itemID == id)
            { return (byte)i; }
        }

        return 0;
    }

    public BlockType GetBlockTypeFromID(Item.ID id)
    {
        return blockTypes[GameManager.currentWorld.GetByteFromID(id)];
    }

    public void DestroyVoxel(Transform highlightBlock)
    {
        GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, GetByteFromID(Item.ID.AIR));
    }

    public void PlaceVoxel(Transform placeBlock, byte selectedBlock)
    {
        GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, selectedBlock);
    }

    public byte GetVoxel (Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        byte voxelValue = 0;
        /* IMMUTABLE PASS */
        
        //if outside world, return air.
        if (!isVoxelInWorld(pos)) 
        {
            return GetByteFromID(Item.ID.AIR);
        }

        //if bottom block of chunk, return bedrock
        if (yPos == 0) 
        {
            return GetByteFromID(Item.ID.BEDROCK);
        }

        /* BASIC TERRAIN PASS */
        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        if (yPos == terrainHeight) {
            voxelValue = GetByteFromID(Item.ID.GRASS);
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4) {
            voxelValue = GetByteFromID(Item.ID.DIRT);
        }
        else if (yPos > terrainHeight) {
            voxelValue = GetByteFromID(Item.ID.AIR);
        }
        else {
            voxelValue = GetByteFromID(Item.ID.STONE);
        }

        /* SECOND PASS */

        if (voxelValue == GetByteFromID(Item.ID.STONE))
        {
           
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = GetByteFromID(lode.blockID);
                    }
                }
            }
            
        }

        /* TREE PASS */

        if (yPos == terrainHeight)
        {
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.treeZoneScale) > biome.treeZoneThreshold)
            {
                voxelValue = GetByteFromID(Item.ID.GRASS);
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.treePlacementScale) > biome.treePlacementThreshold)
                {

                    voxelValue = GetByteFromID(Item.ID.DIRT);
                    modifications.Enqueue(Structure.MakeTree(pos, biome.maxTreeHeight, biome.maxTreeHeight));
                }
            }
        }
        
        return voxelValue; 
    }

    bool isChunkInWorld(ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.worldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.worldSizeInChunks - 1) 
        { return true; }
        else 
        { return false; }
    }

    bool isVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.worldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.chunkHeight && pos.z >= 0 && pos.z < VoxelData.worldSizeInVoxels)
        { return true; }
        else 
        { return false; }
    }
}

[System.Serializable]
public class BlockType
{
    public BlockData presetBlockData;

    [HideInInspector] public bool isSolid;
    [HideInInspector] public bool isTransparent;
    [HideInInspector] public Sprite icon;
    private string blockName;

    [Header("Texture Values")]
    private int backFaceTexture;
    private int frontFaceTexture;
    private int topFaceTexture;
    private int bottomFaceTexture;
    private int leftFaceTexture;
    private int rightFaceTexture;
    //Back, Front, Top, Bottom, Left, Right

    public void SetPresetVariables()
    {
        blockName = presetBlockData.itemName;
        isSolid = presetBlockData.isSolid;
        isTransparent = presetBlockData.isTransparent;
        icon = presetBlockData.icon;

        backFaceTexture = presetBlockData.backFaceTexture;
        frontFaceTexture = presetBlockData.frontFaceTexture;
        topFaceTexture = presetBlockData.topFaceTexture;
        bottomFaceTexture = presetBlockData.bottomFaceTexture;
        leftFaceTexture = presetBlockData.leftFaceTexture;
        rightFaceTexture = presetBlockData.rightFaceTexture;
    }

    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextrueID; invalid face index");
                return 0;
        }
    }
}

public class VoxelMod
{
    public Vector3 position;
    public byte id;

    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }

    public VoxelMod(Vector3 _position, byte _id)
    {
        position = _position;
        id = _id;
    }
}
