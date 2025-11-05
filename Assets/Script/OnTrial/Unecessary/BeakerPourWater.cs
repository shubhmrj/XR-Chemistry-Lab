// // using UnityEngine;
// // using ManoMotion;

// // public class BeakerPourWater : MonoBehaviour
// // {
// //     [SerializeField] private GameObject beakerModel;
// //     [SerializeField] private ParticleSystem waterEffect; // Particle system for water effect
// //     [SerializeField] private Transform pourPoint; // The point from which water will pour
    
// //     [Header("Pouring Settings")]
// //     [SerializeField] private float pouringThresholdAngle = 30f; // Degrees of tilt needed to start pouring
// //     [SerializeField] private float maxPourRate = 1.0f; // Maximum emission rate multiplier
    
// //     private float tiltSpeed = 30f;
// //     private float rotationSpeed = 50f;
// //     private float moveSpeed = 5f;
    
// //     // Keep track of liquid amount
// //     [SerializeField] [Range(0f, 1f)] private float liquidAmount = 1.0f; // 0 = empty, 1 = full

// //     void Start()
// //     {
// //         if (ManoMotionManager.Instance != null)
// //         {
// //             ManoMotionManager.Instance.ShouldCalculateGestures(true);
// //         }
        
// //         // Make sure the water effect is initially stopped
// //         if (waterEffect != null)
// //         {
// //             waterEffect.Stop();
// //         }
// //     }

// //     void Update()
// //     {
// //         if (ManoMotionManager.Instance == null || ManoMotionManager.Instance.HandInfos == null)
// //             return;

// //         HandInfo[] handInfos = ManoMotionManager.Instance.HandInfos;

// //         foreach (var handInfo in handInfos)
// //         {
// //             if (handInfo.gestureInfo.manoClass == ManoClass.NO_HAND)
// //                 continue;
            
// //             Vector3 handPosition = new Vector3(
// //                handInfo.trackingInfo.boundingBox.topLeft.x + handInfo.trackingInfo.boundingBox.width / 2,
// //                handInfo.trackingInfo.boundingBox.topLeft.y - handInfo.trackingInfo.boundingBox.height / 2,
// //                0
// //            );

// //             ManoGestureContinuous gesture = handInfo.gestureInfo.manoGestureContinuous;

// //             // Approximate palm center using the center of the bounding box
// //             BoundingBox boundingBox = handInfo.trackingInfo.boundingBox;

// //             float centerX = boundingBox.topLeft.x + boundingBox.width / 2f;
// //             float centerY = boundingBox.topLeft.y - boundingBox.height / 2f;

// //             // Normalize coordinates to range (-0.5, 0.5)
// //             float normalizedX = centerX - 0.5f;
// //             float normalizedY = 0.5f - centerY;

// //             switch (gesture)
// //             {
// //                 case ManoGestureContinuous.OPEN_HAND_GESTURE:
// //                     float tiltX = normalizedY * tiltSpeed * Time.deltaTime;
// //                     float tiltZ = -normalizedX * tiltSpeed * Time.deltaTime;

// //                     beakerModel.transform.Rotate(tiltX, 0, tiltZ);
// //                     break;

// //                 case ManoGestureContinuous.OPEN_PINCH_GESTURE:
// //                     // beakerModel.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
// //                     beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 2f);
// //                     break;

// //                 case ManoGestureContinuous.CLOSED_HAND_GESTURE:
// //                     beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 2f);
                    
// //                     Vector3 targetPosition = ManoUtils.Instance.CalculateNewPositionWithDepth(handPosition, 10);
// //                     beakerModel.transform.position = Vector3.Lerp(beakerModel.transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    
// //                     Handheld.Vibrate();
// //                     break;
// //             }
// //         }
        
// //         // Check if beaker is tilted enough for water to pour
// //         UpdateWaterPouring();
// //     }
    
// //     void UpdateWaterPouring()
// //     {
// //         if (waterEffect == null || liquidAmount <= 0)
// //         {
// //             // No water effect or no liquid left
// //             if (waterEffect != null && waterEffect.isPlaying)
// //                 waterEffect.Stop();
// //             return;
// //         }
        
// //         // Calculate the forward vector in world space
// //         Vector3 beakerUp = beakerModel.transform.up;
        
// //         // Angle between beaker's up vector and world up vector
// //         float tiltAngle = Vector3.Angle(beakerUp, Vector3.up);
        
// //         // Direction of tilt (to determine where water should pour from)
// //         Vector3 tiltDirection = Vector3.ProjectOnPlane(beakerUp, Vector3.up).normalized;
        
