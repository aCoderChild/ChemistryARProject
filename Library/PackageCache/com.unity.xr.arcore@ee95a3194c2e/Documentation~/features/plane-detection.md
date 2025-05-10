---
uid: arcore-plane-detection
---
# Plane detection

This page is a supplement to the AR Foundation [Plane detection](xref:arfoundation-plane-detection) manual. The following sections only contain information about APIs where ARCore exhibits unique platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Subsumption

ARCore supports plane subsumption. This means that you can include a plane inside another. Unity keeps the subsumed plane and doesn't update it.

## Battery life

The ARCore plane subsystem requires a significant amount of energy. You can reduce the thermal impact of plane detection and improve battery life by following these best practices:

1. Use the **Detection Mode** property of the [AR Plane Manager component](xref:arfoundation-plane-arplanemanager) to disable either horizontal or vertical plane detection if your app does not require it.

2. Disable plane detection when your app does not require it. For example, your app might direct the user to discover planes in their environment, then disable plane detection once discovery is complete.

## Native pointer

[BoundedPlane.nativePtr](xref:UnityEngine.XR.ARSubsystems.BoundedPlane.nativePtr) values returned by this package contain a pointer to the following struct:

```c
typedef struct UnityXRNativePlane
{
    int version;
    void* planePtr;
} UnityXRNativePlane;
```

This package also provides a header file containing the definitions of various native data structs including `UnityXRNativePlane`. It can be found in the package directory under `Includes~/UnityXRNativePtrs.h`.

Cast `void* planePtr` to an [ArPlane](https://developers.google.com/ar/reference/c/group/ar-plane) handle in C++ using the following example code:

```cpp
// Marhshal the native plane data from the BoundedPlane.nativePtr in C#
UnityXRNativePlane nativePlaneData;
ArPlane* planeHandle = static_cast<ArPlane*>(nativePlaneData.planePtr);
```

To learn more about native pointers and their usage, refer to [Extending AR Foundation](xref:arfoundation-extensions).
