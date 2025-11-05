using UnityEngine;

public class WaterRipple : MonoBehaviour
{
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float maxScale = 0.5f;
    [SerializeField] private AnimationCurve scaleCurve;
    [SerializeField] private AnimationCurve alphaCurve;
    
    private float startTime;
    private Material rippleMaterial;
    private MeshRenderer meshRenderer;
    
    void Start()
    {
        startTime = Time.time;
        meshRenderer = GetComponent<MeshRenderer>();
        
        if (meshRenderer != null)
        {
            rippleMaterial = meshRenderer.material;
        }
        
        // Destroy after duration
        Destroy(gameObject, duration);
    }
    
    void Update()
    {
        float normalizedTime = (Time.time - startTime) / duration;
        
        // Scale the ripple over time
        if (normalizedTime <= 1.0f)
        {
            float scale = scaleCurve.Evaluate(normalizedTime) * maxScale;
            transform.localScale = new Vector3(scale, scale, scale);
            
            // Fade out ripple
            if (rippleMaterial != null)
            {
                Color color = rippleMaterial.color;
                color.a = alphaCurve.Evaluate(normalizedTime);
                rippleMaterial.color = color;
            }
        }
    }
}