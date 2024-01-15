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
    public GameObject serverLists;
    public TMP_Text lobbyTxt;
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    public string playerName;
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
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

    public async void CreateHost()
    {
        await CreateLobby();
    }

    public async void ListAllServers()
    {
        await ListLobbies();
    }

    public async void QuickJoin()
    {
        await QuickJoinLobby();
    }

    public async void JoinLobby(string lobbyCode)
    {
        await JoinLobbyByCode(lobbyCode);
    }

    private Player CreatePlayer()
    {
        Player player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName+Random.Range(0,99))}
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
            Player = CreatePlayer(),
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
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryOptions);
            Debug.Log("Lobbies : " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log("name : " + lobby.Name + " " + "code : " + lobby.LobbyCode + " " + "Max Players : " + lobby.MaxPlayers);
                lobbyTxt.SetText("name : " + lobby.Name + " " + "code : " + lobby.LobbyCode + " " + "Max Players : " + lobby.MaxPlayers);
                //lobbyTxt.transform.SetParent(ServerLists.transform);
                //ServerLists.GetComponent<VerticalLayoutGroup>().
            }
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
            await LobbyService.Instance.QuickJoinLobbyAsync();
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
        }catch(LobbyServiceException e)
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

    private async void KickPlayer(string playerId)
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
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }
}
