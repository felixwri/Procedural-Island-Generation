using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour
{

    float rotationX = 0.0f;
    float rotationY = 0.0f;

    private bool hover = true;

    public float cameraSensitivity = 15f;
    public float cameraSpeed = 10.0f;

    [SerializeField]
    Camera mainCamera;

    public LayerMask groundMask;

    public Action<Vector3> OnLeftMouseClick;
    public Action<Vector3> OnRightMouseClick;
    public Action<Vector3> OnMouseHover;
    public Action<Vector3> OnMouseHold;
    public Action<Vector3> OnMouseRelease;
    public Action ToggleRoadPlacer;
    public Action Rotate;

    void Update()
    {
        MouseInputs();
        KeyboardInputs();
    }

    private void MouseInputs()
    {
        if (Input.GetKey(KeyCode.LeftAlt))
        {

            rotationX += Input.GetAxis("Mouse Y") * -1 * cameraSensitivity;
            rotationY += Input.GetAxis("Mouse X") * cameraSensitivity;
            mainCamera.transform.localEulerAngles = new Vector3(rotationX, rotationY, 0.0f);
        }

        if (hover)
        {
            Vector3? position = RaycastGround();
            if (position != null)
            {
                OnMouseHover?.Invoke(position.Value);
            }
        }

        if (Input.GetMouseButton(0))
        {
            Vector3? position = RaycastGround();
            if (position != null)
            {
                OnMouseHold?.Invoke(position.Value);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector3? position = RaycastGround();
            if (position != null)
            {
                OnMouseRelease?.Invoke(position.Value);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3? position = RaycastGround();
            if (position != null)
            {
                OnLeftMouseClick?.Invoke(position.Value);
            }
        };

        if (Input.GetMouseButtonDown(0))
        {
            Vector3? position = RaycastGround();
            if (position != null)
            {
                OnRightMouseClick?.Invoke(position.Value);
            }
        };
    }

    private void KeyboardInputs()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            hover = !hover;
            ToggleRoadPlacer?.Invoke();
        }

        if (Input.GetKey(KeyCode.A))
        {
            mainCamera.transform.position -= cameraSpeed * Time.deltaTime * mainCamera.transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            mainCamera.transform.position += cameraSpeed * Time.deltaTime * mainCamera.transform.right;
        }
        if (Input.GetKey(KeyCode.W))
        {
            float tx = mainCamera.transform.forward.x;
            float tz = mainCamera.transform.forward.z;
            Vector3 forward = new Vector3(tx, 0, tz).normalized;

            mainCamera.transform.position += cameraSpeed * Time.deltaTime * forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            float tx = mainCamera.transform.forward.x;
            float tz = mainCamera.transform.forward.z;
            Vector3 backward = new Vector3(tx, 0, tz).normalized;

            mainCamera.transform.position -= cameraSpeed * Time.deltaTime * backward;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            mainCamera.transform.position -= cameraSpeed * Time.deltaTime * mainCamera.transform.up;
        }
        if (Input.GetKey(KeyCode.E))
        {
            mainCamera.transform.position += cameraSpeed * Time.deltaTime * mainCamera.transform.up;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Rotate?.Invoke();
        }
    }

    private Vector3? RaycastGround()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        // Debug.Log(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Vector3 position = hit.point;
            // Debug.Log("Hit: " + position.ToString() + " Normal: " + hit.normal.ToString() + " [Distance " + hit.distance.ToString() + " from " + hit.collider.gameObject.name + "]");
            return position;
        }
        return null;
    }

}
