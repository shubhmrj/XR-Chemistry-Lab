using UnityEngine;
using ManoMotion;
using System.Collections.Generic;

/// <summary>
/// Advanced XR Chemistry Lab System
/// Supports multiple beakers, fixed sizing, professional UI, and intuitive interactions
/// </summary>
public class AdvancedXRChemistryLab : MonoBehaviour
{
    [Header("Lab Equipment")]
    [SerializeField] private List<GameObject> beakers = new List<GameObject>();
    [SerializeField] private GameObject waterParticlesPrefab;
    
    [Header("Beaker Settings")]
    [SerializeField] private Vector3 FIXED_BEAKER_SCALE = new Vector3(5f, 5f, 5f);
    [SerializeField] private float pouringThresholdAngle = 25f;
    [SerializeField] private float maxPourRate = 100.0f;
    
    [Header("Interaction Settings")]
    [SerializeField] private float grabSmoothness = 0.15f;
    [SerializeField] private float tiltSmoothSpeed = 12f;
    [SerializeField] private float maxTiltAngle = 60f;
    [SerializeField] private float grabDetectionRadius = 1.5f;
    
    [Header("Safety & Bounds")]
    [SerializeField] private bool enableSafetyBounds = true;
    [SerializeField] private Vector3 minBounds = new Vector3(-5f, -2f, 5f);
    [SerializeField] private Vector3 maxBounds = new Vector3(5f, 5f, 15f);
    
    // Beaker data structure
    private class BeakerData
    {
        public GameObject beakerObject;
        public Transform pourPoint;
        public GameObject waterEffect;
        public ParticleSystem waterParticles;
        public float liquidAmount = 1.0f;
        public Vector3 initialPosition;
        public Quaternion initialRotation;
        public bool isGrabbed = false;
        public Color liquidColor = new Color(0.3f, 0.7f, 1f, 0.8f);
    }
    
    private List<BeakerData> beakerDataList = new List<BeakerData>();
    private BeakerData currentlyGrabbedBeaker = null;
    private BeakerData currentlyTiltedBeaker = null;
    
    // Gesture tracking
    private ManoGestureContinuous currentGesture = ManoGestureContinuous.NO_GESTURE;
    private string systemStatus = "System Ready";
    
    // UI State
    private bool showUI = true;
    private GUIStyle cachedButtonStyle;
    private GUIStyle cachedLabelStyle;
    private GUIStyle cachedPanelStyle;

    void Start()
    {
        InitializeManoMotion();
        InitializeBeakers();
        Debug.Log($"Advanced XR Chemistry Lab initialized with {beakerDataList.Count} beakers");
    }

    void InitializeManoMotion()
    {
        if (ManoMotionManager.Instance != null)
        {
            ManoMotionManager.Instance.ShouldCalculateGestures(true);
        }
    }

    void InitializeBeakers()
    {
        // Initialize all beakers in the scene
        foreach (var beakerObj in beakers)
        {
            if (beakerObj == null) continue;
            
            BeakerData data = new BeakerData
            {
                beakerObject = beakerObj,
                initialPosition = beakerObj.transform.position,
                initialRotation = beakerObj.transform.rotation,
                liquidAmount = 1.0f
            };
            
            // CRITICAL: Lock scale to FIXED size
            beakerObj.transform.localScale = FIXED_BEAKER_SCALE;
            
            // Create pour point
            GameObject pourPointObj = new GameObject($"PourPoint_{beakerObj.name}");
            pourPointObj.transform.parent = beakerObj.transform;
            pourPointObj.transform.localPosition = new Vector3(0, 0.45f, 0.25f);
            data.pourPoint = pourPointObj.transform;
            
            // Create water particle system
            if (waterParticlesPrefab != null)
            {
                data.waterEffect = Instantiate(waterParticlesPrefab);
                data.waterEffect.name = $"WaterEffect_{beakerObj.name}";
                data.waterParticles = data.waterEffect.GetComponent<ParticleSystem>();
                
                if (data.waterParticles != null)
                {
                    var main = data.waterParticles.main;
                    main.startColor = data.liquidColor;
                    data.waterParticles.Stop();
                }
            }
            
            beakerDataList.Add(data);
        }
    }

    void Update()
    {
        // Enforce scale lock on ALL beakers every frame
        EnforceScaleLock();
        
        // Process hand gestures
        ProcessGestures();
        
        // Update water pouring for all beakers
        UpdateWaterPouring();
        
        // Update system status
        UpdateSystemStatus();
    }