// //         if (tiltAngle > pouringThresholdAngle && liquidAmount > 0)
// //         {
// //             // Calculate pour rate based on tilt angle
// //             float pourRate = Mathf.Clamp01((tiltAngle - pouringThresholdAngle) / (90f - pouringThresholdAngle));
            
// //             // Reduce liquid amount based on tilt
// //             liquidAmount -= pourRate * Time.deltaTime * 0.1f;
// //             liquidAmount = Mathf.Max(0, liquidAmount);
            
// //             // If we have water effect and pour point, update them
// //             if (waterEffect != null && pourPoint != null)
// //             {
// //                 // Position water effect at pour point
// //                 waterEffect.transform.position = pourPoint.position;
                
// //                 // Orient the particle system in the direction of pour
// //                 waterEffect.transform.rotation = Quaternion.LookRotation(tiltDirection, Vector3.up);
                
// //                 // Adjust emission rate based on tilt and liquid amount
// //                 var emission = waterEffect.emission;
// //                 emission.rateOverTimeMultiplier = pourRate * maxPourRate * liquidAmount;
                
// //                 if (!waterEffect.isPlaying)
// //                     waterEffect.Play();
// //             }
// //         }
// //         else
// //         {
// //             // Not tilted enough to pour
// //             if (waterEffect != null && waterEffect.isPlaying)
// //                 waterEffect.Stop();
// //         }
// //     }
    
// //     // Method to refill the beaker if needed
// //     public void RefillBeaker()
// //     {
// //         liquidAmount = 1.0f;
// //     }
// // }


// using UnityEngine;
// using ManoMotion;

// public class BeakerPourWater : MonoBehaviour
// {
//     [SerializeField] private GameObject beakerModel;
    
//     // The pour point will be created automatically if not assigned
//     [SerializeField] private Transform pourPoint;
    
//     [Header("Pouring Settings")]
//     [SerializeField] private float pouringThresholdAngle = 30f;
//     [SerializeField] private float maxPourRate = 1.0f;
//     [SerializeField] private Color waterColor = new Color(0.7f, 0.85f, 0.92f, 0.7f);
    
//     private float tiltSpeed = 30f;
//     private float rotationSpeed = 50f;
//     private float moveSpeed = 5f;
    
//     // Keep track of liquid amount
//     [SerializeField] [Range(0f, 1f)] private float liquidAmount = 1.0f;
    
//     // Particle system reference (will be created automatically)
//     private ParticleSystem waterEffect;

//     void Start()
//     {
//         if (ManoMotionManager.Instance != null)
//         {
//             ManoMotionManager.Instance.ShouldCalculateGestures(true);
//         }
        
//         // Create pour point if not assigned
//         if (pourPoint == null)
//         {
//             GameObject pourPointObj = new GameObject("PourPoint");
//             pourPointObj.transform.parent = beakerModel.transform;
//             pourPointObj.transform.localPosition = new Vector3(0, 0.5f, 0.25f); // Adjust based on your beaker model
//             pourPoint = pourPointObj.transform;
//         }
        
//         // Create water particle system
//         CreateWaterParticleSystem();
//     }
    
//     private void CreateWaterParticleSystem()
//     {
//         // Create a new GameObject for the water effect
//         GameObject waterEffectObj = new GameObject("WaterEffect");
//         waterEffectObj.transform.parent = transform;
        
//         // Add particle system component
//         waterEffect = waterEffectObj.AddComponent<ParticleSystem>();
        
//         // Configure main module
//         var main = waterEffect.main;
//         main.duration = 5.0f;
//         main.loop = true;
//         main.startLifetime = 1.5f;
//         main.startSpeed = 3.0f;
//         main.startSize = 0.08f;
//         main.startColor = waterColor;
//         main.gravityModifier = 1.2f;
//         main.simulationSpace = ParticleSystemSimulationSpace.World;
//         main.playOnAwake = false;
//         main.maxParticles = 500;
        
//         // Configure emission
//         var emission = waterEffect.emission;
//         emission.enabled = true;
//         emission.rateOverTime = 80.0f;
        
//         // Configure shape
//         var shape = waterEffect.shape;
//         shape.enabled = true;
//         shape.shapeType = ParticleSystemShapeType.Cone;
//         shape.angle = 5.0f;
//         shape.radius = 0.05f;
//         shape.arc = 360.0f;
        
//         // Configure velocity over lifetime
//         var velocity = waterEffect.velocityOverLifetime;
//         velocity.enabled = true;
//         velocity.space = ParticleSystemSimulationSpace.Local;
        
//         // Create a min/max curve for velocity x
//         AnimationCurve constantZeroCurve = new AnimationCurve();
//         constantZeroCurve.AddKey(0.0f, 0.0f);
//         constantZeroCurve.AddKey(1.0f, 0.0f);
        
