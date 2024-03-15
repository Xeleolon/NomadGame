using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLife : MonoBehaviour
{

    #region Awake
    public static PlayerLife instance;
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("more than one player life!");
        }
        instance = this;
    }
    #endregion


    [Header("Health")]
    [SerializeField] bool freazeDecays;
    [SerializeField] float health;
    float curHealth;
    [SerializeField] float thirst;
    float curThrist;
    [SerializeField] float thirstDecay;
    [SerializeField] float hunger;
    float curHunger;
    [SerializeField] float hungerDecay;
    [Header("Inventory")]
    [SerializeField] float waterSkinSize = 10;
    [SerializeField] float waterSkinFill = 10;


    
    [Header("Paused")]
    public bool gamePaused;


    void Start()
    {
        curHealth = health;
        curHunger = hunger;
        curThrist = thirst;
    }

    void Update()
    {

        if (!gamePaused)
        {
            if (!freazeDecays)
            {
                SurvivalDecays();
            }
        }

    }
    #region Survial

    void SurvivalDecays()
    {
        if (curThrist > 0)
        {
            curThrist -= thirstDecay * Time.deltaTime;
        }
        else
        {
            AlterHealth(0.2f * Time.deltaTime);
        }

        if (curHunger > 0)
        {
            curHunger -= hungerDecay * Time.deltaTime;
        }
        else
        {
            AlterHealth(0.2f * Time.deltaTime);
        }
    }

    public void AlterHealth(float temp)
    {
        curHealth += temp;

        if (curHealth <= 0)
        {
            Debug.Log("Player Died");
        }
        else if (curHealth > health)
        {
            curHealth = health;
        }

    }
    #endregion
    


}
