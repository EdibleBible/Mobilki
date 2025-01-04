using Photon.Pun;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrowableItem : MonoBehaviourPun, IThrowableItem, IPickable
{
    [SerializeField] private Rigidbody itemRb;
    public float flightCheckThreshold { get; set; } = 0.1f;
    public float flightCheckDelay { get; set; } = 0.05f;
    public float maxFlightTime { get; set; } = 10f; // Maksymalny czas lotu przed zniszczeniem
    public bool IsDisable { get; set; }
    public bool CanHit { get; set; }

    private bool hasHit = false;
    private bool isHeld = false;
    private float flightTime = 0f; // Licznik czasu lotu

    public void DropItem(ref GameObject ItemInPlayerHand)
    {
        IsDisable = false;
        itemRb.useGravity = true;
        itemRb.isKinematic = false;
        isHeld = false;
        transform.parent = null;
        transform.SetParent(null);
        PhotonView itemPhotonView = ItemInPlayerHand.GetComponent<PhotonView>();
        if (itemPhotonView != null)
        {
            int itemViewID = itemPhotonView.ViewID;

            // Wywołaj RPC, przekazując ViewID
            photonView.RPC("DropObject", RpcTarget.AllBuffered, itemViewID);
        }
        else
        {
            Debug.LogError("Obiekt nie posiada komponentu PhotonView.");
        }
        ItemInPlayerHand = null;
    }

    [PunRPC]
    public void AssignParent(int playerID)
    {
        GameObject player = PhotonNetwork.GetPhotonView(playerID).gameObject;
        if (player != null)
        {
            transform.SetParent(player.transform); // Obiekt staje się dzieckiem gracza
            IsDisable = true;
            CanHit = false;
            isHeld = true;

            itemRb.useGravity = false;
            itemRb.isKinematic = true;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

    [PunRPC]
    public void DropObject(int itemViewID)
    {
        PhotonView itemPhotonView = PhotonView.Find(itemViewID);
        if (itemPhotonView != null)
        {
            GameObject itemInPlayerHand = itemPhotonView.gameObject;

            itemInPlayerHand.transform.SetParent(null);

            Rigidbody itemRb = itemInPlayerHand.GetComponent<Rigidbody>();
            if (itemRb != null)
            {
                itemRb.useGravity = true;
                itemRb.isKinematic = false;
            }

            isHeld = false;
            IsDisable = false;
        }
        else
        {
            Debug.LogError("Nie znaleziono obiektu z podanym ViewID.");
        }
    }

    public void HighLight()
    {
        Debug.Log($"item: {gameObject.name} is highLight");
    }

    public GameObject PickUpItem(Transform holdParent, int PlayerPhothonViewId)
    {
        if (isHeld)
        {
            return null;
        }
        IsDisable = true;
        CanHit = false;
        isHeld = true;

        itemRb.useGravity = false;
        itemRb.isKinematic = true;

        transform.SetParent(holdParent);
        photonView.RPC("AssignParent", RpcTarget.AllBuffered, PlayerPhothonViewId);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        return gameObject;
    }

    public void ThrowItemWithDrop(ref GameObject ItemInPlayerHand, float power, Vector3 direction)
    {
        DropItem(ref ItemInPlayerHand);
        ThrowItem(power, direction);
    }

    public void ThrowItem(float power, Vector3 direction)
    {
        flightTime = 0f; // Resetujemy licznik czasu lotu
        Invoke(nameof(CheckFlightState), flightCheckDelay);
        photonView.RPC("ThrowItemRPC", RpcTarget.All, power, direction);
    }

    [PunRPC]
    public void ThrowItemRPC(float power, Vector3 direction)
    {
        itemRb.AddForce(direction * power);
        CanHit = true;
        flightTime = 0f; // Reset licznika u wszystkich
        Invoke(nameof(CheckFlightState), flightCheckDelay);
    }

    public void CheckFlightState()
    {
        // Sprawdzamy, czy obiekt nadal leci
        if (itemRb.linearVelocity.magnitude >= flightCheckThreshold)
        {
            flightTime += flightCheckDelay;

            if (flightTime >= maxFlightTime)
            {
                Debug.Log("Obiekt był zbyt długo w locie i zostanie zniszczony.");
                photonView.RPC("DestroyObjectRPC", RpcTarget.AllBuffered);
                return;
            }

            CanHit = true;
            Invoke(nameof(CheckFlightState), flightCheckDelay);
        }
        else
        {
            // Jeśli obiekt przestał się poruszać, resetujemy stany
            IsDisable = false;
            CanHit = false;
        }
    }

    [PunRPC]
    public void DestroyObjectRPC()
    {
        Debug.Log($"Obiekt {gameObject.name} został zniszczony.");
        Destroy(gameObject);
    }

    public void OnItemHit()
    {
        Debug.Log($"gameobject: {name} Hitted");
        photonView.RPC("DestroyObjectRPC", RpcTarget.AllBuffered);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasHit && other.gameObject.TryGetComponent(out PlayerHitController hitControler) && other.gameObject.TryGetComponent(out PlayerPickUpObject pickUp))
        {
            if (CanHit)
            {
                PhotonView photonView = other.gameObject.GetComponent<PhotonView>();
                if (photonView != null)
                {
                    Photon.Realtime.Player hitPlayer = photonView.Owner;
                    hitControler.OnHit(itemRb, 10, hitPlayer);

                    hasHit = true;
                    OnItemHit();
                }
            }
        }
    }
}
