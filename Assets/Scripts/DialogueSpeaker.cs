using UnityEngine;

public class DialogueSpeaker : MonoBehaviour, IDialogueSpeaker
{
    // SerializedField'ý kaldýrabilirsin veya debug için ReadOnly býrakabilirsin.
    [SerializeField] private CustomerID _speakerID = CustomerID.None;
    [SerializeField] private Transform _lookAtPoint;

    public CustomerID SpeakerID { get => _speakerID; set => _speakerID = value; }
    public Transform LookAtPoint { get => _lookAtPoint; set => _lookAtPoint = value; }

    // Start() SÝLÝNDÝ. Kendi kendine kaydolmasýn.

    // Controller bunu çaðýracak

    private void OnEnable()
    {
        // Havuzdan tekrar çýkarsa ve kimliði belliyse tekrar kaydolsun
        if (_speakerID != CustomerID.None)
        {
            RegisterToManager();
        }
    }

    private void OnDisable()
    {
        if (DialogueManager.Instance != null)
        {
            // Olayý basitleþtirdik: Direkt kendini sildiriyor.
            DialogueManager.Instance.UnregisterSpeaker(this);
        }
    }

    private void RegisterToManager()
    {
        if (DialogueManager.Instance != null && _speakerID != CustomerID.None)
        {
            DialogueManager.Instance.RegisterSpeaker(this);
        }
    }
}