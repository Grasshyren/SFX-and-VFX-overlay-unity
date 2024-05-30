using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuFocusToggle : MonoBehaviour
{
    [SerializeField]
    List<GameObject> gameObjects = new List<GameObject> { };

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnApplicationFocus(bool hasFocus)
    {
        foreach(GameObject go in gameObjects)
        {
            go.SetActive(hasFocus);
        }
    }
}
