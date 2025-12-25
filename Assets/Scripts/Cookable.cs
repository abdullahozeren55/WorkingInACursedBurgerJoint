using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cookable : MonoBehaviour
{
    public enum CookAmount
    {
        RAW,
        REGULAR,
        BURNT
    }

    [SerializeField] private CookableData cookableData;
    [SerializeField] private GameObject[] cookTexts;

    public CookAmount cookAmount;

    private MeshRenderer meshRenderer;


    private bool isCooking;
    private float currentCookedTime;

    private ParticleSystem currentCookingParticles;
    private ParticleSystem currentSmokeParticlesWorld;
    private ParticleSystem currentSmokeParticlesLocal;

    private BurgerIngredient burgerIngredient;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        burgerIngredient = GetComponent<BurgerIngredient>();

        currentCookedTime = 0;

    }

    private void Update()
    {
        if (isCooking && cookAmount != CookAmount.BURNT)
        {
            currentCookedTime += Time.deltaTime;

            if (currentCookedTime > cookableData.cookTime[1])
            {
                ChangeCookAmount(2);

                if (currentCookingParticles != null)
                    StopCookingParticles();

                SoundManager.Instance.RemoveItemFromGrill(burgerIngredient.data.ingredientType);
            }
            else if (currentCookedTime > cookableData.cookTime[0])
            {
                ChangeCookAmount(1);
            }
        }
    }

    private void LateUpdate()
    {
        if (currentSmokeParticlesLocal != null)
        {
            currentSmokeParticlesLocal.transform.position = transform.position;
        }
    }

    private void CreateCookingParticles()
    {
        currentCookingParticles =  Instantiate(cookableData.cookingParticles, transform.position, Quaternion.Euler(-90f, 0f, 0f), transform);
        currentSmokeParticlesWorld =  Instantiate(cookableData.smokeParticlesWorld, transform.position, Quaternion.Euler(-90f, 0f, 0f), transform);
        
        if (currentSmokeParticlesLocal == null)
        {
            currentSmokeParticlesLocal = Instantiate(cookableData.smokeParticlesLocal, transform.position, Quaternion.Euler(-90f, 0f, 0f));
        }

        
    }

    private void StopCookingParticles()
    {
        // 1. Cýzbýz Efekti (World)
        if (currentCookingParticles != null)
        {
            DetachAndDestroy(currentCookingParticles);
            currentCookingParticles = null;
        }

        // 2. Izgara Dumaný (World)
        if (currentSmokeParticlesWorld != null)
        {
            DetachAndDestroy(currentSmokeParticlesWorld);
            currentSmokeParticlesWorld = null;
        }

        // 3. Piþmiþlik Dumaný (Local) - SADECE ÇÝÐ ÝSE YOK ET
        if (currentSmokeParticlesLocal != null && cookAmount == CookAmount.RAW)
        {
            // Destroy(currentSmokeParticlesLocal.gameObject); // <-- BUNU SÝLDÝK

            // YERÝNE BUNU YAZDIK:
            // 1. Duman üretimini durdur (mevcutlar sönerek kaybolsun)
            currentSmokeParticlesLocal.Stop();

            // 2. Referansý kopar (LateUpdate artýk takip etmesin, olduðu yerde kalsýn)
            currentSmokeParticlesLocal = null;
        }
    }

    // YENÝ YARDIMCI FONKSÝYON:
    // Partikülü Burgerden ayýrýr ama boyutunu bozmaz.
    private void DetachAndDestroy(ParticleSystem ps)
    {
        // 1. "Hayalet" bir obje yarat
        GameObject ghost = new GameObject("Temp_Particle_Ghost");

        // 2. Bu hayaleti Burgerin olduðu yere ve AYNI SCALE deðerine getir
        ghost.transform.SetPositionAndRotation(transform.position, transform.rotation);
        ghost.transform.localScale = transform.lossyScale; // <-- Scale sorununu çözen satýr!

        // 3. Partikülü Burgerden alýp Hayalete evlatlýk ver
        ps.transform.SetParent(ghost.transform);

        // 4. Durdur
        ps.Stop();

        // 5. Hayaleti (ve içindeki partikülü) 15 saniye sonra komple yok et
        Destroy(ghost, 15f);
    }

    private void OnDisable()
    {
        // Köfte cebe girince (Disable), dumaný da gizle.
        if (currentSmokeParticlesLocal != null)
        {
            // YENÝ ÖZELLÝK: Prewarm'ý açýyoruz.
            // Böylece tekrar açýldýðýnda "baþtan tütmek" yerine "tütmüþ halde" gelecek.
            var main = currentSmokeParticlesLocal.main;
            main.prewarm = true;

            currentSmokeParticlesLocal.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // Köfte cepten çýktý.

        // Eðer duman efektimiz varsa (yani yok edilmemiþse - ki bu piþmiþ demek oluyor)
        if (currentSmokeParticlesLocal != null)
        {
            // EKSTRA GÜVENLÝK: Yine de çið mi diye bak. 
            // Belki bir bug oldu, çið köftede duman kaldý.
            if (cookAmount == CookAmount.RAW)
            {
                currentSmokeParticlesLocal.gameObject.SetActive(false);
                // Hatta temizleyelim:
                Destroy(currentSmokeParticlesLocal.gameObject);
                currentSmokeParticlesLocal = null;
            }
            else
            {
                // Piþmiþse aç dumaný tütsün
                currentSmokeParticlesLocal.gameObject.SetActive(true);

                if (!currentSmokeParticlesLocal.isPlaying)
                    currentSmokeParticlesLocal.Play();
            }
        }
    }

    private void ChangeCookAmount(int index)
    {
        if (index == 0)
        {
            cookAmount = CookAmount.RAW;
        }
        else if (index == 1)
        {
            cookAmount = CookAmount.REGULAR;
        }
        else if (index == 2)
        {
            cookAmount = CookAmount.BURNT;
        }

        meshRenderer.material = cookableData.materials[index];
        burgerIngredient.ChangeCookAmount(index);
    }

    public void StopCooking()
    {
        if (isCooking)
        {
            isCooking = false;

            if (currentCookingParticles != null)
                StopCookingParticles();

            SoundManager.Instance.RemoveItemFromGrill(burgerIngredient.data.ingredientType);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Grill") && isActiveAndEnabled)
        {
            if (cookAmount == CookAmount.BURNT) return;

            isCooking = true;

            if (currentCookingParticles == null)
                CreateCookingParticles();

            SoundManager.Instance.AddItemToGrill(burgerIngredient.data.ingredientType);
            SoundManager.Instance.PlaySoundFX(cookableData.cookingSound, transform, cookableData.cookingSoundVolume, cookableData.cookingSoundMinPitch, cookableData.cookingSoundMaxPitch);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Grill") && isActiveAndEnabled)
        {
            StopCooking();
        }
    }

    private void OnDestroy()
    {
        if (currentSmokeParticlesLocal != null)
            Destroy(currentSmokeParticlesLocal.gameObject);
    }
}
