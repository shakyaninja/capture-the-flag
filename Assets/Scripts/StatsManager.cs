using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class StatsManager : NetworkBehaviour
{
    [SerializeField] private int Score;
    [SerializeField] private NetworkVariable<float> Timer = new NetworkVariable<float>();
    //[SerializeField]private float Timer = 0;

    override public void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Score = 0;
            Timer.Value = 0;
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

    public int GetScore()
    {
        return Score;
    }

    public void IncrementScore()
    {
        Score = Score + 1;
        GameManager.Instance.UpdateScoreOfPlayerServerRpc();
    }
}
