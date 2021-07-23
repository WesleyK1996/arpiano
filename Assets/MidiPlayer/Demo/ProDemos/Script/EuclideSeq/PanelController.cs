using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace MPTKDemoEuclidean
{
    public class PanelController : MonoBehaviour
    {
        public enum Mode
        {
            Drum,
            Instrument
        }
        public TextSlider Step;
        public TextSlider Fill;
        public TextSlider Accent;
        public TextSlider Offset;
        public Transform Arrow;
        public int CountStep;
        public Mode PlayMode;
        public int CountFill;
        public int CountAccent;
        public int CountOffset;
        public int CurrentBeat;
        public int LastBeat;
        public double Tempo;
        public int CurrentInstrument;
        public bool IsReady = false;
        public bool ToBeRemoved;
        public bool ToBeRandomsed;
        public bool ToBeDuplicated;
        public PanelController DuplicateFrom;

        public Transform Parent;

        Dropdown DropdownInstrument;
        static List<string> Drums;
        static List<string> Instruments;
        public GameObject SphereHitTemplate;
        Texture2D texRythme;
        public Material HitCurrent;
        public Material HitDisable;
        public Material HitEnable;

        const int sizeBorder = 10;
        const int sizeTexture = 500;
        const int sizeCircle = 40;
        const float size3D = 8;

        RawImage Image;
        private GameObject[] HitsPool;
        private GameObject[] HitsStep;

        private float ratioStep;

        private BjorklundAlgo BjorklundAlgoFill;
        private BjorklundAlgo BjorklundAlgoAccent;

        // Angle of the sprite arrow
        private float angleCurrent;

        public bool SequenceHit
        {
            get
            {
                if (BjorklundAlgoFill != null && BjorklundAlgoFill.Sequence != null && CurrentBeat < BjorklundAlgoFill.Sequence.Count && CurrentBeat >= 0)
                    return BjorklundAlgoFill.Sequence[CurrentBeat];
                else
                    return false;
            }
        }

        public bool Accentuation
        {
            get
            {
                if (BjorklundAlgoAccent != null && BjorklundAlgoAccent.Sequence != null && CurrentBeat < BjorklundAlgoAccent.Sequence.Count && CurrentBeat >= 0)
                    return BjorklundAlgoAccent.Sequence[CurrentBeat];
                else
                    return false;
            }
        }

        private void Awake()
        {
            //Debug.Log("Awake " + name);
            IsReady = false;
            ToBeRandomsed = false;
            ToBeDuplicated = false;
            ToBeRemoved = false;
        }

        // Use this for initialization
        void Start()
        {
            //Debug.Log("Start " + name);
            if (Drums == null)
                Drums = new List<string>()
        {
            "35 Acoustic Bass Drum",
            "36 Bass Drum 1",
            "37 Side Stick/Rimshot",
            "38 Acoustic Snare",
            "39 Hand Clap",
            "40 Electric Snare",
            "41 Low Floor Tom",
            "42 Closed Hi-hat",
            "43 High Floor Tom",
            "44 Pedal Hi-hat",
            "45 Low Tom",
            "46 Open Hi-hat",
            "47 Low-Mid Tom",
            "48 Hi-Mid Tom",
            "49 Crash Cymbal 1",
            "50 High Tom",
            "51 Ride Cymbal 1",
            "52 Chinese Cymbal",
            "53 Ride Bell",
            "54 Tambourine",
            "55 Splash Cymbal",
            "56 Cowbell",
            "57 Crash Cymbal 2",
            "58 Vibra Slap",
            "59 Ride Cymbal 2",
            "60 High Bongo",
            "61 Low Bongo",
            "62 Mute High Conga",
            "63 Open High Conga",
            "64 Low Conga",
            "65 High Timbale",
            "66 Low Timbale",
            "67 High Agogô",
            "68 Low Agogô",
            "69 Cabasa",
            "70 Maracas",
            "71 Short Whistle",
            "72 Long Whistle",
            "73 Short Güiro",
            "74 Long Güiro",
            "75 Claves",
            "76 High Wood Block",
            "77 Low Wood Block",
            "78 Mute Cuíca",
            "79 Open Cuíca",
            "80 Mute Triangle",
            "81 Open Triangle"
        };
            if (Instruments == null)
            {
                Instruments = new List<string>();
                foreach (MidiPlayerTK.MPTKListItem preset in MidiPlayerTK.MidiPlayerGlobal.MPTK_ListPreset)
                    Instruments.Add(preset.Label);
            }

            BjorklundAlgoFill = new BjorklundAlgo();
            BjorklundAlgoFill.Generate(CountStep, CountFill, CountOffset);

            BjorklundAlgoAccent = new BjorklundAlgo();
            BjorklundAlgoAccent.Generate(CountStep, CountAccent, CountOffset);

            DropdownInstrument = GetComponentInChildren<Dropdown>();
            DropdownInstrument.ClearOptions();
            if (PlayMode == Mode.Drum)
                DropdownInstrument.AddOptions(Drums);
            else
                DropdownInstrument.AddOptions(Instruments);

            DropdownInstrument.onValueChanged.AddListener((int v) =>
            {
                if (v >= 0 && v < Drums.Count)
                {
                    CurrentInstrument = Convert.ToInt32(Drums[v].Substring(0, 2));
                }
            });
            DropdownInstrument.value = 6; // 41 

            Parent = SphereHitTemplate.transform.parent;

            Image = GetComponentInChildren<RawImage>(true);
            texRythme = new Texture2D(sizeTexture, sizeTexture);

            foreach (Transform t in Parent.transform)
                if (t.gameObject.name.StartsWith(SphereHitTemplate.name + " "))
                    Destroy(t.gameObject);

            SphereHitTemplate.SetActive(false);
            HitsPool = new GameObject[TestEuclideanRhythme.MaxStep];

            for (int i = 0; i < TestEuclideanRhythme.MaxStep; i++)
            {
                HitsPool[i] = Instantiate<GameObject>(SphereHitTemplate, Parent);
                // HitsPool[i].gameObject.name = string.Format("{0} {1,2:00}", SphereHitTemplate.name, i);
                HitsPool[i].gameObject.name = $"{SphereHitTemplate.name} {i,2:00}";
            }

            Step.OnEventValue.AddListener((int v) =>
            {
                //Debug.Log("Step");
                CountStep = v;
                HitsStep = new GameObject[CountStep];

                ratioStep = CountStep > 0 ? (float)TestEuclideanRhythme.MaxStep / (float)CountStep : 32f;

                angleCurrent = 360f * (float)CurrentBeat / (float)CountStep;

                Hit3dPosition(CountStep);

                Fill.SetRange(1, CountStep);
                Accent.SetRange(0, CountStep);
                Offset.SetRange(0, CountStep);

                BjorklundAlgoFill.Generate(CountStep, CountFill, CountOffset);
                BjorklundAlgoAccent.Generate(CountStep, CountAccent, CountOffset);
                Refresh();
            });

            Fill.OnEventValue.AddListener((int v) =>
            {
                //Debug.Log("Fill");
                CountFill = v;
                BjorklundAlgoFill.Generate(CountStep, CountFill, CountOffset);
                Refresh();
            });

            Accent.OnEventValue.AddListener((int v) =>
            {
                //Debug.Log("Accent");
                CountAccent = v;
                BjorklundAlgoAccent.Generate(CountStep, CountAccent, CountOffset);
                Refresh();
            });

            Offset.OnEventValue.AddListener((int v) =>
            {
                //Debug.Log("Offset");
                CountOffset = v;
                BjorklundAlgoFill.Generate(CountStep, CountFill, CountOffset);
                Refresh();
            });

            if (DuplicateFrom != null)
            {
                Duplicate(DuplicateFrom);
                DuplicateFrom = null;
            }

            Step.OnEventValue.Invoke(Step.Value);
            Fill.OnEventValue.Invoke(Fill.Value);
            Accent.OnEventValue.Invoke(Accent.Value);
            Offset.OnEventValue.Invoke(Offset.Value);

            IsReady = true;
        }

        private void Hit3dPosition(int countStep)
        {
            float rayon = ((RectTransform)SphereHitTemplate.transform.parent).sizeDelta.x / 2f - SphereHitTemplate.transform.localScale.x;
            for (int i = 0; i < TestEuclideanRhythme.MaxStep; i++)
            {
                if (i < countStep)
                {
                    HitsStep[i] = HitsPool[i];
                    // negative: turn clockwise ; 2PI: 360 deg ; +PI/2 to start at the top
                    float angle = -2f * Mathf.PI / countStep * i + Mathf.PI / 2f;
                    float x = rayon * Mathf.Cos(angle);
                    float y = rayon * Mathf.Sin(angle);
                    HitsStep[i].transform.localPosition = new Vector3(x, y, SphereHitTemplate.transform.localPosition.z);

                    //HitsPool[i].gameObject.SetActive(true);
                }
                else
                    HitsPool[i].gameObject.SetActive(false);
            }
        }

        public void Generate()
        {
            BjorklundAlgoFill.Generate(CountStep, CountFill);
            BjorklundAlgoAccent.Generate(CountStep, CountAccent);
        }

        public void SetDefault()
        {
            Step.Value = 32;// 8;
            Fill.Value = 32;// 2;
            Accent.Value = 1;
            Fill.SetRange(1, CountStep);
            Accent.SetRange(0, CountStep);
        }

        public void Duplicate(PanelController toCopy)
        {
            Step.Value = toCopy.Step.Value;
            Fill.Value = toCopy.Fill.Value;
            Accent.Value = toCopy.Accent.Value;
            Offset.Value = toCopy.Offset.Value;
            DropdownInstrument.value = toCopy.DropdownInstrument.value;
        }

        public void Random()
        {
            //if (IsReady)
            {
                Debug.Log("Random " + name);
                Step.Value = Convert.ToInt32(UnityEngine.Random.Range(1, 17));
                Fill.Value = Convert.ToInt32(UnityEngine.Random.Range(1, Step.Value + 1));
                Accent.Value = Convert.ToInt32(UnityEngine.Random.Range(1, Step.Value + 1));
                Offset.Value = Convert.ToInt32(UnityEngine.Random.Range(1, Step.Value + 1));
                DropdownInstrument.value = Convert.ToInt32(UnityEngine.Random.Range(1, DropdownInstrument.options.Count));
                ToBeRandomsed = false;
            }
            //else
            //    Debug.Log("not ready Random " + name);
        }

        public void OrganizeUI()
        {
            //Button[] bts = this.GetComponentsInChildren<Button>(true);
            //foreach (Button bt in bts)
            //    if (bt.name.ToLower().Contains("remove"))
            //        bt.gameObject.SetActive();
        }

        private bool lastDisplay2d;
        private bool lastDisplay3d;

        public void Refresh()
        {
            Refresh(lastDisplay2d, lastDisplay3d);
        }

        public void Refresh(bool display2d, bool display3d)
        {
            //Debug.Log("Panel Controller " + Parent.name + " " + ((RectTransform)Parent).CountCornersVisibleFrom(Camera.current));// Camera.main));
            if (IsReady && ((RectTransform)Parent).CountCornersVisibleFrom(Camera.current) > 0)
            {
                //Debug.Log("Panel Controller visible " + name + " " + Image.rectTransform.CountCornersVisibleFrom());// Camera.main));
                if (display2d)
                {
                    Image.gameObject.SetActive(true);
                    Refresh2D();
                }
                else
                {
                    Image.gameObject.SetActive(false);
                }
                lastDisplay2d = display2d;

                if (lastDisplay3d != display3d)
                    Hit3dPosition(display3d ? CountStep : 0);
                lastDisplay3d = display3d;

                if (display3d)
                    Refresh3D();
            }
        }

        public void Refresh3D()
        {
            if (HitsStep == null)
                return;

            for (int i = 0; i < CountStep; i++)
            {
                GameObject hit = HitsStep[i];

                if (hit != null)
                {
                    float sizeHit = size3D;
                    HitsPool[i].gameObject.SetActive(true);
                    Renderer materialHit = hit.gameObject.GetComponent<Renderer>();

                    if (BjorklundAlgoFill.Sequence[i] && CurrentBeat == i)
                        materialHit.material = HitCurrent;
                    else if (BjorklundAlgoFill.Sequence[i])
                        materialHit.material = HitEnable;
                    else
                    {
                        materialHit.material = HitDisable;
                        //sizeHit *= 0.5f;
                    }
                    sizeHit *= ratioStep;
                    sizeHit = Mathf.Clamp(sizeHit, 2f, 18f);

                    if (BjorklundAlgoAccent.Sequence[i])
                        sizeHit *= 1.3f;

                    hit.transform.localScale = new Vector3(sizeHit, sizeHit, sizeHit);
                }
            }
        }

        public void Refresh2D()
        {
            if (Image == null /*|| trfm == null*/ || texRythme == null) return;

            // Resize image to full height 
            //Image.rectTransform.sizeDelta = new Vector2(trfm.sizeDelta.y, trfm.sizeDelta.y);

            if (sizeTexture > 0)
            {
                texRythme.Clear();

                //tmpTex.DrawCircle(sizeTexture / 2, sizeTexture / 2, true, sizeCircle, border);
                for (int i = 0; i < CountStep; i++)
                {
                    // negative: turn clockwise ; 2PI: 360 deg ; +PI/2 to start at the top
                    float angle = -2f * Mathf.PI / CountStep * i + Mathf.PI / 2f;
                    // why 0.5f: sizeTexture/2=rayon du cercle inscrit ; why 0.7: to get a distance from the border
                    float rayon = sizeTexture * 0.5f * 0.7f;
                    int x = (int)(rayon * Mathf.Cos(angle) + (float)sizeTexture / 2f);
                    int y = (int)(rayon * Mathf.Sin(angle) + (float)sizeTexture / 2f);

                    // default value
                    int size = sizeCircle;
                    int border = sizeBorder;
                    bool fill = false;
                    texRythme.SetColorFill(Color.gray);
                    texRythme.SetColorBorder(Color.black);

                    if (CurrentBeat == i)
                    {
                        size += 10;
                        texRythme.SetColorFill(new Color(0.85f, 0.5f, 0.5f));
                    }

                    if (BjorklundAlgoAccent.Sequence[i])
                        border += 10;

                    if (BjorklundAlgoFill.Sequence[i])
                        fill = true;

                    if (CountStep > 16)
                    {
                        size /= 2;
                        border /= 2;
                    }

                    texRythme.DrawCircle(x, y, fill, size, border);
                }
                texRythme.Apply();

                Image.texture = texRythme;
            }
        }


        public void Update()
        {
            if (ToBeRandomsed)
            {
                Random();
            }
            if (CountStep > 0 && TestEuclideanRhythme.IsPlaying)
            {
                // Time in millisecnds to make a loop
                double timeToLoop = Tempo * CountStep;
                if (timeToLoop > 0d)
                {
                    // Therorical loop speed (count of loop per second)
                    double loopSecond = 1000d / timeToLoop;

                    // Therorical position of the arrow. This value could be used to update the sprite rotation
                    // but the display is jerky, not pleasant.
                    // It's better to play with rotation speed and to adapt the speed to the need.
                    float angleTarget = 360f * (float)CurrentBeat / (float)CountStep;

                    // Delta between real and theorical position
                    float angleDelta = angleCurrent - angleTarget;
                    angleDelta = Mathf.Clamp(angleDelta, -25f, 25f);

                    // Update speed to bring together the real and theorical position. Bound the update at 50% (delta is clamp between -25 and 25)
                    loopSecond *= (1f - angleDelta / 50f);

                    // Real position of the arrow calculated from speed of rotation
                    angleCurrent += (float)360f * (float)loopSecond * Time.deltaTime;
                    if (angleCurrent > 360f) angleCurrent = 0f;

                    // Rotate sprite with the current angle
                    Arrow.localEulerAngles = new Vector3(0f, 0f, -angleCurrent);
                }
            }
        }
    }

}

