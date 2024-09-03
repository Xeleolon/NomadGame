using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerLife : MonoBehaviour
{
    [System.Serializable]
public class ToolInfo
{
    public bool locked;
    public GameObject heldRefernce;
    public Sprite assignedIcon;
    public string[] animtionNames;

    public void ActivateObject(bool activate)
    {
        if (heldRefernce != null)
        {
            if (activate && !heldRefernce.activeSelf)
            {
                heldRefernce.SetActive(true);
            }
            else if (!activate && heldRefernce.activeSelf)
            {
                heldRefernce.SetActive(false);
            }
        }

    }
}

    #region Awake & Inputs
    public static PlayerLife instance;
    private PlayerInputActions playerControls;
    private InputAction strike;
    private InputAction holdStrike;
    private InputAction inputToolA;
    private InputAction inputToolB;
    private InputAction inputToolC;
    private InputAction openMenu;
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("more than one player life!");
        }
        instance = this;
        playerControls = new PlayerInputActions();
    }

    void OnEnable()
    {
        strike = playerControls.Player.Fire;
        strike.Enable();

        holdStrike = playerControls.Player.Fire2;
        holdStrike.Enable();

        inputToolA = playerControls.Player.ToolA;
        inputToolA.Enable();
        inputToolA.performed += ToolASelect;

        inputToolB = playerControls.Player.ToolB;
        inputToolB.Enable();
        inputToolB.performed += ToolBSelect;

        inputToolC = playerControls.Player.ToolC;
        inputToolC.Enable();
        inputToolC.performed += ToolCSelect;

        openMenu = playerControls.Player.OpenMenu;
        openMenu.Enable();
        openMenu.performed += MenuInput;

    }

    void OnDisable()
    {
        strike.Disable();
        holdStrike.Disable();
        inputToolA.Disable();
        inputToolB.Disable();
        inputToolC.Disable();
        openMenu.Disable();
    }
    #endregion

    #region Valaribles
    [Header("Health")]
    [SerializeField] bool freazeDecays;
    [SerializeField] float health;
    float curHealth;
    [SerializeField] float hunger;
    float curHunger;
    [SerializeField] float hungerDecay;
    
    public enum CollectableItemType {coin, arrow, fobiddenCoin}
    [Header("Inventory")]
    [SerializeField] int coins = 0;

    [SerializeField] int arrows = 10;
    [SerializeField] int maxArrows = 20;
    [SerializeField] int fobiddenCoin = 0;
    [SerializeField] Transform mainCamera;
    [SerializeField] CameraMovement cameraMovementScript;


    public enum ToolType {empty, spear, torch, bow, rope}

    [Header("ToolSelection")]

    [SerializeField] Transform playerBody;
    [SerializeField] ToolType toolA;
    [SerializeField] ToolType toolB;

    [SerializeField] ToolType toolC;
    [SerializeField] public ToolType curTool = ToolType.empty;
    [SerializeField] Animator toolsAnimator;
    private InteractionTrigger interactTrigger;

    [Header("Weapons")]
    //spear
    [SerializeField] WeaponItem spear;
    [SerializeField] ToolInfo spearInfo;
    bool canHarm = true;

    //Bow
    [SerializeField] RangedWeaponItem bow;
    [SerializeField] ToolInfo bowInfo;
    [SerializeField] GameObject arrowPrefab;
    private bool holdingFire;
    
    [Header("Tools")]
    //torch
    [SerializeField] public ToolInfo torchInfo;

    public enum TorchStates {empty, unlit, lit, drenched, attachingTorch, throwingTorch}
    [SerializeField] public TorchStates torchState = TorchStates.empty; //0 = no torch, 2 = lit torch, 3 = drenched torch.


    //throwing torch varables
    bool throwHold;
    [SerializeField] float throwPower = 3;
    [SerializeField] float throwUpwardForce = 1.5f;

    [SerializeField] bool infinteThrow;

    //
    [SerializeField] GameObject torchLight;
    [SerializeField] GameObject torchPrefab;

    [System.Serializable]
    public class UIVariables
    {
        public GameObject healthyUiPrefab;
        public Transform healthParent;
        public Animation toolanimation;
        public Image toolSlotA;
        public Image toolSlotB;
        public Image toolSlotC;
        public TMP_Text coinText;
        public TMP_Text fobiddenCoinText;
        public TMP_Text arrowsText; 
    }

    [Header("UI")]
    [SerializeField] UIVariables UI;
    [SerializeField] private GameObject menuUi;
    private GameObject healthyUiPrefab {get {return UI.healthyUiPrefab;} set {UI.healthyUiPrefab = value;}}
    private Transform healthParent {get {return UI.healthParent;} set {UI.healthParent = value;}}
    private Animation toolanimation {get {return UI.toolanimation;} set {UI.toolanimation = value;}}
    private Image toolSlotA {get {return UI.toolSlotA;} set {UI.toolSlotA = value;}}
    private Image toolSlotB {get {return UI.toolSlotB;} set {UI.toolSlotB = value;}}
    private Image toolSlotC {get {return UI.toolSlotC;} set {UI.toolSlotC = value;}}
    private TMP_Text coinText {get {return UI.coinText;} set {UI.coinText = value;}}
    private TMP_Text fobiddenCoinText {get {return UI.fobiddenCoinText;} set {UI.fobiddenCoinText = value;}}
    private TMP_Text arrowsText {get {return UI.arrowsText;} set {UI.arrowsText = value;}}
    private Vector3 spawnPoint;
    private Vector3 checkPointSpawn;


    
    [Header("Paused")]
    public bool gamePaused;

    #endregion

    void Start()
    {
        spawnPoint = transform.position;
        checkPointSpawn = transform.position;
        curHealth = health;
        curHunger = hunger;
        UpdateHealthUI();
        ToolChange(curTool);
        interactTrigger = playerBody.GetComponent<InteractionTrigger>();
    }

    void Update()
    {

        if (!gamePaused)
        {
            if (!freazeDecays)
            {
                SurvivalDecays();
            }

            if (curTool == ToolType.bow)
            {
                BowFire();
            }
            else
            {
                float fireInput = strike.ReadValue<float>(); 
                if (fireInput > 0)
                {
                    FireTool();
                }
                else
                {
                    HoldStrike();
                }
                holdingFire = false;
            }
        }

    }
    #region Inventory
    public void AddItem(CollectableItemType Item, int num)
    {
        switch (Item)
        {
            case CollectableItemType.coin:
            
                coins += num;
                if (coins < 0)
                {
                    coins = 0;
                }
                
                if (coinText != null)
                {
                    coinText.SetText(coins.ToString());
                }

            
            break;

            case CollectableItemType.arrow:
                arrows += num;
                if (arrows < 0)
                {
                    arrows = 0;
                }
                else if (arrows > maxArrows)
                {
                    arrows = maxArrows;
                }
                if (arrowsText != null)
                {
                    arrowsText.SetText(arrowsText.ToString());
                }
            break;

            case CollectableItemType.fobiddenCoin:
                fobiddenCoin += num;
                if (fobiddenCoin < 0)
                {
                    fobiddenCoin = 0;
                }

                if (fobiddenCoinText != null)
                {
                    fobiddenCoinText.SetText(fobiddenCoin.ToString());
                }
            break;
            
        }
    }
    #endregion

    #region Survial

    void SurvivalDecays()
    {

        if (curHunger > 0)
        {
            curHunger -= hungerDecay * Time.deltaTime;
        }
        else
        {
            //AlterHealth(-(0.2f * Time.deltaTime));
        }
    }

    public void AlterHealth(float temp)
    {
        curHealth += temp;
        //Debug.Log("Player recieved damage");

        if (curHealth <= 0)
        {
            Debug.Log("Player Died");
            Respawn();
        }
        else if (curHealth > health)
        {
            curHealth = health;
        }
        UpdateHealthUI();

    }
    public void Respawn()
    {
        curHealth = health;
        transform.position = spawnPoint;

        OpenMenu();
    }

    public void CheckPointRespawn()
    {
        transform.position = checkPointSpawn;
    }

    public void RestRestore(Vector3 newPosition)
    {
        spawnPoint = newPosition;
        checkPointSpawn = newPosition;
        curHealth = health;
        UpdateHealthUI();
    }

    public void SetCheckPoint(Vector3 newPosition)
    {
        checkPointSpawn = newPosition;
    }
    #endregion
    
    #region InputInteract/Fire

    bool enteringThroughFire = false;
    void FireTool() 
    {
        //complete spefic function for the tool with a click hold.
        //Debug.Log("Logging input from fire input");
        switch (curTool)
        {
            case ToolType.empty: //No Tool
            //Debug.Log("Firing at with no tool 0");
            break;

            case ToolType.spear: //Spear
                //Debug.Log("spear Actack input working");
            SpearActack();

            break;

            case ToolType.torch: //Torch
                changeTorch(TorchStates.lit);
                if (torchLight != null && !torchLight.activeSelf)
                {
                    //Debug.Log("Made to stage 4");
                    enteringThroughFire = true;
                    changeTorch(TorchStates.lit);
                }
                else
                {

                    //actack with torch
                    if (toolanimation != null && toolanimation.IsPlaying(torchInfo.animtionNames[0]))
                    {
                        PlayAnimation(torchInfo.animtionNames[2]);
                    }
                }
            break;

            case ToolType.rope: //Rope 
            break;
        }
    }
    void PlayAnimation(string name)
    {
        if (name != null && toolsAnimator != null)
        {
            toolsAnimator.Play(name);
        }
    }

    void HoldStrike()
    {
        //Debug.Log("")
        float strikeInput = holdStrike.ReadValue<float>();
        switch (curTool)
        {
            case ToolType.empty: //No Tool
                 //Debug.Log("strike with no tool 0");
            break;

            case ToolType.spear: //Spear
                

            break;

            case ToolType.torch: //Torch
                ThrowTorch(strikeInput);
            break;

            case ToolType.rope: //Rope 
            break;
        }
    }

    #endregion
    #region ToolSelection

    void ToolASelect(InputAction.CallbackContext context) { ToolChange(toolA); }

    void ToolBSelect(InputAction.CallbackContext context) { ToolChange(toolB); }

    void ToolCSelect(InputAction.CallbackContext context) { ToolChange(toolC);}

    void ToolChange(ToolType heldTool)
    {
        if (curTool == heldTool) {UpdateToolIconUI(); return; }
        Debug.Log("cur tool " + curTool + " new tool " + heldTool + " " + toolC);
        curTool = heldTool;

        switch (curTool)
        {
            case ToolType.empty:
            //no tool
            spearInfo.ActivateObject(false);
            torchInfo.ActivateObject(false);
            bowInfo.ActivateObject(false);

            break;

            case ToolType.spear:
            //spear
            if (spear != null)
            { 
                spearInfo.ActivateObject(true); 
            }
            else
            {
                spearInfo.ActivateObject(false);
                Debug.Log("spear data missing");
                curTool = ToolType.empty;
            }

            torchInfo.ActivateObject(false);
            bowInfo.ActivateObject(false);
            break;

            case ToolType.torch:
            //torch
            if (torchState != TorchStates.empty)
            {torchInfo.ActivateObject(true);}
            else
            {
                torchInfo.ActivateObject(false);
                curTool = ToolType.empty;
            }

            spearInfo.ActivateObject(false);
            bowInfo.ActivateObject(false);
            break;

            case ToolType.bow:
            //bow
            
                bowInfo.ActivateObject(true);


                spearInfo.ActivateObject(false);
                torchInfo.ActivateObject(false);
            break;

            case ToolType.rope:
            //rope
            
            break;

        }

        UpdateToolIconUI();

        if (interactTrigger != null)
        { 
            interactTrigger.RequirementCheck();
        }
    }

    public void AnimationRecieveCall(int state)
    {
        switch (curTool)
        {
            case ToolType.torch:
                TorchAnimation(state);
            return;
        }
    }
    #endregion

    #region Weapons
    void SpearActack()
    {
        if (canHarm)
        {
            //Debug.Log("spear Actacked");
            canHarm = false;
            PlayAnimation(spearInfo.animtionNames[1]);
            Ray ray;
            ray = new Ray(transform.position, playerBody.forward);
    
            RaycastHit hit;
    
            int layerMask = 1 << 15;
            layerMask = ~layerMask;
    
            if (Physics.Raycast(ray, out hit, spear.range, layerMask))
            {
                EmenyHealth emeny = hit.collider.gameObject.GetComponent<EmenyHealth>();
                Debug.Log("Hit object " + emeny + " type " + hit.collider.gameObject.name);
                if (emeny != null)
                {
                    emeny.Damage(spear.damage);
                    Debug.Log("Hit " + hit.collider.gameObject.name);
                }
            }
            Invoke(nameof(Reset), spear.actackSpeed);
        }
    }

    void Reset()
    {
        if (!canHarm)
        {
            canHarm = true;
        }
    }

    public float bowPower;
    void BowFire()
    {
        if (bow != null && arrows > 0)
        {
            float fireInput = strike.ReadValue<float>();
            
            if (fireInput <= 0 && holdingFire)
            {
                //release arrow
                arrows -= 1;
                
                GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);

                Vector3 newArrowRotation = Vector3.RotateTowards(arrow.transform.forward, mainCamera.forward, 200, 0);
                arrow.transform.rotation = Quaternion.LookRotation(newArrowRotation);

                arrow.GetComponent<Rigidbody>().AddForce(mainCamera.forward * bowPower * 10);

                bowPower = bow.minPower;
                holdingFire = false;
                PlayerMovement.instance.ChangeCameraType(PlayerMovement.CameraSets.forceFollow);
                cameraMovementScript.bowCameraSetting = true;
            }
            else if (fireInput > 0 )
            {
                holdingFire = true;
                if (bowPower > bow.maxPower)
                {
                    bowPower = bow.maxPower;
                }
                else if (bowPower < bow.maxPower)
                {
                    bowPower = Mathf.MoveTowards(bowPower, bow.maxPower, bow.drawSpeed * Time.deltaTime);
                }
                PlayerMovement.instance.ChangeCameraType(PlayerMovement.CameraSets.forceFollow);
                cameraMovementScript.bowCameraSetting = true;
                //pull back to max
            }
            else
            {
                PlayerMovement.instance.ChangeCameraType(PlayerMovement.CameraSets.reset);
                cameraMovementScript.bowCameraSetting = false;

            }
            return;
        }

        Debug.Log("No bow Info Added to player");

        ToolChange(ToolType.empty);
    }

    public float ArrowHit()
    {
        return 0;
    }
    #endregion

    #region Torch
    public void changeTorch(TorchStates tempTorchState)
    {
        Debug.Log("changing torch " + tempTorchState + " " +  torchState);
        if (torchState != TorchStates.empty /*&& curTool == ToolType.torch*/ && torchState != tempTorchState)
        {
            switch (tempTorchState)
            {
                case TorchStates.empty:
                torchState = TorchStates.empty;
                torchInfo.ActivateObject(false);
                break;

                case TorchStates.unlit:
                torchState = TorchStates.unlit;
                //Debug.Log("offcourse");
                if (torchLight != null && torchLight.activeSelf)
                {
                    torchLight.SetActive(false);
                }
                break;

                case TorchStates.lit:
                //Debug.Log("Made to stage 2");
                if (torchState != TorchStates.drenched)
                {
                    //Debug.Log("Made to stage 3");
                    torchState = TorchStates.lit;
                    //turn light on through animation
                    if (enteringThroughFire)
                    {
                        PlayAnimation(torchInfo.animtionNames[1]);
                    }
                    else if (torchLight != null && !torchLight.activeSelf)
                    {
                        torchLight.SetActive(true);
                    }
                    
                }
                break;

                case TorchStates.drenched:
                torchState = TorchStates.drenched;
                if (torchLight != null && torchLight.activeSelf) // turn light off
                {
                    torchLight.SetActive(false);
                }
                break;
            }
        }
    }

    public bool AddTorch(TorchStates torchData)
    {
        //Debug.Log("tring to add torch");
        if (torchState == TorchStates.empty)
        {
            torchState = TorchStates.attachingTorch;
            ToolChange(ToolType.torch);
            changeTorch(torchData);
            
            return true;
        }
        return false;
    }

    bool throwingTorch = false;
    void DropTorch()
    {
        //spawn Torch and drop it to the ground,
        if (torchState != TorchStates.empty && !throwingTorch)
        {
            
            
            torchInfo.ActivateObject(false);
            if (torchPrefab != null)
            {
                GameObject tempObject = Instantiate(torchPrefab, playerBody.forward * 1.5f, Quaternion.Euler(Random.Range(-1.0f, 1.0f), 0 , Random.Range(-1.0f, 1.0f)));
                tempObject.GetComponent<PickUpTorch>().ChangeState(torchState);
            }

            torchState = TorchStates.empty;
        }

    }
    void ThrowTorch(float strikeInput)
    {
        if (strikeInput > 0 && (infinteThrow || torchState != TorchStates.empty))
        {
            if (infinteThrow)
            {
                torchInfo.ActivateObject(true);
            }

            if (!throwingTorch)
            {
                throwingTorch = true;
                throwHold = false;
                //play torchState Animation;
                PlayAnimation(torchInfo.animtionNames[3]);
                return;
            }
            else if (!throwHold)
            {
                return;
            }
        }
        else if (strikeInput <= 0 && throwingTorch)
        {
            if (throwHold && (infinteThrow || torchState != TorchStates.empty))
            {
                throwHold = false;
                Debug.Log("Throwing Torch");
                //play throw Animation to active torch launch;
                PlayAnimation(torchInfo.animtionNames[4]);
            }
            else if (!throwHold)
            {
                PlayAnimation(torchInfo.animtionNames[5]);
            }
            throwingTorch = false;

        }

    }

    void ThrowTorchLaunch() //keep separate if wanting activated for animation
    {
        ToolChange(ToolType.empty);

        if (torchPrefab != null)
        {
            GameObject tempObject = Instantiate(torchPrefab, torchInfo.heldRefernce.transform.position, Quaternion.Euler(1, 0, 1));
            tempObject.GetComponent<PickUpTorch>().ChangeState(torchState);
            tempObject.GetComponent<Rigidbody>().mass = 0.1f;
            tempObject.GetComponent<Rigidbody>().AddForce(mainCamera.forward * throwPower * 10 + transform.up * throwUpwardForce * 10);
            
        }

        torchState = TorchStates.empty;
    }

    public void DrenchTorch(bool drench)
    {
        if (curTool == ToolType.torch)
        {
            if (drench)
            {
                torchState = TorchStates.drenched;
                //torch become unlit
            }
            else if (torchState == TorchStates.drenched)
            {
                torchState = TorchStates.lit;
            }
        }
    }

    void TorchAnimation(int State)
    {
        switch (State)
        {
            case 1: //can throw torch
            if (throwingTorch)
            {
                throwHold = true;
            }
            break;

            case 2: //release torch from throw
                ThrowTorchLaunch();
            break;

            case 3: //light torch
                if (torchLight != null && !torchLight.activeSelf)
                {
                    //Debug.Log("Made to stage 4");
                    torchLight.SetActive(true);
                }
            break;
        }
    }
    #endregion

    #region Menu

    void MenuInput(InputAction.CallbackContext context)
    {
        OpenMenu();
    }

    void OpenMenu()
    {
        if (menuUi.activeSelf)
        {
            menuUi.SetActive(false);
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
            //gamePaused = true;
        }
        else
        {
            menuUi.SetActive(true);
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            //gamePaused = false;
        }
    }

    #endregion

    #region UI

    void UpdateToolIconUI()
    {
        ToolUILoop(toolSlotA, CollectSprite(toolA));
        ToolUILoop(toolSlotB, CollectSprite(toolB));
        ToolUILoop(toolSlotC, CollectSprite(toolC));

        void ToolUILoop(Image givenSlot, Sprite iconSprite)
        {
            //Debug.Log(iconSprite, givenSlot);
            if (givenSlot == null)
            {
                Debug.Log("Tool Slot Image not assinged");
                return;
            }
            if (iconSprite == null && givenSlot.gameObject.activeSelf)
            {
                givenSlot.gameObject.SetActive(false);
            }
            else if (iconSprite != null && !givenSlot.gameObject.activeSelf)
            {
                if (givenSlot.sprite != null)
                {
                    givenSlot.sprite = null;
                }
                givenSlot.sprite = iconSprite;
                givenSlot.gameObject.SetActive(true);
            }
        }

        Sprite CollectSprite (ToolType type)
        {
            switch (type)
            {
                case ToolType.rope:
                Debug.Log("No Assignment of info variable for Rope");
                return null;

                case ToolType.bow:
                return bowInfo.assignedIcon;

                case ToolType.spear:
                return spearInfo.assignedIcon;

                case ToolType.torch:
                return torchInfo.assignedIcon;

                case ToolType.empty:
                return null;
            }
            Debug.LogError("Type Not Listed for Sprite Assignment to Tool Slots");
            return null;
        }
    }

    void UpdateHealthUI()
    {
        int numChildren = healthParent.childCount;
        int intHealth = -(Mathf.FloorToInt(-curHealth));
        //Debug.Log(numChildren + " " + intHealth + " " + curHealth);
        if (numChildren != intHealth)
        {
            if (numChildren > intHealth)
            {
                Debug.Log("removing ui element");
                //int target = numChildren - intHealth
                for (int i = numChildren - intHealth; i > 0; i--)
                {
                    //remove extra element
                    Destroy(healthParent.GetChild(0).gameObject);
                }
            }
            else
            {
                for (int i = intHealth- numChildren; i > 0; i--)
                {
                    //remove extra element
                    Instantiate(healthyUiPrefab, healthParent);
                    Debug.Log("add health element");
                }
            }
        }


    }


    #endregion
    
    


}
