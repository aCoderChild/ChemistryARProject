---
uid: arcore-whats-new
---
# What's new in version 6.1

## New features

### Vulkan Graphics API rendering support

- Added support for camera background rendering when using the Vulkan Graphics API. Refer to [Project Configuration](xref:arcore-project-config) for more information.
- Added three new session subsystem API override methods to the `ARCoreSessionSubsystem` provider class to handle Universal Render Pipeline
rendering events signaled by the `ARCommandBufferSupportRendererFeature` in order to support rendering through the Vulkan Graphics API: `ARCoreSessionSubsystem.requiresCommandBuffer`, `ARCoreSessionSubsystem.OnCommandBufferSupportEnabled`, and `ARCoreSessionSubsystem.OnCommandBufferExecute`.
- Added support for occlusion, environment probes, and CPU images when using the Vulkan Graphics API. Refer to [Project Configuration](xref:arcore-project-config) for more information.
- `Session Recording`, through the `ARCoreSessionSubsystem`, is not supported when the graphics rendering API is set to `Vulkan`.
- Added the ability to turn the camera torch light on and off. Refer to the AR Foundation [Camera torch mode (flash)](xref:arfoundation-camera-torch-mode) documentation for more information

For a full list of changes in this version including backwards-compatible bugfixes, refer to the package [changelog](xref:arcore-changelog).

## Deprecated

### Session Recording and Playback related APIs have been updated

- Deprecated and replaced the following APIs.
  - [ARCoreSessionSubsystem.StartPlayback](xref:UnityEngine.XR.ARCore.ARCoreSessionSubsystem.StartPlayback) to [ARCoreSessionSubsystem.StartPlaybackUri](xref:UnityEngine.XR.ARCore.ARCoreSessionSubsystem.StartPlaybackUri).
  - [ARCoreSessionSubsystem.StopPlayback](xref:UnityEngine.XR.ARCore.ARCoreSessionSubsystem.StopPlayback) to [ARCoreSessionSubsystem.StopPlaybackUri](xref:UnityEngine.XR.ARCore.ARCoreSessionSubsystem.StopPlaybackUri).
  - [ARRecordingConfig.GetMp4DatasetFilePath](xref:UnityEngine.XR.ARCore.ARRecordingConfig.GetMp4DatasetFilePath) to [ARRecordingConfig.GetMp4DatasetUri](xref:UnityEngine.XR.ARCore.ARRecordingConfig.GetMp4DatasetUri).
  - [ARRecordingConfig.SetMp4DatasetFilePath](xref:UnityEngine.XR.ARCore.ARRecordingConfig.SetMp4DatasetFilePath) to [ARRecordingConfig.SetMp4DatasetUri](xref:UnityEngine.XR.ARCore.ARRecordingConfig.SetMp4DatasetUri).
  - [ARSession.SetPlaybackDataset](xref:UnityEngine.XR.ARCore.ARSession.SetPlaybackDataset) to [ARSession.SetPlaybackDatasetUri](xref:UnityEngine.XR.ARCore.ARSession.SetPlaybackDatasetUri).