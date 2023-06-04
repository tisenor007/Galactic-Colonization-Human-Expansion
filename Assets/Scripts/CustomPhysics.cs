using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPhysics : MonoBehaviour
{
    public bool isGrounded;
    public float entityWidth;
    public float entityHeight;
    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public float horizontal = 0;
    [HideInInspector] public float vertical = 0;
    [HideInInspector] public float verticalMomentum = 0;

    private void Start()
    {
    }

    public void CalculateVelocity(GameObject body, float force)
    {
        // Affect vertical momentum with gravity
        if (verticalMomentum > GameManager.currentWorld.gravity) { verticalMomentum += Time.fixedDeltaTime * GameManager.currentWorld.gravity; }

        velocity = ((body.transform.forward * vertical) + (body.transform.right * horizontal)).normalized * Time.deltaTime * force;

        //apply vertical momentum
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
        { velocity.z = 0; }
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
        { velocity.x = 0; }

        if (velocity.y < 0)
        { velocity.y = CheckDownSpeed(velocity.y); }
        else if (velocity.y > 0)
        { velocity.y = CheckUpSpeed(velocity.y); }
    }

    #region Collision
    private float CheckDownSpeed(float downSpeed)
    {
        if (
            GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x - entityWidth, transform.position.y + downSpeed, transform.position.z - entityWidth)) ||
            GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x + entityWidth, transform.position.y + downSpeed, transform.position.z - entityWidth)) ||
            GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x + entityWidth, transform.position.y + downSpeed, transform.position.z + entityWidth)) ||
            GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x - entityWidth, transform.position.y + downSpeed, transform.position.z + entityWidth))
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
    private float CheckUpSpeed(float upSpeed)
    {
        if (
            GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x - entityWidth, transform.position.y + entityHeight + upSpeed, transform.position.z - entityWidth)) ||
            GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x + entityWidth, transform.position.y + entityHeight + upSpeed, transform.position.z - entityWidth)) ||
            GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x + entityWidth, transform.position.y + entityHeight + upSpeed, transform.position.z + entityWidth)) ||
            GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x - entityWidth, transform.position.y + entityHeight + upSpeed, transform.position.z + entityWidth))
            )
        { return 0; }
        else
        { return upSpeed; }
    }

    public bool front
    {
        get
        {
            if (
                GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + entityWidth)) ||
                GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + entityWidth))
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
                GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - entityWidth)) ||
                GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - entityWidth))
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
                GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x - entityWidth, transform.position.y, transform.position.z)) ||
                GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x - entityWidth, transform.position.y + 1f, transform.position.z))
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
                GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x + entityWidth, transform.position.y, transform.position.z)) ||
                GameManager.currentWorld.CheckForVoxel(new Vector3(transform.position.x + entityWidth, transform.position.y + 1f, transform.position.z))
                )
            { return true; }
            else
            { return false; }
        }
    }
    #endregion
}
