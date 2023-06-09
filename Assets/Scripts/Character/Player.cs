using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : CharacterController
{
    public enum GameMode
    {
        SURVIVAL,
        CREATIVE,
        ADVENTURE,
    }

    public GameObject playerCamTarget;
    public GameObject playerCam;
    public Inventory inventory;

    public Transform highlightBlock;
    public Transform placeBlock;

    public GameMode currentGameMode;

    [HideInInspector] public float checkIncrement = 0.1f;
    [HideInInspector] public float reach = 8f;

    [HideInInspector] public byte selectedBlockIndex = 1;

    public LayerMask ignoreMask;
    [HideInInspector] public Vector3 defaultItemDropPos;

    protected enum CameraAngle
    {
        FIRST_PERSON,
        THIRD_PERSON_SHORT,
        THIRD_PERSON_MEDIUM,
        THIRD_PERSON_FAR
    }

    protected float mouseSensitivity = 4.0f;
    protected CameraAngle currentCamAngle;

    private Vector3 charMoveDirection;
    private float mouseYRotation = 0.0f;
    private float mouseXRotation = 0.0f;
    private Transform desiredCamPosition;
    //temporary....
    private GameObject playerHead;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        desiredCamPosition = playerCamTarget.transform.GetChild(0);
        playerHead = charModel.transform.GetChild(0).gameObject;
        currentCamAngle = CameraAngle.FIRST_PERSON;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        charModel.transform.localRotation = Quaternion.Euler(0, mouseXRotation, 0);
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
        PlaceCursorBlocks();
        //stangely only spawns at target postion and not in front of it.. fix later
        defaultItemDropPos = this.transform.position + (playerCamTarget.transform.forward * 20f);
    }

    public void CycleThroughCamAngle()
    {
        if ((int)currentCamAngle < 4)
        { currentCamAngle++; }
        else if ((int)currentCamAngle >= 4)
        { currentCamAngle = 0; }
    }

    public void CollectItemObject()
    {
        if (Physics.Raycast(playerCamTarget.transform.position, playerCam.transform.forward, out RaycastHit raycastHit, reach / 2, ~ignoreMask))
        {
            if (raycastHit.transform.parent.GetComponent<ItemObject>())
            { raycastHit.transform.parent.GetComponent<ItemObject>().OnThisItemCollected();}
        }
    }

    public void UpdatePlayerCam()
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

    private void UpdatePlayerMovement()
    {
        //Input is getting crowded.... make a input manager in the future?
        if (!GameManager.gManager.inputManager.PlayerIsMoving() && physics.isGrounded) { currMoveState = MoveState.IDLE; }
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
}
