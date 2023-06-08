using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Helped with world Gen: https://www.youtube.com/playlist?list=PLVsTSlfj0qsWEJ-5eMtXsYp03Y9yF1dEn
public class Chunk
{
    public ChunkCoord coord;

    public GameObject chunkObject;

    public Vector3 position;

    public VoxelState[,,] voxelMap = new VoxelState[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    List<Vector2> uvs = new List<Vector2>();

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<int> transparentTriangles = new List<int>();
    Material[] materials = new Material[2];
    List<Color> colors = new List<Color>();
    List<Vector3> normals = new List<Vector3>();

    World world;

    private bool _isActive;
    private bool isVoxelMapPopulated = false;

    public Chunk (ChunkCoord _coord, World _world)
    {
        coord = _coord;
        world = _world;
    }

    public void Init()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = world.material;
        materials[1] = world.transparentMaterial;
        meshRenderer.materials = materials;

        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkWidth, 0f, coord.z * VoxelData.chunkWidth);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.z;

        position = chunkObject.transform.position;

        PopulateVoxelMap();
    }

    public void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    voxelMap[x, y, z] = new VoxelState(world.GetVoxel(new Vector3(x, y, z) + position));
                }
            }
        }

        isVoxelMapPopulated = true;

        lock (world.chunkUpdateThreadLock)
        {
            world.chunksToUpdate.Add(this);
        }
    }

    public void UpdateChunk()
    {

        while (modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue();
            Vector3 pos = v.position -= position;
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z].id = v.id;
        }

        ClearMeshData();
        CalculateLight();

        for (int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    if (world.blockTypes[voxelMap[x, y, z].id].isSolid)
                    { UpdateMeshData(new Vector3(x, y, z)); }
                }
            }
        }

        world.chunksToDraw.Enqueue(this);
    }

    void CalculateLight()
    {
        Queue<Vector3Int> litVoxels = new Queue<Vector3Int>();

        for (int x = 0; x < VoxelData.chunkWidth; x++)
        {
            for (int z = 0; z < VoxelData.chunkWidth; z++) 
            {
                float lightRay = 1f;

                for (int y = VoxelData.chunkHeight - 1; y >= 0; y--)
                {
                    VoxelState thisVoxel = voxelMap[x, y, z];

                    if (thisVoxel.id != world.GetByteFromID(Item.ID.AIR) && world.blockTypes[thisVoxel.id].transparency < lightRay) 
                    { lightRay = world.blockTypes[thisVoxel.id].transparency; }

                    thisVoxel.globalLightPercent = lightRay;

                    voxelMap[x, y, z] = thisVoxel;

                    if (lightRay > VoxelData.lightFallOff) 
                    { litVoxels.Enqueue(new Vector3Int(x, y, z)); }
                }
            }
        }

        while (litVoxels.Count > 0)
        {
            Vector3Int v = litVoxels.Dequeue();
            for (int p = 0; p < 6; p++)
            {
                Vector3 currentVoxel = v + VoxelData.faceChecks[p];
                Vector3Int neighbor = new Vector3Int((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z);

                if (IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z))
                {
                    if (voxelMap[neighbor.x, neighbor.y, neighbor.z].globalLightPercent < voxelMap[v.x, v.y, v.z].globalLightPercent - VoxelData.lightFallOff)
                    {
                        voxelMap[neighbor.x, neighbor.y, neighbor.z].globalLightPercent = voxelMap[v.x, v.y, v.z].globalLightPercent - VoxelData.lightFallOff;

                        if (voxelMap[neighbor.x, neighbor.y, neighbor.z].globalLightPercent > VoxelData.lightFallOff)
                        {
                            litVoxels.Enqueue(neighbor);
                        }
                    }
                }
            }
        }
    }

    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
        colors.Clear();
        normals.Clear();
    }

    public bool isActive
    {
        get { return _isActive; }
        set {
            _isActive = value;
            if (chunkObject != null) { chunkObject.SetActive(value); }
        }
    }

    public bool isEditable
    {
        get 
        {
            if (!isVoxelMapPopulated) { return false; }
            else { return true; }
        }
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1)
        { return false; }
        else
        { return true; }
    }

    public void EditVoxel (Vector3 pos, byte newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[xCheck, yCheck, zCheck].id = newID;

        lock (world.chunkUpdateThreadLock)
        {

            world.chunksToUpdate.Insert(0, this);
            UpdateSurroundingVoxel(xCheck, yCheck, zCheck);

        }
    }

    void UpdateSurroundingVoxel(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int p = 0; p < 6; p++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];

            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.chunksToUpdate.Insert(0, world.GetChunkFromVector3(currentVoxel + position));
            }
        }
    }

    VoxelState CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x); //using FloorToInt istead of (int) to be safe
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

       if (!IsVoxelInChunk(x, y, z)) 
       { return world.GetVoxelState(pos + position); }

        return voxelMap[x, y, z];
    }
    
    public VoxelState GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(position.x);
        zCheck -= Mathf.FloorToInt(position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }

    void UpdateMeshData(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        byte blockID = voxelMap[x, y, z].id;
        //bool isTransparent = world.blockTypes[blockID].renderNeighbourFaces;

        for (int p = 0; p < 6; p++)
        {
            VoxelState neighbor = CheckVoxel(pos + VoxelData.faceChecks[p]);

            if (neighbor != null && world.blockTypes[neighbor.id].renderNeighbourFaces)
            {

                vertices.Add(pos + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 0]]);
                vertices.Add(pos + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 1]]);
                vertices.Add(pos + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 2]]);
                vertices.Add(pos + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 3]]);

                for (int i = 0; i < 4; i++)
                { normals.Add(VoxelData.faceChecks[p]); }

                AddTexture(world.blockTypes[blockID].GetTextureID(p));

                float lightLevel = neighbor.globalLightPercent;

              

                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));

                if (!world.blockTypes[neighbor.id].renderNeighbourFaces)
                {
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);
                }
                else
                {
                    transparentTriangles.Add(vertexIndex);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 3);
                }

                vertexIndex += 4;
                
            }
        }
    }
    
    public void CreateMesh()
    {
 
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();

        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);
        //mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();
        mesh.normals = normals.ToArray();
        //mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void AddTexture (int textureID)
    {
        float y = textureID / VoxelData.textureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.textureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        //similar pattern to voxeluvs
        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));

    }
}

public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.chunkWidth;
        z = zCheck / VoxelData.chunkWidth;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null) { return false; }
        else if (other.x == x && other.z == z) { return true; }
        else { return false; }
    }
}

public class VoxelState
{
    public byte id;
    public float globalLightPercent;

    public VoxelState()
    {
        id = 0;
        globalLightPercent = 0f;
    }

    public VoxelState (byte _id)
    {
        id = _id;
        globalLightPercent = 0f;
    }
}
