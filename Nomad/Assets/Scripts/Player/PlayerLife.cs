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
    private Vector3 spawnPoint;


    
    [Header("Paused")]
    public bool gamePaused;


    void Start()
    {
        spawnPoint = transform.position;
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
            //AlterHealth(-(0.2f * Time.deltaTime));
        }
    }

    public void AlterHealth(float temp)
    {
        curHealth += temp;
        //Debug.Log("Player recieved damage");

        if (curHealth <= 0)
        {
            Debug.Log("Player Died");
            Respawn();
        }
        else if (curHealth > health)
        {
            curHealth = health;
        }
        UpdateHealthUI();

    }
    void Respawn()
    {
        curHealth = health;
        transform.position = spawnPoint;
    }

    public void RestRestore(Vector3 newPosition)
    {
        spawnPoint = newPosition;
        curHealth = health;
        UpdateHealthUI();
    }
    #endregion

    #region UI

    void UpdateHealthUI()
    {
        int numChildren = healthParent.childCount;
        int intHealth = -(Mathf.FloorToInt(-curHealth));
        //Debug.Log(numChildren + " " + intHealth + " " + curHealth);
        if (numChildren != intHealth)
        {
            if (numChildren > intHealth)
            {
                Debug.Log("removing ui element");
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
