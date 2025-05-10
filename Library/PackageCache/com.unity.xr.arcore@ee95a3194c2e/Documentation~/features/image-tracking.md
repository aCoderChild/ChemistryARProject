---
uid: arcore-image-tracking
---
# Image tracking

This page is a supplement to the AR Foundation [Image tracking](xref:arfoundation-image-tracking) manual. The following sections only contain information about APIs where ARCore exhibits unique platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Optional feature support

ARCore implements the following optional features of AR Foundation's [XRImageTrackingSubsystem](xref:UnityEngine.XR.ARSubsystems.XRImageTrackingSubsystem):

| Feature | Descriptor Property | Supported |
| :------ | :--------------- | :----------: |
| **Moving images** | [supportsMovingImages](xref:UnityEngine.XR.ARSubsystems.XRImageTrackingSubsystemDescriptor.supportsMovingImages) | Yes |
| **Requires physical image dimensions** | [requiresPhysicalImageDimensions](xref:UnityEngine.XR.ARSubsystems.XRImageTrackingSubsystemDescriptor.requiresPhysicalImageDimensions) | |
| **Mutable library** | [supportsMutableLibrary](xref:UnityEngine.XR.ARSubsystems.XRImageTrackingSubsystemDescriptor.supportsMutableLibrary) | Yes |
| **Image validation** | [supportsImageValidation](xref:UnityEngine.XR.ARSubsystems.XRImageTrackingSubsystemDescriptor.supportsImageValidation) | Yes |

## Reference image libraries

When you build an ARCore app for the Android platform, this package creates an `imgdb` file for each reference image library. ARCore creates these files in your project's `StreamingAssets` folder, in a subdirectory called `HiddenARCore`, so Unity can access them at runtime.

## Texture formats

You can use .jpg or .png files as AR reference images in ARCore. If a reference image in the `XRReferenceImageLibrary` isn't a .jpg or .png, a script in this package called the `ARCoreBuildProcessor` will attempt to convert the texture to a .`png` so that ARCore can use it.

When you export a `Texture2D` to `.png`, it can fail if the texture's [Texture Import Settings](https://docs.unity3d.com/Manual/class-TextureImporter.html) have **Read/Write Enabled** disabled and **Compression** is set to **None**.

To use the texture at runtime (not as a source Asset for the reference image), create a separate .jpg or .png copy for the source Asset. This reduces the performance impact of the Texture Import Settings at runtime.

## AssetBundles

Reference image libraries can be stored in AssetBundles and loaded at runtime, but setting up your project to build the AssetBundles correctly requires special instructions. Refer to [Use reference image libraries with AssetBundles](xref:arfoundation-image-tracking-assetbundles) in AR Foundation for more information.

## Reference image dimensions

To improve image detection in ARCore you can specify the image dimensions. When you specify the dimensions for a reference image, ARCore receives the image's width, and then determines the height from the image's aspect ratio.

## Native pointer

[XRTrackedImage.nativePtr](xref:UnityEngine.XR.ARSubsystems.XRTrackedImage.nativePtr) values returned by this package contain a pointer to the following struct:

```c
typedef struct UnityXRNativeImage
{
    int version;
    void* imageTrackable;
} UnityXRNativeImage;
```

This package also provides a header file containing the definitions of various native data structs including `UnityXRNativeImage`. It can be found in the package directory under `Includes~/UnityXRNativePtrs.h`.

Cast `void* imageTrackable` to an [ArAugmentedImage](https://developers.google.com/ar/reference/c/group/ar-augmented-image) handle in C++ using the following example code:

```cpp
// Marshal the native image data from the XRTrackedImage.nativePtr in C#
UnityXRNativeImage nativeImageData;
ArAugmentedImage* augmentedImageHandle = static_cast<ArAugmentedImage*>(nativeImageData.imageTrackable);
```

To learn more about native pointers and their usage, refer to [Extending AR Foundation](xref:arfoundation-extensions).
