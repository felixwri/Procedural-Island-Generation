
using System.Collections.Generic;
using UnityEngine;


public class Selector : MonoBehaviour
{
    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    public Island island;

    public bool inValidPlacement = false;

    float[] xCoords;
    float[] zCoords;

    int rotation = 0;

    public float elevation = 0.2f;

    Vector2Int size = new(1, 1);
    Vector3 position = new(0, 0, 0);

    void Start()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }


    public void SetSize(int x, int z) => SetSize(new Vector2Int(x, z));
    public void SetSize(Vector2Int size)
    {
        this.size = size;
        Initialize();
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

    public Quad[] ConvertToQuads()
    {
        Quad[] quads = new Quad[size.x * size.y];

        for (int i = 0, z = 0; z < zCoords.Length - 1; z++)
        {
            for (int x = 0; x < xCoords.Length - 1; x++)
            {
                Vector3 bottomLeft = new Vector3(position.x + x, 0, position.z + z);
                Vector3 topLeft = new Vector3(position.x + x, 0, position.z + z + 1);
                Vector3 bottomRight = new Vector3(position.x + x + 1, 0, position.z + z);
                Vector3 topRight = new Vector3(position.x + x + 1, 0, position.z + z + 1);

                quads[i] = new Quad(bottomLeft, topLeft, topRight, bottomRight);
                i++;
            }
        }
        return quads;
    }

    public int GetRotation() => rotation;

    private void Initialize()
    {
        transform.position = new Vector3(position.x, position.y, position.z);

        inValidPlacement = false;

        mesh.Clear();

        xCoords = new float[size.x + 1];
        for (int i = 0; i < xCoords.Length; i++)
        {
            xCoords[i] = i;
        }
        zCoords = new float[size.y + 1];
        for (int i = 0; i < zCoords.Length; i++)
        {
            zCoords[i] = i;
        }

        CreateMesh();

        if (inValidPlacement)
        {
            SetColor(Color.red);
        }
        else
        {
            SetColor(Color.white);
        }
    }

    void CreateMesh()
    {
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

        for (int z = 0; z < zCoords.Length - 1; z++)
        {
            for (int x = 0; x < xCoords.Length - 1; x++)
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
    }

    void UpdateMeshPosition()
    {
        inValidPlacement = false;

        transform.position = new Vector3(position.x, position.y, position.z);

        Vector3[] vertices = mesh.vertices;
        int i = 0;
        for (int zi = 0; zi < zCoords.Length; zi++) // If zCoords is a List, use zCoords.Count
        {
            float z = zCoords[zi];
            for (int xi = 0; xi < xCoords.Length; xi++) // If xCoords is a List, use xCoords.Count
            {
                float x = xCoords[xi];
                Point point = island.GetPoint((int)(x + position.x), (int)(z + position.z));

                if (point.id == 0) inValidPlacement = true;

                if (zi < zCoords.Length - 1 && xi < xCoords.Length - 1)
                {
                    if (island.GetQuad((int)(x + position.x), (int)(z + position.z)).isBuilding) inValidPlacement = true;
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

    public void Rotate()
    {
        rotation += 90;
        if (rotation == 360) rotation = 0;
        Debug.Log("Rotation: " + rotation);

        int temp = size.x;
        size.x = size.y;
        size.y = temp;

        Initialize();
        UpdateMeshPosition();
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
            Initialize();
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
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(true);
    }
}
