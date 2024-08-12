using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpTorch : InteractBase
{
    [Header("Pick Up Torch")]
    [Tooltip("0 = no torch, 2 = lit torch, 3 = drenched torch.")]
    
    [SerializeField] private PlayerLife.TorchStates startState = PlayerLife.TorchStates.unlit;
    [SerializeField] private bool Unpickable;
    [SerializeField] private GameObject lightSource;
    [SerializeField] private string pickUpStatement;
    [SerializeField] private string lightStatement;
    [SerializeField] private string unDrenchStatement;

    private PlayerLife.TorchStates lightState = PlayerLife.TorchStates.unlit;
    private bool skipStartLight;
    private PlayerLife playerLife;
    void Start()
    {
        if (!skipStartLight)
        {
            ChangeState(startState);
        }
        playerLife = PlayerLife.instance;
    }

    public override void Interact()
    {
        if (!Unpickable && door == null && playerLife.AddTorch(lightState))
        {
            //Debug.Log("pick up torch");
            Destroy(gameObject);
        }
        else //take effect from light source
        {
            TorchStaticInteraction();
        }
    }

    public override bool Requirements()
    {
        PlayerLife.ToolType curTool = playerLife.curTool;
        PlayerLife.TorchStates torchState = playerLife.torchState;

        if (curTool != PlayerLife.ToolType.torch)
        {
            if (playerLife.torchInfo.locked || torchState == PlayerLife.TorchStates.empty)
            {
                //torch no they
                displayInstructions = pickUpStatement;
                return true;
            }

            return false;
        }
        if (torchState == PlayerLife.TorchStates.lit && (startState == PlayerLife.TorchStates.unlit || startState == PlayerLife.TorchStates.drenched))
        {
            //this torch unlit
            displayInstructions = lightStatement;
            return true;
        }
        else if (torchState == PlayerLife.TorchStates.unlit && startState == PlayerLife.TorchStates.lit)
        {
            //player torch unlit
            displayInstructions = lightStatement;
            return true;
        }
        else if (torchState == PlayerLife.TorchStates.drenched && startState == PlayerLife.TorchStates.lit)
        {
            //torch undrenched
            displayInstructions = unDrenchStatement;
            return true;
        }

        return false;
    }

    void TorchStaticInteraction()
    {
         if (playerLife.curTool == PlayerLife.ToolType.torch)
         {
            switch (playerLife.torchState)
            {
                case PlayerLife.TorchStates.unlit:
                if (lightState == PlayerLife.TorchStates.lit)
                {
                    playerLife.changeTorch(lightState);
                }
                break;
                case PlayerLife.TorchStates.lit:
                if (lightState == PlayerLife.TorchStates.unlit)
                {
                    //Debug.Log("Changing to light");
                    ChangeState(PlayerLife.TorchStates.lit);
                    OpenDoor();
                }
                else if (lightState == PlayerLife.TorchStates.drenched)
                {
                    //Debug.Log("Changing to off");
                    ChangeState(PlayerLife.TorchStates.unlit);
                }
                break;
                case PlayerLife.TorchStates.drenched:
                if (lightState == PlayerLife.TorchStates.lit)
                {
                    playerLife.changeTorch(PlayerLife.TorchStates.unlit);
                }
                break;
            }
         }
    }

    public void ChangeState(PlayerLife.TorchStates newState)
    {
        skipStartLight = true;
        //Debug.Log("made to stage 1 : " + newState);
        if (lightState != newState)
        {
            switch (newState)
            {
                case PlayerLife.TorchStates.empty:
                lightState = PlayerLife.TorchStates.unlit;
                break;

                case PlayerLife.TorchStates.unlit:
                lightState = PlayerLife.TorchStates.unlit;
                //Debug.Log("offcourse");
                if (lightSource != null && lightSource.activeSelf)
                {
                    lightSource.SetActive(false);
                }
                break;

                case PlayerLife.TorchStates.lit:
                //Debug.Log("Made to stage 2");
                if (lightState != PlayerLife.TorchStates.drenched)
                {
                    //Debug.Log("Made to stage 3");
                    lightState = PlayerLife.TorchStates.lit;
                    //turn light on
                    if (lightSource != null && !lightSource.activeSelf)
                    {
                        //Debug.Log("Made to stage 4");
                        lightSource.SetActive(true);
                    }
                }
                break;

                case PlayerLife.TorchStates.drenched:
                lightState = PlayerLife.TorchStates.drenched;
                if (lightSource != null && lightSource.activeSelf) // turn light off
                {
                    lightSource.SetActive(false);
                }
                break;
            }
        }
    }

}
