using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTT : MonoBehaviour
{
    public int a;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        if(Physics.Raycast(transform.position, new Vector3(0, 10, 0))) {
            Debug.Log("nnnn");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, 10, 0));
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(a+"---Collider");

    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(a + "---Trigger");

    }

}
