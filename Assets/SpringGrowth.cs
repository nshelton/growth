using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpringGrowth : MonoBehaviour
{

    public MeshFilter m_meshFilter;

    public Mesh m_originalMesh;

    public float m_targetDistance = 0.1f;

    private List<List<int>> m_neighbors;



    List<Vector3> verts;

    int GetIndexOf(List<Vector3> list, Vector3 v)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if ((list[i] - v).sqrMagnitude < 0.0001)
                return i;
        }
        return -1;
    }

    void MergeVerts(Mesh mesh)
    {
        mesh.GetVertices(verts);
        var oldIndices = mesh.GetIndices(0);
        var newVerticesArray = new List<Vector3>();
        var oldToNewIndex = new int[mesh.GetIndexCount(0)];

        Debug.LogFormat("have {0} verts", verts.Count);

        for (int i = 0; i < oldToNewIndex.Length; i++)
        {
            var vertInQuestion = verts[oldIndices[i]];
            int newIndex = GetIndexOf(newVerticesArray, vertInQuestion);
            if (newIndex > -1)
            {
                oldToNewIndex[i] = newIndex;
            }
            else
            {
                oldToNewIndex[i] = newVerticesArray.Count;
                newVerticesArray.Add(vertInQuestion);
            }
        }

        mesh.SetVertices(newVerticesArray);
        mesh.SetIndices(oldToNewIndex, MeshTopology.Triangles, 0);

        Debug.LogFormat("have {0} verts after", newVerticesArray.Count);
    }

    void Start()
    {
        if (verts == null)
        {
            verts = new List<Vector3>();
        }

        m_neighbors = new List<List<int>>();


        var mesh = m_meshFilter.mesh;

        MergeVerts(mesh);

        mesh.UploadMeshData(false);

        var indices = mesh.GetIndices(0);
        var numVerts = mesh.vertexCount;

        for (int i = 0; i < numVerts; i++)
        {
            m_neighbors.Add(new List<int>());

            for (int j = 0; j < indices.Length; j += 3)
            {
                int i0 = indices[j + 0];
                int i1 = indices[j + 1];
                int i2 = indices[j + 2];

                if (i == i0)
                {
                    m_neighbors[i].Add(i1);
                    m_neighbors[i].Add(i2);
                }

                if (i == i1)
                {
                    m_neighbors[i].Add(i0);
                    m_neighbors[i].Add(i2);
                }

                if (i == i2)
                {
                    m_neighbors[i].Add(i1);
                    m_neighbors[i].Add(i0);
                }
            }
        }
    }

    [ContextMenu("subdivide")]
    void Subdivide()
    {
        m_meshFilter.mesh.GetVertices(verts);
        var oldIndices = m_meshFilter.mesh.GetIndices(0);
        var newIndices = new int[oldIndices.Length + 9];

        for (int i = 0; i < oldIndices.Length; i++)
        {
            newIndices[i] = oldIndices[i];
        }

        float numfaces = oldIndices.Length / 3;
        int face = (int)(Random.value * numfaces);

        // old tri vertid
        int a = oldIndices[face + 0];
        int b = oldIndices[face + 1];
        int c = oldIndices[face + 2];

        // new tri vertid
        int ab = verts.Count + 0;
        int bc = verts.Count + 1;
        int ca = verts.Count + 2;

        newIndices[a] = ab;
        newIndices[b] = bc;
        newIndices[c] = ca;
        
        int ii = oldIndices.Length;

        newIndices[ii++] = a;
        newIndices[ii++] = ab;
        newIndices[ii++] = ca;

        newIndices[ii++] = ab;
        newIndices[ii++] = b;
        newIndices[ii++] = bc;

        newIndices[ii++] = ca;
        newIndices[ii++] = bc;
        newIndices[ii++] = c;

        verts.Add((verts[a] + verts[b]) / 2);
        verts.Add((verts[b] + verts[c]) / 2);
        verts.Add((verts[c] + verts[a]) / 2);

        m_meshFilter.mesh.SetVertices(verts);

        m_meshFilter.mesh.SetIndices(newIndices, MeshTopology.Triangles, 0);

        // update Neighbors List with new connectivity 



}

    void Update()
    {
        Subdivide();

        m_meshFilter.mesh.GetVertices(verts);

        for (int i = 0; i < m_neighbors.Count; i++)
        {
            for (int j = 0; j < m_neighbors[i].Count; j++)
            {
                var neighbor = verts[m_neighbors[i][j]];

                Vector3 dir = (neighbor - verts[i]);

                float sign = Mathf.Sign(m_targetDistance * m_targetDistance - dir.sqrMagnitude);

                verts[i] -= sign * dir * 0.001f;
            }

            for (int j = 0; j < m_neighbors.Count; j++)
            {
                Vector3 dir = (verts[i] - verts[j]);

                if (dir.sqrMagnitude > 0.01)

                    verts[i] += 0.0001f * dir.normalized / (dir.sqrMagnitude);
            }
        }

        m_meshFilter.mesh.SetVertices(verts);
    }
}
