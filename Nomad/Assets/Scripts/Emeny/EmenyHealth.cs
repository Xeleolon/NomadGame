using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmenyHealth : MonoBehaviour
{
    [SerializeField] private float health = 3;
    [SerializeField] private bool dayCreature;
    [SerializeField] private bool disableDayCycle;
    private float curHealth;
    [SerializeField] GameObject remainsPrefab;
    [SerializeField] GameObject particaleEffectPrefab;
    [SerializeField] private GameObject damageParticlePrefab;
    public SkellyMovement movementScript;
    public BaseEmenyMovement hornedCharger;
    private Vector3 spawn;
    [SerializeField] Renderer renderer;
    Collider collider;

    public bool testKill;
    public bool testReset;
    
    private void Start()
    {
        if (!disableDayCycle)
        {
            if(dayCreature)
            {
                LevelManager.instance.onSunSetCallback += dayNightShift;
            }
            else
            {
                LevelManager.instance.onSunRiseCallback += dayNightShift;
            }
        }
        spawn = transform.position;
        LevelManager.instance.onResetRespawn += Reset;
        curHealth = health;
    }

    private void Update()
    {
        if (testKill)
        {
            Killed();
            testKill = false;
        }

        if (testReset)
        {
            Reset();
            testReset = false;
        }
    }
    private void Killed()
    {
        if (remainsPrefab != null && SpawnCylce.instance.makeSpace(true))
        {
            GameObject temp = Instantiate(remainsPrefab, transform.position, Quaternion.identity);
            SpawnCylce.instance.AddSpawnPoint(temp, true);
        }

        if (hornedCharger != null)
        {
            hornedCharger.DisableMovement(true);
        }
        
        if (!dayCreature)
        {
            LevelManager.instance.onSunRiseCallback -= Killed;
        }
        //Destroy(gameObject);
        transform.position = spawn;
        CheckRendererBoxCollider();
        renderer.enabled = false;
        collider.enabled = false;

    }
    private void Reset()
    {
        if (hornedCharger != null)
        {
            hornedCharger.DisableMovement(false);
        }
        transform.position = spawn;
        CheckRendererBoxCollider();
        renderer.enabled = true;
        collider.enabled = true;
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
            if (damageParticlePrefab != null)
            {
                Instantiate(damageParticlePrefab, transform.position, transform.rotation);
            }
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

    void CheckRendererBoxCollider()
    {
        if (renderer == null)
        {
            renderer = gameObject.GetComponent<Renderer>();
        }
        if (collider == null)
        {
            collider = gameObject.GetComponent<Collider>();
        }
    }
}
