using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostMenuController : MonoBehaviourPunCallbacks
{
    PhotonView photonView;
    [SerializeField] private GameTimer timer;
    [SerializeField] private PlayerPropertiesManager playerPropertiesManager;
    [SerializeField] private PlayerPositionReset playerPositionReset;

    // Funkcja do opuszczenia gry
    public void LeaveGame()
    {
        // Sprawdź, czy gracz jest hostem
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("Lobby"); // Zmienna do wczytania odpowiedniej sceny po opuszczeniu gry
        }
    }

    public void ResetGame()
    {
        // Tylko host może resetować grę
        if (PhotonNetwork.IsMasterClient)
        {
            timer.ResetGameTimer();
            
            playerPropertiesManager.ResetGamePoints();
            
            playerPositionReset.ResetPlayerPosition();
        }
        
    }

}
