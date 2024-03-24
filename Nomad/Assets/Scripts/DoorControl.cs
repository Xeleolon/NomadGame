using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorControl : MonoBehaviour
{
    public bool Locked;
    bool DoorOpened;
    [SerializeField] bool testsDoor;

    Animator animator;
    [SerializeField] string openingAnimation;
    [SerializeField] string closingAnimation;
    [Range(1, 10)]
    [SerializeField] int numLocks = 1;
    int currentNumLock;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (numLocks <= 0)
        {
            numLocks = 1;
        }
        currentNumLock = numLocks;
    }

    void FixedUpdate()
    {
        if (testsDoor)
        {
            OpenDoor(0);
            testsDoor = false;
        }

    }
    

    public bool OpenDoor(int Open)
    {
        switch (Open)
        {
            case 0:
            if (!Locked)
            {
                if (DoorOpened)
                {
                    ClosingDoor();
                    return true;
                }
                else
                {
                    currentNumLock -= 1;
                    if (!Locked && currentNumLock <= 0)
                    {
                        OpeningDoor();
                        return true;
                    }
                }
            }
            break;

            case 1:
            currentNumLock -= 1;
            if (!Locked && currentNumLock <= 0)
            {
                OpeningDoor();
                return true;
            }
            break;

            case 2:
            ClosingDoor();
            return true;
            break;

            case 3:
            if (!DoorOpened)
            {
                Debug.Log("Reseting Lock");
                currentNumLock = numLocks;
                return true;
            }
            break;

            default:
            Debug.LogError("Error");
            break;
        }
        return false;

    }

    void OpeningDoor()
    {
        if (!DoorOpened && animator != null && openingAnimation != null)
        {
            animator.Play(openingAnimation);
            DoorOpened = true;
        }
        //Debug.Log(DoorOpened + " " + animator + " " + openingAnimation);
    }

    void ClosingDoor()
    {
        if (DoorOpened && animator != null && closingAnimation != null)
        {
            animator.Play(closingAnimation);
            DoorOpened = false;
        }
        //Debug.Log(DoorOpened + " " + animator + " " + closingAnimation);
    }
}