//         ParticleSystem.MinMaxCurve velocityXCurve = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
//         velocity.x = velocityXCurve;
//         velocity.y = new ParticleSystem.MinMaxCurve(0f);
//         velocity.z = new ParticleSystem.MinMaxCurve(0f, 0.5f);
        
//         // Configure size over lifetime
//         var sizeOverLifetime = waterEffect.sizeOverLifetime;
//         sizeOverLifetime.enabled = true;
//         AnimationCurve sizeCurve = new AnimationCurve();
//         sizeCurve.AddKey(0.0f, 1.0f);
//         sizeCurve.AddKey(1.0f, 0.2f);
//         sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);
        
//         // Configure color over lifetime
//         var colorOverLifetime = waterEffect.colorOverLifetime;
//         colorOverLifetime.enabled = true;
//         Gradient colorGradient = new Gradient();
//         colorGradient.SetKeys(
//             new GradientColorKey[] { new GradientColorKey(waterColor, 0.0f), new GradientColorKey(waterColor, 1.0f) },
//             new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
//         );
//         colorOverLifetime.color = colorGradient;
        
//         // Configure collision
//         var collision = waterEffect.collision;
//         collision.enabled = true;
//         collision.type = ParticleSystemCollisionType.World;
//         collision.mode = ParticleSystemCollisionMode.Collision3D;
//         collision.dampen = 0.2f;
//         collision.bounce = 0.3f;
//         collision.lifetimeLoss = 0.5f;
//         collision.minKillSpeed = 0.01f;
        
//         // Configure renderer
//         var renderer = waterEffect.GetComponent<ParticleSystemRenderer>();
//         renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
//         // Create a material for the particle system
//         Material waterMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
//         waterMaterial.SetColor("_Color", waterColor);
//         waterMaterial.SetFloat("_Glossiness", 0.9f);
//         waterMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
//         waterMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
//         waterMaterial.DisableKeyword("_ALPHATEST_ON");
//         waterMaterial.EnableKeyword("_ALPHABLEND_ON");
//         waterMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
//         waterMaterial.renderQueue = 3000;
//         renderer.material = waterMaterial;
        
//         // Stop the particle system initially
//         waterEffect.Stop();
//     }

//     void Update()
//     {
//         if (ManoMotionManager.Instance == null || ManoMotionManager.Instance.HandInfos == null)
//             return;

//         HandInfo[] handInfos = ManoMotionManager.Instance.HandInfos;

//         foreach (var handInfo in handInfos)
//         {
//             if (handInfo.gestureInfo.manoClass == ManoClass.NO_HAND)
//                 continue;
            
//             Vector3 handPosition = new Vector3(
//                handInfo.trackingInfo.boundingBox.topLeft.x + handInfo.trackingInfo.boundingBox.width / 2,
//                handInfo.trackingInfo.boundingBox.topLeft.y - handInfo.trackingInfo.boundingBox.height / 2,
//                0
//            );

//             ManoGestureContinuous gesture = handInfo.gestureInfo.manoGestureContinuous;

//             // Approximate palm center using the center of the bounding box
//             BoundingBox boundingBox = handInfo.trackingInfo.boundingBox;

//             float centerX = boundingBox.topLeft.x + boundingBox.width / 2f;
//             float centerY = boundingBox.topLeft.y - boundingBox.height / 2f;

//             // Normalize coordinates to range (-0.5, 0.5)
//             float normalizedX = centerX - 0.5f;
//             float normalizedY = 0.5f - centerY;

//             switch (gesture)
//             {
//                 case ManoGestureContinuous.OPEN_HAND_GESTURE:
//                     float tiltX = normalizedY * tiltSpeed * Time.deltaTime;
//                     float tiltZ = -normalizedX * tiltSpeed * Time.deltaTime;

//                     beakerModel.transform.Rotate(tiltX, 0, tiltZ);
//                     break;

//                 case ManoGestureContinuous.OPEN_PINCH_GESTURE:
//                     beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 2f);
//                     break;

//                 case ManoGestureContinuous.CLOSED_HAND_GESTURE:
//                     beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 2f);
                    
//                     Vector3 targetPosition = ManoUtils.Instance.CalculateNewPositionWithDepth(handPosition, 10);
//                     beakerModel.transform.position = Vector3.Lerp(beakerModel.transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    
//                     Handheld.Vibrate();
//                     break;
//             }
//         }
        
//         // Check if beaker is tilted enough for water to pour
//         UpdateWaterPouring();
//     }
    
//     void UpdateWaterPouring()
//     {
//         if (waterEffect == null || liquidAmount <= 0)
//         {
//             // No water effect or no liquid left
//             if (waterEffect != null && waterEffect.isPlaying)
//                 waterEffect.Stop();
//             return;
//         }
        
