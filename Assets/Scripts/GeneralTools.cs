using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GeneralTools : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public enum Direction
{
    FORWARD, // z+ direction
    RIGHT, // +x direction
    BACKWARDS, // -z direction
    LEFT, // -x direction
    UP, // +y direction
    DOWN // -y direction

}

public static class DirectionExtensions
{
    public static Vector3Int GetVector(this Direction direction)
    {
        return direction switch
        {
            Direction.UP => Vector3Int.up,
            Direction.DOWN => Vector3Int.down,
            Direction.RIGHT => Vector3Int.right,
            Direction.LEFT => Vector3Int.left,
            Direction.FORWARD => Vector3Int.forward,
            Direction.BACKWARDS => Vector3Int.back,
            _ => throw new Exception("Invalid input direction")
        };
    }
}
