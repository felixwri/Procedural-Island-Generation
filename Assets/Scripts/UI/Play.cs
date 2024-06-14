using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Play : MonoBehaviour
{
    public GameObject UICanvas;
    public CameraMovement PlayerController; 

    // Example: Deactivate the Canvas
    public void DeactivateCanvas()
    {
        UICanvas.SetActive(false);
        PlayerController.enableMovement = true;
    }
}
