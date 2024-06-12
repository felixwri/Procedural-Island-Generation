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
    public GameObject treePrefab;
    public GameObject bushPrefab;

    private readonly InstancedPrefab tree = new();
    private readonly InstancedPrefab bush = new();

    // Start is called before the first frame update
    void Start()
    {
        tree.Init(treePrefab);
        bush.Init(bushPrefab);
    }

    public void AddTree(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        tree.Add(position, rotation, scale);
    }

    public void AddBush(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        bush.Add(position, rotation, scale);
    }

    public void Clear()
    {
        Debug.Log("Clearing martices");
        tree.Clear();
        bush.Clear();
    }

    public void Log()
    {
        Debug.Log("Tree Matrices: " + tree.Count());
        Debug.Log("Bush Matrices: " + bush.Count());
    }

    void Update()
    {
        tree.Update();
        bush.Update();
    }
}
