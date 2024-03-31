using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestWaterHole : InteractBase
{
    PlayerLife playerLife;
    void Start()
    {
        playerLife = PlayerLife.instance;
    }
    public override void Interact()
    {
        if (playerLife != null)
        {
            Debug.Log("Player Resting, Health Restored");
            playerLife.RestRestore();
        }
    }
}
