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

    public void Rotate90()
    {
        Vector3 center = CalculateCenter();

        bottomLeft = RotatePoint90(bottomLeft - center) + center;
        topLeft = RotatePoint90(topLeft - center) + center;
        topRight = RotatePoint90(topRight - center) + center;
        bottomRight = RotatePoint90(bottomRight - center) + center;
    }

    private Vector3 RotatePoint90(Vector3 point)
    {
        return new Vector3(point.z, point.y, -point.x);
    }

    private Vector3 CalculateCenter()
    {
        return (bottomLeft + topLeft + topRight + bottomRight) / 4;
    }
}
