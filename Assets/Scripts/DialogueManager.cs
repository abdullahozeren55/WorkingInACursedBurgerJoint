using Febucci.UI;
using Febucci.UI.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class FontAtlasData
    {
        public FontType type;
        public Texture2D atlasSprite;
    }
    public enum TalkType
    {
        TalkWithCustomer,
        TalkWithSeller,
        TalkWithYourself,
        TalkWithMascott,
        TalkWithYourselfInCutscene,
        TalkWithYourselfAfterInteraction
    }

    public enum FontType
    {
        Default,
        Sinan,
        Hikmet
    }
    public static DialogueManager Instance { get; private set; }
    [Space]
    [SerializeField] private FontAtlasData[] fontAtlasDatas;
    [Space]

    public bool IsInDialogue;

    [Space]
    [SerializeField] private FirstPersonController firstPersonController;
    [Space]
    [SerializeField] private TAnimPlayerBase textAnimPlayerBase;
    [Space]
    [SerializeField] private AudioClip defaultDialogueAudio;
    [Space]
    [SerializeField] private TMP_Text dialogueText;
    [Space]
    [SerializeField] private ShopSeller shopSeller;

    private DialogueData dialogueData;

    private int dialogueIndex = 0;
    private TalkType talkType;

    private AudioSource audioSource;

    private KeyCode skipKey;

    private float coroutineTimeBeforeSkip = 0.15f;
    private float currentCoroutineTime;

    private ICustomer currentCustomer;
    private IInteractable currentInteractable;

    private void Awake()
    {
        // Eðer Instance zaten varsa, bu nesneyi yok et
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        //Clone it so changes on material only effects itself
        dialogueText.fontMaterial = new Material(dialogueText.fontMaterial);

        Instance = this;

        audioSource = GetComponent<AudioSource>();

        skipKey = firstPersonController.interactKey;

        currentCustomer = null;
        currentInteractable = null;

        IsInDialogue = false;
    }

    private void Update()
    {
        if (IsInDialogue)
        {
            if (currentCoroutineTime > 0f)
            {
                currentCoroutineTime -= Time.deltaTime;
            }

            if (Input.GetKeyDown(skipKey))
            {
                dialogueIndex++;

                if (dialogueIndex >= dialogueData.dialogueSegments.Length)
                {
                    if (talkType == TalkType.TalkWithCustomer)
                        EndCustomerDialogue();
                    else if (talkType == TalkType.TalkWithYourselfAfterInteraction)
                        EndAfterInteractionSelfDialogue();
                    else if (talkType == TalkType.TalkWithSeller)
                        EndSellerDialogue();
                    else if (talkType == TalkType.TalkWithYourself)
                        EndSelfDialogue();
                    else if (talkType == TalkType.TalkWithYourselfInCutscene)
                        EndSelfDialogueInCutscene();
                }
                else
                {
                    HandleDialogue();
                }
            }
            
        }
    }

    private void HandleDialogue()
    {
        CameraManager.Instance.SwitchToCamera(dialogueData.dialogueSegments[dialogueIndex].cam);

        RectTransform rect = dialogueText.rectTransform;
        rect.anchoredPosition += dialogueData.dialogueSegments[dialogueIndex].DialogueOffset;

        DecideFontType(dialogueData.dialogueSegments[dialogueIndex].fontType);
        dialogueText.SetText(dialogueData.dialogueSegments[dialogueIndex].DialogueToPrint);
    }

    public void StartCustomerDialogue(ICustomer customer, DialogueData data)
    {
        GameManager.Instance.SetOrderThrowArea(false);
        dialogueData = data;

        IsInDialogue = true;

        talkType = TalkType.TalkWithCustomer;

        firstPersonController.CanMove = false;
        firstPersonController.CanUseHeadbob = false;
        dialogueIndex = 0;

        currentCustomer = customer;

        currentCoroutineTime = coroutineTimeBeforeSkip;

        HandleDialogue();
    }

    private void EndCustomerDialogue()
    {

        if (dialogueData.type == DialogueData.DialogueType.ENDSWITHACHOICE)
        {

            ChoiceManager.Instance.StartTheCustomerChoice(dialogueData.question, dialogueData.optionA, dialogueData.optionD,
                                    currentCustomer, currentCustomer.OptionADialogueData, currentCustomer.OptionDDialogueData, currentCustomer.NotAnsweringDialogueData, dialogueData.choiceCam);
        }
        else
        {
            currentCustomer.HandleFinishDialogue();
        }

        IsInDialogue = false;

        textAnimPlayerBase.StartDisappearingText();

        firstPersonController.CanUseHeadbob = true;
    }

    public void StartAfterInteractionSelfDialogue(IInteractable interactable, DialogueData data)
    {
        dialogueData = data;

        IsInDialogue = true;

        talkType = TalkType.TalkWithYourselfAfterInteraction;

        firstPersonController.CanMove = false;
        firstPersonController.CanUseHeadbob = false;
        dialogueIndex = 0;

        currentInteractable = interactable;

        currentCoroutineTime = coroutineTimeBeforeSkip;
    }

    private void EndAfterInteractionSelfDialogue()
    {

        currentInteractable.HandleFinishDialogue();

        IsInDialogue = false;

        firstPersonController.CanUseHeadbob = true;
    }

    public void StartSellerDialogue(DialogueData data)
    {
        dialogueData = data;

        IsInDialogue = true;

        talkType = TalkType.TalkWithSeller;

        firstPersonController.CanMove = false;
        firstPersonController.CanUseHeadbob = false;
        dialogueIndex = 0;

        currentCoroutineTime = coroutineTimeBeforeSkip;
    }

    private void EndSellerDialogue()
    {
        shopSeller.HandleFinishDialogue();
        IsInDialogue = false;

        firstPersonController.CanUseHeadbob = true;

    }

    public void StartSelfDialogue(DialogueData data)
    {
        dialogueData = data;

        IsInDialogue = true;

        talkType = TalkType.TalkWithYourself;

        firstPersonController.CanMove = false;
        firstPersonController.CanUseHeadbob = false;
        dialogueIndex = 0;

        currentCoroutineTime = coroutineTimeBeforeSkip;

    }

    private void EndSelfDialogue()
    {
        IsInDialogue = false;

        firstPersonController.CanUseHeadbob = true;

        firstPersonController.CanMove = true;
    }

    public void StartSelfDialogueInCutscene(DialogueData data)
    {
        dialogueData = data;

        IsInDialogue = true;

        talkType = TalkType.TalkWithYourselfInCutscene;

        firstPersonController.CanMove = false;
        firstPersonController.CanUseHeadbob = false;
        dialogueIndex = 0;

        currentCoroutineTime = coroutineTimeBeforeSkip;
    }

    private void EndSelfDialogueInCutscene()
    {
        IsInDialogue = false;

        firstPersonController.CanUseHeadbob = true;

        if (dialogueData.type == DialogueData.DialogueType.ENDSWITHACUTSCENE)
        {
            CutsceneManager.Instance.StopCutscene();
            CutsceneManager.Instance.PlayCutscene(dialogueData.cutsceneType);
        }
    }

    private void DecideFontType(FontType type)
    {
        for (int i = 0; i < fontAtlasDatas.Length; i++)
        {
            if (type == fontAtlasDatas[i].type)
            {
                dialogueText.fontMaterial.SetTexture("_MainTex", fontAtlasDatas[i].atlasSprite);
                break;
            }
        }
        
    }

    private void SetRandomPitch()
    {
        float pitch = Random.Range(1, 1.2f);

        audioSource.pitch = pitch;
    }
}
