using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instancer : MonoBehaviour
{
    public GameObject treePrefab;
    public GameObject bushPrefab;

    List<Matrix4x4> treeMatrices = new List<Matrix4x4>();
    List<Matrix4x4> bushMatrices = new List<Matrix4x4>();

    private MeshFilter treeMeshFilter;
    private MeshRenderer treeMeshRenderer;
    private MeshFilter bushMeshFilter;
    private MeshRenderer bushMeshRenderer;

    // Start is called before the first frame update
    void Start()
    {

        treeMeshFilter = treePrefab.GetComponent<MeshFilter>();
        treeMeshRenderer = treePrefab.GetComponent<MeshRenderer>();
        bushMeshFilter = bushPrefab.GetComponent<MeshFilter>();
        bushMeshRenderer = bushPrefab.GetComponent<MeshRenderer>();


        treeMeshRenderer.sharedMaterial.enableInstancing = true;

        for (int i = 0; i < treeMeshRenderer.sharedMaterials.Length; i++)
        {
            treeMeshRenderer.sharedMaterials[i].enableInstancing = true;
        }

        bushMeshRenderer.sharedMaterial.enableInstancing = true;

    }

    public void AddTree(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        treeMatrices.Add(Matrix4x4.TRS(position, rotation, scale));
    }

    public void AddBush(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        bushMatrices.Add(Matrix4x4.TRS(position, rotation, scale));
    }

    public void Clear()
    {
        Debug.Log("Clearing martices");
        treeMatrices.Clear();
        bushMatrices.Clear();
    }

    public void Log()
    {
        Debug.Log("Tree Matrices: " + treeMatrices.Count);
        Debug.Log("Bush Matrices: " + bushMatrices.Count);
    }

    void Update()
    {
        for (int i = 0; i < treeMeshRenderer.sharedMaterials.Length; i++)
        {
            Graphics.DrawMeshInstanced(treeMeshFilter.sharedMesh, i, treeMeshRenderer.sharedMaterials[i], treeMatrices);
        }

        Graphics.DrawMeshInstanced(bushMeshFilter.sharedMesh, 0, bushMeshRenderer.sharedMaterial, bushMatrices);
    }
}
