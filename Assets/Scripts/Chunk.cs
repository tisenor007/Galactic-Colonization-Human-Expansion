using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Chunk : MonoBehaviour
{
    List<Vector2> uvs = new List<Vector2>();

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    byte[,,] voxelMap = new byte[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

    World world;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
    }

    // Start is called before the first frame update
    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>(); //not efficient, will change later...

        PopulateVoxelMap();
        CreateMeshData();        
        CreateMesh();
    }

    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    //temp block regulation
                    if (y < 1) { voxelMap[x, y, z] = 1; }
                    else if ( y == VoxelData.chunkHeight - 1) { voxelMap[x, y, z] = 2; }
                    else{ voxelMap[x, y, z] = 0; }
                }
            }
        }
    }

    void CreateMeshData()
    {
        for (int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {

                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x); //using FloorToInt istead of (int) to be safe
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1) 
        { return false; }

        return world.blockTypes[voxelMap[x, y, z]].isSolid;
    }   

    void AddVoxelDataToChunk(Vector3 position)
    {
        for (int p = 0; p < 6; p++)
        {
            if (!CheckVoxel(position + VoxelData.faceChecks[p]))
            {
                byte blockID = voxelMap[(int)position.x, (int)position.y, (int)position.z];

                vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 0]]);
                vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 1]]);
                vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 2]]);
                vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 3]]);

                AddTexture(world.blockTypes[blockID].GetTextureID(p));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;
            }
        }
    }
    
    void CreateMesh()
    {
 
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

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
