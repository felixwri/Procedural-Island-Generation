using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class AgentMind : MonoBehaviour
{
    public InputController inputController;
    public Island world;

    Vector3 velocity = Vector3.zero;
    public Vector3 acceleration = new Vector3(0.01f, 0.01f, 0.01f);
    public Vector3 maxVelocity = new Vector3(0.5f, 0.5f, 0.5f);

    public float choiceWeight = 1f;

    Vector3Int targetPosition;

    List<Vector3Int> path;

    void Start()
    {
        transform.position = new Vector3(0, 0, 0);
        // inputController.OnMouseClick += OnMouseClick;
    }

    void Update()
    {
        if (targetPosition == null || targetPosition == Vector3Int.RoundToInt(transform.position))
        {
            return;
        }

        if (path.Count > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, path[0], 0.02f);
            // Debug.Log(transform.position + " | " + path[0]);
            if (Vector3Int.RoundToInt(transform.position) == path[0])
            {
                // Debug.Log("Completed Section");
                path.RemoveAt(0);
            }
        }

        // if (velocity.magnitude < maxVelocity.magnitude)
        // {
        //     velocity += acceleration;
        // }
        // transform.position = Vector3.MoveTowards(transform.position, targetPosition, velocity.magnitude);
    }

    private void OnMouseClick(Vector3Int position)
    {
        Debug.Log("Clicked on " + position);
        targetPosition = new Vector3Int(position.x, position.y, position.z);
        calculatePath();
    }

    private void calculatePath()
    {
        // int heightDelta(Vector3 nextPosition, Vector3 direction)
        // {
        //     if (nextPosition.x + direction.x < 0 || nextPosition.z + direction.z < 0)
        //     {
        //         return 0;
        //     }

        //     int delta = world.GetVertice((int)nextPosition.x + (int)direction.x, (int)nextPosition.z + (int)direction.z).y -
        //         world.GetVertice((int)nextPosition.x, (int)nextPosition.z).y;
        //     return delta;
        // }
        path = new List<Vector3Int>();

        path.Add(Vector3Int.RoundToInt(transform.position));

        int index = 1;
        while (index < 100)
        {

            Vector3 nextPosition = path[index - 1];

            float left = Vector3.Distance(nextPosition + Vector3.left, targetPosition);
            if (nextPosition.x - 1 < 0) left = Mathf.Infinity;


            float right = Vector3.Distance(nextPosition + Vector3.right, targetPosition);

            float forward = Vector3.Distance(nextPosition + Vector3.forward, targetPosition);
            float backward = Vector3.Distance(nextPosition + Vector3.back, targetPosition);
            if (nextPosition.z - 1 < 0) backward = Mathf.Infinity;

            // Debug.Log("UW: " + left + " | " + right + " | " + forward + " | " + backward);
            // int deltaLeft = heightDelta(nextPosition, Vector3.left);
            // int deltaRight = heightDelta(nextPosition, Vector3.right);
            // int deltaForward = heightDelta(nextPosition, Vector3.forward);
            // int deltaBackward = heightDelta(nextPosition, Vector3.back);

            // left += deltaLeft * choiceWeight;
            // right += deltaRight * choiceWeight;
            // forward += deltaForward * choiceWeight;
            // backward += deltaBackward * choiceWeight;

            // Debug.Log("XW: " + left + " | " + right + " | " + forward + " | " + backward);

            if (left <= right && left <= forward && left <= backward)
            {
                nextPosition += Vector3Int.left;
            }
            else if (right <= left && right <= forward && right <= backward)
            {
                nextPosition += Vector3Int.right;
            }
            else if (forward <= left && forward <= right && forward <= backward)
            {
                nextPosition += Vector3Int.forward;
            }
            else if (backward <= left && backward <= right && backward <= forward)
            {
                nextPosition += Vector3Int.back;
            }

            nextPosition.y = world.GetVertice((int)nextPosition.x, (int)nextPosition.z).y;
            nextPosition.y += 1;

            path.Add(Vector3Int.RoundToInt(nextPosition));

            // Debug.Log(nextPosition.ToString() + " | " + targetPosition.ToString());
            if (nextPosition.x == targetPosition.x && nextPosition.z == targetPosition.z)
            {
                Debug.Log("Found Path: " + path.Count + " steps long");
                break;
            }

            index++;
        }

    }

    void OnDrawGizmos()
    {
        if (path != null)
        {
            Gizmos.color = Color.red;
            foreach (Vector3Int position in path)
            {
                Gizmos.DrawSphere(position, 0.2f);
            }
        }
    }
}