    void LateUpdate()
    {
        // Final scale enforcement after all Unity operations
        EnforceScaleLock();
    }

    void EnforceScaleLock()
    {
        foreach (var beakerData in beakerDataList)
        {
            if (beakerData.beakerObject != null)
            {
                beakerData.beakerObject.transform.localScale = FIXED_BEAKER_SCALE;
            }
        }
    }

    void ProcessGestures()
    {
        if (ManoMotionManager.Instance?.HandInfos == null) return;

        foreach (var handInfo in ManoMotionManager.Instance.HandInfos)
        {
            if (handInfo.gestureInfo.manoClass == ManoClass.NO_HAND) continue;

            currentGesture = handInfo.gestureInfo.manoGestureContinuous;
            var bbox = handInfo.trackingInfo.boundingBox;
            
            Vector3 handPosition = CalculateHandPosition(bbox);

            switch (currentGesture)
            {
                case ManoGestureContinuous.CLOSED_HAND_GESTURE:
                    HandleGrabGesture(handPosition);
                    break;

                case ManoGestureContinuous.OPEN_HAND_GESTURE:
                    HandleTiltGesture(bbox);
                    break;

                case ManoGestureContinuous.OPEN_PINCH_GESTURE:
                    HandleRefillGesture();
                    break;

                default:
                    ReleaseAllBeakers();
                    break;
            }
        }
    }

    void HandleGrabGesture(Vector3 handPosition)
    {
        // ABSOLUTE SCALE LOCK before grab
        EnforceScaleLock();
        
        // If no beaker is currently grabbed, find the nearest one
        if (currentlyGrabbedBeaker == null)
        {
            currentlyGrabbedBeaker = FindNearestBeaker(handPosition);
            if (currentlyGrabbedBeaker != null)
            {
                currentlyGrabbedBeaker.isGrabbed = true;
                systemStatus = $"Grabbed: {currentlyGrabbedBeaker.beakerObject.name}";
            }
        }

        // Move the grabbed beaker
        if (currentlyGrabbedBeaker != null)
        {
            Vector3 targetPosition = CalculateBeakerPosition(handPosition);
            
            // Apply safety bounds
            if (enableSafetyBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
                targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.z, maxBounds.z);
            }
            
            // Smooth movement
            currentlyGrabbedBeaker.beakerObject.transform.position = Vector3.Lerp(
                currentlyGrabbedBeaker.beakerObject.transform.position,
                targetPosition,
                grabSmoothness
            );
            
            // Keep upright while grabbing
            currentlyGrabbedBeaker.beakerObject.transform.rotation = Quaternion.Lerp(
                currentlyGrabbedBeaker.beakerObject.transform.rotation,
                Quaternion.identity,
                Time.deltaTime * 3f
            );
        }
        
