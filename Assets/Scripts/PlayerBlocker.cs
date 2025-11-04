using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBlocker : MonoBehaviour
{
    [SerializeField] private DialogueData[] dialogues;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!DialogueManager.Instance.IsInDialogue)
            {
                int rand = Random.Range(0, dialogues.Length);
                DialogueManager.Instance.StartSelfDialogue(dialogues[rand]);
            }
        }
    }
}
