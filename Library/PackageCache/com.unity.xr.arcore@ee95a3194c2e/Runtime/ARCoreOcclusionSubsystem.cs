using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.XR.CoreUtils.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARCore
{
    /// <summary>
    /// The ARCore implementation of the
    /// [XROcclusionSubsystem](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystem).
    /// Do not create this directly. Use the
    /// [SubsystemManager](xref:UnityEngine.SubsystemManager)
    /// instead.
    /// </summary>
    [Preserve]
    public sealed class ARCoreOcclusionSubsystem : XROcclusionSubsystem
    {
        /// <summary>
        /// Registers the ARCore occlusion subsystem if iOS and not the editor.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Register()
        {
            if (!Api.platformAndroid || !Api.loaderPresent)
                return;

            const string k_SubsystemId = "ARCore-Occlusion";

            var occlusionSubsystemCinfo = new XROcclusionSubsystemDescriptor.Cinfo()
            {
                id = k_SubsystemId,
                providerType = typeof(ARCoreOcclusionSubsystem.ARCoreProvider),
                subsystemTypeOverride = typeof(ARCoreOcclusionSubsystem),
                environmentDepthImageSupportedDelegate = NativeApi.UnityARCore_OcclusionProvider_DoesSupportEnvironmentDepth,
                // Confidence and smoothing is implicitly supported if environment depth is supported.
                environmentDepthConfidenceImageSupportedDelegate = NativeApi.UnityARCore_OcclusionProvider_DoesSupportEnvironmentDepth,
                environmentDepthTemporalSmoothingSupportedDelegate = NativeApi.UnityARCore_OcclusionProvider_DoesSupportEnvironmentDepth,
            };

            XROcclusionSubsystemDescriptor.Register(occlusionSubsystemCinfo);
        }

        class ARCoreProvider : Provider
        {
            const string k_TextureEnvironmentDepthPropertyName = "_EnvironmentDepth";
            const string k_EnvironmentDepthEnabledMaterialKeyword = "ARCORE_ENVIRONMENT_DEPTH_ENABLED";
            static readonly int k_TextureEnvironmentDepthPropertyId =
                Shader.PropertyToID(k_TextureEnvironmentDepthPropertyName);

            static readonly List<string> k_EnvironmentDepthShaderKeywords = new(){ k_EnvironmentDepthEnabledMaterialKeyword };
            static readonly ReadOnlyList<string> k_EnvironmentDepthShaderKeywordsReadOnly = new(k_EnvironmentDepthShaderKeywords);
            static readonly XRShaderKeywords k_DepthEnabledShaderKeywords =new(k_EnvironmentDepthShaderKeywordsReadOnly, null);
            static readonly XRShaderKeywords k_DepthDisabledShaderKeywords = new(null, k_EnvironmentDepthShaderKeywordsReadOnly);

            OcclusionPreferenceMode m_OcclusionPreferenceMode;

            public ARCoreProvider()
            {
                bool supportsR16 = SystemInfo.SupportsTextureFormat(TextureFormat.R16);
                bool supportsRHalf = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf);
                bool supportsRenderTextureRHalf = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf);
                bool useAdvancedRendering = supportsR16 && supportsRHalf && supportsRenderTextureRHalf;
                NativeApi.UnityARCore_OcclusionProvider_Construct(k_TextureEnvironmentDepthPropertyId, useAdvancedRendering);
            }

            public override void Start() => NativeApi.UnityARCore_OcclusionProvider_Start();
            public override void Stop() => NativeApi.UnityARCore_OcclusionProvider_Stop();
            public override void Destroy() => NativeApi.UnityARCore_OcclusionProvider_Destruct();

            public override EnvironmentDepthMode requestedEnvironmentDepthMode
            {
                get => NativeApi.UnityARCore_OcclusionProvider_GetRequestedEnvironmentDepthMode();
                set
                {
                    NativeApi.UnityARCore_OcclusionProvider_SetRequestedEnvironmentDepthMode(value);
                    Api.SetFeatureRequested(Feature.EnvironmentDepth, value.Enabled());
                }
            }

            public override EnvironmentDepthMode currentEnvironmentDepthMode
                => NativeApi.UnityARCore_OcclusionProvider_GetCurrentEnvironmentDepthMode();

            public override bool environmentDepthTemporalSmoothingEnabled =>
                NativeApi.UnityARCore_OcclusionProvider_GetEnvironmentDepthTemporalSmoothingEnabled();

            public override bool environmentDepthTemporalSmoothingRequested
            {
                get => Api.GetRequestedFeatures().Any(Feature.EnvironmentDepthTemporalSmoothing);
                set => Api.SetFeatureRequested(Feature.EnvironmentDepthTemporalSmoothing, value);
            }

            public override OcclusionPreferenceMode requestedOcclusionPreferenceMode
            {
                get => m_OcclusionPreferenceMode;
                set => m_OcclusionPreferenceMode = value;
            }

            public override OcclusionPreferenceMode currentOcclusionPreferenceMode => m_OcclusionPreferenceMode;

            public override bool TryGetEnvironmentDepth(out XRTextureDescriptor environmentDepthDescriptor)
                => NativeApi.UnityARCore_OcclusionProvider_TryGetEnvironmentDepth(out environmentDepthDescriptor);

            /// <remarks>
            /// If  <see cref='environmentDepthTemporalSmoothingEnabled'/> is <c>true</c> then the CPU image construction information
            /// will be for the temporally smoothed environmental depth image otherwise it will be for the raw environmental depth image.
            /// </remarks>
            public override bool TryAcquireEnvironmentDepthCpuImage(out XRCpuImage.Cinfo cinfo)
            {
                return environmentDepthTemporalSmoothingEnabled
                    ? ARCoreCpuImageApi.TryAcquireLatestImage(ARCoreCpuImageApi.ImageType.EnvironmentDepth, out cinfo)
                    : ARCoreCpuImageApi.TryAcquireLatestImage(ARCoreCpuImageApi.ImageType.RawEnvironmentDepth, out cinfo);
            }

            public override bool TryAcquireRawEnvironmentDepthCpuImage(out XRCpuImage.Cinfo cinfo) =>
                ARCoreCpuImageApi.TryAcquireLatestImage(ARCoreCpuImageApi.ImageType.RawEnvironmentDepth, out cinfo);

            public override bool TryAcquireSmoothedEnvironmentDepthCpuImage(out XRCpuImage.Cinfo cinfo) =>
                ARCoreCpuImageApi.TryAcquireLatestImage(ARCoreCpuImageApi.ImageType.EnvironmentDepth, out cinfo);

            public override XRCpuImage.Api environmentDepthCpuImageApi => ARCoreCpuImageApi.instance;

            public override bool TryGetEnvironmentDepthConfidence(out XRTextureDescriptor environmentDepthConfidenceDescriptor)
                => NativeApi.UnityARCore_OcclusionProvider_TryGetEnvironmentDepthConfidence(out environmentDepthConfidenceDescriptor);

            public override bool TryAcquireEnvironmentDepthConfidenceCpuImage(out XRCpuImage.Cinfo cinfo)
                => ARCoreCpuImageApi.TryAcquireLatestImage(
                    ARCoreCpuImageApi.ImageType.RawEnvironmentDepthConfidence,
                    out cinfo);

            public override XRCpuImage.Api environmentDepthConfidenceCpuImageApi => ARCoreCpuImageApi.instance;

            public override unsafe NativeArray<XRTextureDescriptor> GetTextureDescriptors(
                XRTextureDescriptor defaultDescriptor,
                Allocator allocator)
            {
                var textureDescriptors = NativeApi.UnityARCore_OcclusionProvider_AcquireTextureDescriptors(
                    out var length,
                    out var elementSize);

                try
                {
                    return NativeCopyUtility.PtrToNativeArrayWithDefault(
                        defaultDescriptor,
                        textureDescriptors,
                        elementSize,
                        length,
                        allocator);
                }
                finally
                {
                    NativeApi.UnityARCore_OcclusionProvider_ReleaseTextureDescriptors(textureDescriptors);
                }
            }

            [Obsolete]
            public override void GetMaterialKeywords(out List<string> enabledKeywords, out List<string> disabledKeywords)
#pragma warning restore CS0672
            {
                bool isEnvDepthEnabled = NativeApi.UnityARCore_OcclusionProvider_IsEnvironmentDepthEnabled();

                if (m_OcclusionPreferenceMode == OcclusionPreferenceMode.NoOcclusion || !isEnvDepthEnabled)
                {
                    enabledKeywords = null;
                    disabledKeywords = k_EnvironmentDepthShaderKeywords;
                }
                else
                {
                    enabledKeywords = k_EnvironmentDepthShaderKeywords;
                    disabledKeywords = null;
                }
            }

            [Obsolete]
            public override ShaderKeywords GetShaderKeywords()
            {
                var isEnvDepthEnabled = NativeApi.UnityARCore_OcclusionProvider_IsEnvironmentDepthEnabled();

                return m_OcclusionPreferenceMode == OcclusionPreferenceMode.NoOcclusion || !isEnvDepthEnabled
                    ? new ShaderKeywords(null, k_EnvironmentDepthShaderKeywords.AsReadOnly())
                    : new ShaderKeywords(k_EnvironmentDepthShaderKeywords.AsReadOnly(), null);
            }

            public override XRShaderKeywords GetShaderKeywords2()
            {
                var isEnvDepthEnabled = NativeApi.UnityARCore_OcclusionProvider_IsEnvironmentDepthEnabled();

                return m_OcclusionPreferenceMode == OcclusionPreferenceMode.NoOcclusion || !isEnvDepthEnabled
                    ? k_DepthDisabledShaderKeywords
                    : k_DepthEnabledShaderKeywords;
            }
        }

        static class NativeApi
        {
            [DllImport(Constants.k_LibraryName)]
            public static extern Supported UnityARCore_OcclusionProvider_DoesSupportEnvironmentDepth();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_OcclusionProvider_Construct(int textureEnvDepthPropertyId, bool useAdvancedRendering);

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_OcclusionProvider_Start();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_OcclusionProvider_Stop();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_OcclusionProvider_Destruct();

            [DllImport(Constants.k_LibraryName)]
            public static extern EnvironmentDepthMode UnityARCore_OcclusionProvider_GetRequestedEnvironmentDepthMode();

            [DllImport(Constants.k_LibraryName)]
            public static extern void UnityARCore_OcclusionProvider_SetRequestedEnvironmentDepthMode(
                EnvironmentDepthMode environmentDepthMode);

            [DllImport(Constants.k_LibraryName)]
            public static extern EnvironmentDepthMode UnityARCore_OcclusionProvider_GetCurrentEnvironmentDepthMode();

            [DllImport(Constants.k_LibraryName)]
            public static extern bool UnityARCore_OcclusionProvider_TryGetEnvironmentDepth(
                out XRTextureDescriptor envDepthDescriptor);

            [DllImport(Constants.k_LibraryName)]
            public static extern unsafe void* UnityARCore_OcclusionProvider_AcquireTextureDescriptors(
                out int length, out int elementSize);

            [DllImport(Constants.k_LibraryName)]
            public static extern unsafe void UnityARCore_OcclusionProvider_ReleaseTextureDescriptors(void* descriptors);

            [DllImport(Constants.k_LibraryName)]
            public static extern bool UnityARCore_OcclusionProvider_IsEnvironmentDepthEnabled();

            [DllImport(Constants.k_LibraryName)]
            public static extern bool UnityARCore_OcclusionProvider_GetEnvironmentDepthTemporalSmoothingEnabled();

            [DllImport(Constants.k_LibraryName)]
            public static extern bool UnityARCore_OcclusionProvider_TryGetEnvironmentDepthConfidence(
                out XRTextureDescriptor environmentDepthConfidenceDescriptor);
        }
    }
}
