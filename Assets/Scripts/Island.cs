using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using Voronoi;
using UnityEngine;


public class Island : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    Tile[] tiles;

    Vector2[] uvs;
    int[] triangles;
    Color[] colors;

    public Instancer instancer;
    public int seed = 0;

    public int xSize = 40;
    public int zSize = 40;

    public float perlinScaleOne = 0.05f;

    public float terrainMaxHeight = 25f;
    public float waterLevel = 2f;

    private Color black = new Color(0, 0, 0, 1);
    private Color white = new Color(1, 1, 1, 1);


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

    public float ocCarverPos = 100;

    VoronoiNoise DebugNoise = new VoronoiNoise(0, 0.008f);

    float[] falloffMap;

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

    public void OnValidate()
    {
        if (mesh == null)
        {
            return;
        }
        instancer.Clear();
        CreateShape();
        UpdateMesh();
    }

    public Vector3 GetVertice(int x, int z)
    {
        return vertices[z * (xSize + 1) + x];
    }

    public Tile GenerateTile(int x, int z)
    {
        Tile tile = new Tile(
            GetVertice(x, z),
            GetVertice(x, z + 1),
            GetVertice(x + 1, z + 1),
            GetVertice(x + 1, z)
            );
        return tile;
    }

    public Tile GetTile(int x, int z)
    {
        Tile tile = tiles[z * zSize + x];
        return tile;
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
                float value = 1 - Mathf.Max(Mathf.Abs(xCoord), Mathf.Abs(zCoord));
                value = 1 / (1 + Mathf.Pow((value * 4) / (1 - value), -3f));
                if (value > 1) value = 1f;
                points[z * (xSize + 1) + x] = value;
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
        falloffMap = FalloffMap();

        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = Mathf.PerlinNoise((x + seed) * perlinScaleOne, (z + seed) * perlinScaleOne);

                float riverSample = DebugNoise.Sample2D(x, z) * 8;

                y *= riverSample;

                y *= terrainMaxHeight;

                y *= falloffMap[i];

                // float mountain = Mathf.PerlinNoise(100 + seed + x * 0.05f, seed + z * 0.05f);

                if (y > waterLevel)
                {
                    y = waterLevel + (y - waterLevel) * 0.4f;
                }

                // y += mountain;

                y = Mathf.Round(y * 2) / 2;

                EvaluateColor(x, y, z, i, colors);

                if (y < waterLevel) y = waterLevel;

                vertices[i] = new Vector3(x, y, z);
                i++;

                if (y < minHeight) minHeight = y;
                if (y > maxHeight) maxHeight = y;

            }

        }

        GenerateMesh();

        GenerateColors();

        GenerateFeatures();

        Debug.Log("Tiles: " + tiles.Count());
    }

    void GenerateMesh()
    {
        triangles = new int[xSize * zSize * 6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                if (vertices[vert].y <= waterLevel
                    && vertices[vert + xSize + 1].y <= waterLevel
                    && vertices[vert + 1].y <= waterLevel
                    && vertices[vert + xSize + 2].y <= waterLevel)
                {
                    vert++;
                    continue;
                }
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
    }


    void GenerateColors()
    {
        uvs = new Vector2[vertices.Length];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                uvs[i] = new Vector2((float)x / xSize, (float)z / zSize);
                i++;
            }
        }
    }

    void GenerateFeatures()
    {
        tiles = new Tile[xSize * zSize];

        for (int i = 0, z = 0; z <= zSize - 1; z++)
        {
            for (int x = 0; x <= xSize - 1; x++)
            {
                Tile tile = new Tile(
                    GetVertice(x, z),
                    GetVertice(x, z + 1),
                    GetVertice(x + 1, z + 1),
                    GetVertice(x + 1, z)
                );

                float y = tile.bottomLeft.y;

                if (y < waterLevel)
                {
                    tile.isWater = true;
                }

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
                            tile.containsTree = true;
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

                tiles[i] = tile;
                i++;
            }
        }

        instancer.Log();
    }

    private void DebugColor(int x, float y, int z, int i, Color[] colors)
    {
        // float noise = DebugNoise.Sample2D(x, z);
        // colors[i] = gradient.Evaluate(noise);

        // Color col = DebugNoise.Sample2DColor(x, z);
        // colors[i] = col;

        // if (noise < 0.3f)
        // {
        //     colors[i] = black;
        // }
        // else
        // {
        //     colors[i] = white;
        // }
    }

    private void EvaluateColor(int x, float y, int z, int i, Color[] colors)
    {
        if (y == waterLevel)
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

        GenerateUnderside();
    }

    void GenerateUnderside()
    {
        Vector3[] clonedVertices = new Vector3[vertices.Length];
        Array.Copy(vertices, clonedVertices, vertices.Length);

        int[] clonedTriangles = new int[triangles.Length];
        Array.Copy(triangles, clonedTriangles, triangles.Length);

        Color[] underSideColors = new Color[clonedVertices.Length];
        for (int i = 0; i < underSideColors.Length; i++)
        {
            underSideColors[i] = new Color(0.1f, 0.1f, 0.1f, 1f);
        }

        GameObject newObject = new GameObject("UnderSide");
        newObject.transform.parent = transform;

        newObject.AddComponent<MeshFilter>();
        newObject.AddComponent<MeshRenderer>();

        Mesh newMesh = new()
        {
            vertices = clonedVertices,
            triangles = clonedTriangles,
            colors = underSideColors
        };

        newObject.GetComponent<MeshFilter>().mesh = newMesh;

        newObject.GetComponent<MeshRenderer>().material = transform.GetComponent<MeshRenderer>().material;

        newMesh.RecalculateNormals();

        newObject.transform.position = new Vector3(0, 12, 0);
        newObject.transform.localScale = new Vector3(1, -5, 1);
    }
}