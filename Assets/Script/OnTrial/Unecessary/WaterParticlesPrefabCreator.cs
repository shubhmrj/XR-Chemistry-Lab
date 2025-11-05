using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WaterParticlesPrefabCreator : MonoBehaviour
{
    public Color waterColor = new Color(0.7f, 0.85f, 0.92f, 0.7f);
    public float particleSize = 0.08f;
    public float particleSpeed = 3.0f;
    public float emissionRate = 80.0f;
    
    #if UNITY_EDITOR
    [ContextMenu("Create Water Particles Prefab")]
    void CreateWaterParticlesPrefab()
    {
        // Create a new GameObject for the water effect
        GameObject waterEffectObj = new GameObject("WaterEffect");
        
        // Add particle system component
        ParticleSystem waterEffect = waterEffectObj.AddComponent<ParticleSystem>();
        
        // Configure main module
        var main = waterEffect.main;
        main.duration = 5.0f;
        main.loop = true;
        main.startLifetime = 1.5f;
        main.startSpeed = particleSpeed;
        main.startSize = particleSize;
        main.startColor = waterColor;
        main.gravityModifier = 1.2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.maxParticles = 500;
        
        // Configure emission
        var emission = waterEffect.emission;
        emission.enabled = true;
        emission.rateOverTime = emissionRate;
        
        // Configure shape
        var shape = waterEffect.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 5.0f;
        shape.radius = 0.05f;
        shape.arc = 360.0f;
        
        // Configure velocity over lifetime
        var velocity = waterEffect.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        
        // Create a min/max curve for velocity x
        AnimationCurve constantZeroCurve = new AnimationCurve();
        constantZeroCurve.AddKey(0.0f, 0.0f);
        constantZeroCurve.AddKey(1.0f, 0.0f);
        
        ParticleSystem.MinMaxCurve velocityXCurve = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
        velocity.x = velocityXCurve;
        velocity.y = new ParticleSystem.MinMaxCurve(0f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0.5f);
        
        // Configure size over lifetime
        var sizeOverLifetime = waterEffect.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0.0f, 1.0f);
        sizeCurve.AddKey(1.0f, 0.2f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);
        
        // Configure color over lifetime
        var colorOverLifetime = waterEffect.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(waterColor, 0.0f), new GradientColorKey(waterColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = colorGradient;
        
        // Configure collision
        var collision = waterEffect.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.dampen = 0.2f;
        collision.bounce = 0.3f;
        collision.lifetimeLoss = 0.5f;
        collision.minKillSpeed = 0.01f;
        
        // Configure renderer
        var renderer = waterEffect.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        // Create a material for the particle system
        Material waterMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
        waterMaterial.SetColor("_Color", waterColor);
        waterMaterial.SetFloat("_Glossiness", 0.9f);
        waterMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        waterMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        waterMaterial.DisableKeyword("_ALPHATEST_ON");
        waterMaterial.EnableKeyword("_ALPHABLEND_ON");
        waterMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        waterMaterial.renderQueue = 3000;
        renderer.material = waterMaterial;
        
        // Create Water Splash Effect (child)
        GameObject splashObj = new GameObject("WaterSplash");
        splashObj.transform.parent = waterEffectObj.transform;
        ParticleSystem splashEffect = splashObj.AddComponent<ParticleSystem>();
        
        // Configure splash main module
        var splashMain = splashEffect.main;
        splashMain.duration = 0.5f;
        splashMain.loop = false;
        splashMain.startLifetime = 0.7f;
        splashMain.startSpeed = 2.0f;
        splashMain.startSize = 0.05f;
        splashMain.startColor = waterColor;
        splashMain.gravityModifier = 1.0f;
        splashMain.simulationSpace = ParticleSystemSimulationSpace.World;
        splashMain.playOnAwake = false;
        splashMain.maxParticles = 30;
        
        // Configure splash emission
        var splashEmission = splashEffect.emission;
        splashEmission.enabled = true;
        splashEmission.rateOverTime = 0;
        var burst = new ParticleSystem.Burst(0.0f, 20);
        splashEmission.SetBurst(0, burst);
        
        // Configure splash shape
        var splashShape = splashEffect.shape;
        splashShape.enabled = true;
        splashShape.shapeType = ParticleSystemShapeType.Hemisphere;
        splashShape.radius = 0.1f;
        
        // Create the material for splash
        Material splashMaterial = new Material(waterMaterial);
        splashObj.GetComponent<ParticleSystemRenderer>().material = splashMaterial;
        
        // Add a trigger script for splash effect
        waterEffectObj.AddComponent<WaterSplashTrigger>();
        
        // Create prefab
        string prefabPath = "Assets/Prefabs/WaterParticles.prefab";
        
        // Make sure directory exists
        string directory = System.IO.Path.GetDirectoryName(prefabPath);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        
        // Create the prefab
        PrefabUtility.SaveAsPrefabAsset(waterEffectObj, prefabPath);
        Debug.Log("Water particles prefab created at: " + prefabPath);
        
        // Destroy the temp object
        DestroyImmediate(waterEffectObj);
    }
    #endif
}

// Additional script for water splash effect
public class WaterSplashTrigger : MonoBehaviour
{
    private ParticleSystem waterEffect;
    private ParticleSystem splashEffect;
    
    void Start()
    {
        waterEffect = GetComponent<ParticleSystem>();
        splashEffect = transform.Find("WaterSplash").GetComponent<ParticleSystem>();
        
        // Set up collision callback
        ParticleSystem.CollisionModule collision = waterEffect.collision;
        
        #if UNITY_2020_1_OR_NEWER
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.sendCollisionMessages = true;
        #endif
    }
    
    void OnParticleCollision(GameObject other)
    {
        // Play splash effect at collision position
        if (splashEffect != null && !splashEffect.isPlaying)
        {
            splashEffect.transform.position = other.transform.position;
            splashEffect.Play();
        }
    }
}