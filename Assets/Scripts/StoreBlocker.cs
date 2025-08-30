using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreBlocker : MonoBehaviour
{

    [SerializeField] private Camera cam;
    [SerializeField] private DialogueData dialogueData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueManager.Instance.StartSelfDialogue(dialogueData);
        }
    }
}
