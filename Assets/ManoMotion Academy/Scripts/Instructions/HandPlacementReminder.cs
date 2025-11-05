using ManoMotion;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// "Breathing" image of hand to remind user to place hand in front of the camera at a distance.
/// </summary>
public class HandPlacementReminder : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] float minAlpha = 0, maxAlpha = 1;
    [SerializeField] float breathTime = 2;
    [SerializeField] float timeBeforeBreathingStarts = 1f;

    bool isHandVisible = true, wasHandVisible = true;
    float missingHandTime = 0;
    float currentAlpha = 1f;
    float currentBreathTime = 0;

    private void Awake()
    {
        missingHandTime = timeBeforeBreathingStarts;
    }

    void Update()
    {
        wasHandVisible = isHandVisible;
        isHandVisible = ManoMotionManager.Instance.AnyHandVisible();

        if (!isHandVisible)
        {
            if (wasHandVisible && !isHandVisible)
                currentBreathTime = minAlpha;

            missingHandTime += Time.deltaTime;
            if (missingHandTime < timeBeforeBreathingStarts)
                return;
            
            currentBreathTime += Time.deltaTime / breathTime;
            currentAlpha = Mathf.PingPong(currentBreathTime, maxAlpha - minAlpha) + minAlpha;
            SetAlpha(currentAlpha);
        }
        else
        {
            missingHandTime = 0;
            SetAlpha(0);
        }
    }

    void SetAlpha(float a)
    {
        Color color = image.color;
        color.a = a;
        image.color = color;
    }
}
