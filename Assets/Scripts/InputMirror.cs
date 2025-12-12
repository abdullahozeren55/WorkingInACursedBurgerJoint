using UnityEngine;
using TMPro;

public class InputMirror : MonoBehaviour
{
    [Header("Otomatik Bulur (Script bu objede ise boþ býrak)")]
    public TMP_InputField driverInput;

    [Header("Takipçi (Monitördeki)")]
    public TMP_InputField passengerInput;

    // Asýl metin objelerinin transformlarý
    private RectTransform driverTextRect;
    private RectTransform passengerTextRect;

    private void Start()
    {
        // Eðer driverInput atanmadýysa, scriptin üzerinde olduðu objeden al
        if (driverInput == null)
            driverInput = GetComponent<TMP_InputField>();

        // Referanslarý al (Hata vermesin diye kontrol ediyoruz)
        if (driverInput != null && driverInput.textComponent != null)
            driverTextRect = driverInput.textComponent.rectTransform;

        if (passengerInput != null && passengerInput.textComponent != null)
            passengerTextRect = passengerInput.textComponent.rectTransform;
    }

    // Manuel olarak çaðýrmak istersen diye public yaptýk
    public void SyncInputs()
    {
        if (driverInput == null || passengerInput == null) return;

        // 1. ÖNCE METNÝ KOPYALA
        // Monitördeki metni güncelle
        passengerInput.text = driverInput.text;

        // 2. TMP'YÝ GÜNCELLEMEYE ZORLA (KRÝTÝK ADIM)
        // Metni deðiþtirdiðimiz an TMP layout'u hemen hesaplamazsa, 
        // pozisyonu eþitlemeye çalýþtýðýmýzda eski boyutta kalabilir.
        // Bu komut "Hemen þimdi hesapla þunlarý" der.
        passengerInput.ForceLabelUpdate();

        // 3. EN SON POZÝSYONU EÞÝTLE
        if (driverTextRect != null && passengerTextRect != null)
        {
            passengerTextRect.anchoredPosition = driverTextRect.anchoredPosition;
        }
    }
}