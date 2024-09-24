using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    Vector3 spawnOffset;
    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerLife.instance.SetCheckPoint(transform.position + spawnOffset);
        }
    }

    public virtual void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + spawnOffset, new Vector3(1, 2, 1));
    }
}
