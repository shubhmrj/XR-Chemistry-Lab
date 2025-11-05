using UnityEngine;
using ManoMotion;

[RequireComponent(typeof(Transform))]
public class BeakerTiltControllerAdvance : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject beakerModel;

    [Header("Tilt Settings")]
    [SerializeField] private float tiltSpeed = 30f;

    [Header("Rotation Reset Settings")]
    [SerializeField] private float resetRotationSpeed = 2f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float handDepth = 10f;

    [Header("Liquid Pour")]
    [SerializeField] private ParticleSystem liquidPourPS;
    [SerializeField, Tooltip("Degrees from upright to start pouring")]
    private float pourAngleThreshold = 45f;
    [SerializeField, Tooltip("Emission rate per degree over threshold")]
    private float emissionPerDegree = 10f;

    private ManoMotionManager manoManager;
    private ParticleSystem.EmissionModule pourEmission;

    private void Awake()
    {
        manoManager = ManoMotionManager.Instance;
        if (manoManager == null)
        {
            Debug.LogError("[BeakerTiltController] ManoMotionManager not found.");
            enabled = false;
            return;
        }

        if (liquidPourPS != null)
            pourEmission = liquidPourPS.emission;
    }

    private void OnEnable() => manoManager.ShouldCalculateGestures(true);

    private void Update()
    {
        // … existing hand‐gesture handling …

        UpdateLiquidPour();
    }

    private void UpdateLiquidPour()
    {
        if (liquidPourPS == null) return;

        // Angle between current rotation and upright
        float tiltAngle = Quaternion.Angle(beakerModel.transform.rotation, Quaternion.identity);

        if (tiltAngle > pourAngleThreshold)
            pourEmission.rateOverTime = (tiltAngle - pourAngleThreshold) * emissionPerDegree;
        else
            pourEmission.rateOverTime = 0f;
    }

    // … rest of your helper methods (GetNormalizedHandCenter, ApplyTilt, etc.) …
}
