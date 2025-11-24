using Cinemachine;
using DG.Tweening;
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
        [Space]
        public float typeSpeed = 1f;

        public Vector2 DialogueOffset;
        public bool Skippable;
        public float autoSkipTime = 10f; //for self dialogue auto skip
        public float delay = 0f; //waiting before start showing text
        public float minAudioPitch = 0.85f;
        public float maxAudioPitch = 1.15f;

        [Space]
        public AudioClip audioToPlay;
        public float audioToPlayVolume = 1f;
        public float audioToPlayMinPitch = 0.85f;
        public float audioToPlayMaxPitch = 1.15f;
        public float audioToPlayDelay = 0f;
        [Space]
        public CameraManager.CameraName cam;
        [Space]
        public ICustomer.DialogueAnim dialogueAnim;
        public float dialogueAnimDelay = 0f;
        [Space]
        public DialogueCamType dialogueCamType;
        [Space]
        public float TargetFOV = 60;
        public float FOVDuration = 1f;
        public Ease FOVEase = Ease.InOutBack;
        public float FOVEaseValue = 1.7f;
    }

    public enum DialogueType
    {
        NORMAL,
        ENDSWITHACHOICE,
        ENDSWITHACUTSCENE
    }

    public enum DialogueCamType
    {
        NOTHING,
        CHANGEFOV,
        RESETFOV
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
