---
uid: arcore-point-clouds
---
# Point clouds

This page is a supplement to the AR Foundation [Point clouds](xref:arfoundation-point-clouds) manual. The following sections only contain information about APIs where ARCore exhibits unique platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

ARCore's point cloud subsystem only produces one [XRPointCloud](xref:UnityEngine.XR.ARSubsystems.XRPointCloud).

When you use the raycast subsystem to cast a ray at a point in the cloud, the returned pose orientation provides an estimate for the surface that point might represent.

## Native pointer

[XRPointCloud.nativePtr](xref:UnityEngine.XR.ARSubsystems.XRPointCloud.nativePtr) values returned by this package contain a pointer to the following struct:

```c
typedef struct UnityXRNativePointCloud
{
    int version;
    void* pointCloud;
} UnityXRNativePointCloud;
```

This package also provides a header file containing the definitions of various native data structs including `UnityXRNativePointCloud`. It can be found in the package directory under `Includes~/UnityXRNativePtrs.h`.

Cast `void* pointCloud` to an [ArPointCloud](https://developers.google.com/ar/reference/c/group/ar-point-cloud) handle in C++ using the following example code:

```cpp
// Marhshal the native point cloud data from the XRPointCloud.nativePtr in C#
UnityXRNativePointCloud nativePointCloudData;
ArPointCloud* pointCloudHandle = static_cast<ArPointCloud*>(nativePointCloudData.pointCloud);
```

To learn more about native pointers and their usage, refer to [Extending AR Foundation](xref:arfoundation-extensions).
