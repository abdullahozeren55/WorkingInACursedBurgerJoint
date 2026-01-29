using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCustomerProfile", menuName = "Data/CustomerProfile")]
public class CustomerProfile : ScriptableObject
{
    [Header("Identity")]
    public CustomerID ID;
    public Material SkinMaterial; // Görünüþü
    public float WalkSpeed = 3.5f;
    public float ArrivalDistance = 0.5f;

    [Header("Sitting Settings")]
    [Tooltip("Koltuk noktasýna göre ne kadar yukarýda/aþaðýda/ileride duracak?")]
    public Vector3 SitPositionOffset; // Örn: Y=0.1 (Biraz yukarý kaldýr), Z=-0.1 (Biraz arkaya yasla)

    [Tooltip("Otururken ekstra bir dönüþ gerekir mi?")]
    public Vector3 SitRotationOffset; // Genelde (0,0,0) olur ama yamuk oturan varsa buradan ayarlanýr.

    [System.Serializable]
    public struct PotentialOrder
    {
        public OrderData Order;

        [Header("Normal Interaction")]
        public DialogueData NormalDialogue; // Eskiden OrderDialogue idi, adýný netleþtirdik.

        [Header("Horror Interaction")]
        public DialogueData GlitchDialogue; // Korkunçlu versiyon
        [Range(0f, 100f)] public float GlitchChance; // % kaç ihtimalle korkunç olacak?
    }

    // --- YENÝ STRUCT: Sadece diyalog ve glitch ihtimali ---
    [System.Serializable]
    public struct InteractionDialogue
    {
        [Header("Normal")]
        public DialogueData NormalDialogue;

        [Header("Horror")]
        public DialogueData GlitchDialogue;
        [Range(0f, 100f)] public float GlitchChance; // % kaç ihtimalle korkunç cevap verecek?
    }

    // Bu profildeki müþteri neleri sipariþ edebilir?
    public List<PotentialOrder> PossibleOrders;

    [Header("Feedbacks")]
    // ESKÝSÝ: public DialogueData WrongOrderDialogue;
    // YENÝSÝ:
    public List<InteractionDialogue> PossibleWrongOrderDialogues;

    // ESKÝSÝ: public DialogueData CorrectOrderDialogue;
    // YENÝSÝ:
    public List<InteractionDialogue> PossibleCorrectOrderDialogues;

    [Header("Reactions")]
    // Tepsi boþsa ne desin? ("Dalga mý geçiyorsun?", "Tabak boþ?")
    public List<InteractionDialogue> PossibleEmptyTrayDialogues;

    // Býçak yerse ne desin? ("AH!", "DELÝ MÝSÝN?!", "SENÝ POLÝSE VERECEÐÝM!")
    public List<InteractionDialogue> PossibleKnifeHitDialogues;

    [Header("Audio")]
    public FootstepData FootstepSounds;
}