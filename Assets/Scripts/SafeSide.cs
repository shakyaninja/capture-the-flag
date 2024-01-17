using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SafeSide : NetworkBehaviour
{
    private float startTime = 0f;
    private float safeTime = 5f;
    private bool isInsideSafeArea = false;
    public bool hasFlag = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (hasFlag && isInsideSafeArea &&  Time.time > startTime + safeTime)
        {
            //safe side 
            Debug.Log("Scored !!");
            //GameManager.Instance.statsManager.IncrementScoreByPlayerId();
            isInsideSafeArea = false;
            startTime = 0;

            //reset flag
            Destroy(gameObject.transform.GetChild(0).gameObject);
            GameManager.Instance.RespawnFlag();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null && collision.gameObject.CompareTag("SafeArea"))
        {
            Debug.Log("inside safe area !!");
            startTime = Time.time;
            isInsideSafeArea = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision != null && collision.gameObject.CompareTag("SafeArea"))
        {
            Debug.Log("exited safe area !!");
            startTime = 0;
            isInsideSafeArea = false;
        }
    }
}
