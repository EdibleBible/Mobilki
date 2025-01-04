using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostMenuController : MonoBehaviourPunCallbacks
{
    PhotonView photonView;

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
            photonView = GetComponent<PhotonView>();
            // Wysyłamy RPC do wszystkich graczy, aby zresetowali grę
            photonView.RPC("ResetGameRPC", RpcTarget.Others);
        }
    }

    // RPC do resetowania gry na wszystkich klientach
    [PunRPC]
    public void ResetGameRPC()
    {
        // Możesz dodać tu kod do resetowania gry na poziomie klienta,
        // np. usunięcie punktów, resetowanie postaci itp.
        Debug.Log("Gra została zresetowana u wszystkich graczy!");

        // Tworzymy nowy pokój
        CreateNewRoom();
    }

    private void CreateNewRoom()
    {
        // Ustalamy parametry dla nowego pokoju
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4; // Ustaw liczba graczy w pokoju (możesz to zmienić według potrzeby)

        // Tworzymy nowy pokój
        PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        // Po utworzeniu nowego pokoju, host pozostaje hostem
        Debug.Log("Nowy pokój został utworzony!");
        
        // Wczytujemy scenę tylko po stworzeniu nowego pokoju
        PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
    }

    public override void OnJoinedRoom()
    {
        // Po dołączeniu do pokoju, host pozostaje hostem
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Jestem hostem w nowym pokoju!");
        }

        // Wczytujemy scenę po dołączeniu do pokoju
        PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
    }
}
