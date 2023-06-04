using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemObject : MonoBehaviour
{
    public Text itemText;
    public int itemAmount;
    public CustomPhysics physics;
    public GameObject itemAppearance;
    public Item.ID itemObjectID;
    [HideInInspector] public Item itemData;
    private float rotationSpeed = 35f;
    private float visableTextDistance = 5f;
    // Start is called before the first frame update
    void Start()
    {
        itemData = GameManager.currentWorld.blockTypes[GameManager.currentWorld.GetByteFromID(itemObjectID)].presetBlockData;
        physics.CalculateVelocity(itemAppearance, 0);
        transform.Translate(physics.velocity, Space.World);
        transform.position = GameManager.player.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        physics.CalculateVelocity(itemAppearance, 0);
        transform.Translate(physics.velocity, Space.World);
    }

    void Update()
    {
        GameManager.uiManagerRef.UpdateItemObjectText(itemText, visableTextDistance, itemData, this.gameObject);  
        RotateItem();
    }

    public void OnThisItemCollected()
    {
        GameManager.player.inventory.AutoFindSlot(itemData).AddItem(itemData, itemAmount);
        Destroy(this.gameObject);
    }

    private void RotateItem()
    {
        itemAppearance.transform.Rotate(Vector3.up * -rotationSpeed * Time.deltaTime, Space.World);
    }
}
