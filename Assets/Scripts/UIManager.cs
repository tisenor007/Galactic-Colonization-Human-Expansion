using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject debugScreen;
    public static UIManager uiManager;
    public RectTransform highlight;
    public ItemSlot[] itemSlots;

    int slotIndex = 0;

    private float timer;
    private float frameRate;
    private World worldRef;
    private CharacterController playerRef;


    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;

    void Awake()
    {
        //singleton pattern
        if (uiManager == null)
        { DontDestroyOnLoad(this.gameObject); uiManager = this; }
        else if (uiManager != this)
        { Destroy(this.gameObject); }
    }

    void Start()
    {
        worldRef = GameManager.currentWorld;
        playerRef = GameManager.player;
        halfWorldSizeInChunks = VoxelData.worldSizeInChunks / 2;
        halfWorldSizeInVoxels = VoxelData.worldSizeInVoxels / 2;

        PopulateToolBarIcons();
    }

    void Update()
    {
        UpdateDebugScreen();
        UpdateToolBar();
    }

    private void PopulateToolBarIcons()
    {
        foreach (ItemSlot slot in itemSlots)
        {
            slot.icon.sprite = worldRef.blockTypes[slot.itemID].icon;
            slot.icon.enabled = true;
        }
    }

    private void UpdateToolBar()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            if (scroll > 0){ slotIndex--;}
            else { slotIndex++; }

            if (slotIndex > itemSlots.Length - 1) { slotIndex = 0; }
            if (slotIndex < 0) {slotIndex = (itemSlots.Length - 1); }

            highlight.position = itemSlots[slotIndex].icon.transform.position;
            playerRef.selectedBlockIndex = itemSlots[slotIndex].itemID;
        }
    }

    #region Debug Screen
    public void ToggleDebugScreen()
    {
        debugScreen.SetActive(!debugScreen.activeSelf);
    }

    private void UpdateDebugScreen()
    {
        string debugText = "Galactic Colonization: Human Expansion BETA v0.1";
        debugText += "\n";
        debugText += frameRate + "fps";
        debugText += "\n\n";
        debugText += "XYZ: " + (Mathf.FloorToInt(worldRef.player.transform.position.x) - halfWorldSizeInVoxels) + " / " +
            Mathf.FloorToInt(worldRef.player.transform.position.y) + " / " +
            (Mathf.FloorToInt(worldRef.player.transform.position.z) - halfWorldSizeInVoxels);
        debugText += "\n";
        //slight error without if as there is a short time where the playerChunkCoord is null
        if (worldRef.playerChunkCoord == null) { return; }
        debugText += "Chunk: " + (worldRef.playerChunkCoord.x - halfWorldSizeInChunks) + " / " + (worldRef.playerChunkCoord.z - halfWorldSizeInChunks);
        debugScreen.GetComponent<Text>().text = debugText;

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else { timer += Time.deltaTime; }

    }
    #endregion
}

[System.Serializable]
public class ItemSlot
{
    public byte itemID;
    public Image icon;
}
