---
uid: arcore-environment-probes
---
# Environment Probes

This page is a supplement to the AR Foundation [Environment Probes](xref:arfoundation-environment-probes) manual. The following sections only contain information about APIs where ARCore exhibits unique platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Native pointer

[XREnvironmentProbe.nativePtr](xref:UnityEngine.XR.ARSubsystems.XREnvironmentProbe.nativePtr) values returned by this package contain a pointer to the following struct:

```c
typedef struct UnityXRNativeEnvironmentProbe
{
    int version;
    void* arImageCubemap[6];
} UnityXRNativeEnvironmentProbe;
```

This package also provides a header file containing the definitions of various native data structs including `UnityXRNativeEnvironmentProbe`. It can be found in the package directory under `Includes~/UnityXRNativePtrs.h`.

Cast `void* arImageCubemap` to an [ArImageCubemap](https://developers.google.com/ar/reference/c/group/ar-image#arimagecubemap) handle in C++ using the following example code:

```cpp
// Marhshal the native environment probe data from the XREnvironmentProbe.nativePtr in C#
UnityXRNativeEnvironmentProbe nativeEnvironmentProbeData;
ArImageCubemap* cubemapHandle = static_cast<ArImageCubemap*>(&nativeEnvironmentProbeData.arImageCubemap);
```

To learn more about native pointers and their usage, refer to [Extending AR Foundation](xref:arfoundation-extensions).
