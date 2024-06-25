using UnityEngine;

/// <summary>
/// A wrapper class for a Quad piece which can calculate the required transform
/// of road pieces
/// </summary>
public class Road
{
    public Quad Left
    {
        get { return world.GetTile((int)position.x - 1, (int)position.z); }
    }
    public Quad Right
    {
        get { return world.GetTile((int)position.x + 1, (int)position.z); }
    }
    public Quad Forward
    {
        get { return world.GetTile((int)position.x, (int)position.z + 1); }
    }
    public Quad Back
    {
        get { return world.GetTile((int)position.x, (int)position.z - 1); }
    }

    public Quad quad;
    public Quad orientedTile;

    public Island world;

    public Vector3 position;
    public Direction direction;

    public Road(Quad quad, Direction direction, Island world)
    {
        this.quad = quad;

        this.position = quad.bottomLeft;
        this.direction = direction;

        this.orientedTile = OrientedTile();
    }

    private Quad OrientedTile()
    {
        Quad orientedTile = quad.Clone();

        if (direction == Direction.Right)
        {
            orientedTile.TurnRight();
        }
        else if (direction == Direction.Left)
        {
            orientedTile.TurnLeft();
        }
        else if (direction == Direction.Backward)
        {
            orientedTile.TurnBack();
        }
        return orientedTile;
    }

    public bool IsInclineUp()
    {
        if (orientedTile.bottomLeft.y == orientedTile.bottomRight.y && (
            orientedTile.topLeft.y > orientedTile.bottomLeft.y ||
            orientedTile.topRight.y > orientedTile.bottomRight.y))
        {
            return true;
        }
        if (orientedTile.bottomLeft.y != orientedTile.bottomRight.y && (
            orientedTile.topLeft.y > orientedTile.bottomLeft.y &&
            orientedTile.topRight.y > orientedTile.bottomRight.y))
        {
            return true;
        }
        return false;
    }

    public bool IsInclineDown()
    {
        if (orientedTile.bottomLeft.y == orientedTile.bottomRight.y && (
            orientedTile.topLeft.y < orientedTile.bottomLeft.y &&
            orientedTile.topRight.y < orientedTile.bottomRight.y))
        {
            return true;
        }
        if (orientedTile.bottomLeft.y != orientedTile.bottomRight.y && (
            orientedTile.topLeft.y < orientedTile.bottomLeft.y ||
            orientedTile.topRight.y < orientedTile.bottomRight.y))
        {
            return true;
        }
        return false;
    }

    public Vector3 Place()
    {
        quad.isRoad = true;

        Vector3 maxPosition = orientedTile.bottomLeft;
        if (orientedTile.bottomRight.y > orientedTile.bottomLeft.y)
        {
            maxPosition = orientedTile.bottomRight;
        }


        return new Vector3(position.x + 0.5f, maxPosition.y, position.z + 0.5f);
    }

    public Vector3 Place(float x, float y, float z)
    {
        quad.isRoad = true;

        Vector3 maxPosition = orientedTile.bottomLeft;
        if (orientedTile.bottomRight.y > orientedTile.bottomLeft.y)
        {
            maxPosition = orientedTile.bottomRight;
        }

        return new Vector3(position.x + 0.5f + x, maxPosition.y + y, position.z + 0.5f + z);
    }

    public float Rotation()
    {
        return (float)direction;
    }
}
