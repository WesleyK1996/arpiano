#define SHOWDEFAULT
using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MidiPlayerTK
{
    /// <summary>
    /// Inspector for the midi global player component
    /// </summary>
    [CustomEditor(typeof(MidiInReader))]
    public class MidiInReaderEditor : Editor
    {
        private SerializedProperty CustomEventOnEventInputMidi;

        private static MidiInReader instance;
        private MidiCommonEditor commonEditor;

#if SHOWDEFAULT
        private static bool showDefault;
#endif

        // Manage skin
        public CustomStyle myStyle;


        void OnEnable()
        {
            try
            {
                instance = (MidiInReader)target;
                CustomEventOnEventInputMidi = serializedObject.FindProperty("OnEventInputMidi");
                if (!Application.isPlaying)
                {
                    // Load description of available soundfont
                    if (MidiPlayerGlobal.CurrentMidiSet == null || MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo == null)
                    {
                        MidiPlayerGlobal.InitPath();
                        ToolsEditor.LoadMidiSet();
                        ToolsEditor.CheckMidiSet();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        public override void OnInspectorGUI()
        {
            try
            {
                // Set custom Style. 
                if (myStyle == null) myStyle = new CustomStyle();

                GUI.changed = false;
                GUI.color = Color.white;
                if (commonEditor == null) commonEditor = ScriptableObject.CreateInstance<MidiCommonEditor>();

                //mDebug.Log(Event.current.type);

                commonEditor.DrawCaption("Midi In Reader - Read Midi events from your Midi keyboard ", "https://paxstellar.fr/prefab-midiinreader/");

                // Endpoints
                var endpointCount = instance.MPTK_CountEndpoints;
                var temp = "Detected MIDI devices:";
                for (var i = 0; i < endpointCount; i++)
                {
                    temp += "\n" + instance.MPTK_GetEndpointDescription(i);
                }
                EditorGUILayout.LabelField(temp, myStyle.BlueText, GUILayout.Height(40));
                //Debug.Log(temp);
                instance.MPTK_ReadMidiInput = EditorGUILayout.Toggle(new GUIContent("Read Midi Events", ""), instance.MPTK_ReadMidiInput);
                instance.MPTK_LogEvents = EditorGUILayout.Toggle(new GUIContent("Log Midi Events", ""), instance.MPTK_LogEvents);
                instance.MPTK_DirectSendToPlayer = EditorGUILayout.Toggle(new GUIContent("Send To MPTK Synth", "Midi events are send to the midi player directly"), instance.MPTK_DirectSendToPlayer);
                EditorGUILayout.PropertyField(CustomEventOnEventInputMidi);
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.Separator();
                commonEditor.AllPrefab(instance);
                commonEditor.SynthParameters(instance, serializedObject);
#if SHOWDEFAULT
                showDefault = EditorGUILayout.Foldout(showDefault, "Show default editor");
                if (showDefault)
                {
                    EditorGUI.indentLevel++;
                    commonEditor.DrawAlertOnDefault();
                    DrawDefaultInspector();
                    EditorGUI.indentLevel--;
                }
#endif
                MidiCommonEditor.SetSceneChangedIfNeed(instance, GUI.changed);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

    }

}