//         // Calculate the forward vector in world space
//         Vector3 beakerUp = beakerModel.transform.up;
        
//         // Angle between beaker's up vector and world up vector
//         float tiltAngle = Vector3.Angle(beakerUp, Vector3.up);
        
//         // Direction of tilt (to determine where water should pour from)
//         Vector3 tiltDirection = Vector3.ProjectOnPlane(beakerUp, Vector3.up).normalized;
        
//         if (tiltAngle > pouringThresholdAngle && liquidAmount > 0)
//         {
//             // Calculate pour rate based on tilt angle
//             float pourRate = Mathf.Clamp01((tiltAngle - pouringThresholdAngle) / (90f - pouringThresholdAngle));
            
//             // Reduce liquid amount based on tilt
//             liquidAmount -= pourRate * Time.deltaTime * 0.1f;
//             liquidAmount = Mathf.Max(0, liquidAmount);
            
//             // Position water effect at pour point
//             waterEffect.transform.position = pourPoint.position;
            
//             // Orient the particle system in the direction of pour
//             waterEffect.transform.rotation = Quaternion.LookRotation(tiltDirection, Vector3.up);
            
//             // Adjust emission rate based on tilt and liquid amount
//             var emission = waterEffect.emission;
//             emission.rateOverTimeMultiplier = pourRate * maxPourRate * liquidAmount;
            
//             if (!waterEffect.isPlaying)
//                 waterEffect.Play();
//         }
//         else
//         {
//             // Not tilted enough to pour
//             if (waterEffect.isPlaying)
//                 waterEffect.Stop();
//         }
//     }
    
//     // Method to refill the beaker if needed
//     public void RefillBeaker()
//     {
//         liquidAmount = 1.0f;
//     }
// }

// using UnityEngine;
// using ManoMotion;

// public class WaterAttachToBeaker : MonoBehaviour
// {
//     [SerializeField] private GameObject beakerModel;
//     [SerializeField] private Transform pourPoint;
//     [SerializeField] private GameObject waterParticlesPrefab;
    
//     [Header("Pouring Settings")]
//     [SerializeField] private float pouringThresholdAngle = 30f;
//     [SerializeField] private float maxPourRate = 1.0f;
//     [SerializeField] private Color waterColor = new Color(0.7f, 0.85f, 0.92f, 0.7f);
    
//     private float tiltSpeed = 30f;
//     private float rotationSpeed = 50f;
//     private float moveSpeed = 5f;
    
//     // Keep track of liquid amount
//     [SerializeField] [Range(0f, 1f)] private float liquidAmount = 1.0f;
    
//     // Water effect references
//     private GameObject waterEffectObj;
//     private ParticleSystem waterEffect;
//     private ParticleSystem splashEffect;
    
//     // Debug visualization
//     [SerializeField] private bool showDebugVisuals = false;
//     private GameObject debugSphere;

//     void Start()
//     {
//         if (ManoMotionManager.Instance != null)
//         {
//             ManoMotionManager.Instance.ShouldCalculateGestures(true);
//         }
        
//         // Create pour point if not assigned
//         if (pourPoint == null)
//         {
//             GameObject pourPointObj = new GameObject("PourPoint");
//             pourPointObj.transform.parent = beakerModel.transform;
            
//             // Adjust based on your beaker model dimensions - position at the lip/edge
//             // These values need to be adjusted for your specific beaker model
//             pourPointObj.transform.localPosition = new Vector3(0, 0.45f, 0.25f); 
//             pourPoint = pourPointObj.transform;
//         }
        
//         // Create debug visuals if needed
//         if (showDebugVisuals)
//         {
//             debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//             debugSphere.transform.localScale = Vector3.one * 0.05f;
//             debugSphere.GetComponent<Renderer>().material.color = Color.red;
//             Destroy(debugSphere.GetComponent<Collider>());
//         }
        
//         // Instantiate the water particles prefab
//         if (waterParticlesPrefab != null)
//         {
//             // Instantiate and parent the water particle effect to the pour point so that it
//             // always spawns from the beaker lip and follows the beaker while still
//             // simulating in world space.
//             waterEffectObj = Instantiate(waterParticlesPrefab);
//             waterEffectObj.transform.SetParent(pourPoint);
//             waterEffectObj.transform.localPosition = Vector3.zero;
//             waterEffectObj.transform.localRotation = Quaternion.identity;
            
//             // Get references to particle systems
//             waterEffect = waterEffectObj.GetComponent<ParticleSystem>();
//             if (waterEffect == null)
//             {
//                 Debug.LogError("Particle system component not found on water particles prefab!");
//             }
//             else
//             {
//                 // Ensure particles use world simulation space
//                 var main = waterEffect.main;
//                 // Use LOCAL simulation so newly emitted particles follow the pour point immediately.
//                 main.simulationSpace = ParticleSystemSimulationSpace.Local;
                
