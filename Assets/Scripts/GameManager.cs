using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        GAMEPLAY,
        INVENTORY,
        PAUSE,
        MAIN_MENU
    }
    public static GameManager gManager;
    public UIManager uiManagerRef;
    public static World currentWorld;
    public static Player player;
    public InputManager inputManager;
    public Settings settings;
    public GameState currentGameState;


    public void Awake()
    {
        //singleton pattern
        if (gManager == null)
        { DontDestroyOnLoad(this.gameObject); gManager = this; }
        else if (gManager != this)
        { Destroy(this.gameObject); }

        //uiManagerRef = UIManager.uiManager;
        currentWorld = GameObject.Find("World").GetComponent<World>();
        //starts on gameplay for testing purposes
        currentGameState = GameState.GAMEPLAY;
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    // Start is called before the first frame update
    public void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { Application.Quit(); }
        //Debug.Log(currentGameState);
        switch (currentGameState)
        {
            case GameState.GAMEPLAY:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                inputManager.UpdateMoveInput();
                inputManager.UpdateEditVoxelInput();
                inputManager.UpdatePickUpInput();
                inputManager.UpdateToggleDebugScreenInput();
                inputManager.UpdateToggleCamModeInput();
                inputManager.UpdateToggleInventoryInput();
                player.UpdatePlayerCam();

                break;
            case GameState.INVENTORY:
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                inputManager.UpdateToggleInventoryInput();
                inputManager.UpdateOpenInventoryInput();
                inputManager.UpdateToggleDebugScreenInput();
                break;
        }
    }
}

[System.Serializable]
public class Settings
{
    public int viewDistance;
    public bool enableThreading;

    public string version;
}
