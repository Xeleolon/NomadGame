using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealth : EmenyHealth
{
    [SerializeField] float healthStage1;
    [SerializeField] float healthStage2;
    [SerializeField] float healthStage3;

    [SerializeField] BossStages bossStages;

    private Animator animator;

    [SerializeField] private string heartDamage;


    int stage = 1;

    private float healthBoss;
    private void Start()
    {
        animator = GetComponent<Animator>();
        healthBoss = healthStage1 + healthStage2 + healthStage3;
    }
    public override void Damage(float damage)
    {
        Debug.Log("Taking Hit");
        animator.Play(heartDamage);
        switch(stage)
        {
            case 1:
                healthBoss -= damage;
                if (healthBoss <= healthStage2 + healthStage3)
                {
                    healthBoss = healthStage2 + healthStage3;
                    Debug.Log("Stage 2");
                    //call next stage.
                    bossStages.StageDefeated();
                    stage = 2;
                }

            return;
            case 2:
                healthBoss -= damage;
                if (healthBoss <= healthStage3)
                {
                    healthBoss = healthStage3;
                    //call next stage.
                    Debug.Log("Stage 3");
                    bossStages.StageDefeated();
                    stage = 3;
                }

            return;
            case 3:
                healthBoss -= damage;
                if (healthBoss <= 0)
                {
                    healthBoss = 0;
                    bossStages.StageDefeated();
                    //call death result
                    Debug.Log("Killed Boss");
                }

            return;


        }
    }
}
