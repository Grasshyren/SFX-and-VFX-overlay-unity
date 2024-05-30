using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queue : MonoBehaviour
{

    void Awake()
    {
        //audioSFX = GameObject.Find("Audio Normal").GetComponent<AudioSource>();
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
