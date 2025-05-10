---
uid: arcore-session-recording
---
# ARCore Session Recording

**Note**: The session recording feature described below is currently only supported when the graphics rendering API is set to `OpenGLES3`.  Attempting to play back a session recording through the ARCore playback APIs, when the graphics rendering API is set to `Vulkan`, will not succeed.

ARCore allows you to record an ArSession to an `.mp4` and play it back at a later time. To support this feature, the [ARCoreSessionSubsystem](xref:UnityEngine.XR.ARCore.ARCoreSessionSubsystem) exposes the following methods:

* [StartRecording](xref:UnityEngine.XR.ARCore.ARCoreSessionSubsystem.StartRecording(UnityEngine.XR.ARCore.ArRecordingConfig))
* [StopRecording](xref:UnityEngine.XR.ARCore.ARCoreSessionSubsystem.StopRecording)
* [StartPlaybackUri](xref:UnityEngine.XR.ARCore.ARCoreSessionSubsystem.StartPlaybackUri(System.String))
* [StopPlaybackUri](xref:UnityEngine.XR.ARCore.ARCoreSessionSubsystem.StopPlaybackUri)

To start a recording, supply an [ArRecordingConfig](xref:UnityEngine.XR.ARCore.ArRecordingConfig). This specifies the file name that Unity saves the recording as, as well as other options. Call `StopRecording` to stop recording. When Unity stops recording, it creates the `.mp4` file as specified in the `ArRecordingConfig`. This contains the camera feed and sensor data required by ARCore.

To play back a video, use the `StartPlaybackUri` method, and specify an `.mp4` file created during an earlier recording.

**Note**: The file location string parameters, provided to the recording and playback APIs, must be in URI format.  For example, referencing a file on the Android local file system requires escaping the path, and have the "file://" protocol prefix.  For references related to file paths as URIs, there are articles available [RFC-8089](https://dl.acm.org/doi/10.17487/RFC8089), and [here](https://en.wikipedia.org/wiki/File_URI_scheme).

To start or stop a recorded file in ARCore, the [ARCoreSessionSubsystem](xref:UnityEngine.XR.ARCore.ARCoreSessionSubsystem) pauses the session. Pausing and resuming a session can take between 0.5 and 1.0 seconds.

**Note**: Video recordings contain sensor data, but not the computed results. ARCore does not always produce the same output, which means trackables might not be consistent between playbacks of the same recording. For example, multiple playbacks of the same recording might give different plane detection results.
