using System.Linq;
using Photon.Pun;
using System.Threading.Tasks;
using Photon.Realtime;
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
    public bool HasTriggered = false;
    
    public Player ThrowObjectParent;

    public void DropItem(ref GameObject ItemInPlayerHand, bool isThrow = false)
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
            photonView.RPC("DropObject", RpcTarget.AllBuffered, itemViewID, isThrow);

            if (!isThrow)
            {
                Debug.Log("Drop Item");
                ThrowObjectParent = null;
            }
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
        Player p = PhotonNetwork.GetPhotonView(playerID).Owner;

        if (player != null && p != null)
        {
            transform.SetParent(player.transform); // Obiekt staje się dzieckiem gracza
            IsDisable = true;
            CanHit = false;
            isHeld = true;

            itemRb.useGravity = false;
            itemRb.isKinematic = true;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            ThrowObjectParent = p;
        }
        else
        {
            Debug.Log("Gameobject or player is NULL");
        }
    }

    [PunRPC]
    public void DropObject(int itemViewID, bool isThrow)
    {
        PhotonView itemPhotonView = PhotonView.Find(itemViewID);
        if (itemPhotonView != null)
        {
            GameObject itemInPlayerHand = itemPhotonView.gameObject;

            itemInPlayerHand.transform.SetParent(null);

            if (!isThrow)
            {
                Debug.Log("Drop Item");
                ThrowObjectParent = null;
            }


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
        ThrowObjectParent = PhotonNetwork.GetPhotonView(PlayerPhothonViewId).Owner;
        Debug.Log($"ThrowObjectParent ustawiony na {PhotonNetwork.GetPhotonView(PlayerPhothonViewId).Owner.NickName}");

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        return gameObject;
    }

    public void ThrowItemWithDrop(ref GameObject ItemInPlayerHand, float power, Vector3 direction)
    {
        DropItem(ref ItemInPlayerHand, true);
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
            ThrowObjectParent = null;
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
        if (ThrowObjectParent == null)
        {
            Debug.Log("ThrowObjectParent == null");
            return;
        }

        if (!HasTriggered && other.gameObject.TryGetComponent(out PlayerHitController hitControler) &&
            other.gameObject.TryGetComponent(out PlayerPickUpObject pickUp))
        {
            if (CanHit)
            {
                PhotonView photonView = other.gameObject.GetComponent<PhotonView>();
                if (photonView != null)
                {
                    Photon.Realtime.Player hitPlayer = photonView.Owner;
                    hitControler.OnHit(itemRb, 10, ThrowObjectParent,HasTriggered);

                    HasTriggered = true;  // Zmieniamy flagę na true, aby nie wywoływać tego ponownie
                    OnItemHit();
                }
            }
        }
    }

// Dodatkowa funkcja resetująca flagę (np. po jakimś czasie lub warunku)
    public void ResetTriggerFlag()
    {
        HasTriggered = false;  // Resetujemy flagę, aby zdarzenie mogło się ponownie wykonać
    }
}