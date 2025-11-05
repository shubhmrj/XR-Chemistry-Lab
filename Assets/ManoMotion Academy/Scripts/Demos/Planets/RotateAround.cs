using UnityEngine;

public class RotateAround : MonoBehaviour
{
    [SerializeField] GameObject target;
    [SerializeField] int speed;

    void Update()
    {
        transform.RotateAround(target.transform.position, new Vector3(0, 1, 0), speed * Time.deltaTime);
    }
}