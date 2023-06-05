using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    //public GameObject slotObjectHolder;
    public Item.ID itemID;
    public int amount;
    public Image icon;
    [HideInInspector] public Item storedItem;
    public Text amountText;

    public void UpdateItemSlot()
    {
        //if (storedItem == null) { ClearItemSlot(); return; }
        if (storedItem != GameManager.currentWorld.GetBlockTypeFromID(itemID).presetBlockData)
        { storedItem = GameManager.currentWorld.GetBlockTypeFromID(itemID).presetBlockData; }
        //if (itemID != storedItem.itemID) { itemID = storedItem.itemID; }
        if (icon.sprite != storedItem.icon) { icon.sprite = storedItem.icon; }
        if (storedItem.itemID != Item.ID.AIR) { icon.enabled = true; }
        else if (storedItem.itemID == Item.ID.AIR) { icon.enabled = false; }
        if (amount <= 1) { amountText.gameObject.SetActive(false); }
        else if (amount > 1) { amountText.gameObject.SetActive(true); }

        amountText.text = amount.ToString();
        if (amount > storedItem.stackLimit) { amount = storedItem.stackLimit; }
        if (amount <= 0) { ClearItemSlot(); }
    }

    public void UseItem(World thisWorld, Player player)
    {
        if (storedItem is BlockData)
        {
            if (amount > 0)
            {
                thisWorld.PlaceVoxel(player.placeBlock, player.selectedBlockIndex);

                if (player.currentGameMode == Player.GameMode.SURVIVAL) { RemoveItem(1); }
                else if (player.currentGameMode == Player.GameMode.CREATIVE) { /*NOTHING*/ }
            }
        }
        else if (!(storedItem is BlockData))
        {

        }
    }

    #region ItemSlot_Setting
    public void AddItem(Item itemToPickUp, int amnt)
    {
        if (itemID != itemToPickUp.itemID) { ClearItemSlot(); ; }
        //storedItem = itemToPickUp;
        itemID = itemToPickUp.itemID;
        amount = amount + amnt;
    }

    public void RemoveItem(int amnt)
    {
        amount = amount - amnt;
    }

    public void DropItem(int amnt,  ref Vector3 dropPos)
    {
        if (itemID == Item.ID.AIR) { return; }
        RemoveItem(amnt);
        GameObject objectToSpawn;
        objectToSpawn = GameObject.Instantiate(storedItem.physicalItem, dropPos, Quaternion.identity);
        objectToSpawn.GetComponent<ItemObject>().itemObjectID = itemID;
        objectToSpawn.GetComponent<ItemObject>().itemAmount = amnt;
       
    }

    public void ClearItemSlot()
    {
        amount = 0;
        //default block...
        itemID = Item.ID.AIR;
    }
    #endregion
}
