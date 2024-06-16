using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Net.Sockets;
using System.IO;
using System.Timers;
using System;
using System.Linq;

public class Twitch : MonoBehaviour
{

    public bool twitch_Enabled = true;

    //private static ChatParser_Twitch _instance;
    //public static ChatParser_Twitch Instance
    //{
    //    get
    //    {
    //        if (_instance == null)
    //        {
    //            _instance = new ChatParser_Twitch();
    //        }
    //        return _instance;
    //    }
    //}
    ChatParser chatParser;
    bool isConnected = false;
    bool isReconnecting = false;
    TcpClient _twitchClient;
    StreamReader _reader;
    StreamWriter _writer;
    List<string> viewerList = new List<string> { };
    List<string> chatList = new List<string> { };
    string chatMessages;
    String twitch_OAuth_Token = ""; //A Twitch OAuth token which can be used to connect to the chat can be gotten from https://twitchapps.com/tmi/
    System.Timers.Timer reconnectTimer;
    
    //[TODO] Add bool to use darkmode colors instead
    //private Dictionary<string, string> twitchDarkmodeNameColors = new Dictionary<string, string>()
    //{
    //    { "#FF0000", "#FF5858" }, //Red
    //    { "#0000FF", "#7878FF" }, //Blue
    //    { "#008000", "#01E001" }, //Green
    //    { "#B22222", "#E05B5B" }, //Firebrick
    //    { "#FF7F50", "#FF7F50" }, //Coral
    //    { "#9ACD32", "#9ACD32" }, //Yellow Green
    //    { "#FF4500", "#FF581A" }, //Orange Red
    //    { "#2E8B57", "#3DB974" }, //Sea Green
    //    { "#DAA520", "#D3A020" }, //Goldenrod
    //    { "#D2691E", "#E1762A" }, //Chocolate
    //    { "#5F9EA0", "#5C989A" }, //Cadet Blue
    //    { "#1E90FF", "#359BFF" }, //Dodger Blue
    //    { "#FF69B4", "#FF69B4" }, //Hot Pink
    //    { "#8A2BE2", "#AA64EA" }, //Blue Violet
    //    { "#00FF7F", "#00FF7F" } //Spring Green
    //};


    void Awake()
    {
        //_instance = this;
        //DontDestroyOnLoad(this);
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
        chatParser = GameObject.Find("Scripts Bot").GetComponentInChildren<ChatParser>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
        LoadCredentials();
    }

    // Update is called once per frame
    void Update()
    {
        if (_twitchClient != null && _twitchClient.Connected)
        {
            ParseChat();
        }
    }

    void OnApplicationQuit()
    {
        if (isConnected == true)
        {
            TwitchSendMessage(GlobalVars.bot_Disconnect_Message);
            Disconnect();
        }
        if (reconnectTimer != null)
        {
            reconnectTimer.Stop();
            reconnectTimer.Dispose();
            Debug.Log("Ending Reconnect Timer");
        }
    }

    public void Connect()
    {
        _twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
        _reader = new StreamReader(_twitchClient.GetStream());
        _writer = new StreamWriter(_twitchClient.GetStream());
        _writer.WriteLine("CAP REQ :twitch.tv/membership");
        _writer.WriteLine("CAP REQ :twitch.tv/tags");
        _writer.WriteLine("CAP REQ :twitch.tv/commands");
        _writer.WriteLine("PASS " + twitch_OAuth_Token);
        _writer.WriteLine("NICK " + GlobalVars.twitch_Username);
        _writer.WriteLine("USER " + GlobalVars.twitch_Username + " 8 * :" + GlobalVars.twitch_Username);
        _writer.WriteLine("JOIN #" + GlobalVars.twitch_Channel_To_Listen_To);
        _writer.Flush();
    }

    public void Disconnect()
    {
        _writer.WriteLine("PART #" + GlobalVars.twitch_Channel_To_Listen_To);
        _writer.Flush();
        _twitchClient.GetStream().Close();
        _reader.Close();
        _writer.Close();
        _twitchClient.Close();
        Debug.Log("Closed reader, writer, twitch connections");
    }

