using UnityEngine;

namespace ManoMotion.CameraSystem
{
    /// <summary>
    /// Takes control of the devices camera and broadcasts camera updates.
    /// </summary>
    public class InputManagerAdjustable : InputManagerBase
    {
        [Tooltip("When true the background will be zoomed in to cover the entire screen, when false the entire image will be visible but there will be black borders.")]
        [SerializeField] bool shouldBackgroundCoverScreen;
        [SerializeField] AddOn addOn;

        private WebCamTexture currentPlayingCamera;
        int textureWidth, textureHeight;
        bool isFrameUpdated;

        private void Awake()
        {
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;
            float ratio = (float)LOWEST_RESOLUTION_VALUE / Mathf.Min(screenWidth, screenHeight);
            textureWidth = (int)(screenWidth * ratio);
            textureHeight = (int)(screenHeight * ratio);

            ForceApplicationPermissions();
        }

        private void Start()
        {
            OnAddonSet?.Invoke(addOn);
            InitializeManoMotionFrame();
            HandleNewCameraDeviceSelected();
            ResizeManoMotionFrameResolution(currentPlayingCamera.width, currentPlayingCamera.height);
        }

        void Update()
        {
            ManoUtils.Instance.ShouldBackgroundCoverScreen(shouldBackgroundCoverScreen);
            GetCameraFrameInformation();

            if (currentPlayingCamera)
            {
                isFrameUpdated = currentPlayingCamera.didUpdateThisFrame;

                // Pointer changes in iOS so need to update it every frame.
#if UNITY_IOS
                OnFrameInitializedPointer?.Invoke(currentFrame.texture, splittingFactor);
#endif
            }
        }

        private void HandleNewCameraDeviceSelected()
        {
            // Stop current camera
            if (currentPlayingCamera)
            {
                currentPlayingCamera.Stop();
                currentPlayingCamera = null;
            }

            // Assign new camera
            for (int i = 0; i < WebCamTexture.devices.Length; i++)
            {
                WebCamDevice device = WebCamTexture.devices[i];
                if (device.isFrontFacing == isFrontFacing)
                {
                    currentPlayingCamera = new WebCamTexture(device.name, textureWidth, textureHeight);
                    break;
                }
            }

            // Get default camera if none was found.
            if (!currentPlayingCamera && WebCamTexture.devices.Length > 0)
                currentPlayingCamera = new WebCamTexture(WebCamTexture.devices[0].name, textureWidth, textureHeight);

            // Start new camera
            if (currentPlayingCamera)
                currentPlayingCamera.Play();

            OnChangeCamera?.Invoke();
        }

        /// <summary>
        /// Initializes the ManoMotion Frame and lets the subscribers of the event know of its information.
        /// </summary>
        private void InitializeManoMotionFrame()
        {
            currentFrame = new ManoMotionFrame();
            currentFrame.orientation = Input.deviceOrientation;
        }

        /// <summary>
        /// Gets the camera frame pixel colors.
        /// </summary>
        protected void GetCameraFrameInformation()
        {
            if (!currentPlayingCamera)
            {
                Debug.LogWarning("No camera device available");
                HandleNewCameraDeviceSelected();
                return;
            }

            Color32[] pixels = currentPlayingCamera.GetPixels32();

            if (currentFrame.texture.GetPixels32().Length != pixels.Length)
            {
                ResizeManoMotionFrameResolution(currentPlayingCamera.width, currentPlayingCamera.height);
                return;
            }

            currentFrame.texture.SetPixels32(pixels);

            //Flip the texture if using front facing to match the image to the device.
            if (isFrontFacing)
            {
#if UNITY_ANDROID || UNITY_STANDALONE
                FlipTextureHorizontal(ref currentFrame.texture);
#elif UNITY_IOS
                FlipTextureVertical(ref currentFrame.texture);
#endif
            }

            currentFrame.orientation = Input.deviceOrientation;
            OnFrameUpdated?.Invoke(currentFrame);
        }

        /// <summary>
        /// Sets the resolution of the currentManoMotion frame that is passed to the subscribers that want to make use of the input camera feed.
        /// </summary>
        /// <param name="width">Requires a width value.</param>
        /// <param name="height">Requires a height value.</param>
        protected void ResizeManoMotionFrameResolution(int width, int height)
        {
            currentFrame.texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            currentFrame.texture.Apply();

            Texture2D image = currentFrame.texture;
            image.filterMode = FilterMode.Trilinear;
            image.Apply();

            OnFrameResized?.Invoke(currentFrame);
            OnFrameInitializedPointer?.Invoke(currentFrame.texture);
        }

        protected override void UpdateFrontFacing(bool isFrontFacing)
        {
#if UNITY_ANDROID || UNITY_IOS
            HandleNewCameraDeviceSelected();
            ResizeManoMotionFrameResolution(currentPlayingCamera.width, currentPlayingCamera.height);
#endif
        }

        public override bool IsFrameUpdated() => isFrameUpdated;

        public void ToggleBackgroundCover()
        {
            shouldBackgroundCoverScreen = !shouldBackgroundCoverScreen;
        }

        /// <summary>
        /// Start the camera when enabled.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            if (currentPlayingCamera)
                currentPlayingCamera.Play();
        }

        /// <summary>
        /// Stops the camera when disabled.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            if (currentPlayingCamera)
                currentPlayingCamera.Stop();  
        }
    }
}