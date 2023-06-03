using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject debugScreen;
    public GameObject inventoryUI;
    public static UIManager uiManager;
    public RectTransform highlight;

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
    }

    void Update()
    {
        //if (worldRef == null) { worldRef = GameManager.currentWorld; return; }
        UpdateDebugScreen();

        playerRef.inventory.UpdateToolBarUI(this.highlight);
    }

    public void ToggleInventory()
    {
        switch (GameManager.gManager.currentGameState)
        {
            case GameManager.GameState.GAMEPLAY:
                GameManager.gManager.currentGameState = GameManager.GameState.INVENTORY;
                break;
            case GameManager.GameState.INVENTORY:
                GameManager.gManager.currentGameState = GameManager.GameState.GAMEPLAY;
                break;
        }
        inventoryUI.SetActive(!inventoryUI.activeSelf);
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
