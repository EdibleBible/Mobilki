using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ConnectionController : MonoBehaviourPunCallbacks
{
    // Zmienna przechowująca referencję do gospodarza w momencie rozpoczęcia gry
    private Player initialMasterClient;

    void Start()
    {
        // Zapisz początkowego gospodarza
        initialMasterClient = PhotonNetwork.MasterClient;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left: " + otherPlayer.NickName);

        // Sprawdzamy, czy to był gospodarz, który opuścił pokój
        if (otherPlayer == initialMasterClient)
        {
            Debug.Log("Gracz, który wyszedł, był gospodarzem!");
            PhotonNetwork.LeaveRoom();
            // Przenosimy wszystkich graczy do nowej sceny (np. Lobby)
            PhotonNetwork.LoadLevel("Lobby");
        }
    }
}