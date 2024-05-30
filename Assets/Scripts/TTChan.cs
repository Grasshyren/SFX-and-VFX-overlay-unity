using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SpeechLib;

public class TTChan : MonoBehaviour
{
    public int tts_Master_Volume = 75;

    public List<string> commands = new List<string> {"k", "t"}; //[TODO] User input

    ChatParser chatParser;
    SpVoice currentVoice = new SpVoice();
    List<ChatMessageData> ttsQueue = new List<ChatMessageData> { };
    int speakRate = 0; // min -10 max 10

    //[TODO] Delete these
    //int defaultTTChanVol = 75;


    void Awake()
    {
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
    }
    // Start is called before the first frame update
    void Start()
    {
        chatParser = GameObject.Find("Scripts Bot").GetComponentInChildren<ChatParser>();


        //foreach (SpObjectToken sp in currentVoice.GetVoices())
        //{
        //    numberOfVoices += 1;
        //    sp.GetDescription();
        //    Debug.Log(sp.GetDescription());
        //}
        //Debug.Log(numberOfVoices);
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    var donespeaking = currentVoice.SpeakCompleteEvent();
        //    Debug.Log(donespeaking);

        //    currentVoice.Rate = speakRate;
        //    currentVoice.Volume = speakVolume;
        //    currentVoice.Speak("You want some pie?", SpeechVoiceSpeakFlags.SVSFlagsAsync | SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak); //[TODO] Remove this

        //    donespeaking = currentVoice.SpeakCompleteEvent();
        //    Debug.Log(donespeaking);
        //}
    }

    public void ParseMessageForCommand(ChatMessageData data)
    {
        Speak(data);
    }

    void Speak(ChatMessageData data)
    {
        string[] split = data.Message.Split(' ');
        if ((!split[0].StartsWith(GlobalVars.bot_Command_Prefix)) || (split.Length <= 1))
        {
            return;
        }
        if (!commands.Contains(split[0].Replace(GlobalVars.bot_Command_Prefix, "")))
        {
            return;
        }
        int currentSpeakRate = speakRate;
        if (data.Message.Contains("rate"))
        {
            foreach(string s in split)
            {
                if (s.StartsWith("rate"))
                {
                    string rateAsString;
                    rateAsString = s.Replace("rate", "");
                    int tempNumber;
                    bool canConvert = int.TryParse(rateAsString, out tempNumber);
                    if (canConvert == true)
                    {
                        
                        if (tempNumber < -10)
                        {
                            currentSpeakRate = -10;
                        }
                        else if (tempNumber > 10)
                        {
                            currentSpeakRate = 10;
                        }
                        else
                        {
                            currentSpeakRate = tempNumber;
                        }
                        Debug.Log(currentSpeakRate);
                        data.Message = data.Message.Replace(" " + s, "");
                    }
                }
            }
        }

        if (data.Message.StartsWith("!k") || data.Message.StartsWith("!K"))
        {
            data.DisplayName = data.DisplayName + "<sprite=\"grassh5TTChan\" index=0>";
            //currentVoice.Voice = currentVoice.GetVoices().Item(0);
            currentVoice.Rate = currentSpeakRate;
            currentVoice.Volume = tts_Master_Volume;
            data.Message = data.Message.Replace("!k", "");
            data.Message = data.Message.Replace("!K", "");
            currentVoice.Speak(data.Message, SpeechVoiceSpeakFlags.SVSFlagsAsync | SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak);
            chatParser.ParseMessageForDisplay(data);
        }
        if (data.Message.StartsWith("!t") || data.Message.StartsWith("!t"))
        {
            //currentVoice.Voice = currentVoice.GetVoices().Item(1);
            //currentVoice.Volume = defaultTTChanVol;
            data.DisplayName = data.DisplayName + "<sprite=\"grassh5TTChan\" index=0>";
            currentVoice.Rate = currentSpeakRate;
            currentVoice.Volume = tts_Master_Volume;
            data.Message = data.Message.Replace("!t", "");
            data.Message = data.Message.Replace("!T", "");
            currentVoice.Speak(data.Message, SpeechVoiceSpeakFlags.SVSFlagsAsync | SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak);
            chatParser.ParseMessageForDisplay(data);
        }

    }
}
