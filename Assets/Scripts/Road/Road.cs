using UnityEngine;

/// <summary>
/// A wrapper class for a tile piece which can calculate the required transform
/// of road pieces
/// </summary>
public class Road
{
    public Tile Left
    {
        get { return world.GetTile((int)position.x - 1, (int)position.z); }
    }
    public Tile Right
    {
        get { return world.GetTile((int)position.x + 1, (int)position.z); }
    }
    public Tile Forward
    {
        get { return world.GetTile((int)position.x, (int)position.z + 1); }
    }
    public Tile Back
    {
        get { return world.GetTile((int)position.x, (int)position.z - 1); }
    }

    public Tile tile;
    public Tile orientedTile;

    public Island world;

    public Vector3 position;
    public Direction direction;

    public Road(Tile tile, Direction direction, Island world)
    {
        this.tile = tile;

        this.position = tile.bottomLeft;
        this.direction = direction;

        this.orientedTile = OrientedTile();
    }

    private Tile OrientedTile()
    {
        Tile orientedTile = tile.Clone();

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
        tile.isRoad = true;

        Vector3 maxPosition = orientedTile.bottomLeft;
        if (orientedTile.bottomRight.y > orientedTile.bottomLeft.y)
        {
            maxPosition = orientedTile.bottomRight;
        }


        return new Vector3(position.x + 0.5f, maxPosition.y, position.z + 0.5f);
    }

    public Vector3 Place(float x, float y, float z)
    {
        tile.isRoad = true;

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
