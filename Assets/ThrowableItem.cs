using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrowableItem : MonoBehaviour, IThrowableItem, IPickable
{
    [SerializeField] private Rigidbody itemRb;
    public float flightCheckThreshold { get; set; } = 0.1f;
    public float flightCheckDelay { get; set; } = 0.5f;
    public bool IsDisable { get; set; }

    public void DropItem(ref GameObject ItemInPlayerHand)
    {
        IsDisable = false;
        itemRb.useGravity = true;
        itemRb.isKinematic = false;
        ItemInPlayerHand = null;
        transform.parent = null;
    }

    public void HighLight()
    {
        Debug.Log($"item: {gameObject.name} is highLight");
    }
    public GameObject PickUpItem(Transform holdParent)
    {
        IsDisable = true;

        itemRb.useGravity = false;
        itemRb.isKinematic = true;

        transform.SetParent(holdParent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        return gameObject;
    }
    public void ThrowItemWithDrop(ref GameObject ItemInPlayerHand,float power, Vector3 direction)
    {
        DropItem(ref ItemInPlayerHand);
        ThrowItem(power, direction);
    }
    public void ThrowItem(float power, Vector3 direction)
    {
        itemRb.AddForce(direction * power);
        Invoke(nameof(CheckFlightState), flightCheckDelay);
    }
    public void CheckFlightState()
    {
        // Sprawdź, czy prędkość rigidbody jest poniżej progu
        if (itemRb.linearVelocity.magnitude < flightCheckThreshold)
        {
            IsDisable = false;
        }
        else
        {
            // Jeśli nadal jest w ruchu, ponownie sprawdź za chwilę
            Invoke(nameof(CheckFlightState), flightCheckDelay);
        }
    }

    public void OnItemHit()
    {
        Debug.Log($"gameobject: {name} Hitted");
        Destroy(gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerHitController obj))
        {
            obj.OnHit(itemRb);
            OnItemHit();
        }
    }
}
