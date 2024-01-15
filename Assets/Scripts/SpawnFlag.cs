using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnFlag : NetworkBehaviour
{
    [SerializeField] private Transform[] FlagSpawnpositions;
    [SerializeField] private GameObject Flag;

    public void spawnFlag()
    {
        Debug.Log("spawn...");
        int randomIndex = Random.Range(0, FlagSpawnpositions.Length - 1);
        GameObject flag = Instantiate(Flag, FlagSpawnpositions[randomIndex].position, FlagSpawnpositions[randomIndex].rotation);
        flag.GetComponent<NetworkObject>().Spawn();
    }


}
