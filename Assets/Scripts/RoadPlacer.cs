using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum Direction
{
    Left = -90,
    Right = 90,
    Forward = 0,
    Backward = 180
}

public enum Pieces
{
    Floor = 0,
    Corner = 1,
    Incline = 2,
    InclineCorner = 3,
}

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
    public Tile placedTile;

    public MeshGenerator world;

    public Vector3 position;
    public Direction direction;

    public Road(Tile tile, Direction direction, MeshGenerator world)
    {
        this.tile = tile;
        this.position = tile.bottomLeft;
        this.direction = direction;
    }

    public bool IsInclineUp()
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



        if (orientedTile.topLeft.y > orientedTile.bottomLeft.y)
        {
            return true;
        }
        if (orientedTile.topRight.y > orientedTile.bottomRight.y)
        {
            return true;
        }
        return false;
    }

    public bool IsInclineDown()
    {
        Tile orientedTile = tile.Clone();
        if (direction == Direction.Right)
        {
            orientedTile.TurnRight();
        }
        if (orientedTile.topLeft.y < orientedTile.bottomLeft.y)
        {
            return true;
        }
        return false;
    }

    public Vector3 Place()
    {
        tile.isRoad = true;
        return new Vector3(position.x + 0.5f, position.y, position.z + 0.5f);
    }

    public Vector3 Place(float x, float y, float z)
    {
        tile.isRoad = true;
        return new Vector3(position.x + 0.5f + x, position.y + y, position.z + 0.5f + z);
    }

    public float Rotation()
    {
        return (float)direction;
    }
}


public class RoadPlacer : MonoBehaviour
{
    public InputController inputController;
    public MeshGenerator world;

    private List<GameObject> prefabs;

    public GameObject floor;
    public GameObject corner;
    public GameObject incline;
    public GameObject incline_corner;

    public GameObject hoverObject;

    private Vector3 startingPosition;
    private Vector3 endingPosition;
    private List<Road> path;

    private bool active = false;

    // Start is called before the first frame update
    void Start()
    {
        inputController.OnMouseClick += OnMouseClick;
        inputController.OnMouseHover += OnMouseHover;
        inputController.OnMouseHold += OnMouseHold;
        inputController.OnMouseRelease += OnMouseRelease;
        inputController.ToggleRoadPlacer += Toggle;
    }

    void Toggle()
    {
        active = !active;
        hoverObject.SetActive(active);
    }

    private Vector3 Snap(Vector3 v)
    {
        return new Vector3(Mathf.Floor(v.x), Mathf.Round(v.y * 2) / 2, Mathf.Floor(v.z));

    }

    private void OnMouseHover(Vector3 position)
    {
        if (!active) return;
        hoverObject.transform.position = Snap(position);
    }

    private void OnMouseClick(Vector3 position)
    {
        if (!active) return;
        Debug.Log("Clicked on " + position);
        startingPosition = Snap(position);
    }


    private void OnMouseHold(Vector3 position)
    {
        if (!active) return;
        hoverObject.transform.position = Snap(position);

        if (endingPosition != Snap(position))
        {
            endingPosition = Snap(position);

            ClearPath();
            CalculatePath();
            PlacePath();
        }
    }


    private void OnMouseRelease(Vector3 position)
    {
        if (!active) return;
        Debug.Log("Released on " + position);
        hoverObject.transform.position = Snap(position);
        endingPosition = Snap(position);

        ClearPath();
        CalculatePath();
        PlacePath();
    }

    private Vector3 AddRoad(Vector3 currentPosition, Vector2 change, Direction direction)
    {
        currentPosition.x += change.x;
        currentPosition.z += change.y;

        path.Add(new Road(world.GetTile((int)currentPosition.x, (int)currentPosition.z), direction, world));

        return currentPosition;
    }

