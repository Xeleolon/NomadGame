using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    #region Singleton
    public static LevelManager instance;
    void Awake ()
    {
        if (instance != null)
        {
            Debug.LogWarning("More Than One instance of LevelManager found!");
        }
        instance = this;
    }

    #endregion
    private bool gameFrozen;
    public delegate void onGameFreeze();
    public onGameFreeze freezeGame;
    public onGameFreeze unFreezeGame;

    //[Header("DayNightCycle")]
    public delegate void onDayNightShift();

    public onDayNightShift onSunSetCallback;
    public onDayNightShift onSunRiseCallback;

    public enum ResetStates {checkpoint, respawn}
    public delegate void onResetDelegate();
    public onResetDelegate onResetCheckPoint;
    public onResetDelegate onResetRespawn;
    public bool disableCursorLockMode;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        if (!disableCursorLockMode)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        onResetCheckPoint += DebugLogReset;
        onResetRespawn += DebugLogReset;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    void DebugLogReset()
    {
        Debug.Log("resetting Level");
    }

    

    public void DayNightShift(bool nightShift)
    {
        if (!nightShift)
        {
            if (onSunRiseCallback != null)
            {
                onSunRiseCallback.Invoke();
            }
            
            Debug.Log("Invoking Day time");
        }
        else
        {
            if (onSunSetCallback != null)
            {
                onSunSetCallback.Invoke();
            }
            
            Debug.Log("Invoking Night time");
        }
    }

    public void ResetTo(ResetStates resetState)
    {
        switch (resetState)
        {
            case ResetStates.checkpoint:
            onResetCheckPoint.Invoke();
            return;

            case ResetStates.respawn:
            onResetRespawn.Invoke();
            return;

        }
    }

    public void FreezeGame(bool freeze)
    {
        gameFrozen = freeze;
        if (gameFrozen)
        {
            freezeGame.Invoke();
        }
        else
        {
            unFreezeGame.Invoke();
        }
    }

}
