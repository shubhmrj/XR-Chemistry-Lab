using UnityEngine;
using UnityEditor;

namespace ManoMotion.Editor
{
    /// <summary>
    /// Updates the Project Settings to work for Windows, Android and iOS.
    /// </summary>
    [InitializeOnLoad]
    public class ManoMotionSetup
    {
        /// <summary>
        /// Automaticly sets up the "Project Settings/Player/Other Settings" for you.
        /// </summary>
        static ManoMotionSetup()
        {
            Debug.Log("Setting up ManoMotion Library Requirements");

            // General
            PlayerSettings.colorSpace = ColorSpace.Linear;

            // Android
            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.PreferExternal;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });

            // iOS
            int arm64Architecture = 1;
            PlayerSettings.iOS.cameraUsageDescription = "This application requires camera permission in order to detect a hand when you place it in front of the camera and understand the gesture interaction.";
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, arm64Architecture);

            // Windows
#if UNITY_STANDALONE_WIN
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
#endif
            Debug.Log("Successfully set up ManoMotion Library Requirements");
        }
    }
}