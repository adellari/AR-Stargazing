using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

[ExecuteAlways]
public class MeshInverter : MonoBehaviour
{
    [SerializeField]
    private bool inverted = false;
    
    public bool Inverted
    {
        get
        {
            return inverted;
        }

        set
        {
                Invert();
            inverted = value;
            
        }
    }

    void Invert()
    {
        Mesh mesh = this.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
            normals[i] = -1 * normals[i];
        mesh.normals = normals;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] tris = mesh.GetTriangles(i);
            for (int j = 0; j < tris.Length; j += 3)
            {
                //swap order of tri vertices
                int temp = tris[j];
                tris[j] = tris[j + 1];
                tris[j + 1] = temp;
            }
            mesh.SetTriangles(tris, i);
        }
        Debug.Log("inverted mesh");
    }

    private void Start()
    {
        Invert();
    }
}
