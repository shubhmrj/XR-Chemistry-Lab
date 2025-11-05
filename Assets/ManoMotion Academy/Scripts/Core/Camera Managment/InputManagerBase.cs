using System;
using System.IO;
using UnityEngine;
using System.Collections;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ManoMotion.CameraSystem
{
    /// <summary>
    /// Base class for InputManagers.
    /// </summary>
    public abstract class InputManagerBase : MonoBehaviour
    {
        [SerializeField, Range(1, 10), Tooltip("Value to scale down image by in SDK")] protected int splittingFactor = 1;
        [SerializeField] protected bool isFrontFacing;

        protected ManoMotionFrame currentFrame;

        protected const int LOWEST_RESOLUTION_VALUE = 480;

        public static Action<ManoMotionFrame> OnFrameInitialized;
        public static Action<ManoMotionFrame> OnFrameResized;
        public static Action<ManoMotionFrame> OnFrameUpdated;

        public static FrameInitializedPointer OnFrameInitializedPointer;
        public delegate void FrameInitializedPointer(Texture2D image, int splittingFactor = 1);

        public static Action<AddOn> OnAddonSet;
        public static Action OnChangeCamera;
        public static Action OnCameraFacingChanged;

        public bool IsFrontFacing => isFrontFacing;

        public virtual bool IsFrameUpdated() { return true; }

        /// <summary>
        /// Used to change between back- and front facing camera/scenario 
        /// Update with specific changes for each InputManager
        /// </summary>
        public void SetFrontFacing(bool isFrontFacing)
        {
            if (this.isFrontFacing != isFrontFacing)
            {
                this.isFrontFacing = isFrontFacing;
                UpdateFrontFacing(isFrontFacing);
                OnCameraFacingChanged?.Invoke();
            }
        }

        protected virtual void UpdateFrontFacing(bool isFrontFacing) { }

        /// <summary>
        /// Forces the application to ask for camera permissions and external storage read and write.
        /// </summary>
        protected virtual void ForceApplicationPermissions()
        {
#if UNITY_ANDROID
            /* Since 2018.3, Unity doesn't automatically handle permissions on Android, ask for camera permissions. */
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }
#endif
        }

        /// <summary>
        /// Checks if the app has storage permissions
        /// </summary>
        public void StoragePermissionCheck()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
                Debug.Log("I dont have external write");
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
                Debug.Log("I dont have external read");
            }
#endif
        }

        /// <summary>
        /// Flips the texture horizontally
        /// </summary>
        /// <param name="original">Texture to filp</param>
        protected void FlipTextureHorizontal(ref Texture2D original)
        {
            int textureWidth = original.width;
            int textureHeight = original.height;

            Color[] colorArray = original.GetPixels();

            for (int j = 0; j < textureHeight; j++)
            {
                int rowStart = 0;
                int rowEnd = textureWidth - 1;

                while (rowStart < rowEnd)
                {
                    Color hold = colorArray[(j * textureWidth) + (rowStart)];
                    colorArray[(j * textureWidth) + (rowStart)] = colorArray[(j * textureWidth) + (rowEnd)];
                    colorArray[(j * textureWidth) + (rowEnd)] = hold;
                    rowStart++;
                    rowEnd--;
                }
            }

            original.SetPixels(colorArray);
            original.Apply();
        }

        /// <summary>
        /// Flips the texture vertically.
        /// </summary>
        /// <param name="orignal">Texture to flip</param>
        protected void FlipTextureVertical(ref Texture2D orignal)
        {
            int width = orignal.width;
            int height = orignal.height;
            Color[] pixels = orignal.GetPixels();
            Color[] pixelsFlipped = orignal.GetPixels();
            for (int i = 0; i < height; i++)
            {
                Array.Copy(pixels, i * width, pixelsFlipped, (height - i - 1) * width, width);
            }
            orignal.SetPixels(pixelsFlipped);
            orignal.Apply();
        }

        /// <summary>
        /// Saves the texture to file in png format.
        /// </summary>
        protected void SaveImage(Texture2D image, string filename)
        {
            byte[] bytes = image.EncodeToPNG();
            File.WriteAllBytes(Application.persistentDataPath + "/" + filename, bytes);
        }

        #region Application on Background

        /// <summary>
        /// Stops processing when application is put to background.
        /// </summary>
        protected void OnApplicationFocus(bool focus)
        {
            RunCameraCapture(focus);
        }

        /// <summary>
        /// Stops the processing if application is paused.
        /// </summary>
        protected void OnApplicationPause(bool paused)
        {
            RunCameraCapture(!paused);
        }

        /// <summary>
        /// Call to resume or stop processing/camera updates
        /// </summary>
        private void RunCameraCapture(bool run)
        {
            if (run)
                ResumeCameraCapture();
#if !UNITY_EDITOR
            else
                ManoMotionManager.Instance.StopProcessing();
#endif
        }

        protected void ResumeCameraCapture()
        {
            if (currentFrame.texture)
                OnFrameInitializedPointer?.Invoke(currentFrame.texture, splittingFactor);
        }

        protected virtual void OnEnable()
        {
            ManoUtils.OnOrientationChanged += OrientationChanged;
            ResumeCameraCapture();
        }

        protected virtual void OnDisable()
        {
            ManoUtils.OnOrientationChanged -= OrientationChanged;
            ManoMotionManager.Instance.StopProcessing();
        }

        protected void OrientationChanged()
        {
            StartCoroutine(SetTexture());

            IEnumerator SetTexture()
            {
                // Need to wait a frame before updating the texture reference after orientation changed.
                yield return null;
                if (currentFrame.texture)
                    OnFrameInitializedPointer?.Invoke(currentFrame.texture, splittingFactor);
            }
        }

        #endregion
    }
}