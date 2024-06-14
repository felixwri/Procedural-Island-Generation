using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGameScript : MonoBehaviour
{
    public GameObject NewGameCanvas;

    public void Start(){
        NewGameCanvas.SetActive(false);

    }



    public void canvasActivator(){
        if (NewGameCanvas.activeSelf){
                NewGameCanvas.SetActive(false);
        }
        else{
            NewGameCanvas.SetActive(true);
        }
    }
}
