using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Selector : MonoBehaviour
{
    Mesh mesh;
    GameObject selection;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Island island;

    public float elevation = 0.2f;

    Vector2Int size = new(1, 1);
    Vector3 position = new(0, 0, 0);

    public Selector(Island island)
    {
        this.island = island;
        mesh = new Mesh();

        selection = new GameObject("Selection Temp Object");
        meshFilter = selection.AddComponent<MeshFilter>();
        meshRenderer = selection.AddComponent<MeshRenderer>();
    }


    public void SetSize(int x, int z) => SetSize(new Vector2Int(x, z));
    public void SetSize(Vector2Int size)
    {
        this.size = size;
        BuildMesh();
    }

    public void SetPos(float x, float y, float z) => SetPos(new Vector3(x, y, z));
    public void SetPos(Vector3 pos)
    {
        position = pos;
        if (mesh != null)
        {
            UpdateMeshPosition();
        }
    }

    private void BuildMesh()
    {
        selection.transform.position = new Vector3(position.x, position.y, position.z);

        bool inValidPlacement = false;

        mesh.Clear();

        Vector3[] vertices = new Vector3[(size.x + 1) * (size.y + 1)];

        int[] triangles = new int[size.x * size.y * 6];

        for (int i = 0, z = 0; z <= size.y; z++)
        {
            for (int x = 0; x <= size.x; x++)
            {
                vertices[i] = new Vector3(x, 0, z);
                i++;
            }
        }

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < size.y; z++)
        {
            for (int x = 0; x < size.x; x++)
            {
                triangles[tris + 0] = vert;
                triangles[tris + 1] = vert + size.x + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + size.x + 1;
                triangles[tris + 5] = vert + size.x + 2;
                vert++;
                tris += 6;
            }
            vert++;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        if (inValidPlacement)
        {
            SetColor(Color.red);
        }
        else
        {
            SetColor(Color.white);
        }
    }

    void UpdateMeshPosition()
    {
        bool inValidPlacement = false;

        selection.transform.position = new Vector3(position.x, position.y, position.z);

        Vector3[] vertices = mesh.vertices;
        for (int i = 0, z = 0; z <= size.y; z++)
        {
            for (int x = 0; x <= size.x; x++)
            {
                Point point = island.GetPoint(x + (int)position.x, z + (int)position.z);

                if (point.id == 0)
                {
                    inValidPlacement = true;
                }

                float height = point.y - position.y;

                vertices[i] = new Vector3(x, height + elevation, z);
                i++;
            }
        }
        mesh.vertices = vertices;

        if (inValidPlacement)
        {
            SetColor(Color.red);
        }
        else
        {
            SetColor(Color.white);
        }
    }

    /// <summary>
    /// Updates the position of the selector.
    /// </summary>
    /// <param name="translation"></param>
    public void Translate(Vector3 translation)
    {
        if (position == translation) return;

        position = translation;
        if (mesh == null)
        {
            BuildMesh();
        }
        else
        {
            UpdateMeshPosition();
        }

    }

    public void SetColor(Color color)
    {
        meshRenderer.material.color = color;
    }

    public void Show()
    {
        selection.SetActive(true);
    }

    public void Hide()
    {
        selection.SetActive(false);
    }
}
