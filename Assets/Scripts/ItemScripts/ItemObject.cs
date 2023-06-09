using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemObject : MonoBehaviour
{
    public Text itemText;
    public int itemAmount;
    public CustomPhysics physics;
    public GameObject itemObject;
    public Image itemAppearance;
    public Item.ID itemObjectID;
    [HideInInspector] public Item itemData;
    private float rotationSpeed = 35f;
    private float visableTextDistance = 5f;
    // Start is called before the first frame update
    public void Start()
    {
        itemData = GameManager.currentWorld.blockTypes[GameManager.currentWorld.GetByteFromID(itemObjectID)].presetBlockData;
        physics.CalculateVelocity(itemObject, 0);
        transform.Translate(physics.velocity, Space.World);
        transform.position = GameManager.player.transform.position;
        //CreateAppearance();
        this.transform.parent = GameManager.currentWorld.GetChunkFromVector3(this.transform.position).chunkObject.transform;
    }

    // Update is called once per frame
    public void FixedUpdate()
    {
        physics.CalculateVelocity(itemObject, 0);
        transform.Translate(physics.velocity, Space.World);
    }

    public void Update()
    {
        GameManager.gManager.uiManagerRef.UpdateItemObjectText(itemText, visableTextDistance, itemData, this.gameObject, itemAmount);
        GameManager.gManager.uiManagerRef.RotateUIToFaceCamera(itemText.gameObject.transform, GameManager.player.playerCam);
        if (itemAppearance.sprite != itemData.icon) { itemAppearance.sprite = itemData.icon; }
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
