using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using UnityEngine.Video;



//public struct Queue
//{
//    public ChatMessageData ChatMessageData;
//}

// OR

//
// List<ChatMessageData> Queue = new List<ChatMessageData>();

// OR

// 
// Make it it's own Queue class with public AddToQueue Function <--- Seems like the best option so it can be modified later easier

//[TODO] Create file with variables and add any missing ones to it on program start/end.

public class GlobalVars : MonoBehaviour
{
    public static String folderPath;
    GameObject vfxPrefab;
    //Find and link all scripts here
    GameObject scriptsHolder;
    ChatParser chatparser;
    Counter counter;
    SFX sfx;
    TTChan ttchan;
    Twitch twitch;
    VFX vfx;


    public static string bot_Connect_Message = "Stream Desktop Overlay Connected";
    public static string bot_Command_Prefix = "!";
    public static string bot_Disconnect_Message = "Stream Desktop Overlay Disconnected";

    
    public static string twitch_Username = ""; //The username which was used to generate the OAuth token (make this lower case)
    public static string twitch_Channel_To_Listen_To = ""; //The username which was used to generate the OAuth token (make this lower case)

    bool firstLoad = true;


    //[TODO] Implement these into the settings file

    public static float chat_Emote_OnScreenForSeconds = 5;
    public static float chat_Emote_UI_Z = -501;
    public static int screenWidth = 1920;
    public static int screenHeight = 1080;



    void Awake()
    {
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif

        scriptsHolder = GameObject.Find("Scripts Bot");
        chatparser = scriptsHolder.GetComponentInChildren<ChatParser>();
        counter = scriptsHolder.GetComponentInChildren<Counter>();
        sfx = scriptsHolder.GetComponentInChildren<SFX>();
        ttchan = scriptsHolder.GetComponentInChildren<TTChan>();
        twitch = scriptsHolder.GetComponentInChildren<Twitch>();
        vfx = scriptsHolder.GetComponentInChildren<VFX>();



        vfxPrefab = GameObject.Find("VFX_Cube");

        folderPath = Path.Combine(Application.dataPath, "../");
        folderPath = Path.GetFullPath(folderPath);
    }

