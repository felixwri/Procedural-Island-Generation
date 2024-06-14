using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enableUi : MonoBehaviour
{
    public bool EnableUI;
    public GameObject UICanvas;
    public CameraMovement PlayerController; 
    // Start is called before the first frame update
    void Start()
    {
        if(EnableUI == false){
            UICanvas.SetActive(false);
            PlayerController.enableMovement = true;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