//                 // Set the water color
//                 main.startColor = waterColor;
//             }
            
//             // Find splash particles if they exist
//             Transform splashTransform = waterEffectObj.transform.Find("WaterSplash");
//             if (splashTransform != null)
//             {
//                 splashEffect = splashTransform.GetComponent<ParticleSystem>();
//                 if (splashEffect != null)
//                 {
//                     var splashMain = splashEffect.main;
//                     splashMain.simulationSpace = ParticleSystemSimulationSpace.Local;
//                     splashMain.startColor = waterColor;
//                 }
//             }
            
//             // Stop the particle systems initially
//             if (waterEffect != null) waterEffect.Stop();
//             if (splashEffect != null) splashEffect.Stop();
//         }
//         else
//         {
//             Debug.LogError("Water particles prefab not assigned!");
//         }
//     }

//     void Update()
//     {
//         if (ManoMotionManager.Instance == null || ManoMotionManager.Instance.HandInfos == null)
//             return;

//         HandInfo[] handInfos = ManoMotionManager.Instance.HandInfos;

//         foreach (var handInfo in handInfos)
//         {
//             if (handInfo.gestureInfo.manoClass == ManoClass.NO_HAND)
//                 continue;
            
//             Vector3 handPosition = new Vector3(
//                handInfo.trackingInfo.boundingBox.topLeft.x + handInfo.trackingInfo.boundingBox.width / 2,
//                handInfo.trackingInfo.boundingBox.topLeft.y - handInfo.trackingInfo.boundingBox.height / 2,
//                0
//            );

//             ManoGestureContinuous gesture = handInfo.gestureInfo.manoGestureContinuous;

//             // Approximate palm center using the center of the bounding box
//             BoundingBox boundingBox = handInfo.trackingInfo.boundingBox;

//             float centerX = boundingBox.topLeft.x + boundingBox.width / 2f;
//             float centerY = boundingBox.topLeft.y - boundingBox.height / 2f;

//             // Normalize coordinates to range (-0.5, 0.5)
//             float normalizedX = centerX - 0.5f;
//             float normalizedY = 0.5f - centerY;

//             switch (gesture)
//             {
//                 case ManoGestureContinuous.OPEN_HAND_GESTURE:
//                     float tiltX = normalizedY * tiltSpeed * Time.deltaTime;
//                     float tiltZ = -normalizedX * tiltSpeed * Time.deltaTime;

//                     beakerModel.transform.Rotate(tiltX, 0, tiltZ);
//                     break;

//                 case ManoGestureContinuous.OPEN_PINCH_GESTURE:
//                     beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 2f);
//                     break;

//                 case ManoGestureContinuous.CLOSED_HAND_GESTURE:
//                     beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 2f);
                    
//                     Vector3 targetPosition = ManoUtils.Instance.CalculateNewPositionWithDepth(handPosition, 10);
//                     beakerModel.transform.position = Vector3.Lerp(beakerModel.transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    
//                     Handheld.Vibrate();
//                     break;
//             }
//         }
        
//         // Update debug visuals
//         if (showDebugVisuals && debugSphere != null && pourPoint != null)
//         {
//             debugSphere.transform.position = pourPoint.position;
//         }
        
//         // Check if beaker is tilted enough for water to pour
//         UpdateWaterPouring();
//     }
    
//     void UpdateWaterPouring()
//     {
//         if (waterEffect == null || liquidAmount <= 0)
//         {
//             // No water effect or no liquid left
//             if (waterEffect != null && waterEffect.isPlaying)
//                 waterEffect.Stop();
//             return;
//         }
        
//         // Calculate the up vector of the beaker in world space
//         Vector3 beakerUp = beakerModel.transform.up;
        
//         // Angle between beaker's up vector and world up vector
//         float tiltAngle = Vector3.Angle(beakerUp, Vector3.up);
        
//         // Direction of tilt (to determine where water should pour from)
//         Vector3 tiltDirection = Vector3.zero;
//         if (tiltAngle > 1f) // Avoid normalizing zero vector
//         {
//             tiltDirection = Vector3.ProjectOnPlane(beakerUp, Vector3.up).normalized;
//         }
//         else
//         {
//             tiltDirection = Vector3.forward; // Default direction if not tilted
//         }
        
//         if (tiltAngle > pouringThresholdAngle && liquidAmount > 0)
//         {
//             // Calculate pour rate based on tilt angle
//             float pourRate = Mathf.Clamp01((tiltAngle - pouringThresholdAngle) / (90f - pouringThresholdAngle));
            
