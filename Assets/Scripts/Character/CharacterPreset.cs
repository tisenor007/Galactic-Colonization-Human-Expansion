using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterPreset : ScriptableObject
{
    public CharacterController.CharBehavior presetBehavior;
    public float presetWalkSpeed;
    public float presetSprintSpeed;
    public float presetJumpHeight;
    public string presetCharName;
    public string presetNickName;
    public string presetAge;
    public int presetHealth;
}
