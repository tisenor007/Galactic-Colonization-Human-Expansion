using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public ItemSlot[] toolBarSlots = new ItemSlot[9];
    public ItemSlot[] inventorySlots = new ItemSlot[36];

    [HideInInspector] public int slotIndex = 0;

    private World worldRef;
    private CharacterController playerRef;
    void Awake()
    {
        SetUpItemSlots();
    }
    // Start is called before the first frame update
    void Start()
    {
        worldRef = GameManager.currentWorld;
        playerRef = GameManager.player;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateItemSlots();
    }

    public ItemSlot FindAvailableSlot(Item itemFiller)
    {
        for (int b = 0; b < toolBarSlots.Length; b++)
        {
            if ((toolBarSlots[b].itemID == itemFiller.itemID && toolBarSlots[b].amount < itemFiller.stackLimit) || toolBarSlots[b].itemID == Item.ID.AIR)
            { return toolBarSlots[b]; }
        }
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if ((inventorySlots[i].itemID == itemFiller.itemID && inventorySlots[i].amount < itemFiller.stackLimit) || inventorySlots[i].itemID == Item.ID.AIR)
            { return inventorySlots[i]; }
        }
        return null;
    }

    public void CollectItem( CharacterController player)
    {
        FindAvailableSlot(worldRef.blockTypes[worldRef.GetChunkFromVector3(player.highlightBlock.position).GetVoxelFromGlobalVector3(player.highlightBlock.position)].
        presetBlockData).AddItem(worldRef.blockTypes[worldRef.GetChunkFromVector3(player.highlightBlock.position).GetVoxelFromGlobalVector3(player.highlightBlock.position)].
        presetBlockData, 1);
        worldRef.DestroyVoxel(player.highlightBlock);
    }


    public void UpdateToolBarUI(RectTransform HL)
    {
        if (HL == null) { return; }
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        //update highlight for toolbar...
        if (scroll != 0)
        {
            if (scroll > 0) { slotIndex--; }
            else { slotIndex++; }

            if (slotIndex > toolBarSlots.Length - 1) { slotIndex = 0; }
            if (slotIndex < 0) { slotIndex = (toolBarSlots.Length - 1); }

            HL.position = toolBarSlots[slotIndex].icon.transform.position;
            //playerRef.selectedBlockIndex = worldRef.GetID(itemSlots[slotIndex].itemID);
        }

        if (playerRef.selectedBlockIndex != worldRef.GetByteFromID(toolBarSlots[slotIndex].itemID)) 
        { playerRef.selectedBlockIndex = worldRef.GetByteFromID(toolBarSlots[slotIndex].itemID); }
    }

    private void SetUpItemSlots()
    {
        foreach (ItemSlot slot in toolBarSlots){slot.SetUpSlot();}
        foreach (ItemSlot iSlot in inventorySlots) { iSlot.SetUpSlot(); }
    }

    private void UpdateItemSlots()
    {
        foreach (ItemSlot slot in toolBarSlots){slot.UpdateItemSlot();}
        foreach (ItemSlot iSlot in inventorySlots) { iSlot.UpdateItemSlot(); }
    }

}

[System.Serializable]
public class ItemSlot
{
    public GameObject slotObjectHolder;
    public Item.ID itemID;
    public int amount;
    [HideInInspector] public Image icon;
    [HideInInspector]public Item storedItem;
    private Text amountText;

    public void SetUpSlot()
    {
        icon = slotObjectHolder.transform.GetChild(0).gameObject.GetComponent<Image>();
        amountText = slotObjectHolder.transform.GetChild(1).gameObject.GetComponent<Text>();
    }

    public void UpdateItemSlot()
    {
        //if (storedItem == null) { ClearItemSlot(); return; }
        if (storedItem != GameManager.currentWorld.GetBlockTypeFromID(itemID).presetBlockData)
        { storedItem = GameManager.currentWorld.GetBlockTypeFromID(itemID).presetBlockData; } 
        //if (itemID != storedItem.itemID) { itemID = storedItem.itemID; }
        if (icon.sprite != storedItem.icon) { icon.sprite = storedItem.icon; }
        if (storedItem.icon != null) { icon.enabled = true; }
        else if (storedItem.icon == null) { icon.enabled = false; }
        if (amount <= 1) { amountText.gameObject.SetActive(false); }
        else if (amount > 1) { amountText.gameObject.SetActive(true); }

        amountText.text = amount.ToString();
        if (amount > storedItem.stackLimit) { amount = storedItem.stackLimit; }
        if (amount <= 0) { ClearItemSlot(); }
    }

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
        if (dropItem) {
            //dropitem();
        }
    }

    public void UseItem(World thisWorld, CharacterController player)
    {
        if (storedItem is BlockData)
        {
            if (amount > 0) 
            {
                thisWorld.PlaceVoxel(player.placeBlock, player.selectedBlockIndex);
                RemoveItem(1, false);
            }
        }
        else if (!(storedItem is BlockData))
        {

        }
    }

    public void ClearItemSlot()
    {
        amount = 0;
        //default block...
        itemID = Item.ID.AIR;
    }
}
