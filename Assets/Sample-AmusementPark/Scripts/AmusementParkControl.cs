using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YVR.Core;
using System.Linq;
using UnityEngine.InputSystem;
using YVR.Enterprise.LBE;

[Serializable]
public class MarkIdGameObjectMapping
{
    public long id;
    public GameObject virtualObject;
    public GameObject relativeObject;
}

public class AmusementParkControl : MonoBehaviour
{
    public GameObject particleEffect;

    public GameObject origin;
    public MarkIdGameObjectMapping[] mapping;

    private Dictionary<long, Matrix4x4> m_MarkIdMatrixObjectDic = new();
    private Dictionary<long, MarkIdGameObjectMapping> m_MarkIdObjectMappingDic = new();

    private Matrix4x4 m_OriginMatrix;
    private GameObject m_PreviousActiveRelativeObject;
    private long m_LatestMarkerId;
    private float m_LatestConfidence;

    protected void Start()
    {
        YVRPlugin.Instance.SetPassthrough(true);

        LBEPlugin.instance.SetMarkerDetectionEnable(true);
        LBEPlugin.instance.SetMarkerTrackingUpdateCallback(OnReceiveMarkerTrackingUpdateData);

        origin.gameObject.SetActive(false);

        m_OriginMatrix = origin.transform.localToWorldMatrix;

        foreach (MarkIdGameObjectMapping mappingItem in mapping)
        {
            mappingItem.relativeObject.gameObject.SetActive(false);
            m_MarkIdMatrixObjectDic[mappingItem.id] = mappingItem.virtualObject.transform.localToWorldMatrix;
            m_MarkIdObjectMappingDic[mappingItem.id] = mappingItem;
        }
    }

    private void OnReceiveMarkerTrackingUpdateData(MarkerTrackingUpdateData data)
    {
        Debug.Log($"MarkerTrackingMgr MarkerTrackingCallback markerId:{data.markerId},confidence:{data.confidence}");
        
        if (mapping.Where((item) => item.id == data.markerId).ToList().Count > 0)
        {
            origin.gameObject.SetActive(true);
        }

        if (!(data.confidence >= 0.7f)) return;

        if (!m_MarkIdMatrixObjectDic.ContainsKey(data.markerId)) return;

        if (m_LatestMarkerId != data.markerId)
        {
            m_LatestMarkerId = data.markerId;
            m_LatestConfidence = 0;
        }

        if (m_PreviousActiveRelativeObject != null)
            m_PreviousActiveRelativeObject.gameObject.SetActive(false);

        MarkIdGameObjectMapping mappingInfo = m_MarkIdObjectMappingDic[data.markerId];
        GameObject relativeObject = mappingInfo.relativeObject;
        relativeObject.gameObject.SetActive(true);
        m_PreviousActiveRelativeObject = relativeObject;

        Matrix4x4 virtualMarkMatrix = m_MarkIdMatrixObjectDic[data.markerId];
        Matrix4x4 actualMarkMatrix
            = Matrix4x4.TRS(data.markerPose.position, data.markerPose.orientation, Vector3.one);
        Matrix4x4 originLocalMatrixInVirtual = virtualMarkMatrix.inverse * m_OriginMatrix;
        Matrix4x4 fixedOriginWorldMatrix = actualMarkMatrix * originLocalMatrixInVirtual;

        origin.transform.localPosition = fixedOriginWorldMatrix.GetPosition();
        origin.transform.localRotation = fixedOriginWorldMatrix.rotation;
        origin.transform.localScale = fixedOriginWorldMatrix.lossyScale;

        if (!(m_LatestConfidence < data.confidence)) return;

        m_LatestConfidence = data.confidence;
        Instantiate(particleEffect, mappingInfo.virtualObject.transform.position,
                    mappingInfo.virtualObject.transform.rotation);
    }

    private void OnApplicationPause(bool pause) { LBEPlugin.instance.SetMarkerDetectionEnable(!pause); }

    private void OnApplicationQuit() { LBEPlugin.instance.SetMarkerDetectionEnable(false); }
}