    public void Reload()
    {
        if (twitch_Enabled == false)
        {
            if (isConnected == true)
            {
                TwitchSendMessage(GlobalVars.bot_Disconnect_Message);
                Disconnect();
            }
            if (reconnectTimer != null)
            {
                reconnectTimer.Stop();
                reconnectTimer.Dispose();
                Debug.Log("Ending Reconnect Timer");
            }
        }
        else
        {
            LoadCredentials();
        }
    }

    public void TwitchSendMessage(string m)
    {
        if (GlobalVars.twitch_Channel_To_Listen_To == GlobalVars.twitch_Username)
        {
            _writer.WriteLine("PRIVMSG #" + GlobalVars.twitch_Channel_To_Listen_To + " :" + m);
            _writer.Flush();
        }
    }

    public void TwitchStart()
    {
        if (twitch_Enabled == true)
        {
            SetReconnectTimer();
            Connect();
        }
    }

    void CreateCredentials()
    {
        String currentPath = GlobalVars.folderPath + @"Configurable\Logins\Twitch.txt";

        List<string> lines = new List<string> { };

        try
        {
            lines.Add("//A Twitch OAuth token which can be used to connect to the chat can be gotten from https://twitchapps.com/tmi/");
            lines.Add("twitch_OAuth_Token = ");
            lines.Add("");
            lines.Add("//The username which was used to generate the OAuth token");
            lines.Add("twitch_Username = ");
            lines.Add("");
            lines.Add("//The channel you wish to have the bot listen on, if different from username it won't send messages");
            lines.Add("twitch_Channel_To_Listen_To = ");
        }
        catch
        {
            Debug.Log("Ran into error writing on line #" + lines.Count + " Contents: " + lines[lines.Count]);
        }

        File.WriteAllLines(currentPath, lines);
    }

    List<string> Emotes_Twitch_Parse(string s)
    {
        List<string> output = new List<string> { "" };
        //if (s.Contains("emotes=;") == false)
        //{
        //    s = Tags_IndexAfter("emotes=", ";", s);
        //    string[] emoteList = s.Split('/');
        //    // grab all emote textures
        //    for (int i = 0; i < emoteList.Length; i++)
        //    {
        //        var emoteID = emoteList[i];
        //        int emoteCount = 1;
        //        if (emoteID.Contains(','))
        //        {
        //            emoteCount = (emoteID.Length - emoteID.Replace(",", "").Length) + 1;
        //        }
        //        emoteID = emoteID.Substring(0, emoteID.IndexOf(":"));
        //        //if (emoteID.StartsWith("emotesv2_"))
        //        //{
        //        //}
        //        if (ChatBot.EmoteDict_Gifs_AsTextures.ContainsKey(emoteID))
        //        {
        //            StartCoroutine(Emote_Get_Gif(emoteID, "https://static-cdn.jtvnw.net/emoticons/v2/" + emoteID + "/default/dark/2.0", emoteCount));
        //        }
        //        else if (ChatBot.EmoteDict_PNGs_AsTextures.ContainsKey(emoteID))
        //        {
        //            StartCoroutine(Emote_Get_PNG(emoteID, "https://static-cdn.jtvnw.net/emoticons/v2/" + emoteID + "/default/dark/2.0", emoteCount));
        //        }
        //        else if (!ChatBot.EmoteDict_PNGs_AsTextures.ContainsKey(emoteID))
        //        {
        //            StartCoroutine(Emote_Get_PNG(emoteID, "https://static-cdn.jtvnw.net/emoticons/v2/" + emoteID + "/default/dark/2.0", emoteCount));
        //        }
        //        //else if (!ChatBot.EmoteDict_Gifs_AsTextures.ContainsKey(emoteID))
        //        //{
        //        //    StartCoroutine(Emote_Get_Gif(emoteID, "https://static-cdn.jtvnw.net/emoticons/v2/" + emoteID + "/default/dark/2.0", emoteCount));
        //        //}
        //        else
        //        {
        //            Debug.Log("Don't know what file type from twitch emote");
        //        }


        //        //else
        //        //{
        //        //    // /1.0 /2.0 /3.0 are the sizes
        //        //    StartCoroutine(Emote_Get_PNG(emoteID, "https://static-cdn.jtvnw.net/emoticons/v1/" + emoteID + "/2.0", emoteCount));
        //        //}
        //    }
        //}
        return output;
    }

