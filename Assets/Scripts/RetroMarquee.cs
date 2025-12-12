using UnityEngine;
using TMPro;
using System.Collections;

public class RetroMarquee : MonoBehaviour
{
    [Header("Hýz Ayarlarý")]
    public float stepInterval = 0.1f;
    public float pixelsPerStep = 5f;
    public float gap = 50f;

    private RectTransform rect1;
    private RectTransform rect2;
    private float textWidth;
    private float parentWidth; // Ekranýn geniþliði
    private bool isScrolling = true;

    private void Start()
    {
        rect1 = GetComponent<RectTransform>();
        TextMeshProUGUI tmp = GetComponent<TextMeshProUGUI>();

        StartCoroutine(SetupMarquee(tmp));
    }

    IEnumerator SetupMarquee(TextMeshProUGUI originalTMP)
    {
        // Unity UI'ý oturtana kadar bekle
        yield return new WaitForEndOfFrame();

        textWidth = rect1.rect.width;

        // --- KRÝTÝK EKLEME: PARENT GENÝÞLÝÐÝNÝ BUL ---
        // Yazýnýn içinde durduðu Panelin (Maskenin) geniþliðini alýyoruz.
        if (transform.parent != null)
        {
            RectTransform pRect = transform.parent.GetComponent<RectTransform>();
            if (pRect != null) parentWidth = pRect.rect.width;
        }
        else
        {
            // Eðer parent yoksa (ki olmalý) rastgele bir geniþlik ver ki hata vermesin
            parentWidth = 500f;
        }

        // Kopyayý oluþtur
        GameObject cloneObj = Instantiate(gameObject, transform.parent);
        cloneObj.name = gameObject.name + "_Clone";
        Destroy(cloneObj.GetComponent<RetroMarquee>());

        rect2 = cloneObj.GetComponent<RectTransform>();

        // --- KONUMLARI GÜNCELLE ---

        // Rect1 (Asýl): Artýk 0'da deðil, Panelin EN SAÐINDA baþlýyor.
        rect1.anchoredPosition = new Vector2(parentWidth, rect1.anchoredPosition.y);

        // Rect2 (Kopya): Rect1'in arkasýnda, vagon gibi takip ediyor.
        // Konumu: Ekran Geniþliði + Metin Geniþliði + Boþluk
        rect2.anchoredPosition = new Vector2(parentWidth + textWidth + gap, rect1.anchoredPosition.y);

        StartCoroutine(ScrollRoutine());
    }

    IEnumerator ScrollRoutine()
    {
        while (isScrolling)
        {
            yield return new WaitForSeconds(stepInterval);

            MoveRect(rect1);
            MoveRect(rect2);
        }
    }

    void MoveRect(RectTransform rect)
    {
        Vector2 pos = rect.anchoredPosition;
        pos.x -= pixelsPerStep;

        // Döngü mantýðý ayný: Ekrandan tamamen çýkýnca arkaya ýþýnla
        if (pos.x < -textWidth)
        {
            RectTransform otherRect = (rect == rect1) ? rect2 : rect1;
            pos.x = otherRect.anchoredPosition.x + textWidth + gap;
        }

        rect.anchoredPosition = pos;
    }

    public void RefreshText(string newText)
    {
        GetComponent<TextMeshProUGUI>().text = newText;
        if (rect2 != null) Destroy(rect2.gameObject);
        StopAllCoroutines();
        StartCoroutine(SetupMarquee(GetComponent<TextMeshProUGUI>()));
    }
}