    // Start is called before the first frame update
    void Start()
    {
        ReloadAll();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnApplicationQuit()
    {
        WriteGlobalsToFile();
    }

    void LoadGlobals()
    {
        String currentPath = GlobalVars.folderPath + @"Configurable\Settings.txt";

        if (File.Exists(currentPath))
        {
            using (StreamReader reader = new StreamReader(currentPath))
            {
                String line;

                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        if (line.Contains("="))
                        {
                            SetGlobalFromFile(line);
                        }
                    }
                    catch
                    {
                        Debug.Log(String.Format("Error reading data from the Settings file at Line:{0}", line));
                    }
                }
            }
        }
        else
        {
            WriteGlobalsToFile();
        }
        firstLoad = false;
    }


    void SetGlobalFromFile(String line)
    {
        String variableFinder;
        String userInput;
        variableFinder = line.Substring(0, line.IndexOf("="));
        variableFinder = variableFinder.ToLower();
        variableFinder = variableFinder.Replace(" ", "");
        if (line.IndexOf("=") + 1 > line.Length)
        {
            Debug.Log("String Not Long Enough, " + line.IndexOf("=" + 1) + " VS " + line.Length);
            return;
        }
        userInput = line.Substring(line.IndexOf("=") + 1);
        Debug.Log(userInput);

        //[TODO] Implement Globalvars for vertical_Chat_X vertical_Chat_Y vertical_Chat_Width vertical_Chat_Height horizontal_Chat_Bottom (bool)
        switch (variableFinder)
        {
            case "bot_connect_message":
                bot_Connect_Message = userInput.Trim();
                Debug.Log("bot_connect_message Changed to: " + bot_Connect_Message);
                break;  
            case "bot_command_prefix":
                userInput = userInput.Replace(" ", "");
                bot_Command_Prefix = userInput.ToLower();
                Debug.Log("bot_Command_Prefix Changed to: " + bot_Command_Prefix);
                break;
            case "bot_disconnect_message":
                bot_Disconnect_Message = userInput.Trim();
                Debug.Log("bot_disconnect_message Changed to: " + bot_Disconnect_Message);
                break;
            case "chat_background_color":
                chatparser.chat_Background_Color = userInput.Trim();
                Debug.Log("chat_background_color Changed to: " + chatparser.chat_Background_Color);
                break;
            case "chat_first_message_color":
                chatparser.chat_First_Message_Color = userInput.Trim();
                Debug.Log("chat_first_message_color Changed to: " + chatparser.chat_First_Message_Color);
                break;
            case "chat_font_color":
                chatparser.chat_Font_Color = userInput.Trim();
                Debug.Log("chat_font_color Changed to: " + chatparser.chat_Font_Color);
                break;
            case "chat_font_size":
                userInput = userInput.Replace(" ", "");
                int.TryParse(userInput, out chatparser.chat_Font_Size);
                Debug.Log("chat_Font_Size Changed to: " + chatparser.chat_Font_Size);
                break;
            case "chat_highlighted_message_color":
                chatparser.chat_Highlighted_Message_Color = userInput.Trim();
                Debug.Log("chat_highlight_color Changed to: " + chatparser.chat_Highlighted_Message_Color);
                break;
            case "chat_horizontal":
                userInput = userInput.Replace(" ", "");
                bool.TryParse(userInput, out chatparser.chat_Horizontal);
                Debug.Log("chat_Horizontal Changed to: " + chatparser.chat_Horizontal);
                break; 
            case "chat_horizontal_top":
                userInput = userInput.Replace(" ", "");
                bool.TryParse(userInput, out chatparser.chat_Horizontal_Top);
                Debug.Log("chat_Horizontal_Top Changed to: " + chatparser.chat_Horizontal_Top);
                break;
            case "chat_users_to_ignore":
                chatparser.chat_Users_To_Ignore.Clear();
                if (userInput.Contains(','))
                {
                    userInput = userInput.Trim();
                    string[] namesSplit = userInput.Split(',');
                    foreach(string name in namesSplit)
                    {
                        chatparser.chat_Users_To_Ignore.Add(name.Trim());
                        Debug.Log("chat_users_to_ignore Added: " + name.Trim());
                    }
                }
                else
                {
                    chatparser.chat_Users_To_Ignore.Add(userInput);
                    Debug.Log("chat_users_to_ignore Added: " + userInput);
                }
                break;
            case "counter_enabled":
                userInput = userInput.Replace(" ", "");
                bool.TryParse(userInput, out counter.counter_Enabled);
                Debug.Log("counter_Enabled Changed to: " + counter.counter_Enabled);
                break;
            case "counter_font_color":
                userInput = userInput.Replace(" ", "");
                userInput = userInput.Replace("#", "");
                counter.counter_Font_Color = userInput;
                Debug.Log("counter_Font_Color Changed to: " + counter.counter_Font_Color);
                break;
            case "counter_ui_x":
                userInput = userInput.Replace(" ", "");
                int.TryParse(userInput, out counter.counter_UI_X);
                Debug.Log("counter_UI_X Changed to: " + counter.counter_UI_X);
                break;
            case "counter_ui_y":
                userInput = userInput.Replace(" ", "");
                int.TryParse(userInput, out counter.counter_UI_Y);
                Debug.Log("counter_UI_Y Changed to: " + counter.counter_UI_Y);
                break;
            case "counter_value":
                if (firstLoad == false)
                {
                    break;
                }
                userInput = userInput.Replace(" ", "");
                int.TryParse(userInput, out counter.counter_Value);
                Debug.Log("counter_Value Changed to: " + counter.counter_Value);
                break;
            case "sfx_folder_path":
                userInput = userInput.Trim();
                if (Directory.Exists(userInput))
                {
                    sfx.sfx_Folder_Path = userInput;
                    sfx.sfx_Folder_Disabledicon.SetActive(false);
                    Debug.Log("sfx_folder_path changed to: " + sfx.sfx_Folder_Path);
                }
                break;
            case "sfx_master_vol":
                userInput = userInput.Replace(" ", "");
                float.TryParse(userInput, out sfx.sfx_Master_Vol);
                Debug.Log("sfx_Master_Vol Changed to: " + sfx.sfx_Master_Vol);
                break;
            case "tts_master_volume":
                userInput = userInput.Replace(" ", "");
                int.TryParse(userInput, out ttchan.tts_Master_Volume);
                if (ttchan.tts_Master_Volume > 100)
                {
                    ttchan.tts_Master_Volume = 100;
                }
                else if (ttchan.tts_Master_Volume < 0)
                {
                    ttchan.tts_Master_Volume = 0;
                }
                Debug.Log("tts_maseter_volume changed to: " + ttchan.tts_Master_Volume);
                break;
            case "twitch_enabled":
                userInput = userInput.Replace(" ", "");
                bool.TryParse(userInput, out twitch.twitch_Enabled);
                Debug.Log("twitch_Enabled Changed to: " + twitch.twitch_Enabled);
                break;
            case "vfx_default_x":
                userInput = userInput.Replace(" ", "");
                int.TryParse(userInput, out vfx.vfx_Default_X);
                Debug.Log("vfx_Default_X Changed to: " + vfx.vfx_Default_X);
                break;
            case "vfx_default_y":
                userInput = userInput.Replace(" ", "");
                int.TryParse(userInput, out vfx.vfx_Default_Y);
                Debug.Log("vfx_Default_Y Changed to: " + vfx.vfx_Default_Y);
                break;
            case "vfx_folder_path":
                userInput = userInput.Trim();
                if (Directory.Exists(userInput))
                {
                    vfx.vfx_Folder_Path = userInput;
                    vfx.vfx_Folder_Disabledicon.SetActive(false);
                    Debug.Log("vfx_folder_path changed to: " + vfx.vfx_Folder_Path);
                }
                break;
            case "vfx_master_vol":
                userInput = userInput.Replace(" ", "");
                float.TryParse(userInput, out vfx.vfx_Master_Vol);
                vfxPrefab.GetComponent<VideoPlayer>().SetDirectAudioVolume(0, vfx.vfx_Master_Vol);
                Debug.Log("vfx_Master_Vol Changed to: " + vfx.vfx_Master_Vol);
                break;
            default:
                break;
        }
    }

    void WriteGlobalsToFile()
    {
        String currentPath = GlobalVars.folderPath + @"Configurable\Settings.txt";



        List<string> lines = new List<string> { };

        try
        {
            string line = "";
            int i = 0;
            lines.Add("bot_connect_message = " + bot_Connect_Message);
            lines.Add("// Symbol or sequence the bot will listen for at the start of a chat message (Can't use spaces!)");
            lines.Add("bot_command_prefix = " + bot_Command_Prefix);
            lines.Add("bot_disconnect_message = " + bot_Disconnect_Message);
            lines.Add("");
            lines.Add("chat_background_color = " + chatparser.chat_Background_Color);
            lines.Add("chat_first_message_color = " + chatparser.chat_First_Message_Color);
            lines.Add("chat_font_color = " + chatparser.chat_Font_Color);
            lines.Add("chat_font_size = " + chatparser.chat_Font_Size);
            lines.Add("chat_highlighted_message_color = " + chatparser.chat_Highlighted_Message_Color);
            lines.Add("// For vertical chat set chat_horizontal to false");
            lines.Add("chat_horizontal = " + chatparser.chat_Horizontal);
            lines.Add("chat_horizontal_top = " + chatparser.chat_Horizontal_Top);
            lines.Add("// Add any bot names here to not have them display on stream chat. Seperate names by ,");
            line = "chat_users_to_ignore = ";
            i = 0;
            foreach (string name in chatparser.chat_Users_To_Ignore)
            {
                if (i == 0)
                {
                    line = line + name;
                }
                else
                {
                    line = line + "," + name;
                }
                i++;
            }
            lines.Add(line);
            lines.Add("");
            lines.Add("counter_enabled = " + counter.counter_Enabled);
            lines.Add("counter_font_color = " + counter.counter_Font_Color);
            lines.Add("counter_ui_x = " + counter.counter_UI_X);
            lines.Add("counter_ui_y = " + counter.counter_UI_Y);
            lines.Add("counter_value = " + counter.counter_Value);
            lines.Add("");
            lines.Add("sfx_folder_path = " + sfx.sfx_Folder_Path);
            lines.Add("// Volume needs to be between 0 and 1 with 1 being 100%, .5 for 50%");
            lines.Add("sfx_master_vol = " + sfx.sfx_Master_Vol);
            lines.Add("");
            lines.Add("tts_master_volume = " + ttchan.tts_Master_Volume);
            lines.Add("");
            lines.Add("// If a VFX is not full screen, it will default to this location");
            lines.Add("vfx_default_x = " + vfx.vfx_Default_X);
            lines.Add("vfx_default_y = " + vfx.vfx_Default_Y);
            lines.Add("vfx_folder_path = " + vfx.vfx_Folder_Path);
            lines.Add("// Volume needs to be between 0 and 1 with 1 being 100%, .5 for 50%");
            lines.Add("vfx_master_vol = " + vfx.vfx_Master_Vol);
            lines.Add("");
            lines.Add("// Chat services for the bot to listen to");
            lines.Add("twitch_enabled = " + twitch.twitch_Enabled);
        }
        catch
        {
            Debug.Log("Ran into error writing on line #" + lines.Count + " Contents: " + lines[lines.Count]);
        }

        File.WriteAllLines(currentPath, lines);

    }

    public void ReloadAll()
    {
        LoadGlobals();
        chatparser.Reload();
        counter.Reload();
        sfx.Reload();
        vfx.Reload();
        twitch.Reload();

        vfxPrefab.GetComponent<VideoPlayer>().SetDirectAudioVolume(0, vfx.vfx_Master_Vol);
    }
    //public void SetMaxVolSFX_UI(float newvalue)
    //{
    //    sfx_Master_Vol = newvalue;
    //}    
    //public void SetMaxVolVFX_UI(float newvalue)
    //{
    //    vfx_Master_Vol = newvalue;
    //    vfxPrefab.GetComponent<VideoPlayer>().SetDirectAudioVolume(0, newvalue);
    //}
    //public void SetChatFontSize_UI(string newvalue)
    //{
    //    int tempNumber;
    //    bool canConvert = int.TryParse(newvalue, out tempNumber);
    //    if (canConvert == true)
    //    {
    //        chatparser.font_Size = tempNumber;
    //    }
    //}
    //public void SetChatHorizontal_UI(bool newvalue)
    //{
    //    chatparser.horizontal = newvalue;
    //}
    //public void SetSFXFolder_UI()
    //{
    //    Debug.Log("SFX Button Clicked");
    //}
    //public void SetVFXFolder_UI()
    //{
    //    Debug.Log("VFX Button Clicked");
        
    //    Debug.Log(sfx_Folder_Path);
    //}
    //public void SetVFXDefaultX_UI(string newvalue)
    //{
    //    int tempNumber;
    //    bool canConvert = int.TryParse(newvalue, out tempNumber);
    //    if (canConvert == true)
    //    {
    //        vfx_Default_X = tempNumber;
    //    }
    //}
    //public void SetVFXDefaultY_UI(string newvalue)
    //{
    //    int tempNumber;
    //    bool canConvert = int.TryParse(newvalue, out tempNumber);
    //    if (canConvert == true)
    //    {
    //        vfx_Default_Y = tempNumber;
    //    }
    //}
    //public void SetCommandPrefix_UI(string newvalue)
    //{
    //    bot_Command_Prefix = newvalue.Replace(" ", "");
    //}
    //public void SetTwitchEnabled_UI(bool newvalue)
    //{
    //    twitch_Enabled = newvalue;
    //}




}
