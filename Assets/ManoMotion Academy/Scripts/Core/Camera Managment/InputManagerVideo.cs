using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ManoMotion.CameraSystem
{
    public class InputManagerVideo : InputManagerBase
    {
        [SerializeField] VideoPlayer videoPlayer;
        [SerializeField] DeviceOrientation orientation;

        [Space]
        [SerializeField] TMP_Text frameNumberText, timeText;
        [SerializeField] Slider timelineSlider, volumeSlider;
        [SerializeField] Button playButton, pauseButton;
        [SerializeField] Toggle loopToggle;
        [SerializeField] Image volumeImage;
        [SerializeField] Sprite soundSprite, muteSprite;

        [Space]
        [SerializeField] bool runProcessingInUpdate = true;
        [SerializeField] bool customCopyResolution;
        [SerializeField] Vector2Int customResolution;

        bool initialized = false;
        RenderTexture renderTexture;
        Rect rect;

        private void Start()
        {
            videoPlayer.Prepare();

            currentFrame = new ManoMotionFrame();
            currentFrame.texture = new Texture2D(0, 0);
            currentFrame.orientation = orientation;
            OnFrameInitializedPointer?.Invoke(currentFrame.texture, splittingFactor);

            SetLooping(videoPlayer.isLooping);
            SetPlaying(videoPlayer.playOnAwake);
            SetVolume(videoPlayer.GetDirectAudioVolume(0));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            videoPlayer.loopPointReached += VideoPlayer_loopPointReached;
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            videoPlayer.loopPointReached -= VideoPlayer_loopPointReached;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }

        private void VideoPlayer_loopPointReached(VideoPlayer source)
        {
            if (!videoPlayer.isLooping)
            {
                SetPlaying(false);
            }
        }

        private void OnVideoPrepared(VideoPlayer videoPlayer)
        {
            timelineSlider.maxValue = (float)videoPlayer.clip.length;

            Texture texture = videoPlayer.texture;

            if (customCopyResolution)
            {
                currentFrame.texture.Reinitialize(customResolution.x, customResolution.y);
                renderTexture = new RenderTexture(customResolution.x, customResolution.y, 16);
                rect = new Rect(0, 0, customResolution.x, customResolution.y);
            }     
            else
            {
                currentFrame.texture.Reinitialize(texture.width, texture.height);
                renderTexture = new RenderTexture(texture.width, texture.height, 16);
                rect = new Rect(0, 0, texture.width, texture.height);
            }       

            initialized = true;
            OnFrameInitialized?.Invoke(currentFrame);
            OnFrameInitializedPointer?.Invoke(currentFrame.texture, splittingFactor);
        }

        private void Update()
        {
            if (!initialized)
                return;

            // Pause
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TogglePlaying();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                videoPlayer.frame--;
                SetPlaying(false);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                videoPlayer.StepForward();
                SetPlaying(false);
            }

            UpdateUIElements();

            if (runProcessingInUpdate)
                ProcessFrame();
        }

        private void CopyTexture(Texture frame, Texture2D texture)
        {
            RenderTexture original = RenderTexture.active;

            RenderTexture.active = renderTexture;
            Graphics.Blit(frame, renderTexture);
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();

            RenderTexture.active = original;
            renderTexture.Release();
        }

        private void UpdateUIElements()
        {
            frameNumberText.text = $"Frame: {videoPlayer.frame}";
            double currentTime = videoPlayer.time * 100;
            double length = videoPlayer.clip.length * 100;
            timeText.text = $"{currentTime.ToString("00:00:00")} / {length.ToString("00:00:00")}";
            timelineSlider.SetValueWithoutNotify((float)videoPlayer.time);
        }

        public void SetLooping(bool loop)
        {
            videoPlayer.isLooping = loop;
            loopToggle.isOn = loop;
        }

        public void SetPlaying(bool play)
        {
            if (play)
                videoPlayer.Play();
            else
                videoPlayer.Pause();
            playButton.gameObject.SetActive(!play);
            pauseButton.gameObject.SetActive(play);
        }

        public void SetVolume(float volume)
        {
            videoPlayer.SetDirectAudioVolume(0, volume);
            volumeImage.sprite = volume > 0 ? soundSprite : muteSprite;
        }

        public void TogglePlaying()
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();
            else
                videoPlayer.Play();
            playButton.gameObject.SetActive(videoPlayer.isPaused);
            pauseButton.gameObject.SetActive(videoPlayer.isPlaying);
        }

        public void SetFrame(int frame)
        {
            videoPlayer.frame = frame;    
            SetPlaying(false);
            UpdateUIElements();
        }

        public void SetTime(float time)
        {
            videoPlayer.time = time;
            UpdateUIElements();
        }
 
        /// <summary>
        /// Iterations for synced processing.
        /// </summary>
        public void ProcessFrame(int iterations = 1)
        {
            CopyTexture(videoPlayer.texture, currentFrame.texture);
            for (int i = 0; i < iterations; i++)
                OnFrameUpdated?.Invoke(currentFrame);
            OnFrameInitializedPointer?.Invoke(currentFrame.texture, splittingFactor);
        }
    }
}