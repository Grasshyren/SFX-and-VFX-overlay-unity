using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleVisibility : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleGameObjectVisibility(GameObject sentGameObject)
    {
        if (sentGameObject.activeInHierarchy == true)
        {
            sentGameObject.SetActive(false);
        }
        else
        {
            sentGameObject.SetActive(true);
        }
    }
}
