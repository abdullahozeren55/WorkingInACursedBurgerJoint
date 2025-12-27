using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class FryerBasket : MonoBehaviour, IInteractable
{
    // --- IInteractable Özellikleri ---
    public bool CanInteract { get => canInteract; set => canInteract = value; }
    [SerializeField] private bool canInteract = true;

    public string FocusTextKey { get => focusTextKeys[basketStateNum]; set => focusTextKeys[basketStateNum] = value; }
    [SerializeField] private string[] focusTextKeys; // 0: "Ýndir", 1: "Kaldýr/As"
    private int basketStateNum = 0; // 0: Askýda (Havada), 1: Kýzarýyor (Yaðda)

    public PlayerManager.HandRigTypes HandRigType { get => handRigType; set => handRigType = value; }
    [SerializeField] private PlayerManager.HandRigTypes handRigType;

    public bool OutlineShouldBeRed { get => outlineShouldBeRed; set => outlineShouldBeRed = value; }
    [SerializeField] private bool outlineShouldBeRed;
    // ---------------------------------

    [Header("Simulation State")]
    public bool IsHeavy = false; // Ýleride içine malzeme girince true olacak

    [Header("Position References (Normal)")]
    [SerializeField] private Transform cookingPoint; // Yaðdaki hali (Sabit)
    [SerializeField] private Transform hangingPoint; // Askýdaki hali (Normal)
    [SerializeField] private Transform apexPoint;    // Havadaki tepe noktasý (Normal)

    [Header("Position References (Heavy)")]
    [SerializeField] private Transform hangingHeavyPoint; // Askýdaki hali (Aðýr/Eðik)
    [SerializeField] private Transform apexHeavyPoint;

    [Header("Movement Settings")]
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private Ease moveEase = Ease.InOutSine;

    [Header("Interrupt Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float directReturnThreshold = 0.35f; // %35'in altýndaysa direkt dön

    [Header("Fryer Integration")]
    [SerializeField] private Fryer connectedFryer; // Yað efektini tetiklemek için

    [Header("Audio")]
    //public AudioClip liftSound; // Metal sesi
    //public AudioClip dropSound; // Yaða girme sesi
    //public string audioTag = "SFX";

    private bool isFrying; // true: yaðda, false: askýda
    private bool isPhysicallyInOil = false;

    private Sequence currentSeq; // Ana hareket (A -> B)
    private Sequence settleSeq;  // Yerleþme/Yüzme hareketi (Bitiþteki loop)

    // Layer Cache
    private int interactableLayer;
    private int interactableOutlinedLayer;
    private int interactableOutlinedRedLayer;

    private void Awake()
    {
        // Layer ID'lerini al
        interactableLayer = LayerMask.NameToLayer("Interactable");
        interactableOutlinedLayer = LayerMask.NameToLayer("InteractableOutlined");
        interactableOutlinedRedLayer = LayerMask.NameToLayer("InteractableOutlinedRed");

        isFrying = false;
        basketStateNum = 0;

        // Baþlangýç pozisyonunu ayarla
        UpdateTransformToState(false);
    }

    private void UpdateTransformToState(bool frying)
    {
        Transform targetHanging = IsHeavy ? hangingHeavyPoint : hangingPoint;
        Transform target = frying ? cookingPoint : targetHanging;

        transform.position = target.position;
        transform.rotation = target.rotation;
    }

    public void OnInteract()
    {
        if (!CanInteract) return;

        // ESKÝSÝ: if (DOTween.IsTweening(transform)) return; <-- Bunu sildik.

        // YENÝSÝ:
        // Eðer hali hazýrda bir hareket varsa, onu anýnda öldür.
        // Complete parametresini false yapýyoruz ki eski animasyonun
        // "OnComplete" eventleri (ses çalma, fryer tetikleme vs.) çalýþmasýn.
        transform.DOKill(false);

        TogglePosition();
    }

    private void TogglePosition()
    {
        // --- KESÝLME KONTROLÜ ---
        float currentProgress = 0f;
        bool isEarlyReturn = false;

        if (currentSeq != null && currentSeq.IsActive())
        {
            currentProgress = currentSeq.ElapsedPercentage();
            if (currentProgress < directReturnThreshold) isEarlyReturn = true;
            currentSeq.Kill();
        }

        // Settle animasyonu varsa onu da öldür
        if (settleSeq != null && settleSeq.IsActive()) settleSeq.Kill();

        isFrying = !isFrying;
        basketStateNum = isFrying ? 1 : 0;

        //SoundManager.Instance.PlaySoundFX(isFrying ? dropSound : liftSound, transform, 1f, 1f, 1f, true, audioTag);

        Transform currentHanging = IsHeavy ? hangingHeavyPoint : hangingPoint;
        Transform currentApex = IsHeavy ? apexHeavyPoint : apexPoint;
        Transform targetTransform = isFrying ? cookingPoint : currentHanging;
        Transform pathApex = currentApex;

        // Sequence Baþlat
        currentSeq = DOTween.Sequence();

        // --- HAREKET VE ZAMANLAMA ---
        float duration = isEarlyReturn ? moveDuration * 0.6f : moveDuration;

        if (isEarlyReturn)
        {
            // Erken Dönüþ (Düz Hareket)
            currentSeq.Append(transform.DOMove(targetTransform.position, duration).SetEase(Ease.OutQuad));
            currentSeq.Join(transform.DORotateQuaternion(targetTransform.rotation, duration).SetEase(Ease.OutQuad));
        }
        else
        {
            // Normal Dönüþ (Kavisli)
            Vector3[] pathPoints = new Vector3[] { pathApex.position, targetTransform.position };
            currentSeq.Append(transform.DOPath(pathPoints, duration, PathType.CatmullRom).SetEase(moveEase));

            float halfDuration = duration / 2f;
            currentSeq.Insert(0f, transform.DORotateQuaternion(pathApex.rotation, halfDuration).SetEase(Ease.OutSine));
            currentSeq.Insert(halfDuration, transform.DORotateQuaternion(targetTransform.rotation, halfDuration).SetEase(Ease.InSine));
        }

        // --- KRÝTÝK NOKTA: YAÐ TETÝKLEME ZAMANLAMASI ---
        if (connectedFryer != null)
        {
            if (isFrying)
            {
                // SENARYO: AÞAÐI ÝNÝYORUZ (DALDIRMA)
                // Tam suya gireceði zamaný hesapla (Yolun %85'i gibi)
                // Eðer EarlyReturn ise yol kýsa olduðu için %70 gibi daha erken bir nokta olabilir
                float hitWaterTime = duration * (isEarlyReturn ? 0.7f : 0.85f);

                // O saniyeye gelince "SyncFryerState" fonksiyonunu çalýþtýr
                currentSeq.InsertCallback(hitWaterTime, () => SyncFryerState(true));
            }
            else
            {
                // SENARYO: YUKARI ÇIKIYORUZ (KALDIRMA)
                // Hareket baþladýðý an (0. saniye) yaðdan çýkmýþ sayýlýrýz
                currentSeq.InsertCallback(0f, () => SyncFryerState(false));
            }
        }

        // --- BÝTÝÞ EFEKTLERÝ (Sadece Salýným) ---
        currentSeq.OnComplete(() =>
        {
            settleSeq = DOTween.Sequence();

            if (!isFrying) // ASKIDA
            {
                Vector3 targetEuler = IsHeavy ? hangingHeavyPoint.eulerAngles : hangingPoint.eulerAngles;
                float momentumAngle = IsHeavy ? 6f : 4f;
                float settleTime = IsHeavy ? 0.35f : 0.2f;

                settleSeq.Append(transform.DORotate(targetEuler + new Vector3(momentumAngle, 0, 0), 0.1f).SetEase(Ease.OutSine));
                settleSeq.Append(transform.DORotate(targetEuler, settleTime).SetEase(Ease.OutBack));
            }
            else // YAÐDA (YÜZME)
            {
                float floatAmount = IsHeavy ? 0.015f : 0.04f;
                float floatDuration = IsHeavy ? 0.4f : 0.6f;

                settleSeq.Append(transform.DOMove(cookingPoint.position + new Vector3(0, floatAmount, 0), floatDuration * 0.4f).SetEase(Ease.OutSine));
                settleSeq.Append(transform.DOMove(cookingPoint.position, floatDuration * 0.6f).SetEase(Ease.InOutSine));
            }

            PlayerManager.Instance.TryChangingFocusText(this, FocusTextKey);
        });
    }

    // --- YARDIMCI METOD (KOD TEKRARINI ÖNLEMEK ÝÇÝN) ---
    private void SyncFryerState(bool enteringOil)
    {
        if (connectedFryer == null) return;

        if (enteringOil)
        {
            // Ýçeri giriyoruz, eðer zaten içeride deðilsek haber ver
            if (!isPhysicallyInOil)
            {
                connectedFryer.OnBasketDown(IsHeavy);
                isPhysicallyInOil = true;
            }
        }
        else
        {
            // Dýþarý çýkýyoruz, eðer içerideysek haber ver
            if (isPhysicallyInOil)
            {
                connectedFryer.OnBasketUp(IsHeavy);
                isPhysicallyInOil = false;
            }
        }
    }

    // --- IInteractable Standart Metotlar ---

    public void OnFocus()
    {
        if (!CanInteract) return;
        ChangeLayer(OutlineShouldBeRed ? interactableOutlinedRedLayer : interactableOutlinedLayer);
    }

    public void OnLoseFocus()
    {
        if (!CanInteract) return;
        ChangeLayer(interactableLayer);
    }

    public void OutlineChangeCheck()
    {
        if (gameObject.layer == interactableOutlinedLayer && OutlineShouldBeRed)
            ChangeLayer(interactableOutlinedRedLayer);
        else if (gameObject.layer == interactableOutlinedRedLayer && !OutlineShouldBeRed)
            ChangeLayer(interactableOutlinedLayer);
    }

    public void ChangeLayer(int layer)
    {
        ChangeLayerRecursive(gameObject, layer);
    }

    // Bu da iþi yapan yardýmcý fonksiyon (Private olabilir)
    private void ChangeLayerRecursive(GameObject obj, int newLayer)
    {
        if (null == obj) return;

        // 1. Þu anki objeyi deðiþtir
        obj.layer = newLayer;

        // 2. Çocuklarý gez
        foreach (Transform child in obj.transform)
        {
            if (null == child) continue;

            // DÝKKAT: Burada sadece layer atamak yerine, fonksiyonu tekrar çaðýrýyoruz.
            // Böylece child da kendi içine (torunlara) bakýyor.
            ChangeLayerRecursive(child.gameObject, newLayer);
        }
    }

    public void HandleFinishDialogue() { }
}