    void HideMenu()
    {
        //[TODO] Change how this work in favor of disabling individual component so the menu script can reenable stuff if necessary
        //[TODO] Example: on gaining focus enable ability to interact with program through the transparentapp.cs (needs extra code) and show buttons (this should work already with script on menu)

        //GameObject connectButton = GameObject.Find("Connect");
        //connectButton.gameObject.SetActive(false);
        GameObject menuCanvas = GameObject.Find("Canvas Menu");
        menuCanvas.gameObject.SetActive(false); 
    }

    void LoadCredentials()
    {
        String currentPath = GlobalVars.folderPath + @"Configurable\Logins\Twitch.txt";

        if (File.Exists(currentPath))
        {
            using (StreamReader reader = new StreamReader(currentPath))
            {
                String line;
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        line = line.Replace(" ", "");
                        if (line.StartsWith("twitch_OAuth_Token="))
                        {
                            line = line.Replace("twitch_OAuth_Token=", "");
                            twitch_OAuth_Token = line;
                        }
                        if (line.StartsWith("twitch_Username="))
                        {
                            line = line.Replace("twitch_Username=", "");
                            GlobalVars.twitch_Username = line.ToLower();
                        }
                        if (line.StartsWith("twitch_Channel_To_Listen_To="))
                        {
                            line = line.Replace("twitch_Channel_To_Listen_To=", "");
                            GlobalVars.twitch_Channel_To_Listen_To = line.ToLower();
                        }
                    }
                    catch
                    {
                        Debug.Log(String.Format("Error reading data from the Twitch Login file at Line:{0}", line));
                    }
                }
            }
        }
        else
        {
            CreateCredentials();
        }
    }

    void PongResponse()
    {
        _writer.WriteLine("PONG");
        _writer.Flush();
        Debug.Log("PONG response");
    }

    void ParseChat()
    {
        if (_twitchClient.Available > 0)
        {
            string message = _reader.ReadLine();
            Debug.Log(message);
            // Twitch sends a PING message every 5 minutes or so. We MUST respond back with PONG or we will be disconnected 
            if (message.StartsWith("PING"))
            {
                PongResponse();
                return;
            }
            if (message.StartsWith(":"))
            {
                // Add viewer to list on joining stream
                if (message.Contains(string.Format("JOIN #{0}", GlobalVars.twitch_Channel_To_Listen_To)))
                {
                    ViewerList_Add(message);
                    return;
                }
                // Remove viewer from list on leaving stream
                if (message.Contains(string.Format("PART #{0}", GlobalVars.twitch_Channel_To_Listen_To)))
                {
                    ViewerList_Remove(message);
                    return;
                }
                // Create list of viewers on connect [TODO] Make sure if it spans multiple lines to get all viewers
                if (message.StartsWith(string.Format(":{0}.tmi.twitch.tv 353", GlobalVars.twitch_Channel_To_Listen_To)))
                {
                    ViewerList_Create(message);
                    return;
                }
                // Sets connection boolean to true and sends bot connected message
                if (message.StartsWith(":tmi.twitch.tv 001"))
                {
                    isConnected = true;
                    TwitchSendMessage(GlobalVars.bot_Connect_Message);
                    TransparentApp tApp_script = GameObject.Find("Scripts Bot").GetComponentInChildren<TransparentApp>();
                    tApp_script.Activate();
                    HideMenu();
                    return;
                }
                // Twitch told us to reconnect, may reconnect too quickly so double check that.
                if (message.StartsWith(":tmi.twitch.tv RECONNECT"))
                {
                    if (reconnectTimer != null)
                    {
                        reconnectTimer.Stop();
                        reconnectTimer.Dispose();
                        Debug.Log("Ending Reconnect Timer");
                    }
                    isConnected = false;
                    isReconnecting = false;
                    SetReconnectTimer();
                }
                return;
            }
            if (message.Contains("CLEARCHAT"))
            {
                int count = message.Length - message.Replace(":", "").Length;
                //Clear All
                if (count == 1)
                {
                    chatParser.ClearAllMessages();
                    return;
                }
                //Clear by User
                if (count >= 2)
                {
                    int index;
                    string user;
                    index = message.IndexOf(":");
                    user = message.Substring(index + 1);
                    index = user.IndexOf(":");
                    user = user.Substring(index + 1);
                    chatParser.ClearUserMessages(user);
                    return;
                }
                return;
            }            
            if (message.Contains("CLEARMSG"))
            {
                int index;
                string[] tags = message.Split(';');
                string user = "";
                string deleteMessage;
                user = Tags_IndexAfter("@login=", ";", message);
                index = message.IndexOf(":");
                deleteMessage = message.Substring(index + 1);
                index = deleteMessage.IndexOf(":");
                deleteMessage = deleteMessage.Substring(index + 1);
                chatParser.ClearSingleMessage(user, deleteMessage);
                return;
            }
            if (message.Contains("USERNOTICE")) //[TODO] Implement Subscription logic
            {

            }    
            if (message.Contains("PRIVMSG"))
            {
                //int splitPoint;
                var tags = Tags_Create(message);

                var messageID = Tags_IndexAfter("id=", ";", tags);

                var nameColor = Tags_IndexAfter("color=", ";", tags);
                if (nameColor == "")
                {
                    //nameColor = "#FFFFFF";
                    // Random dark mode name color (Looks cool, but can easily get confusing)
                    //nameColor = twitchDarkmodeNameColors.ElementAt(UnityEngine.Random.Range(0, twitchDarkmodeNameColors.Count() - 1)).Value;
                    nameColor = "#9244ff"; //Twitch's glitch logo color
                }
                var firstMessage = Tags_IndexAfter("first-msg=", ";", tags);
                //if (twitchDarkmodeNameColors.ContainsKey(nameColor))
                //{
                //    nameColor = twitchDarkmodeNameColors[nameColor];
                //}
                var displayName = Tags_IndexAfter("display-name=", ";", tags);
                var userName = displayName.ToLower();


                // users message
                message = message.Substring(message.IndexOf("PRIVMSG") + 7);
                message = message.Substring((message.IndexOf(":") + 1));

                // remove ability to bypass noparse for user submitted text, completely skips message
                if (message.Replace(" ", "").Contains("</noparse>"))
                {
                    //message.Replace("</noparse>", " "); //Doing it this way allowed for users to use spaces to bypass the check and still function
                    return;
                }

                // if emotes parse emotes
                Emotes_Twitch_Parse(tags);

                var data = new ChatMessageData
                {
                    MessageID = messageID,
                    NameColor = nameColor,
                    DisplayName = displayName,
                    UserName = userName,
                    Message = message,
                    Command = "",
                    Tags = tags,
                    IsFirstMessage = false,
                    IsHighlightedMessage = false,
                    IsItalicized = false
                };

                if (tags.Contains("custom-reward-id="))
                {
                    return;
                }

                if (message.StartsWith(GlobalVars.bot_Command_Prefix))
                {

                }
                else
                {
                    if (chatParser.chat_Users_To_Ignore.Contains(userName))
                    {
                        return;
                    }
                    //[TODO] Ignore messages starting with list of user input words
                    //[TODO] Ignore messages containing list of user input words
                    //if (message.Length == 1)
                    //{
                    //    if (message == "w" || message == "a" || message == "s" || message == "d" || message == "r" || message == "u")
                    //    {
                    //        return;
                    //    }
                    //}
                    //if (message.Length == 2)
                    //{
                    //    if (message.Substring(0, 1) == "w" || message.Substring(0, 1) == "a" || message.Substring(0, 1) == "s" || message.Substring(0, 1) == "d" || message.Substring(0, 1) == "r" || message.Substring(0, 1) == "u")
                    //    {
                    //        if (message.Substring(1) == "1" || message.Substring(1) == "2" || message.Substring(1) == "3" || message.Substring(1) == "4" || message.Substring(1) == "5" || message.Substring(1) == "6" || message.Substring(1) == "7" || message.Substring(1) == "8" || message.Substring(1) == "9")
                    //        {
                    //            return;
                    //        }
                    //    }
                    //}










                    if (userName == "pokemoncommunitygame")
                    {
                        //[TODO] Add bool for disabling this
                        if (data.Message.Contains("A wild") && data.Message.Contains("appears"))
                        {
                            string[] sArray = data.Message.Split(' ');
                            int ttsStart = 0;
                            int ttsEnd = 0;
                            ttsStart = Array.IndexOf(sArray, "wild");
                            ttsStart += 1;
                            ttsEnd = Array.IndexOf(sArray, "appears");
                            string speechText = "";
                            while (ttsStart != ttsEnd)
                            {
                                speechText = speechText + " " + sArray[ttsStart];
                                ttsStart += 1;
                            }
                            speechText = "!k A wild" + speechText + " appears!";
                            data.Message = speechText;
                            chatParser.NewMessage(data);
                        }
                        return;
                    }
                    //ChatBot.ResetClearChatTimer();
                }
                //Attempt to filter bot / scam messages.
                if (Tags_IndexAfter("first-msg=", ";", data.Tags) == "1")
                {
                    data.IsFirstMessage = true;
                    var lowerCaseMessage = data.Message.ToLower();
                    lowerCaseMessage = lowerCaseMessage.Replace(" ", "");
                    string[] splitMessage = data.Message.Split();
                    foreach (string word in splitMessage)
                    {
                        if (word.Contains("/"))
                        {
                            if (word.Contains("."))
                            {
                                return;
                            }
                        }
                    }
                    if (lowerCaseMessage.Contains("best"))
                    {
                        if (lowerCaseMessage.Contains("viewer") || lowerCaseMessage.Contains("follower"))
                        {
                            return;
                        }
                    }
                    if (lowerCaseMessage.Contains("buy") || lowerCaseMessage.Contains("get") || lowerCaseMessage.Contains("offer") || lowerCaseMessage.Contains("cheap"))
                    {
                        if (lowerCaseMessage.Contains("follow") || lowerCaseMessage.Contains("view"))
                        {
                            return;
                        }
                    }
                }
                if (data.Tags.Contains("msg-id=highlighted-message"))
                {
                    data.IsHighlightedMessage = true;
                }
                if (data.Message.StartsWith("\u0001ACTION"))
                {
                    data.IsItalicized = true;
                    data.Message = data.Message.Substring(8, (data.Message.Length - 9));
                }


                chatParser.NewMessage(data);
                return;
            }

        }
    }

    void Reconnect(System.Object source, ElapsedEventArgs e)
    {
        if (isConnected == true)
        {
            reconnectTimer.Stop();
            reconnectTimer.Dispose();
            Debug.Log("Connected... stopping reconnect timer");
        }
        else if (isReconnecting == false)
        {
            isReconnecting = true;
            Debug.Log("Disconnecting due to timer.");
            try
            {
                Disconnect();
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
            return;
        }
        else if (isReconnecting == true)
        {
            isReconnecting = false;
            Debug.Log("Attempting reconnect!");
            Connect();
        }
    }

    void SetReconnectTimer()
    {
        reconnectTimer = new System.Timers.Timer(4000);
        reconnectTimer.Elapsed += Reconnect;
        reconnectTimer.AutoReset = true;
        reconnectTimer.Enabled = true;
    }

    string Tags_Create(string s)
    {
        var output = s.Substring(0, s.IndexOf("PRIVMSG"));
        //string subscriber = Tags_IndexAfter("subscriber=", ";", tags);
        //Debug.Log(subscriber);
        return output;
    }

    string Tags_IndexAfter(string find, string to, string s)
    {
        s = s.Substring((s.IndexOf(find) + find.Count()));
        s = s.Substring(0, s.IndexOf(@to));
        return s;
    }

    void ViewerList_Create(string s)
    {
        string viewers = s.Substring(s.IndexOf(":") + 1);
        viewers = viewers.Substring(viewers.IndexOf(":") + 1);
        var tempList = viewers.Split(' ').ToList();
        foreach (var t in tempList)
        {
            if (viewerList.Contains(t) == false)
            {
                viewerList.Add(t);
                //Debug.Log("Added User to viewerlist: " + t);
            }
        }

        //string testOutput = "";
        //foreach (string viewer in viewerList)
        //{
        //    testOutput = testOutput + viewer + ", ";
        //}
        //Debug.Log(testOutput);
    }

    void ViewerList_Add(string s)
    {
        string userID = s.Substring(1, s.IndexOf("!") - 1);
        if (viewerList.Contains(userID) == false)
        {
            viewerList.Add(userID);
            //Debug.Log("User Joined: " + userID);
        }
    }

    void ViewerList_Remove(string s)
    {
        string userID = s.Substring(1, s.IndexOf("!") - 1);
        if (viewerList.Contains(userID) == true)
        {
            viewerList.Remove(userID);
            //Debug.Log("User Left: " + userID);
        }
    }

}
