using UnityEngine;

public class StickyTopping : MonoBehaviour
{
    private bool stuck = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (stuck) return;
        stuck = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Lo congela al instante en el punto de choque
            Destroy(rb);
        }
        Destroy(GetComponent<Collider>());
        transform.SetParent(collision.transform);
    }
}