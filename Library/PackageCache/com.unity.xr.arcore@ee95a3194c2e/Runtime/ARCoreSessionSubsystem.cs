using AOT;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARCore
{
    /// <summary>
    /// The ARCore implementation of the
    /// [XRSessionSubsystem](xref:UnityEngine.XR.ARSubsystems.XRSessionSubsystem).
    /// Do not create this directly. Use the
    /// [SubsystemManager](xref:UnityEngine.SubsystemManager)
    /// instead.
    /// </summary>
    [Preserve]
    public sealed class ARCoreSessionSubsystem : XRSessionSubsystem
    {
        /// <summary>
        /// Invoked when the subsystem is created.
        /// </summary>
        protected override void OnCreate()
        {
            ((ARCoreProvider)provider).beforeSetConfiguration += ConfigurationChangedFromProvider;
            s_VulkanTaskHandlerEventFunc = NativeApi.UnityARCore_session_getVulkanTaskHandlerEventFunc();
        }

        void ConfigurationChangedFromProvider(ARCoreBeforeSetConfigurationEventArgs eventArgs) => beforeSetConfiguration?.Invoke(eventArgs);

        /// <summary>
        /// Initiates a configuration change.
        /// </summary>
        /// <remarks>
        /// When you call this function, the session dispatches a <see cref="beforeSetConfiguration"/> event.
        /// </remarks>
        public void SetConfigurationDirty()
        {
            NativeApi.UnityARCore_session_setConfigurationDirty();
        }

        // Must match UnityXRNativeSession
        struct NativePtr
        {
            public int verison;
            public IntPtr sessionPtr;
        }

        /// <summary>
        /// (Read Only) The <see cref="ArSession"/> associated with the subsystem. May be `null`.
        /// </summary>
        /// <value>The session instance.</value>
        public ArSession session => ((ARCoreProvider)provider).session;

        /// <summary>
        /// Starts recording a session.
        /// </summary>
        /// <param name="recordingConfig">The configuration for the recording.</param>
        /// <returns>Returns <see cref="ArStatus.Success"/> if recording successfully begins. Returns one of the
        /// following otherwise:
        /// - <see cref="ArStatus.ErrorIllegalState"/>
        /// - <see cref="ArStatus.ErrorInvalidArgument"/>
        /// - <see cref="ArStatus.ErrorRecordingFailed"/>
        /// </returns>
        /// <seealso cref="StopRecording"/>
        /// <seealso cref="StartPlaybackUri"/>
        /// <seealso cref="StopPlaybackUri"/>
        public ArStatus StartRecording(ArRecordingConfig recordingConfig) =>
            ((ARCoreProvider)provider).StartRecording(recordingConfig);

        /// <summary>
        /// Stops recording a session.
        /// </summary>
        /// <returns>Returns <see cref="ArStatus.Success"/> if successful. Returns
        ///     <see cref="ArStatus.ErrorRecordingFailed"/> otherwise.</returns>
        /// <seealso cref="StartRecording"/>
        /// <seealso cref="StartPlaybackUri"/>
        /// <seealso cref="StopPlaybackUri"/>
        public ArStatus StopRecording() => ((ARCoreProvider)provider).StopRecording();

        /// <summary>
        /// Starts playing back a previously recorded session. (refer to <see cref="StartRecording"/>)
        /// </summary>
        /// <remarks>
        /// To begin playback, the session must first be paused, the playback dataset set, then resumed.
        /// This method does all of those things for you, but can take significant time (.5 - 1 seconds) to do so.
        /// </remarks>
        /// <param name="playbackDataset">The path to an mp4 file previously recorded using
        ///     <see cref="StartRecording"/>.</param>
        /// <returns>Returns <see cref="ArStatus.Success"/> if successful. Returns one of the following otherwise:
        /// - Returns <see cref="ArStatus.ErrorSessionUnsupported"/> if playback is incompatible with selected features.
        /// - Returns <see cref="ArStatus.ErrorPlaybackFailed"/> if an error occurred with the MP4 dataset file such as not being able to open the file or the file is unable to be decoded.
        /// </returns>
        /// <seealso cref="StartRecording"/>
        /// <seealso cref="StopRecording"/>
        /// <seealso cref="StopPlayback"/>
        [Obsolete("StartPlayback is deprecated in AR Foundation 6.1. Use StartPlaybackUri(string playbackDatasetUri) instead.")]
        public ArStatus StartPlayback(string playbackDataset) => ((ARCoreProvider)provider).SetPlaybackDataset(playbackDataset);

        /// <summary>
        /// Starts playing back a previously recorded session. (refer to <see cref="StartRecording"/>)
        /// </summary>
        /// <remarks>
        /// To begin playback, the session must first be paused, the playback dataset set, then resumed.
        /// This method does all of those things for you, but can take significant time (.5 - 1 seconds) to do so.
        /// </remarks>
        /// <param name="playbackDatasetUri">The uri to an mp4 file previously recorded using
        ///     <see cref="StartRecording"/>.</param>
        /// <returns>Returns <see cref="ArStatus.Success"/> if successful. Returns one of the following otherwise:
        /// - Returns <see cref="ArStatus.ErrorSessionUnsupported"/> if playback is incompatible with selected features.
        /// - Returns <see cref="ArStatus.ErrorPlaybackFailed"/> if an error occurred with the MP4 dataset file such as not being able to open the file or the file is unable to be decoded.
        /// </returns>
        /// <seealso cref="StartRecording"/>
        /// <seealso cref="StopRecording"/>
        /// <seealso cref="StopPlaybackUri"/>
        public ArStatus StartPlaybackUri(string playbackDatasetUri) => ((ARCoreProvider)provider).SetPlaybackDatasetUri(playbackDatasetUri);

        /// <summary>
        /// Stops playing back a session recording.
        /// </summary>
        /// <returns>Returns <see cref="ArStatus.Success"/> if successful. Returns one of the following otherwise:
        /// - <see cref="ArStatus.ErrorSessionUnsupported"/> if playback is incompatible with selected features.
        /// - <see cref="ArStatus.ErrorPlaybackFailed"/> if an error occurred with the MP4 dataset file such as not being able to open the file or the file is unable to be decoded.
        /// </returns>
        /// <seealso cref="StartPlayback"/>
        /// <seealso cref="StartRecording"/>
        /// <seealso cref="StopRecording"/>
        [Obsolete("StopPlayback is deprecated in AR Foundation 6.1. Use StopPlaybackUri(ArRecordingConfig recordingConfig) instead.")]
        public ArStatus StopPlayback() => ((ARCoreProvider)provider).SetPlaybackDataset(null);

        /// <summary>
        /// Stops playing back a session recording.
        /// </summary>
        /// <returns>Returns <see cref="ArStatus.Success"/> if successful. Returns one of the following otherwise:
        /// - <see cref="ArStatus.ErrorSessionUnsupported"/> if playback is incompatible with selected features.
        /// - <see cref="ArStatus.ErrorPlaybackFailed"/> if an error occurred with the MP4 dataset file such as not being able to open the file or the file is unable to be decoded.
        /// </returns>
        /// <seealso cref="StartPlaybackUri"/>
        /// <seealso cref="StartRecording"/>
        /// <seealso cref="StopRecording"/>
        public ArStatus StopPlaybackUri() => ((ARCoreProvider)provider).SetPlaybackDatasetUri(null);

        /// <summary>
        /// (Read Only) The current recording status.
        /// </summary>
        /// <value>Whether or not the session is recording (or has stopped because of an error).</value>
        public ArRecordingStatus recordingStatus => ((ARCoreProvider)provider).recordingStatus;

        /// <summary>
        /// (Read Only) The current playback status.
        /// </summary>
        /// <value>Whether or not the session is playing back a recording (or has stopped because of an error).</value>
        public ArPlaybackStatus playbackStatus => ((ARCoreProvider)provider).playbackStatus;

        /// <summary>
        /// An event that is triggered right before the configuration is set on the session.
        /// Allows changes to be made to the configuration before it is set.
        /// </summary>
        /// <value>An Action delegate that provides access to the new session config before it is applied.</value>
        public event Action<ARCoreBeforeSetConfigurationEventArgs> beforeSetConfiguration;

        /// <summary>
        /// An event callback to handle the native Vulkan command buffer recording and submitting for various subsystems.
        /// </summary>
        internal static IntPtr s_VulkanTaskHandlerEventFunc = IntPtr.Zero;

        /// <summary>
        /// A boolean flag to check whether necessary renderer feature is enabled when Vulkan is used.
        /// </summary>
        internal static bool s_VulkanSupportRendererFeatureEnabled = false;

        // For built-in render pipeline Vulkan plugin event callback injection.
#if !URP_7_OR_NEWER
        /// <summary>
        /// The camera to add the Vulkan callback event.
        /// We cache a reference to the main camera so that in case main camera changed (e.g. scene changed)
        /// we will know to add the command buffer to the new camera.
        /// </summary>
        private static Camera m_CameraWithPluginCallback = null;

        /// <summary>
        /// Command buffer for the render event injection.
        /// </summary>
        private static CommandBuffer m_PluginCallbackCommandBuffer = null;

        /// <summary>
        /// Name of the command buffer.
        /// </summary>
        private static readonly string k_PluginCallbackCommandBufferName = "Vulkan Support Event Injection Pass (Built-in Render Pipeline)";
#endif // !URP_7_OR_NEWER

        class ARCoreProvider : Provider
        {
            GCHandle m_ProviderHandle;
            Action<ArSession, ArConfig, IntPtr> m_SetConfigurationCallback = SetConfigurationCallback;
            Guid m_SessionId;

            [Obsolete("Use SetPlaybackDatasetUri(string uri) instead")]
            public ArStatus SetPlaybackDataset(string playbackDataset)
            {
                var session = this.session;

                if (session == null)
                    throw new InvalidOperationException($"{nameof(SetPlaybackDataset)} requires a valid {nameof(ArSession)}");

                return SetPlaybackDatasetInternal(session, playbackDataset, session.SetPlaybackDataset);
            }

            public ArStatus SetPlaybackDatasetUri(string playbackDatasetUri)
            {
                var session = this.session;

                if (session == null)
                    throw new InvalidOperationException($"{nameof(SetPlaybackDatasetUri)} requires a valid {nameof(ArSession)}");

                return SetPlaybackDatasetInternal(session, playbackDatasetUri, session.SetPlaybackDatasetUri);
            }

            public ARCoreProvider()
            {
                var textureUpdateMode = NativeApi.TextureUpdateMode.DefaultOpenGLES;
                switch (SystemInfo.graphicsDeviceType)
                {
                    case GraphicsDeviceType.Vulkan:
                        textureUpdateMode = NativeApi.TextureUpdateMode.HardwareBufferVulkan;
                        break;
                }

                NativeApi.UnityARCore_session_construct(CameraPermissionRequestProvider, textureUpdateMode, SystemInfo.graphicsMultiThreaded);
                if (SystemInfo.graphicsMultiThreaded)
                {
                    m_RenderEventFunc = NativeApi.UnityARCore_session_getRenderEventFunc();
                }

                m_ProviderHandle = GCHandle.Alloc(this);
                NativeApi.UnityARCore_session_setConfigCallback(m_SetConfigurationCallback, GCHandle.ToIntPtr(m_ProviderHandle));
            }

            public override void Start()
            {
                if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
                {
                    // Texture *must* be created before ARCore session resume is called
                    CreateTexture();
                }

                m_SessionId = Guid.NewGuid();
                NativeApi.UnityARCore_session_resume(m_SessionId);
            }

            public override void Stop() => NativeApi.UnityARCore_session_pause();

            public override void Update(XRSessionUpdateParams updateParams, Configuration configuration)
            {
                // For built-in render pipeline only. Check or configure Vulkan camera event callback.
#if !URP_7_OR_NEWER
                if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan)
                {
                    // Check per frame in case main camera changed due to user script or scene change.
                    bool cameraEventAdded = TryAddCameraEventCallbackForVulkanSupport();

                    if (cameraEventAdded && !s_VulkanSupportRendererFeatureEnabled)
                    {
                        OnVulkanSupportRendererFeatureEnabled();
                    }
                    else if (!cameraEventAdded && s_VulkanSupportRendererFeatureEnabled)
                    {
                        OnVulkanSupportRendererFeatureDisabled();
                    }
                }
#endif // !URP_7_OR_NEWER

                NativeApi.UnityARCore_session_update(
                    updateParams.screenOrientation,
                    updateParams.screenDimensions,
                    configuration.descriptor.identifier,
                    configuration.features);

                if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan && !s_VulkanSupportRendererFeatureEnabled)
                    throw new InvalidOperationException("If Vulkan is the Graphics API: "
                        + "1. When using URP, ARCommandBufferSupportRendererFeature must be added to the current renderer. "
                        + "2. When using built-in render pipeline, there must be an active main camera in the scene.");
            }

            public ArStatus StartRecording(ArRecordingConfig recordingConfig)
            {
                if (session == null)
                    throw new InvalidOperationException($"{nameof(StartRecording)} may not be called without a valid {nameof(session)}");

                return session.StartRecording(recordingConfig);
            }

            public ArStatus StopRecording()
            {
                if (session == null)
                    throw new InvalidOperationException($"{nameof(StopRecording)} may not be called without a valid {nameof(session)}");

                return session.StopRecording();
            }

            public ArRecordingStatus recordingStatus => session == null
                ? ArRecordingStatus.None
                : session.recordingStatus;

            public ArPlaybackStatus playbackStatus => session == null
                ? ArPlaybackStatus.None
                : session.playbackStatus;

            public ArSession session
            {
                get
                {
                    var ptr = nativePtr;
                    return ptr == IntPtr.Zero
                        ? default
                        : ArSession.FromIntPtr(Marshal.PtrToStructure<NativePtr>(ptr).sessionPtr);
                }
            }

            public override unsafe NativeArray<ConfigurationDescriptor> GetConfigurationDescriptors(Allocator allocator)
            {
                NativeApi.UnityARCore_session_getConfigurationDescriptors(out IntPtr ptr, out int count, out int stride);
                Assert.AreNotEqual(IntPtr.Zero, ptr, "Configuration descriptors pointer was null.");
                Assert.IsTrue(count > 0, "There are no configuration descriptors.");

                var descriptors = new NativeArray<ConfigurationDescriptor>(count, allocator);
                unsafe
                {
                    UnsafeUtility.MemCpyStride(
                        descriptors.GetUnsafePtr(),
                        UnsafeUtility.SizeOf<ConfigurationDescriptor>(),
                        (void*)ptr, stride, stride, count);
                }

                return descriptors;
            }

            /// <summary>
            /// Event that is triggered right before the configuration is set on the session. Allows changes to be made to the configuration before it is set.
            /// </summary>
            public event Action<ARCoreBeforeSetConfigurationEventArgs> beforeSetConfiguration;

            [MonoPInvokeCallback(typeof(Action<ArSession, ArConfig, IntPtr>))]
            static void SetConfigurationCallback(ArSession session, ArConfig config, IntPtr context)
            {
                var providerHandle = (GCHandle)context;

                if (providerHandle.Target is ARCoreProvider provider)
                {
                    provider.beforeSetConfiguration?.Invoke(new ARCoreBeforeSetConfigurationEventArgs(session, config));
                }
            }

            public override void Destroy()
            {
                m_ProviderHandle.Free();

                if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
                {
                    DeleteTexture();
                }

                NativeApi.UnityARCore_session_destroy();
            }

            public override void Reset()
            {
                m_SessionId = Guid.NewGuid();
                NativeApi.UnityARCore_session_reset();
                if (running)
                    Start();
            }

            public override void OnApplicationPause() => NativeApi.UnityARCore_session_onApplicationPause();

            public override void OnApplicationResume() => NativeApi.UnityARCore_session_onApplicationResume();

            public override Promise<SessionAvailability> GetAvailabilityAsync()
            {
                return ExecuteAsync<SessionAvailability>((context) =>
                {
                    NativeApi.ArPresto_checkApkAvailability(OnCheckApkAvailability, context);
                });
            }

            public override Promise<SessionInstallationStatus> InstallAsync()
            {
                return ExecuteAsync<SessionInstallationStatus>((context) =>
                {
                    NativeApi.ArPresto_requestApkInstallation(true, OnApkInstallation, context);
                });
            }

            public override IntPtr nativePtr => NativeApi.UnityARCore_session_getNativePtr();

            public override TrackingState trackingState => NativeApi.UnityARCore_session_getTrackingState();

            public override NotTrackingReason notTrackingReason => NativeApi.UnityARCore_session_getNotTrackingReason();

            public override Guid sessionId => m_SessionId;

            public override Feature requestedFeatures => Api.GetRequestedFeatures();

            public override Feature requestedTrackingMode
            {
                get => Api.GetRequestedFeatures();
                set
                {
                    Api.SetFeatureRequested(Feature.AnyTrackingMode, false);
                    Api.SetFeatureRequested(value, true);
                }
            }

            public override Feature currentTrackingMode => NativeApi.GetCurrentTrackingMode();

            public override bool matchFrameRateEnabled => NativeApi.UnityARCore_session_getMatchFrameRateEnabled();

            public override bool matchFrameRateRequested
            {
                get => NativeApi.UnityARCore_session_getMatchFrameRateRequested();
                set => NativeApi.UnityARCore_session_setMatchFrameRateRequested(value);
            }

            public override int frameRate
            {
                get
                {
                    if (ARCoreCameraSubsystem.TryGetCurrentConfiguration(out XRCameraConfiguration configuration) && configuration.framerate.HasValue)
                    {
                        return configuration.framerate.Value;
                    }

                    return 30;
                }
            }

            public override void OnCommandBufferSupportEnabled()
            {
                OnVulkanSupportRendererFeatureEnabled();
            }

            public override void OnCommandBufferExecute(CommandBuffer commandBuffer)
            {
                AddVulkanRenderSupportHandler(commandBuffer);
            }

            public override bool requiresCommandBuffer => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan;

            ArStatus SetPlaybackDatasetInternal(ArSession session, string datasetLocation, Func<string, ArStatus> setPlaybackFunction)
            {
                var shouldResume = !NativeApi.IsPauseDesired();

                Stop();

                // Spin-wait for the session to pause
                var status = setPlaybackFunction.Invoke(datasetLocation);
                while (status == ArStatus.ErrorSessionNotPaused)
                {
                    Thread.Yield();
                    status = setPlaybackFunction.Invoke(datasetLocation);
                }

                if (shouldResume)
                {
                    Start();
                }

                return status;
            }

            static ushort GetApiLevel()
            {
                // Set this to minimal allowed SDK version in 2023.2 for AR Foundation 6.0
                ushort apiLevel = 23;

                // returns a string similar to "Android OS 13 / API-33 (TQ3A.230901.001/10750268)"
                var osInfo = SystemInfo.operatingSystem;

                const string api = "API-";
                var apiIndex = osInfo.IndexOf(api, StringComparison.Ordinal);
                if (apiIndex < 0)
                    return apiLevel;

                var startIndex = apiIndex + api.Length;
                var endIndex = osInfo.IndexOf(" ", startIndex, StringComparison.Ordinal);
                if (endIndex <= startIndex)
                    return apiLevel;

                var apiString = osInfo.Substring(startIndex, endIndex - startIndex);
                if (short.TryParse(apiString, out var sdkVersion))
                    apiLevel = (ushort)sdkVersion;

                return apiLevel;
            }

            static Promise<T> ExecuteAsync<T>(Action<IntPtr> apiMethod)
            {
                var promise = new ARCorePromise<T>();
                GCHandle gch = GCHandle.Alloc(promise);
                apiMethod(GCHandle.ToIntPtr(gch));
                return promise;
            }

            [MonoPInvokeCallback(typeof(Action<NativeApi.ArPrestoApkInstallStatus, IntPtr>))]
            static void OnApkInstallation(NativeApi.ArPrestoApkInstallStatus status, IntPtr context)
            {
                var sessionInstallation = SessionInstallationStatus.None;
                switch (status)
                {
                    case NativeApi.ArPrestoApkInstallStatus.ErrorDeviceNotCompatible:
                        sessionInstallation = SessionInstallationStatus.ErrorDeviceNotCompatible;
                        break;

                    case NativeApi.ArPrestoApkInstallStatus.ErrorUserDeclined:
                        sessionInstallation = SessionInstallationStatus.ErrorUserDeclined;
                        break;

                    case NativeApi.ArPrestoApkInstallStatus.Requested:
                        // This shouldn't happen
                        sessionInstallation = SessionInstallationStatus.Error;
                        break;

                    case NativeApi.ArPrestoApkInstallStatus.Success:
                        sessionInstallation = SessionInstallationStatus.Success;
                        break;

                    case NativeApi.ArPrestoApkInstallStatus.Error:
                    default:
                        sessionInstallation = SessionInstallationStatus.Error;
                        break;
                }

                ResolvePromise(context, sessionInstallation);
            }

            [MonoPInvokeCallback(typeof(Action<NativeApi.ArAvailability, IntPtr>))]
            static void OnCheckApkAvailability(NativeApi.ArAvailability availability, IntPtr context)
            {
                var sessionAvailability = SessionAvailability.None;
                switch (availability)
                {
                    case NativeApi.ArAvailability.SupportedNotInstalled:
                    case NativeApi.ArAvailability.SupportedApkTooOld:
                        sessionAvailability = SessionAvailability.Supported;
                        break;

                    case NativeApi.ArAvailability.SupportedInstalled:
                        sessionAvailability = SessionAvailability.Supported | SessionAvailability.Installed;
                        break;

                    default:
                        sessionAvailability = SessionAvailability.None;
                        break;
                }

                ResolvePromise(context, sessionAvailability);
            }

            [MonoPInvokeCallback(typeof(NativeApi.CameraPermissionRequestProviderDelegate))]
            static void CameraPermissionRequestProvider(NativeApi.CameraPermissionsResultCallbackDelegate callback, IntPtr context)
            {
                ARCorePermissionManager.RequestPermission(k_CameraPermissionName, (permissionName, granted) =>
                {
                    callback(granted, context);
                });
            }

            static void ResolvePromise<T>(IntPtr context, T arg) where T : struct
            {
                GCHandle gch = GCHandle.FromIntPtr(context);
                var promise = (ARCorePromise<T>)gch.Target;
                if (promise != null)
                    promise.Resolve(arg);
                gch.Free();
            }

            void IssueRenderEventAndWaitForCompletion(NativeApi.RenderEvent renderEvent)
            {
                // NB: If m_RenderEventFunc is zero, it means
                //     1. We are running in the Editor.
                //     2. The UnityARCore library could not be loaded or similar catastrophic failure.
                if (m_RenderEventFunc != IntPtr.Zero)
                {
                    NativeApi.UnityARCore_session_setRenderEventPending();
                    GL.IssuePluginEvent(m_RenderEventFunc, (int)renderEvent);
                    NativeApi.UnityARCore_session_waitForRenderEvent();
                }
            }

            // Safe to call multiple times; does nothing if already created.
            void CreateTexture()
            {
                if (SystemInfo.graphicsMultiThreaded)
                {
                    IssueRenderEventAndWaitForCompletion(NativeApi.RenderEvent.CreateTexture);
                }
                else
                {
                    NativeApi.UnityARCore_session_createTextureMainThread();
                }
            }

            // Safe to call multiple times; does nothing if already destroyed.
            void DeleteTexture()
            {
                if (SystemInfo.graphicsMultiThreaded)
                {
                    IssueRenderEventAndWaitForCompletion(NativeApi.RenderEvent.DeleteTexture);
                }
                else
                {
                    NativeApi.UnityARCore_session_deleteTextureMainThread();
                }
            }

            const string k_CameraPermissionName = "android.permission.CAMERA";

            IntPtr m_RenderEventFunc;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            if (!Api.platformAndroid || !Api.loaderPresent)
                return;

            XRSessionSubsystemDescriptor.Register(new XRSessionSubsystemDescriptor.Cinfo
            {
                id = "ARCore-Session",
                providerType = typeof(ARCoreProvider),
                subsystemTypeOverride = typeof(ARCoreSessionSubsystem),
                supportsInstall = true,
                supportsMatchFrameRate = true
            });
        }

        /// <summary>
        /// An internal callback to set the flag when Vulkan plugin render thread callback is registered.
        /// Used for both URP and built-in render pipeline.
        /// </summary>
        internal static void OnVulkanSupportRendererFeatureEnabled()
        {
            if (s_VulkanTaskHandlerEventFunc != IntPtr.Zero)
            {
                s_VulkanSupportRendererFeatureEnabled = true;
                NativeApi.UnityARCore_session_onVulkanSupportRendererFeatureEnabled();
            }
        }

        /// <summary>
        /// An internal callback to set the flag when main camera changed and camera event command buffer needs to be re-added.
        /// Useful only for built-in render pipeline.
        /// </summary>
        internal static void OnVulkanSupportRendererFeatureDisabled()
        {
            if (s_VulkanTaskHandlerEventFunc != IntPtr.Zero)
            {
                s_VulkanSupportRendererFeatureEnabled = false;
                NativeApi.UnityARCore_session_onVulkanSupportRendererFeatureDisabled();
            }
        }

        /// <summary>
        /// This adds a command to the <paramref name="commandBuffer"/> to make call from the render thread
        /// to a callback on the `SessionProvider` implementation. The callback handles the native Vulkan
        /// command buffer recording and submitting for various subsystems.
        /// </summary>
        /// <param name="commandBuffer">The CommandBuffer object that allows us to enqueue plugin event for the render thread.</param>
        internal static void AddVulkanRenderSupportHandler(CommandBuffer commandBuffer)
        {
            if (s_VulkanTaskHandlerEventFunc != IntPtr.Zero)
            {
                commandBuffer.IssuePluginEvent(s_VulkanTaskHandlerEventFunc, 0);
            }
        }

