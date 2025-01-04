using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameTimer : MonoBehaviourPunCallbacks
{
    public float GameDuration = 300f; // Czas trwania gry w sekundach
    private double StartTime;
    private bool gameEnded = false;

    [SerializeField] private GameObject hostPlayerMenu;
    [SerializeField] private GameObject normalPlayerMenu;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartTime = PhotonNetwork.Time; // Zapisujemy czas początkowy gry
            // Ustawiamy StartTime w CustomProperties pokoju tylko raz, na początku
            Hashtable roomProperties = new Hashtable { { "StartTime", StartTime } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
        else
        {
            // Gracze dołączający odczytują StartTime z CustomProperties
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("StartTime", out object startTimeObj))
            {
                StartTime = (double)startTimeObj;
            }
        }
    }

    void Update()
    {
        if (!gameEnded)
        {
            double elapsedTime = PhotonNetwork.Time - StartTime;
            Debug.Log(elapsedTime);
            if (elapsedTime >= GameDuration)
            {
                EndGame(); // Zakończenie gry
            }
        }
    }

    private void EndGame()
    {
        gameEnded = true;

        if (PhotonNetwork.IsMasterClient)
        {
            // Host wyświetla menu po zakończeniu czasu
            ShowHostMenu();
            photonView.RPC("DisplayWaitMessage", RpcTarget.Others);

        }
        else
        {
            // Gracze czekający na decyzję hosta
        }
    }

    // Funkcja wyświetlająca menu hosta
    private void ShowHostMenu()
    {
        // Tutaj wywołaj metodę odpowiedzialną za wyświetlenie menu resetu/wyjścia u hosta
        Debug.Log("Host Menu: Option to Reset or Exit");
        // Możesz tu uruchomić UI (np. Canvas z menu)
        hostPlayerMenu.SetActive(true);
    }

    public void ResetGameTimer()
    {
        photonView.RPC("CloseMenu", RpcTarget.All);
        Debug.Log("RPC Close Menu");
        
        photonView.RPC("ResetTime", RpcTarget.All);
        Debug.Log("RPC Reset Time");
        
        photonView.RPC("ResetTimeUI",RpcTarget.All);
        Debug.Log("RPC Reset TimeUI");
        
        Debug.Log("Host Menu: Option to Reset or Exit");
    }

    [PunRPC]
    private void DisplayWaitMessage()
    {
        normalPlayerMenu.SetActive(true);
    }
    
    [PunRPC]
    private void CloseMenu()
    {
        if (hostPlayerMenu != null)
        {
            Debug.Log("Host Close Menu");
            hostPlayerMenu.SetActive(false);
        }

        if (normalPlayerMenu != null)
        {
            Debug.Log("Normal Close Menu");
            normalPlayerMenu.SetActive(false);
        }
    }
    
    [PunRPC]
    private void ResetTime()
    {
        gameEnded = false;
        if (PhotonNetwork.IsMasterClient)
        {
            StartTime = PhotonNetwork.Time; // Zapisujemy czas początkowy gry
            // Ustawiamy StartTime w CustomProperties pokoju tylko raz, na początku
            Hashtable roomProperties = new Hashtable { { "StartTime", StartTime } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
        else
        {
            // Gracze dołączający odczytują StartTime z CustomProperties
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("StartTime", out object startTimeObj))
            {
                StartTime = (double)startTimeObj;
            }
        }
    }
}
