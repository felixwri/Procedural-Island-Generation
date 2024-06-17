using System;
using System.Collections.Generic;
using System.Linq;
using Voronoi;
using UnityEngine;


public class Point
{
    public float x;
    public float y;
    public float z;
    public int id = 0;

    public Point(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Point(Vector3 vector)
    {
        this.x = vector.x;
        this.y = vector.y;
        this.z = vector.z;
    }

    public static implicit operator Vector3(Point p)
    {
        return new Vector3(p.x, p.y, p.z);
    }

    public static implicit operator Point(Vector3 v)
    {
        return new Point(v);
    }
}

public class Island : MonoBehaviour
{
    Mesh mesh;
    GameObject underside;

    Vector3[] vertices;
    Vector3[] undersideVertices;

    Point[] points;
    Tile[] tiles;

    Vector2[] uvs;

    int[] triangles;
    int[] undersideTriangles;

    Color[] colors;

    public Instancer instancer;

    public int seed = 0;

    public int xSize = 40;
    public int zSize = 40;

    public int heightDelta = 2;

    public float perlinScaleOne = 0.05f;
    public float terrainMaxHeight = 25f;

    public float waterLevel = 2f;

    public Gradient gradient;

    public float treeNoise = 1f;
    public float firAmount = 0.1f;
    public float oakAmount = 0.1f;

    VoronoiNoise DebugNoise = new VoronoiNoise(0, 0.008f);

    // List<Point> DebugPoints = new List<Point>();

    float[] falloffMap;

    float minHeight;
    float maxHeight;

    // Start is called before the first frame update
    void Start()
    {
        DebugNoise = new VoronoiNoise(seed, 0.008f);
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        Generate();
        UpdateMesh();
    }

    public void OnValidate()
    {
        if (mesh == null)
        {
            return;
        }
        DebugNoise = new VoronoiNoise(seed, 0.008f);
        instancer.Clear();
        Generate();
        UpdateMesh();
    }


    public Vector3 GetVertice(int x, int z) => vertices[z * (xSize + 1) + x];

    public Point GetPoint(int x, int z) => points[z * (xSize + 1) + x];

    public Tile GetTile(int x, int z) => tiles[z * zSize + x];

    public bool IsWater(int x, int z) => vertices[z * (xSize + 1) + x].y < waterLevel;

    /// <summary>
    /// Creates a transition between the edge of the map and the center.
    /// Uses a funky equation to determine the falloff curve
    /// </summary>
    /// <returns>An array of floats between 0 and 1</returns>
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

                // Funky equation
                value = 1 / (1 + Mathf.Pow((value * 4) / (1 - value), -3f));