        // ABSOLUTE SCALE LOCK after grab
        EnforceScaleLock();
    }

    void HandleTiltGesture(BoundingBox bbox)
    {
        // ABSOLUTE SCALE LOCK before tilt
        EnforceScaleLock();
        
        // Use the nearest beaker or currently grabbed one
        BeakerData beakerToTilt = currentlyTiltedBeaker ?? FindNearestBeaker(CalculateHandPosition(bbox));
        
        if (beakerToTilt != null)
        {
            currentlyTiltedBeaker = beakerToTilt;
            systemStatus = $"Tilting: {beakerToTilt.beakerObject.name}";
            
            // Calculate tilt based on hand position
            float centerX = bbox.topLeft.x + bbox.width / 2f;
            float normalizedX = centerX - 0.5f;
            float tiltInput = normalizedX * 2f;
            float desiredTiltZ = Mathf.Clamp(tiltInput * maxTiltAngle, -maxTiltAngle, maxTiltAngle);
            
            // Apply tilt rotation
            float currentTiltZ = beakerToTilt.beakerObject.transform.eulerAngles.z;
            if (currentTiltZ > 180f) currentTiltZ -= 360f;
            
            float angleDiff = desiredTiltZ - currentTiltZ;
            
            if (Mathf.Abs(angleDiff) > 0.5f)
            {
                float rotateAmount = angleDiff * Time.deltaTime * tiltSmoothSpeed;
                beakerToTilt.beakerObject.transform.RotateAround(
                    beakerToTilt.pourPoint.position,
                    Vector3.forward,
                    rotateAmount
                );
            }
        }
        
        // ABSOLUTE SCALE LOCK after tilt
        EnforceScaleLock();
    }

    void HandleRefillGesture()
    {
        // Refill the nearest beaker or currently selected one
        BeakerData beakerToRefill = currentlyGrabbedBeaker ?? currentlyTiltedBeaker;
        
        if (beakerToRefill == null && beakerDataList.Count > 0)
        {
            beakerToRefill = beakerDataList[0];
        }
        
        if (beakerToRefill != null)
        {
            beakerToRefill.liquidAmount = Mathf.Min(1.0f, beakerToRefill.liquidAmount + Time.deltaTime * 2f);
            beakerToRefill.beakerObject.transform.rotation = Quaternion.Lerp(
                beakerToRefill.beakerObject.transform.rotation,
                Quaternion.identity,
                Time.deltaTime * 3f
            );
            systemStatus = $"Refilling: {beakerToRefill.beakerObject.name}";
        }
    }

    void ReleaseAllBeakers()
    {
        if (currentlyGrabbedBeaker != null)
        {
            currentlyGrabbedBeaker.isGrabbed = false;
            currentlyGrabbedBeaker = null;
        }
        currentlyTiltedBeaker = null;
        
        if (currentGesture == ManoGestureContinuous.NO_GESTURE)
        {
            systemStatus = "System Ready";
        }
    }

    BeakerData FindNearestBeaker(Vector3 handPosition)
    {
        BeakerData nearest = null;
        float minDistance = grabDetectionRadius;
        
        foreach (var beakerData in beakerDataList)
        {
            if (beakerData.beakerObject == null) continue;
            
            float distance = Vector3.Distance(handPosition, beakerData.beakerObject.transform.position);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = beakerData;
            }
        }
        
        return nearest;
    }

    Vector3 CalculateHandPosition(BoundingBox bbox)
    {
        float centerX = bbox.topLeft.x + bbox.width / 2f;
        float centerY = bbox.topLeft.y - bbox.height / 2f;
        
        Vector3 screenPos = new Vector3(centerX * Screen.width, centerY * Screen.height, 10f);
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    Vector3 CalculateBeakerPosition(Vector3 handPosition)
    {
        return handPosition + new Vector3(0, 0f, 10f);
    }

    void UpdateWaterPouring()
    {
        foreach (var beakerData in beakerDataList)
        {
            if (beakerData.waterParticles == null || beakerData.liquidAmount <= 0) continue;
            
            // Check tilt angle
            Vector3 beakerUp = beakerData.beakerObject.transform.up;
            float tiltAngle = Vector3.Angle(beakerUp, Vector3.up);
            
            if (tiltAngle > pouringThresholdAngle)
            {
                // Start pouring
                if (!beakerData.waterParticles.isPlaying)
                {
                    beakerData.waterParticles.Play();
                }
                
                // Position particles at pour point
                beakerData.waterEffect.transform.position = beakerData.pourPoint.position;
                beakerData.waterEffect.transform.rotation = beakerData.beakerObject.transform.rotation;
                
                // Decrease liquid
                float pourRate = (tiltAngle - pouringThresholdAngle) / 90f;
                beakerData.liquidAmount -= pourRate * Time.deltaTime * 0.5f;
                beakerData.liquidAmount = Mathf.Max(0, beakerData.liquidAmount);
            }
            else
            {
                // Stop pouring
                if (beakerData.waterParticles.isPlaying)
                {
                    beakerData.waterParticles.Stop();
                }
            }
        }
    }

    void UpdateSystemStatus()
    {
        if (currentGesture == ManoGestureContinuous.NO_GESTURE && 
            currentlyGrabbedBeaker == null && 
            currentlyTiltedBeaker == null)
        {
            systemStatus = "System Ready";
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC METHODS - For UI Buttons
    // ═══════════════════════════════════════════════════════════
    
    public void ResetAllBeakers()
    {
        foreach (var beakerData in beakerDataList)
        {
            beakerData.beakerObject.transform.position = beakerData.initialPosition;
            beakerData.beakerObject.transform.rotation = beakerData.initialRotation;
            beakerData.beakerObject.transform.localScale = FIXED_BEAKER_SCALE;
            beakerData.liquidAmount = 1.0f;
            beakerData.isGrabbed = false;
        }
        currentlyGrabbedBeaker = null;
        currentlyTiltedBeaker = null;
        systemStatus = "All Beakers Reset";
    }

    public void RefillAllBeakers()
    {
        foreach (var beakerData in beakerDataList)
        {
            beakerData.liquidAmount = 1.0f;
        }
        systemStatus = "All Beakers Refilled";
    }

    public void ClearAllBeakers()
    {
        foreach (var beakerData in beakerDataList)
        {
            beakerData.liquidAmount = 0f;
        }
        systemStatus = "All Beakers Cleared";
    }

    public void ToggleUI()
    {
        showUI = !showUI;
    }

    // ═══════════════════════════════════════════════════════════
    // ADVANCED XR GUI - BOTTOM PANEL
    // ═══════════════════════════════════════════════════════════
    
    void OnGUI()
    {
        if (!showUI) return;
        
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        
        // ═══════════════════════════════════════════════════════════
        // BOTTOM CONTROL PANEL - Single Line Buttons
        // ═══════════════════════════════════════════════════════════
        
        int panelHeight = 80;
        int panelY = screenHeight - panelHeight - 10;
        int panelX = 10;
        int panelWidth = screenWidth - 20;
        
        // Draw panel background
        GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), "", GetPanelStyle());
        
        // Button dimensions
        int buttonHeight = 50;
        int buttonWidth = 120;
        int buttonSpacing = 15;
        int buttonY = panelY + 15;
        
        // Calculate starting X for centered buttons
        int totalButtonsWidth = (buttonWidth * 5) + (buttonSpacing * 4);
        int startX = panelX + (panelWidth - totalButtonsWidth) / 2;
        
        // RESET Button
        if (GUI.Button(new Rect(startX, buttonY, buttonWidth, buttonHeight), 
            "⟲ RESET\nALL", GetButtonStyle(new Color(0.9f, 0.3f, 0.3f))))
        {
            ResetAllBeakers();
        }
        
        // REFILL Button
        if (GUI.Button(new Rect(startX + (buttonWidth + buttonSpacing), buttonY, buttonWidth, buttonHeight), 
            "💧 REFILL\nALL", GetButtonStyle(new Color(0.2f, 0.6f, 1f))))
        {
            RefillAllBeakers();
        }
        
        // CLEAR Button
        if (GUI.Button(new Rect(startX + (buttonWidth + buttonSpacing) * 2, buttonY, buttonWidth, buttonHeight), 
            "🗑 CLEAR\nALL", GetButtonStyle(new Color(0.5f, 0.5f, 0.5f))))
        {
            ClearAllBeakers();
        }
        
        // HELP Button
        if (GUI.Button(new Rect(startX + (buttonWidth + buttonSpacing) * 3, buttonY, buttonWidth, buttonHeight), 
            "❓ HELP\nGUIDE", GetButtonStyle(new Color(0.3f, 0.8f, 0.3f))))
        {
            // Show help overlay
            systemStatus = "✋ Tilt | ✊ Grab | 👌 Refill";
        }
        
        // TOGGLE UI Button
        if (GUI.Button(new Rect(startX + (buttonWidth + buttonSpacing) * 4, buttonY, buttonWidth, buttonHeight), 
            "👁 HIDE\nUI", GetButtonStyle(new Color(0.6f, 0.4f, 0.8f))))
        {
            ToggleUI();
        }
        
        // ═══════════════════════════════════════════════════════════
        // TOP STATUS BAR - Compact Info
        // ═══════════════════════════════════════════════════════════
        
        int statusHeight = 60;
        int statusY = 10;
        int statusWidth = 500;
        int statusX = (screenWidth - statusWidth) / 2;
        
        // Status background
        GUI.Box(new Rect(statusX, statusY, statusWidth, statusHeight), "", GetStatusPanelStyle());
        
        // Gesture icon and status
        string gestureIcon = GetGestureIcon();
        Color gestureColor = GetGestureColor();
        
        GUIStyle iconStyle = new GUIStyle(GUI.skin.label);
        iconStyle.fontSize = 32;
        iconStyle.alignment = TextAnchor.MiddleCenter;
        iconStyle.normal.textColor = gestureColor;
        GUI.Label(new Rect(statusX + 10, statusY + 10, 50, 40), gestureIcon, iconStyle);
        
        GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.fontSize = 16;
        statusStyle.fontStyle = FontStyle.Bold;
        statusStyle.alignment = TextAnchor.MiddleLeft;
        statusStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(statusX + 70, statusY + 10, statusWidth - 80, 20), systemStatus, statusStyle);
        
        // Beaker count
        GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
        infoStyle.fontSize = 12;
        infoStyle.alignment = TextAnchor.MiddleLeft;
        infoStyle.normal.textColor = new Color(0.7f, 0.9f, 1f);
        GUI.Label(new Rect(statusX + 70, statusY + 32, statusWidth - 80, 20), 
            $"Beakers: {beakerDataList.Count} | Active: {(currentlyGrabbedBeaker != null ? currentlyGrabbedBeaker.beakerObject.name : "None")}", 
            infoStyle);
        
        // ═══════════════════════════════════════════════════════════
        // SIDE INFO PANEL - Beaker Details (Compact)
        // ═══════════════════════════════════════════════════════════
        
        int sideWidth = 280;
        int sideHeight = 200;
        int sideX = screenWidth - sideWidth - 10;
        int sideY = 80;
        
        GUI.Box(new Rect(sideX, sideY, sideWidth, sideHeight), "", GetSidePanelStyle());
        
        // Title
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = new Color(0.8f, 1f, 1f);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(sideX + 10, sideY + 10, sideWidth - 20, 25), "⚗️ BEAKER STATUS", titleStyle);
        
        // Beaker list
        int yPos = sideY + 45;
        int lineHeight = 30;
        
        GUIStyle beakerStyle = new GUIStyle(GUI.skin.label);
        beakerStyle.fontSize = 12;
        beakerStyle.normal.textColor = Color.white;
        
        int displayCount = Mathf.Min(beakerDataList.Count, 4);
        for (int i = 0; i < displayCount; i++)
        {
            var beaker = beakerDataList[i];
            string beakerName = beaker.beakerObject.name;
            float liquid = beaker.liquidAmount * 100f;
            Color barColor = liquid > 50f ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.6f, 0.3f);
            
            // Beaker name
            GUI.Label(new Rect(sideX + 15, yPos, sideWidth - 30, 15), 
                $"{i + 1}. {beakerName}", beakerStyle);
            
            // Liquid bar
            float barWidth = sideWidth - 30;
            GUI.Box(new Rect(sideX + 15, yPos + 16, barWidth, 10), "", GetBarBorderStyle());
            GUI.Box(new Rect(sideX + 16, yPos + 17, (barWidth - 2) * (liquid / 100f), 8), "", GetBarFillStyle(barColor));
            
            yPos += lineHeight;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // GUI STYLE METHODS
    // ═══════════════════════════════════════════════════════════
    
    string GetGestureIcon()
    {
        switch (currentGesture)
        {
            case ManoGestureContinuous.CLOSED_HAND_GESTURE: return "✊";
            case ManoGestureContinuous.OPEN_HAND_GESTURE: return "✋";
            case ManoGestureContinuous.OPEN_PINCH_GESTURE: return "👌";
            default: return "👋";
        }
    }

    Color GetGestureColor()
    {
        switch (currentGesture)
        {
            case ManoGestureContinuous.CLOSED_HAND_GESTURE: return new Color(1f, 0.6f, 0.3f);
            case ManoGestureContinuous.OPEN_HAND_GESTURE: return new Color(0.3f, 1f, 0.4f);
            case ManoGestureContinuous.OPEN_PINCH_GESTURE: return new Color(0.3f, 0.7f, 1f);
            default: return new Color(0.6f, 0.6f, 0.6f);
        }
    }

    GUIStyle GetPanelStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0.02f, 0.05f, 0.12f, 0.95f));
        return style;
    }

    GUIStyle GetStatusPanelStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0.03f, 0.08f, 0.15f, 0.9f));
        return style;
    }

    GUIStyle GetSidePanelStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0.02f, 0.06f, 0.12f, 0.92f));
        return style;
    }

    GUIStyle GetButtonStyle(Color baseColor)
    {
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;
        style.normal.background = MakeTex(2, 2, new Color(baseColor.r, baseColor.g, baseColor.b, 0.9f));
        style.hover.background = MakeTex(2, 2, new Color(baseColor.r * 1.2f, baseColor.g * 1.2f, baseColor.b * 1.2f, 1f));
        style.active.background = MakeTex(2, 2, new Color(baseColor.r * 0.8f, baseColor.g * 0.8f, baseColor.b * 0.8f, 1f));
        return style;
    }

    GUIStyle GetBarBorderStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0.4f, 0.7f, 1f, 0.6f));
        return style;
    }

    GUIStyle GetBarFillStyle(Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, color);
        return style;
    }

    Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
