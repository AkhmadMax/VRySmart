using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MatchManager : MonoBehaviour
{
    public NetworkManager manager;
    public GameObject JoinRoomListRowPrefab;
    public GameObject RoomListPanel;
    public Text SnackbarText;
    public Text NoPreviousRoomsText;
    public GameObject CurrentRoomLabel;

    /// <summary>
    /// The return to lobby button in AR Scene.
    /// </summary>
    public GameObject ReturnButton;

    private ActiveScreen _currentActiveScreen = ActiveScreen.LobbyScreen;
    private const int matchPageSize = 5;
    private List<GameObject> joinRoomButtonsPool = new List<GameObject>();
    private string currentRoomNumber;
    private const string _hasDisplayedStartInfoKey = "HasDisplayedStartInfo";

    /// <summary>
    /// Enumerates the active UI screens the example application can be in.
    /// </summary>
    public enum ActiveScreen
    {
        /// <summary>
        /// Enume mode that indicate the example application is on lobby screen.
        /// </summary>
        LobbyScreen,

        /// <summary>
        /// Enume mode that indicate the example application is on start screen.
        /// </summary>
        StartScreen,

        /// <summary>
        /// Enume mode that indicate the example application is on AR screen.
        /// </summary>
        GameScreen,
    }



    private void Awake()
    {
        //manager = GetComponent<NetworkManager>();

        // Initialize the pool of Join Room buttons.
        for (int i = 0; i < matchPageSize; i++)
        {
            GameObject button = Instantiate(JoinRoomListRowPrefab);
            button.transform.SetParent(RoomListPanel.transform, false);
            button.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(0, -(100 * i));
            button.SetActive(true);
            button.GetComponentInChildren<Text>().text = string.Empty;
            joinRoomButtonsPool.Add(button);
        }

        manager.StartMatchMaker();
        manager.matchMaker.ListMatches(
            startPageNumber: 0,
            resultPageSize: matchPageSize,
            matchNameFilter: string.Empty,
            filterOutPrivateMatchesFromResults: false,
            eloScoreTarget: 0,
            requestDomain: 0,
            callback: OnMatchList);
        ChangeLobbyUIVisibility(true);
    }

    public void OnCreateRoomClicked()
    {
        manager.matchMaker.CreateMatch(
            manager.matchName, manager.matchSize, true, string.Empty, string.Empty,
            string.Empty, 0, 0, OnMatchCreate);
    }

    /// <summary>
    /// Callback that happens when a <see cref="NetworkMatch.CreateMatch"/> request has been
    /// processed on the server.
    /// </summary>
    /// <param name="success">Indicates if the request succeeded.</param>
    /// <param name="extendedInfo">A text description for the error if success is false.</param>
    /// <param name="matchInfo">The information about the newly created match.</param>
#pragma warning disable 618
    private void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
#pragma warning restore 618
    {
        if (!success)
        {
            SnackbarText.text = "Could not create match: " + extendedInfo;
            return;
        }

        manager.OnMatchCreate(success, extendedInfo, matchInfo);
        currentRoomNumber = GetRoomNumberFromNetworkId(matchInfo.networkId);
        SnackbarText.text = "Connecting to server...";
        ChangeLobbyUIVisibility(false);
        CurrentRoomLabel.GetComponentInChildren<Text>().text = "Room: " + currentRoomNumber;
    }

    public void OnRefhreshRoomListClicked()
    {
        manager.matchMaker.ListMatches(
            startPageNumber: 0,
            resultPageSize: matchPageSize,
            matchNameFilter: string.Empty,
            filterOutPrivateMatchesFromResults: false,
            eloScoreTarget: 0,
            requestDomain: 0,
            callback: OnMatchList);
    }

    private void OnJoinRoomClicked(MatchInfoSnapshot match)
    {
        manager.matchName = match.name;
        manager.matchMaker.JoinMatch(match.networkId, string.Empty, string.Empty,
                                     string.Empty, 0, 0, OnMatchJoined);
    }

    /// <summary>
    /// Handles a user intent to return to the lobby.
    /// </summary>
    public void OnReturnToLobbyClick()
    {
        ReturnButton.GetComponent<Button>().interactable = false;
        if (manager.matchInfo == null)
        {
            OnMatchDropped(true, null);
            return;
        }

        manager.matchMaker.DropConnection(manager.matchInfo.networkId,
            manager.matchInfo.nodeId, manager.matchInfo.domain, OnMatchDropped);
    }

    private void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (!success)
        {
            SnackbarText.text = "Could not join to match: " + extendedInfo;
            return;
        }

        manager.OnMatchJoined(success, extendedInfo, matchInfo);
        currentRoomNumber = GetRoomNumberFromNetworkId(matchInfo.networkId);
        SnackbarText.text = "Connecting to server...";
        ChangeLobbyUIVisibility(false);
        CurrentRoomLabel.GetComponentInChildren<Text>().text = "Room: " + currentRoomNumber;
    }

    /// <summary>
    /// Callback that happens when a <see cref="NetworkMatch.DropConnection"/> request has been
    /// processed on the server.
    /// </summary>
    /// <param name="success">Indicates if the request succeeded.</param>
    /// <param name="extendedInfo">A text description for the error if success is false.
    /// </param>
    private void OnMatchDropped(bool success, string extendedInfo)
    {
        ReturnButton.GetComponent<Button>().interactable = true;
        if (!success)
        {
            SnackbarText.text = "Could not drop the match: " + extendedInfo;
            return;
        }

        manager.OnDropConnection(success, extendedInfo);
        NetworkManager.Shutdown();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        if (!success)
        {
            SnackbarText.text = "Could not list matches: " + extendedInfo;
            return;
        }

        manager.OnMatchList(success, extendedInfo, matches);
        if (manager.matches != null)
        {
            // Reset all buttons in the pool.
            foreach (GameObject button in joinRoomButtonsPool)
            {
                button.SetActive(false);
                button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                button.GetComponentInChildren<Text>().text = string.Empty;
            }

            NoPreviousRoomsText.gameObject.SetActive(manager.matches.Count == 0);

            // Add buttons for each existing match.
            int i = 0;
#pragma warning disable 618
            foreach (var match in manager.matches)
#pragma warning restore 618
            {
                if (i >= matchPageSize)
                {
                    break;
                }

                var text = "Room " + GetRoomNumberFromNetworkId(match.networkId);
                GameObject button = joinRoomButtonsPool[i++];
                button.GetComponentInChildren<Text>().text = text;
                button.GetComponentInChildren<Button>().onClick.AddListener(() => OnJoinRoomClicked(match));
                //button.GetComponentInChildren<Button>().onClick.AddListener(CloudAnchorsExampleController.OnEnterResolvingModeClick);
                button.SetActive(true);
            }
        }
    }

    private void ChangeLobbyUIVisibility(bool visible)
    {
        foreach (GameObject button in joinRoomButtonsPool)
        {
            bool active = visible && button.GetComponentInChildren<Text>().text != string.Empty;
            button.SetActive(active);
        }

        OnLobbyVisibilityChanged(visible);
    }

    /// <summary>
    /// Callback called when the lobby screen's visibility is changed.
    /// </summary>
    /// <param name="visible">If set to <c>true</c> visible.</param>
    public void OnLobbyVisibilityChanged(bool visible)
    {
        if (visible)
        {
            SwitchActiveScreen(ActiveScreen.LobbyScreen);
        }
        else if (PlayerPrefs.HasKey(_hasDisplayedStartInfoKey))
        {
            SwitchActiveScreen(ActiveScreen.GameScreen);
        }
        else
        {
            SwitchActiveScreen(ActiveScreen.StartScreen);
        }
    }

    public GameObject LobbyScreen;

    /// <summary>
    /// The Start Screen to see help information.
    /// </summary>
    public GameObject StartScreen;

    /// <summary>
    /// The AR Screen which display the AR view, return to lobby button and room number.
    /// </summary>
    public GameObject GameScreen;

    private void SwitchActiveScreen(ActiveScreen activeScreen)
    {
        LobbyScreen.SetActive(activeScreen == ActiveScreen.LobbyScreen);
        //StatusScreen.SetActive(activeScreen != ActiveScreen.StartScreen);
        StartScreen.SetActive(activeScreen == ActiveScreen.StartScreen);

        bool switchToGameScreen = activeScreen == ActiveScreen.GameScreen;
        GameScreen.SetActive(switchToGameScreen);

        _currentActiveScreen = activeScreen;

        if (_currentActiveScreen == ActiveScreen.StartScreen)
        {
            PlayerPrefs.SetInt(_hasDisplayedStartInfoKey, 1);
        }
    }

    private string GetRoomNumberFromNetworkId(NetworkID networkID)
    {
        return (System.Convert.ToInt64(networkID.ToString()) % 10000).ToString();
    }
}
