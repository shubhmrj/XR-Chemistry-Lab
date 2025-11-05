using ManoMotion;
using ManoMotion.Demos;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DistanceTracker : MonoBehaviour
{
    [SerializeField] Grabber[] grabbers;
    [SerializeField] List<Transform> trackedTargets = new List<Transform>();
    [SerializeField] float castRadius = 0.03f;
    [SerializeField] float targetRadius = 0.1f;

    [Space, Header("UI")]
    [SerializeField] Slider distanceSlider;
    [SerializeField] Image targetImagePrefab;
    [SerializeField] bool updateSliderHandle;
    [SerializeField] Image sliderHandle;
    [SerializeField] float maxDistance = 1f;
    [SerializeField] float sliderWidth = 160;
    [SerializeField] Transform left, right;

    [SerializeField] UnityEvent OnStart, OnFinish;

    List<Image> targetImages = new List<Image>();
    int startingTargets = 0;

    private void OnEnable()
    {
        Grabbable.OnSpawned += OnDragged;
    }

    private void OnDisable()
    {
        Grabbable.OnSpawned -= OnDragged;
    }

    void OnDragged(Grabbable grabbable)
    {
        trackedTargets.Add(grabbable.transform);
        CreateImageForTarget();
    }

    private void Start()
    {
        startingTargets = trackedTargets.Count;
        for (int i = 0; i < startingTargets; i++)
        {
            CreateImageForTarget();
        }
    }

    private void CreateImageForTarget()
    {
        Image targetImage = Instantiate(targetImagePrefab, distanceSlider.transform);
        targetImages.Add(targetImage);
    }

    void Update()
    {
        HandInfo[] handInfos = ManoMotionManager.Instance.HandInfos;
        distanceSlider.value = 0;

        for (int i = 0; i < handInfos.Length; i++)
        {
            if (handInfos[i].trackingInfo.skeleton.confidence == 1)
            {
                if (updateSliderHandle)
                {
                    VisualizationInfo visualInfo = ManoMotionManager.Instance.VisualizationInfo;
                    Texture2D handTexture = i == 0 ? visualInfo.occlusionRGB : visualInfo.occlusionRGBsecond;
                    Sprite handSprite = Sprite.Create(handTexture, new Rect(0, 0, handTexture.width, handTexture.height), Vector2.one / 0.5f);
                    sliderHandle.sprite = handSprite;
                }

                SetSliderValue(i);
                break;
            }
        }

        UpdateTargetPositions();
    }

    Vector3 GetHandPosition(int index)
    {
        ManoClass gesture = ManoMotionManager.Instance.HandInfos[index].gestureInfo.manoClass;
        return gesture switch
        {
            ManoClass.GRAB_GESTURE => grabbers[index].GrabPosition,
            ManoClass.PINCH_GESTURE => grabbers[index].PinchPosition,
            _ => grabbers[index].GrabPosition
        };
    }

    private void SetSliderValue(int i)
    {
        Vector3 position = GetHandPosition(i);
        float distance = Vector3.Distance(position, Camera.main.transform.position);
        float sliderValue = Mathf.InverseLerp(0, maxDistance, distance);
        distanceSlider.value = sliderValue;
    }

    private void UpdateTargetPositions()
    {
        Camera camera = Camera.main;

        for (int i = 0; i < trackedTargets.Count; i++)
        {
            Renderer renderer = trackedTargets[i].GetComponent<Renderer>();
            float distance = Vector3.Distance(trackedTargets[i].position, camera.transform.position);

            bool isVisible = renderer.isVisible;
            bool isTooFarAway = distance > maxDistance;
            bool isInFrontOfTheCamera = Vector3.Dot(camera.transform.forward, trackedTargets[i].position - camera.transform.position) > 0;

            if (!isVisible || isTooFarAway || !isInFrontOfTheCamera)
            {
                targetImages[i].enabled = false;
                break;
            }

            // Show image for target.
            targetImages[i].enabled = true;
            targetImages[i].transform.position = GetPositionFromDistance(distance);
            targetImages[i].color = trackedTargets[i].GetComponent<Renderer>().material.color;
        }
    }

    private Vector2 GetPositionFromDistance(float distance)
    {
        float t = Mathf.InverseLerp(0, maxDistance, distance);
        return Vector2.Lerp(left.position, right.position, t);
    }

    public void ResetScene()
    {
        for (int i = trackedTargets.Count - 1; i >= startingTargets; i--)
        {
            Destroy(trackedTargets[i].gameObject);
            trackedTargets.RemoveAt(i);
            Destroy(targetImages[i].gameObject);
            targetImages.RemoveAt(i);
        }
    }
}
