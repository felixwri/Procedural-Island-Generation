
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceBuilding : MonoBehaviour
{
    public InputController inputController;
    public Island world;
    public GameObject SelectionBox;
    private Vector3 startingPosition;
    private bool active = true;
    public GameObject building;
    private List<GameObject> prefabs;

    //private List<GameObject> buildings;
    //public GameObject building;

    void Start()
    {
        prefabs = new List<GameObject>();
        inputController.OnLeftMouseClick += OnLeftMouseClick;
        inputController.OnMouseHover += OnMouseHover;

    }


    private void OnLeftMouseClick(Vector3 position)
    {
        if (!active) return;
        Debug.Log("Clicked on " + position);
        startingPosition = Snap(position);


        GameObject prefab = Instantiate(building, startingPosition, Quaternion.Euler(-90, 0, 0));
        prefabs.Add(prefab);
    }

    private Vector3 Snap(Vector3 v)
    {
        return new Vector3(Mathf.Floor(v.x), Mathf.Round(v.y * 2) / 2, Mathf.Floor(v.z));

    }

    private void OnMouseHover(Vector3 position)
    {
        if (!active) return;
        SelectionBox.transform.position = Snap(position);
    }

    void OnApplicationQuit()
    {
        if (prefabs == null) return;
        for (int i = 0; i < prefabs.Count; i++)
        {
            Destroy(prefabs[i]);
        }
        prefabs = null;
    }
}
