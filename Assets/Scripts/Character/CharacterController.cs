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
    //
    protected float charRotationSpeed = 10.0f;
    private Rigidbody rb;
    private MoveState currMoveState;

    private Ray ray;
    private RaycastHit rayHit;

    private int freeFallTimer;
    private int timeBeforeFreeFall = 1;

    //player variables
    [Header("Only Applies For Player:")]
    public GameObject playerCamTarget;
    public GameObject playerCam;
    protected float mouseSensitivity = 4.0f;
    protected float currentSpeed;

    private KeyCode forwardInput = KeyCode.W;
    private KeyCode backwardInput = KeyCode.S;
    private KeyCode leftInput = KeyCode.A;
    private KeyCode rightInput = KeyCode.D;
    private KeyCode sprintInput = KeyCode.LeftShift;
    private KeyCode jumpInput = KeyCode.Space;

    private Vector3 charMoveDirection;
    private float mouseYRotation = 0.0f;
    private float mouseXRotation = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        SetPresetStats();
        currMoveState = MoveState.IDLE;
    }

    // Update is called once per frame
    void Update()
    {
        playerCam.SetActive(true);

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
        UpdatePlayerCam();

        switch (currMoveState)
        {
            case MoveState.IDLE:
                currentSpeed = 0;
                UpdatePlayerMovement();
                UpdateFreeFallTimer();
                break;
            case MoveState.WALKING:
                currentSpeed = walkSpeed;
                transform.Translate(charMoveDirection * currentSpeed * Time.deltaTime, Space.World);
                UpdatePlayerMovement();
                UpdateFreeFallTimer();
                break;
            case MoveState.SPRINTING:
                currentSpeed = sprintSpeed;
                transform.Translate(charMoveDirection * currentSpeed * Time.deltaTime, Space.World);
                UpdatePlayerMovement();
                UpdateFreeFallTimer();
                break;
            case MoveState.JUMPING:
                transform.Translate(charMoveDirection * currentSpeed * Time.deltaTime, Space.World);
                UpdatePlayerMovement();
                UpdateFreeFallTimer();
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
    }
    #endregion

    #region PlayerFunctions
    private void UpdatePlayerMovement()
    {
        if (!PlayerIsMoving() && IsGrounded()) { currMoveState = MoveState.IDLE; }
        if (!IsGrounded() && freeFallTimer > 0) { currMoveState = MoveState.JUMPING; }
        //Input
        if (Input.GetKey(forwardInput)) { charMoveDirection += new Vector3(playerCamTarget.transform.GetChild(0).forward.x, 0, playerCamTarget.transform.GetChild(0).forward.z); }
        if (Input.GetKey(backwardInput)) { charMoveDirection += -new Vector3(playerCamTarget.transform.GetChild(0).forward.x, 0, playerCamTarget.transform.GetChild(0).forward.z); }
        if (Input.GetKey(leftInput)) { charMoveDirection += -playerCamTarget.transform.GetChild(0).right; }
        if (Input.GetKey(rightInput)) { charMoveDirection += playerCamTarget.transform.GetChild(0).right; }
        if (Input.GetKeyDown(jumpInput) && IsGrounded()) { rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + jumpHeight, rb.velocity.z); }
        if (!Input.GetKey(sprintInput) && PlayerIsMoving() && IsGrounded()) { currMoveState = MoveState.WALKING; }
        if (Input.GetKey(sprintInput) && PlayerIsMoving() && IsGrounded()) { currMoveState = MoveState.SPRINTING; }

        charMoveDirection.Normalize();

        if (charMoveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(charMoveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, charRotationSpeed);
        }
    }

    private void UpdatePlayerCam()
    {
        mouseYRotation += Input.GetAxis("Mouse Y") * mouseSensitivity;
        mouseXRotation += Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseYRotation = Mathf.Clamp(mouseYRotation, -45, 90);
        playerCamTarget.transform.localRotation = Quaternion.Euler(-mouseYRotation, mouseXRotation, 0);
        playerCamTarget.transform.position = transform.position;

        playerCam.transform.position = playerCamTarget.transform.GetChild(0).transform.position;
        playerCam.transform.rotation = Quaternion.Euler(playerCamTarget.transform.GetChild(0).transform.eulerAngles.x + 55,
        playerCamTarget.transform.GetChild(0).transform.eulerAngles.y, playerCamTarget.transform.GetChild(0).transform.eulerAngles.z);
    }

    private bool PlayerIsMoving()
    {
        if (Input.GetKey(forwardInput)) { return true; }
        if (Input.GetKey(backwardInput)) { return true; }
        if (Input.GetKey(leftInput)) { return true; }
        if (Input.GetKey(rightInput)) { return true; }
        //if (!IsGrounded()) { return true; }
        return false;
    }

    #endregion

    private void UpdateFreeFallTimer()
    {
        //freefall timer logic
        if (IsGrounded()) { freeFallTimer = (timeBeforeFreeFall * (int)(1 / Time.deltaTime)); }
        if (freeFallTimer <= 0) { currMoveState = MoveState.FALLING; }
        if (!IsGrounded()) { freeFallTimer--; }
    }

    private bool IsGrounded()
    {
        //a little broken as ray length is too long and can detect ground that is far down preventing character falling....
        ray.origin = new Vector3(this.transform.position.x, this.transform.position.y - 0.5f, this.transform.position.z);
        ray.direction = -this.transform.up;

        if (Physics.Raycast(ray, out rayHit, 0.75f))
        {
            return true;
        }
        return false;
        //return true;
    }
}
