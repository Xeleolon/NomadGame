using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    [SerializeField] string pickUpName = "coin";

    [SerializeField] int value;
    [SerializeField] PlayerLife.CollectableItemType collectable = PlayerLife.CollectableItemType.coin;
    [SerializeField] GameObject awakenObject;
    [SerializeField] bool setObjectToAwake = true;

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerLife.instance.AddItem(collectable, value);
            Debug.Log("Player picking up " + value + " of " + pickUpName);

            if (awakenObject != null)
            {
                if (setObjectToAwake && !awakenObject.activeSelf)
                {
                    awakenObject.SetActive(true);
                }
                else if (!setObjectToAwake && awakenObject.activeSelf)
                {
                    awakenObject.SetActive(false);
                }

            }

            Destroy(gameObject);
        }
    }
}
