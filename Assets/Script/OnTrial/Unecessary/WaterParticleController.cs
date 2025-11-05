using UnityEngine;

public class WaterParticleController : MonoBehaviour
{
    private ParticleSystem waterParticles;
    private ParticleSystem splashParticles;
    
    [Header("Water Settings")]
    [SerializeField] private Color waterColor = new Color(0.7f, 0.85f, 0.92f, 0.7f);
    [SerializeField] private float emissionRate = 80f;
    
    // Splash detection
    [Header("Splash Settings")]
    [SerializeField] private bool enableSplash = true;
    [SerializeField] private float splashCheckInterval = 0.2f;
    [SerializeField] private LayerMask splashLayers = -1; // Default to all layers
    
    private float lastSplashCheck;
    
    void Awake()
    {
        // Get references to particle systems
        waterParticles = GetComponent<ParticleSystem>();
        
        // Find splash particles (child object)
        Transform splashTransform = transform.Find("WaterSplash");
        if (splashTransform != null)
        {
            splashParticles = splashTransform.GetComponent<ParticleSystem>();
        }
        else if (enableSplash)
        {
            Debug.LogWarning("WaterSplash child not found. Splash effects will be disabled.");
        }
        
        // Initialize particle systems
        InitializeParticleSystem();
    }
    
    void InitializeParticleSystem()
    {
        if (waterParticles != null)
        {
            // Set main module properties
            var main = waterParticles.main;
            main.startColor = waterColor;
            
            // Set emission rate
            var emission = waterParticles.emission;
            emission.rateOverTime = emissionRate;
            
            // Ensure correct simulation space
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            // Apply similar settings to splash if available
            if (splashParticles != null)
            {
                var splashMain = splashParticles.main;
                splashMain.startColor = waterColor;
                splashMain.simulationSpace = ParticleSystemSimulationSpace.World;
            }
            
            // Stop particles initially
            waterParticles.Stop();
            if (splashParticles != null)
                splashParticles.Stop();
        }
    }
    
    void Update()
    {
        if (enableSplash && waterParticles.isPlaying && splashParticles != null)
        {
            CheckForSplash();
        }
    }
    
    void CheckForSplash()
    {
        // Only check periodically to save performance
        if (Time.time - lastSplashCheck < splashCheckInterval)
            return;
            
        lastSplashCheck = Time.time;
        
        // Cast a ray downward to detect surfaces
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f, splashLayers))
        {
            // Position the splash at the hit point
            splashParticles.transform.position = hit.point;
            splashParticles.transform.up = hit.normal;
            
            // Play the splash if it's not already playing
            if (!splashParticles.isPlaying)
            {
                splashParticles.Play();
            }
        }
    }
    
    // Public methods for external control
    
    public void Play()
    {
        if (waterParticles != null && !waterParticles.isPlaying)
        {
            waterParticles.Play();
        }
    }
    
    public void Stop()
    {
        if (waterParticles != null && waterParticles.isPlaying)
        {
            waterParticles.Stop();
        }
        
        if (splashParticles != null && splashParticles.isPlaying)
        {
            splashParticles.Stop();
        }
    }
    
    public void SetEmissionRate(float rate)
    {
        if (waterParticles != null)
        {
            var emission = waterParticles.emission;
            emission.rateOverTime = rate;
        }
    }
}