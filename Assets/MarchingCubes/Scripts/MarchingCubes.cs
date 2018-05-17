using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// from https://github.com/pavelkouril/unity-marching-cubes-gpu


public class MarchingCubes : MonoBehaviour
{

    public struct Mesh
    {
        public Vector3[] verts;
        public int[] faces;
    }

    [SerializeField, Range(0, 1)]
    public float m_isoLevel = 0.5f;

    public Material mat;
    public ComputeShader MarchingCubesCS;

    private int kernelMC;

    private ComputeBuffer appendVertexBuffer;
    private ComputeBuffer argBuffer;


    private int m_resolution;
    public void Init(int res)
    {
        m_resolution = res;
        kernelMC = MarchingCubesCS.FindKernel("MarchingCubes");
        appendVertexBuffer = new ComputeBuffer((m_resolution - 1) * (m_resolution - 1) * (m_resolution - 1) * 5, sizeof(float) * 18, ComputeBufferType.Append);
        argBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);

        MarchingCubesCS.SetInt("_gridSize", m_resolution);
        MarchingCubesCS.SetFloat("_isoLevel", m_isoLevel);

        MarchingCubesCS.SetBuffer(kernelMC, "triangleRW", appendVertexBuffer);
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
        args[0] *= 3;
        argBuffer.SetData(args);

    }

    public void RenderProcedural()
    {
        mat.SetPass(0);
        mat.SetBuffer("triangles", appendVertexBuffer);
        mat.SetMatrix("model", transform.localToWorldMatrix);
        Graphics.DrawProceduralIndirect(MeshTopology.Triangles, argBuffer);
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
        Debug.Log(ObjExporterScript.MeshToString (ExportMesh()));

    }

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
        outmesh.faces = new int[numVerts];
        outmesh.verts = new Vector3[numVerts];

        for ( int i = 0; i < numVerts; i ++)
        {
            outmesh.faces[i] = i;
        }
        
        int vertIndex = 0;

        for ( int i = 0; i < numTris; i ++)
        {
            int addr = i * 18;

            outmesh.verts[vertIndex++] = new Vector3(dataCPU[addr++], dataCPU[addr++], dataCPU[addr++]);
            Vector3 n0 = new Vector3(dataCPU[addr++], dataCPU[addr++], dataCPU[addr++]);
            
            outmesh.verts[vertIndex++] = new Vector3(dataCPU[addr++], dataCPU[addr++], dataCPU[addr++]);
            Vector3 n1 = new Vector3(dataCPU[addr++], dataCPU[addr++], dataCPU[addr++]);
            
            outmesh.verts[vertIndex++] = new Vector3(dataCPU[addr++], dataCPU[addr++], dataCPU[addr++]);
            Vector3 n2 = new Vector3(dataCPU[addr++], dataCPU[addr++], dataCPU[addr++]);
        }

        return outmesh;
    }


}