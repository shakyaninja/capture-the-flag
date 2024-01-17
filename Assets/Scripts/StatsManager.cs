using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class StatsManager : NetworkBehaviour
{
    [SerializeField] private List<PlayersScoreData> playersScore;
    [SerializeField] private NetworkVariable<float> Timer = new NetworkVariable<float>();

    public struct PlayersScoreData
    {
        public Player player;
        public int score;

        internal void SetScore(int v)
        {
            score = v;
        }
    }
    override public void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Timer.Value = 0;
            playersScore.Clear();
            playersScore = new List<PlayersScoreData>();
            GameManager.Instance.activeLobby.Players.ForEach(p =>
            {
                PlayersScoreData playerInfo = new PlayersScoreData
                {
                    player = p,
                    score = 0
                };
                playersScore.Add(playerInfo);
            });
        }
    }
    private void Update()
    {
        if (Timer.Value <= 60 && IsServer)
        {
            StartTimer();
        }
    }

    private void StartTimer()
    {
        Timer.Value += Time.deltaTime;
    }

    public int GetTimer()
    {
        return (int)Math.Floor(Timer.Value);
    }

    public int GetScoreByPlayerId(string playerId)
    {
        foreach (PlayersScoreData playerScore in playersScore)
        {
            if(playerScore.player.Id == playerId)
            {
                return playerScore.score;
            }
        }
        return 0;
    }

    public void IncrementScoreByPlayerId(string playerId)
    {
        for (int i = 0;i< playersScore.Count;i++)
        {
            if (playersScore[i].player.Id == playerId)
            {
                playersScore[i].SetScore(playersScore[i].score + 1);
            }
        }
        GameManager.Instance.UpdateScoreOfPlayerServerRpc();
    }
}