//             // Reduce liquid amount based on tilt
//             liquidAmount -= pourRate * Time.deltaTime * 0.1f;
//             liquidAmount = Mathf.Max(0, liquidAmount);
            
//             // IMPORTANT: Explicitly position the water effect at the pour point position
//             waterEffectObj.transform.position = pourPoint.position;
            
//             // Orient the water particles in the direction of pour
//             waterEffectObj.transform.rotation = Quaternion.LookRotation(tiltDirection, Vector3.up);
            
//             // Log positions for debugging
//             Debug.Log($"Beaker: {beakerModel.transform.position}, Pour Point: {pourPoint.position}, Water: {waterEffectObj.transform.position}");
            
//             // Adjust emission rate based on tilt and liquid amount
//             var emission = waterEffect.emission;
//             emission.rateOverTimeMultiplier = pourRate * maxPourRate * liquidAmount;
            
//             if (!waterEffect.isPlaying)
//             {
//                 // Clear to remove any stray particles first so emission starts exactly from current position.
//                 waterEffect.Clear(true);
//                 waterEffect.Play();
//             }
                
//             // Handle splash effects
//             if (splashEffect != null)
//             {
//                 // Raycast to find where water would hit
//                 RaycastHit hit;
//                 if (Physics.Raycast(pourPoint.position, Vector3.down, out hit, 10f))
//                 {
//                     splashEffect.transform.position = hit.point;
//                     splashEffect.transform.up = hit.normal;
                    
//                     if (!splashEffect.isPlaying)
//                         {
//                             splashEffect.Clear(true);
//                             splashEffect.Play();
//                         }
//                 }
//                 else if (splashEffect.isPlaying)
//                 {
//                     splashEffect.Stop();
//                 }
//             }
//         }
//         else
//         {
//             // Not tilted enough to pour
//             if (waterEffect.isPlaying)
//                 waterEffect.Stop();
                
//             if (splashEffect != null && splashEffect.isPlaying)
//                 splashEffect.Stop();
//         }
//     }
    
//     // Method to refill the beaker if needed
//     public void RefillBeaker()
//     {
//         liquidAmount = 1.0f;
//     }
// }

// using UnityEngine;
// using ManoMotion;

// public class WaterAttachToBeaker : MonoBehaviour
// {
//     [SerializeField] private GameObject beakerModel;
//     [SerializeField] private Transform pourPoint;
//     [SerializeField] private GameObject waterParticlesPrefab;
    
//     [Header("Pouring Settings")]
//     [SerializeField] private float pouringThresholdAngle = 30f;
//     [SerializeField] private float maxPourRate = 1.0f;
//     [SerializeField] private Color waterColor = new Color(0.7f, 0.85f, 0.92f, 0.7f);
    
//     [SerializeField] private float maxTiltAngle = 60f;   // maximum tilt based on hand offset
//     [SerializeField] private float tiltSmoothSpeed = 5f; // higher = faster response; lower = smoother
//     private float moveSpeed = 5f;
//     private Quaternion baseRotation;
    
//     // Keep track of liquid amount
//     [SerializeField] [Range(0f, 1f)] private float liquidAmount = 1.0f;
    
//     // Water effect references
//     private GameObject waterEffectObj;
//     private ParticleSystem waterEffect;
//     private ParticleSystem splashEffect;
    
//     // Debug visualization
//     [SerializeField] private bool showDebugVisuals = false;
//     private GameObject debugSphere;

//     void Start()
//     {
//         if (ManoMotionManager.Instance != null)
//         {
//             ManoMotionManager.Instance.ShouldCalculateGestures(true);
//         }
        
//         // Create pour point if not assigned
//         if (pourPoint == null)
//         {
//             GameObject pourPointObj = new GameObject("PourPoint");
//             pourPointObj.transform.parent = beakerModel.transform;
            
//             // Adjust based on your beaker model dimensions - position at the lip/edge
//             // These values need to be adjusted for your specific beaker model
//             pourPointObj.transform.localPosition = new Vector3(0, 0.45f, 0.25f); 
//             pourPoint = pourPointObj.transform;
//         }
        
//         // Create debug visuals if needed
//         if (showDebugVisuals)
//         {
//             debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//             debugSphere.transform.localScale = Vector3.one * 0.05f;
//             debugSphere.GetComponent<Renderer>().material.color = Color.red;
//             Destroy(debugSphere.GetComponent<Collider>());
//         }
        
//         // Store the beaker's initial upright rotation for tilt calculations
//         baseRotation = beakerModel.transform.rotation;

