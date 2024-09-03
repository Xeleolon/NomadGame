using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmenyHealth : MonoBehaviour
{
    [SerializeField] private float health = 3;
    [SerializeField] private bool dayCreature;
    private float curHealth;
    [SerializeField] GameObject remainsPrefab;
    [SerializeField] GameObject particaleEffectPrefab; 
    public SkellyMovement movementScript;
    private void Start()
    {

        if(dayCreature)
        {
            LevelManager.instance.onSunSetCallback += dayNightShift;
        }
        else
        {
            LevelManager.instance.onSunRiseCallback += dayNightShift;
        }
        curHealth = health;
    }

    private void Update()
    {
        
    }
    private void Killed()
    {
        if (remainsPrefab != null && SpawnCylce.instance.makeSpace(true))
        {
            GameObject temp = Instantiate(remainsPrefab, transform.position, Quaternion.identity);
            SpawnCylce.instance.AddSpawnPoint(temp, true);
        }
        
        if (!dayCreature)
        {
            LevelManager.instance.onSunRiseCallback -= Killed;
        }
        Destroy(gameObject);
    }
    public void ForceDeath()
    {
        health = 0;
        Killed();
    }

    public virtual void Damage(float damage)
    {
        if (health > 0)
        {
            health -= damage;
            Debug.Log(gameObject.name + " lost " + damage + " damage health now " + health);
            if (health <= 0)
            {
                Killed();
            }
            else if (movementScript != null)
            {
                movementScript.TakeDamage();
            }
        }
        else
        {
            Debug.LogError("!!!attemping to apply damage when zero health!!!");
        }
    }

    void dayNightShift()
    {
        LevelManager.instance.onSunRiseCallback -= dayNightShift;
        LevelManager.instance.onSunSetCallback -= dayNightShift;
        if (dayCreature)
        {
            if (particaleEffectPrefab != null)
            {
                Instantiate(particaleEffectPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
        else
        {
            if (particaleEffectPrefab != null)
            {
                Instantiate(particaleEffectPrefab, transform.position, Quaternion.identity);
            }
            Killed();
        }
    }
}
