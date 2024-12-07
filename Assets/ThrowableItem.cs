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

    public void DropItem(ref GameObject ItemInPlayerHand)
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

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Pickup_RPC(NetworkObject pickingPlayer)
    {
        if (pickingPlayer != null)
        {
            PlayerPickUpObject pickUp = pickingPlayer.GetComponent<PlayerPickUpObject>();
            objectHolder = pickingPlayer.transform;
            IsDisable = true;

            itemRb.useGravity = false;
            itemRb.isKinematic = true;

            transform.SetParent(pickUp.HoldItemTransform);

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

    public GameObject PickUpItem(Transform holdParent, Transform playerParent)
    {
        objectHolder = playerParent;
        IsDisable = true;

        itemRb.useGravity = false;
        itemRb.isKinematic = true;

        transform.SetParent(holdParent);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Wywołanie RPC z pojedynczym argumentem - obiekt NetworkObject gracza
        Pickup_RPC(playerParent.GetComponent<NetworkObject>());

        return gameObject;
    }



    public void ThrowItemWithDrop(ref GameObject ItemInPlayerHand, float power, Vector3 direction)
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
        if (IsDisable)
            return;

        if (other.gameObject.TryGetComponent(out PlayerHitController obj))
        {
            if (obj.transform == objectHolder)
                return;

            obj.OnHit(itemRb);
            OnItemHit();
        }
    }

    private IEnumerator TimeToDisableObjectHolder()
    {
        yield return new WaitForSeconds(0.2f);
        objectHolder = null;
    }
}