//         // Instantiate the water particles prefab
//         if (waterParticlesPrefab != null)
//         {
//             // Instantiate and parent the water particle effect to the pour point so that it
//             // always spawns from the beaker lip and follows the beaker while still
//             // simulating in world space.
//             waterEffectObj = Instantiate(waterParticlesPrefab);
//             waterEffectObj.transform.SetParent(pourPoint);
//             waterEffectObj.transform.localPosition = Vector3.zero;
//             waterEffectObj.transform.localRotation = Quaternion.identity;
            
//             // Get references to particle systems
//             waterEffect = waterEffectObj.GetComponent<ParticleSystem>();
//             if (waterEffect == null)
//             {
//                 Debug.LogError("Particle system component not found on water particles prefab!");
//             }
//             else
//             {
//                 // Use WORLD simulation so once particles are emitted they fall independently of the beaker.
//                 var main = waterEffect.main;
//                 main.simulationSpace = ParticleSystemSimulationSpace.World;
                
//                 // Set the water color
//                 main.startColor = waterColor;
//             }
            
//             // Find splash particles if they exist
//             Transform splashTransform = waterEffectObj.transform.Find("WaterSplash");
//             if (splashTransform != null)
//             {
//                 splashEffect = splashTransform.GetComponent<ParticleSystem>();
//                 if (splashEffect != null)
//                 {
//                     var splashMain = splashEffect.main;
//                     splashMain.simulationSpace = ParticleSystemSimulationSpace.World;
//                     splashMain.startColor = waterColor;
//                 }
//             }
            
//             // Stop the particle systems initially
//             if (waterEffect != null) waterEffect.Stop();
//             if (splashEffect != null) splashEffect.Stop();
//         }
//         else
//         {
//             Debug.LogError("Water particles prefab not assigned!");
//         }
//     }

//     void Update()
//     {
//         if (ManoMotionManager.Instance == null || ManoMotionManager.Instance.HandInfos == null)
//             return;

//         HandInfo[] handInfos = ManoMotionManager.Instance.HandInfos;

//         foreach (var handInfo in handInfos)
//         {
//             if (handInfo.gestureInfo.manoClass == ManoClass.NO_HAND)
//                 continue;
            
//             Vector3 handPosition = new Vector3(
//                handInfo.trackingInfo.boundingBox.topLeft.x + handInfo.trackingInfo.boundingBox.width / 2,
//                handInfo.trackingInfo.boundingBox.topLeft.y - handInfo.trackingInfo.boundingBox.height / 2,
//                0
//            );

//             ManoGestureContinuous gesture = handInfo.gestureInfo.manoGestureContinuous;

//             // Approximate palm center using the center of the bounding box
//             BoundingBox boundingBox = handInfo.trackingInfo.boundingBox;

//             float centerX = boundingBox.topLeft.x + boundingBox.width / 2f;
//             float centerY = boundingBox.topLeft.y - boundingBox.height / 2f;

//             // Normalize coordinates to range (-0.5, 0.5)
//             float normalizedX = centerX - 0.5f;
//             float normalizedY = 0.5f - centerY;

//             switch (gesture)
//             {
//                 case ManoGestureContinuous.OPEN_HAND_GESTURE:
//                     // Calculate desired tilt angle (only left direction)
//                     float desiredTiltZ = -normalizedX * maxTiltAngle;
//                     desiredTiltZ = Mathf.Clamp(desiredTiltZ, -maxTiltAngle, 0f);
                    
//                     // Calculate current tilt angle around Z-axis
//                     Vector3 currentEuler = beakerModel.transform.eulerAngles;
//                     float currentTiltZ = currentEuler.z;
//                     // Normalize angle to -180 to 180 range
//                     if (currentTiltZ > 180f) currentTiltZ -= 360f;
                    
//                     // Calculate the angle difference we need to rotate
//                     float angleDiff = desiredTiltZ - currentTiltZ;
                    
//                     // Apply smooth rotation around the pour point
//                     if (Mathf.Abs(angleDiff) > 0.1f)
//                     {
//                         float rotateAmount = angleDiff * Time.deltaTime * tiltSmoothSpeed;
//                         // Use pour point as pivot, rotate around forward axis (Z-axis in world space)
//                         beakerModel.transform.RotateAround(pourPoint.position, Vector3.forward, rotateAmount);
//                     }
//                     break;

//                 case ManoGestureContinuous.OPEN_PINCH_GESTURE:
//                     // Reset rotation smoothly around pour point
//                     Vector3 currentEulerReset = beakerModel.transform.eulerAngles;
//                     float currentTiltZReset = currentEulerReset.z;
//                     if (currentTiltZReset > 180f) currentTiltZReset -= 360f;
                    
//                     if (Mathf.Abs(currentTiltZReset) > 0.1f)
//                     {
//                         float resetAmount = -currentTiltZReset * Time.deltaTime * 2f;
//                         beakerModel.transform.RotateAround(pourPoint.position, Vector3.forward, resetAmount);
//                     }
//                     break;

