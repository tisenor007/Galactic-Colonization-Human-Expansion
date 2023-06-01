using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Helped with world Gen: https://www.youtube.com/playlist?list=PLVsTSlfj0qsWEJ-5eMtXsYp03Y9yF1dEn
public static class Noise
{
    public static float Get2DPerlin (Vector2 positon, float offset, float scale)
    {
        return Mathf.PerlinNoise((positon.x + 0.1f) / VoxelData.chunkWidth * scale + offset, (positon.y + 0.1f) / VoxelData.chunkWidth * scale + offset);
    }


    //3D Perlin Noise https://www.youtube.com/watch?v=Aga0TBJkchM&ab_channel=Carlpilot
    public static bool Get3DPerlin(Vector3 position, float offset, float scale, float threshold)
    {
        float x = (position.x + offset + 0.1f) * scale;
        float y = (position.y + offset + 0.1f) * scale;
        float z = (position.z + offset + 0.1f) * scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        if ((AB + BC + AC + BA + CB + CA) / 6 > threshold)
        { return true; }
        else 
        { return false; }
    }
}
