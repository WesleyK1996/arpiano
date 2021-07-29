using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

using Button = UnityEngine.UI.Button;
using Application = UnityEngine.Application;
using Directory = System.IO.Directory;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;

public class SelectSong : MonoBehaviour
{
    public GridLayoutGroup GLG;
    public GameObject chooseSong;

    [Header("Options stuff")]
    public GameObject options;
    public Slider playbackSpeed;
    public TMP_InputField playbackSpeedValue;
    public TMP_InputField amountOfKeys;
    public TMP_Dropdown startKey;
    public TMP_Dropdown wrongKeyPress;
    public Slider waitToStart;
    public TMP_InputField waitToStartValue;

    public static string selectedSong;
    public Text stuffthinggedoe;

    string path;
    char separator;

    // Start is called before the first frame update
    void Start()
    {
        separator = Path.AltDirectorySeparatorChar;
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
#if UNITY_ANDROID //&& !UNITY_EDITOR
        //string textPath = Path.Combine(Application.dataPath, "Resources", "Text");
        yield return StartCoroutine(WriteResoureToPersistentDataPath("Bohemian Rhapsody midi"));
        yield return StartCoroutine(WriteResoureToPersistentDataPath("Muusika - Part Uusberg"));
        yield return StartCoroutine(WriteResoureToPersistentDataPath("Steal away - Howard Helvey"));
#endif
        yield return StartCoroutine(CheckAppData());

        SetCellSize();
        GetSongs();
        yield return null;
    }

#if UNITY_ANDROID //&& !UNITY_EDITOR
    private IEnumerator WriteResoureToPersistentDataPath(string fileName)
    {
        //FileInfo file = new FileInfo(path);
        TextAsset textAsset = (TextAsset)Resources.Load(Path.Combine("Text", fileName), typeof(TextAsset));
        print(textAsset.text);
        print(Path.Combine(Application.persistentDataPath, "Music", fileName));
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "Music", fileName + ".txt"), textAsset.text);
        //DirectoryInfo dir = new DirectoryInfo(path);
        //FileInfo[] files = dir.GetFiles();

        //foreach (FileInfo file in files)
        //{
        //    if (file.Extension == ".txt")
        //    {
        //        print(Path.Combine("Text", file.Name.Replace(".txt", "")));
        //        TextAsset textAsset = (TextAsset)Resources.Load(Path.Combine("Text", file.Name.Replace(".txt", "")), typeof(TextAsset));
        //        File.WriteAllText(Path.Combine(Application.persistentDataPath, "Music", file.Name), textAsset.text);
        //    }
        //}
        //PlayerPrefs.SetInt("Stored", 1);
        yield return null;
    }