//                 case ManoGestureContinuous.CLOSED_HAND_GESTURE:
//                     // Reset rotation while moving
//                     Vector3 currentEulerMove = beakerModel.transform.eulerAngles;
//                     float currentTiltZMove = currentEulerMove.z;
//                     if (currentTiltZMove > 180f) currentTiltZMove -= 360f;
                    
//                     if (Mathf.Abs(currentTiltZMove) > 0.1f)
//                     {
//                         float resetAmount = -currentTiltZMove * Time.deltaTime * 2f;
//                         beakerModel.transform.RotateAround(pourPoint.position, Vector3.forward, resetAmount);
//                     }
                    
//                     Vector3 targetPosition = ManoUtils.Instance.CalculateNewPositionWithDepth(handPosition, 10);
//                     beakerModel.transform.position = Vector3.Lerp(beakerModel.transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    
//                     Handheld.Vibrate();
//                     break;
//             }
//         }
        
//         // Update debug visuals
//         if (showDebugVisuals && debugSphere != null && pourPoint != null)
//         {
//             debugSphere.transform.position = pourPoint.position;
//         }
        
//         // Check if beaker is tilted enough for water to pour
//         UpdateWaterPouring();
//     }
    
//     void UpdateWaterPouring()
//     {
//         if (waterEffect == null || liquidAmount <= 0)
//         {
//             // No water effect or no liquid left
//             if (waterEffect != null && waterEffect.isPlaying)
//                 waterEffect.Stop();
//             return;
//         }
        
//         // Calculate the up vector of the beaker in world space
//         Vector3 beakerUp = beakerModel.transform.up;
        
//         // Angle between beaker's up vector and world up vector
//         float tiltAngle = Vector3.Angle(beakerUp, Vector3.up);
        
//         // Direction of tilt (to determine where water should pour from)
//         Vector3 tiltDirection = Vector3.zero;
//         if (tiltAngle > 1f) // Avoid normalizing zero vector
//         {
//             tiltDirection = Vector3.ProjectOnPlane(beakerUp, Vector3.up).normalized;
//         }
//         else
//         {
//             tiltDirection = Vector3.forward; // Default direction if not tilted
//         }
        
//         if (tiltAngle > pouringThresholdAngle && liquidAmount > 0)
//         {
//             // Calculate pour rate based on tilt angle
//             float pourRate = Mathf.Clamp01((tiltAngle - pouringThresholdAngle) / (90f - pouringThresholdAngle));
            
//             // Reduce liquid amount based on tilt
//             liquidAmount -= pourRate * Time.deltaTime * 0.1f;
//             liquidAmount = Mathf.Max(0, liquidAmount);
            
//             // IMPORTANT: Explicitly position the water effect at the pour point position
//             waterEffectObj.transform.position = pourPoint.position;
            
//             // Orient the water particles in the direction of pour
//             waterEffectObj.transform.rotation = Quaternion.LookRotation(tiltDirection, Vector3.up);
            
//             // Log positions for debugging
//             Debug.Log($"Beaker: {beakerModel.transform.position}, Pour Point: {pourPoint.position}, Water: {waterEffectObj.transform.position}");
            
//             // Adjust emission rate based on tilt and liquid amount
//             var emission = waterEffect.emission;
//             emission.rateOverTimeMultiplier = pourRate * maxPourRate * liquidAmount;
            
//             if (!waterEffect.isPlaying)
//             {
//                 // Clear to remove any stray particles first so emission starts exactly from current position.
//                 waterEffect.Clear(true);
//                 waterEffect.Play();
//             }
                
//             // Handle splash effects
//             if (splashEffect != null)
//             {
//                 // Raycast to find where water would hit
//                 RaycastHit hit;
//                 if (Physics.Raycast(pourPoint.position, Vector3.down, out hit, 10f))
//                 {
//                     splashEffect.transform.position = hit.point;
//                     splashEffect.transform.up = hit.normal;
                    
//                     if (!splashEffect.isPlaying)
//                         {
//                             splashEffect.Clear(true);
//                             splashEffect.Play();
//                         }
//                 }
//                 else if (splashEffect.isPlaying)
//                 {
//                     splashEffect.Stop();
//                 }
//             }
//         }
//         else
//         {
//             // Not tilted enough to pour
//             if (waterEffect.isPlaying)
//                 waterEffect.Stop();
                
//             if (splashEffect != null && splashEffect.isPlaying)
//                 splashEffect.Stop();
//         }
//     }
    
//     // Method to refill the beaker if needed
//     public void RefillBeaker()
//     {
//         liquidAmount = 1.0f;
//     }
// }