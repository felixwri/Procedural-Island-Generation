using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class InstancedPrefab
{
    private MeshFilter filter;
    private MeshRenderer renderer;
    private List<Matrix4x4> matrices = new List<Matrix4x4>();

    public void Init(GameObject prefab)
    {
        filter = prefab.GetComponent<MeshFilter>();
        renderer = prefab.GetComponent<MeshRenderer>();

        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
        {
            renderer.sharedMaterials[i].enableInstancing = true;
        }
    }

    public void Add(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        matrices.Add(Matrix4x4.TRS(position, rotation, scale));
    }

    public void Clear()
    {
        matrices.Clear();
    }

    public int Count()
    {
        return matrices.Count;
    }

    public void Update()
    {
        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
        {
            Graphics.DrawMeshInstanced(filter.sharedMesh, i, renderer.sharedMaterials[i], matrices);
        }
    }
}


public class Instancer : MonoBehaviour
{
    public GameObject firPrefab;
    public GameObject oakPrefab;
    public GameObject bushPrefab;

    private readonly InstancedPrefab fir = new();
    private readonly InstancedPrefab oak = new();
    private readonly InstancedPrefab bush = new();

    // Start is called before the first frame update
    void Start()
    {
        fir.Init(firPrefab);
        oak.Init(oakPrefab);
        bush.Init(bushPrefab);
    }

    public void AddFir(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        fir.Add(position, rotation, scale);
    }

    public void AddOak(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        oak.Add(position, rotation, scale);
    }


    public void AddBush(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        bush.Add(position, rotation, scale);
    }

    public void Clear()
    {
        //Debug.Log("Clearing martices");
        fir.Clear();
        oak.Clear();
        bush.Clear();
    }

    public void Log()
    {
        Debug.Log("Fir Trees Matrices: " + fir.Count());
        Debug.Log("Oak Trees Matrices: " + oak.Count());
        Debug.Log("Bush Matrices: " + bush.Count());
    }

    void Update()
    {
        if (fir == null || oak == null || bush == null) return;
        fir.Update();
        oak.Update();
        bush.Update();
    }
}
