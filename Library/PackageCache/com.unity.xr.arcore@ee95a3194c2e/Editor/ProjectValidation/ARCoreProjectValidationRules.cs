using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils.Editor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEngine.Rendering;
using UnityEngine.XR.ARCore;

namespace UnityEditor.XR.ARCore
{
    static class ARCoreProjectValidationRules
    {
        const string k_PreferencesExternalTools = "Preferences/External Tools";
        const string k_Catergory = "Google ARCore";
        const string k_GradleVersionUnknown = "cannot be determined";
        static readonly Version k_MinimumGradleVersion = new(5, 6, 4);

        [InitializeOnLoadMethod]
        static void AddARCoreValidationRules()
        {
            const AndroidSdkVersions minSupportedSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            const AndroidSdkVersions minSdkVersionWithVulkan = AndroidSdkVersions.AndroidApiLevel29;
            const AndroidSdkVersions minSdkVersionWithOpenGLES3 = AndroidSdkVersions.AndroidApiLevel24;
            // Minimum required is ApiLevel 14, however Unity's minimum is always higher than 14
            const AndroidSdkVersions minSdkVersionOptional = minSupportedSdkVersion;

            // When adding a new validation rule, please remember to add it in the docs also with a user-friendly description
            var androidGlobalRules = new[]
            {
                new BuildValidationRule
                {
                    Category = k_Catergory,
                    Message = "It is recommended to use OpenGLES3 as the second Graphics API when using Vulkan as first Graphics API and AR is 'Required', so that devices that do not support the needed Vulkan extension fall back to a working renderer.",
                    IsRuleEnabled = IsARCorePluginEnabled,
                    CheckPredicate = () =>
                    {
                        var graphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                        return graphicsApis[0] == GraphicsDeviceType.Vulkan ? graphicsApis.Contains(GraphicsDeviceType.OpenGLES3) : true;
                    },
                    FixItMessage = "Open Project Settings > Player Settings > Android tab. In the list of 'Graphics APIs', make sure that " +
                                   "'Vulkan' is listed as the first API, and 'OpenGLES3' as the second one.",
                    FixIt = () =>
                    {
                        var graphicApis =
                            new List<GraphicsDeviceType>(PlayerSettings.GetGraphicsAPIs(BuildTarget.Android))
                            {
                                GraphicsDeviceType.OpenGLES3
                            };

                        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, graphicApis.ToArray());
                    },
                    Error = false
                },
                new BuildValidationRule
                {
                    Category = k_Catergory,
                    Message = $"With Vulkan Graphics API, Google ARCore requires targeting minimum Android 10.0 API level {minSdkVersionWithVulkan} when AR is 'Required', or Android 4.0 'Ice Cream Sandwich' API Level {minSdkVersionOptional} when AR is 'Optional' (currently: {PlayerSettings.Android.minSdkVersion}).",
                    IsRuleEnabled = IsARCorePluginEnabled,
                    CheckPredicate = () =>
                    {
                        var graphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                        if (graphicsApis.Length == 0 || graphicsApis[0] != GraphicsDeviceType.Vulkan)
                            return true;

                        var arcoreSettings = ARCoreSettings.GetOrCreateSettings();
                        var minSdkVersion = arcoreSettings.requirement == ARCoreSettings.Requirement.Optional ? minSdkVersionOptional : minSdkVersionWithVulkan;

                        return PlayerSettings.Android.minSdkVersion >= minSdkVersion;
                    },
                    FixItMessage = "Open Project Settings > Player Settings > Android tab and increase the 'Minimum API Level' to" +
                        $" 'API Level {minSdkVersionWithVulkan}' (if using Vulkan) for AR Required," +
                        $" and to 'API Level {minSdkVersionOptional}' or greater for AR Optional.",
                    FixIt = () =>
                    {
                        var graphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                        var minRequiredVersion = graphicsApis.Length > 0 && graphicsApis[0] == GraphicsDeviceType.Vulkan ? minSdkVersionWithVulkan : minSdkVersionWithOpenGLES3;

                        var arcoreSettings = ARCoreSettings.GetOrCreateSettings();
                        var minSdkVersion = arcoreSettings.requirement == ARCoreSettings.Requirement.Optional ? minSdkVersionOptional : minRequiredVersion;
                        PlayerSettings.Android.minSdkVersion = minSdkVersion;
                    },
                    Error = true
                },
                new BuildValidationRule
                {
                    Category = k_Catergory,
                    Message = $"With OpenGLES3 Graphics API, Google ARCore requires targeting minimum Android 7.0 'Nougat' API level {minSdkVersionWithOpenGLES3} when AR is 'Required', or Android 4.0 'Ice Cream Sandwich' API Level {minSdkVersionOptional} when AR is 'Optional' (currently: {PlayerSettings.Android.minSdkVersion}).",
                    IsRuleEnabled = IsARCorePluginEnabled,
                    CheckPredicate = () =>
                    {
                        var graphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                        if (graphicsApis.Length == 0 || graphicsApis[0] != GraphicsDeviceType.OpenGLES3)
                            return true;

                        var arcoreSettings = ARCoreSettings.GetOrCreateSettings();
                        var minSdkVersion = arcoreSettings.requirement == ARCoreSettings.Requirement.Optional ? minSupportedSdkVersion : minSdkVersionWithOpenGLES3;

                        return PlayerSettings.Android.minSdkVersion >= minSdkVersion;
                    },
                    FixItMessage = "Open Project Settings > Player Settings > Android tab and increase the 'Minimum API Level' to" +
                        $" 'API Level {minSdkVersionWithOpenGLES3}' or greater for AR Required," +
                        $" and to 'API Level {minSupportedSdkVersion}' or greater for AR Optional.",
                    FixIt = () =>
                    {
                        var graphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                        var minRequiredVersion = graphicsApis.Length > 0 && graphicsApis[0] == GraphicsDeviceType.OpenGLES3 ? minSdkVersionWithOpenGLES3 : minSdkVersionWithVulkan;

                        var arcoreSettings = ARCoreSettings.GetOrCreateSettings();
                        var minSdkVersion = arcoreSettings.requirement == ARCoreSettings.Requirement.Optional ? minSdkVersionOptional : minRequiredVersion;
                        PlayerSettings.Android.minSdkVersion = minSdkVersion;
                    },
                    Error = true
                },
                new BuildValidationRule
                {
                    Category = k_Catergory,
                    Message = "Google ARCore requires OpenGLES3 or Vulkan graphics API.",
                    IsRuleEnabled = IsARCorePluginEnabled,
                    CheckPredicate = () =>
                    {
                        var graphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                        return graphicsApis.Length > 0 &&
                            graphicsApis[0] == GraphicsDeviceType.OpenGLES3 ||
                            graphicsApis[0] == GraphicsDeviceType.Vulkan;
                    },
                    FixItMessage = "Open Project Settings > Player Settings > Android tab and disable " +
                                   "'Auto Graphics API'. In the list of 'Graphics APIs', make sure that " +
                                   " either 'OpenGLES3' or 'Vulkan' is listed as the first API.",
                    FixIt = () =>
                    {
                        var currentGraphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                        GraphicsDeviceType[] correctGraphicsApis;
                        if (currentGraphicsApis.Length == 0)
                        {
                            correctGraphicsApis = new[]
                            {
                                GraphicsDeviceType.OpenGLES3
                            };
                        }
                        else
                        {
                            var graphicApis = new List<GraphicsDeviceType>(currentGraphicsApis.Length);
                            graphicApis.Add(GraphicsDeviceType.OpenGLES3);
                            foreach (var graphicsApi in currentGraphicsApis)
                            {
                                if (graphicsApi != GraphicsDeviceType.OpenGLES3)
                                    graphicApis.Add(graphicsApi);
                            }

                            correctGraphicsApis = graphicApis.ToArray();
                        }

                        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
                        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, correctGraphicsApis);
                    },
                    Error = true
                },
                new BuildValidationRule
                {
                    Category = k_Catergory,
                    Message = "IL2CPP scripting backend and ARM64 architecture is recommended for Google ARCore.",
                    HelpLink = "https://developers.google.com/ar/64bit",
                    IsRuleEnabled = IsARCorePluginEnabled,
                    CheckPredicate = () =>
                    {
                        return PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP
                            && (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) != AndroidArchitecture.None;
                    },
                    FixItMessage = "Open Project Settings > Player Settings > Android tab and ensure 'Scripting Backend'" +
                        " is set to 'IL2CPP'. Then under 'Target Architectures' enable 'ARM64'.",
                    FixIt = () =>
                    {
                        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
                        PlayerSettings.Android.targetArchitectures |= AndroidArchitecture.ARM64;
                    },
                    Error = false
                },
                new BuildValidationRule
                {
                    Category = k_Catergory,
                    Message = $"Google ARCore requires at least Gradle version {k_MinimumGradleVersion} (currently: {GetGradleVersionString()}).",
                    HelpLink = "https://developers.google.com/ar/develop/unity-arf/android-11-build",
                    IsRuleEnabled = () => IsARCorePluginEnabled() && EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android,
                    CheckPredicate = () =>
                    {
                        var settings = ARCoreSettings.GetOrCreateSettings();
                        if (settings.ignoreGradleVersion)
                            return true;

                        var gradleVersion = GetGradleVersion();
                        return gradleVersion >= k_MinimumGradleVersion;
                    },
                    FixItMessage = $"Open Preferences > External Tools > Gradle and update the path to a Gradle version {k_MinimumGradleVersion} or greater.",
                    FixIt = () =>
                    {
                        SettingsService.OpenUserPreferences(k_PreferencesExternalTools);
                    },
                    Error = true
                }
            };

            BuildValidator.AddRules(BuildTargetGroup.Android, androidGlobalRules);
        }

        static bool IsARCorePluginEnabled()
        {
            var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(
                BuildTargetGroup.Android);
            if (generalSettings == null)
                return false;

            var managerSettings = generalSettings.AssignedSettings;

            return managerSettings != null && managerSettings.activeLoaders.Any(loader => loader is ARCoreLoader);
        }

        static Version GetGradleVersion()
        {
            return Gradle.TryGetVersion(out var gradleVersion, out var _) ? gradleVersion : new Version(0, 0);
        }

        static string GetGradleVersionString()
        {
            var gradleVersion = GetGradleVersion();

            return gradleVersion.Major != 0 ? gradleVersion.ToString() : k_GradleVersionUnknown;
        }
    }
}
