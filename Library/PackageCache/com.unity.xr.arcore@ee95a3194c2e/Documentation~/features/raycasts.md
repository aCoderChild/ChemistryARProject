---
uid: arcore-raycasts
---
# Ray casts

This page is a supplement to the AR Foundation [Ray casts](xref:arfoundation-raycasts) manual. The following sections only contain information about APIs where ARCore exhibits unique platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Optional feature support

ARCore implements the following optional features of AR Foundation's [XRRaycastSubsystem](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystem):

| Feature                    | Descriptor Property | Supported |
| :------------------------- | :------------------ | :-------: |
| **Viewport based raycast** | [supportsViewportBasedRaycast](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystemDescriptor.supportsViewportBasedRaycast)| Yes |
| **World based raycast**    |  [supportsWorldBasedRaycast](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystemDescriptor.supportsWorldBasedRaycast)   | Yes |
| **Tracked raycasts**       | [supportsTrackedRaycasts](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystemDescriptor.supportsTrackedRaycasts) | Yes |

### Supported trackables

ARCore supports ray casting against the following trackable types:

| TrackableType           | Supported |
| :---------------------- | :-------: |
| **BoundingBox**         |           |
| **Depth**               |    Yes    |
| **Face**                |           |
| **FeaturePoint**        |    Yes    |
| **Image**               |           |
| **Planes**              |    Yes    |
| **PlaneEstimated**      |    Yes    |
| **PlaneWithinBounds**   |    Yes    |
| **PlaneWithinInfinity** |           |
| **PlaneWithinPolygon**  |    Yes    |

> [!NOTE]
> Refer to AR Foundation [Ray cast platform support](xref:arfoundation-raycasts-platform-support) for more information on the optional features of the Raycast subsystem.

## Native pointer

[XRRaycast.nativePtr](xref:UnityEngine.XR.ARSubsystems.XRRaycast.nativePtr) values returned by this package contain a pointer to the following struct:

```c
typedef struct UnityXRNativeRaycast
{
    int version;
    void* instantPoint;
    void* anchor;
} UnityXRNativeRaycast;
```

This package also provides a header file containing the definitions of various native data structs including `UnityXRNativeRaycast`. It can be found in the package directory under `Includes~/UnityXRNativePtrs.h`.

Cast `void* instantPoint` to an [ArTrackable](https://developers.google.com/ar/reference/c/group/ar-trackable) and `void* anchor` to an [ArAnchor](https://developers.google.com/ar/reference/c/group/ar-anchor) handle in C++ using the following example code:

```cpp
// Marhshal the native ray cast data from XRRaycast.nativePtr in C#
UnityXRNativeRaycast nativeRaycastData;
ArTrackable* pointTrackableHandle = static_cast<ArTrackable*>(nativeRaycastData.instantPoint);
ArAnchor* anchorHandle = static_cast<ArAnchor*>(nativeRaycastData.anchor);
```

To learn more about native pointers and their usage, refer to [Extending AR Foundation](xref:arfoundation-extensions).
