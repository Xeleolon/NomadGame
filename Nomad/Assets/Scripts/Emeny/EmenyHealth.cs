using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmenyHealth : MonoBehaviour
{
    [SerializeField] private float health = 3;
    private float curHealth;
    [SerializeField] GameObject remainsPrefab;
    public bool skellyType;
    private void Start()
    {
    
        if (skellyType)
        {
            LevelManager.instance.onSunRiseCallback += Killed;
        }
        curHealth = health;
    }

    private void Update()
    {
        
    }
    private void Killed()
    {
        Instantiate(remainsPrefab, transform.position, Quaternion.identity);
        if (skellyType)
        {
            LevelManager.instance.onSunRiseCallback -= Killed;
        }
        Destroy(gameObject);
    }

    public void Damage(float damage)
    {
        if (health > 0)
        {
            health -= damage;
            Debug.Log(gameObject.name + " lost " + damage + " damage health now " + health);
            if (health <= 0)
            {
                Killed();
            }
        }
        else
        {
            Debug.LogError("!!!attemping to apply damage when zero health!!!");
        }
    }
}
