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
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;   
    }

    // Update is called once per frame
    void FixedUpdate()
    {

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
