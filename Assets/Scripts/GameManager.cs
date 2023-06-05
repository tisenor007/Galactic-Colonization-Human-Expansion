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
    public static UIManager uiManagerRef;
    public static World currentWorld;
    public static Player player;
    public static InputManager inputManager;
    public GameState currentGameState;


    public void Awake()
    {
        //singleton pattern
        if (gManager == null)
        { DontDestroyOnLoad(this.gameObject); gManager = this; }
        else if (gManager != this)
        { Destroy(this.gameObject); }

        uiManagerRef = UIManager.uiManager;
        currentWorld = GameObject.Find("World").GetComponent<World>();
        //starts on gameplay for testing purposes
        currentGameState = GameState.GAMEPLAY;
        player = GameObject.Find("Player").GetComponent<Player>();
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
    }

    // Start is called before the first frame update
    public void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
        //Debug.Log(currentGameState);
        switch (currentGameState)
        {
            case GameState.GAMEPLAY:
                Cursor.lockState = CursorLockMode.Locked;
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
                inputManager.UpdateToggleInventoryInput();
                inputManager.UpdateOpenInventoryInput();
                inputManager.UpdateToggleDebugScreenInput();
                break;
        }
    }
}
