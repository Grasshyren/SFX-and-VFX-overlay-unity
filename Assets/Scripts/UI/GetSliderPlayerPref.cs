using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class GetSliderPlayerPref : MonoBehaviour
{
    public float sliderValue;

    private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Slider>().value = PlayerPrefs.GetFloat(gameObject.name, sliderValue);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnValueChange(float newvalue)
    {
        sliderValue = newvalue;
    }

    private void OnApplicationQuit()
    {
        float testFloat = 0;
        testFloat = PlayerPrefs.GetFloat(gameObject.name, testFloat);
        if(testFloat != sliderValue)
            PlayerPrefs.SetFloat(gameObject.name, sliderValue);
    }
}
