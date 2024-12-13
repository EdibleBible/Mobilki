using System.Collections;
using UnityEngine;
using Fusion;

[RequireComponent(typeof(Rigidbody))]
public class ThrowableItem : NetworkBehaviour, IThrowableItem, IPickable
{
    [SerializeField] private Rigidbody itemRb;

    private Transform objectHolder;
    public float flightCheckThreshold { get; set; } = 0.1f;
    public float flightCheckDelay { get; set; } = 0.5f;
    public bool IsDisable { get; set; }

    public void DropItem(ref NetworkObject ItemInPlayerHand)
    {
        StartCoroutine(TimeToDisableObjectHolder());
        IsDisable = false;
        itemRb.useGravity = true;
        itemRb.isKinematic = false;
        ItemInPlayerHand = null;
        transform.parent = null;
    }

    public void HighLight()
    {
        Debug.Log($"item: {gameObject.name} is highlighted");
    }


    public NetworkObject PickUpItem(Transform holdParent, Transform playerParent)
    {
        objectHolder = playerParent;
        IsDisable = true;

        itemRb.useGravity = false;
        itemRb.isKinematic = true;

        transform.SetParent(holdParent);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        return gameObject.GetComponent<NetworkObject>();
    }



    public void ThrowItemWithDrop(ref NetworkObject ItemInPlayerHand, float power, Vector3 direction)
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
        Debug.Log($"gameobject: {name} hit something");
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
/*        if (IsDisable)
            return;

        if (other.gameObject.TryGetComponent(out PlayerHitController obj))
        {
            if (obj.transform == objectHolder)
                return;

            obj.OnHit(itemRb);
            OnItemHit();
        }*/
    }

    private IEnumerator TimeToDisableObjectHolder()
    {
        yield return new WaitForSeconds(0.2f);
        objectHolder = null;
    }
}
