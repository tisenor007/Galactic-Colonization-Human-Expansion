using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    //input
    //public static InputManager iManager;
    private KeyCode forwardInput = KeyCode.W;
    private KeyCode backwardInput = KeyCode.S;
    private KeyCode leftInput = KeyCode.A;
    private KeyCode rightInput = KeyCode.D;
    private KeyCode sprintInput = KeyCode.LeftShift;
    private KeyCode jumpInput = KeyCode.Space;
    private KeyCode toggleFPInput = KeyCode.F5;
    private KeyCode toggleDebugScreenInput = KeyCode.F3;
    private KeyCode dropItemInput = KeyCode.Q;
    private KeyCode viewInventoryInput = KeyCode.I;
    private int attackInput = 0;
    private int blockInput = 1;
    private KeyCode pickUpInventory = KeyCode.E;
    private Player player;

    public void Awake()
    {
        //if (iManager == null)
        //{ DontDestroyOnLoad(this.gameObject); iManager = this; }
        //else if (iManager != this)
        //{ Destroy(this.gameObject); }
    }
    // Start is called before the first frame update
    public void Start()
    {
        player = GameManager.player;
    }

    // Update is called once per frame
    public void Update()
    {

    }

    public bool PlayerIsMoving()
    {
        if (Input.GetKey(forwardInput)) { return true; }
        if (Input.GetKey(backwardInput)) { return true; }
        if (Input.GetKey(leftInput)) { return true; }
        if (Input.GetKey(rightInput)) { return true; }
        if (Input.GetKey(jumpInput)) { return true; }
        return false;
    }

    #region Input_Checking
    public void UpdateMoveInput()
    {
        if (Input.GetKey(forwardInput)) { player.physics.vertical = 1; }//charMoveDirection += new Vector3(desiredCamPosition.forward.x, 0, desiredCamPosition.forward.z); }
        if (Input.GetKey(backwardInput)) { player.physics.vertical = -1; }//charMoveDirection += -new Vector3(desiredCamPosition.forward.x, 0, desiredCamPosition.forward.z); }
        if (!Input.GetKey(forwardInput) && !Input.GetKey(backwardInput)) { player.physics.vertical = 0; }
        if (Input.GetKey(leftInput)) { player.physics.horizontal = -1; }//charMoveDirection += -desiredCamPosition.right; }
        if (Input.GetKey(rightInput)) { player.physics.horizontal = 1; }//charMoveDirection += desiredCamPosition.right; }
        if (!Input.GetKey(leftInput) && !Input.GetKey(rightInput)) { player.physics.horizontal = 0; }
        if (Input.GetKeyDown(jumpInput) && player.physics.isGrounded) { player.jumpRequest = true; }
        if (!Input.GetKey(sprintInput) && PlayerIsMoving() && player.physics.isGrounded) { player.currMoveState = Player.MoveState.WALKING; }
        if (Input.GetKey(sprintInput) && PlayerIsMoving() && player.physics.isGrounded) { player.currMoveState = Player.MoveState.SPRINTING; }
    }

    public void UpdatePickUpInput()
    {
        if (Input.GetKeyDown(dropItemInput)) { player.inventory.toolBarSlots[player.inventory.slotIndex].DropItem(1, ref player.defaultItemDropPos); }
        if (Input.GetKeyDown(pickUpInventory)) { player.CollectItemObject(); }
    }

    public void UpdateEditVoxelInput()
    {
        if (Input.GetMouseButtonDown(attackInput) && player.highlightBlock.gameObject.activeSelf)
        { player.inventory.AutoCollectItem(player); }
        if (Input.GetMouseButtonDown(blockInput) && player.highlightBlock.gameObject.activeSelf)
        { player.inventory.toolBarSlots[player.inventory.slotIndex].UseItem(player.worldReference, player); }
    }

    public void UpdateOpenInventoryInput()
    {
        player.inventory.cursorItemSlot.gameObject.transform.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(attackInput))
        {
            if (Input.GetKey(sprintInput)) { player.inventory.AutoAlignItem(player.inventory.CheckForSlot()); }
            else if (!Input.GetKey(sprintInput)) { player.inventory.ManageSlotLeftClick(player.inventory.CheckForSlot()); }
        }
        else if (Input.GetMouseButtonDown(blockInput))
        {
            player.inventory.ManageSlotRightClick(player.inventory.CheckForSlot());
        }
    }

    public void UpdateToggleInventoryInput()
    {
        if (Input.GetKeyDown(viewInventoryInput)) { GameManager.gManager.uiManagerRef.ToggleInventory(); }
    }

    public void UpdateToggleCamModeInput()
    {
        if (Input.GetKeyDown(toggleFPInput)) { player.CycleThroughCamAngle(); }
    }

    public void UpdateToggleDebugScreenInput()
    {
        if (Input.GetKeyDown(toggleDebugScreenInput)) { GameManager.gManager.uiManagerRef.ToggleDebugScreen(); }
    }

    #endregion
}
