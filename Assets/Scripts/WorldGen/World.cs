using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

//Helped with world Gen: https://www.youtube.com/playlist?list=PLVsTSlfj0qsWEJ-5eMtXsYp03Y9yF1dEn

public class World : MonoBehaviour
{
    public int seed;
    public BiomeAttributes[] biomes;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public Material transparentMaterial;

    public float gravity = -9.8f;

    [Range(0f, 1f)]
    public float globalLightLevel;
    public Color day;
    public Color night;

    public BlockType[] blockTypes;

    Chunk[,] chunks = new Chunk[VoxelData.worldSizeInChunks, VoxelData.worldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    public List<Chunk> chunksToUpdate = new List<Chunk>();
    bool applyingModifications = false;

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    Thread chunkUpdateThread;
    public object chunkUpdateThreadLock = new object();

    //private void Awake()
    //{
    //    foreach (BlockType blockType in blockTypes){ blockType.SetPresetVariables(); }
    //}

    private void Start()
    {
        //string jsonExport = JsonUtility.ToJson(GameManager.gManager.settings);
        //File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);

        string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
        GameManager.gManager.settings = JsonUtility.FromJson<Settings>(jsonImport);

        Random.InitState(seed);

        Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightLevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);

        if (GameManager.gManager.settings.enableThreading)
        {
            chunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            chunkUpdateThread.Start();
        }

        SetGlobalLightValue();
        foreach (BlockType blockType in blockTypes) { blockType.SetPresetVariables(); }
        spawnPosition = new Vector3((VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f, VoxelData.chunkHeight - 50f, (VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    public void SetGlobalLightValue()
    {
        Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
        Camera.main.backgroundColor = Color.Lerp(night, day, globalLightLevel);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {CheckViewDistance();}
        
        if (chunksToCreate.Count > 0) 
        { CreateChunk(); }

        if (chunksToDraw.Count > 0)
        {
          
            if (chunksToDraw.Peek().isEditable) { chunksToDraw.Dequeue().CreateMesh(); }

        }

        if (!GameManager.gManager.settings.enableThreading)
        {
            if (!applyingModifications)
            { ApplyModifactions(); }

            if (chunksToUpdate.Count > 0)
            { UpdateChunks(); }
        }
        
    }

    void GenerateWorld()
    {
        for (int x = (VoxelData.worldSizeInChunks / 2) - GameManager.gManager.settings.viewDistance; x < (VoxelData.worldSizeInChunks / 2) + GameManager.gManager.settings.viewDistance; x++)
        {
            for (int z = (VoxelData.worldSizeInChunks / 2) - GameManager.gManager.settings.viewDistance; z < (VoxelData.worldSizeInChunks / 2) + GameManager.gManager.settings.viewDistance; z++)
            {
                ChunkCoord newChunk = new ChunkCoord(x, z);
                chunks[x, z] = new Chunk(newChunk, this);
                chunksToCreate.Add(newChunk);
            }
        }

        player.position = spawnPosition;
        CheckViewDistance();
    }

    void CreateChunk()
    {
        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        chunks[c.x, c.z].Init();
    }

    void UpdateChunks()
    {
        bool updated = false;
        int index = 0;

        lock (chunkUpdateThreadLock)
        {

            while (!updated && index < chunksToUpdate.Count - 1)
            {
                if (chunksToUpdate[index].isEditable)
                {
                    chunksToUpdate[index].UpdateChunk();
                    if (!activeChunks.Contains(chunksToUpdate[index].coord))
                    { activeChunks.Add(chunksToUpdate[index].coord);}
                    chunksToUpdate.RemoveAt(index);
                    updated = true;
                }
                else
                {
                    index++;
                }
            }
        }
    }

    void ThreadedUpdate()
    {
        while (true)
        {
            if (!applyingModifications)
            { ApplyModifactions(); }

            if (chunksToUpdate.Count > 0)
            { UpdateChunks(); }
        }
    }

    private void OnDisable()
    {
        if (GameManager.gManager.settings.enableThreading)
        {
            chunkUpdateThread.Abort();
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
                    chunks[c.x, c.z] = new Chunk(c, this);
                    chunksToCreate.Add(c);
                }

                chunks[c.x, c.z].modifications.Enqueue(v);
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

        activeChunks.Clear();

        for (int x = coord.x - GameManager.gManager.settings.viewDistance; x < coord.x + GameManager.gManager.settings.viewDistance; x++)
        {
            for (int z = coord.z - GameManager.gManager.settings.viewDistance; z < coord.z + GameManager.gManager.settings.viewDistance; z++)
            {

                ChunkCoord thisChunkCoord = new ChunkCoord(x, z);

                if (isChunkInWorld(thisChunkCoord))
                {
                    if (chunks[x, z] == null) 
                    { 
                        chunks[x, z] = new Chunk(thisChunkCoord, this); 
                        chunksToCreate.Add(thisChunkCoord); 
                    }
                    else if (!chunks[x, z].isActive) { chunks[x,z].isActive = true; }
                    activeChunks.Add(thisChunkCoord);
                }

                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(thisChunkCoord))
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
        { return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos).id].isSolid; }

        return blockTypes[GetVoxel(pos)].isSolid;

    }

    public VoxelState GetVoxelState(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!isChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.chunkHeight) { return null; }
        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
        { return chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos); }

        return new VoxelState(GetVoxel(pos));

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

        /* BIOME SELECTION PASS */
        int solidGroundHeight = 42;
        float sumOfHeights = 0f;
        int count = 0;
        float strongestWeight = 0f;
        int strongestBiomeIndex = 0;

        for (int i = 0; i < biomes.Length; i++)
        {
            float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);

            //keep track of which weight is strongest.
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }

            //Get height of terrain (for the current biome) and multiply it by its weight.
            float height = biomes[i].terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biomes[i].terrainScale) * weight;

            //if the height value is greater than 0 add it to the sum of heights
            if (height > 0)
            {
                sumOfHeights += height;
                count++;
            }
        }

        //set biome to the one with the strongest weight.
        BiomeAttributes biome = biomes[strongestBiomeIndex];

        //get the average of the heights
        sumOfHeights /= count;

        int terrainHeight = Mathf.FloorToInt(sumOfHeights + solidGroundHeight);

    //BiomeAttributes biome = biomes[index];

    /* BASIC TERRAIN PASS */
    
        if (yPos == terrainHeight) {
            voxelValue = GetByteFromID(biome.surfaceBlock);
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4) {
            voxelValue = GetByteFromID(biome.subSurfaceBlock);
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

        if (yPos == terrainHeight && biome.placeMajorFlora)
        {
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
            {
                //voxelValue = GetByteFromID(Item.ID.GRASS);
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
                {

                    //voxelValue = GetByteFromID(Item.ID.DIRT);
                    modifications.Enqueue(Structure.GenerateMajorFlora(biome.majorFloraIndex, pos, biome.maxHeight, biome.maxHeight));
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
    [HideInInspector] public bool renderNeighbourFaces;
    [HideInInspector] public float transparency;
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
        transparency = presetBlockData.transparency;
        renderNeighbourFaces = presetBlockData.renderNeighborFaces;
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
