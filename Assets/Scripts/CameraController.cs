using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    float rotationX = 0.0f;
    float rotationY = 0.0f;
    public float sensitivityX = 15f;

    public float speed = 10.0f;

    bool cursorLocked = true;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        rotationX += Input.GetAxis("Mouse Y") * -1 * sensitivityX;
        rotationY += Input.GetAxis("Mouse X") * sensitivityX;
        transform.localEulerAngles = new Vector3(rotationX, rotationY, 0.0f);

        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.W))
        {
            float tx = transform.forward.x;
            float tz = transform.forward.z;
            Vector3 forward = new Vector3(tx, 0, tz);

            transform.position += speed * Time.deltaTime * forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            float tx = transform.forward.x;
            float tz = transform.forward.z;
            Vector3 backward = new Vector3(tx, 0, tz);

            transform.position -= speed * Time.deltaTime * backward;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLocked = !cursorLocked;
            if (cursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

    }

}