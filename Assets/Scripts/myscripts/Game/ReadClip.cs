using PitchDetector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class Line
{
    public int startTick;
    public string rest;
}

[Serializable]
public class NoteInfo
{
    public string note;
    public int startTick;
    public int length;
    //public int velocity;
    public bool isLeft;
}

[Serializable]
public class TimeSignature
{
    public int startTick;
    public int numerator;
    public int denominator;
}

[Serializable]
public class Tempo
{
    public int startick;
    public float speed;
}

public class ReadClip : MonoBehaviour
{
    public static ReadClip Instance;
    string path;

    public List<NoteInfo> notes = new List<NoteInfo>();

    public int micSampleRate = 16000;
    private bool recording;
    public List<string> NotesHeard;

    private RAPTPitchDetector pitchDetector;
    private RAPTPitchDetector Detector
    {
        get
        {
            if (pitchDetector == null)
            {
                pitchDetector = new RAPTPitchDetector((float)micSampleRate, 50f, 800f);
            }
            return pitchDetector;
        }
    }

    public enum AllNotes { C, CS, D, DS, E, F, FS, G, GS, A, AS, B };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        StartCoroutine(NoteReader());
        //StartCoroutine(RecordingCoroutine());
    }

    private IEnumerator NoteReader()
    {
        recording = false;
        path = SelectSong.selectedSong;
        string line;
        yield return new WaitUntil(() => File.Exists(path));
        StreamReader file = File.OpenText(path);
        file.BaseStream.Position = 0;
        string[] data;
        int r;
        List<Line> lines = new List<Line>();
        data = file.ReadLine().Split(' ');
        SheetManager.Instance.ticksPerQuarterNote = int.Parse(data[9]);
        while ((line = file.ReadLine()) != null)
        {
            data = line.Split(' ');
            if (int.TryParse(data[0], out r))
            {
                Line l = new Line();
                l.startTick = r;
                for (int i = 1; i < data.Length; i++)
                {
                    l.rest += " " + data[i];
                }
                lines.Add(l);
            }
        }
        file.Close();
        lines = lines.OrderBy(l => l.startTick).ToList();

        yield return StartCoroutine(WriteListToFile(lines));
        file = File.OpenText(Path.Combine(Application.persistentDataPath, "Music", "song.txt"));
        while ((line = file.ReadLine()) != null)
        {
            data = line.Split(' ');
            if (data[1] == "SetTempo")
            {
                Tempo t = new Tempo()
                {
                    startick = int.Parse(data[0]),
                    speed = float.Parse(data[2].Replace("bpm", ""))
                };
                SheetManager.Instance.tempos.Add(t);
            }
            if (data[1] == "TimeSignature")
            {
                TimeSignature t = new TimeSignature()
                {
                    startTick = int.Parse(data[0]),
                    numerator = data[2][0] - 48,
                    denominator = data[2][2] - 48
                }; // -48 because char gets converted to int
                SheetManager.Instance.signatures.Add(t);
            }
            if (data[1] == "NoteOn")
            {
                if (data[3] == "1" || data[3] == "2")
                {
                    if (int.Parse(data[7]) == 0)
                        break;
                    NoteInfo n = new NoteInfo();

                    n.note = data[4];
                    n.startTick = int.Parse(data[0]);
                    n.length = int.Parse(data[7]);
                    if (n.length < SheetManager.Instance.lowestLength && n.length != 0)
                        SheetManager.Instance.lowestLength = n.length;
                    n.isLeft = data[3] == "1" ? false : true;

                    notes.Add(n);
                }
            }
        }

        SheetManager.Instance.notesToPlay = notes.OrderBy(l => l.startTick).ToList();
        SheetManager.Instance.loaded = true;
        //SheetManager.Instance.LoadStaffPair();

        //ShowKeys.Instance.ShowFirstKey();

        recording = true;
        yield return null;
    }

    IEnumerator WriteListToFile(List<Line> lines)
    {
        string r = "";
        foreach (Line line in lines)
        {
            r += line.startTick + line.rest + "\n";
        }
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "Music", "song.txt"), r);
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator RecordingCoroutine()
    {
        AudioClip rec = Microphone.Start(null, true, 1, micSampleRate);
        float[] readBuffer = new float[rec.samples];
        int recPos = 0;
        int prevRecPos = 0;
        Func<bool> enoughSamples = () =>
        {
            int count = (readBuffer.Length + Microphone.GetPosition(null) - prevRecPos) % readBuffer.Length;
            return count > Detector.RequiredSampleCount((float)micSampleRate);
        };
        while (true)
        {
            while (recording)
            {
                prevRecPos = recPos;
                yield return new WaitUntil(enoughSamples);
                recPos = Microphone.GetPosition(null);
                rec.GetData(readBuffer, 0);
                int sampleCount = (readBuffer.Length + recPos - prevRecPos) % readBuffer.Length;
                float db = 0f;

                List<float> pitchValues = Detector.getPitch(readBuffer, prevRecPos, ref recPos, ref db, (float)micSampleRate, true, !recording);
                if (pitchValues != null && pitchValues.Count > 0)
                {
                    sampleCount = (readBuffer.Length + recPos - prevRecPos) % readBuffer.Length;
                    if (sampleCount > 0)
                    {
                        yield return StartCoroutine(LogPitch(pitchValues));

                        if (db > -30 && NotesHeard.Count > 0)
                        {
                            NoteInfo note = new NoteInfo()
                            {
                                note = NotesHeard[0]
                            };
                            StartCoroutine(ShowKeys.Instance.CheckKey(note));
                        }
                        NotesHeard.RemoveRange(0, NotesHeard.Count);
                    }
                }
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
        //Microphone.End(null);
    }

    public IEnumerator LogPitch(List<float> pitchList)
    {
        List<int> midis = RAPTPitchDetectorExtensions.HerzToMidi(pitchList);
        if (midis.NoteString().Length == 0)
            yield return null;

        string s = (AllNotes)(midis[0] % 12) + "" + (Mathf.Floor(midis[0] / 12) - 2);
        if (s.Length < 4)
            NotesHeard.Add(s);
        yield return new WaitForEndOfFrame();
    }
}
