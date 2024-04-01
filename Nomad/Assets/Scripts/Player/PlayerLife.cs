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
    [SerializeField] float hunger;
    float curHunger;
    [SerializeField] float hungerDecay;
    [Header("Inventory")]
    [SerializeField] GameObject healthyUiPrefab;
    [SerializeField] Transform healthParent;
    [SerializeField] float waterSkinSize = 10;
    [SerializeField] float waterSkinFill = 10;


    
    [Header("Paused")]
    public bool gamePaused;


    void Start()
    {
        curHealth = health;
        curHunger = hunger;
        UpdateHealthUI();
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
        UpdateHealthUI();

    }

    public void RestRestore()
    {
        curHealth = health;
        UpdateHealthUI();
    }
    #endregion

    #region UI

    void UpdateHealthUI()
    {
        int numChildren = healthParent.childCount;
        int intHealth = -(Mathf.FloorToInt(-health));
        if (numChildren != intHealth)
        {
            if (numChildren > intHealth)
            {
                //int target = numChildren - intHealth
                for (int i = numChildren - intHealth; i > 0; i--)
                {
                    //remove extra element
                    Destroy(healthParent.GetChild(0).gameObject);
                }
            }
            else
            {
                for (int i = intHealth- numChildren; i > 0; i--)
                {
                    //remove extra element
                    Instantiate(healthyUiPrefab, healthParent);
                    Debug.Log("add health element");
                }
            }
        }


    }


    #endregion
    


}
