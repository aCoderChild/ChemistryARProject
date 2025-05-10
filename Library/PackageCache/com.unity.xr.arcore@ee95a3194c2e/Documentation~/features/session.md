---
uid: arcore-session
---
# Session

This page is a supplement to the AR Foundation [Session](xref:arfoundation-session) manual. The following sections only contain information about APIs where ARCore exhibits unique platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Check if AR is supported

ARCore implements [XRSessionSubsystem.GetAvailabilityAsync](xref:UnityEngine.XR.ARSubsystems.XRSessionSubsystem.GetAvailabilityAsync). Use this function to determine if the device ARCore is currently running on is supported. ARCore's list of supported devices is frequently updated to include additional devices. For a full list of devices that ARCore supports, refer to [ARCore supported devices](https://developers.google.com/ar/discover/supported-devices).

If ARCore isn't already installed on a device, your app needs to check with the Google Play store to see if there's a version of ARCore that supports that device. To do this, use `GetAvailabilityAsync` to return a `Promise` that you can use in a coroutine. For ARCore, this check can take some time.

If the device is supported, but ARCore is not installed or requires an update, call [XRSessionSubsystem.InstallAsync](UnityEngine.XR.ARSubsystems.XRSessionSubsystem.InstallAsync), which also returns a `Promise`.

## Native pointer

[XRSessionSubsystem.nativePtr](xref:UnityEngine.XR.ARSubsystems.XRSessionSubsystem.nativePtr) values returned by this package contain a pointer to the following struct:

```c
typedef struct UnityXRNativeSession
{
    int version;
    void* sessionPtr;
} UnityXRNativeSession;
```

This package also provides a header file containing the definitions of various native data structs including `UnityXRNativeSession`. It can be found in the package directory under `Includes~/UnityXRNativePtrs.h`.

Cast `void* sessionPtr` to an [ArSession](https://developers.google.com/ar/reference/c/group/ar-session) handle in C++ using the following example code:

```cpp
// Marhshal the native session data from the XRSessionSubsystem.nativePtr in C#
UnityXRNativeSession nativeSessionData;
ArSession* session = static_cast<ArSession*>(nativeSessionData.sessionPtr);
```

To learn more about native pointers and their usage, refer to [Extending AR Foundation](xref:arfoundation-extensions).
