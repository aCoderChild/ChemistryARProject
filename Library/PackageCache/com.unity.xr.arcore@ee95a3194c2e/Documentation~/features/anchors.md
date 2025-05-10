---
uid: arcore-anchors
---
# Anchors

This page is a supplement to the AR Foundation [Anchors](xref:arfoundation-anchors) manual. The following sections describe the optional features of AR Foundation's [XRAnchorSubsystem](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystem) supported by ARCore.

[!include[](../snippets/arf-docs-tip.md)]

## Optional feature support

ARCore implements the following optional features of AR Foundation's [XRAnchorSubsystem](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystem):

| Feature | Descriptor Property | Supported |
| :------ | :------------------ | :-------: |
| **Trackable attachments** | [supportsTrackableAttachments](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsTrackableAttachments) | Yes |
| **Synchronous add** | [supportsSynchronousAdd](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsSynchronousAdd) | Yes |
| **Save anchor** | [supportsSaveAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsSaveAnchor) |  |
| **Load anchor** | [supportsLoadAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsLoadAnchor) |  |
| **Erase anchor** | [supportsEraseAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsEraseAnchor) |  |
| **Get saved anchor IDs** | [supportsGetSavedAnchorIds](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsGetSavedAnchorIds) |  |
| **Async cancellation** | [supportsAsyncCancellation](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsAsyncCancellation) |  |

> [!NOTE]
> Refer to AR Foundation [Anchors platform support](xref:arfoundation-anchors-platform-support) for more information
> on the optional features of the anchor subsystem.

## Native pointer

[XRAnchor.nativePtr](xref:UnityEngine.XR.ARSubsystems.XRAnchor.nativePtr) values returned by this package contain a pointer to the following struct:

```c
typedef struct UnityXRNativeAnchor
{
    int version;
    void* anchorPtr;
} UnityXRNativeAnchor;
```

This package also provides a header file containing the definitions of various native data structs including `UnityXRNativeAnchor`. It can be found in the package directory under `Includes~/UnityXRNativePtrs.h`.

Cast `void* anchorPtr` to an [ArAnchor](https://developers.google.com/ar/reference/c/group/ar-anchor) handle in C++ using the following example code:

```cpp
// Marhshal the native anchor data from the XRAnchor.nativePtr in C#
UnityXRNativeAnchor nativeAnchorData;
ArAnchor* anchorHandle = static_cast<ArAnchor*>(nativeAnchorData.anchorPtr);
```

To learn more about native pointers and their usage, refer to [Extending AR Foundation](xref:arfoundation-extensions).
