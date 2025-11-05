using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ManoMotion.CameraSystem
{
    /// <summary>
    /// Clones the camera image from ARCameraBackground and broadcasts camera updates.
    /// </summary>
    public class InputManagerARFoundation : InputManagerBase
    {
        [SerializeField] ARCameraBackground arCameraBackground;
        [SerializeField] ARCameraManager arCameraManager;

        private RenderTexture inputRenderTexture;
        int width, height;

        const TextureFormat ImageTextureFormat = TextureFormat.RGBA32;
        const int TextureDepth = 16; // Depth necessary to grab AR background with RenderTexture.

        private void Awake()
        {
            arCameraBackground ??= FindObjectOfType<ARCameraBackground>();
            arCameraManager ??= FindObjectOfType<ARCameraManager>();

            UpdateFrontFacing(isFrontFacing);
            StoragePermissionCheck();
            ForceApplicationPermissions();
        }

        private void Start()
        {
            SetResolutionValues();

            currentFrame = new ManoMotionFrame();
            ResizeFrames();

            OnAddonSet?.Invoke(AddOn.ARFoundation);
            OnFrameInitialized?.Invoke(currentFrame);
        }

        /// <summary>
        /// Set width and height so that the smallest value will be LOWEST_RESOLUTION_VALUE while keeping the aspect ratio.
        /// Will always correspond to what width x height would be in portrait orientation.
        /// </summary>
        private void SetResolutionValues()
        {
            int screenWidth;
            int screenHeight;

            switch (Input.deviceOrientation)
            {
                case DeviceOrientation.LandscapeLeft:
                case DeviceOrientation.LandscapeRight:
                    screenWidth = Screen.height;
                    screenHeight = Screen.width;
                    break;
                default:
                    screenWidth = Screen.width;
                    screenHeight = Screen.height;
                    break;
            }

            float ratio = (float)LOWEST_RESOLUTION_VALUE / Mathf.Min(screenWidth, screenHeight);
            width = (int)(screenWidth * ratio);
            height = (int)(screenHeight * ratio);
        }

        protected override void UpdateFrontFacing(bool isFrontFacing)
        {
            arCameraManager.requestedFacingDirection = isFrontFacing ? CameraFacingDirection.User : CameraFacingDirection.World;
        }

        private void LateUpdate()
        {
            if (arCameraBackground.material == null)
            {
                Debug.LogError("arCameraBackground.material is NULL!");
                return;
            }

            // Copies the camera frame
            Graphics.Blit(null, inputRenderTexture, arCameraBackground.material);

            // Writes the pixels from RenderTexture to Texture2D
            currentFrame.texture.ReadPixels(new Rect(0, 0, inputRenderTexture.width, inputRenderTexture.height), 0, 0);
            currentFrame.orientation = Input.deviceOrientation;

            OnFrameUpdated?.Invoke(currentFrame);
            
            // Pointer changes in iOS so need to update it every frame.
#if UNITY_IOS
            OnFrameInitializedPointer?.Invoke(currentFrame.texture, splittingFactor);
#endif
        }

        protected override void OnEnable() => ManoUtils.OnOrientationChanged += ResizeFrames;

        protected override void OnDisable() => ManoUtils.OnOrientationChanged -= ResizeFrames;

        /// <summary>
        /// Calls the main methods of resizing the RenderTexture (Camera Clone) and Texture2D (Visual information).
        /// Informs the subscribers of this event that the frames have been resized
        /// </summary>
        void ResizeFrames()
        {
            switch (ManoUtils.Instance.Orientation)
            {
                case SupportedOrientation.UNKNOWN:
                case SupportedOrientation.PORTRAIT:
                case SupportedOrientation.PORTRAIT_UPSIDE_DOWN:
                case SupportedOrientation.PORTRAIT_FRONT_FACING:
                case SupportedOrientation.PORTRAIT_UPSIDE_DOWN_FRONT_FACING:
                    SetResolution(width, height);
                    break;
                case SupportedOrientation.LANDSCAPE_LEFT:
                case SupportedOrientation.LANDSCAPE_LEFT_FRONT_FACING:
                case SupportedOrientation.LANDSCAPE_RIGHT:
                case SupportedOrientation.LANDSCAPE_RIGHT_FRONT_FACING:
                    SetResolution(height, width);
                    break;
            }
        }

        /// <summary>
        /// Resizes the dimensions of the Render Texture that is used to get the image colors from ARFoundation.
        /// </summary>
        private void SetResolution(int width, int height)
        {
            RenderTexture oldRenderTexture = inputRenderTexture;

            // Initializes the Input Parameters
            inputRenderTexture = new RenderTexture(width, height, TextureDepth);
            RenderTexture.active = inputRenderTexture;
            ResizeCurrentFrameTexture(width, height);

            if (oldRenderTexture)
                oldRenderTexture.Release();
        }

        /// <summary>
        /// Resizes the Texture2D information used in the ManoMotionFrame.
        /// Though this method should not happen very often, the garbage collector and resources unload are called to prevent a memory leak.
        /// </summary>
        private void ResizeCurrentFrameTexture(int width, int height)
        {
            Texture2D image = new Texture2D(width, height, ImageTextureFormat, false);
            image.filterMode = FilterMode.Trilinear;
            image.Apply();

            currentFrame.texture = image;
            OnFrameResized?.Invoke(currentFrame);
            OnFrameInitializedPointer?.Invoke(image, splittingFactor);

            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
    }
}