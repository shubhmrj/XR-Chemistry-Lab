using UnityEngine;
using ManoMotion;

public class WaterAttachToBeaker : MonoBehaviour
{
    [SerializeField] private GameObject beakerModel;
    [SerializeField] private Transform pourPoint;
    [SerializeField] private GameObject waterParticlesPrefab;
    
    [Header("Pouring Settings")]
    [SerializeField] private float pouringThresholdAngle = 25f; // Reduced from 30f for easier pouring
    [SerializeField] private float maxPourRate = 100.0f; // Increased for more visible water flow
    [SerializeField] public Color waterColor = new Color(0.7f, 0.85f, 0.92f, 0.7f);
    
    private float tiltSpeed = 30f;
    private float rotationSpeed = 50f;
    [SerializeField] private float maxTiltAngle = 60f; // maximum tilt angle in both directions
    [SerializeField] private float tiltSmoothSpeed = 15f; // smoothing factor for roll (increased for faster response)
    [SerializeField] private float moveSpeed = 8f; // Increased for better grab responsiveness
    [SerializeField] private float grabSmoothness = 0.3f; // Higher = smoother but slower grab movement

    [Header("Orientation Settings")]
    [SerializeField] private bool isLandscapeMode = true;
    [SerializeField] private float coordinateScale = 3f; // Scale factor for hand coordinates (adjusted for better control)
    [SerializeField] private Vector3 handPositionOffset = new Vector3(0, 0f, 10f); // Offset for hand position
    [SerializeField] private bool useDirectPositioning = false; // Use camera-relative positioning instead
    [SerializeField] private float grabMovementMultiplier = 1.5f; // Multiplier for grab movement distance
    
    // Keep track of liquid amount
    [SerializeField] [Range(0f, 1f)] public float liquidAmount = 1.0f;
    
    // Water effect references
    private GameObject waterEffectObj;
    private ParticleSystem waterEffect;
    private ParticleSystem splashEffect;

    // Track last emitter position to clear stray particles when beaker moves
    private Vector3 lastEmitPosition;
    
    // Store original beaker scale to prevent scaling issues
    private Vector3 FIXED_BEAKER_SCALE = new Vector3(5f, 5f, 5f); // FIXED scale for proper size
    private Vector3 originalBeakerScale;
    private Vector3 initialBeakerPosition; // Store initial position for reset
    private Quaternion initialBeakerRotation; // Store initial rotation
    
    // Debug visualization
    [SerializeField] private bool showDebugVisuals = true;
    private GameObject debugSphere;
    
    // Current gesture tracking
    private ManoGestureContinuous currentGesture = ManoGestureContinuous.NO_GESTURE;
    private string beakerStatus = "Ready";
    
    // Safety bounds
    [Header("Safety Settings")]
    [SerializeField] private bool enableSafetyBounds = true;
    [SerializeField] private Vector3 minBounds = new Vector3(-5f, -3f, 5f);
    [SerializeField] private Vector3 maxBounds = new Vector3(5f, 5f, 15f);

