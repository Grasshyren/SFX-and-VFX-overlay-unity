using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using System.Timers;
using System;
using System.Linq;
using UnityEngine.Networking;

public struct ChatMessageData
{
    public string MessageID;
    public string NameColor;
    public string DisplayName;
    public string UserName;
    public string Message;
    public string Command;
    public string Tags;
    public bool IsFirstMessage;
    public bool IsHighlightedMessage;
    public bool IsItalicized;
}

public class ChatParser : MonoBehaviour
{

    public string chat_Background_Color = "0e0c1360";
    public float chat_Emote_OnScreenForSeconds = 5;
    public float chat_Emote_UI_Z = -501;
    public bool chat_Enabled = true;
    public string chat_First_Message_Color = "FFD700";
    public string chat_Font_Color = "FFFFFF";
    public string chat_Highlighted_Message_Color = "9146FF";
    public int chat_Font_Size = 16;
    public bool chat_Horizontal = true;
    public bool chat_Horizontal_Top = false;
    public List<string> chat_Users_To_Ignore = new List<string> { "streamelements", "tangiabot" };

    //TextMeshProUGUI uIChat;
    GlobalVars globalVars;
    Counter counter;
    Dictionary<string, string> EmoteDict_PNGs = new Dictionary<string, string>() { };
    Dictionary<string, Texture2D> EmoteDict_PNGs_AsTextures = new Dictionary<string, Texture2D>() { };
    SFX sFX;
    VFX vFX;
    TTChan ttChan;
    GameObject emotePNGPrefab;
    GameObject horizontalChat;
    GameObject horizontalChatGroup;
    GameObject horizontalChat_Background;
    GameObject verticalChat;
    GameObject verticalChatGroup;
    GameObject verticalChat_Background;
    GameObject vfxCanvas;
    List<ChatMessageData> chatMessageList = new List<ChatMessageData> { };
    int chatMessageLimit = 10; //[TODO] User Set

