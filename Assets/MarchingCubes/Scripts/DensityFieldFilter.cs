using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;


public class DensityFieldFilter : MonoBehaviour
{

    [SerializeField]
    public MarchingCubes m_marchingCubes;

    [SerializeField]
    public ComputeParticles m_particles;
    

    public int m_resolution;

    [SerializeField, Range(0,15)]
    public float m_param;

    public ComputeShader GeneratorCS;
    private int initKernel;
    private int advanceKernel;
    private int depositKernel;

    private RenderTexture sourceTexture;
    private RenderTexture destTexture;



    RenderTexture CreateTexture()
    {
        RenderTexture tex = new RenderTexture(
            m_resolution, m_resolution, m_resolution,
            RenderTextureFormat.RFloat, RenderTextureReadWrite.Default);

        tex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        tex.volumeDepth = m_resolution;

        tex.enableRandomWrite = true;

        return tex;
    }

    void Start()
    {
        sourceTexture = CreateTexture();
        destTexture = CreateTexture();

        initKernel = GeneratorCS.FindKernel("Initialize");
        advanceKernel = GeneratorCS.FindKernel("Advance");
        depositKernel = GeneratorCS.FindKernel("Deposit");

        GeneratorCS.SetInt("_gridSize", m_resolution);

        InitField();

        m_marchingCubes.Init(m_resolution);
    }

    public void InitField()
    {
        RunKernel(initKernel);
    }

    void SwapBuffers()
    {
        RenderTexture tmp = sourceTexture;
        sourceTexture = destTexture;
        destTexture = tmp;
    }

    private void Update()
    {
        SwapBuffers();

        RunKernel(advanceKernel);
        m_marchingCubes.RunMarchingCubes(destTexture);

        RunKernel(depositKernel);
    }

    private void OnRenderObject()
    {
        m_marchingCubes.RenderProcedural();
    }

    void RunKernel(int kernelName)
    {
        GeneratorCS.SetFloat("_param", m_param);

        GeneratorCS.SetFloat("_Time", Time.time);

        GeneratorCS.SetTexture(kernelName, "_gridIn", sourceTexture);
        GeneratorCS.SetTexture(kernelName, "_gridOut", destTexture);

        RenderTexture.active = destTexture;

        if (kernelName == depositKernel)
        {
            GeneratorCS.SetBuffer(kernelName, "_particleBuffer", m_particles.particleBuffer);
            GeneratorCS.Dispatch(kernelName, m_particles.mWarpCount, 1, 1);

        }
        else
        {
            GeneratorCS.Dispatch(kernelName, m_resolution / 8, m_resolution / 8, m_resolution / 8);
        }
        RenderTexture.active = null;
    }
}



[CustomEditor(typeof(DensityFieldFilter))]
public class DensityFieldFilterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DensityFieldFilter myScript = (DensityFieldFilter)target;

        if (GUILayout.Button("Reset"))
        {
            myScript.InitField();
        }
    }
}