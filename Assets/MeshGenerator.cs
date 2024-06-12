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

    public Instancer instancer;

    public int xSize = 40;
    public int zSize = 40;

    public float perlinScaleOne = 0.05f;

    public float terrainMaxHeight = 25f;
    public float waterLevel = 2f;


    public Color sea = new Color(0.1f, 0.1f, 0.7f, 1f);
    public Color shallow = new Color(0.2f, 0.2f, 0.8f, 1f);
    public Color land = new Color(0.1f, 0.6f, 0.2f, 1f);
    public Color arid = new Color(0.3f, 0.6f, 0.2f, 1f);
    public Color green = new Color(0.1f, 0.55f, 0.25f, 1f);
    public Color shore = new Color(0.6f, 0.6f, 0.3f, 1f);
    public Color mountain = new Color(0.35f, 0.32f, 0.3f, 1f);

    public Gradient gradient;

    public float treeNoise = 1f;
    public float treeActivationAmount = 1f;

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

    // void OnValidate()
    // {
    //     if (mesh == null)
    //     {
    //         return;
    //     }
    //     CreateShape();
    //     UpdateMesh();
    // }

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
        colors = new Color[vertices.Length];

        float[] falloffPoints = FalloffMap();

        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {

                float y = Mathf.PerlinNoise(x * perlinScaleOne, z * perlinScaleOne);

                y *= terrainMaxHeight;

                y *= falloffPoints[i];

                y = Mathf.Round(y * 2) / 2;

                EvaluateColor(x, y, z, i, colors);

                // if (y < waterLevel) y = waterLevel;

                vertices[i] = new Vector3(x, y, z);
                i++;

                if (y > waterLevel)
                {
                    float tn = Mathf.PerlinNoise(x * treeNoise, z * treeNoise);
                    if (tn > treeActivationAmount)
                    {
                        if (UnityEngine.Random.Range(0, 100) > 70)
                        {
                            float rh = UnityEngine.Random.Range(120f, 200f);
                            Vector3 scale = new Vector3(rh, rh, rh);

                            float xOffset = x + UnityEngine.Random.Range(-0.5f, 0.5f);
                            float zOffset = z + UnityEngine.Random.Range(-0.5f, 0.5f);

                            instancer.AddTree(new Vector3(xOffset, y, zOffset), Quaternion.Euler(-90, 0, 0), scale);

                        }

                    }

                    if (UnityEngine.Random.Range(0, 100) > 90)
                    {
                        float rh = UnityEngine.Random.Range(90f, 110f);
                        Vector3 scale = new Vector3(rh, rh, rh);

                        float xOffset = x + UnityEngine.Random.Range(-0.5f, 0.5f);
                        float zOffset = z + UnityEngine.Random.Range(-0.5f, 0.5f);

                        instancer.AddBush(new Vector3(xOffset, y, zOffset), Quaternion.Euler(-90, 0, 0), scale);
                    }

                }

                if (y < minHeight) minHeight = y;
                if (y > maxHeight) maxHeight = y;

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
        // colors = new Color[vertices.Length];


        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float height = Mathf.InverseLerp(minHeight, maxHeight, vertices[i].y);
                // colors[i] = gradient.Evaluate(height);

                uvs[i] = new Vector2((float)x / xSize, (float)z / zSize);
                i++;
            }
        }
        instancer.Log();
    }

    private void EvaluateColor(int x, float y, int z, int i, Color[] colors)
    {
        float shallowWater = waterLevel - 1;
        if (y < shallowWater)
        {
            colors[i] = sea;
        }
        else if (y >= shallowWater && y < waterLevel)
        {
            colors[i] = shallow;
        }
        else if (y == waterLevel)
        {
            colors[i] = shore;
        }
        else if (y > waterLevel * 5)
        {
            colors[i] = mountain;
        }
        else
        {
            float noise = Mathf.PerlinNoise(100 + x * 0.05f, 100 + z * 0.05f);
            if (noise < 0.3f)
            {
                colors[i] = green;
            }
            else if (noise > 0.8f)
            {
                colors[i] = arid;
            }
            else
            {
                colors[i] = land;
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
