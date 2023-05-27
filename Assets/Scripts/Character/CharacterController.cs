using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
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

    protected float currentSpeed;
    protected float charRotationSpeed = 10.0f;
    private float verticalMomentum = 0;
    private bool jumpRequest;
    private MoveState currMoveState;
    public bool isGrounded;
    //
    private World worldReference;
    private Vector3 velocity;
    private float horizontal = 0;
    private float vertical = 0;

    //player variables
    [Header("Only Applies For Player:")]
    public GameObject playerCamTarget;
    public GameObject charModel;
    public GameObject playerCam;
    protected float mouseSensitivity = 4.0f;
    protected CameraAngle currentCamAngle;

    private KeyCode forwardInput = KeyCode.W;
    private KeyCode backwardInput = KeyCode.S;
    private KeyCode leftInput = KeyCode.A;
    private KeyCode rightInput = KeyCode.D;
    private KeyCode sprintInput = KeyCode.LeftShift;
    private KeyCode jumpInput = KeyCode.Space;
    private KeyCode toggleFPInput = KeyCode.F5;

    private Vector3 charMoveDirection;
    private float mouseYRotation = 0.0f;
    private float mouseXRotation = 0.0f;

    private Transform desiredCamPosition;
    //temporary....
    private GameObject playerHead;


    // Start is called before the first frame update
    void Start()
    {
        worldReference = GameObject.Find("World").GetComponent<World>();
        desiredCamPosition = playerCamTarget.transform.GetChild(0);
        playerHead = charModel.transform.GetChild(0).gameObject;
        SetPresetStats();
        currMoveState = MoveState.IDLE;
        currentCamAngle = CameraAngle.FIRST_PERSON;
    }

    private void FixedUpdate()
    {
        if (jumpRequest) { Jump(); }
        CalculateVelocity();
        transform.Translate(velocity, Space.World);
    }

    // Update is called once per frame
    void Update()
    {
        playerCam.SetActive(true);
        UpdatePlayerCam();
        charModel.transform.localRotation = Quaternion.Euler(0, mouseXRotation, 0);

        switch (behavior)
        {
            case CharBehavior.PLAYER:
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
    }
    #endregion

    #region PlayerFunctions
    private void UpdatePlayerMovement()
    {
        if (!PlayerIsMoving() && isGrounded) { currMoveState = MoveState.IDLE; }
        //Input //would be a switch statment, however, inputs can be pressed/function at the same time....
        if (Input.GetKey(forwardInput)) { vertical = 1; }//charMoveDirection += new Vector3(desiredCamPosition.forward.x, 0, desiredCamPosition.forward.z); }
        if (Input.GetKey(backwardInput)) { vertical = -1; }//charMoveDirection += -new Vector3(desiredCamPosition.forward.x, 0, desiredCamPosition.forward.z); }
        if (!Input.GetKey(forwardInput) && !Input.GetKey(backwardInput)) { vertical = 0; }
        if (Input.GetKey(leftInput)) { horizontal = -1; }//charMoveDirection += -desiredCamPosition.right; }
        if (Input.GetKey(rightInput)) { horizontal = 1; }//charMoveDirection += desiredCamPosition.right; }
        if (!Input.GetKey(leftInput) && !Input.GetKey(rightInput)) { horizontal = 0; }
        if (Input.GetKeyDown(jumpInput) && isGrounded) { jumpRequest = true; }
        if (!Input.GetKey(sprintInput) && PlayerIsMoving() && isGrounded) { currMoveState = MoveState.WALKING; }
        if (Input.GetKey(sprintInput) && PlayerIsMoving() && isGrounded) { currMoveState = MoveState.SPRINTING; }
        if (Input.GetKeyDown(toggleFPInput)) { CycleThroughCamAngle(); }
        
    }

    private void UpdatePlayerCam()
    {
        mouseYRotation += Input.GetAxis("Mouse Y") * mouseSensitivity;
        mouseXRotation += Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseYRotation = Mathf.Clamp(mouseYRotation, -45, 90);
        playerCamTarget.transform.localRotation = Quaternion.Euler(-mouseYRotation, mouseXRotation, 0);
        playerCamTarget.transform.position = new Vector3(charModel.transform.position.x, charModel.transform.position.y + .48f, charModel.transform.position.z);

        switch (currentCamAngle)
        {
            case CameraAngle.FIRST_PERSON:
                playerCam.transform.position = playerCamTarget.transform.position;
                playerCam.transform.rotation = Quaternion.Euler(playerCamTarget.transform.eulerAngles.x,
                playerCamTarget.transform.eulerAngles.y, playerCamTarget.transform.eulerAngles.z);
                if (playerHead.activeSelf) { playerHead.SetActive(false); }
                break;
            case CameraAngle.THIRD_PERSON_MEDIUM:
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
    void Jump()
    {
        verticalMomentum = jumpHeight;
        currMoveState = MoveState.JUMPING;
        isGrounded = false;
        jumpRequest = false;
    }
    private void CalculateVelocity()
    {
        // Affect vertical momentum with gravity
        if (verticalMomentum > worldReference.gravity) { verticalMomentum += Time.fixedDeltaTime * worldReference.gravity; }

        velocity = ((charModel.transform.forward * vertical) + (charModel.transform.right * horizontal)).normalized * Time.deltaTime * currentSpeed;

        //apply vertical momentum
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
        { velocity.z = 0; }
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
        { velocity.x = 0; }

        if (velocity.y < 0)
        { velocity.y = checkDownSpeed(velocity.y); }
        else if (velocity.y > 0)
        { velocity.y = checkUpSpeed(velocity.y); }
    }

    #region Collision
    private float checkDownSpeed(float downSpeed)
    {
        if (
            worldReference.CheckForVoxel(transform.position.x - characterWidth, transform.position.y + downSpeed, transform.position.z - characterWidth) ||
            worldReference.CheckForVoxel(transform.position.x + characterWidth, transform.position.y + downSpeed, transform.position.z - characterWidth) ||
            worldReference.CheckForVoxel(transform.position.x + characterWidth, transform.position.y + downSpeed, transform.position.z + characterWidth) ||
            worldReference.CheckForVoxel(transform.position.x - characterWidth, transform.position.y + downSpeed, transform.position.z + characterWidth)
            )
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }
    private float checkUpSpeed(float upSpeed)
    {
        if (
            worldReference.CheckForVoxel(transform.position.x - characterWidth, transform.position.y + characterHeight + upSpeed, transform.position.z - characterWidth) ||
            worldReference.CheckForVoxel(transform.position.x + characterWidth, transform.position.y + characterHeight + upSpeed, transform.position.z - characterWidth) ||
            worldReference.CheckForVoxel(transform.position.x + characterWidth, transform.position.y + characterHeight + upSpeed, transform.position.z + characterWidth) ||
            worldReference.CheckForVoxel(transform.position.x - characterWidth, transform.position.y + characterHeight + upSpeed, transform.position.z + characterWidth)
            )
        {return 0;}
        else
        {return upSpeed;}
    }

    public bool front 
    {
        get
        {
            if (
                worldReference.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z + characterWidth) ||
                worldReference.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z + characterWidth)
                )
            { return true; }
            else
            { return false; }
        }
    }
    public bool back
    {
        get
        {
            if (
                worldReference.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z - characterWidth) ||
                worldReference.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z - characterWidth)
                )
            { return true; }
            else
            { return false; }
        }
    }
    public bool left
    {
        get
        {
            if (
                worldReference.CheckForVoxel(transform.position.x - characterWidth, transform.position.y, transform.position.z) ||
                worldReference.CheckForVoxel(transform.position.x - characterWidth, transform.position.y + 1f, transform.position.z)
                )
            { return true; }
            else
            { return false; }
        }
    }
    public bool right
    {
        get
        {
            if (
                worldReference.CheckForVoxel(transform.position.x + characterWidth, transform.position.y, transform.position.z) ||
                worldReference.CheckForVoxel(transform.position.x + characterWidth, transform.position.y + 1f, transform.position.z)
                )
            { return true; }
            else
            { return false; }
        }
    }
    #endregion
}
