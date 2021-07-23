using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EasyValue
{
    public float value;
    public bool hasDot;
}

public class SheetManager : MonoBehaviour
{
    public static SheetManager Instance;
    static Transform leftSheet, rightSheet;
    public Transform leftNotes, rightNotes;
    public Transform leftLayout, rightLayout;

    public int numerator, denominator;
    public int ticksPerQuarterNote;// ticks per measure should be this * 4 * denominator
    public List<NoteInfo> notesToPlay = new List<NoteInfo>();
    public List<GameObject> notesOnLeftSheet = new List<GameObject>();
    public List<GameObject> notesOnRightSheet = new List<GameObject>();
    public List<TimeSignature> signatures = new List<TimeSignature>();
    public List<Tempo> tempos = new List<Tempo>();
    public int lowestLength;
    public int loadedTick;
    public GameObject pair;
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
        loadedTick = 0;
        playSpeed = 0;
        StartCoroutine(WaitUntilLoaded());
    }

    private void Update()
    {
        if (playSpeed != 0)
        {
            for (int i = 0; i < notesOnLeftSheet.Count; i++)
            {
                notesOnLeftSheet[i].transform.Translate(transform.right * -1 / playSpeed);
                if (notesOnLeftSheet[i].transform.localPosition.z >= 0)
                {
                    GameObject go = notesOnLeftSheet[i];
                    notesOnLeftSheet.RemoveAt(i);
                    Destroy(go);
                }
            }
            for (int i = 0; i < notesOnRightSheet.Count; i++)
            {
                notesOnRightSheet[i].transform.Translate(transform.right * -1 / playSpeed);
                if (notesOnRightSheet[i].transform.localPosition.z >= 0)
                {
                    GameObject go = notesOnRightSheet[i];
                    notesOnRightSheet.RemoveAt(i);
                    Destroy(go);
                }
            }
        }
    }

    IEnumerator WaitUntilLoaded()
    {
        yield return new WaitUntil(() => loaded);
        yield return StartCoroutine(SpawnNotesWhenSpace());
    }

    private IEnumerator SpawnNotesWhenSpace()
    {
        while (notesToPlay.Count != 0)
        {
            float distancePerQuarterNote = (layoutWidth / AmountOfNotes);
            float DistancePerMeasure = signatures[0].numerator * (4 / signatures[0].denominator);
            //float distanceTillNewNoteHasToPlay = distancePerQuarterNote;
            //float CurNoteLength = GetKeyValueToEasyValue(notesToPlay[0].length).value;
            if (notesToPlay[0].isLeft)
            {
                if (notesOnLeftSheet.Count > 0)
                {
                    if (notesOnLeftSheet[notesOnLeftSheet.Count - 1].transform.localPosition.z >= -layoutWidth + distancePerQuarterNote)
                        yield return StartCoroutine(SpawnNote(notesOnLeftSheet));
                }
                else
                    yield return StartCoroutine(SpawnNote(notesOnLeftSheet));
            }
            else
            {
                if (notesOnRightSheet.Count > 0)
                {
                    if (notesOnRightSheet[notesOnRightSheet.Count - 1].transform.localPosition.z >= -layoutWidth + distancePerQuarterNote)
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
        pair = Instantiate(Resources.Load(Path.Combine("Prefabs", "StaffPair")), GameObject.FindGameObjectWithTag("Piano").transform) as GameObject;
        leftSheet = pair.transform.Find("Left");
        leftNotes = leftSheet.Find("Notes");
        leftLayout = leftSheet.Find("Layout");
        rightSheet = pair.transform.Find("Right");
        rightNotes = rightSheet.Find("Notes");
        rightLayout = rightSheet.Find("Layout");
        ShowKeys.Instance.leftPlayLine = leftLayout.Find("PlayLine");
        ShowKeys.Instance.rightPlayLine = rightLayout.Find("PlayLine");
    }

    IEnumerator SpawnNote(List<GameObject> list)
    {
        GameObject go = Instantiate(Resources.Load(Path.Combine("Prefabs", "Note"))) as GameObject;
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

        go.transform.parent = notesToPlay[0].isLeft ? leftNotes.transform : rightNotes.transform;
        go.name = notesToPlay[0].note;

        list.Add(go);

        yield return StartCoroutine(AddPos(list));

        loadedTick = notesToPlay[0].startTick;
        if (tempos.Count > 0 && (notesToPlay[1].startTick >= tempos[0].startick || playSpeed == 0))
        {
            playSpeed = tempos[0].speed;
            tempos.RemoveAt(0);
        }
        notesToPlay.RemoveAt(0);

        yield return new WaitForEndOfFrame();
    }

    EasyValue GetKeyValueToEasyValue(int length)
    {
        EasyValue e = new EasyValue();
        if (length == 0)
        {
            e.value = 0;
            return e;
        }

        int thirtySecondNote;
        thirtySecondNote = ticksPerQuarterNote / 8;

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
            z = list[list.Count - 2].transform.localPosition.z - distancePerQuarterNote * GetKeyValueToEasyValue(notesToPlay[0].length).value;
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
