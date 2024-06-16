using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System;
using UnityEngine.UI;
using TMPro;

public class Counter : MonoBehaviour
{
    public bool counter_Enabled = true;
    public string counter_Font_Color = "ffffffd1";
    public string counter_Name = "Deaths: "; //[TODO] Allow user to change
    public int counter_UI_X = 0;
    public int counter_UI_Y = 16;
    public int counter_Value = 0;

    string command = "d"; //[TODO] Allow user to change. Default !d
    GameObject counterUI;

    void Awake()
    {
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif

        counterUI = GameObject.Find("Counter_UI");
    }

    // Start is called before the first frame update
    void Start()
    {
        Reload();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ParseMessageForCommand(ChatMessageData chatMessageData)
    {
        if (counter_Enabled == false)
        {
            return;
        }

        string usercommand = chatMessageData.Message.ToLower().Trim();
        if (!usercommand.StartsWith(GlobalVars.bot_Command_Prefix + command))
            return;
        usercommand = usercommand.Split(' ')[0];
        switch (usercommand)
        {
            case { } when usercommand.StartsWith(GlobalVars.bot_Command_Prefix + command + "+"):
                Add(chatMessageData);
                break;
            case { } when usercommand.StartsWith(GlobalVars.bot_Command_Prefix + command + "-"):
                Subtract(chatMessageData);
                break;
            case { } when usercommand.StartsWith(GlobalVars.bot_Command_Prefix + command + "reset"):
                Reset();
                break;
            case { } when usercommand.StartsWith(GlobalVars.bot_Command_Prefix + command + "set"):
                Set(chatMessageData);
                break;
            case { } when usercommand == GlobalVars.bot_Command_Prefix + command:
                DetermineContextually(chatMessageData); 
                break;
            default:
                break;
        }

        
    }

    void Add(ChatMessageData data)
    {

        //string target = data.Message.Substring(data.Message.IndexOf(" ") + 1);
        //var splitpoint = target.IndexOf(" ");
        //target = splitpoint > -1 ? target.Substring(0, splitpoint) : target;
        //string dvalue = data.Message.Substring(data.Message.IndexOf(" ") + 1);
        //dvalue = dvalue.Substring(dvalue.IndexOf(" ") + 1);
        //splitpoint = dvalue.IndexOf(" ");
        //dvalue = splitpoint > -1 ? dvalue.Substring(0, splitpoint) : dvalue;
        //dvalue = dvalue.TrimEnd();


        string dvalue = "";
        dvalue = data.Message.ToLower().Replace(GlobalVars.bot_Command_Prefix + command, "");
        dvalue = dvalue.Replace("+", "");
        dvalue = dvalue.Trim();
        int intValue = 0;
        Int32.TryParse(dvalue, out intValue);
        try
        {
            if (dvalue == "")
            {
                intValue = 1;
            }
            Debug.Log(String.Format("Add Value:{0}", intValue));
            if (intValue >= 1000)
            {
                Debug.Log("Error: Adding too many to counter at once!");
            }
            else if (intValue <= -1)
            {
                Debug.Log("Error: Can't add negative value!");
            }
            else
            {
                counter_Value += intValue;
                DisplayCounter();
            }
        }
        catch
        {
            Debug.Log(String.Format("Error adding {0} to {1} {2} counter.", dvalue, counter_Value, counter_Name));
        }

        //try
        //{
        //    if (dvalue == GlobalVars.bot_Command_Prefix + command | dvalue == GlobalVars.bot_Command_Prefix + command + "+")
        //    {
        //        dvalue = "1";
        //    }
        //    Debug.Log(String.Format("Add Value:{0}", Int32.Parse(dvalue)));
        //    if (Int32.Parse(dvalue) >= 1000)
        //    {
        //        Debug.Log("Error: Adding too many to counter at once!");
        //    }
        //    else if (Int32.Parse(dvalue) <= -1)
        //    {
        //        Debug.Log("Error: Can't add negative value!");
        //    }
        //    else
        //    {
        //        counter_Value += Int32.Parse(dvalue);
        //        DisplayCounter();
        //    }
        //}
        //catch
        //{
        //    Debug.Log(String.Format("Error adding {0} to {1} {2} counter.", dvalue, counter_Value, counter_Name));
        //}
    }

    void Reset()
    {
        counter_Value = 0;
        DisplayCounter();
    }

    void Set(ChatMessageData data)
    {
        string dvalue = "";
        dvalue = data.Message.ToLower().Replace(GlobalVars.bot_Command_Prefix + command + "set", "");
        dvalue = dvalue.Trim();
        int intValue = counter_Value;
        Int32.TryParse(dvalue, out intValue);
        try
        {
            if (intValue >= 2000000001)
            {
                Debug.Log("Error: Too close to the 32 bit integer threshold");
            }
            else if (intValue <= -2000000001)
            {
                Debug.Log("Error: Too close to the 32 bit integer threshold");
            }
            else
            {
                counter_Value = intValue;
                DisplayCounter();
            }
        }
        catch
        {
            Debug.Log(String.Format("Error with {0} when trying to set current {1} {2} counter.", dvalue, counter_Value, counter_Name));
        }
    }

    void Subtract(ChatMessageData data)
    {
        string dvalue = "";
        dvalue = data.Message.ToLower().Replace(GlobalVars.bot_Command_Prefix + command, "");
        dvalue = dvalue.Replace("-", "");
        dvalue = dvalue.Trim();
        int intValue = 0;
        Int32.TryParse(dvalue, out intValue);
        try
        {
            if (dvalue == "")
            {
                intValue = 1;
            }
            Debug.Log(String.Format("Subtract Value:{0}", intValue));
            if (intValue >= 1000)
            {
                Debug.Log("Error: Subtracting too many to counter at once!");
            }
            else if (intValue <= -1)
            {
                Debug.Log("Error: Can't subtract negative value!");
            }
            else
            {
                counter_Value -= intValue;
                DisplayCounter();
            }
        }
        catch
        {
            Debug.Log(String.Format("Error subtracting {0} to {1} {2} counter.", dvalue, counter_Value, counter_Name));
        }


        //string target = data.Message.Substring(data.Message.IndexOf(" ") + 1);
        //var splitpoint = target.IndexOf(" ");
        //target = splitpoint > -1 ? target.Substring(0, splitpoint) : target;
        //string dvalue = data.Message.Substring(data.Message.IndexOf(" ") + 1);
        //dvalue = dvalue.Substring(dvalue.IndexOf(" ") + 1);
        //splitpoint = dvalue.IndexOf(" ");
        //dvalue = splitpoint > -1 ? dvalue.Substring(0, splitpoint) : dvalue;
        //dvalue = dvalue.TrimEnd();
        //try
        //{
        //    if (dvalue == GlobalVars.bot_Command_Prefix + command + "-")
        //    {
        //        dvalue = "1";
        //    }
        //    if (Int32.Parse(dvalue) >= 1000)
        //    {
        //        Debug.Log("Error: Subtracting too many to counter at once!");
        //    }
        //    else if (Int32.Parse(dvalue) <= -1)
        //    {
        //        Debug.Log("Error: Can't subtract negative value!");
        //    }
        //    else
        //    {
        //        Debug.Log(String.Format("Subtract Value:{0}", Int32.Parse(dvalue)));
        //        counter_Value -= Int32.Parse(dvalue);
        //        DisplayCounter();
        //    }
        //}
        //catch
        //{
        //    Debug.Log(String.Format("Error adding {0} to {1} {2} counter.", dvalue, counter_Value, counter_Name));
        //}
    }

    void DetermineContextually(ChatMessageData data)
    {
        if (data.Message.Contains("-"))
        {
            Subtract(data);
        }
        else
        {
            Add(data);
        }
    }

    void DisplayCounter()
    {
        if (counter_Enabled)
        {
            counterUI.GetComponent<TextMeshProUGUI>().SetText(String.Format("<color=#{0}>{1}{2}", counter_Font_Color,counter_Name , counter_Value));
            counterUI.transform.position = new Vector3(counter_UI_X, counter_UI_Y, counterUI.transform.position.z);
        }
        else
        {
            counterUI.GetComponent<TextMeshProUGUI>().SetText("");
        }
    }

    public void Reload()
    {
        counterUI.SetActive(counter_Enabled);
        DisplayCounter();
    }
}
