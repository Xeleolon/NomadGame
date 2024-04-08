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

    [Header("DayNightCycle")]
    [SerializeField] float dayLength = 10;
    private float dayClock;
    public bool freazeDayCycle;
    private bool nightBool;
    public delegate void onDayNightShift();
    public Light sunSource;
    public Vector2 lightRange = new Vector2(232,65000);

    public onDayNightShift onSunSetCallback;
    public onDayNightShift onSunRiseCallback;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        dayClock = 0;   
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!freazeDayCycle)
        {
            DayNightCycle();
        }
    }

    void DayNightCycle()
    {
        if (dayClock >= dayLength)
        {
            dayClock = 0;
            DayNightShift();
        }
        else
        {
            dayClock += 1 * Time.deltaTime;
        }
    }

    private void DayNightShift()
    {
        if (nightBool)
        {
            if (onSunRiseCallback != null)
            {
                onSunRiseCallback.Invoke();
            }
            if (sunSource != null)
            {
                sunSource.intensity = lightRange.y;
            }
            nightBool = false;
            Debug.Log("Day time");
        }
        else
        {
            if (onSunSetCallback != null)
            {
                onSunSetCallback.Invoke();
            }
            if (sunSource != null)
            {
                sunSource.intensity = lightRange.x;
            }
            nightBool = true;
            Debug.Log("Night time");
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

    public void AlterEmenyBallance()
    {
        //increase amount of emeny spawn during night vs day
    }
}