    void Awake()
    {
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif

        emotePNGPrefab = GameObject.Find("Emote_PNG");
        horizontalChatGroup = GameObject.Find("HorizontalChatGroup");
        horizontalChat = GameObject.Find("HorizontalChat_UI");
        horizontalChat_Background = GameObject.Find("HorizontalChat_UI_Background");
        verticalChatGroup = GameObject.Find("VerticalChatGroup");
        verticalChat = GameObject.Find("VerticalChat_UI");
        verticalChat_Background = GameObject.Find("VerticalChat_UI_Background");
        vfxCanvas = GameObject.Find("Canvas VFX Objects");
        counter = GameObject.Find("Scripts Bot").GetComponentInChildren<Counter>();
        vFX = GameObject.Find("Scripts Bot").GetComponentInChildren<VFX>();
        sFX = GameObject.Find("Scripts Bot").GetComponentInChildren<SFX>();
        ttChan = GameObject.Find("Scripts Bot").GetComponentInChildren<TTChan>();
        globalVars = GameObject.Find("Scripts Bot").GetComponentInChildren<GlobalVars>();

        if (chat_Horizontal == true)
        {
            horizontalChatGroup.SetActive(true);
            verticalChatGroup.SetActive(false);
        }
        else
        {
            horizontalChatGroup.SetActive(false);
            verticalChatGroup.SetActive(true);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //uIChat = GameObject.Find("Chat_UI").GetComponent<TextMeshProUGUI>();
        //Reload();
        //horizontalChat.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width,Screen.height);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void OnValueChanged(string newvalue)
    //{
    //    int tempNumber;
    //    bool canConvert = int.TryParse(newvalue, out tempNumber);
    //    if (canConvert == true)
    //    {
    //        fontSize = int.Parse(newvalue);
    //        AdjustFontSize();
    //    }
    //}

    public void ClearSingleMessage(string userName, string message)
    {
        int i = 0;
        foreach (ChatMessageData data in chatMessageList)
        {
            if (userName == data.UserName && message == data.Message)
            {
                chatMessageList.RemoveAt(i);
                break;
            }
            i++;
        }
        UpdateUI();
    }

    public void ClearUserMessages(string user)
    {
        int i = 0;
        List<int> messagesToRemove = new List<int> { };
        foreach (ChatMessageData data in chatMessageList)
        {
            if (user == data.UserName)
            {
                messagesToRemove.Insert(0, i);
            }
            i++;
        }

        foreach (int messageIndex in messagesToRemove)
        {
            chatMessageList.RemoveAt(messageIndex);
        }
        UpdateUI();
    }

    public void ClearAllMessages()
    {
        chatMessageList.Clear();
        UpdateUI();
    }

    public void NewMessage(ChatMessageData data)
    {
        //[TODO] Consider using a switch case for speed / reduce redundant code
        //[TODO] Add to queue when conditions met instead of what is below
        sFX.ParseMessageForSymbols(data);
        if (data.Message.ToLower().StartsWith(GlobalVars.bot_Command_Prefix))
        {
            counter.ParseMessageForCommand(data);
            ParseMessageForReload(data);
            sFX.ParseMessageForCommand(data);
            vFX.ParseMessageForCommand(data);
            ttChan.ParseMessageForCommand(data);
        }
        else
        {
            ParseMessageForDisplay(data);
        }


    }

    void ParseMessageForReload(ChatMessageData data)
    {
        string[] splitMessage = data.Message.Split(' ');

        if ((data.UserName != GlobalVars.twitch_Username) || (splitMessage[0].ToLower() != GlobalVars.bot_Command_Prefix + "reload"))
        {
            return;
        }
        globalVars.ReloadAll();
    }
    public void Reload()
    {
        if (chat_Enabled == false)
        {
            horizontalChatGroup.SetActive(false);
            verticalChatGroup.SetActive(false);
            return;
        }

        AdjustFontSize();
        SetHorizontalChatBool(chat_Horizontal);
        TopHorizontalChatCheck();
    }
    public void ShowMessage(string s)
    {

    }

    public void ShowMessage(ChatMessageData data)
    {

    }

    void ParseEmotes(ChatMessageData data)
    {
        ParseEmotes_Twitch(data);
    }

    void ParseEmotes_Twitch(ChatMessageData data)
    {
        //Checking to make sure it's not null or empty
        if ((data.Tags == null) || (data.Tags.Contains("emotes=") == false) || (data.Tags.Contains("emotes=;") == true))
        {
            return;
        }

        string tags = data.Tags;
        tags = Tags_IndexAfter("emotes=", ";", tags);
        string[] emoteList = tags.Split('/');
        // grab all emote textures
        for (int i = 0; i < emoteList.Length; i++)
        {
            var emoteID = emoteList[i];
            int emoteCount = 1;
            if (emoteID.Contains(','))
            {
                emoteCount = (emoteID.Length - emoteID.Replace(",", "").Length) + 1;
            }
            emoteID = emoteID.Substring(0, emoteID.IndexOf(":"));
            //if (emoteID.StartsWith("emotesv2_"))
            //{
            //}
            //if (ChatBot.EmoteDict_Gifs_AsTextures.ContainsKey(emoteID))
            //{
            //    StartCoroutine(Emote_Get_Gif(emoteID, "https://static-cdn.jtvnw.net/emoticons/v2/" + emoteID + "/default/dark/2.0", emoteCount));
            //}
            if (EmoteDict_PNGs_AsTextures.ContainsKey(emoteID))
            {
                StartCoroutine(Emote_Get_PNG(emoteID, "https://static-cdn.jtvnw.net/emoticons/v2/" + emoteID + "/default/dark/2.0"));
            }
            else if (!EmoteDict_PNGs_AsTextures.ContainsKey(emoteID))
            {
                StartCoroutine(Emote_Get_PNG(emoteID, "https://static-cdn.jtvnw.net/emoticons/v2/" + emoteID + "/default/dark/2.0"));
            }
            //else if (!ChatBot.EmoteDict_Gifs_AsTextures.ContainsKey(emoteID))
            //{
            //    StartCoroutine(Emote_Get_Gif(emoteID, "https://static-cdn.jtvnw.net/emoticons/v2/" + emoteID + "/default/dark/2.0", emoteCount));
            //}
            else
            {
                Debug.Log("Don't know what file type from twitch emote");
            }


            //else
            //{
            //    // /1.0 /2.0 /3.0 are the sizes
            //    StartCoroutine(Emote_Get_PNG(emoteID, "https://static-cdn.jtvnw.net/emoticons/v1/" + emoteID + "/2.0", emoteCount));
            //}
        }
    }

    public void ParseMessageForDisplay(ChatMessageData chatMessageData)
    {
        if (chat_Enabled == false)
        {
            return;
        }

        string s = "";
        GameObject chatUI;
        GameObject chatUI_Background;
        if (chat_Users_To_Ignore.Contains(chatMessageData.UserName))
            return;
        if (chatMessageData.Message.StartsWith(GlobalVars.bot_Command_Prefix))
            return;

        if (chat_Horizontal == true)
        {
            chatUI = horizontalChat;
            chatUI_Background = horizontalChat_Background;
        }
        else
        {
            chatUI = verticalChat;
            chatUI_Background = verticalChat_Background;
        }

        chatMessageList.Add(chatMessageData);

        if (chatMessageList.Count > chatMessageLimit)
            chatMessageList.RemoveAt(0);

        chatUI.GetComponent<TextMeshProUGUI>().SetText("");
        chatUI_Background.GetComponent<TextMeshProUGUI>().SetText("");

        if (chat_Horizontal == true)
        {
            string fontColor;
            foreach (ChatMessageData data in chatMessageList)
            {
                fontColor = chat_Font_Color;
                if (data.IsHighlightedMessage == true)
                {
                    fontColor = chat_Highlighted_Message_Color;
                }
                if (data.IsFirstMessage == true)
                {
                    fontColor = chat_First_Message_Color;
                }
                if (data.IsItalicized == true)
                {
                    s = s + String.Format(" <color={0}>{1}</color> <i><color=#{3}><noparse>{2}</noparse></color></i>", data.NameColor, data.DisplayName, data.Message, fontColor);
                }
                else
                {
                    s = s + String.Format(" <color={0}>{1}</color>: <color=#{3}><noparse>{2}</noparse></color>", data.NameColor, data.DisplayName, data.Message, fontColor);
                }
            }
            s = s.Trim();
            chatUI.GetComponent<TextMeshProUGUI>().SetText(s);
            chatUI_Background.GetComponent<TextMeshProUGUI>().SetText("<mark=#" + chat_Background_Color + ">" + s);
        }
        else // Vertical Chat aka Twitch Style
        {
            string fontColor;
            foreach (ChatMessageData data in chatMessageList)
            {
                fontColor = chat_Font_Color;
                if (data.IsHighlightedMessage == true)
                {
                    fontColor = chat_Highlighted_Message_Color;
                }
                if (data.IsFirstMessage == true)
                {
                    fontColor = chat_First_Message_Color;
                }
                if (data.IsItalicized == true)
                {
                    s = s + String.Format("<color={0}>{1}</color> <i><color=#{3}><noparse>{2}</noparse></color></i>", data.NameColor, data.DisplayName, data.Message, fontColor) + "\n";
                }
                else
                {
                    s = s + String.Format("<color={0}>{1}</color>: <color=#{3}><noparse>{2}</noparse></color>", data.NameColor, data.DisplayName, data.Message, fontColor) + "\n";
                }
            }
            s = s.Trim();
            chatUI.GetComponent<TextMeshProUGUI>().SetText(s);
            chatUI_Background.GetComponent<TextMeshProUGUI>().SetText("<mark=#" + chat_Background_Color + ">" + s);
        }

        //[TODO] Place parsing emotes in a better location
        ParseEmotes(chatMessageData);

    }

    public void SetHorizontalChatBool(bool b)
    {
        chat_Horizontal = b;
        if (b == true)
        {
            horizontalChatGroup.SetActive(true);
            verticalChatGroup.SetActive(false);
            AdjustFontSize();
            ChatMessageData data = new ChatMessageData { NameColor = "#B59410", DisplayName = "Stream Desktop Overlay", Message = "Horizontal Chat Enabled"};
            ParseMessageForDisplay(data);
        }
        else
        {
            verticalChatGroup.SetActive(true);
            horizontalChatGroup.SetActive(false);
            AdjustFontSize();
            ChatMessageData data = new ChatMessageData { NameColor = "#B59410", DisplayName = "Stream Desktop Overlay", Message = "Vertical Chat Enabled"};
            ParseMessageForDisplay(data);
        }
    }
    void AdjustFontSize()
    {
        if (chat_Horizontal == true)
        {
            horizontalChat.GetComponent<TextMeshProUGUI>().fontSize = chat_Font_Size;
            horizontalChat_Background.GetComponent<TextMeshProUGUI>().fontSize = chat_Font_Size;
        }
        else
        {
            verticalChat.GetComponent<TextMeshProUGUI>().fontSize = chat_Font_Size;
            verticalChat_Background.GetComponent<TextMeshProUGUI>().fontSize = chat_Font_Size;
        }
    }
    string Tags_IndexAfter(string find, string to, string s)
    {
        s = s.Substring((s.IndexOf(find) + find.Count()));
        s = s.Substring(0, s.IndexOf(@to));
        return s;
    }
    void TopHorizontalChatCheck()
    {
        if (chat_Horizontal == false)
        {
            return;
        }
        Debug.Log("TopCheck");
        GameObject horizontalChatGroup;
        horizontalChatGroup = GameObject.Find("HorizontalChatGroup");
        if (chat_Horizontal_Top == true)
        {
            //[TODO] Make Y position user defined
            horizontalChatGroup.transform.position = new Vector3(horizontalChatGroup.transform.position.x, 1061, horizontalChatGroup.transform.position.z);
        }
        else
        {
            horizontalChatGroup.transform.position = new Vector3(horizontalChatGroup.transform.position.x, 0, horizontalChatGroup.transform.position.z);
        }
    }
    void UpdateUI()
    {

        string s = "";
        GameObject chatUI;
        GameObject chatUI_Background;

        if (chat_Horizontal == true)
        {
            chatUI = horizontalChat;
            chatUI_Background = horizontalChat_Background;
        }
        else
        {
            chatUI = verticalChat;
            chatUI_Background = verticalChat_Background;
        }

        if (chatMessageList.Count > chatMessageLimit)
            chatMessageList.RemoveAt(0);

        chatUI.GetComponent<TextMeshProUGUI>().SetText("");
        chatUI_Background.GetComponent<TextMeshProUGUI>().SetText("");

        if (chat_Horizontal == true)
        {
            foreach (ChatMessageData data in chatMessageList)
            {
                s = s + String.Format(" <color={0}>{1}</color>: <noparse>{2}</noparse>", data.NameColor, data.DisplayName, data.Message);
            }
            s = s.Trim();
            chatUI.GetComponent<TextMeshProUGUI>().SetText(s);
            chatUI_Background.GetComponent<TextMeshProUGUI>().SetText("<mark=#" + chat_Background_Color + ">" + s);
        }
        else // Vertical Chat aka Twitch Style
        {
            foreach (ChatMessageData data in chatMessageList)
            {
                s = s + String.Format("<color={0}>{1}</color>: <noparse>{2}</noparse>", data.NameColor, data.DisplayName, data.Message) + "\n";
            }
            s = s.Trim();
            chatUI.GetComponent<TextMeshProUGUI>().SetText(s);
            chatUI_Background.GetComponent<TextMeshProUGUI>().SetText("<mark=#" + chat_Background_Color + ">" + s);
        }
    }


    IEnumerator Emote_Get_PNG(string emoteCode, string url)
    {
        Texture2D img;
        if (EmoteDict_PNGs_AsTextures.ContainsKey(emoteCode))
        {
            img = EmoteDict_PNGs_AsTextures[emoteCode];
            Debug.Log("Image from cache");
        }
        else
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            img = DownloadHandlerTexture.GetContent(www);

            //
            // This is code to test.
            //
            if (img == null)
            {
                //StartCoroutine(Emote_Get_Gif(emoteCode, url, particleNumber));
                yield break;
            }


            // Store emote into dictionary cache
            EmoteDict_PNGs_AsTextures.Add(emoteCode, img);
            Debug.Log("Image from net");
        }


        //[TODO] Put this into separate Show Emote Function.
        GameObject newEmote = Instantiate(emotePNGPrefab, vfxCanvas.transform, worldPositionStays: false);

        // Get Emote as texture
        RawImage rawIMG;
        rawIMG = newEmote.GetComponent<RawImage>();
        newEmote.GetComponent<RawImage>().texture = img;

        newEmote.GetComponent<RectTransform>().sizeDelta = new Vector2(rawIMG.texture.width, rawIMG.texture.height);

        float width;
        float height;
        width = (float)(GlobalVars.screen_Width * 0.5);
        height = (float)(GlobalVars.screen_Height * 0.5);

        if (rawIMG.texture.width < 50 || rawIMG.texture.height < 50)
        {
            float scaling = 2; //Multiply emote's base size by
            newEmote.GetComponent<RectTransform>().localScale = new Vector3(scaling, scaling, 1);
            width = UnityEngine.Random.Range(width * -1 + (rawIMG.texture.width * scaling), width);
            height = UnityEngine.Random.Range(height * -1, height - (rawIMG.texture.height * scaling));
        }
        else
        {
            width = UnityEngine.Random.Range(width * -1 + rawIMG.texture.width, width);
            height = UnityEngine.Random.Range(height * -1, height - rawIMG.texture.height);
        }
        Debug.Log("Emote Randomed: " + width + " " + height);

        //[TODO] Does not take into account scaling for calculating edges
        //newEmote.transform.position = new Vector3(width, height, GlobalVars.emote_UI_Z);
        //newEmote.GetComponent<RectTransform>().position = new Vector3(diswidth, disheight, 1);
        newEmote.transform.localPosition = new Vector3(width, height, chat_Emote_UI_Z);
        Destroy(newEmote, chat_Emote_OnScreenForSeconds);
    }
}
