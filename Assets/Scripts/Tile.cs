using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Tile
{
    public Vector3 bottomLeft;
    public Vector3 topLeft;
    public Vector3 bottomRight;
    public Vector3 topRight;

    public bool isWater = false;
    public bool isRoad = false;
    public bool containsTree = false;

    public Tile(Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight)
    {
        this.bottomLeft = bottomLeft;
        this.topLeft = topLeft;
        this.bottomRight = bottomRight;
        this.topRight = topRight;
    }

    public void TurnRight()
    {
        Vector3 temp = bottomLeft;
        bottomLeft = topLeft;
        topLeft = topRight;
        topRight = bottomRight;
        bottomRight = temp;
    }

    public void TurnLeft()
    {
        Vector3 temp = bottomLeft;
        bottomLeft = bottomRight;
        bottomRight = topRight;
        topRight = topLeft;
        topLeft = temp;
    }

    public Tile Clone()
    {
        return new Tile(bottomLeft, topLeft, topRight, bottomRight);
    }

    public override String ToString()
    {
        return "BottomLeft: " + bottomLeft + " TopLeft: " + topLeft + " TopRight: " + topRight + " BottomRight: " + bottomRight;
    }
}
