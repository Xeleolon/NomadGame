using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugController : MonoBehaviour
{
    [SerializeField] private bool freazeGame;
    private bool curFreaze;

    void Update()
    {
        if (freazeGame != curFreaze)
        {
            LevelManager.instance.FreezeGame(freazeGame);
            curFreaze = freazeGame;
        }
    }
}
