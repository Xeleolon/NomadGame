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

    void Start()
    {
        animator = GetComponent<Animator>();
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
                    OpeningDoor();
                    return true;
                }
            }
            break;

            case 1:
            if (!Locked)
            {
                OpeningDoor();
                return true;
            }
            break;

            case 2:
            ClosingDoor();
            return true;
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