                if (value > 1) value = 1f;
                points[z * (xSize + 1) + x] = value;
            }
        }

        return points;
    }

    /// <summary>
    /// Builds the vertices and the triangles of both the top mesh and the underside mesh.
    /// </summary>
    void Generate()
    {
        minHeight = Mathf.Infinity;
        maxHeight = -Mathf.Infinity;

        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        undersideVertices = new Vector3[vertices.Length];
        points = new Point[vertices.Length];

        uvs = new Vector2[vertices.Length];

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

                // Flattens the terrain above the waterlevel
                if (y > waterLevel)
                {
                    y = waterLevel + (y - waterLevel) * 0.4f;
                }

                y = Mathf.Round(y * 2) / 2;

                if (y < waterLevel) y = waterLevel;

                vertices[i] = new Vector3(x, y, z);
                undersideVertices[i] = new Vector3(x, (-(y - waterLevel * 2) * 3) - (waterLevel * 2), z);

                uvs[i] = new Vector2((float)x / xSize, (float)z / zSize);

                points[i] = new Point(x, y, z);

                i++;

                if (y < minHeight) minHeight = y;
                if (y > maxHeight) maxHeight = y;
            }
        }

        GenerateMesh();

        Seperator();

        ModifyVerticies();

        GenerateFeatures();

        Debug.Log("Tiles: " + tiles.Count());
    }

    /// <summary>
    /// Applies on offset based on the points id.
    /// </summary>
    void ModifyVerticies()
    {

        for (int i = 0; i < points.Length; i++)
        {
            Point point = points[i];
            int vertexIndex = (int)point.z * (xSize + 1) + (int)point.x;
            float yOffset = (point.id % 10) * heightDelta;
            undersideVertices[vertexIndex].y -= yOffset;
            vertices[vertexIndex].y -= yOffset;
        }
    }

    void GenerateMesh()
    {
        triangles = new int[xSize * zSize * 6];
        undersideTriangles = new int[xSize * zSize * 6];
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

                undersideTriangles[tris + 0] = vert;
                undersideTriangles[tris + 1] = vert + 1;
                undersideTriangles[tris + 2] = vert + xSize + 1;

                undersideTriangles[tris + 3] = vert + 1;
                undersideTriangles[tris + 4] = vert + xSize + 2;
                undersideTriangles[tris + 5] = vert + xSize + 1;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    /// <summary>
    /// Places the foliage on the terrain based on the perlin noise. <br/>
    /// Creates a tile for each vertice.
    /// </summary>
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

                Point point = points[z * (xSize + 1) + x];

                float yOffset = (point.id % 10) * heightDelta;

                float normalisedHeight = y + yOffset;

                EvaluateColor(x, normalisedHeight, z, i, point.id);

                if (point.id == 0)
                {
                    tile.isWater = true;
                }
                else if (normalisedHeight < 12)
                {
                    float tn = Mathf.PerlinNoise(x * treeNoise, z * treeNoise);
                    if (tn > oakAmount)
                    {
                        if (UnityEngine.Random.Range(0, 100) > 70)
                        {
                            float rh = UnityEngine.Random.Range(120f, 200f);
                            Vector3 scale = new Vector3(rh, rh, rh);

                            float xOffset = x + UnityEngine.Random.Range(-0.5f, 0.5f);
                            float zOffset = z + UnityEngine.Random.Range(-0.5f, 0.5f);

                            instancer.AddOak(new Vector3(xOffset, y, zOffset), Quaternion.Euler(-90, 0, 0), scale);
                            tile.containsTree = true;
                        }

                    }
                    else if (tn < firAmount)
                    {
                        if (UnityEngine.Random.Range(0, 100) > 70)
                        {
                            float rh = UnityEngine.Random.Range(120f, 200f);
                            Vector3 scale = new Vector3(rh, rh, rh);

                            float xOffset = x + UnityEngine.Random.Range(-0.5f, 0.5f);
                            float zOffset = z + UnityEngine.Random.Range(-0.5f, 0.5f);

                            instancer.AddFir(new Vector3(xOffset, y, zOffset), Quaternion.Euler(-90, 0, 0), scale);
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


    /// <summary>
    /// Uses a breadth first seach to find the islands.
    /// </summary>
    void Seperator()
    {
        int islands = 0;

        for (int j = 0; j < zSize; j++)
        {
            for (int i = 0; i < xSize; i++)
            {
                Point point = points[j * (xSize + 1) + i];
                if (point.id == 0 && point.y > waterLevel)
                {
                    int x = i;
                    int z = j;

                    islands++;

                    int id = UnityEngine.Random.Range(1000, 9999);

                    point.id = id;

                    List<Point> unvisitedPoints = new List<Point>();
                    unvisitedPoints.Add(point);

                    while (unvisitedPoints.Count > 0)
                    {
                        Point current = unvisitedPoints[0];
                        unvisitedPoints.RemoveAt(0);

                        if (current.x > 0)
                        {
                            Point left = GetPoint((int)current.x - 1, (int)current.z);
                            if (left.id == 0 && left.y > waterLevel)
                            {
                                left.id = id;
                                unvisitedPoints.Add(left);
                            }
                        }
                        if (current.x < xSize)
                        {
                            Point right = GetPoint((int)current.x + 1, (int)current.z);
                            if (right.id == 0 && right.y > waterLevel)
                            {
                                right.id = id;
                                unvisitedPoints.Add(right);
                            }
                        }
                        if (current.z > 0)
                        {
                            Point bottom = GetPoint((int)current.x, (int)current.z - 1);
                            if (bottom.id == 0 && bottom.y > waterLevel)
                            {
                                bottom.id = id;
                                unvisitedPoints.Add(bottom);
                            }
                        }
                        if (current.z < zSize)
                        {
                            Point top = GetPoint((int)current.x, (int)current.z + 1);
                            if (top.id == 0 && top.y > waterLevel)
                            {
                                top.id = id;
                                unvisitedPoints.Add(top);
                            }
                        }
                        if (current.x > 0 && current.z < zSize)
                        {
                            Point topLeft = GetPoint((int)current.x - 1, (int)current.z + 1);
                            if (topLeft.id == 0 && topLeft.y > waterLevel)
                            {
                                topLeft.id = id;
                                unvisitedPoints.Add(topLeft);
                            }
                        }
                        if (current.x < xSize && current.z < zSize)
                        {
                            Point topRight = GetPoint((int)current.x + 1, (int)current.z + 1);
                            if (topRight.id == 0 && topRight.y > waterLevel)
                            {
                                topRight.id = id;
                                unvisitedPoints.Add(topRight);
                            }
                        }
                    }
                };
            }
        }
        Debug.Log("Number of islands: " + islands);

        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                int maxId = 0;

                if (x > 0)
                {
                    Point left = GetPoint(x - 1, z);
                    if (left.y > waterLevel)
                    {
                        maxId = Mathf.Max(maxId, left.id);
                    }
                }
                if (x < xSize)
                {
                    Point right = GetPoint(x + 1, z);
                    if (right.y > waterLevel)
                    {
                        maxId = Mathf.Max(maxId, right.id);
                    }
                }
                if (z > 0)
                {
                    Point bottom = GetPoint(x, z - 1);
                    if (bottom.y > waterLevel)
                    {
                        maxId = Mathf.Max(maxId, bottom.id);
                    }
                }
                if (z < zSize)
                {
                    Point top = GetPoint(x, z + 1);
                    if (top.y > waterLevel)
                    {
                        maxId = Mathf.Max(maxId, top.id);
                    }
                }
                if (x > 0 && z < zSize)
                {
                    Point topLeft = GetPoint(x - 1, z + 1);
                    if (topLeft.y > waterLevel)
                    {
                        maxId = Mathf.Max(maxId, topLeft.id);
                    }
                }
                if (x < xSize && z < zSize)
                {
                    Point topRight = GetPoint(x + 1, z + 1);
                    if (topRight.y > waterLevel)
                    {
                        maxId = Mathf.Max(maxId, topRight.id);
                    }
                }
                if (x > 0 && z > 0)
                {
                    Point bottomLeft = GetPoint(x - 1, z - 1);
                    if (bottomLeft.y > waterLevel)
                    {
                        maxId = Mathf.Max(maxId, bottomLeft.id);
                    }
                }
                if (x < xSize && z > 0)
                {
                    Point bottomRight = GetPoint(x + 1, z - 1);
                    if (bottomRight.y > waterLevel)
                    {
                        maxId = Mathf.Max(maxId, bottomRight.id);
                    }
                }

                if (maxId != 0)
                {
                    points[z * (xSize + 1) + x].id = maxId;
                }
            }
        }
    }

    /// <summary>
    /// Evaluates the color of the vertex based on the height and the perlin noise.
    /// </summary>
    private void EvaluateColor(int x, float y, int z, int i, int id)
    {
        float idColorOffset = id % 10;
        idColorOffset /= 20;

        int index = z * (xSize + 1) + x;

        Color land = new Color(0.1f, 0.6f, 0.2f, 1f);
        Color arid = new Color(0.3f, 0.6f, 0.2f, 1f);
        Color green = new Color(0.1f, 0.55f + idColorOffset, 0.25f, 1f);
        Color shore = new Color(0.6f, 0.6f, 0.3f, 1f);
        Color mountain = new Color(0.35f, 0.32f, 0.3f, 1f);

        if (y > 8)
        {
            if (y > 16)
            {
                colors[index] = Color.white;
            }
            else
            {
                colors[index] = mountain;
            }
        }
        else
        {
            colors[index] = green;
        }
    }

    /// <summary>
    /// Applys the generated mesh to the mesh and underside mesh
    /// </summary>
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

    /// <summary>
    /// Updates the vertices of the underside mesh. Will generate the underside GameObject if it does not exist.
    /// </summary>
    void GenerateUnderside()
    {
        Color[] underSideColors = new Color[undersideVertices.Length];
        for (int i = 0; i < underSideColors.Length; i++)
        {
            underSideColors[i] = new Color(0.1f, 0.1f, 0.1f, 1f);
        }

        if (underside == null)
        {
            underside = new GameObject("UnderSide");
            underside.transform.parent = transform;

            underside.AddComponent<MeshFilter>();
            underside.AddComponent<MeshRenderer>();
        }

        Mesh newMesh = new()
        {
            vertices = undersideVertices,
            triangles = undersideTriangles,
            colors = underSideColors
        };

        underside.GetComponent<MeshFilter>().mesh = newMesh;
        underside.GetComponent<MeshRenderer>().material = transform.GetComponent<MeshRenderer>().material;


        newMesh.RecalculateNormals();

        underside.transform.position = new Vector3(0, 0, 0);
    }
}