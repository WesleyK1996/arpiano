using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class EasyValue
{
    public float value;
    public bool hasDot;
}

public class SheetManager : MonoBehaviour
{
    public static SheetManager Instance;
    public Transform pair;
    public Transform leftSheet, rightSheet;
    public Transform leftNotes, rightNotes;
    public Transform leftLayout, rightLayout;
    public Transform leftPlayLine, rightPlayLine;

    public int numerator, denominator;
    public int ticksPerQuarterNote;
    public List<NoteInfo> notesToPlay = new List<NoteInfo>();
    public List<GameObject> notesOnLeftSheet = new List<GameObject>();
    public List<GameObject> notesOnRightSheet = new List<GameObject>();
    public List<TimeSignature> signatures = new List<TimeSignature>();
    public int lowestLength;
    int lastLeftLength;
    int lastRightLength;
    public float playSpeed;
    public bool loaded = false;
    public float layoutWidth = 10;
    float AmountOfNotes = 16f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        lowestLength = 999;
        playSpeed = 0;
        StartCoroutine(WaitUntilLoaded());
    }

    private void Update()
    {
        if (loaded)
            if (notesOnRightSheet.Count > 0 && Vector3.Distance(notesOnRightSheet[0].transform.position, rightPlayLine.position) < .2f)
            {
                if (notesOnRightSheet[0].name[0] == 'T')
                    playSpeed = SetTempo(int.Parse(notesOnRightSheet[0].name.Replace("T", "")));
                else
                    ShowKeys.Instance.ShowKey(notesOnRightSheet[0].name);
            }
        if (notesOnLeftSheet.Count > 0 && Vector3.Distance(notesOnLeftSheet[0].transform.position, leftPlayLine.position) < .2f)
            ShowKeys.Instance.ShowKey(notesOnLeftSheet[0].name);
    }

    public float SetTempo(float speed)
    {
        return 10f / (1000f / (speed / 60f)) / 32f;// /32 = divide by scale .25(piano parts.z) == *4, .5(note.z) == *2
    }

    IEnumerator WaitUntilLoaded()
    {
        yield return new WaitUntil(() => loaded);
        yield return StartCoroutine(SpawnNotesWhenSpace());
    }

    private IEnumerator SpawnNotesWhenSpace()//how to spawn double notes??(see bohemian sheet)
    {
        while (notesToPlay.Count != 0)
        {
            float distancePerQuarterNote = (layoutWidth / AmountOfNotes);
            float DistancePerMeasure = signatures[0].numerator * (4 / signatures[0].denominator);
            //float distanceTillNewNoteHasToPlay = distancePerQuarterNote;
            //float CurNoteLength = GetKeyValueToEasyValue(notesToPlay[0].length).value;
            if (notesToPlay[0].isLeft)
            {
                lastLeftLength = notesToPlay[0].length;
                if (notesOnLeftSheet.Count > 0)
                {
                    if (notesOnLeftSheet[notesOnLeftSheet.Count - 1].transform.localPosition.z >= -layoutWidth + distancePerQuarterNote * GetKeyValueToEasyValue(lastLeftLength).value)
                        yield return StartCoroutine(SpawnNote(notesOnLeftSheet));
                }
                else
                    yield return StartCoroutine(SpawnNote(notesOnLeftSheet));
            }
            else
            {
                lastRightLength = notesToPlay[0].length;
                if (notesOnRightSheet.Count > 0)
                {
                    if (notesOnRightSheet[notesOnRightSheet.Count - 1].transform.localPosition.z >= -layoutWidth + distancePerQuarterNote * GetKeyValueToEasyValue(lastRightLength).value)
                        yield return StartCoroutine(SpawnNote(notesOnRightSheet));
                }
                else
                    yield return StartCoroutine(SpawnNote(notesOnRightSheet));
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void LoadStaffPair()
    {
        pair = GameObject.FindGameObjectWithTag("Piano").transform.Find("StaffPair");
        //pair = Instantiate(Resources.Load(Path.Combine("Prefabs", "StaffPair")), GameObject.FindGameObjectWithTag("Piano").transform) as GameObject;
        leftSheet = pair.transform.Find("Left");
        leftNotes = leftSheet.Find("Notes");
        leftLayout = leftSheet.Find("Layout");
        rightSheet = pair.transform.Find("Right");
        rightNotes = rightSheet.Find("Notes");
        rightLayout = rightSheet.Find("Layout");
        leftPlayLine = leftLayout.Find("PlayLine");
        rightPlayLine = rightLayout.Find("PlayLine");
    }

    IEnumerator SpawnNote(List<GameObject> list)
    {
        GameObject go = Instantiate(Resources.Load(Path.Combine("Prefabs", "Note")), notesToPlay[0].isLeft ? leftNotes : rightNotes) as GameObject;
        go.transform.localPosition = new Vector3(-.02f, SetY(), -10);
        if (notesToPlay[0].note == "T")
        {
            go.transform.Find("Whole").gameObject.SetActive(false);
            go.name = "T" + notesToPlay[0].bpm;
        }
        else
        {
            switch (GetKeyValueToEasyValue(notesToPlay[0].length).value)
            {
                case 3:
                case 2:
                    go.transform.Find("Half").gameObject.SetActive(true);
                    break;
                case 1.5f:
                case 1:
                    go.transform.Find("Half").gameObject.SetActive(true);
                    go.transform.Find("Quarter").gameObject.SetActive(true);
                    break;
                case .75f:
                case .5f:
                    go.transform.Find("Half").gameObject.SetActive(true);
                    go.transform.Find("Quarter").gameObject.SetActive(true);
                    go.transform.Find("Eighth").gameObject.SetActive(true);
                    break;
                case .375f:
                case .25f:
                    go.transform.Find("Half").gameObject.SetActive(true);
                    go.transform.Find("Quarter").gameObject.SetActive(true);
                    go.transform.Find("Eighth").gameObject.SetActive(true);
                    go.transform.Find("16th").gameObject.SetActive(true);
                    break;
                case .1875f:
                case .125f:
                    go.transform.Find("Half").gameObject.SetActive(true);
                    go.transform.Find("Quarter").gameObject.SetActive(true);
                    go.transform.Find("Eighth").gameObject.SetActive(true);
                    go.transform.Find("16th").gameObject.SetActive(true);
                    go.transform.Find("32nd").gameObject.SetActive(true);
                    break;
                default: break;
            }
            if (notesToPlay[0].note[1] == '#')
                go.transform.Find("Sharp").gameObject.SetActive(true);

            go.name = notesToPlay[0].note;
        }

        list.Add(go);

        //yield return StartCoroutine(AddPos(list));
        StartCoroutine(MoveNote(go, notesToPlay[0].isLeft));

        notesToPlay.RemoveAt(0);

        yield return new WaitForEndOfFrame();
    }

    private IEnumerator MoveNote(GameObject go, bool isLeft)
    {
        float t = Time.timeSinceLevelLoad;
        float elapsed = 0;
        Vector3 start = go.transform.localPosition;
        Vector3 end = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, 0);
        while (elapsed < 1f)
        {
            go.transform.localPosition = Vector3.Lerp(start, end, elapsed);
            elapsed = (Time.timeSinceLevelLoad - t) / ((playSpeed / AmountOfNotes) + 7.5f); // hoeveelheid seconden berekenen
            yield return new WaitForEndOfFrame();
        }

        if (isLeft)
            notesOnLeftSheet.Remove(go);
        else
            notesOnRightSheet.Remove(go);
        Destroy(go);
    }

    EasyValue GetKeyValueToEasyValue(int length)
    {
        EasyValue e = new EasyValue();
        if (length == 0)
        {
            print(0);
            e.value = 0;
            return e;
        }

        int thirtySecondNote = ticksPerQuarterNote / 8;

        for (float i = 1f / 32f; i < length; thirtySecondNote *= 2, i *= 2)
        {
            if (length <= thirtySecondNote)
            {
                e.value = i * 4f;
                e.hasDot = false;
                return e;
            }
            else if (length <= thirtySecondNote * 1.5f)
            {
                e.value = i * 4f * 1.5f;
                e.hasDot = true;
                return e;
            }
        }
        e.value = -1;
        return e;
    }

    private IEnumerator AddPos(List<GameObject> list)
    {
        Vector3 locPos;
        locPos.x = -.02f;
        locPos.y = SetY();
        locPos.z = SetZ(list);

        print(locPos);
        list[list.Count - 1].transform.localPosition = locPos;
        yield return new WaitForEndOfFrame();
    }

    private float SetY()
    {
        float y = 0;
        switch (notesToPlay[0].note[0])
        {
            case 'A':
                y = -.25f;
                break;
            case 'C':
                y = .25f;
                break;
            case 'D':
                y = .5f;
                break;
            case 'E':
                y = .75f;
                break;
            case 'F':
                y = 1f;
                break;
            case 'G':
                y = -.5f;
                break;
        }
        if (notesToPlay[0].isLeft)
            y -= .5f;
        return y;
    }

    private float SetZ(List<GameObject> list)
    {
        float distancePerQuarterNote = (layoutWidth / AmountOfNotes);
        float z;
        if (list.Count >= 2)
        {
            if (list[list.Count - 2].name[0] == 'T')
            {
                z = list[list.Count - 2].transform.localPosition.z - distancePerQuarterNote * GetKeyValueToEasyValue(notesToPlay[1].length).value;
            }
            else
                z = list[list.Count - 2].transform.localPosition.z - distancePerQuarterNote * GetKeyValueToEasyValue(notesToPlay[0].length).value;
        }
        else
            z = -layoutWidth;
        return z;
    }

    public static int NoteNameToNumber(string Key)
    {
        MakeKeys.PianoKeys CurrentKey = MakeKeys.StringToKeystroke(Key);
        int NoteKey = 0;
        switch ((int)CurrentKey)
        {
            case 0:
                NoteKey = 0;
                break;
            case 1:
            case 2:
            case 3:
                NoteKey = (int)CurrentKey - 1;
                break;
            case 4:
            case 5:
                NoteKey = (int)CurrentKey - 2;
                break;
            case 6:
            case 7:
            case 8:
                NoteKey = (int)CurrentKey - 3;
                break;
            case 9:
            case 10:
                NoteKey = (int)CurrentKey - 4;
                break;
            case 11:
                NoteKey = (int)CurrentKey - 5;
                break;
        }
        return NoteKey;
    }

    public bool IsSharp(string Key)
    {
        if (Key.Length == 2)
            return true;
        else
            return false;
    }
}
