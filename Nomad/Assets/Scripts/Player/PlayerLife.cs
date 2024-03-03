using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLife : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] float health;
    float curHealth;
    [Header("Actack")]
    float coolDown;
    public WeaponItem weapon;

    #region Inputs
    private PlayerInputActions playerControls;
    private InputAction strike;
    void Awake()
    {
        playerControls = new PlayerInputActions();
    }
    void OnEnable()
    {
        strike = playerControls.Player.Hit;
        strike.Enable();
        strike.performed += Actack;
    }

    void OnDisable()
    {
        strike.Disable();
    }
    #endregion

    void Start()
    {
        curHealth = health;
    }

    void Update()
    {
        if (coolDown > 0)
        {
            coolDown -= 1 * Time.deltaTime;
        }
    }
    #region Actacks

    void Actack(InputAction.CallbackContext context)
    {
        if (coolDown <= 0 )
        {
            switch (weapon.type)
            {
                case 1:
                SpearActack();
                break;

                case 2:

                break;
            }
            Debug.Log("actack");
        }
    }

    void SpearActack()
    {
        Ray ray;
        ray = new Ray(gameObject.transform.position, gameObject.transform.forward);

        RaycastHit hit;

        int layerMask = 1 << 10;
        layerMask = ~layerMask;

        if (Physics.Raycast(ray, out hit, weapon.range, layerMask))
        {
            EmenyHealth emeny = hit.collider.gameObject.GetComponent<EmenyHealth>();
            if (emeny != null)
            {
                emeny.Damage(weapon.damage);
                Debug.Log("Hit " + hit.collider.gameObject.name);
                coolDown = weapon.actackSpeed;
            }
        }
    }
    #endregion


}
