using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Captured : MonoBehaviour
{
    [SerializeField] private Vector3 TopOffset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other != null && other.CompareTag("Player"))
        {
            gameObject.transform.SetParent(other.gameObject.transform);
            gameObject.transform.localPosition = Vector3.zero + TopOffset;
            gameObject.transform.localRotation = Quaternion.identity;   
            other.GetComponent<SafeSide>().hasFlag = true;
        }
    }
}