#if !URP_7_OR_NEWER
        /// <summary>
        /// Support ARCore Vulkan rendering by injecting necessary plugin event callback to the main camera.
        /// Implemenation required for built-in render pipeline only.
        /// </summary>
        /// <remarks>
        /// If using URP 7 or newer then the event injection will be handled by ARCommandBufferSupportRendererFeature,
        /// in which case this path will be disabled.
        /// </remarks>
        /// <returns>Returns true if the callback is successfully added. Returns false if main camera is empty.</returns>
        internal static bool TryAddCameraEventCallbackForVulkanSupport()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return false;
            }

            // Return success if the event command buffer is already added.
            if (mainCamera == m_CameraWithPluginCallback)
            {
                return true;
            }

            // Create a new command buffer and queue commands if first time.
            if (m_PluginCallbackCommandBuffer == null)
            {
                m_PluginCallbackCommandBuffer = new CommandBuffer();
                m_PluginCallbackCommandBuffer.name = k_PluginCallbackCommandBufferName;
                AddVulkanRenderSupportHandler(m_PluginCallbackCommandBuffer);
            }

            // Remove the old command buffer in case it is a camera that has been used before.
            mainCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, m_PluginCallbackCommandBuffer);
            mainCamera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, m_PluginCallbackCommandBuffer);

            // Attach the command buffer to the camera.
            mainCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, m_PluginCallbackCommandBuffer);
            mainCamera.AddCommandBuffer(CameraEvent.BeforeGBuffer, m_PluginCallbackCommandBuffer);

            m_CameraWithPluginCallback = mainCamera;

            return true;
        }
