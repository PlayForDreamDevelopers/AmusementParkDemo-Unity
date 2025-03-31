[![zh](https://img.shields.io/badge/lang-zh-blue.svg)](./README.zh.md)

<br />
<div align="center">
    <a href="https://github.com/PlayForDreamDevelopers/AmusementParkDemo-Unity">
        <img src="https://www.pfdm.cn/en/static/img/logo.2b1b07e.png" alt="Logo" width="20%">
    </a>
    <h1 align="center"> AmusementParkDemo-Unity </h1>
    <p align="center">
        游乐园项目演示
        <br />
        <a href="https://github.com/PlayForDreamDevelopers/AmusementParkDemo-Unity">查看示例</a>
        &middot;
        <a href="https://github.com/PlayForDreamDevelopers/AmusementParkDemo-Unity/issues/new?template=bug_report.yml">报告错误</a>
        &middot;
        <a href="https://github.com/PlayForDreamDevelopers/AmusementParkDemo-Unity/issues/new?template=feature_request.yml">请求功能</a>
        &middot;
        <a href="https://github.com/PlayForDreamDevelopers/AmusementParkDemo-Unity/issues/new?template=documentation_update.yml">改进文档</a>
    </p>

</div>

## 项目简介

基于LBE API开发的游乐园场景演示项目

## 功能示例

### 二维码扫描功能

扫描二维码后，物体将出现在混合现实(MR)场景中

https://github.com/user-attachments/assets/5c287aa1-159f-4c9a-a9ac-2972001ce8fb

`AmusementParkControl.cs`中的这段代码展示了如何通过获取锚点位姿后，使用矩阵运算确定展示物体的整体位置

```c#
	Matrix4x4 virtualMarkMatrix = m_MarkIdMatrixObjectDic[data.markerId];
	Matrix4x4 actualMarkMatrix = Matrix4x4.TRS(data.markerPose.position, data.markerPose.orientation, Vector3.one);
	Matrix4x4 originLocalMatrixInVirtual = virtualMarkMatrix.inverse * m_OriginMatrix;
	Matrix4x4 fixedOriginWorldMatrix = actualMarkMatrix * originLocalMatrixInVirtual;

	origin.transform.localPosition = fixedOriginWorldMatrix.GetPosition();
	origin.transform.localRotation = fixedOriginWorldMatrix.rotation;
	origin.transform.localScale = fixedOriginWorldMatrix.lossyScale;
```

1. 矩阵计算与变换：

    - 从字典m_MarkIdMatrixObjectDic中获取与标记ID对应的虚拟标记矩阵(virtualMarkMatrix)
    
    - 使用输入的标记位姿数据(data.markerPose)构建实际标记矩阵(actualMarkMatrix)，包含位置、旋转(orientation)和单位缩放(Vector3.one)
    
    - 通过将虚拟标记矩阵的逆矩阵(virtualMarkMatrix.inverse)与原点矩阵(m_OriginMatrix)相乘，计算原点在虚拟标记局部空间中的变换矩阵(originLocalMatrixInVirtual)
    
    - 通过将实际标记矩阵与原点的局部矩阵相乘，计算修正后的原点世界空间矩阵(fixedOriginWorldMatrix)，实现将原点从虚拟标记空间变换到真实标记空间

2. 更新原点变换：

    - 将计算得到的位置、旋转和缩放(lossy scale)从fixedOriginWorldMatrix应用到原点物体的本地变换
    
    - 这确保了原点物体的姿态与AR环境中检测到的标记正确对齐

## 系统要求

-   Unity 2022.3.52f1或更高版本
-   所需Unity包：
    -   [YVR Utilities](https://github.com/PlayForDreamDevelopers/com.yvr.Utilities-mirror)
    -   [YVR Platform](https://github.com/PlayForDreamDevelopers/com.yvr.platform-mirror)
    -   [YVR Core](https://github.com/PlayForDreamDevelopers/com.yvr.core-mirror)
    -   [YVR Enterprise](https://github.com/PlayForDreamDevelopers/com.yvr.enterprise-mirror)
