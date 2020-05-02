using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    void Awake()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Generated Mesh";

        List<Vector3> points = new List<Vector3>()
        {
            new Vector3(-1,1),
            new Vector3(1,1),
            new Vector3(-1,-1),
            new Vector3(1,-1)
        };

        int[] triIndices = new int[]
        {
            1,2,0,
            2,1,3
        };

        mesh.SetVertices(points);
        mesh.triangles = triIndices;

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
