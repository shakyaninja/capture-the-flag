using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassFlag : MonoBehaviour
{
    [SerializeField]private bool isPassable = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision != null && collision.gameObject.CompareTag("Player") && isPassable)
        {
            Transform flag = collision.transform.GetChild(0);
            flag.SetParent(gameObject.transform);
            flag.localPosition = Vector3.zero + new Vector3(0,3,0);
        }
    }
}