    private void CalculatePath()
    {
        path = new List<Road>();

        int expectedDistance = (int)Mathf.Abs(endingPosition.x - startingPosition.x) + (int)Mathf.Abs(endingPosition.z - startingPosition.z);

        Debug.Log("Expected distance: " + expectedDistance);
        Debug.Log("Start -> " + startingPosition.x + ", " + startingPosition.z);

        Vector3 currentPosition = startingPosition;

        currentPosition = AddRoad(currentPosition, new Vector2(0, 0), Direction.Forward);

        for (int i = 0; i <= expectedDistance; i++)
        {
            if (currentPosition.z < endingPosition.z)
            {
                Debug.Log(Direction.Forward.ToString());
                currentPosition = AddRoad(currentPosition, new Vector2(0, 1), Direction.Forward);
            }
            else if (currentPosition.z > endingPosition.z)
            {
                Debug.Log(Direction.Backward.ToString());
                currentPosition = AddRoad(currentPosition, new Vector2(0, -1), Direction.Backward);
            }
            else if (currentPosition.x < endingPosition.x)
            {
                Debug.Log(Direction.Right.ToString());
                currentPosition = AddRoad(currentPosition, new Vector2(1, 0), Direction.Right);
            }
            else if (currentPosition.x > endingPosition.x)
            {
                Debug.Log(Direction.Left.ToString());
                currentPosition = AddRoad(currentPosition, new Vector2(-1, 0), Direction.Left);
            }
        }
    }

    // LOL
    private Direction TurnRight(int i)
    {
        Vector3 currentDirection = path[i].position - path[i - 1].position;
        Vector3 nextDirection = path[i + 1].position - path[i].position;

        float crossProduct = currentDirection.x * nextDirection.z - currentDirection.z * nextDirection.x;

        float epsilon = 0.0001f; // You can adjust this value as needed

        if (crossProduct > epsilon)
        {
            return Direction.Left;
        }
        else if (crossProduct < -epsilon)
        {
            return Direction.Right;
        }
        return Direction.Forward;
    }

    private void PlacePath()
    {
        if (path == null) return;
        prefabs = new List<GameObject>();

        for (int i = 0; i < path.Count; i++)
        {
            // Debug.Log("Placed -> " + path[i].position + ", " + path[i].Rotation());
            if (i < path.Count - 1 && i > 0)
            {
                if (path[i + 1].Rotation() != path[i].Rotation())
                {
                    Direction turn = TurnRight(i);
                    print("TURN:::: " + turn.ToString());
                    if (turn == Direction.Right)
                    {
                        GameObject prefab = Instantiate(corner, path[i].Place(), Quaternion.Euler(-90, path[i].Rotation(), 0));
                        prefabs.Add(prefab);
                        continue;
                    }
                    else if (turn == Direction.Left)
                    {
                        GameObject prefab = Instantiate(corner, path[i].Place(), Quaternion.Euler(-90, path[i].Rotation() + 90, 0));
                        prefabs.Add(prefab);
                        continue;
                    }

                }
            }
            if (path[i].IsInclineUp())
            {
                GameObject prefab = Instantiate(incline, path[i].Place(), Quaternion.Euler(-90, path[i].Rotation(), 0));
                prefabs.Add(prefab);
            }
            else if (path[i].IsInclineDown())
            {
                GameObject prefab = Instantiate(incline, path[i].Place(0f, -0.5f, 0f), Quaternion.Euler(-90, path[i].Rotation() + 180, 0));
                prefabs.Add(prefab);
            }
            else
            {
                GameObject prefab = Instantiate(floor, path[i].Place(), Quaternion.Euler(-90, path[i].Rotation(), 0));
                prefabs.Add(prefab);
            }
        }
        path = null;
    }

    private void ClearPath()
    {
        if (prefabs == null) return;
        for (int i = 0; i < prefabs.Count; i++)
        {
            Destroy(prefabs[i]);
        }
        prefabs = null;
    }
}

