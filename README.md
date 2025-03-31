[![zh](https://img.shields.io/badge/lang-zh-blue.svg)](./README.zh.md)

<br />
<div align="center">
    <a href="https://github.com/PlayForDreamDevelopers/AmusementParkDemo-Unity">
        <img src="https://www.pfdm.cn/en/static/img/logo.2b1b07e.png" alt="Logo" width="20%">
    </a>
    <h1 align="center"> AmusementParkDemo-Unity </h1>
    <p align="center">
        Demo of amusement park
        <br />
        <a href="https://github.com/PlayForDreamDevelopers/AmusementParkDemo-Unity/blob/main/README.md"><strong>View Documentation »</strong></a>
        <br />
        <br />
        <a href="https://github.com/PlayForDreamDevelopers/AmusementParkDemo-Unity">View Samples</a>
        &middot;
        <a href="https://github.com/PlayForDreamDevelopers/AmusementParkDemo-Unity/issues/new?template=bug_report.yml">Report Bug</a>
        &middot;
        <a href="https://github.com/PlayForDreamDevelopers/AmusementParkDemo-Unity/issues/new?template=feature_request.yml">Request Feature</a>
        &middot;
        <a href="https://github.com/PlayForDreamDevelopers/AmusementParkDemo-Unity/issues/new?template=documentation_update.yml">Improve Documentation</a>
    </p>

</div>

## About The Project

Amusement park demo based on LBE API

## Sample

### Scan QR code

Scan QR code and objects appear in the MR scene

https://github.com/user-attachments/assets/5c287aa1-159f-4c9a-a9ac-2972001ce8fb

This code in `AmusementParkControl.cs` demonstrates how to obtain the overall position of the displayed object through matrix operations after obtaining the anchor pose

```c#
	Matrix4x4 virtualMarkMatrix = m_MarkIdMatrixObjectDic[data.markerId];
	Matrix4x4 actualMarkMatrix = Matrix4x4.TRS(data.markerPose.position, data.markerPose.orientation, Vector3.one);
	Matrix4x4 originLocalMatrixInVirtual = virtualMarkMatrix.inverse * m_OriginMatrix;
	Matrix4x4 fixedOriginWorldMatrix = actualMarkMatrix * originLocalMatrixInVirtual;

	origin.transform.localPosition = fixedOriginWorldMatrix.GetPosition();
	origin.transform.localRotation = fixedOriginWorldMatrix.rotation;
	origin.transform.localScale = fixedOriginWorldMatrix.lossyScale;
```

1. Matrix Calculations and Transformations:

	- The virtual marker matrix (virtualMarkMatrix) corresponding to the marker ID is fetched from the dictionary m_MarkIdMatrixObjectDic.

	- The actual marker matrix (actualMarkMatrix) is constructed using the input marker pose data (data.markerPose), which includes position, rotation (orientation), and uniform scale (Vector3.one).

	- The origin’s local transformation matrix in virtual marker space (originLocalMatrixInVirtual) is computed by multiplying the inverse of the virtual marker matrix (virtualMarkMatrix.inverse) with the origin matrix (m_OriginMatrix).

	- The corrected world-space matrix for the origin (fixedOriginWorldMatrix) is calculated by multiplying the actual marker matrix with the origin’s local matrix, effectively transforming the origin from virtual marker space to real-world marker space.

2. Updating the Origin’s Transform:

	- The computed position, rotation, and scale (lossy scale) from fixedOriginWorldMatrix are applied to the local transform of the origin object.

	- This ensures that the origin object’s pose aligns correctly with the detected marker in the AR environment.

## Requirements

-   Unity 2022.3.52f1 or later
-   Unity Packages:
    -   [YVR Utilities](https://github.com/PlayForDreamDevelopers/com.yvr.Utilities-mirror)
    -   [YVR Platform](https://github.com/PlayForDreamDevelopers/com.yvr.platform-mirror)
    -   [YVR Core](https://github.com/PlayForDreamDevelopers/com.yvr.core-mirror)
    -   [YVR Enterprise](https://github.com/PlayForDreamDevelopers/com.yvr.enterprise-mirror)
