using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject playerCamera;
    private Transform player;
    public float mouseSensitivity;
    public float playerSpeed ;
    private float inputX = 0f;
    private float inputY = 0f;
    // Start is called before the first frame update
    void Start()
    {
        player = playerCamera.transform;
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
        
    }

    // Update is called once per frame
    void Update()
    {
        float xMovement =  Input.GetAxis("Horizontal");
        float yMovement = Input.GetAxis("Vertical");
        print(xMovement);
        Vector3 move= new Vector3(xMovement, 0, yMovement);
        move = Camera.main.transform.TransformDirection(move);
        player.position += (move*playerSpeed);
        print(player.position);

        if(Input.GetMouseButton(1)){
            changeCamera();

        }

    }

    void changeCamera(){
            inputX = inputX -Input.GetAxis("Mouse X")*mouseSensitivity;
            inputY =  Mathf.Clamp(inputY - Input.GetAxis("Mouse Y")*mouseSensitivity, -90f, 90f);
            player.rotation = Quaternion.Euler(inputY,-1*(inputX),0);
    }
}
