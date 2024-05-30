using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class GetInputPlayerPref : MonoBehaviour
{
    public int inputValue = 14;

    private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        inputValue = PlayerPrefs.GetInt(gameObject.name, inputValue);
        gameObject.GetComponent<TMP_InputField>().text = PlayerPrefs.GetInt(gameObject.name, inputValue).ToString();
        //gameObject.GetComponent<TMP_InputField>().pointSize = PlayerPrefs.GetInt(gameObject.name, inputValue);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnValueChange(string newvalue)
    {
        int tempNumber;
        bool canConvert = int.TryParse(newvalue, out tempNumber);
        if (canConvert == true)
        {
            inputValue = tempNumber;
        }
    }

    private void OnApplicationQuit()
    {
        int testInt = 0;
        testInt = PlayerPrefs.GetInt(gameObject.name, testInt);
        if (testInt != inputValue)
            PlayerPrefs.SetInt(gameObject.name, inputValue);
    }
}