#endif

    private IEnumerator CheckAppData()
    {
        string musicFolder = Path.Combine(Application.persistentDataPath, "Music");
        if (!Directory.Exists(musicFolder))
            Directory.CreateDirectory(musicFolder);
        FileInfo[] files = new DirectoryInfo(musicFolder).GetFiles();
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Extension != ".txt" && files[i].Extension != ".meta")
                File.Delete(Path.Combine(musicFolder, files[i].Name));
        }
        yield return null;
    }

    public void TapToStartPressed()
    {
        transform.Find("TapToStart").gameObject.SetActive(false);
        chooseSong.SetActive(true);
    }

    void SetCellSize()
    {
        RectTransform rt = GLG.GetComponent<RectTransform>();
        GLG.cellSize = new Vector2(rt.rect.width / 2, rt.rect.height / 4);
    }

    void GetSongs()
    {
        string path = Path.Combine(Application.persistentDataPath, "Music");
        DirectoryInfo d = new DirectoryInfo(path);
        foreach (FileInfo file in d.GetFiles("*.txt"))
        {
            if (file.Name != "song.txt")
            {
                Button b = Instantiate(Resources.Load(Path.Combine("Prefabs", "Song")) as GameObject, GLG.transform).GetComponent<Button>();
                b.onClick.AddListener(delegate { OnSongButtonClick(file.Name); });
                b.transform.GetChild(0).GetComponent<Text>().text = file.Name.Replace(".txt", "");
            }
        }
    }

    public void OnSongButtonClick(string s)
    {
        selectedSong = Path.Combine(Application.persistentDataPath, "Music", s);
        SceneManager.LoadScene("SampleScene");
    }

    public void ToggleOptions()
    {
        options.SetActive(!options.activeSelf);
        chooseSong.SetActive(!chooseSong.activeSelf);

        if (options.activeSelf)
        {
            if (PlayerPrefs.HasKey("playbackSpeed"))
            {
                playbackSpeed.value = PlayerPrefs.GetFloat("playbackSpeed");
                playbackSpeedValue.text = "" + PlayerPrefs.GetFloat("playbackSpeed");
            }
            if (PlayerPrefs.HasKey("amountOfKeys"))
                amountOfKeys.text = "" + PlayerPrefs.GetInt("amountOfKeys");
            if (PlayerPrefs.HasKey("startKey"))
                startKey.value = PlayerPrefs.GetInt("startKey");
            if (PlayerPrefs.HasKey("wrongKeyPress"))
                wrongKeyPress.value = PlayerPrefs.GetInt("wrongKeyPress");
            if (PlayerPrefs.HasKey("waitToStart"))
            {
                waitToStart.value = PlayerPrefs.GetFloat("waitToStart");
                waitToStartValue.text = "" + PlayerPrefs.GetFloat("waitToStart");
            }
        }
        else
        {
            foreach (Transform child in GLG.transform)
                Destroy(child.gameObject);
            GetSongs();
        }
    }

    public void OnPlaybackspeedValueChange(float v)
    {
        playbackSpeedValue.text = v.ToString("f2");
    }

    public void OnPlaybackspeedValueChange(string v)
    {
        try
        {
            playbackSpeed.value = float.Parse(v);
        }
        catch
        {
            playbackSpeed.value = 1;
        }
        PlayerPrefs.SetFloat("playbackSpeed", playbackSpeed.value);
    }

    public void OnKeyAmountValueChange(string v)
    {
        try
        {
            PlayerPrefs.SetInt("amountOfKeys", int.Parse(v));
        }
        catch
        {
            PlayerPrefs.SetInt("amountOfKeys", 88);
        }
    }

    public void OnStartKeyChange(int v)
    {
        PlayerPrefs.SetInt("startKey", v);
    }

    public void Calibrate()
    {
        SceneManager.LoadScene("Calibration");
    }


    public void OnWrongKeyPressChange(int v)
    {
        print(v);
        PlayerPrefs.SetInt("wrongKeyPress", v);
    }

    public void OnWaitToStartChange(float v)
    {
        waitToStartValue.text = v.ToString();
    }

    public void OnWaitToStartChange(string v)
    {
        try
        {
            waitToStart.value = int.Parse(v);
        }
        catch
        {
            waitToStart.value = 5;
        }
        PlayerPrefs.SetFloat("waitToStart", waitToStart.value);
    }

    public void AddFile()
    {
        print(Directory.GetCurrentDirectory());
        if (NativeGallery.CheckPermission(NativeGallery.PermissionType.Read) == NativeGallery.Permission.ShouldAsk)
        {
            NativeGallery.RequestPermission(NativeGallery.PermissionType.Read);
        }
        if (NativeGallery.CheckPermission(NativeGallery.PermissionType.Write) == NativeGallery.Permission.ShouldAsk)
        {
            NativeGallery.RequestPermission(NativeGallery.PermissionType.Read);
        }
        if (NativeGallery.CheckPermission(NativeGallery.PermissionType.Read) == NativeGallery.Permission.Denied || NativeGallery.CheckPermission(NativeGallery.PermissionType.Write) == NativeGallery.Permission.Denied)
        {
            throw new NotImplementedException();
        }
        NativeGallery.GetAudioFromGallery(CallBack, "Pick one or more Midi files");
    }

    void CallBack(string path)
    {
        if (path != null)
        {
            StartCoroutine(MiditoText(path));
            CopyFile(path);
        }
    }

    public IEnumerator MiditoText(string path)
    {
        FileInfo file = new FileInfo(path);
        string newPath = Path.Combine(Application.persistentDataPath, "Music", file.Name.Replace(".mid", ".txt"));
        if (!File.Exists(newPath))
            File.WriteAllText(newPath, new MPTK.NAudio.Midi.MidiFile(File.ReadAllBytes(path), false).ToString());
        else
        {
            File.Delete(Path.Combine(newPath));
            File.WriteAllText(newPath, new MPTK.NAudio.Midi.MidiFile(File.ReadAllBytes(path), false).ToString());
        }
        yield return new WaitForEndOfFrame();
    }

    private void CopyFile(string path)
    {
        string[] chosenFile = path.Split(Path.AltDirectorySeparatorChar);
        string newFile = Path.Combine(Application.persistentDataPath, "Music", chosenFile[chosenFile.Length - 1]);
        File.Copy(path, newFile);
        stuffthinggedoe.text = newFile;
    }
}

