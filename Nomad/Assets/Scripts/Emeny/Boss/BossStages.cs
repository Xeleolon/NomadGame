using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStages : MonoBehaviour
{
    [SerializeField] private GameObject fireBallPrefab;
    [SerializeField] private GameObject metalBallPrefab;

    private GameObject player;

    private int stage = 1;
    [SerializeField] private string stage2Start;
    [SerializeField] private string stage3Start;
    [SerializeField] private string destruction;
    [SerializeField] private string disbleBody;
    
    
    private Animator animator;

    public enum ProjectileTypes {fireBall, metalBall}

    private bool torchTrigger;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player");
        torchTrigger = false;
    }


    public void FireProjectile(ProjectileTypes projectileType)
    {
        Debug.Log("Boss Firing Projecile");
        Vector3 targetDesitatin = player.transform.position;

        switch (projectileType)
        {
            case ProjectileTypes.fireBall:
                //cast fireball
                Quaternion rotation = Quaternion.LookRotation(player.transform.position + Vector3.up * 0.5f, Vector3.up);

                Instantiate(fireBallPrefab, transform.position, rotation);
                break;

            case ProjectileTypes.metalBall:

                break;

        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PickUpTorch>() != null && torchTrigger)
        {
            Debug.Log("Disabling Body " + other.name);
            animator.Play(disbleBody);

            torchTrigger = false;
        }
    }

    public void EnableTorchTrigger()
    {
        torchTrigger = true;
    }

    public void DisableTorchTrigger()
    {
        torchTrigger = false;
    }

    public void StageDefeated()
    {
        switch(stage)
        {
            case 1:
                stage = 2;
                Debug.Log("Play stage 2 animations");
                if (stage2Start != null)
                {
                    animator.Play(stage2Start);
                }
            break;

            case 2:
                stage = 3;
                Debug.Log("Play stage 3 animations");
                if (stage3Start != null)
                {
                    animator.Play(stage3Start);
                }
            break;

            case 3:
                Debug.Log("Play stage destruction animations");
                if (destruction != null)
                {
                    animator.Play(destruction);
                }
            break;
        }
    }
}
