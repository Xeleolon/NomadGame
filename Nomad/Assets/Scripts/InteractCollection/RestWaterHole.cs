using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestWaterHole : InteractBase
{
    PlayerLife playerLife;
    [SerializeField] Vector3 spawnPoint;
    [SerializeField] bool resetSpawn = true;
    void OnValidate()
    {
        if (resetSpawn)
        {
            spawnPoint = transform.position;
            resetSpawn = false;
        }
    }
    void Start()
    {
        playerLife = PlayerLife.instance;
    }
    public override void Interact()
    {
        if (playerLife != null)
        {
            Debug.Log("Player Resting, Health Restored");
            playerLife.RestRestore(spawnPoint);
        }
    }

    public virtual void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(spawnPoint, new Vector3(1, 2, 1));
    }
}
