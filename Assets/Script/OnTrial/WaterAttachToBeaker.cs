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
        
        // CRITICAL: Store original beaker scale, position and rotation BEFORE any operations
        originalBeakerScale = beakerModel.transform.localScale;
        initialBeakerPosition = beakerModel.transform.position;
        initialBeakerRotation = beakerModel.transform.rotation;
        
        // FORCE scale immediately to prevent any initial scaling issues
        beakerModel.transform.localScale = originalBeakerScale;
        
        Debug.Log($"WaterAttachToBeaker initialized. Beaker at: {initialBeakerPosition}, Scale: {originalBeakerScale}");
        
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
                    // ALWAYS preserve scale during any gesture
                    beakerModel.transform.localScale = originalBeakerScale;
                    
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
                    
                    // Debug logging
                    if (showDebugVisuals)
                    {
                        Debug.Log($"OPEN_HAND: normalizedX={normalizedX:F3}, tiltInput={tiltInput:F3}, desiredTilt={desiredTiltZ:F1}°, currentTilt={currentTiltZ:F1}°, diff={angleDiff:F1}°");
                    }
                    break;

                case ManoGestureContinuous.OPEN_PINCH_GESTURE:
                    // ALWAYS preserve scale during any gesture
                    beakerModel.transform.localScale = originalBeakerScale;
                    
                    beakerStatus = "Refilling";
                    
                    // Reset rotation to upright
                    beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 3f);
                    
                    // Refill the beaker
                    if (liquidAmount < 1.0f)
                    {
                        liquidAmount = Mathf.Min(1.0f, liquidAmount + Time.deltaTime * 2f); // Refill over 0.5 seconds
                    }
                    
                    if (showDebugVisuals)
                    {
                        Debug.Log($"PINCH: Refilling beaker - Liquid: {liquidAmount:P0}");
                    }
                    break;

                case ManoGestureContinuous.CLOSED_HAND_GESTURE:
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
                    
                    // Store current scale before ANY transform operation
                    Vector3 scaleBeforeMove = originalBeakerScale;
                    
                    // Smooth movement with configurable speed
                    beakerModel.transform.position = Vector3.Lerp(
                        beakerModel.transform.position, 
                        targetPosition, 
                        grabSmoothness
                    );
                    
                    // IMMEDIATELY restore scale after movement
                    beakerModel.transform.localScale = scaleBeforeMove;
                    
                    if (showDebugVisuals)
                    {
                        Debug.Log($"GRAB: Hand={handPosition}, Target={targetPosition}, Current={beakerModel.transform.position}, Scale={beakerModel.transform.localScale}");
                    }
                    break;
            }
        }
        
        // CRITICAL: ALWAYS enforce scale every frame BEFORE and AFTER all operations
        // This prevents ANY scaling issues from Unity's transform system
        Vector3 currentScale = beakerModel.transform.localScale;
        if (Vector3.Distance(currentScale, originalBeakerScale) > 0.001f)
        {
            beakerModel.transform.localScale = originalBeakerScale;
            Debug.LogWarning($"Scale corrected! Was: {currentScale}, Reset to: {originalBeakerScale}");
        }
        
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
    
    // LateUpdate runs AFTER all Update functions - final scale enforcement
    void LateUpdate()
    {
        // FINAL scale enforcement after ALL Unity operations
        if (beakerModel != null && Vector3.Distance(beakerModel.transform.localScale, originalBeakerScale) > 0.001f)
        {
            beakerModel.transform.localScale = originalBeakerScale;
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
        beakerModel.transform.localScale = originalBeakerScale;
        liquidAmount = 1.0f;
        beakerStatus = "Reset Complete";
        Debug.Log($"Beaker reset to initial state - Position: {initialBeakerPosition}, Scale: {originalBeakerScale}");
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
    
    private GUIStyle GetBorderStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0.2f, 0.8f, 1f, 0.5f)); // Cyan glow
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
            // BOTTOM-LEFT PANEL - DIGITAL CHEMISTRY LAB INTERFACE
            // ═══════════════════════════════════════════════════════════
            int panelWidth = 380;
            int panelHeight = 280;
            int panelX = 15;
            int panelY = screenHeight - panelHeight - 15; // Bottom-left positioning
            
            // Draw main panel with modern dark background
            GUIStyle panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = MakeTex(2, 2, new Color(0.05f, 0.05f, 0.1f, 0.92f)); // Dark blue-black
            GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), "", panelStyle);
            
            // Draw accent border (cyan glow effect)
            GUI.Box(new Rect(panelX - 2, panelY - 2, panelWidth + 4, panelHeight + 4), "", GetBorderStyle());
            GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), "", panelStyle);
            
            // Draw header section with gradient
            GUIStyle headerBg = new GUIStyle(GUI.skin.box);
            headerBg.normal.background = MakeTex(2, 2, new Color(0.1f, 0.4f, 0.6f, 0.8f)); // Cyan gradient
            GUI.Box(new Rect(panelX, panelY, panelWidth, 45), "", headerBg);
            
            // Lab title with icon
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;
            titleStyle.alignment = TextAnchor.MiddleLeft;
            GUI.Label(new Rect(panelX + 15, panelY + 8, panelWidth - 130, 30), "🧪 DIGITAL CHEMISTRY LAB", titleStyle);
            
            // Add RESET button with modern style
            GUIStyle resetButtonStyle = GetModernButtonStyle();
            if (GUI.Button(new Rect(panelX + panelWidth - 115, panelY + 8, 100, 30), "⟲ RESET", resetButtonStyle))
            {
                ResetBeakerPosition();
            }
            
            int yPos = panelY + 55;
            int lineHeight = 24;
            
            // Get beaker tilt angle
            float currentTiltZ = beakerModel.transform.eulerAngles.z;
            if (currentTiltZ > 180f) currentTiltZ -= 360f;
            Vector3 beakerUp = beakerModel.transform.up;
            float tiltAngle = Vector3.Angle(beakerUp, Vector3.up);
            
            // Section: BEAKER STATUS
            DrawSectionHeader(panelX + 15, yPos, panelWidth - 30, "📊 BEAKER STATUS");
            yPos += 28;
            
            // Beaker Tilt with icon
            GUI.Label(new Rect(panelX + 20, yPos, panelWidth - 40, lineHeight), $"↻ Tilt: {currentTiltZ:F1}°  |  Angle: {tiltAngle:F1}°", GetDataLabelStyle(15, new Color(0.7f, 0.9f, 1f)));
            yPos += lineHeight;
            
            // Show beaker position with icon
            Vector3 beakerPos = beakerModel.transform.position;
            GUI.Label(new Rect(panelX + 20, yPos, panelWidth - 40, lineHeight), $"📍 Position: ({beakerPos.x:F1}, {beakerPos.y:F1}, {beakerPos.z:F1})", GetDataLabelStyle(15, new Color(0.7f, 0.9f, 1f)));
            yPos += lineHeight;
            
            // Show beaker scale with status indicator
            Vector3 beakerScale = beakerModel.transform.localScale;
            bool scaleCorrect = Vector3.Distance(beakerScale, originalBeakerScale) < 0.001f;
            Color scaleColor = scaleCorrect ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.3f, 0.3f);
            string scaleIcon = scaleCorrect ? "✓" : "✗";
            GUI.Label(new Rect(panelX + 20, yPos, panelWidth - 40, lineHeight), $"{scaleIcon} Scale: ({beakerScale.x:F2}, {beakerScale.y:F2}, {beakerScale.z:F2})", GetDataLabelStyle(15, scaleColor));
            yPos += lineHeight + 5;
            
            // Section: LIQUID LEVEL
            DrawSectionHeader(panelX + 15, yPos, panelWidth - 30, "💧 LIQUID LEVEL");
            yPos += 28;
            
            // Liquid Amount with modern progress bar
            Color liquidColor = liquidAmount > 0.5f ? new Color(0.3f, 1f, 0.3f) : (liquidAmount > 0.2f ? new Color(1f, 0.9f, 0.3f) : new Color(1f, 0.3f, 0.3f));
            GUI.Label(new Rect(panelX + 20, yPos, panelWidth - 40, lineHeight), $"Volume: {liquidAmount:P0}", GetDataLabelStyle(16, liquidColor));
            yPos += 22;
            
            // Draw modern liquid progress bar with border
            float barWidth = panelWidth - 40;
            int barHeight = 12;
            GUI.Box(new Rect(panelX + 20, yPos, barWidth, barHeight), "", GetBarBackgroundStyle());
            if (liquidAmount > 0)
            {
                GUI.Box(new Rect(panelX + 20, yPos, barWidth * liquidAmount, barHeight), "", GetBarFillStyle(liquidColor));
            }
            yPos += barHeight + 10;
            
            // Show pouring status with icon
            if (tiltAngle > pouringThresholdAngle && liquidAmount > 0)
            {
                GUI.Label(new Rect(panelX + 20, yPos, panelWidth - 40, lineHeight), "💧 POURING ACTIVE", GetStatusLabelStyle(17, new Color(0.3f, 1f, 0.3f)));
            }
            else if (liquidAmount <= 0)
            {
                GUI.Label(new Rect(panelX + 20, yPos, panelWidth - 40, lineHeight), "⚠ BEAKER EMPTY", GetStatusLabelStyle(17, new Color(1f, 0.3f, 0.3f)));
            }
            else
            {
                GUI.Label(new Rect(panelX + 20, yPos, panelWidth - 40, lineHeight), "● READY TO POUR", GetStatusLabelStyle(17, new Color(0.3f, 0.8f, 1f)));
            }
            yPos += lineHeight + 5;

            // Section: HAND TRACKING
            DrawSectionHeader(panelX + 15, yPos, panelWidth - 30, "👋 HAND TRACKING");
            yPos += 28;
            
            // Hand tracking info
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
                        
                        GUI.Label(new Rect(panelX + 20, yPos, panelWidth - 40, lineHeight), $"✓ Hand Detected: ({centerX:F2}, {centerY:F2})", GetDataLabelStyle(14, new Color(0.3f, 1f, 0.3f)));
                        break;
                    }
                }
                
                if (!handDetected)
                {
                    GUI.Label(new Rect(panelX + 20, yPos, panelWidth - 40, lineHeight), "⚠ No Hand Detected", GetDataLabelStyle(14, new Color(1f, 0.5f, 0.3f)));
                }
            }
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
    
    // Progress bar background
    GUIStyle GetBarBackgroundStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.15f, 0.9f));
        return style;
    }
    
    // Progress bar fill
    GUIStyle GetBarFillStyle(Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, color);
        return style;
    }
}