using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static Queue<VoxelMod> GenerateMajorFlora(int index, Vector3 pos, int minTrunkHeight, int maxTrunkHeight)
    {
        switch (index)
        {
            case 0:
                return MakeTree(pos, minTrunkHeight, maxTrunkHeight);
            case 1:
                return MakeCacti(pos, minTrunkHeight, maxTrunkHeight);
        }

        return new Queue<VoxelMod>();
    }

    public static Queue<VoxelMod> MakeTree(Vector3 pos, int minTrunkHeight, int maxTrunkHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, 3f));

        if (height < minTrunkHeight) { height = minTrunkHeight; }

        for (int i = 1; i < height; i++) 
        { queue.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + i, pos.z), GameManager.currentWorld.GetByteFromID(Item.ID.LOGS))); }

        //magic numbers.. will change in the future
        for (int x = -3; x < 4; x++)
        {
            for (int y = 0; y < 7; y++) 
            {
                for (int z = -3; z < 4; z++)
                {
                    queue.Enqueue(new VoxelMod(new Vector3(pos.x + x, pos.y + height + y, pos.z + z), GameManager.currentWorld.GetByteFromID(Item.ID.LEAVES)));
                }
            }
        }

        return queue;
    }
    public static Queue<VoxelMod> MakeCacti(Vector3 pos, int minTrunkHeight, int maxTrunkHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 10000f, 2f));

        if (height < minTrunkHeight) { height = minTrunkHeight; }

        for (int i = 1; i <= height; i++)
        { queue.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + i, pos.z), GameManager.currentWorld.GetByteFromID(Item.ID.CACTUS))); }
       

        return queue;
    }
}
