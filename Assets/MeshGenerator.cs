using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;
    Color[] colors;


    public int xSize = 40;
    public int zSize = 40;

    public float perlinScaleOne = 0.3f;
    public float smallPerlinDampener = 0.8f;

    public float perlinScaleTwo = 0.2f;
    public float perlinTwoOffset = 100;

    public float logTransform = 1f;
    public float rootTransform = 1f;

    public float terrainMaxHeight = 2f;
    public float waterLevel = 0.2f;

    public Gradient gradient;

    float minHeight;
    float maxHeight;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        UpdateMesh();

    }

    public Vector3Int GetVertice(int x, int z)
    {
        return Vector3Int.RoundToInt(vertices[z * (xSize + 1) + x]);
    }

    public bool IsWater(int x, int z)
    {
        return vertices[z * (xSize + 1) + x].y < waterLevel;
    }

    float[] FalloffMap()
    {
        float[] points = new float[(xSize + 1) * (zSize + 1)];
        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float xCoord = x / (float)xSize * 2 - 1;
                float zCoord = z / (float)zSize * 2 - 1;
                float value = Mathf.Max(Mathf.Abs(xCoord), Mathf.Abs(zCoord));
                points[z * (xSize + 1) + x] = (1 - value);
            }
        }

        return points;
    }

    // Update is called once per frame
    void CreateShape()
    {
        minHeight = Mathf.Infinity;
        maxHeight = -Mathf.Infinity;

        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        float[] falloffPoints = FalloffMap();

        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {

                float y = Mathf.PerlinNoise(x * perlinScaleOne, z * perlinScaleOne) * smallPerlinDampener;
                float height = Mathf.PerlinNoise(x * perlinScaleTwo + perlinTwoOffset, z * perlinScaleTwo);

                height = Mathf.Sqrt(height) * rootTransform;

                height *= terrainMaxHeight;


                y *= height;

                y *= falloffPoints[i];

                if (y < waterLevel) y = waterLevel;

                vertices[i] = new Vector3(x, y, z);

                if (y > maxHeight) maxHeight = y;
                if (y < minHeight) minHeight = y;
                i++;
            }
        }


        triangles = new int[xSize * zSize * 6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {

                triangles[tris + 0] = vert;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        uvs = new Vector2[vertices.Length];
        colors = new Color[vertices.Length];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float height = Mathf.InverseLerp(minHeight, maxHeight, vertices[i].y);
                colors[i] = gradient.Evaluate(height);

                uvs[i] = new Vector2((float)x / xSize, (float)z / zSize);
                i++;
            }
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.uv = uvs;

        mesh.RecalculateNormals();

        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
