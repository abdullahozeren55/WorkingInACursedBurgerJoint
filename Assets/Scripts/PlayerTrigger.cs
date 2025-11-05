using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        CloseTheDoorAndStartNoodlePrepare
    }

    public TriggerType type;


    [Header("CloseTheDoorAndStartNoodlePrepare")]
    public Door houseDoor;
    public DialogueData closeTheDoorAndStartNoodlePrepareDialogue;

    private Collider col;

    private void Awake()
    {
        col = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            col.enabled = false;

            if (type == TriggerType.CloseTheDoorAndStartNoodlePrepare)
                HandleCloseTheDoorAndStartNoodlePrepare();
        }
    }

    private void HandleCloseTheDoorAndStartNoodlePrepare()
    {
        if (!houseDoor.isOpened)
        {
            PlayerManager.Instance.ResetPlayerInteract(houseDoor, true);
            DialogueManager.Instance.StartSelfDialogue(closeTheDoorAndStartNoodlePrepareDialogue);
        }
            
        else
        {
            houseDoor.dialogueAfterInteraction = closeTheDoorAndStartNoodlePrepareDialogue;
            houseDoor.shouldBeUninteractableAfterInteraction = true;
            houseDoor.shouldPlayDialogueAfterInteraction = true;
        }

        NoodleManager.Instance.SetCurrentNoodleUseable();
    }
}
