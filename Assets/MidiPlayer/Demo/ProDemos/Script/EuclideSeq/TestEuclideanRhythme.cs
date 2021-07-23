using MidiPlayerTK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MPTKDemoEuclidean
{
    public class TestEuclideanRhythme : MonoBehaviour
    {
        public const int MaxStep = 32;

        public static bool IsPlaying;

        public TextSlider SldTempo;
        public TextSlider SldVolume;
        public Text Info;

        /// <summary>
        /// Humanizing: a little random on each hit and volume. Between 0 and 100.
        /// </summary>
        public TextSlider SldHumanize;

        public RectTransform ContentScroller;
        public RectTransform PanelAbout;
        public Button BtPlay;
        public bool Display2D = false;
        public bool Display3D = true;
        public double tempo;
        public int beat;
        public PanelController templateController;
        private List<PanelController> Controlers;
        public MidiStreamPlayer midiStream;

        /// <summary>
        /// ms
        /// </summary>
        public double lastMidiTimePlayCore;

        /// <summary>
        /// ms
        /// </summary>
        public double timeMidiFromStartPlay;

        /// <summary>
        /// ms
        /// </summary>
        public double timeSinceLastBeat;

        private Thread SequencerThread;
        private bool playThread;
        private int humanize;
        private float volume;



        void Start()
        {
            SldTempo.Value = 30;
            SldVolume.Value = 100;
            Info.text = Application.version;

            // List of controller to be played. 
            Controlers = new List<PanelController>();

            // The first controller is used as a template for the others, it is disabled and will be never played.
            templateController.gameObject.SetActive(false);

            BtPlay.onClick.AddListener(() =>
            {
                IsPlaying = !IsPlaying;
                SetLabelBtPlay();
                Play();
            });

            Play();
        }

        void SetLabelBtPlay()
        {
            BtPlay.GetComponentInChildren<Text>().text = IsPlaying ? "Stop" : "Play";
        }

        public PanelController CreateContoller(PanelController.Mode mode)
        {
            PanelController controler = Instantiate<PanelController>(templateController);
            controler.IsReady = false;
            controler.PlayMode = mode;
            controler.name = string.Format("{0} {1,2:00}", templateController.name, Controlers.Count);
            controler.transform.position = templateController.transform.position;
            controler.transform.SetParent(templateController.transform.parent);
            // changing parent can affect scale, reset to 1
            controler.transform.localScale = new Vector3(1, 1, 1);
            controler.OrganizeUI();
            controler.gameObject.SetActive(true);
            return controler;
        }

        public void AddContoller(string smode)
        {
            PanelController.Mode mode = smode == "drum" ? PanelController.Mode.Drum : PanelController.Mode.Instrument;

            PanelController controler = CreateContoller(mode);
            controler.ToBeRandomsed = true;
            Controlers.Add(controler);
            // Resize the content of the scroller to reflect the position of the scroll bar (100=height of PanelController + space)
            ContentScroller.sizeDelta = new Vector2(ContentScroller.sizeDelta.x, Controlers.Count * 115);
            //Debug.Log($"AddController {mode} {Controlers.Count} { ContentScroller.sizeDelta}");
        }

        public void DuplicateContoller(PanelController toDuplicate, int position)
        {
            toDuplicate.ToBeDuplicated = false;
            PanelController controler = CreateContoller(toDuplicate.PlayMode);
            controler.transform.SetSiblingIndex(toDuplicate.transform.GetSiblingIndex() + 1);
            controler.DuplicateFrom = toDuplicate;
            Controlers.Insert(position, controler);
            // Resize the content of the scroller to reflect the position of the scroll bar (100=height of PanelController + space)
            ContentScroller.sizeDelta = new Vector2(ContentScroller.sizeDelta.x, Controlers.Count * 115);
            //Debug.Log($"AddController {mode} {Controlers.Count} { ContentScroller.sizeDelta}");
        }

        /// <summary>
        /// This action removes a controller. 
        /// Called from On Click()  defined in the inspector of the controller.
        /// The controller himself is passed with the parameter.
        /// </summary>
        /// <param name="ctrl"></param>
        public void RemoveController(PanelController ctrl)
        {
            // Remove controller (see update)
            // Need to synch with the Update to avoid conflict when playing.
            ctrl.ToBeRemoved = true;
        }

        /// <summary>
        /// This action randomly chooses new value for the controller. 
        /// Called from On Click()  defined in the inspector of the controller.
        /// The controller himself is passed with the parameter.
        /// </summary>
        /// <param name="ctrl"></param>
        public void RandomController(PanelController ctrl)
        {
            ctrl.ToBeRandomsed = true;
        }

        public void DuplicateController(PanelController ctrl)
        {
            ctrl.ToBeDuplicated = true;
        }

        public void Play()
        {
            lastMidiTimePlayCore = System.DateTime.Now.Ticks / 10000D;
            timeMidiFromStartPlay = 0d;
            timeSinceLastBeat = 999999d; // start with a first beat
            beat = -1; // start with a first beat
            playThread = true;
            if (SequencerThread == null)
                SequencerThread = new Thread(PlaySequencerThread);
            if (!SequencerThread.IsAlive)
                SequencerThread.Start();
        }

        void OnDisable()
        {
            playThread = false;
        }


        public void Quit()
        {
            for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                //Debug.Log(SceneUtility.GetScenePathByBuildIndex(i));
                if (SceneUtility.GetScenePathByBuildIndex(i).Contains("ScenesDemonstration"))
                {
                    SceneManager.LoadScene(i, LoadSceneMode.Single);
                    return;
                }
            }

            Application.Quit();
        }

        public void GotoWeb(string uri)
        {
            Application.OpenURL(uri);
        }

        public void DisplayAbout(bool display)
        {
            PanelAbout.gameObject.SetActive(display);
        }

        void PlaySequencerThread()
        {
            System.Random rnd = new System.Random();

            while (playThread)
            {
                if (Controlers.Count > 100) break;
                double now = System.DateTime.Now.Ticks / 10000D;
                double deltaTime = now - lastMidiTimePlayCore;
                lastMidiTimePlayCore = now;
                timeMidiFromStartPlay += deltaTime;
                timeSinceLastBeat += deltaTime;
                tempo = 5000d / SldTempo.Value;
                if (timeSinceLastBeat > tempo)
                {
                    timeSinceLastBeat = 0d;
                    //if (++beat >= MaxStep) beat = 0;
                    beat++;

                    lock (this)
                    {
                        for (int c = 0; c < Controlers.Count; c++)
                        {
                            PanelController controler = Controlers[c];

                            if (controler.CountStep > 0)
                            {
                                if (IsPlaying)
                                {
                                    controler.CurrentBeat = beat % controler.CountStep;
                                    controler.Tempo = tempo;
                                    if (controler.SequenceHit)
                                    {
                                        int delayAlea = rnd.Next(0, Convert.ToInt32(200f * (humanize / 100f)));
                                        float velAlea = rnd.Next(0, humanize);
                                        //Debug.Log($"{delayAlea} {velAlea}");
                                        midiStream.MPTK_PlayEvent(new MPTKEvent()
                                        {
                                            Channel = controler.PlayMode == PanelController.Mode.Drum ? 9 : 0,     // default channel for drum
                                            Duration = 2000, // drum hit are always short, 10 sec here.
                                            Delay = delayAlea,
                                            Value = controler.CurrentInstrument, // each note sound a different drum
                                            Velocity = (int)
                                            (
                                                (controler.Accentuation ? 127f : 80f) *
                                                (volume / 100f) * (1f - velAlea / 100f)

                                            )
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(1);
            }
        }

        private void Update()
        {
            // refresh screen and  remove controler. 
            // Search for each controller in case of multiple controller must be deleted (quite impossible!)
            // Use a for loop in place a foreach.
            // Because removing an element in the list change the list and foreach loop don't like this ...
            lock (this)
            {
                humanize = SldHumanize.Value;
                volume = SldVolume.Value;
                for (int c = 0; c < Controlers.Count;)
                {
                    PanelController controler = Controlers[c];

                    if (controler.ToBeRemoved)
                    {
                        DestroyImmediate(controler.gameObject);
                        Controlers.RemoveAt(c);
                    }
                    else
                    {
                        if (controler.ToBeDuplicated)
                            DuplicateContoller(controler, c);

                        controler.Refresh(Display2D, Display3D);
                        c++;
                    }
                }
            }
        }
    }
}