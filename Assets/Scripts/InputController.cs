using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour
{

    float rotationX = 0.0f;
    float rotationY = 0.0f;

    public float cameraSensitivity = 15f;
    public float cameraSpeed = 10.0f;

    [SerializeField]
    Camera mainCamera;

    public LayerMask groundMask;

    public Action<Vector3Int> OnMouseClick;

    // void Start()
    // {
    //     Cursor.lockState = CursorLockMode.Locked;
    //     Cursor.visible = false;
    // }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt))
        {

            rotationX += Input.GetAxis("Mouse Y") * -1 * cameraSensitivity;
            rotationY += Input.GetAxis("Mouse X") * cameraSensitivity;
            mainCamera.transform.localEulerAngles = new Vector3(rotationX, rotationY, 0.0f);
        }

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Vector3Int? position = RaycastGround();
            if (position != null)
            {
                OnMouseClick?.Invoke(position.Value);
            }

        };

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
            Vector3 forward = new Vector3(tx, 0, tz);

            mainCamera.transform.position += cameraSpeed * Time.deltaTime * forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            float tx = mainCamera.transform.forward.x;
            float tz = mainCamera.transform.forward.z;
            Vector3 backward = new Vector3(tx, 0, tz);

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
    }

    private Vector3Int? RaycastGround()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Debug.Log(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Vector3Int positionInt = Vector3Int.RoundToInt(hit.point);
            Debug.Log("Hit: " + positionInt.ToString() + " Normal: " + hit.normal.ToString() + " [Distance " + hit.distance.ToString() + " from " + hit.collider.gameObject.name + "]");
            return positionInt;
        }
        return null;
    }

}
