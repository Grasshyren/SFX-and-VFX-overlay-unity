using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Linq;
using UnityEngine.Networking;

public class SFX : MonoBehaviour
{
    public float sfx_Master_Vol = .5f;
    public string sfx_Folder_Path = "";
    public GameObject sfx_Folder_Disabledicon;

    public string soundEndSymbol = ")"; //[TODO] Allow user to change. Default )
    public string soundStartSymbol = "("; //[TODO] Allow user to change. Default (
    public string command = "s"; //[TODO] Allow user to change.
    public AudioSource audioSFX;
    AudioClip clip;
    Dictionary<string, AudioClip> soundsAsAudioClips = new Dictionary<string, AudioClip>();
    Dictionary<string, string> soundList = new Dictionary<string, string>();
    List<string> soundEffectQueue = new List<string>();
    string lastUser;
    string lastSFX;
    string queueDuplicate;
    //[TODO] Let user define sound folder possibly using windows folder selector, then sound list
    string soundFolderPath = @"C:\OBS Assets\SoundEffects";


    void Awake()
    {
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
        audioSFX = GameObject.Find("Audio Normal").GetComponent<AudioSource>();
        sfx_Folder_Disabledicon = GameObject.Find("SFXFolder");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // update is called once per frame
    void Update()
    {
        if (soundEffectQueue.Count > 0)
        {
            if (!audioSFX.isPlaying)
            {
                Play(soundEffectQueue[0]);
                soundEffectQueue.RemoveAt(0);
            }
        }
    }

    public void Clear()
    {
        audioSFX.Stop();
        soundEffectQueue.Clear();
        //ChatBot.SendChatMessage("SFX queue cleared successfully");
    }

    public void Reload()
    {
        soundList.Clear();
        GetLocalSounds();
    }

    //Populate all playable sounds from user defined folder
    public void GetLocalSounds()
    {
        if (Directory.Exists(soundFolderPath))
        {
            DirectoryInfo dir = new DirectoryInfo(soundFolderPath);
            var allowedExtensions = new[] { ".mp3", ".ogg", ".wav", ".flac", ".aiff", ".aif", ".mod", ".it", ".s3m", ".xm" };
            var results = dir.EnumerateFiles();
            string soundKey;

            foreach (var result in results.Where(file => allowedExtensions.Any(file.Name.ToLower().EndsWith)))
            {
                soundKey = result.Name.ToString().ToLower().Substring(0, result.Name.ToString().LastIndexOf(".")); // makes all lowercase and removes the file extension
                if (!soundList.ContainsKey(soundKey)) //Doesn't allow duplicate files, this is most likely to occur with different file extensions but same name
                    soundList.Add((soundKey.Replace(" ", "")), result.ToString()); //Remove spaces from all call codes to make it easier for viewers
                //Debug.Log(result);
                //Debug.Log(soundList.Last());

            }
            //StartCoroutine(PlaySoundNow());
        }
    }

    public void ParseMessageForSymbols(ChatMessageData chatMessageData)
    {
        bool containsSymbols = false;
        if (chatMessageData.Message.Contains(soundStartSymbol) && chatMessageData.Message.Contains(soundEndSymbol))
        {
            containsSymbols = true;
        }
        if (containsSymbols == false)
        {
            return;
        }
        //[TODO] When Queue gets implemented change this from return to break
        if (chatMessageData.Message.IndexOf(soundStartSymbol) > chatMessageData.Message.IndexOf(soundEndSymbol))
        {
            return;
        }
        int locator;
        string sound;
        locator = chatMessageData.Message.IndexOf(soundStartSymbol) + soundStartSymbol.Length;
        sound = chatMessageData.Message.Substring(locator, chatMessageData.Message.IndexOf(soundEndSymbol) - locator);
        sound = sound.Replace(" ", "");
        if (sound.IndexOfAny(new char[] { '!', '?', '.', ',', '`', '~', '\'', '\"' }) >= 0)
        {
            sound = sound.Replace("!", "");
            sound = sound.Replace("?", "");
            sound = sound.Replace(".", "");
            sound = sound.Replace(",", "");
            sound = sound.Replace("`", "");
            sound = sound.Replace("~", "");
            sound = sound.Replace("\'", "");
            sound = sound.Replace("\"", "");
            Debug.Log("Extra replaces were activated");
        }
        sound = sound.ToLower();

        // [TODO] Add sound to queue
        StartCoroutine(Test(sound));
    }

    public void ParseMessageForCommand(ChatMessageData data)
    {
        string[] splitMessage = data.Message.Split(' ');

        if ((splitMessage[0].ToLower() != GlobalVars.bot_Command_Prefix + command) || (splitMessage.Count() <= 1))
            return;
        string sound = splitMessage[1];
        sound = sound.Trim();
        sound = sound.ToLower();

        // [TODO] Add sound to queue
        StartCoroutine(Test(sound));
    }


    public void Play(string s)
    {
        clip = Resources.Load<AudioClip>(soundFolderPath + s);
        if (clip != null)
        {
            //audioSFX.PlayOneShot(clip, 1);
            audioSFX.clip = clip;
            audioSFX.Play();
        }
        else
        {
            //ChatBot.SendChatMessage(s + " wasn't found, the list of Sounds: https://chippermonkey.com/stream-sound-board/");
        }
    }
    public void PlayNow(string userID, string s)
    {
        if (userID == lastUser && s == lastSFX)
        {
            return;
        }
        clip = Resources.Load<AudioClip>(soundFolderPath + s);
        if (clip != null)
        {
            lastSFX = s;
            lastUser = userID;
            audioSFX.PlayOneShot(clip, 1.0F);
        }
        else
        {
            //ChatBot.SendChatMessage(s + " wasn't found, the list of Sounds: https://chippermonkey.com/stream-sound-board/");
        }
    }

    public void Queue(string userID, string s)
    {
        soundEffectQueue.Add(s);
        //CurrencyHandler.MinusCurrencyIndividual(userID, 20);
    }




    
    //IEnumerator PlaySoundNow()
    //{
    //    AudioClip sound;
    //    UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(soundList.Last(), AudioType.MPEG);
    //    yield return req.SendWebRequest();
    //    sound = DownloadHandlerAudioClip.GetContent(req);
    //    audioSFX.PlayOneShot(sound, 1.0F);
    //}

    IEnumerator Test(string s)
    {
        Debug.Log("we made it to test with " + s);
        if (!soundList.ContainsKey(s))
            yield break;
        if (!soundsAsAudioClips.ContainsKey(s))
        {
            AudioClip sound;
            UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(soundList[s], AudioType.MPEG);
            yield return req.SendWebRequest();
            sound = DownloadHandlerAudioClip.GetContent(req);
            soundsAsAudioClips.Add(s, sound);
            Debug.Log("Added " + s + " to RAM.");
        }
        audioSFX.PlayOneShot(soundsAsAudioClips[s], 1.0F * sfx_Master_Vol);
    }
}
