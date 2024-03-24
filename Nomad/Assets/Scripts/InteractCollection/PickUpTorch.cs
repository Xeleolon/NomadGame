using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpTorch : InteractBase
{
    [Header("Pick Up Torch")]
    [Tooltip("0 = no torch, 2 = lit torch, 3 = drenched torch.")]
    [Range(1, 3)]
    [SerializeField] private int startState = 1;
    [SerializeField] private bool Unpickable;
    [SerializeField] private GameObject lightSource;
    private int lightState;
    private bool skipStartLight;
    void Start()
    {
        if (!skipStartLight)
        {
            ChangeState(startState);
        }
    }

    public override void Interact()
    {
        if (!Unpickable && door == null && ToolsInventory.instance.AddTorch(lightState))
        {
            Debug.Log("pick up torch");
            ToolsInventory.instance.InteractCheck(false);
            Destroy(gameObject);
        }
        else //take effect from light source
        {
            TorchStaticInteraction();
        }
    }

    void TorchStaticInteraction()
    {
         if (ToolsInventory.instance.curTool == 1)
         {
            switch (ToolsInventory.instance.torchState)
            {
                case 1:
                if (lightState == 2)
                {
                    ToolsInventory.instance.changeTorch(lightState);
                }
                break;
                case 2:
                if (lightState == 1)
                {
                    Debug.Log("Changing to light");
                    ChangeState(2);
                    OpenDoor();
                }
                else if (lightState == 3)
                {
                    Debug.Log("Changing to off");
                    ChangeState(1);
                }
                break;
                case 3:
                if (lightState == 2)
                {
                    ToolsInventory.instance.changeTorch(1);
                }
                break;
            }
         }
    }

    public void ChangeState(int newState)
    {
        skipStartLight = true;
        Debug.Log("made to stage 1 : " + newState);
        if (lightState != newState)
        {
            switch (newState)
            {
                case 0:
                lightState = 1;
                break;

                case 1:
                lightState = 1;
                Debug.Log("offcourse");
                if (lightSource != null && lightSource.activeSelf)
                {
                    lightSource.SetActive(false);
                }
                break;

                case 2:
                //Debug.Log("Made to stage 2");
                if (lightState != 3)
                {
                    //Debug.Log("Made to stage 3");
                    lightState = 2;
                    //turn light on
                    if (lightSource != null && !lightSource.activeSelf)
                    {
                        //Debug.Log("Made to stage 4");
                        lightSource.SetActive(true);
                    }
                }
                break;

                case 3:
                lightState = 3;
                if (lightSource != null && lightSource.activeSelf) // turn light off
                {
                    lightSource.SetActive(false);
                }
                break;
            }
        }
    }

}
