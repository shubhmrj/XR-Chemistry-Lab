using UnityEngine;

public class ChemicalBottle : MonoBehaviour, IInteractable
{
    public GameObject liquidPrefab;

    private bool isHeld = false;

    public void OnGrab()
    {
        isHeld = true;
    }

    public void OnRelease()
    {
        if (isHeld)
        {
            PourLiquid();
            isHeld = false;
        }
    }

    private void PourLiquid()
    {
        GameObject liquid = Instantiate(liquidPrefab, transform.position + Vector3.down * 0.2f, Quaternion.identity);
        liquid.GetComponent<Rigidbody>().AddForce(Vector3.down * 2f, ForceMode.Impulse);
    }
}
