using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Linq;
using UnityEngine.Video;

public class VFX : MonoBehaviour
{
    public int vfx_Default_X = 25; //[TODO] Make these work with VFX < full screen size
    public int vfx_Default_Y = 0; //[TODO] Not currently functional, but found URL with how to detect videos dimensions https://stackoverflow.com/questions/45307892/accessing-dimensions-width-height-from-a-videoplayer-source-url
    public string vfx_Folder_Path = "";
    public GameObject vfx_Folder_Disabledicon;
    public int vfx_Limit = 5;
    public float vfx_Master_Vol = .5f;


    public AudioSource vfxAudioSource;
    string command = "v"; //[TODO] User Defined
    
    int vfxSizeCap = 300; //[TODO] User Defined
    GameObject vfxPrefab;
    GameObject vfxCanvas;
    Camera vfxCamera;
    Dictionary<string, string> videoDictionary = new Dictionary<string, string>();
    string videoPath = @"C:\OBS Assets\VFX\MOV\Processed\";
    float vfxZValue = 500f;
    List<GameObject> vfxCurrentlyPlaying = new List<GameObject>();
    Dictionary<string, float> vfxCustomVolume = new Dictionary<string, float> { };


    void Awake()
    {
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
        GetLocalVideos();
        vfx_Folder_Disabledicon = GameObject.Find("VFXFolder");
        vfxPrefab = GameObject.Find("VFX_Cube");
        vfxCanvas = GameObject.Find("Canvas VFX Objects");
        vfxCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {

        //vfxAudioSource = GameObject.Find("Audio Normal").GetComponent<AudioSource>();

        //[TODO] Get screen Resolution, set canvas width / height to that. 
        //[TODO] Set VFX_Cube to screen resolution
        //[TODO] Add viewer ability to define width/height/rotation/position when sending VFX
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void Reload()
    //{
    //    vfxPrefab.GetComponent<VideoPlayer>().SetDirectAudioVolume(0, GlobalVars.master_Vol_VFX);
    //}

    void GetLocalVideos()
    {
        if (Directory.Exists(videoPath))
        {
            DirectoryInfo dir = new DirectoryInfo(videoPath);
            var allowedExtensions = new[] { ".webm" };

            var results = dir.EnumerateFiles();

            foreach (var result in results.Where(file => allowedExtensions.Any(file.Name.ToLower().EndsWith)))
            {
                string s;
                s = result.ToString();
                s = s.Replace(videoPath, "");
                s = s.Replace(".webm", "");
                s = s.ToLower();
                string path;
                path = result.ToString();
                path = path.Replace("\\", "/");
                if (!videoDictionary.ContainsKey(s))
                {
                    videoDictionary.Add(s, "file://" + path);
                }
                //Debug.Log(s + ", " + videoDictionary[s]);
            }
        }
    }

    void loopPointReached(VideoPlayer v)
    {
        v.Stop();
        vfxCurrentlyPlaying.Remove(v.gameObject);
        if (vfxCurrentlyPlaying.Count() == 0)
            vfxZValue = 500f;
        Destroy(v.gameObject);
    }
    public void Reload()
    {
        vfxCustomVolume.Clear();
        LoadVFXVolumesFromFile();
    }
    //[TODO] Load Videos to RAM to make playing multiple easier on computer
    public void Play(string s)
    {
        if (vfxCurrentlyPlaying.Count() >= vfx_Limit)
            return;

        string vfx;
        vfx = s.Split()[0];
        vfx = vfx.Trim();

        Debug.Log(vfx);

        if (!videoDictionary.ContainsKey(vfx))
            return;

        int vfxX = 0;
        int vfxY = 0;
        int vfxRY = 0; //To Flip VFX
        int vfxR = 0; //Rotation Z Axis
        int vfxS = 100; // Full video size by default
        int vfxSX = vfxS;
        int vfxSY = vfxS;
        int vfxSZ = 1; //Set to -1 & set RY to 180 to flip
        bool xHasBeenSet = false;
        bool YHasBeenSet = false;



        Debug.Log("conditionals = " + s.Split().Count());
        if (s.Split().Count() >= 2)
        {
            Debug.Log("vfx with user input conditionals");
            string[] items = s.Split();
            items = items.Skip(1).ToArray();
            for (int i = 0; i < items.Length; i++)
                items[i] = items[i].ToLower();
            foreach (string item in items)
            {
                Debug.Log(item);
                if (item.StartsWith("f") || item.StartsWith("flip"))
                {
                    vfxRY = 180;
                    vfxSZ = -1;
                    continue;
                }

                if (item.StartsWith("sx"))
                {
                    int i;
                    bool succeeded;
                    succeeded = int.TryParse(item.Replace("sx", "").Trim(), out i);
                    if (succeeded == false)
                    {
                        continue;
                    }
                    if (i <= 0)
                    {
                        i = 1;
                    }
                    else if (i >= vfxSizeCap)
                    {
                        i = vfxSizeCap;
                    }
                    vfxSX = i;
                    continue;
                }

                if (item.StartsWith("sy"))
                {
                    int i;
                    bool succeeded;
                    succeeded = int.TryParse(item.Replace("sy", "").Trim(), out i);
                    if (succeeded == false)
                    {
                        continue;
                    }
                    if (i <= 0)
                    {
                        i = 1;
                    }
                    else if (i >= vfxSizeCap)
                    {
                        i = vfxSizeCap;
                    }
                    vfxSY = i;
                    continue;
                }

                if (item.StartsWith("s"))
                {
                    int i;
                    bool succeeded;
                    succeeded = int.TryParse(item.Replace("s", "").Trim(), out i);
                    if (succeeded == false)
                    {
                        continue;
                    }
                    if (i <= 0)
                    {
                        i = 1;
                    }
                    else if (i >= vfxSizeCap)
                    {
                        i = vfxSizeCap;
                    }
                    vfxSX = i;
                    vfxSY = i;
                    continue;
                }

                if (item.StartsWith("r"))
                {
                    int i;
                    bool succeeded;
                    succeeded = int.TryParse(item.Replace("r", "").Trim(), out i);
                    if (succeeded == false)
                    {
                        continue;
                    }
                    if (i <= 0)
                    {
                        i = 0;
                    }
                    else if (i >= 360)
                    {
                        i = 360;
                    }
                    vfxR = i;
                    continue;
                }

                if (item.StartsWith("x"))
                {
                    xHasBeenSet = int.TryParse(item.Replace("x", "").Trim(), out vfxX);
                    continue;
                }

                if (item.StartsWith("y"))
                {
                    YHasBeenSet = int.TryParse(item.Replace("y", "").Trim(), out vfxY);
                    continue;
                }

                if (xHasBeenSet == true && YHasBeenSet == false)
                {
                    //[TODO] Find out how to find the size of the VFX if equal to screen size set i=0 else set to GlobalVars.vfx_Default_Y
                    YHasBeenSet = int.TryParse(item.Trim(), out vfxY);
                    continue;
                }

                if (xHasBeenSet == false)
                {
                    //[TODO] Find out how to find the size of the VFX if equal to screen size set i=0 else set to GlobalVars.vfx_Default_X
                    xHasBeenSet = int.TryParse(item.Trim(), out vfxX);
                    continue;
                }
            }
        }



        GameObject newVFX = Instantiate(vfxPrefab, vfxCanvas.transform, worldPositionStays: false);
        vfxCurrentlyPlaying.Add(newVFX);
        //newVFX.GetComponent<VideoPlayer>().SetTargetAudioSource(1, vfxAudioSource);
        Debug.Log(vfxZValue);
        vfxZValue = vfxZValue - 1f; //Ensures the lastest VFX is always on top
        if (vfxZValue <= 0)
            vfxZValue = 499f;
        Debug.Log(vfxZValue);

        if (vfxCustomVolume.ContainsKey(vfx))
        {
            newVFX.GetComponent<VideoPlayer>().SetDirectAudioVolume(0, vfx_Master_Vol * vfxCustomVolume[vfx]);
        }
        newVFX.transform.position = new Vector3(vfxCanvas.GetComponent<RectTransform>().rect.width * (float)(vfxX * .01), vfxCanvas.GetComponent<RectTransform>().rect.height * (float)(vfxY * .01), vfxZValue);
        newVFX.transform.localScale = new Vector3(vfxCanvas.GetComponent<RectTransform>().rect.width * (float)(vfxSX * .01), vfxCanvas.GetComponent<RectTransform>().rect.height * (float)(vfxSY * .01), vfxSZ);
        newVFX.transform.Rotate(0, vfxRY, vfxR, Space.World);
        newVFX.GetComponent<VideoPlayer>().url = videoDictionary[vfx];
        newVFX.GetComponent<VideoPlayer>().loopPointReached += loopPointReached;

        //[TODO] Complex math to make sure VFX is on screen and if not, get it on screen
    }

    public void ParseMessageForCommand(ChatMessageData data)
    {
        string[] splitMessage = data.Message.Split(' ');

        if ((splitMessage[0].ToLower() != GlobalVars.bot_Command_Prefix + command) || (splitMessage.Count() <= 1))
            return;
        string vfx = data.Message.Replace(GlobalVars.bot_Command_Prefix + command, "");
        vfx = vfx.Trim();
        vfx = vfx.ToLower();
        Play(vfx);
    }

    void LoadVFXVolumesFromFile()
    {
        String currentPath = GlobalVars.folderPath + @"Configurable\VFX_Volumes.txt";

        //[TODO] Create file with VFX example and a // line to explain any extra details
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
                            SetVolumeFromFile(line);
                        }
                    }
                    catch
                    {
                        Debug.Log(String.Format("Error reading data from the VFX_Volumes file at Line:{0}", line));
                    }
                }
            }
        }
    }
    void SetVolumeFromFile(String line)
    {
        String vfxName;
        String userInput;
        bool canConvert;
        float vol;
        if (line.StartsWith("//"))
        {
            return;
        }
        if (line.IndexOf("=") + 1 > line.Length)
        {
            Debug.Log("VFX String Not Long Enough, " + line.IndexOf("=" + 1) + " VS " + line.Length);
            return;
        }
        userInput = line.Substring(line.IndexOf("=") + 1);
        userInput = userInput.ToLower();
        userInput = userInput.Replace(" ", "");
        vfxName = line.Substring(0, line.IndexOf("="));
        vfxName = vfxName.ToLower();
        vfxName = vfxName.Replace(" ", "");
        canConvert = float.TryParse(userInput, out vol);
        if (!canConvert)
        {
            Debug.Log("Couldn't convert vol to float " + userInput);
            return;
        }
        Debug.Log(vfxName + " " + userInput);
        vfxCustomVolume.Add(vfxName, vol);
    }

}
