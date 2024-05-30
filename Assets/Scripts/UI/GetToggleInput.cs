using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class GetToggleInput : MonoBehaviour
{
    public int toggleValue = 1;

    private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt(gameObject.name, toggleValue) == 1)
        {
            gameObject.GetComponent<Toggle>().isOn = true;
            toggleValue = 1;
        }
        else
        {
            gameObject.GetComponent<Toggle>().isOn = false;
            toggleValue = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnValueChange(bool newvalue)
    {
        if (newvalue == true)
            toggleValue = 1;
        else
            toggleValue = 0;
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt(gameObject.name, toggleValue);
    }
}