    void Start()
    {
        if (ManoMotionManager.Instance != null)
        {
            ManoMotionManager.Instance.ShouldCalculateGestures(true);
        }
        
        // CRITICAL: Set beaker to FIXED scale (5, 5, 5) for proper visibility
        beakerModel.transform.localScale = FIXED_BEAKER_SCALE;
        originalBeakerScale = FIXED_BEAKER_SCALE;
        initialBeakerPosition = beakerModel.transform.position;
        initialBeakerRotation = beakerModel.transform.rotation;
        
        Debug.Log($"WaterAttachToBeaker initialized. Beaker set to FIXED scale {FIXED_BEAKER_SCALE}. Position: {initialBeakerPosition}");
        
        // Create pour point if not assigned
        if (pourPoint == null)
        {
            GameObject pourPointObj = new GameObject("PourPoint");
            pourPointObj.transform.parent = beakerModel.transform;
            
            // Adjust based on your beaker model dimensions - position at the lip/edge
            // These values need to be adjusted for your specific beaker model
            pourPointObj.transform.localPosition = new Vector3(0, 0.45f, 0.25f); 
            pourPoint = pourPointObj.transform;
        }
        
        // Create debug visuals if needed
        if (showDebugVisuals)
        {
            debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugSphere.transform.localScale = Vector3.one * 0.05f;
            debugSphere.GetComponent<Renderer>().material.color = Color.red;
            Destroy(debugSphere.GetComponent<Collider>());
        }
        
        // Instantiate the water particles prefab
        if (waterParticlesPrefab != null)
        {
            // Instantiate the water particle effect at the pour-point position but DO NOT
            // parent it. We will manually reposition it each frame. This prevents
            // previously emitted particles from being dragged around when the beaker
            // moves/rotates.
            waterEffectObj = Instantiate(waterParticlesPrefab, pourPoint.position, Quaternion.identity);
            waterEffectObj.transform.parent = null;
            
            // Get references to particle systems
            waterEffect = waterEffectObj.GetComponent<ParticleSystem>();
            if (waterEffect == null)
            {
                Debug.LogError("Particle system component not found on water particles prefab!");
            }
            else
            {
                // Use LOCAL simulation for better control and immediate alignment
                var main = waterEffect.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                
                // Set the water color
                main.startColor = waterColor;
            }
            
            // Find splash particles if they exist
            Transform splashTransform = waterEffectObj.transform.Find("WaterSplash");
            if (splashTransform != null)
            {
                splashEffect = splashTransform.GetComponent<ParticleSystem>();
                if (splashEffect != null)
                {
                    var splashMain = splashEffect.main;
                    splashMain.simulationSpace = ParticleSystemSimulationSpace.Local;
                    splashMain.startColor = waterColor;
                }
            }
            
            // Stop the particle systems initially
            if (waterEffect != null) {
                waterEffect.Stop();
                lastEmitPosition = pourPoint.position; // initialise
            }
            if (splashEffect != null) splashEffect.Stop();
        }
        else
        {
            Debug.LogError("Water particles prefab not assigned!");
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
            
            // Calculate hand position with orientation correction
            Vector3 handPosition = CalculateHandPosition(handInfo.trackingInfo.boundingBox);

            ManoGestureContinuous gesture = handInfo.gestureInfo.manoGestureContinuous;
            currentGesture = gesture; // Track current gesture for UI display

            // Approximate palm center using the center of the bounding box
            BoundingBox boundingBox = handInfo.trackingInfo.boundingBox;

            float centerX = boundingBox.topLeft.x + boundingBox.width / 2f;
            float centerY = boundingBox.topLeft.y - boundingBox.height / 2f;

            // Normalize coordinates with orientation correction
            float normalizedX, normalizedY;
            if (isLandscapeMode)
            {
                // In landscape left: for tilt, we want horizontal hand movement
                // Hand moving left/right on screen should tilt beaker
                normalizedX = (centerX - 0.5f); // Keep X for horizontal tilt
                normalizedY = (centerY - 0.5f); // Use Y for vertical reference
            }
            else
            {
                // Portrait mode (original)
                normalizedX = centerX - 0.5f;
                normalizedY = 0.5f - centerY;
            }

            switch (gesture)
            {
                case ManoGestureContinuous.OPEN_HAND_GESTURE:
                    // ABSOLUTE SCALE LOCK - BEFORE any operation
                    beakerModel.transform.localScale = FIXED_BEAKER_SCALE;
                    
                    beakerStatus = "Tilting";
                    
                    // Map hand horizontal position to tilt angle (both left and right)
                    // normalizedX ranges from -0.5 (left) to +0.5 (right)
                    // We multiply by 2 to get full range: -1 to +1
                    float tiltInput = normalizedX * 2f; // Range: -1 to +1
                    
                    // Scale to maxTiltAngle and allow both directions
                    float desiredTiltZ = Mathf.Clamp(tiltInput * maxTiltAngle, -maxTiltAngle, maxTiltAngle);

                    // Get current roll angle (normalize to -180 to 180 range)
                    float currentTiltZ = beakerModel.transform.eulerAngles.z;
                    if (currentTiltZ > 180f) currentTiltZ -= 360f;

                    float angleDiff = desiredTiltZ - currentTiltZ;
                    
                    // Apply rotation if difference is significant
                    if (Mathf.Abs(angleDiff) > 0.5f)
                    {
                        // Smooth rotation with proportional speed
                        float rotateAmount = angleDiff * Time.deltaTime * tiltSmoothSpeed;
                        beakerModel.transform.RotateAround(pourPoint.position, Vector3.forward, rotateAmount);
                    }
                    
                    // ABSOLUTE SCALE LOCK - AFTER rotation
                    beakerModel.transform.localScale = FIXED_BEAKER_SCALE;
                    
                    // Debug logging
                    if (showDebugVisuals)
                    {
                        Debug.Log($"OPEN_HAND: normalizedX={normalizedX:F3}, tiltInput={tiltInput:F3}, desiredTilt={desiredTiltZ:F1}°, currentTilt={currentTiltZ:F1}°, diff={angleDiff:F1}°");
                    }
                    break;

                case ManoGestureContinuous.OPEN_PINCH_GESTURE:
                    // ABSOLUTE SCALE LOCK - BEFORE any operation
                    beakerModel.transform.localScale = FIXED_BEAKER_SCALE;
                    
                    beakerStatus = "Refilling";
                    
                    // Reset rotation to upright
                    beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 3f);
                    
                    // Refill the beaker
                    if (liquidAmount < 1.0f)
                    {
                        liquidAmount = Mathf.Min(1.0f, liquidAmount + Time.deltaTime * 2f); // Refill over 0.5 seconds
                    }
                    
                    // ABSOLUTE SCALE LOCK - AFTER refill
                    beakerModel.transform.localScale = FIXED_BEAKER_SCALE;
                    
                    if (showDebugVisuals)
                    {
                        Debug.Log($"PINCH: Refilling beaker - Liquid: {liquidAmount:P0}");
                    }
                    break;

                case ManoGestureContinuous.CLOSED_HAND_GESTURE:
                    // ABSOLUTE SCALE LOCK - BEFORE any operation
                    beakerModel.transform.localScale = FIXED_BEAKER_SCALE;
                    
                    beakerStatus = "Grabbed - Moving";

                    // Keep beaker upright while grabbing
                    beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 3f);

                    // Calculate target position based on hand movement
                    Vector3 targetPosition = CalculateBeakerPosition(handPosition);
                    
                    // Clamp position to keep beaker visible (if safety bounds enabled)
                    if (enableSafetyBounds)
                    {
                        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                        targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
                        targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.z, maxBounds.z);
                    }
                    
