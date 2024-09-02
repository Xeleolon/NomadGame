using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    [SerializeField] string pickUpName = "coin";

    [SerializeField] int value;
    [SerializeField] PlayerLife.CollectableItemType collectable = PlayerLife.CollectableItemType.coin;

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerLife.instance.AddItem(collectable, value);
            Debug.Log("Player picking up " + value + " of " + pickUpName);
            Destroy(gameObject);
        }
    }
}
