using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//script from https://gist.github.com/ya7gisa0/742bf24d5edf1e73b971e14a2553ad4e

public class ComputeParticles : MonoBehaviour
{

    private Vector2 cursorPos;

    // struct
    struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
    }

    private const int SIZE_PARTICLE = 24; 

    public bool m_render = false;
    public int particleCount = 1000000;

    public Vector3  m_acceleration;
    public float  m_noiseAmp;
    public float  m_noiseFreq;
    public float  m_noiseScroll;

    public Material material;

    public ComputeShader computeShader;

    private int mComputeShaderKernelID;

    public ComputeBuffer particleBuffer;



    private const int WARP_SIZE = 256; // TODO?

    public int mWarpCount; // TODO?

    //public ComputeShader shader;

    // Use this for initialization
    void Start()
    {
        InitComputeShader();
    }

    void InitComputeShader()
    {
        mWarpCount = Mathf.CeilToInt((float)particleCount / WARP_SIZE);

        // initialize the particles
        Particle[] particleArray = new Particle[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            particleArray[i].position.x = Random.value;
            particleArray[i].position.y = Random.value;
            particleArray[i].position.z = Random.value;

            particleArray[i].velocity = Random.onUnitSphere * 0.1f;
        }

        // create compute buffer
        particleBuffer = new ComputeBuffer(particleCount, SIZE_PARTICLE);

        particleBuffer.SetData(particleArray);

        // find the id of the kernel
        mComputeShaderKernelID = computeShader.FindKernel("CSParticle");

        // bind the compute buffer to the shader and the compute shader
        computeShader.SetBuffer(mComputeShaderKernelID, "particleBuffer", particleBuffer);
        material.SetBuffer("particleBuffer", particleBuffer);
    }

    void OnRenderObject()
    {
        if (m_render)
        {
            material.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Points, 1, particleCount);
        }
    }

    void OnDestroy()
    {
        if (particleBuffer != null)
            particleBuffer.Release();
    }

    // Update is called once per frame
    void Update()
    {

        // Send datas to the compute shader
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.SetFloat("Time", Time.time);
        computeShader.SetVector("acceleration", m_acceleration);
        computeShader.SetVector("noiseParameters", new Vector4(m_noiseAmp, m_noiseFreq, m_noiseScroll));

        // Update the Particles
        computeShader.Dispatch(mComputeShaderKernelID, mWarpCount, 1, 1);
    }

    void OnGUI()
    {
        // GUILayout.BeginArea(new Rect(20, 20, 250, 120));
        // GUILayout.Label("Screen pixels: " + c.pixelWidth + ":" + c.pixelHeight);
        // GUILayout.Label("Mouse position: " + mousePos);
        // GUILayout.Label("World position: " + p.ToString("F3"));
        // GUILayout.EndArea();
    }
}