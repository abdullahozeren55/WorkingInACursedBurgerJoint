using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopSeller : MonoBehaviour, IInteractable
{
    private int interactableLayer;
    private int interactableOutlinedLayer;
    private int uninteractableLayer;
    private int grabableLayer;

    [SerializeField] private DialogueData firstDialogueData;
    [SerializeField] private DialogueData secondDialogueData;
    [SerializeField] private DialogueData noodleBuyDialogueData;
    [SerializeField] private DialogueData noodleBuyDialoguePartTwoData;
    [SerializeField] private float waitingTime = 1f;
    [Space]
    [SerializeField] private GameObject talkWithSellerText;
    [SerializeField] private GameObject storeBlocker;
    [Space]
    [SerializeField] private AudioClip sellerJumpscareSound;


    [HideInInspector] public bool isNoodlePlaced;
    [HideInInspector] public Noodle noodle;

    private bool isNoodleBought;
    private bool isJumpscared;
    private bool firstTalkIsFinished;

    private Animator anim;
    private AudioSource audioSource;

    public GameManager.HandRigTypes HandRigType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private void Awake()
    {
        interactableLayer = LayerMask.NameToLayer("Interactable");
        interactableOutlinedLayer = LayerMask.NameToLayer("InteractableOutlined");
        uninteractableLayer = LayerMask.NameToLayer("Uninteractable");

        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        isNoodlePlaced = false;
        isNoodleBought = false;
        isJumpscared = false;
        firstTalkIsFinished = false;
    }
    public void OnFocus()
    {
        gameObject.layer = interactableOutlinedLayer;
        talkWithSellerText.SetActive(true);
    }

    public void OnInteract()
    {
        talkWithSellerText.SetActive(false);

        if (isNoodlePlaced)
        {
            storeBlocker.SetActive(false);
            isNoodleBought = true;
            DialogueManager.Instance.StartSellerDialogue(noodleBuyDialogueData);
            noodle.OnLoseFocus();
        }  
        else if (!firstTalkIsFinished)
            DialogueManager.Instance.StartSellerDialogue(firstDialogueData);
        else
            DialogueManager.Instance.StartSellerDialogue(secondDialogueData);
    }

    public void OnLoseFocus()
    {
        gameObject.layer = interactableLayer;
        talkWithSellerText.SetActive(false);
    }

    public void HandleFinishDialogue()
    {
        if (isJumpscared)
            StartSitBack();
        else if (isNoodleBought)
            StartJumpscareTalk();
    }

    private void StartJumpscareTalk()
    {
        gameObject.layer = uninteractableLayer;
        audioSource.PlayOneShot(sellerJumpscareSound);
        anim.SetBool("idle", true);

        isJumpscared = true;

        Invoke("StartPartTwoDialogue", waitingTime);

    }

    private void StartSitBack()
    {
        gameObject.layer = uninteractableLayer;
        anim.SetBool("idle", false);
        anim.SetBool("sit", true);
    }

    private void StartPartTwoDialogue()
    {
        DialogueManager.Instance.StartSellerDialogue(noodleBuyDialoguePartTwoData);
    }
}
