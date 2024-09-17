using System;
using UnityEngine;

public class Quad
{
    public Vector3 bottomLeft;
    public Vector3 topLeft;
    public Vector3 bottomRight;
    public Vector3 topRight;

    public bool isWater = false;
    public bool isRoad = false;
    public bool isBuilding = false;
    public bool containsTree = false;

    public bool temp = false;

    public Color color = Color.black;
    public float islandIndex = 0;

    public Quad(Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight)
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

    public void TurnBack()
    {
        Vector3 temp = bottomLeft;
        bottomLeft = topRight;
        topRight = temp;
        temp = topLeft;
        topLeft = bottomRight;
        bottomRight = temp;
    }

    public Quad Clone()
    {
        return new Quad(bottomLeft, topLeft, topRight, bottomRight);
    }

    public override String ToString()
    {
        return "BottomLeft: " + bottomLeft + " TopLeft: " + topLeft + " TopRight: " + topRight + " BottomRight: " + bottomRight;
    }
}
