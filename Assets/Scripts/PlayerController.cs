using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Camera PlayerCamera;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!IsOwner)
        {
            PlayerCamera.enabled = false;
            return;
        }
        PlayerCamera.enabled = true;
        GameManager.Instance.RegisterClientAsPlayerServerRpc();
    }
    void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.W))
            {
                transform.position -= transform.right * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position -= transform.forward * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position += transform.right * moveSpeed * Time.deltaTime;
            }
            if(Input.GetKey(KeyCode.Space))
            {
                transform.position += transform.up * jumpForce * Time.deltaTime;
            }
        }
    }
}
