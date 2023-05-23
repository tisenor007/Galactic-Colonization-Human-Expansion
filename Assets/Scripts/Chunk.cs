using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    List<Vector2> uvs = new List<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        int vertexIndex = 0;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (int p = 0; p < 6; p++)
        {
            for (int i = 0; i < 6; i++)
            {
                int triangleIndex = VoxelData.voxelTriangles[p, i];
                vertices.Add(VoxelData.voxelVertices[triangleIndex]); //populates vertices list
                triangles.Add(vertexIndex); //populates triangles

                uvs.Add(VoxelData.voxelUvs[i]);

                vertexIndex++; //progresses to make sure triangles are added in the proper order

            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

    }

    
    
}
