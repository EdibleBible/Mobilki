using Photon.Pun;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrowableItem : MonoBehaviourPun, IThrowableItem, IPickable
{
    [SerializeField] private Rigidbody itemRb;
    public float flightCheckThreshold { get; set; } = 0.1f;
    public float flightCheckDelay { get; set; } = 0.05f;
    public bool IsDisable { get; set; }
    public bool CanHit { get; set; }

    private bool hasHit = false;

    private bool isHeld = false;

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

    // Ta metoda będzie wywołana jako RPC do upuszczenia obiektu
    [PunRPC]
    public void DropObject(int itemViewID)
    {
        // Znajdź obiekt na podstawie ViewID
        PhotonView itemPhotonView = PhotonView.Find(itemViewID);
        if (itemPhotonView != null)
        {
            GameObject itemInPlayerHand = itemPhotonView.gameObject;

            // Odłącz obiekt od rodzica
            itemInPlayerHand.transform.SetParent(null);

            // Ustawienia fizyki
            Rigidbody itemRb = itemInPlayerHand.GetComponent<Rigidbody>();
            if (itemRb != null)
            {
                itemRb.useGravity = true;
                itemRb.isKinematic = false;
            }

            // Aktualizacja stanu
            isHeld = false;
            IsDisable = false;

            // Dodatkowe operacje, jeśli są wymagane
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
        // Używamy RPC, by wywołać wyrzucenie przedmiotu u wszystkich graczy
        Invoke(nameof(CheckFlightState), flightCheckDelay);
        photonView.RPC("ThrowItemRPC", RpcTarget.All, power, direction); // Wysyłamy parametry do RPC
    }
    [PunRPC]
    public void ThrowItemRPC(float power, Vector3 direction)
    {
        itemRb.AddForce(direction * power);
        CanHit = true;
        Invoke(nameof(CheckFlightState), flightCheckDelay);
    }
    public void CheckFlightState()
    {
        // Sprawdź, czy prędkość rigidbody jest poniżej progu
        if (itemRb.linearVelocity.magnitude < flightCheckThreshold)
        {
            IsDisable = false;
            CanHit = false;

        }
        else
        {
            // Jeśli nadal jest w ruchu, ponownie sprawdź za chwilę
            CanHit = true;
            Invoke(nameof(CheckFlightState), flightCheckDelay);
        }
    }


    public void OnItemHit()
    {
        Debug.Log($"gameobject: {name} Hitted");
        Destroy(gameObject); // Zniszczenie obiektu po trafieniu
    }

    private void OnTriggerEnter(Collider other)
    {
        // Sprawdzamy, czy obiekt trafił w gracza i czy obiekt nie został jeszcze trafiony
        if (!hasHit && other.gameObject.TryGetComponent(out PlayerHitController hitControler) && other.gameObject.TryGetComponent(out PlayerPickUpObject pickUp))
        {
            // Sprawdzamy, czy możemy trafić (np. blokada czy gracz jest w stanie przyjąć trafienie)
            if (CanHit)
            {
                // Uzyskujemy PhotonView gracza i jego obiekt Player
                PhotonView photonView = other.gameObject.GetComponent<PhotonView>();
                if (photonView != null)
                {
                    Photon.Realtime.Player hitPlayer = photonView.Owner; // Dostęp do gracza sieciowego

                    // Wywołanie punktowania, przekazując odpowiedniego gracza
                    hitControler.OnHit(itemRb, 10, hitPlayer); // Wywołanie punktowania

                    hasHit = true; // Flaga ustawiona na true, zapobiegająca wielokrotnemu wywołaniu
                    OnItemHit(); // Zniszczenie obiektu
                }
            }
        }
    }

}
