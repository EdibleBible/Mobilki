using Photon.Pun;
using UnityEngine;

public class PlayerPositionManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private Vector3 playAreaMinBounds; // Minimalne granice pola gry
    [SerializeField] private Vector3 playAreaMaxBounds; // Maksymalne granice pola gry
    [SerializeField] private LayerMask layerMask;

    public void RandomizePlayerPositions()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Only the MasterClient can randomize player positions.");
            return;
        }

        foreach (var player in PhotonNetwork.PlayerList)
        {
            // Generowanie losowej pozycji
            Vector3 randomPosition = new Vector3(
                Random.Range(playAreaMinBounds.x, playAreaMaxBounds.x),
                Random.Range(playAreaMinBounds.y, playAreaMaxBounds.y),
                Random.Range(playAreaMinBounds.z, playAreaMaxBounds.z)
            );

            // Ustawianie pozycji dla wszystkich graczy
            photonView.RPC("SetPlayerPosition", player, randomPosition);
        }
    }

    [PunRPC]
    private void SetPlayerPosition(Vector3 position)
    {
        // Ustaw pozycję obiektu gracza, który posiada dany PhotonView
        if (photonView.IsMine)
        {
            transform.position = position;
        }
    }

    public void RemoveObjectsOfTypeOnLayer()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Only the MasterClient can remove objects.");
            return;
        }

        foreach (var obj in FindObjectsOfType<GameObject>())
        {
            // Sprawdzamy, czy obiekt należy do wskazanej warstwy
            if (((1 << obj.layer) & layerMask) != 0)
            {
                // Wywołujemy RPC na wszystkich klientach, aby zniszczyć obiekt
                photonView.RPC("DestroyObject", RpcTarget.AllBuffered, obj.GetPhotonView()?.ViewID);
            }
        }
    }

    [PunRPC]
    private void DestroyObject(int viewID)
    {
        // Znajdujemy obiekt za pomocą PhotonView i usuwamy go
        var obj = PhotonView.Find(viewID)?.gameObject;
        if (obj != null)
        {
            Destroy(obj);
        }
    }
}
