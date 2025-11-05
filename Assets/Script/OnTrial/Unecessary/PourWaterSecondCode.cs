using UnityEngine;
using ManoMotion;

public class PourWaterSecondCode : MonoBehaviour
{
    [SerializeField] private GameObject beakerModel;
    [SerializeField] private GameObject beakerModel1;

    [SerializeField] private Transform pourPoint;
    [SerializeField] private GameObject waterParticlesPrefab; // Reference to your water particles prefab
    
    [Header("Pouring Settings")]
    [SerializeField] private float pouringThresholdAngle = 30f;
    [SerializeField] private float maxPourRate = 1.0f;
    
    private float tiltSpeed = 30f;
    private float rotationSpeed = 50f;
    private float moveSpeed = 5f;
    
    // Keep track of liquid amount
    [SerializeField] [Range(0f, 1f)] private float liquidAmount = 1.0f;
    
    // Particle system references
    private ParticleSystem waterEffect;
    private ParticleSystem splashEffect;

    void Start()
    {
        if (ManoMotionManager.Instance != null)
        {
            ManoMotionManager.Instance.ShouldCalculateGestures(true);
        }
        
        // Create pour point if not assigned
        if (pourPoint == null)
        {
            GameObject pourPointObj = new GameObject("PourPoint");
            pourPointObj.transform.parent = beakerModel.transform;
            pourPointObj.transform.localPosition = new Vector3(0, 0.5f, 0.25f); // Adjust based on your beaker model
            pourPoint = pourPointObj.transform;
        }
        
        // Instantiate the water particles prefab
        if (waterParticlesPrefab != null)
        {
            GameObject waterEffectObj = Instantiate(waterParticlesPrefab, pourPoint.position, Quaternion.identity);
            waterEffectObj.transform.parent = transform;
            
            // Get references to the particle systems
            waterEffect = waterEffectObj.GetComponent<ParticleSystem>();
            
            // Find splash effect (assuming it's a child named "WaterSplash")
            Transform splashTransform = waterEffectObj.transform.Find("WaterSplash");
            if (splashTransform != null)
            {
                splashEffect = splashTransform.GetComponent<ParticleSystem>();
            }
            
            // Stop both particle systems initially
            if (waterEffect != null)
                waterEffect.Stop();
            if (splashEffect != null)
                splashEffect.Stop();
        }
        else
        {
            Debug.LogError("Water particles prefab not assigned! Please assign it in the inspector.");
        }
    }

    void Update()
    {
        if (ManoMotionManager.Instance == null || ManoMotionManager.Instance.HandInfos == null)
            return;

        HandInfo[] handInfos = ManoMotionManager.Instance.HandInfos;

        foreach (var handInfo in handInfos)
        {
            if (handInfo.gestureInfo.manoClass == ManoClass.NO_HAND)
                continue;
            
            Vector3 handPosition = new Vector3(
               handInfo.trackingInfo.boundingBox.topLeft.x + handInfo.trackingInfo.boundingBox.width / 2,
               handInfo.trackingInfo.boundingBox.topLeft.y - handInfo.trackingInfo.boundingBox.height / 2,
               0
           );

            ManoGestureContinuous gesture = handInfo.gestureInfo.manoGestureContinuous;

            // Approximate palm center using the center of the bounding box
            BoundingBox boundingBox = handInfo.trackingInfo.boundingBox;

            float centerX = boundingBox.topLeft.x + boundingBox.width / 2f;
            float centerY = boundingBox.topLeft.y - boundingBox.height / 2f;

            // Normalize coordinates to range (-0.5, 0.5)
            float normalizedX = centerX - 0.5f;
            float normalizedY = 0.5f - centerY;

            switch (gesture)
            {
                case ManoGestureContinuous.OPEN_HAND_GESTURE:
                    float tiltX = normalizedY * tiltSpeed * Time.deltaTime;
                    float tiltZ = -normalizedX * tiltSpeed * Time.deltaTime;

                    beakerModel.transform.Rotate(tiltX, 0, tiltZ);
                    break;

                case ManoGestureContinuous.POINTER_GESTURE:
                    // beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 2f);
                    // break;
                    Vector3 targetPosition = ManoUtils.Instance.CalculateNewPositionWithDepth(handPosition, 10);
                    beakerModel1.transform.position = Vector3.Lerp(beakerModel1.transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    
                    break;

                case ManoGestureContinuous.CLOSED_HAND_GESTURE:
                    beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 2f);

                    Vector3 targetPosition1 = ManoUtils.Instance.CalculateNewPositionWithDepth(handPosition, 10);
                    beakerModel.transform.position = Vector3.Lerp(beakerModel.transform.position, targetPosition1, moveSpeed * Time.deltaTime);
                    
                    Handheld.Vibrate();
                    break;
            
            }
        }
        
        // Check if beaker is tilted enough for water to pour
        UpdateWaterPouring();
    }
    
    void UpdateWaterPouring()
    {
        if (waterEffect == null || liquidAmount <= 0)
        {
            // No water effect or no liquid left
            if (waterEffect != null && waterEffect.isPlaying)
                waterEffect.Stop();
            return;
        }
        
        // Calculate the forward vector in world space
        Vector3 beakerUp = beakerModel.transform.up;
        
        // Angle between beaker's up vector and world up vector
        float tiltAngle = Vector3.Angle(beakerUp, Vector3.up);
        
        // Direction of tilt (to determine where water should pour from)
        Vector3 tiltDirection = Vector3.ProjectOnPlane(beakerUp, Vector3.up).normalized;
        
        if (tiltAngle > pouringThresholdAngle && liquidAmount > 0)
        {
            // Calculate pour rate based on tilt angle
            float pourRate = Mathf.Clamp01((tiltAngle - pouringThresholdAngle) / (90f - pouringThresholdAngle));
            
            // Reduce liquid amount based on tilt
            liquidAmount -= pourRate * Time.deltaTime * 0.1f;
            liquidAmount = Mathf.Max(0, liquidAmount);
            
            // Position water effect at pour point
            waterEffect.transform.position = pourPoint.position;
            
            // Orient the particle system in the direction of pour
            waterEffect.transform.rotation = Quaternion.LookRotation(tiltDirection, Vector3.up);
            
            // Adjust emission rate based on tilt and liquid amount
            var emission = waterEffect.emission;
            emission.rateOverTimeMultiplier = pourRate * maxPourRate * liquidAmount;
            
            if (!waterEffect.isPlaying)
                waterEffect.Play();
                
            // Check for collisions to trigger splash
            if (splashEffect != null && !splashEffect.isPlaying)
            {
                // Raycast to find where water would hit
                RaycastHit hit;
                if (Physics.Raycast(pourPoint.position, Vector3.down, out hit, 10f))
                {
                    splashEffect.transform.position = hit.point;
                    splashEffect.transform.up = hit.normal;
                    splashEffect.Play();
                }
            }
        }
        else
        {
            // Not tilted enough to pour
            if (waterEffect != null && waterEffect.isPlaying)
                waterEffect.Stop();
        }
    }
    
    // Method to refill the beaker if needed
    public void RefillBeaker()
    {
        liquidAmount = 1.0f;
    }
}