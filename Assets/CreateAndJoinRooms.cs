using Photon.Pun;
using TMPro;
using UnityEngine;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_InputField createInput; // Pole do wpisania nazwy pokoju do stworzenia
    [SerializeField] private TMP_InputField joinInput;   // Pole do wpisania nazwy pokoju do dołączenia
    [SerializeField] private TMP_InputField playerNameInput; // Pole do wpisania nazwy gracza
    [SerializeField] private TMP_InputField maxPlayersInput; // Pole do wpisania maksymalnej liczby graczy
    [SerializeField] private TextMeshProUGUI feedbackText; // Pole tekstowe na komunikaty zwrotne

    private Coroutine feedbackCoroutine;

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings(); // Łączymy się z Photonem
        }

        // Ograniczanie wprowadzania danych w polu maxPlayersInput do cyfr
        if (maxPlayersInput != null)
        {
            maxPlayersInput.onValidateInput += (text, charIndex, addedChar) =>
            {
                return char.IsDigit(addedChar) ? addedChar : '\0';
            };
        }
    }

    public void CreateRoom()
    {
        if (string.IsNullOrWhiteSpace(createInput.text))
        {
            DisplayFeedback("Nazwa pokoju nie może być pusta!");
            return;
        }

        if (string.IsNullOrWhiteSpace(maxPlayersInput.text) || !int.TryParse(maxPlayersInput.text, out int maxPlayers) || maxPlayers < 2 || maxPlayers > 10)
        {
            DisplayFeedback("Podaj poprawną maksymalną liczbę graczy (2-10).");
            return;
        }

        // Ustawienie nazwy gracza przed stworzeniem pokoju
        SetPlayerName();

        Debug.Log($"Tworzenie pokoju: {createInput.text} z maksymalną liczbą graczy: {maxPlayers}");
        PhotonNetwork.CreateRoom(createInput.text, new Photon.Realtime.RoomOptions { MaxPlayers = (byte)maxPlayers });
    }

    public void JoinRoom()
    {
        if (string.IsNullOrWhiteSpace(joinInput.text))
        {
            DisplayFeedback("Nazwa pokoju nie może być pusta!");
            return;
        }

        // Ustawienie nazwy gracza przed dołączeniem do pokoju
        SetPlayerName();

        Debug.Log($"Dołączanie do pokoju: {joinInput.text}");
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public void SetPlayerName()
    {
        if (string.IsNullOrWhiteSpace(playerNameInput.text))
        {
            DisplayFeedback("Nazwa gracza nie może być pusta!");
            return;
        }

        PhotonNetwork.NickName = playerNameInput.text;
        Debug.Log($"Nazwa gracza ustawiona na: {PhotonNetwork.NickName}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Dołączono do pokoju. Ładowanie sceny 'Game'.");
        PhotonNetwork.LoadLevel("Game");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        DisplayFeedback($"Tworzenie pokoju nie powiodło się: {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        switch (returnCode)
        {
            case Photon.Realtime.ErrorCode.GameDoesNotExist:
                DisplayFeedback("Pokój o podanej nazwie nie istnieje.");
                break;
            case Photon.Realtime.ErrorCode.GameFull:
                DisplayFeedback("Pokój jest pełny.");
                break;
            default:
                DisplayFeedback($"Dołączanie do pokoju nie powiodło się: {message}");
                break;
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Połączono z serwerem Master. Gotowy do tworzenia lub dołączania do pokoi.");
    }

    private void DisplayFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;

            // Anulowanie poprzedniej Coroutine (jeśli istnieje)
            if (feedbackCoroutine != null)
            {
                StopCoroutine(feedbackCoroutine);
            }

            // Uruchomienie nowej Coroutine
            feedbackCoroutine = StartCoroutine(HideFeedbackAfterDelay(3f));
        }
        Debug.LogError(message);
    }

    private System.Collections.IEnumerator HideFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (feedbackText != null)
        {
            feedbackText.text = string.Empty; // Czyszczenie tekstu
        }

        feedbackCoroutine = null; // Resetowanie referencji do Coroutine
    }
}
