using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour //class is getting pretty crowded, I might abandon behaviours and make subclasses...
{
    //determines what the charater will be (including player)
    //moving to sub-classes rather than states. May use enum for future feature.
    //public enum CharBehavior
    //{
    //    PLAYER,
    //    HOSTILE,
    //    NEUTRAL,
    //    FRIENDLY
    //}
    //
    public CharacterPreset preset;

    public enum MoveState
    {
        IDLE,
        WALKING,
        SPRINTING,
        JUMPING,
        FALLING,
        DEAD
    }

    //stats..
    //public CharBehavior behavior;
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
    public GameObject charModel;

    [HideInInspector] public MoveState currMoveState;
    [HideInInspector] public World worldReference;
    [HideInInspector] public bool jumpRequest;

    protected float currentSpeed;
    protected float charRotationSpeed = 10.0f;
   
    // Start is called before the first frame update
    public virtual void Start()
    {
        worldReference = GameManager.currentWorld;
        SetPresetStats();
        currMoveState = MoveState.IDLE;
        Move();
    }

    public virtual void FixedUpdate()
    {
        if (jumpRequest) { Jump(); }
        Move();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }

    public void Move()
    {
        physics.CalculateVelocity(charModel, currentSpeed);
        transform.Translate(physics.velocity, Space.World);
    }

    public void Jump()
    {
        physics.verticalMomentum = jumpHeight;
        currMoveState = MoveState.JUMPING;
        physics.isGrounded = false;
        jumpRequest = false;
    }


    protected void SetPresetStats()
    {
        if (preset == null) { return; }
        //behavior = preset.presetBehavior;
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
   
}
