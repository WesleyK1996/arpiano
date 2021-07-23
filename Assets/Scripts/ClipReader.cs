using MidiPlayerTK;
using PitchDetector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace RhythmTool.Examples
{
    public class ClipReader : MonoBehaviour
    {
        //public MusicSheetManager SheetManager;
        public RhythmAnalyzer analyzer;
        public List<AudioClip> songs;
        public AudioClip clip;
        public List<string> Notes = new List<string>();


        //public PitchEvent onPitchDetected;
        public int micSampleRate = 16000;
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
        private bool recording;

        public MusicKeyShower show;

        public List<string> NotesHeard;

        bool busyLoading;

        void Start()
        {
            StartCoroutine(LoadSong());
            StartCoroutine(NoteReader());
            //StartCoroutine(NoteLoader());
            StartCoroutine(RecordingCoroutine());
            /*currentSong++;

            if (currentSong >= songs.Count)
                currentSong = 0;

            AudioClip audioClip = songs[currentSong];
            RhythmData rhythmData = analyzer.Analyze(audioClip);

            StartCoroutine(RecordingCoroutine());*/
        }

        //void Update()
        //{
        //    for (int i = 0; i < 9; i++)
        //    {
        //        if (Input.GetKeyDown(i.ToString()))
        //        {
        //            LoadSongNumber(i);
        //        }
        //    }
        //}

        public IEnumerator LoadSong()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Songs", "midi");
            DirectoryInfo d = new DirectoryInfo(path);
            foreach (FileInfo song in d.GetFiles("*.mid"))
            {
                if (song.Name == SelectSong.selectedSong)
                {
                    if (!File.Exists(Path.Combine(path, "Temp.txt")))
                        File.WriteAllText(Path.Combine(path, "Temp.txt"), new MPTK.NAudio.Midi.MidiFile(File.ReadAllBytes(Path.Combine(path, song.Name)), false).ToString());
                    else
                    {
                        File.Delete(Path.Combine(path, "Temp.txt"));
                        File.WriteAllText(Path.Combine(path, "Temp.txt"), new MPTK.NAudio.Midi.MidiFile(File.ReadAllBytes(Path.Combine(path, song.Name)), false).ToString());
                    }

                    break;
                }
            }
            yield return new WaitForEndOfFrame();
        }

        public void LoadSongNumber(int currentSong)
        {
            if (!busyLoading)
            {
                //    AudioClip audioClip = songs[currentSong];
                //    RhythmData rhythmData = analyzer.Analyze(audioClip);
                GetComponent<KeyStrokeMaker>().ResetKeys();
                StartCoroutine(NoteLoader());
            }
        }

        public void UnLoadSongNumber()
        {
            GetComponent<KeyStrokeMaker>().DeleteAllKeys();
            //SheetManager.RemoveAllNotes();
        }

        // Print pitch values to console
        public void LogPitch(List<float> pitchList, int samples, float db)
        {
            List<int> midis = RAPTPitchDetectorExtensions.HerzToMidi(pitchList);
            //Debug.Log("detected " + pitchList.Count + " values from " + samples + " samples, db:" + db);
            if (midis.NoteString().Length == 0)
                return;
            print(midis[0]);
            if (midis.NoteString().Substring(1, 1) == "#")
            {
                NotesHeard.Add(midis.NoteString().Substring(0, 1) + "S");//Debug.Log(midis.NoteString().Substring(0, 2));
            }
            else if (midis.NoteString().Substring(0, 1) != "?")
            {
                NotesHeard.Add(midis.NoteString().Substring(0, 1));
                //Debug.Log(midis.NoteString().Substring(0, 1));
            }
        }

        private IEnumerator NoteReader()
        {
            Notes.Clear();
            recording = false;

            new WaitUntil(() => clip != null);
            analyzer.Analyze(clip);


            recording = true;
            yield return null;
        }

        // Checks every tick if a pitch is recocnized in the mic
        private IEnumerator NoteLoader()
        {
            busyLoading = true;
            Notes.Clear();
            recording = false;
            // While the song is still loading in, the game should not start.
            while (!analyzer.isDone || !GetComponent<KeyStrokeMaker>().KeysSpawned)
            {
                print("Still waiting");
                yield return new WaitForSecondsRealtime(0.5f);
            }

            print("Music loaded");

            foreach (Note n in Chromagram.Notes)
            {
                string NoteText = n.ToString();
                if (NoteText.Length > 1)
                {
                    NoteText = NoteText.Substring(0, 2);
                    //print(NoteText);
                }
                //print(NoteText);
                Notes.Add(NoteText);
            }
            //SheetManager.LoadFirstNotes(Notes.GetRange(0, SheetManager.AmountOfKeys));

            show.ShowFirstKey();
            recording = true;
            busyLoading = false;
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
                while (recording || KeyStrokeMaker.IsCalibrating)
                {
                    prevRecPos = recPos;
                    yield return new WaitUntil(enoughSamples);
                    recPos = Microphone.GetPosition(null);
                    rec.GetData(readBuffer, 0);
                    int sampleCount = (readBuffer.Length + recPos - prevRecPos) % readBuffer.Length;
                    float db = 0f;
                    List<float> pitchValues = Detector.getPitch(readBuffer, prevRecPos, ref recPos, ref db, (float)micSampleRate, true, !recording);
                    sampleCount = (readBuffer.Length + recPos - prevRecPos) % readBuffer.Length;
                    if (sampleCount > 0)
                    {
                        LogPitch(pitchValues, sampleCount, db);

                        if (NotesHeard.Count > 1)
                        {
                            if (NotesHeard[NotesHeard.Count - 1] == NotesHeard[NotesHeard.Count - 2])
                            {
                                if (NotesHeard.Count > 3)
                                {
                                    print(NotesHeard[0]);
                                    if (KeyStrokeMaker.IsCalibrating)
                                    {
                                        if (NotesHeard[0].ToString() == "C")
                                        {
                                            //GetComponent<KeyStrokeMaker>().FinishCalibrating();
                                        }
                                    }
                                    else
                                    {
                                        StartCoroutine(show.CheckKey(NotesHeard[0]));
                                        // In here we have to send the command to where the song is stored
                                        NotesHeard.RemoveRange(0, NotesHeard.Count);
                                    }
                                }
                            }
                            else
                            {
                                NotesHeard.RemoveRange(0, NotesHeard.Count);
                            }
                        }
                    }
                }
                yield return new WaitForSecondsRealtime(0.1f);
            }
            //Microphone.End(null);
        }
    }
}
