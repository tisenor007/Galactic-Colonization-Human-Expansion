using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item")]
public class Item : ScriptableObject
{
    public enum ID
    {
        AIR,
        DIRT,
        STONE,
        COBBLESTONE,
        BEDROCK,
        SAND,
        GLASS,
        GRASS,
        WATER,
        STEELPLATING,
        LEAVES,
        LOGS,
        PLANKS,
        SNOWYGRASS,
        BRICK,
        COALORE
    }

    public string itemName;
    public Sprite icon;
    public bool isStackable;
    public int stackLimit;

    public ID itemID;

    public GameObject physicalItem;
}
