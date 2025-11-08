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
    [SerializeField] private AudioSource audioSource;
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

                if (audioSource.isPlaying)
                    AudioStopWithFadeOut();

                this.enabled = false;
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
        currentCookingParticles.Stop();
        currentSmokeParticlesWorld.Stop();

        currentCookingParticles = null;
        currentSmokeParticlesWorld = null;

        if (cookAmount == CookAmount.RAW)
        {
            currentSmokeParticlesLocal.Stop();
            currentSmokeParticlesLocal = null;
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

    private void AudioStopWithFadeOut()
    {
        StartCoroutine(FadeOut(audioSource, cookableData.audioFadeOutDuration));
    }

    public void StopCooking()
    {
        if (isCooking)
        {
            isCooking = false;

            if (currentCookingParticles != null)
                StopCookingParticles();

            if (audioSource.isPlaying)
                audioSource.Pause();
        }
    }

    private IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }

        audioSource.Stop(); // Tamamen durdur
        audioSource.volume = startVolume; // Ses seviyesini eski haline getir
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Grill") && isActiveAndEnabled)
        {
            isCooking = true;

            if (currentCookingParticles == null)
                CreateCookingParticles();

            if (!audioSource.isPlaying)
            {
                if (currentCookedTime > 0)
                    audioSource.UnPause();
                else
                    audioSource.PlayOneShot(cookableData.cookingSound);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Grill") && isActiveAndEnabled)
        {
            StopCooking();
        }
    }
}
