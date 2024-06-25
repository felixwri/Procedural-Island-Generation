using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Left = -90,
    Right = 90,
    Forward = 0,
    Backward = 180
}

public class RoadPlacer : MonoBehaviour
{
    public InputController inputController;
    public Island world;

    private List<GameObject> prefabs;

    public GameObject floor;
    public GameObject corner;
    public GameObject incline;
    public GameObject incline_corner_left;
    public GameObject incline_corner_right;

    public GameObject hoverObject;

    private Vector3 startingPosition;
    private Vector3 endingPosition;
    private List<Road> path;

    private bool active = false;

    /// <summary>
    /// Initialieses all the events for the input controller. <br/>
    /// Called before the first frame update
    /// </summary>
    void Start()
    {
        inputController.OnLeftMouseClick += OnLeftMouseClick;
        inputController.OnMouseHover += OnMouseHover;
        inputController.OnMouseHold += OnMouseHold;
        inputController.OnMouseRelease += OnMouseRelease;
        inputController.ToggleRoadPlacer += ToggleActive;
    }

    void ToggleActive()
    {
        active = !active;
        hoverObject.SetActive(active);
    }

    /// <summary>
    /// Calculates the position of the bottom left corner of the tile which the vector is closest to
    /// Rounds the y value to the nearest 0.5
    /// </summary>
    /// <param name="v">Position on the world mesh</param>
    /// <returns></returns>
    private Vector3 Snap(Vector3 v)
    {
        return new Vector3(Mathf.Floor(v.x), Mathf.Round(v.y * 2) / 2, Mathf.Floor(v.z));
    }

    private void OnMouseHover(Vector3 position)
    {
        if (!active) return;
        hoverObject.transform.position = Snap(position);
    }

    private void OnLeftMouseClick(Vector3 position)
    {
        if (!active) return;
        Debug.Log("Clicked on " + position);
        startingPosition = Snap(position);
        endingPosition = Snap(position);
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

    /// <summary>
    /// Iterates through the starting and ending positions to calculate the path.
    /// The path runs across the z axis first, then the x axis.
    /// </summary>
    private void CalculatePath()
    {
        if (world.GetPoint((int)startingPosition.x, (int)startingPosition.z).id != world.GetPoint((int)endingPosition.x, (int)endingPosition.z).id)
        {
            return;
        }

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
                currentPosition = AddRoad(currentPosition, new Vector2(0, 1), Direction.Forward);
            }
            else if (currentPosition.z > endingPosition.z)
            {
                currentPosition = AddRoad(currentPosition, new Vector2(0, -1), Direction.Backward);
            }
            else if (currentPosition.x < endingPosition.x)
            {
                currentPosition = AddRoad(currentPosition, new Vector2(1, 0), Direction.Right);
            }
            else if (currentPosition.x > endingPosition.x)
            {
                currentPosition = AddRoad(currentPosition, new Vector2(-1, 0), Direction.Left);
            }
        }
    }

    /// <summary>
    /// Calculates the direction of the turn based on the current and next road piece
    /// </summary>
    /// <param name="i">The current index in the path list</param>
    /// <returns>A <c>Direction</c> of either left or right. If there is no turn, the direction is set to forward</returns>
    private Direction IsTurn(int i)
    {
        Vector3 currentDirection = path[i].position - path[i - 1].position;
        Vector3 nextDirection = path[i + 1].position - path[i].position;

        float crossProduct = currentDirection.x * nextDirection.z - currentDirection.z * nextDirection.x;

        float epsilon = 0.0001f; // Accounts for floating point error

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

    private void InstanceRoad(GameObject prefab, Vector3 position, float rotation)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.Euler(-90, rotation, 0));
        prefabs.Add(instance);
    }

    /// <summary>
    /// Calculates the correct prefab to place based on the road piece in the path list
    /// </summary>
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
                    Direction turn = IsTurn(i);
                    if (turn == Direction.Right)
                    {
                        if (path[i].IsInclineUp())
                        {
                            InstanceRoad(incline_corner_right, path[i].Place(), path[i].Rotation());
                            continue;
                        }
                        else if (path[i].IsInclineDown())
                        {
                            InstanceRoad(incline_corner_left, path[i].Place(0f, -0.5f, 0f), path[i].Rotation() - 90);
                            continue;
                        }
                        else
                        {
                            InstanceRoad(corner, path[i].Place(), path[i].Rotation());
                            continue;
                        }
                    }
                    else if (turn == Direction.Left)
                    {
                        if (path[i].IsInclineUp())
                        {
                            InstanceRoad(incline_corner_left, path[i].Place(), path[i].Rotation());
                            continue;
                        }
                        else if (path[i].IsInclineDown())
                        {
                            InstanceRoad(incline_corner_right, path[i].Place(0f, -0.5f, 0f), path[i].Rotation() + 90);
                            continue;
                        }
                        else
                        {
                            InstanceRoad(corner, path[i].Place(), path[i].Rotation() + 90);
                            continue;
                        }
                    }
                }
            }
            if (path[i].IsInclineUp())
            {
                InstanceRoad(incline, path[i].Place(), path[i].Rotation());
            }
            else if (path[i].IsInclineDown())
            {
                InstanceRoad(incline, path[i].Place(0f, -0.5f, 0f), path[i].Rotation() + 180);
            }
            else
            {
                InstanceRoad(floor, path[i].Place(), path[i].Rotation());
            }
        }
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

    void OnDrawGizmosSelected()
    {
        if (path == null) return;
        for (int i = 0; i < path.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(path[i].orientedTile.bottomLeft, 0.1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(path[i].orientedTile.topRight, 0.1f);
        }
    }
}

