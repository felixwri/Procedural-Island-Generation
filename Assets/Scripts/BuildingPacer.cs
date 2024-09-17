
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlaceBuilding : MonoBehaviour
{
    public InputController inputController;
    public Island world;
    public Selector selector;

    private Vector3 startingPosition;
    private bool active = true;
    public GameObject building;
    private List<GameObject> prefabs;


    void Start()
    {
        prefabs = new List<GameObject>();
        inputController.OnLeftMouseClick += OnLeftMouseClick;
        inputController.OnMouseHover += OnMouseHover;
        inputController.Rotate += Rotate;

        selector.SetSize(3, 2);
    }


    private void OnLeftMouseClick(Vector3 position)
    {
        if (!active) return;
        Debug.Log("Clicked on " + position);
        startingPosition = Snap(position);
        selector.Translate(Snap(position));

        int rotation = selector.GetRotation();

        Vector3 translationOffset = new(0, 0, 0);

        switch (rotation)
        {
            case 90:
                translationOffset = new Vector3(0, 0, 3);
                break;
            case 180:
                translationOffset = new Vector3(3, 0, 2);
                break;
            case 270:
                translationOffset = new Vector3(2, 0, 0);
                break;
        }

        if (selector.inValidPlacement) return;

        GameObject prefab = Instantiate(building, startingPosition + translationOffset, Quaternion.Euler(-90, 180 + rotation, 0));
        prefabs.Add(prefab);

        float lowestPoint = Mathf.Infinity;
        Quad[] quads = selector.ConvertToQuads();
        for (int i = 0; i < quads.Length; i++)
        {
            world.GetQuad(quads[i].bottomLeft).isBuilding = true;
            Vector3 vert = world.GetVertice(quads[i].bottomLeft);
            if (vert.y < lowestPoint)
            {
                lowestPoint = vert.y;
            }
        }

        for (int i = 0; i < quads.Length; i++)
        {
            Vector3 vertice = world.GetVertice(quads[i].bottomLeft);
            vertice.y = lowestPoint;
            world.SetVertice(quads[i].bottomLeft, vertice);
        }
        world.UpdateMeshVertices();
    }

    private void Rotate()
    {
        selector.Rotate();
    }

    private Vector3 Snap(Vector3 v)
    {
        return new Vector3(Mathf.Floor(v.x), Mathf.Round(v.y * 2) / 2, Mathf.Floor(v.z));

    }

    private void OnMouseHover(Vector3 position)
    {
        if (!active) return;
        selector.Translate(Snap(position));
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
