using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueData", menuName = "Data/Dialogue")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueSegment
    {
        public string DialogueKey;
        [Space]
        public DialogueManager.FontType fontType;
        public DialogueManager.TalkingPerson talkingPerson;
        [Space]
        public FontStyle fontStyle;

        public Vector2 DialogueOffset;
        public bool Skippable;
        public float autoSkipTime = 10f; //for self dialogue auto skip
        public float delay = 0f; //waiting before start showing text
        public float minAudioPitch = 0.85f;
        public float maxAudioPitch = 1.15f;

        [Space]
        public AudioClip audioClip;
        [Space]
        public CameraManager.CameraName cam;
        [Space]
        public ICustomer.DialogueAnim dialogueAnim;
    }

    public enum DialogueType
    {
        NORMAL,
        ENDSWITHACHOICE,
        ENDSWITHACUTSCENE
    }

    public DialogueSegment[] dialogueSegments;
    [Space]
    public DialogueType type;
    [Space]
    public string QuestionKey;
    public string OptionAKey;
    public string OptionDKey;
    public CameraManager.CameraName choiceCam;
    [Space]
    public CutsceneType cutsceneType;
}
