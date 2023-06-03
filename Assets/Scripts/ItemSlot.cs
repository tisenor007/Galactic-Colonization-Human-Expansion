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

    public void UseItem(World thisWorld, CharacterController player)
    {
        if (storedItem is BlockData)
        {
            if (amount > 0)
            {
                thisWorld.PlaceVoxel(player.placeBlock, player.selectedBlockIndex);

                if (player.inventory.currInventoryType == Inventory.InventoryType.SURVIVAL) { RemoveItem(1, false); }
                else if (player.inventory.currInventoryType == Inventory.InventoryType.CREATIVE) { /*NOTHING*/ }
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

    public void RemoveItem(int amnt, bool dropItem)
    {
        amount = amount - amnt;
        if (dropItem)
        {
            //dropitem();
        }
    }

    public void ClearItemSlot()
    {
        amount = 0;
        //default block...
        itemID = Item.ID.AIR;
    }
    #endregion
}
