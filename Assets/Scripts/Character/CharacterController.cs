using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour //class is getting pretty crowded, I might abandon behaviours and make subclasses...
{
    //determines what the charater will be (including player)
    public enum CharBehavior
    {
        PLAYER,
        HOSTILE,
        NEUTRAL,
        FRIENDLY
    }
    //
    public CharacterPreset preset;

    protected enum CameraAngle
    {
        FIRST_PERSON,
        THIRD_PERSON_SHORT,
        THIRD_PERSON_MEDIUM,
        THIRD_PERSON_FAR
    }

    protected enum MoveState
    {
        IDLE,
        WALKING,
        SPRINTING,
        JUMPING,
        FALLING,
        DEAD
    }

    //stats..
    public CharBehavior behavior;
    public float walkSpeed;
    public float sprintSpeed;
    public float jumpHeight;
    public string charName;
    public string nickName;
    public string age;
    public int health;
    public float characterWidth;
    public float characterHeight;
    public CustomPhysics physics;

    protected float currentSpeed;
    protected float charRotationSpeed = 10.0f;
    private bool jumpRequest;
    private MoveState currMoveState;
    //
    private World worldReference;

    //player variables
    [Header("Only Applies For Player:")]
    public GameObject playerCamTarget;
    public GameObject charModel;
    public GameObject playerCam;
    public Inventory inventory;

    public Transform highlightBlock;
    public Transform placeBlock;

    public float checkIncrement = 0.1f;
    public float reach = 8f;

    public byte selectedBlockIndex = 1;

    public LayerMask ignoreMask;
    [HideInInspector] public Vector3 defaultItemDropPos;

    protected float mouseSensitivity = 4.0f;
    protected CameraAngle currentCamAngle;

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


    private Vector3 charMoveDirection;
    private float mouseYRotation = 0.0f;
    private float mouseXRotation = 0.0f;
    private Transform desiredCamPosition;
    //temporary....
    private GameObject playerHead;


    // Start is called before the first frame update
    void Start()
    {
        worldReference = GameManager.currentWorld;
        SetPresetStats();
        currMoveState = MoveState.IDLE;
        defaultItemDropPos = new Vector3(this.transform.position.x, this.transform.position.y + 2, this.transform.position.z + 5);
        Move();

        switch (behavior)
        {
            case CharBehavior.PLAYER:
                desiredCamPosition = playerCamTarget.transform.GetChild(0);
                playerHead = charModel.transform.GetChild(0).gameObject;
                currentCamAngle = CameraAngle.FIRST_PERSON;
                break;
            case CharBehavior.HOSTILE:
                break;
            case CharBehavior.NEUTRAL:
                break;
            case CharBehavior.FRIENDLY:
                break;
        };
    }

    private void FixedUpdate()
    {
        if (jumpRequest) { Jump(); }
        Move();
        switch (behavior)
        {
            case CharBehavior.PLAYER:
                break;
            case CharBehavior.HOSTILE:
                break;
            case CharBehavior.NEUTRAL:
                break;
            case CharBehavior.FRIENDLY:
                break;
        };
    }

    // Update is called once per frame
    void Update()
    {
        //if (worldReference == null) { worldReference = GameManager.currentWorld;  return; }

        switch (behavior)
        {
            case CharBehavior.PLAYER:
                charModel.transform.localRotation = Quaternion.Euler(0, mouseXRotation, 0);
                RunPlayerBehavior();
                break;
            case CharBehavior.HOSTILE:
                break;
            case CharBehavior.NEUTRAL:
                break;
            case CharBehavior.FRIENDLY:
                break;
        };
    }

    protected void RunPlayerBehavior()
    {
        switch (currMoveState)
        {
            case MoveState.IDLE:
                currentSpeed = walkSpeed;
                UpdatePlayerMovement();
                break;
            case MoveState.WALKING:
                currentSpeed = walkSpeed;
                UpdatePlayerMovement();
                break;
            case MoveState.SPRINTING:
                currentSpeed = sprintSpeed;
                UpdatePlayerMovement();
                break;
            case MoveState.JUMPING:
                UpdatePlayerMovement();
                break;
            case MoveState.FALLING:
                UpdatePlayerMovement();
                break;
        }
        if (GameManager.gManager.currentGameState == GameManager.GameState.INVENTORY) { return; }
        playerCam.SetActive(true);
        UpdatePlayerCam();
        PlaceCursorBlocks();
    }

    #region StatHandling
    protected void SetPresetStats()
    {
        if (preset == null) { return; }
        behavior = preset.presetBehavior;
        walkSpeed = preset.presetWalkSpeed;
        sprintSpeed = preset.presetSprintSpeed;
        jumpHeight = preset.presetJumpHeight;
        charName = preset.presetCharName;
        nickName = preset.presetNickName;
        age = preset.presetAge;
        health = preset.presetHealth;
        characterWidth = preset.presetCharacterWidth;
        characterHeight = preset.presetCharacterHeight;

        physics.entityWidth = characterWidth;
        physics.entityHeight = characterHeight;
    }
    #endregion

    #region PlayerFunctions
    private void UpdatePlayerMovement()
    {
        //Input is getting crowded.... make a input manager in the future?
        if (!PlayerIsMoving() && physics.isGrounded) { currMoveState = MoveState.IDLE; }
        //Input //would be a switch statment, however, inputs can be pressed/function at the same time....
        if (Input.GetKey(forwardInput)) { physics.vertical = 1; }//charMoveDirection += new Vector3(desiredCamPosition.forward.x, 0, desiredCamPosition.forward.z); }
        if (Input.GetKey(backwardInput)) { physics.vertical = -1; }//charMoveDirection += -new Vector3(desiredCamPosition.forward.x, 0, desiredCamPosition.forward.z); }
        if (!Input.GetKey(forwardInput) && !Input.GetKey(backwardInput)) { physics.vertical = 0; }
        if (Input.GetKey(leftInput)) { physics.horizontal = -1; }//charMoveDirection += -desiredCamPosition.right; }
        if (Input.GetKey(rightInput)) { physics.horizontal = 1; }//charMoveDirection += desiredCamPosition.right; }
        if (!Input.GetKey(leftInput) && !Input.GetKey(rightInput)) { physics.horizontal = 0; }
        if (Input.GetKeyDown(jumpInput) && physics.isGrounded) { jumpRequest = true; }
        if (!Input.GetKey(sprintInput) && PlayerIsMoving() && physics.isGrounded) { currMoveState = MoveState.WALKING; }
        if (Input.GetKey(sprintInput) && PlayerIsMoving() && physics.isGrounded) { currMoveState = MoveState.SPRINTING; }
        if (Input.GetKeyDown(toggleFPInput)) { CycleThroughCamAngle(); }
        if (Input.GetKeyDown(toggleDebugScreenInput)) { GameManager.uiManagerRef.ToggleDebugScreen(); }
        if (Input.GetMouseButtonDown(attackInput) && highlightBlock.gameObject.activeSelf) 
        { inventory.AutoCollectItem(this); }
        if (Input.GetMouseButtonDown(blockInput) && highlightBlock.gameObject.activeSelf)
        { inventory.toolBarSlots[inventory.slotIndex].UseItem(worldReference, this); }
        if (Input.GetKeyDown(dropItemInput)) { inventory.toolBarSlots[inventory.slotIndex].DropItem(1, defaultItemDropPos); }
        if (Input.GetKeyDown(viewInventoryInput)) { GameManager.uiManagerRef.ToggleInventory(); }
        if (Input.GetKeyDown(pickUpInventory)) { CollectItemObject(); }
    }

    private void CollectItemObject()
    {
        if (Physics.Raycast(GameManager.player.playerCamTarget.transform.position, GameManager.player.playerCamTarget.transform.forward, out RaycastHit raycastHit, reach/2, ~ignoreMask))
        { 
            if (raycastHit.transform.parent.GetComponent<ItemObject>()) 
            {
                raycastHit.transform.parent.GetComponent<ItemObject>().OnThisItemCollected();
            }
        }
    }

    private void PlaceCursorBlocks()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {
            Vector3 pos = playerCamTarget.transform.position + (playerCam.transform.forward * step);

            if (worldReference.CheckForVoxel(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;
            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;
        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }

    private void UpdatePlayerCam()
    {
        mouseYRotation += Input.GetAxis("Mouse Y") * mouseSensitivity;
        mouseXRotation += Input.GetAxis("Mouse X") * mouseSensitivity;
        playerCamTarget.transform.localRotation = Quaternion.Euler(-mouseYRotation, mouseXRotation, 0);
        playerCamTarget.transform.position = new Vector3(charModel.transform.position.x, charModel.transform.position.y + .48f, charModel.transform.position.z);

        switch (currentCamAngle)
        {
            case CameraAngle.FIRST_PERSON:
                mouseYRotation = Mathf.Clamp(mouseYRotation, -90, 90);
                playerCam.transform.position = playerCamTarget.transform.position;
                playerCam.transform.rotation = Quaternion.Euler(playerCamTarget.transform.eulerAngles.x,
                playerCamTarget.transform.eulerAngles.y, playerCamTarget.transform.eulerAngles.z);
                if (playerHead.activeSelf) { playerHead.SetActive(false); }
                break;
            case CameraAngle.THIRD_PERSON_MEDIUM:
                mouseYRotation = Mathf.Clamp(mouseYRotation, -45, 90);
                playerCam.transform.position = desiredCamPosition.transform.position;
                playerCam.transform.rotation = Quaternion.Euler(desiredCamPosition.transform.eulerAngles.x + 55,
                desiredCamPosition.eulerAngles.y, desiredCamPosition.transform.eulerAngles.z);
                if (!playerHead.activeSelf) { playerHead.SetActive(true); }
                break;
        }
    }


    private bool PlayerIsMoving()
    {
        if (Input.GetKey(forwardInput)) { return true; }
        if (Input.GetKey(backwardInput)) { return true; }
        if (Input.GetKey(leftInput)) { return true; }
        if (Input.GetKey(rightInput)) { return true; }
        if (Input.GetKey(jumpInput)) { return true; }
        return false;
    }

    private void CycleThroughCamAngle()
    {
        if ((int)currentCamAngle < 4)
        { currentCamAngle++; }
        else if ((int)currentCamAngle >= 4)
        { currentCamAngle = 0; }
    }

    #endregion

    void Move()
    {
        physics.CalculateVelocity(charModel, currentSpeed);
        transform.Translate(physics.velocity, Space.World);
    }

    void Jump()
    {
        physics.verticalMomentum = jumpHeight;
        currMoveState = MoveState.JUMPING;
        physics.isGrounded = false;
        jumpRequest = false;
    }
   
}
