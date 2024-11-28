using UnityEngine;

public class PlayerHitController : MonoBehaviour
{
    [SerializeField] private GameObject modelObject;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameEvent e_OnPlayerHitted;

    public void OnHit(Rigidbody throwedItemRb)
    {
        // Wywołanie eventu informującego o trafieniu gracza
        e_OnPlayerHitted.Raise(this, gameObject);

        // Dodanie Rigidbody do modelu (jeśli jeszcze go nie ma)
        Rigidbody rb = modelObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = modelObject.AddComponent<Rigidbody>();
        }

        playerMovement.enabled = false;
        Vector3 hitDirection = (modelObject.transform.position - throwedItemRb.position).normalized;
        float hitForce = throwedItemRb.linearVelocity.magnitude * 1.5f;
        rb.AddForce(hitDirection * hitForce, ForceMode.Impulse);
    }

}