#endif // !URP_7_OR_NEWER

        internal static class NativeApi
        {
            public enum ArPrestoApkInstallStatus
            {
                Uninitialized = 0,
                Requested = 1,
                Success = 100,
                Error = 200,
                ErrorDeviceNotCompatible = 201,
                ErrorUserDeclined = 203,
            }

            public enum ArAvailability
            {
                UnknownError = 0,
                UnknownChecking = 1,
                UnknownTimedOut = 2,
                UnsupportedDeviceNotCapable = 100,
                SupportedNotInstalled = 201,
                SupportedApkTooOld = 202,
                SupportedInstalled = 203
            }

            public enum TextureUpdateMode : short
            {
                DefaultOpenGLES = 0,
                HardwareBufferOpenGLES = 1,
                HardwareBufferVulkan = 2,
            }

            public enum RenderEvent
            {
                CreateTexture = 0,
                DeleteTexture = 1,
            }

            public delegate void CameraPermissionRequestProviderDelegate(
                CameraPermissionsResultCallbackDelegate resultCallback,
                IntPtr context);

            public delegate void CameraPermissionsResultCallbackDelegate(
                bool granted,
                IntPtr context);

            [DllImport(Constants.k_LibraryName)]
            public static extern IntPtr UnityARCore_session_getNativePtr();

            [DllImport(Constants.k_LibraryName)]
            public static extern void ArPresto_checkApkAvailability(
                Action<ArAvailability, IntPtr> onResult, IntPtr context);

            [DllImport(Constants.k_LibraryName)]
            public static extern void ArPresto_requestApkInstallation(
                bool userRequested, Action<ArPrestoApkInstallStatus, IntPtr> onResult, IntPtr context);

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_update(
                ScreenOrientation orientation,
                Vector2Int screenDimensions,
                IntPtr configId,
                Feature features);

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_getConfigurationDescriptors(
                out IntPtr ptr, out int count, out int stride);

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_construct(
                CameraPermissionRequestProviderDelegate cameraPermissionRequestProvider,
                TextureUpdateMode textureUpdateMode,
                bool multiThreadedRendering);

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_destroy();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_resume(Guid guid);

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_pause();

            [DllImport(Constants.k_LibraryName, EntryPoint = "UnityARCore_session_isPauseDesired")]
            public static extern bool IsPauseDesired();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_onApplicationResume();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_onApplicationPause();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_reset();

            [DllImport(Constants.k_LibraryName)]
            public static extern TrackingState UnityARCore_session_getTrackingState();

            [DllImport(Constants.k_LibraryName)]
            public static extern NotTrackingReason UnityARCore_session_getNotTrackingReason();

            [DllImport(Constants.k_LibraryName)]
            public static extern IntPtr UnityARCore_session_getRenderEventFunc();

            [DllImport(Constants.k_LibraryName)]
            public static extern IntPtr UnityARCore_session_getVulkanTaskHandlerEventFunc();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_onVulkanSupportRendererFeatureEnabled();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_onVulkanSupportRendererFeatureDisabled();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_setRenderEventPending();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_waitForRenderEvent();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_createTextureMainThread();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_deleteTextureMainThread();

            [DllImport(Constants.k_LibraryName)]
            public static extern bool UnityARCore_session_getMatchFrameRateEnabled();

            [DllImport(Constants.k_LibraryName)]
            public static extern bool UnityARCore_session_getMatchFrameRateRequested();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_setMatchFrameRateRequested(bool value);

            [DllImport(Constants.k_LibraryName, EntryPoint="UnityARCore_session_getCurrentTrackingMode")]
            public static extern Feature GetCurrentTrackingMode();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_setConfigurationDirty();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_session_setConfigCallback(Action<ArSession, ArConfig, IntPtr> callback, IntPtr context);
        }
    }
}
