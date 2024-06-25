using System;
using System.Collections.Generic;
using System.Linq;
using Voronoi;
using UnityEngine;

public class Island : MonoBehaviour
{
    Mesh mesh;
    GameObject underside;

    Vector3[] vertices;
    Vector3[] undersideVertices;

    Point[] points;
    Quad[] quads;

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
    public float terrainHeightDampener = 0.5f;

    public float waterLevel = 2f;

    public Gradient gradient;

    public float treeNoise = 1f;
    public float firAmount = 0.1f;
    public float oakAmount = 0.1f;

    VoronoiNoise voronoiNoise = new VoronoiNoise(0, 0.008f);

    float[] falloffMap;

    float minHeight;
    float maxHeight;

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Random.InitState(seed);
        voronoiNoise = new VoronoiNoise(seed, 0.008f);
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
        UnityEngine.Random.InitState(seed);
        voronoiNoise = new VoronoiNoise(seed, 0.008f);
        instancer.Clear();
        Generate();
        UpdateMesh();
    }


    public Vector3 GetVertice(int x, int z) => vertices[z * (xSize + 1) + x];

    public Point GetPoint(int x, int z) => points[z * (xSize + 1) + x];

    public Quad GetTile(int x, int z) => quads[z * zSize + x];

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

                float riverSample = voronoiNoise.Sample2D(x, z) * 8;

                y *= riverSample;

                y *= terrainMaxHeight;

                y *= falloffMap[i];

                // Flattens the terrain above the waterlevel
                if (y > waterLevel)
                {
                    y = waterLevel + (y - waterLevel) * terrainHeightDampener;
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

        Debug.Log("quads: " + quads.Count());
    }

    /// <summary>
    /// Applies on offset based on the points id.
    /// This is what controls the height of the islands
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
    /// Creates a Quad for each vertice.
    /// </summary>
    void GenerateFeatures()
    {
        quads = new Quad[xSize * zSize];

        for (int i = 0, z = 0; z <= zSize - 1; z++)
        {
            for (int x = 0; x <= xSize - 1; x++)
            {

                Quad quad = new Quad(
                    GetVertice(x, z),
                    GetVertice(x, z + 1),
                    GetVertice(x + 1, z + 1),
                    GetVertice(x + 1, z)
                );

                float y = quad.bottomLeft.y;

                Point point = points[z * (xSize + 1) + x];

                float yOffset = (point.id % 10) * heightDelta;

                float normalisedHeight = y + yOffset;

                EvaluateColor(x, normalisedHeight, z, i, point.id);

                if (point.id == 0)
                {
                    quad.isWater = true;
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
                            quad.containsTree = true;
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
                            quad.containsTree = true;
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

                quads[i] = quad;
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

                    Debug.Log("Island id: " + id);

                    point.id = id;

                    List<Point> unvisitedPoints = new List<Point>();
                    unvisitedPoints.Add(point);

                    void VisitPoint(Point current, int xOffset, int zOffset)
                    {
                        // Prevent out of bounds of Points array
                        if (current.x + xOffset < 0 || current.x + xOffset > xSize || current.z + zOffset < 0 || current.z + zOffset > zSize)
                        {
                            return;
                        }

                        Point p = GetPoint((int)current.x + xOffset, (int)current.z + zOffset);
                        if (p.id == 0 && p.y > waterLevel)
                        {
                            p.id = id;
                            unvisitedPoints.Add(p);
                        }
                    }

                    while (unvisitedPoints.Count > 0)
                    {
                        Point current = unvisitedPoints[0];
                        unvisitedPoints.RemoveAt(0);

                        //left
                        VisitPoint(current, -1, 0);
                        //right
                        VisitPoint(current, 1, 0);
                        //bottom
                        VisitPoint(current, 0, -1);
                        //top
                        VisitPoint(current, 0, 1);

                        //top left
                        VisitPoint(current, -1, 1);
                        //top right
                        VisitPoint(current, 1, 1);
                        //bottom left
                        VisitPoint(current, -1, -1);
                        //bottom right
                        VisitPoint(current, 1, -1);

                        //far top left
                        VisitPoint(current, -2, 2);
                        //far top right
                        VisitPoint(current, 2, 2);
                        //far bottom left
                        VisitPoint(current, -2, -2);
                        //far bottom right
                        VisitPoint(current, 2, -2);
                    }
                };
            }
        }
        Debug.Log("Number of islands: " + islands);

        // Adds every point on the very edge of an island to the islands id
        int CheckPoint(int max, int x, int z)
        {
            if (x < 0 || x > xSize || z < 0 || z > zSize)
            {
                return max;
            }
            Point p = GetPoint(x, z);
            if (p.y > waterLevel)
            {
                max = Mathf.Max(max, p.id);
            }
            return max;
        }

        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                int maxId = 0;
                //left
                maxId = CheckPoint(maxId, x - 1, z);
                //right
                maxId = CheckPoint(maxId, x + 1, z);
                //bottom
                maxId = CheckPoint(maxId, x, z - 1);
                //top
                maxId = CheckPoint(maxId, x, z + 1);

                //top left
                maxId = CheckPoint(maxId, x - 1, z + 1);
                //top right
                maxId = CheckPoint(maxId, x + 1, z + 1);
                //bottom left
                maxId = CheckPoint(maxId, x - 1, z - 1);
                //bottom right
                maxId = CheckPoint(maxId, x + 1, z - 1);

                if (maxId != 0) points[z * (xSize + 1) + x].id = maxId;
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