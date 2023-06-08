using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Helped with world Gen: https://www.youtube.com/playlist?list=PLVsTSlfj0qsWEJ-5eMtXsYp03Y9yF1dEn
public static class VoxelData
{
    public static readonly int chunkWidth = 16;
    public static readonly int chunkHeight = 128;
    public static readonly int worldSizeInChunks = 100;

    //lighting values
    public static float minLightLevel = 0.15f;
    public static float maxLightLevel = 0.8f;
    public static float lightFallOff = 0.08f;

    public static int worldSizeInVoxels
    {
        get { return worldSizeInChunks * chunkWidth; }
    }

    public static readonly int textureAtlasSizeInBlocks = 5;
    public static float NormalizedBlockTextureSize
    {
        get { return 1f / ((float)textureAtlasSizeInBlocks); }
    }

    public static readonly Vector3[] voxelVertices = new Vector3[8] // 8 vertices in a cube
    {
        new Vector3(0.0f,0.0f,0.0f),
        new Vector3(1.0f,0.0f,0.0f),
        new Vector3(1.0f,1.0f,0.0f),
        new Vector3(0.0f,1.0f,0.0f),
        new Vector3(0.0f,0.0f,1.0f),
        new Vector3(1.0f,0.0f,1.0f),
        new Vector3(1.0f,1.0f,1.0f),
        new Vector3(0.0f,1.0f,1.0f)
    };

    public static readonly Vector3[] faceChecks = new Vector3[6]
    {
        new Vector3(0.0f, 0.0f, -1.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, -1.0f, 0.0f),
        new Vector3(-1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f)
    };

    public static readonly int[,] voxelTriangles = new int[6, 4] //6 triangles each face
    {
        //Back, Front, Top, Bottom, Left, Right

        // 0 1 2 2 1 3
        { 0, 3, 1, 2 }, //back face
        { 5, 6, 4, 7 }, //front face
        { 3, 7, 2, 6 }, // top face
        { 1, 5, 0, 4 }, //bottom face
        { 4, 7, 0, 3 }, //left face
        { 1, 2, 5, 6 } //right face
    };

    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {
        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 1.0f)
    };
}
