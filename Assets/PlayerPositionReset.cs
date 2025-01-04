using Photon.Pun;
using UnityEngine;

public class PlayerPositionReset : MonoBehaviourPun
{
    [SerializeField] private PhotonView photonView;

    public void ResetPlayerPosition()
    {
        photonView.RPC("ResetPlayerPositionRPC", RpcTarget.All);
    }

    [PunRPC]
    private void ResetPlayerPositionRPC()
    {
        // To jest wywołane przez wszystkich graczy, ale każdy gracz zmienia tylko swoją pozycję
        if (photonView.IsMine)  // Tylko właściciel obiektu zmienia swoją pozycję
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " zmienia swoją pozycję");

            // Upewnij się, że operacja dotyczy właściwego obiektu gracza
            var playerObj = photonView.gameObject;  // Zamiast szukać po ActorNumber, używamy przypisanego PhotonView
            
            // Losowanie nowej pozycji
            var position = GetRandomPosition(0, 5, 0, 5);

            Debug.Log(playerObj.transform.position + " To twoja stara pozycja");
            Debug.Log(position + " To twoja nowa pozycja");

            // Zmiana pozycji gracza
            var photonTransform = playerObj.GetComponent<PhotonTransformView>();
            

            Debug.Log(playerObj.transform.position + " To twoja pozycja po zmianie");
        }
    }

    public void ResetPlayerHand()
    {
        photonView.RPC("ResetPlayerHandRPC", RpcTarget.All);
    }

    [PunRPC]
    private void ResetPlayerHandRPC()
    {
        var playerObj = PhotonNetwork.GetPhotonView(PhotonNetwork.LocalPlayer.ActorNumber).gameObject;
        var playerView = playerObj.GetComponent<PhotonView>();
        var playerPickUpObj = playerObj.GetComponent<PlayerPickUpObject>();
        
        Destroy(playerPickUpObj.pickupedObject);
        playerPickUpObj.pickupedObject = null;
    }
    
    protected Vector3 GetRandomPosition(float minX, float maxX, float minZ, float maxZ)
    {
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);

        // Zakłada, że pozycja na Y pozostaje niezmieniona
        return new Vector3(randomX, 1, randomZ);
    }
    
}
