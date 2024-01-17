using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] LobbyElement lobbyElement;
    [SerializeField] PlayerElement playerElement;
    public GameObject serverLists;
    public GameObject playerLists;
    public Button CreateHostBtn;
    public Button StopHostBtn;
    public TMP_Text lobbyNameTxt;
    public GameObject MainMenuPanel;
    public GameObject WaitingForPlayer;
    public GameObject LobbyPanel;
    public bool inLobby = false;
    public bool isAuthenticated = false;
    public bool isEveryoneReady = false;
    public Lobby currentLobby;
    public LobbyManager lobbyManager;

    private void Start()
    {
        MainMenuPanel.SetActive(false);
        WaitingForPlayer.SetActive(false);
        LobbyPanel.SetActive(true);
    }
    public void RefreshPlayersList(List<Player> players)
    {
        //remove all previous children
        foreach (Transform child in playerLists.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Player player in players)
        {
            PlayerElement playerEm = Instantiate(playerElement, playerLists.transform);
            playerEm.InitializeValues(player.Data["PlayerName"].Value, player.Id, player.Data["Status"].Value);
        }
    }

    public void RefreshServersList(List<Lobby> lobbies)
    {
        //remove all previous children
        foreach (Transform child in serverLists.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Lobby lobby in lobbies)
        {
            LobbyElement lobbyEm = Instantiate(lobbyElement, serverLists.transform);
            lobbyEm.InitializeValues(lobby.Name, lobby.LobbyCode, string.Format("{0}/{1}", lobby.Players.Count, lobby.MaxPlayers));
        }
    }

    public void JoinLobbyByCode(string lobbyCode)
    {
        lobbyManager.JoinLobby(lobbyCode);
    }


    public void EnterLobby(Lobby lobby)
    {
        serverLists.transform.parent.gameObject.SetActive(false);
        playerLists.transform.parent.gameObject.SetActive(true);
        CreateHostBtn.gameObject.SetActive(false);
        StopHostBtn.gameObject.SetActive(true);
        lobbyNameTxt.SetText("Lobby : "+lobby.Name);
    }

    public void ExitLobby()
    {
        serverLists.transform.parent.gameObject.SetActive(true);
        playerLists.transform.parent.gameObject.SetActive(false);
        CreateHostBtn.gameObject.SetActive(true);
        StopHostBtn.gameObject.SetActive(false);
        lobbyNameTxt.SetText("Lobby");
    }

    public void StartGame()
    {
        if (isAuthenticated && inLobby && isEveryoneReady) {
            SceneManager.LoadScene(2, LoadSceneMode.Single);
        }
    }

    public void Update()
    {
        if(inLobby)
        {
            EnterLobby(currentLobby);
        }
        else
        {
            ExitLobby();
        }
    }
}
