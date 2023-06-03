using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    public enum InventoryType
    {
        SURVIVAL,
        CREATIVE,
        ADVENTURE,
    }

    public InventoryType currInventoryType;
    public ItemSlot[] toolBarSlots = new ItemSlot[9];
    public ItemSlot[] inventorySlots = new ItemSlot[36];
    public ItemSlot cursorItemSlot;

    [HideInInspector] public int slotIndex = 0;

    private World worldRef;
    private CharacterController playerRef;
    [SerializeField] private GraphicRaycaster m_Raycaster = null;
    private PointerEventData m_PointerEventData;
    [SerializeField] private EventSystem m_EventSystem = null;

    void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        worldRef = GameManager.currentWorld;
        playerRef = GameManager.player;

        if (currInventoryType == InventoryType.CREATIVE) { PopulateCreativeInventory(); }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateItemSlots();

        if (GameManager.gManager.currentGameState != GameManager.GameState.INVENTORY) { return; }

        cursorItemSlot.gameObject.transform.position = Input.mousePosition;

        //will move to input manager in the future
        if (Input.GetMouseButtonDown(0))
        {
            if (Input.GetKey(KeyCode.LeftShift)) { AutoAlignItem(CheckForSlot()); }
            else if (!Input.GetKey(KeyCode.LeftShift)) { ManageSlotLeftClick(CheckForSlot()); }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ManageSlotRightClick(CheckForSlot());
        }
    }

    public void AutoCollectItem( CharacterController player)
    {
        AutoFindSlot(worldRef.blockTypes[worldRef.GetChunkFromVector3(player.highlightBlock.position).GetVoxelFromGlobalVector3(player.highlightBlock.position)].
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

    private void PopulateCreativeInventory()
    {
        foreach (ItemSlot tSlot in toolBarSlots) { tSlot.ClearItemSlot(); }
        foreach (ItemSlot iSlot in inventorySlots) { iSlot.ClearItemSlot(); }

        for (int i = 0; i < ((toolBarSlots.Length - 1) + (worldRef.blockTypes.Length - 1)); i++)
        {
            if (i <= toolBarSlots.Length - 1 && i <= worldRef.blockTypes.Length - 1)
            {  toolBarSlots[i].AddItem(worldRef.blockTypes[(byte)i].presetBlockData, 1); }
            else if (i > toolBarSlots.Length - 1 && i <= worldRef.blockTypes.Length - 1) 
            { inventorySlots[i - toolBarSlots.Length].AddItem(worldRef.blockTypes[(byte)i].presetBlockData, 1); }
        }
    }

    #region Slot_Management

    public ItemSlot AutoFindSlot(Item itemFiller)
    {
        ItemSlot nextEmptySlot = FindEmptyItemSlot();
        ItemSlot nextCommonSlot = FindCommonSlot(itemFiller);

        for (int b = 0; b < toolBarSlots.Length; b++)
        {
            if (toolBarSlots[b].itemID == nextEmptySlot.itemID && nextCommonSlot == null)
            { return toolBarSlots[b]; }
            else if (nextCommonSlot != null && toolBarSlots[b].itemID == nextCommonSlot.itemID)
            { return toolBarSlots[b]; }
        }
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].itemID == nextEmptySlot.itemID && nextCommonSlot == null)
            { return inventorySlots[i]; }
            else if (nextCommonSlot != null && inventorySlots[i].itemID == nextCommonSlot.itemID)
            { return inventorySlots[i]; }
        }
        return null;
    }

    public void AutoAlignItem(ItemSlot clickedSlot)
    {
        ItemSlot nextEmptySlot = FindEmptyItemSlot();
        ItemSlot nextCommonSlot = FindCommonSlot(clickedSlot.storedItem);

        if (clickedSlot == null) { return; }

        if (cursorItemSlot.itemID == Item.ID.AIR && clickedSlot.itemID == Item.ID.AIR)
        { return; }


        else if (nextEmptySlot != null && clickedSlot == nextCommonSlot)
        { SwitchItemSlots(clickedSlot, nextEmptySlot); }

        else if (clickedSlot == nextCommonSlot) { return; }

        else if (nextCommonSlot != null)
        { CombineItemSlots(clickedSlot, nextCommonSlot); }
    }

    public void ManageSlotLeftClick(ItemSlot clickedSlot)
    {
        if (clickedSlot == null) { return; }

        if (cursorItemSlot.itemID == Item.ID.AIR && clickedSlot.itemID == Item.ID.AIR)
        { return; }

        if (clickedSlot.itemID == cursorItemSlot.itemID) { CombineItemSlots(cursorItemSlot, clickedSlot); }

        else { SwitchItemSlots(cursorItemSlot, clickedSlot); }
    }

    public void ManageSlotRightClick(ItemSlot clickedSlot)
    {
        if (clickedSlot == null) { return; }

        if (cursorItemSlot.itemID == Item.ID.AIR && clickedSlot.itemID == Item.ID.AIR)
        { return; }


        if (cursorItemSlot.itemID == Item.ID.AIR && clickedSlot.itemID != Item.ID.AIR)
        {
            cursorItemSlot.AddItem(clickedSlot.storedItem, clickedSlot.amount / 2); clickedSlot.RemoveItem(clickedSlot.amount / 2, false);

        }

        else if ((clickedSlot.itemID == Item.ID.AIR && cursorItemSlot.itemID != Item.ID.AIR) || clickedSlot.itemID == cursorItemSlot.itemID)
        {
            clickedSlot.AddItem(cursorItemSlot.storedItem, 1); cursorItemSlot.RemoveItem(1, false);
        }
    }


    private ItemSlot FindCommonSlot(Item itemFiller)
    {
        for (int b = 0; b < toolBarSlots.Length; b++)
        {
            if (toolBarSlots[b].itemID == itemFiller.itemID && toolBarSlots[b].amount < itemFiller.stackLimit)
            { return toolBarSlots[b]; }
        }
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].itemID == itemFiller.itemID && inventorySlots[i].amount < itemFiller.stackLimit)
            { return inventorySlots[i]; }
        }
        return null;
    }

    private ItemSlot FindEmptyItemSlot()
    {
        for (int b = 0; b < toolBarSlots.Length; b++)
        {
            if (toolBarSlots[b].itemID == Item.ID.AIR)
            { return toolBarSlots[b]; }
        }
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].itemID == Item.ID.AIR)
            { return inventorySlots[i]; }
        }
        return null;
    }

    private void SwitchItemSlots(ItemSlot replacer, ItemSlot replaced)
    {
        int amountHolder;
        Item itemHolder;
        amountHolder = replaced.amount;
        itemHolder = replaced.storedItem;
        replaced.AddItem(replacer.storedItem, replacer.amount);
        replacer.AddItem(itemHolder, amountHolder);
    }

    private void CombineItemSlots(ItemSlot adder, ItemSlot added)
    {
        int amountHolder;

        if (adder.amount > added.storedItem.stackLimit) { SwitchItemSlots(adder, added); return; }

        if (added.amount + adder.amount == added.storedItem.stackLimit)
        { added.AddItem(adder.storedItem, adder.amount); adder.ClearItemSlot(); }
        else if (added.amount + adder.amount > added.storedItem.stackLimit)
        {
            amountHolder = adder.amount;
            adder.amount = ((added.amount + adder.amount) - added.storedItem.stackLimit);
            added.AddItem(adder.storedItem, amountHolder);
        }
        else { added.AddItem(adder.storedItem, adder.amount); adder.RemoveItem(adder.amount, false); }

    }

    private ItemSlot CheckForSlot()
    {
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<ItemSlot>() && result.gameObject != cursorItemSlot.gameObject)
            { return result.gameObject.GetComponent<ItemSlot>(); }
        }

        return null;
    }

    private void UpdateItemSlots()
    {
        foreach (ItemSlot slot in toolBarSlots){slot.UpdateItemSlot();}
        foreach (ItemSlot iSlot in inventorySlots) { iSlot.UpdateItemSlot(); }
        cursorItemSlot.UpdateItemSlot();
    }

    #endregion

}
