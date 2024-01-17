using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;
    [SerializeField] LobbyUIManager lobbyUIManager;
    [SerializeField] List<string> randomNames = new List<string>(10) { "Hulk", "Ironman", "spiderman"};
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    public string playerName;
    private bool inLobby = false;
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            lobbyUIManager.isAuthenticated = true;
        };
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (AuthenticationException AuthException)
        {
            Debug.LogError(AuthException.Message);
        }
     
    }

    public void Refresh()
    {
        if (inLobby)
        {
            //refresh players
            showPlayersListInLobby();
        }
        else
        {
            //refresh lobbies
            ListAllServers();
        }
    }

    public void StopHost()
    {
        RemoveLobby();
    }
    public void Ready()
    {
        ReadyForGame();
    }

    public void Leave()
    {
        LeaveLobby();
        inLobby = false;
        lobbyUIManager.inLobby = false;
    }
    public async void ListAllServers()
    {
        await ListLobbies();
    }

    public async void QuickJoin()
    {
        await QuickJoinLobby();
        inLobby = true;
        showPlayersListInLobby();
        lobbyUIManager.inLobby = true;
        lobbyUIManager.currentLobby = joinedLobby;
    }

    public async void JoinLobby(string lobbyCode)
    {
        await JoinLobbyByCode(lobbyCode);
        inLobby = true;
        showPlayersListInLobby();
        lobbyUIManager.inLobby = true;
        lobbyUIManager.currentLobby = joinedLobby;
    }

    public async void CreateHost()
    {
        await CreateLobby();
        //show lobby state
        inLobby = true;
        showPlayersListInLobby();
        lobbyUIManager.inLobby = true;
        lobbyUIManager.currentLobby = hostLobby;
    }

    private void showPlayersListInLobby()
    {
        List<Player> playersInLobby = (hostLobby != null) ?hostLobby.Players:joinedLobby.Players;
        lobbyUIManager.RefreshPlayersList(playersInLobby);
    }

   

    private Player CreatePlayer(bool isHost = false)
    {
        Player player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, randomNames[Random.Range(0,randomNames.Count-1)]+" "+Random.Range(0,999))},
                    { "Status", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, isHost?"Host":"Member")}
                }
        };
        return player;
    }

    private async Task CreateLobby()
    {
        string lobbyName = "LobbyName";
        int maxPlayers = 10;
        CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions { 
            IsPrivate = false,
            IsLocked = false,
            Player = CreatePlayer(true),
           /* Data = new Dictionary<string, DataObject> //ex : can be used for searching lobby as per modes
            {
                {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Capture The Flag",DataObject.IndexOptions.S1) }
            }*/
        };
        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
        hostLobby = lobby;
        Debug.Log("Created Lobby ! " + lobby.Name + " " + lobby.MaxPlayers);
    }

    private async Task ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryOptions = new QueryLobbiesOptions {
                Count = 25, Filters = new List<QueryFilter> {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) ,
                    //new QueryFilter(QueryFilter.FieldOptions.S1, "Capture The Flag", QueryFilter.OpOptions.EQ) 
                },
                Order = new List<QueryOrder> { new QueryOrder(false,QueryOrder.FieldOptions.Created) }
            };
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            Debug.Log("Lobbies : " + queryResponse.Results.Count);
            lobbyUIManager.RefreshServersList(queryResponse.Results);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void Update()
    {
        //for making lobby stay active for more than 30 seconds even in no players state...
        HandleLobbyHeartbeat(); 
        //for polling the changes ion connected lobby
        HandleLobbyPollForUpdate();
    }
    private async void HandleLobbyHeartbeat()
    {
        if(hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0)
            {
                float heartbeatTimeMax = 15;
                heartbeatTimer = heartbeatTimeMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdate()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0)
            {
                float lobbyUpdateTimerMax = 1-1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }
        }
    }

    private async  Task JoinLobby()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async Task QuickJoinLobby()
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            joinedLobby = lobby;
            GameManager.Instance.activeLobby = lobby;
        }catch(LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async Task JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions {
                Player = CreatePlayer()
            };
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            PrintPlayers(lobby);
            joinedLobby = lobby;
            GameManager.Instance.activeLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby : " + lobby.Id);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id);
        }
    }

    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
                }
            });
            joinedLobby = hostLobby;
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }


    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {
                    "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)
                    }
                }
            }
            );
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }
   

    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    public async void KickPlayer(string playerId)
    {
        try
        {
            if(hostLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, playerId);
            }
        }catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    private async void RemoveLobby()
    {
        try
        {
            if(hostLobby != null)
            {
                await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
                GameManager.Instance.activeLobby = null;
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    private async void ReadyForGame()
    {
        try
        {
            //update player data for ready bool to true
            
        }catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }
}
