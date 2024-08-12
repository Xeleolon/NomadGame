using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStages : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;

    private int stage = 1;

    private enum StageProgress { inactive, actack1, actack2, actack3, exposed }
    StageProgress stageProgress = StageProgress.inactive;

    void Start()
    {
        stageProgress = StageProgress.actack1;
    }


    void Update()
    {
        switch (stage)
        {
            case 1:
                StageOne();
            break;

            case 2:
                StageTwo();
            break;

            case 3:
                StageThree();
            break;
        }
    }

    void StageOne()
    {
        switch (stageProgress)
        {
            case StageProgress.actack1:
                //play animation for projectile
                stageProgress = StageProgress.inactive;

                break;

            case StageProgress.exposed:
                //player animation for exposing weakness
                stageProgress = StageProgress.inactive;

                break;
        }
    }

    void StageTwo()
    {
        switch (stageProgress)
        {
            case StageProgress.actack1:
                //play animation for projectile
                stageProgress = StageProgress.inactive;

                break;

            case StageProgress.exposed:
                //player animation for exposing weakness
                stageProgress = StageProgress.inactive;

                break;
        }
    }

    void StageThree()
    {
        switch (stageProgress)
        {
            case StageProgress.actack1:
                //play animation for projectile
                stageProgress = StageProgress.inactive;

                break;

            case StageProgress.exposed:
                //player animation for exposing weakness
                stageProgress = StageProgress.inactive;

                break;
        }
    }

    public void FireProjectile()
    {
        

    }
}
