using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class DisplaySliderFloat : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnValueChanged(float newvalue)
    {
        this.GetComponent<TextMeshProUGUI>().text = newvalue.ToString();
    }
}
