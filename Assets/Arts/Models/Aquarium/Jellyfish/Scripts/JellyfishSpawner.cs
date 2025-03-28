using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class JellyfishSpawner : MonoBehaviour
{
    public GameObject relativeObj;

    public static JellyfishSpawner instance;

    public bool optimizedAnimation = false;

    public GameObject objectPrefab;
    public int numberOfObjects;

    private List<RVO.Vector3> m_Goals;

    public float timeStep = 0.125f;
    public float neighborDist = 15.0f;
    public int maxNeighbors = 10;
    public float timeHorizon = 10.0f;
    public float radius = 1.5f;
    public float maxSpeed = 2.0f;

    public Color colorMin;
    public Color colorMax;

    public GameObject[] dangers;
    public float avoidDistance = 1.0f;

    private RVO.RVOSimulator m_Sim;

    private int m_MaxFrozen = 8;

    private Matrix4x4[] m_Matrices;
    private Vector4[] m_BaseColors;
    private Vector4[] m_Velocities;
    private Vector4[] m_AnimSpeeds;

    private Mesh m_Mesh;
    private Material[] m_Materials;

    private static int s_BaseColorBuffers = Shader.PropertyToID("_BaseColorBuffers");
    private static int s_VelocityBuffers = Shader.PropertyToID("_VelocityBuffers");
    private static int s_AnimSpeedBuffers = Shader.PropertyToID("_AnimSpeedBuffers");
    private static string s_OptimizedKeyWord = "_OPTIMIZED_ANIMATION_ON";

    private void Start()
    {
        Random.InitState(1000);

        instance = this;
        SetupScenario();
        PrepareRenderSetup();
    }

    private void FixedUpdate()
    {
        LogicUpdate();
        SpecialLogic();
    }

    private void OnDestroy() { instance = null; }

    private void PrepareRenderSetup()
    {
        m_Mesh = objectPrefab.GetComponent<MeshFilter>().sharedMesh;
        m_Materials = objectPrefab.GetComponent<MeshRenderer>().sharedMaterials;
    }

    private void SpecialLogic()
    {
        int count = m_Sim.getNumAgents();

        for (int i = 0; i < count; ++i)
        {
            RVO.Vector3 pos = m_Sim.getAgentPosition(i);

            foreach (GameObject d in dangers)
            {
                var p = new Vector3(pos.x(), pos.y(), pos.z());
                Vector3 dir = p - d.transform.position;
                float dist = dir.magnitude;
                float v = m_Sim.getAgentFreezed(i);
                if (dist < avoidDistance)
                {
                    if (v == 0)
                    {
                        m_Sim.setAgentFreezed(i, m_MaxFrozen);
                    }
                }
                else
                {
                    m_Sim.setAgentFreezed(i, Mathf.Max(v - 1, 0));
                }
            }
        }
    }

    private void LogicUpdate()
    {
        SetPreferredVelocities();
        m_Sim.doStep();

        int count = m_Sim.getNumAgents();

        for (int i = 0; i < count; ++i)
        {
            RVO.Vector3 pos = m_Sim.getAgentPosition(i);
            RVO.Vector3 color = m_Sim.getAgentColor(i);
            RVO.Vector3 vel = m_Sim.getAgentVelocity(i);
            float frozen = m_Sim.getAgentFreezed(i);

            m_BaseColors[i].Set(color.x(), color.y(), color.z(), 1.0f);
            m_Velocities[i].Set(vel.x(), vel.y(), vel.z(), 0.0f);
            m_AnimSpeeds[i].Set(Mathf.Max(frozen / m_MaxFrozen * 1.25f, 1.0f), frozen / 2.0f, frozen / m_MaxFrozen,
                                0.0f);
            m_Matrices[i].SetTRS(new Vector3(pos.x(), pos.y(), pos.z()), m_Sim.getAgentRotate(i), Vector3.one);
        }

        ReachedGoal();
    }

    public void DrawInstanced(CommandBuffer cmd)
    {
        if (!relativeObj.activeSelf) return;

        cmd.SetGlobalVectorArray(s_BaseColorBuffers, m_BaseColors);
        cmd.SetGlobalVectorArray(s_VelocityBuffers, m_Velocities);
        cmd.SetGlobalVectorArray(s_AnimSpeedBuffers, m_AnimSpeeds);

        for (int i = 0; i < m_Materials.Length; ++i)
        {
            if (m_Materials[i].renderQueue >= 3000) continue;

            if (optimizedAnimation)
            {
                m_Materials[i].EnableKeyword(s_OptimizedKeyWord);
            }
            else
            {
                m_Materials[i].DisableKeyword(s_OptimizedKeyWord);
            }

            cmd.DrawMeshInstanced(m_Mesh, i, m_Materials[i], m_Materials[i].FindPass("Unlit"), m_Matrices);
        }
    }

    public void DrawTransparentInstanced(CommandBuffer cmd)
    {
        if (!relativeObj.activeSelf) return;

        cmd.SetGlobalVectorArray(s_BaseColorBuffers, m_BaseColors);
        cmd.SetGlobalVectorArray(s_VelocityBuffers, m_Velocities);
        cmd.SetGlobalVectorArray(s_AnimSpeedBuffers, m_AnimSpeeds);

        for (int i = 0; i < m_Materials.Length; ++i)
        {
            if (m_Materials[i].renderQueue < 3000) continue;

            if (optimizedAnimation)
            {
                m_Materials[i].EnableKeyword(s_OptimizedKeyWord);
            }
            else
            {
                m_Materials[i].DisableKeyword(s_OptimizedKeyWord);
            }

            cmd.DrawMeshInstanced(m_Mesh, i, m_Materials[i], m_Materials[i].FindPass("UnlitTransparent"), m_Matrices);
        }
    }

    private void ReachedGoal()
    {
        int count = m_Sim.getNumAgents();
        for (int i = 0; i < count; ++i)
        {
            if (!(RVO.RVOMath.absSq(m_Sim.getAgentPosition(i) - m_Sim.getAgentGoal(i)) <
                  m_Sim.getAgentRadius(i) * m_Sim.getAgentRadius(i))) continue;

            Vector3 randomPosition = GetRandomPosition();
            m_Sim.setAgentGoal(i, new RVO.Vector3(randomPosition.x, randomPosition.y, randomPosition.z));
        }
    }

    private void SetupScenario()
    {
        m_Sim = new RVO.RVOSimulator(timeStep, neighborDist, maxNeighbors, timeHorizon, radius, maxSpeed);

        m_Matrices = new Matrix4x4[numberOfObjects];
        m_BaseColors = new Vector4[numberOfObjects];
        m_Velocities = new Vector4[numberOfObjects];
        m_AnimSpeeds = new Vector4[numberOfObjects];


        Color.RGBToHSV(colorMin, out float h1, out float s1, out float v1);
        Color.RGBToHSV(colorMax, out float h2, out float s2, out float v2);

        for (int i = 0; i < numberOfObjects; i++)
        {
            Vector3 randomPosition = GetRandomPosition();

            m_Matrices[i] = Matrix4x4.TRS(randomPosition, Quaternion.identity, Vector3.one);

            Color colors = Random.ColorHSV(h1, h2, s1, s2, v1, v2, 1, 1);

            m_Sim.addAgent(new RVO.Vector3(randomPosition.x, randomPosition.y, randomPosition.z),
                           new RVO.Vector3(colors.r, colors.g, colors.b));

            Vector3 randomPosition2 = GetRandomPosition();

            m_Sim.setAgentGoal(i, new RVO.Vector3(randomPosition2.x, randomPosition2.y, randomPosition2.z));
        }
    }

    private void SetPreferredVelocities()
    {
        int count = m_Sim.getNumAgents();

        for (int i = 0; i < count; ++i)
        {
            RVO.Vector3 goalVector = m_Sim.getAgentGoal(i) - m_Sim.getAgentPosition(i);

            if (RVO.RVOMath.absSq(ref goalVector) > 1.0f)
            {
                goalVector = RVO.RVOMath.normalize(ref goalVector);
            }

            m_Sim.setAgentPrefVelocity(i, goalVector);
        }
    }

    private Vector3 GetRandomPosition()
    {
        Vector3 spawnAreaCenter = transform.position;
        Vector3 spawnAreaSize = transform.lossyScale;

        float randomX = Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2, spawnAreaCenter.x + spawnAreaSize.x / 2);
        float randomY = Random.Range(spawnAreaCenter.y - spawnAreaSize.y / 2, spawnAreaCenter.y + spawnAreaSize.y / 2);
        float randomZ = Random.Range(spawnAreaCenter.z - spawnAreaSize.z / 2, spawnAreaCenter.z + spawnAreaSize.z / 2);

        return new Vector3(randomX, randomY, randomZ);
    }

    private void OnDrawGizmos()
    {
        Vector3 spawnAreaCenter = transform.position;
        Vector3 spawnAreaSize = transform.lossyScale;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
    }
}