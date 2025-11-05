using UnityEngine;
using System.Collections.Generic;

public class WaterFlowVFX : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem streamParticles;
    [SerializeField] private ParticleSystem splashParticles;
    [SerializeField] private ParticleSystem mistParticles;
    [SerializeField] private ParticleSystem dripsParticles;
    
    [Header("Water Properties")]
    [SerializeField] private Color waterColor = new Color(0.7f, 0.85f, 0.92f, 0.7f);
    [SerializeField] private Gradient waterGradient;
    [SerializeField] private float viscosity = 1.0f; // Higher values = thicker liquid
    [SerializeField] private float surfaceTension = 0.7f; // Higher values = more cohesive streams
    
    [Header("Flow Settings")]
    [SerializeField] private float maxFlowRate = 100f;
    [SerializeField] private AnimationCurve flowCurve;
    [SerializeField] private float minTiltToFlow = 15f;
    [SerializeField] private float maxTiltToFlow = 90f;
    
    [Header("Physics")]
    [SerializeField] private LayerMask collisionLayers = -1;
    [SerializeField] private float maxRaycastDistance = 10f;
    [SerializeField] private GameObject waterRipplePrefab;

    // Internal flow state
    private float currentFlowRate = 0f;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    private Vector3 lastSplashPosition;
    private float timeSinceLastRipple = 0f;
    
    private void Start()
    {
        // Ensure proper initialization
        InitializeParticleSystems();
        
        // Start with everything turned off
        SetParticleSystemsActive(false);
    }
    
    private void InitializeParticleSystems()
    {
        // Main stream particles
        if (streamParticles != null)
        {
            var main = streamParticles.main;
            main.startColor = waterColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            // Adjust emission shape based on viscosity
            var shape = streamParticles.shape;
            shape.angle = Mathf.Lerp(3f, 7f, 1f - viscosity/2f);
            shape.radius = Mathf.Lerp(0.03f, 0.08f, 1f - viscosity/2f);
            
            // Adjust size over lifetime
            var sizeOverLifetime = streamParticles.sizeOverLifetime;
            if (sizeOverLifetime.enabled)
            {
                AnimationCurve sizeCurve = new AnimationCurve();
                sizeCurve.AddKey(0.0f, 1.0f);
                
                // Higher surface tension liquids maintain size better
                float endSize = Mathf.Lerp(0.2f, 0.7f, surfaceTension);
                sizeCurve.AddKey(1.0f, endSize);
                
                sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);
            }
            
            // Adjust velocity based on viscosity
            var velocityOverLifetime = streamParticles.velocityOverLifetime;
            if (velocityOverLifetime.enabled)
            {
                // More viscous fluids have less random movement
                float randomFactor = Mathf.Lerp(0.5f, 0.05f, viscosity);
                
                ParticleSystem.MinMaxCurve xCurve = new ParticleSystem.MinMaxCurve(-randomFactor, randomFactor);
                xCurve.mode = ParticleSystemCurveMode.TwoConstants;
                
                ParticleSystem.MinMaxCurve yCurve = new ParticleSystem.MinMaxCurve(0f, 0f);
                yCurve.mode = ParticleSystemCurveMode.TwoConstants;
                
                ParticleSystem.MinMaxCurve zCurve = new ParticleSystem.MinMaxCurve(-randomFactor, randomFactor);
                zCurve.mode = ParticleSystemCurveMode.TwoConstants;
                
                velocityOverLifetime.x = xCurve;
                velocityOverLifetime.y = yCurve;
                velocityOverLifetime.z = zCurve;
            }
        }
        
        // Initialize other particle systems similarly
        InitializeSecondaryParticleSystems();
    }
    
    private void InitializeSecondaryParticleSystems()
    {
        // Splash particles
        if (splashParticles != null)
        {
            var main = splashParticles.main;
            main.startColor = waterColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
        }
        
        // Mist particles
        if (mistParticles != null)
        {
            var main = mistParticles.main;
            Color mistColor = new Color(waterColor.r, waterColor.g, waterColor.b, 0.3f);
            main.startColor = mistColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
        }
        
        // Drip particles
        if (dripsParticles != null)
        {
            var main = dripsParticles.main;
            main.startColor = waterColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
        }
    }
    
    private void Update()
    {
        // Check if we should create water ripples
        if (streamParticles.isPlaying && waterRipplePrefab != null)
        {
            CheckForWaterSurface();
        }
        
        // Update drips (for slow/stopping flow)
        if (currentFlowRate > 0 && currentFlowRate < 10f && dripsParticles != null)
        {
            if (!dripsParticles.isPlaying)
            {
                dripsParticles.Play();
            }
        }
        else if (dripsParticles != null && dripsParticles.isPlaying && currentFlowRate >= 10f)
        {
            dripsParticles.Stop();
        }
    }

    public void UpdateFlow(float tiltAngle, float liquidAmount)
    {
        // Don't flow if we're below minimum tilt or out of liquid
        if (tiltAngle < minTiltToFlow || liquidAmount <= 0)
        {
            StopFlow();
            return;
        }
        
        // Calculate normalized tilt (0-1)
        float normalizedTilt = Mathf.Clamp01((tiltAngle - minTiltToFlow) / (maxTiltToFlow - minTiltToFlow));
        
        // Apply flow curve for more natural pouring
        float flowMultiplier = flowCurve.Evaluate(normalizedTilt);
        
        // Scale by liquid amount
        currentFlowRate = maxFlowRate * flowMultiplier * liquidAmount;
        
        // Apply to all particle systems
        SetFlow(currentFlowRate);
    }
    
    private void SetFlow(float flowRate)
    {
        if (flowRate <= 0)
        {
            StopFlow();
            return;
        }

        // Start particles if not already playing
        SetParticleSystemsActive(true);
        
        // Update emission rates based on flow
        if (streamParticles != null)
        {
            var emission = streamParticles.emission;
            emission.rateOverTime = flowRate;
        }
        
        if (mistParticles != null)
        {
            var emission = mistParticles.emission;
            // More mist with higher flow rates
            emission.rateOverTime = flowRate * 0.3f;
        }
    }
    
    private void StopFlow()
    {
        currentFlowRate = 0;
        SetParticleSystemsActive(false);
    }
    
    private void SetParticleSystemsActive(bool active)
    {
        // Activate/deactivate main flow
        if (streamParticles != null)
        {
            if (active && !streamParticles.isPlaying)
                streamParticles.Play();
            else if (!active && streamParticles.isPlaying)
                streamParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        
        // Handle secondary effects
        if (mistParticles != null)
        {
            if (active && !mistParticles.isPlaying)
                mistParticles.Play();
            else if (!active && mistParticles.isPlaying)
                mistParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
    
    private void CheckForWaterSurface()
    {
        // Only check periodically
        timeSinceLastRipple += Time.deltaTime;
        if (timeSinceLastRipple < 0.2f) return;
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, maxRaycastDistance, collisionLayers))
        {
            // Create water ripple effect at hit point
            if (Vector3.Distance(hit.point, lastSplashPosition) > 0.1f || timeSinceLastRipple > 1.0f)
            {
                Instantiate(waterRipplePrefab, hit.point + (hit.normal * 0.01f), Quaternion.LookRotation(hit.normal));
                lastSplashPosition = hit.point;
                timeSinceLastRipple = 0;
                
                // Play splash if it's available
                if (splashParticles != null && !splashParticles.isPlaying)
                {
                    splashParticles.transform.position = hit.point;
                    splashParticles.transform.rotation = Quaternion.LookRotation(hit.normal);
                    splashParticles.Play();
                }
            }
        }
    }
    
    private void OnParticleCollision(GameObject other)
    {
        // Handle direct particle collisions
        if (splashParticles != null && streamParticles != null)
        {
            int numCollisionEvents = streamParticles.GetCollisionEvents(other, collisionEvents);
            
            if (numCollisionEvents > 0)
            {
                // Position splash at collision point
                Vector3 collisionPoint = collisionEvents[0].intersection;
                splashParticles.transform.position = collisionPoint;
                splashParticles.transform.rotation = Quaternion.LookRotation(collisionEvents[0].normal);
                
                if (!splashParticles.isPlaying)
                    splashParticles.Play();
            }
        }
    }
}