                    // Smooth movement with configurable speed
                    beakerModel.transform.position = Vector3.Lerp(
                        beakerModel.transform.position, 
                        targetPosition, 
                        grabSmoothness
                    );
                    
                    // ABSOLUTE SCALE LOCK - AFTER movement
                    beakerModel.transform.localScale = FIXED_BEAKER_SCALE;
                    
                    // TRIPLE LOCK - Force again
                    beakerModel.transform.localScale = FIXED_BEAKER_SCALE;
                    
                    if (showDebugVisuals)
                    {
                        Debug.Log($"GRAB: Hand={handPosition}, Target={targetPosition}, Current={beakerModel.transform.position}, Scale={beakerModel.transform.localScale}");
                    }
                    break;
            }
        }
        
        // CRITICAL: ABSOLUTE SCALE ENFORCEMENT every frame
        // This prevents ANY scaling issues from Unity's transform system
        beakerModel.transform.localScale = FIXED_BEAKER_SCALE;
        
        // Update status when no gesture detected
        if (currentGesture == ManoGestureContinuous.NO_GESTURE)
        {
            beakerStatus = "Ready";
        }

        // Update debug visuals
        if (showDebugVisuals && debugSphere != null && pourPoint != null)
        {
            debugSphere.transform.position = pourPoint.position;
        }

        // Check if beaker is tilted enough for water to pour
        UpdateWaterPouring();
    }
    
    // LateUpdate runs AFTER all Update functions - ABSOLUTE FINAL scale enforcement
    void LateUpdate()
    {
        // ABSOLUTE FINAL scale enforcement after ALL Unity operations
        if (beakerModel != null)
        {
            beakerModel.transform.localScale = FIXED_BEAKER_SCALE;
        }
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

        // Calculate the up vector of the beaker in world space
        Vector3 beakerUp = beakerModel.transform.up;

        // Angle between beaker's up vector and world up vector
        float tiltAngle = Vector3.Angle(beakerUp, Vector3.up);
        
        // Debug: Always log tilt angle to see if beaker is tilting
        if (showDebugVisuals)
        {
            Debug.Log($"Beaker Tilt Angle: {tiltAngle:F1}° (Threshold: {pouringThresholdAngle}°) - Liquid: {liquidAmount:F2}");
        }

        // Direction of tilt (to determine where water should pour from)
        Vector3 tiltDirection = Vector3.zero;
        if (tiltAngle > 1f) // Avoid normalizing zero vector
        {
            tiltDirection = Vector3.ProjectOnPlane(beakerUp, Vector3.up).normalized;
        }
        else
        {
            tiltDirection = Vector3.forward; // Default direction if not tilted
        }

        if (tiltAngle > pouringThresholdAngle && liquidAmount > 0)
        {
            // Parent water effect to pour point for immediate alignment
            if (waterEffectObj.transform.parent != pourPoint)
            {
                waterEffectObj.transform.SetParent(pourPoint);
                waterEffectObj.transform.localPosition = Vector3.zero;
                waterEffectObj.transform.localRotation = Quaternion.identity;
            }

            // Clear existing particles only if we just started pouring
            if (!waterEffect.isPlaying)
            {
                waterEffect.Clear(true);
            }

            lastEmitPosition = pourPoint.position;

            // Calculate pour rate based on tilt angle
            float pourRate = Mathf.Clamp01((tiltAngle - pouringThresholdAngle) / (90f - pouringThresholdAngle));

            // Reduce liquid amount based on tilt
            liquidAmount -= pourRate * Time.deltaTime * 0.1f;
            liquidAmount = Mathf.Max(0, liquidAmount);

            // Orient particles downward with slight tilt direction influence
            Vector3 pourDirection = Vector3.down + tiltDirection * 0.3f;
            waterEffectObj.transform.rotation = Quaternion.LookRotation(pourDirection.normalized, Vector3.up);

            // Log positions for debugging
            if (showDebugVisuals)
            {
                Debug.Log($"POURING! Beaker: {beakerModel.transform.position}, Pour Point: {pourPoint.position}, Water: {waterEffectObj.transform.position}");
            }

            // Adjust emission rate based on tilt and liquid amount
            var emission = waterEffect.emission;
            emission.rateOverTimeMultiplier = pourRate * maxPourRate * liquidAmount;

            if (!waterEffect.isPlaying)
            {
                // Clear to remove any stray particles first so emission starts exactly from current position.
                waterEffect.Clear(true);
                waterEffect.Play();
            }

            // Handle splash effects
            if (splashEffect != null)
            {
                // Raycast to find where water would hit
                RaycastHit hit;
                if (Physics.Raycast(pourPoint.position, Vector3.down, out hit, 10f))
                {
                    splashEffect.transform.position = hit.point;
                    splashEffect.transform.up = hit.normal;

                    if (!splashEffect.isPlaying)
                        {
                            splashEffect.Clear(true);
                            splashEffect.Play();
                        }
                }
                else if (splashEffect.isPlaying)
                {
                    splashEffect.Stop();
                }
            }
        }
        else
        {
            // Not tilted enough to pour - detach water effect from pour point
            if (waterEffectObj.transform.parent == pourPoint)
            {
                waterEffectObj.transform.SetParent(null);
            }

            // Stop water effects
            if (waterEffect.isPlaying)
                waterEffect.Stop();

            if (splashEffect != null && splashEffect.isPlaying)
                splashEffect.Stop();
        }
    }

    // Calculate hand position with orientation correction
    Vector3 CalculateHandPosition(BoundingBox boundingBox)
    {
        float centerX = boundingBox.topLeft.x + boundingBox.width / 2f;
        float centerY = boundingBox.topLeft.y - boundingBox.height / 2f;

        Vector3 handPos;

        if (isLandscapeMode)
        {
            // Landscape left orientation correction for GRAB movement
            // Swap X/Y for proper world space mapping during grab
            float normalizedX = (centerY - 0.5f) * coordinateScale * grabMovementMultiplier;
            float normalizedY = (0.5f - centerX) * coordinateScale * grabMovementMultiplier;

            handPos = new Vector3(normalizedX, normalizedY, 0) + handPositionOffset;
        }
        else
        {
            // Portrait mode (original)
            handPos = new Vector3(centerX, centerY, 0);
        }

        return handPos;
    }

    // Calculate beaker position for grab gesture
    Vector3 CalculateBeakerPosition(Vector3 handPosition)
    {
        if (useDirectPositioning)
        {
            // Direct positioning mode - beaker follows hand exactly
            if (isLandscapeMode)
            {
                // Direct mapping with proper scaling for landscape mode
                Vector3 beakerPos = new Vector3(
                    handPosition.x,
                    handPosition.y,
                    handPositionOffset.z
                );
                beakerPos += new Vector3(handPositionOffset.x, handPositionOffset.y, 0);
                return beakerPos;
            }
            else
            {
                return ManoUtils.Instance.CalculateNewPositionWithDepth(handPosition, 10);
            }
        }
        else
        {
            // Camera-relative positioning (RECOMMENDED)
            // This keeps beaker visible and moves it relative to camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("Main camera not found!");
                return beakerModel.transform.position; // Return current position if no camera
            }
            
            // Get hand position in normalized screen space
            BoundingBox bbox = ManoMotionManager.Instance.HandInfos[0].trackingInfo.boundingBox;
            float screenX = bbox.topLeft.x + bbox.width / 2f;
            float screenY = bbox.topLeft.y - bbox.height / 2f;
            
            // Convert to viewport coordinates (0-1 range)
            Vector3 viewportPos = new Vector3(screenX, screenY, handPositionOffset.z);
            
            // Convert viewport to world position
            Vector3 worldPos = mainCam.ViewportToWorldPoint(viewportPos);
            
            return worldPos;
        }
    }

    // Method to refill the beaker if needed
    public void RefillBeaker()
    {
        liquidAmount = 1.0f;
    }
    
    // Method to reset beaker to initial position
    public void ResetBeakerPosition()
    {
        beakerModel.transform.position = initialBeakerPosition;
        beakerModel.transform.rotation = initialBeakerRotation;
        beakerModel.transform.localScale = FIXED_BEAKER_SCALE;
        originalBeakerScale = FIXED_BEAKER_SCALE;
        liquidAmount = 1.0f;
        beakerStatus = "Reset Complete";
        Debug.Log($"Beaker reset to initial state - Position: {initialBeakerPosition}, Scale: {FIXED_BEAKER_SCALE}");
    }

    // Advanced GUI styling
    private GUIStyle GetHeaderStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 22;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = new Color(0.2f, 0.8f, 1f); // Cyan
        style.alignment = TextAnchor.MiddleLeft;
        return style;
    }
    
    private GUIStyle GetLabelStyle(int fontSize = 16, Color? color = null)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = color ?? Color.white;
        style.alignment = TextAnchor.MiddleLeft;
        return style;
    }
    
    private GUIStyle GetButtonStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 16;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        style.normal.background = MakeTex(2, 2, new Color(0.8f, 0.2f, 0.2f, 0.8f));
        style.hover.background = MakeTex(2, 2, new Color(1f, 0.3f, 0.3f, 0.9f));
        style.active.background = MakeTex(2, 2, new Color(0.6f, 0.1f, 0.1f, 0.9f));
        return style;
    }
    
    private GUIStyle GetModernButtonStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 15;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        style.normal.background = MakeTex(2, 2, new Color(0.2f, 0.6f, 0.9f, 0.9f)); // Cyan blue
        style.hover.background = MakeTex(2, 2, new Color(0.3f, 0.7f, 1f, 1f)); // Bright cyan
        style.active.background = MakeTex(2, 2, new Color(0.1f, 0.5f, 0.8f, 1f)); // Dark cyan
        style.alignment = TextAnchor.MiddleCenter;
        return style;
    }
    
    private GUIStyle GetGlassButtonStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 16;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = new Color(0.9f, 1f, 1f);
        style.normal.background = MakeTex(2, 2, new Color(0.15f, 0.45f, 0.7f, 0.85f)); // Glass cyan
        style.hover.background = MakeTex(2, 2, new Color(0.2f, 0.6f, 0.95f, 0.95f)); // Bright glass
        style.active.background = MakeTex(2, 2, new Color(0.1f, 0.35f, 0.6f, 0.95f)); // Dark glass
        style.alignment = TextAnchor.MiddleCenter;
        return style;
    }
    
    private GUIStyle GetBorderStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0.3f, 0.85f, 1f, 0.6f)); // Bright cyan glow
        return style;
    }
    
    private GUIStyle GetOuterGlowStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0.1f, 0.5f, 0.8f, 0.25f)); // Soft outer glow
        return style;
    }
    
    private GUIStyle MakeGlowLineStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0.4f, 0.9f, 1f, 0.8f)); // Bright glow line
        return style;
    }
    
    private GUIStyle GetStatusBoxStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 28;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;
        style.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.7f));
        return style;
    }
    
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    // Debug method to test orientation
    void OnGUI()
    {
        if (showDebugVisuals)
        {
            // ═══════════════════════════════════════════════════════════
            // TOP CENTER - GESTURE STATUS DISPLAY (ADVANCED)
            // ═══════════════════════════════════════════════════════════
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;
            
            int statusWidth = 600;
            int statusHeight = 80;
            int statusX = (screenWidth - statusWidth) / 2;
            int statusY = 20;
            
            // Draw status background with gradient effect
            GUI.Box(new Rect(statusX, statusY, statusWidth, statusHeight), "", GetStatusBoxStyle());
            
            // Determine gesture icon and color
            string gestureIcon = "⚪";
            string gestureText = "NO GESTURE";
            Color gestureColor = Color.gray;
            
            switch (currentGesture)
            {
                case ManoGestureContinuous.OPEN_HAND_GESTURE:
                    gestureIcon = "✋";
                    gestureText = "OPEN HAND - TILTING";
                    gestureColor = new Color(0.2f, 0.8f, 1f); // Cyan
                    break;
                case ManoGestureContinuous.CLOSED_HAND_GESTURE:
                    gestureIcon = "✊";
                    gestureText = "CLOSED HAND - GRABBING";
                    gestureColor = new Color(1f, 0.3f, 0.8f); // Magenta
                    break;
                case ManoGestureContinuous.OPEN_PINCH_GESTURE:
                    gestureIcon = "👌";
                    gestureText = "PINCH - REFILLING";
                    gestureColor = new Color(1f, 0.8f, 0.2f); // Yellow
                    break;
                default:
                    gestureIcon = "👋";
                    gestureText = "READY - SHOW HAND";
                    gestureColor = new Color(0.5f, 0.5f, 0.5f);
                    break;
            }
            
            // Draw gesture icon
            GUIStyle iconStyle = new GUIStyle(GUI.skin.label);
            iconStyle.fontSize = 48;
            iconStyle.alignment = TextAnchor.MiddleCenter;
            iconStyle.normal.textColor = gestureColor;
            GUI.Label(new Rect(statusX + 20, statusY + 10, 80, 60), gestureIcon, iconStyle);
            
            // Draw gesture text
            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = 24;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.alignment = TextAnchor.MiddleLeft;
            textStyle.normal.textColor = gestureColor;
            GUI.Label(new Rect(statusX + 110, statusY + 15, statusWidth - 130, 30), gestureText, textStyle);
            
            // Draw beaker status
            GUIStyle statusTextStyle = new GUIStyle(GUI.skin.label);
            statusTextStyle.fontSize = 18;
            statusTextStyle.fontStyle = FontStyle.Normal;
            statusTextStyle.alignment = TextAnchor.MiddleLeft;
            statusTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(statusX + 110, statusY + 45, statusWidth - 130, 25), $"Status: {beakerStatus}", statusTextStyle);
            
            // ═══════════════════════════════════════════════════════════
            // ADVANCED XR CHEMISTRY LAB - FULL CONTROL INTERFACE
            // ═══════════════════════════════════════════════════════════
            int mainPanelWidth = 480;
            int mainPanelHeight = 420;
            int panelX = 10;
            int panelY = screenHeight - mainPanelHeight - 10; // Bottom-left positioning
            
            // ═══════════════════════════════════════════════════════════
            // MAIN PANEL - Multi-layer glass design
            // ═══════════════════════════════════════════════════════════
            GUI.Box(new Rect(panelX - 6, panelY - 6, mainPanelWidth + 12, mainPanelHeight + 12), "", GetOuterGlowStyle());
            GUI.Box(new Rect(panelX - 3, panelY - 3, mainPanelWidth + 6, mainPanelHeight + 6), "", GetBorderStyle());
            
            GUIStyle mainPanelStyle = new GUIStyle(GUI.skin.box);
            mainPanelStyle.normal.background = MakeTex(2, 2, new Color(0.01f, 0.03f, 0.08f, 0.97f));
            GUI.Box(new Rect(panelX, panelY, mainPanelWidth, mainPanelHeight), "", mainPanelStyle);
            
            // ═══════════════════════════════════════════════════════════
            // HEADER SECTION - Title + Control Buttons
            // ═══════════════════════════════════════════════════════════
            GUIStyle headerBg = new GUIStyle(GUI.skin.box);
            headerBg.normal.background = MakeTex(2, 2, new Color(0.03f, 0.15f, 0.3f, 0.95f));
            GUI.Box(new Rect(panelX, panelY, mainPanelWidth, 55), "", headerBg);
            GUI.Box(new Rect(panelX, panelY + 53, mainPanelWidth, 2), "", MakeGlowLineStyle());
            
            // Title
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 24;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(0.7f, 1f, 1f);
            titleStyle.alignment = TextAnchor.MiddleLeft;
            GUI.Label(new Rect(panelX + 20, panelY + 12, 300, 32), "⚗️ XR CHEMISTRY LAB", titleStyle);
            
            // System Status Indicator
            GUIStyle versionStyle = new GUIStyle(GUI.skin.label);
            versionStyle.fontSize = 11;
            versionStyle.normal.textColor = new Color(0.5f, 0.8f, 0.9f);
            GUI.Label(new Rect(panelX + 20, panelY + 38, 200, 15), "System: ONLINE | v3.0", versionStyle);
            
            // ═══════════════════════════════════════════════════════════
            // CONTROL BUTTONS ROW - Top right
            // ═══════════════════════════════════════════════════════════
            int buttonY = panelY + 10;
            int buttonWidth = 85;
            int buttonHeight = 35;
            int buttonSpacing = 5;
            int buttonStartX = panelX + mainPanelWidth - (buttonWidth * 3 + buttonSpacing * 2 + 15);
            
            // RESET Button
            if (GUI.Button(new Rect(buttonStartX, buttonY, buttonWidth, buttonHeight), "⟲ RESET", GetControlButtonStyle(new Color(0.8f, 0.3f, 0.3f))))
            {
                ResetBeakerPosition();
            }
            
            // REFILL Button
            if (GUI.Button(new Rect(buttonStartX + buttonWidth + buttonSpacing, buttonY, buttonWidth, buttonHeight), "💧 REFILL", GetControlButtonStyle(new Color(0.2f, 0.6f, 0.9f))))
            {
                liquidAmount = 1.0f;
                beakerStatus = "Refilled";
            }
            
            // CLEAR Button
            if (GUI.Button(new Rect(buttonStartX + (buttonWidth + buttonSpacing) * 2, buttonY, buttonWidth, buttonHeight), "🗑 CLEAR", GetControlButtonStyle(new Color(0.6f, 0.6f, 0.6f))))
            {
                liquidAmount = 0f;
                beakerStatus = "Cleared";
            }
            
            int yPos = panelY + 68;
            int lineHeight = 24;
            
            // Get beaker tilt angle
            float currentTiltZ = beakerModel.transform.eulerAngles.z;
            if (currentTiltZ > 180f) currentTiltZ -= 360f;
            Vector3 beakerUp = beakerModel.transform.up;
            float tiltAngle = Vector3.Angle(beakerUp, Vector3.up);
            
            // ═══════════════════════════════════════════════════════════
            // SECTION 1: BEAKER DIAGNOSTICS
            // ═══════════════════════════════════════════════════════════
            DrawXRSectionHeader(panelX + 18, yPos, mainPanelWidth - 36, "🔬 BEAKER DIAGNOSTICS");
            yPos += 30;
            
            // Beaker Tilt with visual indicator
            Color tiltColor = Mathf.Abs(currentTiltZ) > 20f ? new Color(1f, 0.7f, 0.3f) : new Color(0.6f, 0.95f, 1f);
            GUI.Label(new Rect(panelX + 25, yPos, mainPanelWidth - 50, lineHeight), $"↻ Rotation: {currentTiltZ:F1}°  |  Tilt Angle: {tiltAngle:F1}°", GetXRDataStyle(14, tiltColor));
            yPos += lineHeight;
            
            // Show beaker position with coordinate system
            Vector3 beakerPos = beakerModel.transform.position;
            GUI.Label(new Rect(panelX + 25, yPos, mainPanelWidth - 50, lineHeight), $"📍 Position: X:{beakerPos.x:F1} Y:{beakerPos.y:F1} Z:{beakerPos.z:F1}", GetXRDataStyle(14, new Color(0.6f, 0.95f, 1f)));
            yPos += lineHeight;
            
            // Show beaker scale with LOCKED status indicator
            Vector3 beakerScale = beakerModel.transform.localScale;
            bool scaleCorrect = Vector3.Distance(beakerScale, FIXED_BEAKER_SCALE) < 0.01f;
            Color scaleColor = scaleCorrect ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.2f, 0.2f);
            string scaleIcon = scaleCorrect ? "🔒" : "⚠";
            string scaleStatus = scaleCorrect ? "LOCKED" : "ERROR";
            GUI.Label(new Rect(panelX + 25, yPos, mainPanelWidth - 50, lineHeight), $"{scaleIcon} Size: ({beakerScale.x:F1}, {beakerScale.y:F1}, {beakerScale.z:F1}) - {scaleStatus}", GetXRDataStyle(14, scaleColor));
            yPos += lineHeight + 10;
            
            // ═══════════════════════════════════════════════════════════
            // SECTION 2: LIQUID MANAGEMENT
            // ═══════════════════════════════════════════════════════════
            DrawXRSectionHeader(panelX + 18, yPos, mainPanelWidth - 36, "💧 LIQUID MANAGEMENT");
            yPos += 30;
            
            // Calculate liquid volume in mL (assuming 500mL beaker for scale 5)
            float volumeML = liquidAmount * 500f;
            Color liquidColor = liquidAmount > 0.5f ? new Color(0.3f, 1f, 0.4f) : (liquidAmount > 0.2f ? new Color(1f, 0.9f, 0.3f) : new Color(1f, 0.3f, 0.3f));
            GUI.Label(new Rect(panelX + 25, yPos, mainPanelWidth - 50, lineHeight), $"💧 Volume: {volumeML:F0} mL / 500 mL ({liquidAmount:P0})", GetXRDataStyle(15, liquidColor));
            yPos += 26;
            
            // Draw ADVANCED liquid progress bar with 3D effect
            float barWidth = mainPanelWidth - 50;
            int barHeight = 20;
            
            // Draw bar shadow
            GUI.Box(new Rect(panelX + 27, yPos + 2, barWidth, barHeight), "", GetBarShadowStyle());
            
            // Draw bar container with border
            GUI.Box(new Rect(panelX + 25, yPos, barWidth, barHeight), "", GetXRBarBorderStyle());
            GUI.Box(new Rect(panelX + 26, yPos + 1, barWidth - 2, barHeight - 2), "", GetBarBackgroundStyle());
            
            // Draw liquid fill with gradient glow effect
            if (liquidAmount > 0)
            {
                float fillWidth = (barWidth - 2) * liquidAmount;
                GUI.Box(new Rect(panelX + 26, yPos + 1, fillWidth, barHeight - 2), "", GetBarFillStyle(liquidColor));
                
                // Add top glow
                GUI.Box(new Rect(panelX + 26, yPos + 1, fillWidth, 5), "", GetBarGlowStyle(liquidColor));
                
                // Add percentage text on bar
                GUIStyle barTextStyle = new GUIStyle(GUI.skin.label);
                barTextStyle.fontSize = 12;
                barTextStyle.fontStyle = FontStyle.Bold;
                barTextStyle.alignment = TextAnchor.MiddleCenter;
                barTextStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(panelX + 25, yPos, barWidth, barHeight), $"{liquidAmount:P0}", barTextStyle);
            }
            yPos += barHeight + 14;
            
            // Show pouring status with XR terminology
            if (tiltAngle > pouringThresholdAngle && liquidAmount > 0)
            {
                GUI.Label(new Rect(panelX + 25, yPos, mainPanelWidth - 50, lineHeight), "⚗️ LIQUID TRANSFER ACTIVE", GetXRStatusStyle(16, new Color(0.3f, 1f, 0.4f)));
            }
            else if (liquidAmount <= 0)
            {
                GUI.Label(new Rect(panelX + 25, yPos, mainPanelWidth - 50, lineHeight), "⚠ CONTAINER EMPTY", GetXRStatusStyle(16, new Color(1f, 0.3f, 0.3f)));
            }
            else
            {
                GUI.Label(new Rect(panelX + 25, yPos, mainPanelWidth - 50, lineHeight), "✓ READY FOR OPERATION", GetXRStatusStyle(16, new Color(0.4f, 0.9f, 1f)));
            }
            yPos += lineHeight + 12;

            // ═══════════════════════════════════════════════════════════
            // SECTION 3: GESTURE TRACKING SYSTEM
            // ═══════════════════════════════════════════════════════════
            DrawXRSectionHeader(panelX + 18, yPos, mainPanelWidth - 36, "👋 GESTURE TRACKING");
            yPos += 30;
            
            // Hand tracking info with detailed status
            if (ManoMotionManager.Instance?.HandInfos != null)
            {
                bool handDetected = false;
                foreach (var handInfo in ManoMotionManager.Instance.HandInfos)
                {
                    if (handInfo.gestureInfo.manoClass != ManoClass.NO_HAND)
                    {
                        handDetected = true;
                        var bbox = handInfo.trackingInfo.boundingBox;
                        float centerX = bbox.topLeft.x + bbox.width / 2f;
                        float centerY = bbox.topLeft.y - bbox.height / 2f;
                        
                        GUI.Label(new Rect(panelX + 25, yPos, mainPanelWidth - 50, lineHeight), $"✓ System Status: TRACKING ACTIVE", GetXRDataStyle(14, new Color(0.3f, 1f, 0.4f)));
                        yPos += lineHeight;
                        GUI.Label(new Rect(panelX + 25, yPos, mainPanelWidth - 50, lineHeight), $"   Hand Position: ({centerX:F2}, {centerY:F2})", GetXRDataStyle(13, new Color(0.6f, 0.95f, 1f)));
                        yPos += lineHeight;
                        GUI.Label(new Rect(panelX + 25, yPos, mainPanelWidth - 50, lineHeight), $"   Confidence: HIGH", GetXRDataStyle(13, new Color(0.3f, 1f, 0.4f)));
                        break;
                    }
                }
                
                if (!handDetected)
                {
                    GUI.Label(new Rect(panelX + 25, yPos, mainPanelWidth - 50, lineHeight), "⚠ System Status: WAITING FOR INPUT", GetXRDataStyle(14, new Color(1f, 0.6f, 0.3f)));
                    yPos += lineHeight;
                    GUI.Label(new Rect(panelX + 25, yPos, mainPanelWidth - 50, lineHeight), "   Position hand in camera view", GetXRDataStyle(12, new Color(0.6f, 0.6f, 0.6f)));
                }
            }
            
            // ═══════════════════════════════════════════════════════════
            // FOOTER - Quick Actions
            // ═══════════════════════════════════════════════════════════
            yPos = panelY + mainPanelHeight - 45;
            GUI.Box(new Rect(panelX, yPos, mainPanelWidth, 1), "", MakeGlowLineStyle());
            
            GUIStyle footerStyle = new GUIStyle(GUI.skin.label);
            footerStyle.fontSize = 10;
            footerStyle.normal.textColor = new Color(0.5f, 0.7f, 0.8f);
            footerStyle.alignment = TextAnchor.MiddleLeft;
            GUI.Label(new Rect(panelX + 20, yPos + 8, mainPanelWidth - 40, 30), "Quick Actions: ⟲ Reset | 💧 Refill | 🗑 Clear | Gestures: ✋ Tilt | ✊ Move | 👌 Refill", footerStyle);
        }
    }
    
    // Helper method to draw section headers
    void DrawSectionHeader(float x, float y, float width, string text)
    {
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 14;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = new Color(0.5f, 0.9f, 1f);
        headerStyle.alignment = TextAnchor.MiddleLeft;
        
        // Draw underline
        GUI.Box(new Rect(x, y + 18, width, 2), "", GetBorderStyle());
        GUI.Label(new Rect(x, y, width, 20), text, headerStyle);
    }
    
    // Chemistry lab section header with enhanced styling
    void DrawChemSectionHeader(float x, float y, float width, string text)
    {
        // Draw background bar
        GUI.Box(new Rect(x - 5, y, width + 10, 26), "", GetChemHeaderBgStyle());
        
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 15;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = new Color(0.85f, 1f, 1f);
        headerStyle.alignment = TextAnchor.MiddleLeft;
        
        // Draw glow line at bottom
        GUI.Box(new Rect(x, y + 24, width, 2), "", MakeGlowLineStyle());
        GUI.Label(new Rect(x + 2, y + 2, width, 22), text, headerStyle);
    }
    
    GUIStyle GetChemHeaderBgStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0.08f, 0.2f, 0.35f, 0.7f));
        return style;
    }
    
    // Data label style
    GUIStyle GetDataLabelStyle(int fontSize, Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.fontStyle = FontStyle.Normal;
        style.normal.textColor = color;
        style.alignment = TextAnchor.MiddleLeft;
        return style;
    }
    
    // Chemistry data label style (monospace-like)
    GUIStyle GetChemDataStyle(int fontSize, Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.fontStyle = FontStyle.Normal;
        style.normal.textColor = color;
        style.alignment = TextAnchor.MiddleLeft;
        return style;
    }
    
    // Status label style (bold)
    GUIStyle GetStatusLabelStyle(int fontSize, Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = color;
        style.alignment = TextAnchor.MiddleLeft;
        return style;
    }
    
    // Chemistry status label (bold with glow effect)
    GUIStyle GetChemStatusStyle(int fontSize, Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = color;
        style.alignment = TextAnchor.MiddleLeft;
        return style;
    }
    
    // Progress bar background
    GUIStyle GetBarBackgroundStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0.05f, 0.08f, 0.12f, 0.95f));
        return style;
    }
    
    // Progress bar fill
    GUIStyle GetBarFillStyle(Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, color);
        return style;
    }
    
    // Chemistry bar border
    GUIStyle GetChemBarBorderStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0.3f, 0.7f, 0.9f, 0.6f));
        return style;
    }
    
    // Bar glow effect
    GUIStyle GetBarGlowStyle(Color baseColor)
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        Color glowColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.8f);
        style.normal.background = MakeTex(2, 2, glowColor);
        return style;
    }
    
    // ═══════════════════════════════════════════════════════════
    // XR CHEMISTRY LAB - ADVANCED STYLE METHODS
    // ═══════════════════════════════════════════════════════════
    
    // XR Section Header with advanced styling
    void DrawXRSectionHeader(float x, float y, float width, string text)
    {
        // Draw background bar with gradient
        GUI.Box(new Rect(x - 7, y, width + 14, 26), "", GetXRHeaderBgStyle());
        
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 15;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = new Color(0.9f, 1f, 1f);
        headerStyle.alignment = TextAnchor.MiddleLeft;
        
        // Draw glow line at bottom
        GUI.Box(new Rect(x, y + 24, width, 2), "", MakeGlowLineStyle());
        GUI.Label(new Rect(x, y + 3, width, 20), text, headerStyle);
    }
    
    GUIStyle GetXRHeaderBgStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0.05f, 0.15f, 0.28f, 0.8f));
        return style;
    }
    
    // XR Data label style
    GUIStyle GetXRDataStyle(int fontSize, Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.fontStyle = FontStyle.Normal;
        style.normal.textColor = color;
        style.alignment = TextAnchor.MiddleLeft;
        return style;
    }
    
    // XR Status label (bold with emphasis)
    GUIStyle GetXRStatusStyle(int fontSize, Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = color;
        style.alignment = TextAnchor.MiddleLeft;
        return style;
    }
    
    // Control button style (colored buttons)
    GUIStyle GetControlButtonStyle(Color baseColor)
    {
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        style.normal.background = MakeTex(2, 2, new Color(baseColor.r, baseColor.g, baseColor.b, 0.85f));
        style.hover.background = MakeTex(2, 2, new Color(baseColor.r * 1.2f, baseColor.g * 1.2f, baseColor.b * 1.2f, 0.95f));
        style.active.background = MakeTex(2, 2, new Color(baseColor.r * 0.8f, baseColor.g * 0.8f, baseColor.b * 0.8f, 0.95f));
        style.alignment = TextAnchor.MiddleCenter;
        return style;
    }
    
    // XR Bar border (enhanced)
    GUIStyle GetXRBarBorderStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0.4f, 0.8f, 1f, 0.7f));
        return style;
    }
    
    // Bar shadow effect
    GUIStyle GetBarShadowStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.3f));
        return style;
    }
}