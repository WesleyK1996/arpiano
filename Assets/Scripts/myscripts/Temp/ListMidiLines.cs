using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ListMidiLines : MonoBehaviour
{
    public string filename;

    void Start()
    {
        StartCoroutine(DoThing());
    }

    private IEnumerator DoThing()
    {
        string copyPath = Path.Combine(Application.streamingAssetsPath, "midi", filename + ".mid");
        string pastePath = Path.Combine(Application.persistentDataPath, "Music", filename + ".txt");
        File.WriteAllText(pastePath, new MPTK.NAudio.Midi.MidiFile(File.ReadAllBytes(copyPath), false).ToString());
        yield return null;
    }
}
