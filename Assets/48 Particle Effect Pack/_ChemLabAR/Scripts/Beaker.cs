using UnityEngine;

public class Beaker : MonoBehaviour
{
    public AudioSource reactionSound;
    public GameObject reactionEffectPrefab;  // <-- Drag ReactionEffect prefab here

    private int chemicalCount = 0;
    private Renderer beakerRenderer;

    void Start()
    {
        beakerRenderer = GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Liquid"))
        {
            chemicalCount++;
            Destroy(other.gameObject);

            if (reactionSound != null)
                reactionSound.Play();

            if (chemicalCount >= 2)
            {
                PerformReaction();
            }
        }
    }

    private void PerformReaction()
    {
        beakerRenderer.material.color = Color.green;

        if (reactionEffectPrefab != null)
        {
            Instantiate(reactionEffectPrefab, transform.position + Vector3.up * 0.2f, Quaternion.identity);
        }
    }
}
