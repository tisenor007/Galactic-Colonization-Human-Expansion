using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gManager;
    public static UIManager uiManagerRef;
    public static World currentWorld;


    void Awake()
    {
        //singleton pattern
        if (gManager == null)
        { DontDestroyOnLoad(this.gameObject); gManager = this; }
        else if (gManager != this)
        { Destroy(this.gameObject); }
    }

    // Start is called before the first frame update
    void Start()
    {
        uiManagerRef = UIManager.uiManager;
        currentWorld = GameObject.Find("World").GetComponent<World>();
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
