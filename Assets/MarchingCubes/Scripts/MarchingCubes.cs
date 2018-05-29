using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// based on https://github.com/pavelkouril/unity-marching-cubes-gpu

public class MarchingCubes : MonoBehaviour
{

    [SerializeField, Range(0, 1)]
    public float m_isoLevel = 0.5f;

    [SerializeField]
    private MeshFilter m_meshFilter;

    public Material mat;
    public Material surfaceMat;
    public ComputeShader MarchingCubesCS;
    private int kernelMC;

    private ComputeBuffer appendVertexBuffer;
    private ComputeBuffer argBuffer;
    private Mesh m_mesh;
    private const int NUM_TRIS = 3000000;
    private int trisToDraw;
    private int m_resolution;


    public void Init(int res)
    {
        m_resolution = res;
        kernelMC = MarchingCubesCS.FindKernel("MarchingCubes");

        appendVertexBuffer = new ComputeBuffer(
            (m_resolution - 1) * (m_resolution - 1) * (m_resolution - 1) * 5, sizeof(float) * 6,
            ComputeBufferType.Append);

        argBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);

        MarchingCubesCS.SetInt("_gridSize", m_resolution);
        MarchingCubesCS.SetFloat("_isoLevel", m_isoLevel);

        MarchingCubesCS.SetBuffer(kernelMC, "triangleRW", appendVertexBuffer);

        surfaceMat.SetBuffer("_TriangleBuffer", appendVertexBuffer);

        MakeDummyMesh();

        m_meshFilter.mesh = m_mesh;
    }

    public void RunMarchingCubes(RenderTexture texture)
    {
        MarchingCubesCS.SetTexture(kernelMC, "_densityTexture", texture);
        appendVertexBuffer.SetCounterValue(0);

        MarchingCubesCS.Dispatch(kernelMC, m_resolution / 8, m_resolution / 8, m_resolution / 8);

        int[] args = new int[] { 0, 1, 0, 0 };
        argBuffer.SetData(args);

        ComputeBuffer.CopyCount(appendVertexBuffer, argBuffer, 0);

        argBuffer.GetData(args);
        trisToDraw = args[0];
        args[0] *= 3;
        argBuffer.SetData(args);

        Debug.LogFormat("Meshed {0} verts, {1} tris", args[0], args[0] / 3);
    }
    
    void MakeDummyMesh()
    {
        m_mesh = new Mesh();

        if ( NUM_TRIS * 3 > 64000)
            m_mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        var verts = new List<Vector3>();

        for (int i = 0; i < NUM_TRIS * 3; i++)
            verts.Add(Vector3.zero);

        m_mesh.SetVertices(verts);
        var indices = new int[NUM_TRIS * 3];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;

        m_mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        m_mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 2);
    }

    private void OnDestroy()
    {
        appendVertexBuffer.Release();
        argBuffer.Release();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));
    }

    [ContextMenu("printmesh")]
    public void PrintMesh()
    {
        Debug.Log(ObjExporterScript.MeshToString(ExportMesh()));
    }

    void Update()
    {
        surfaceMat.SetInt("numTris", trisToDraw);
    }

    // instanced way
    // void Update()
    // {
    //     int numMeshes = trisToDraw / NUM_TRIS + 1;
        
    //     Matrix4x4[] matrices = new Matrix4x4[numMeshes];

    //     for ( int i = 0; i < matrices.Length; i++)
    //     {
    //         matrices[i] = transform.localToWorldMatrix;
    //     }
    //     MaterialPropertyBlock props = new MaterialPropertyBlock();
    //     Graphics.DrawMeshInstanced(m_mesh, 0, surfaceMat, matrices, matrices.Length, null, UnityEngine.Rendering.ShadowCastingMode.On , true);
    // }

    public Mesh ExportMesh()
    {

        int[] args = new int[] { 0, 1, 0, 0 };
        argBuffer.SetData(args);
        ComputeBuffer.CopyCount(appendVertexBuffer, argBuffer, 0);
        argBuffer.GetData(args);
        int numTris = args[0];
        int numVerts = args[0] * 3;

        // 18 floats per Tri
        // x,y,z,nx,ny,nz
        // x,y,z,nx,ny,nz
        // x,y,z,nx,ny,nz

        float[] dataCPU = new float[numTris * 18];

        appendVertexBuffer.GetData(dataCPU);
        Mesh outmesh = new Mesh();
        outmesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        var indices = new int[numVerts];
        var verts = new List<Vector3>();

        for (int i = 0; i < numVerts; i++)
        {
            indices[i] = i;
        }

        for (int i = 0; i < numTris; i++)
        {
            int addr = i * 18;

            verts.Add(new Vector3(dataCPU[addr++], dataCPU[addr++], dataCPU[addr++]));
            addr += 3; // skip normal

            verts.Add(new Vector3(dataCPU[addr++], dataCPU[addr++], dataCPU[addr++]));
            addr += 3; // skip normal

            verts.Add(new Vector3(dataCPU[addr++], dataCPU[addr++], dataCPU[addr++]));
            addr += 3; // skip normal
        }

        outmesh.SetVertices(verts);
        outmesh.SetIndices(indices, MeshTopology.Triangles, 0);

        // MeshUtility.Optimize(outmesh);

        return outmesh;
    }


}