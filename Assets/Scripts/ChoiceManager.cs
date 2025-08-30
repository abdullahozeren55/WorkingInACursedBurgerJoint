using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceManager : MonoBehaviour
{
    private enum CurrentCoroutine
    {
        PRESSA,
        NOTPRESSA,
        PRESSD,
        NOTPRESSD
    }


    public static ChoiceManager Instance;

    private CurrentCoroutine currentCoroutine;

    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text optionAText;
    [SerializeField] private TMP_Text optionDText;
    [SerializeField] private GameObject visualPart;
    [SerializeField] private Image timer;
    [SerializeField] private GameObject blackAndWhiteEffect;
    [SerializeField] private float bwFadeInMultiplier = 0.1f;
    [SerializeField] private float bwFadeOutMultiplier = 0.9f;
    [SerializeField] private float slowedDownTimeSpeed = 0.25f;
    [SerializeField] private float cameraFovMultiplier = 2f;
    [SerializeField] private float timeToChoose = 5f;
    [SerializeField] private float timeToPressDown = 0.3f;
    [SerializeField] Color pressDownColor;
    [SerializeField] Vector3 pressDownScale;
    [SerializeField] private AudioSource slowMoBeginSource;
    [SerializeField] private AudioSource slowMoEndsSource;
    [SerializeField] private AudioSource clockTickingSource;
    [SerializeField] private AudioClip slowMoBeginSound;
    [SerializeField] private AudioClip slowMoEndsSound;
    [SerializeField] private AudioClip clockTickingSoundFx;

    [Header("Dialogue Parameters")]
    [SerializeField] private DialogueData optionADialogueData;
    [SerializeField] private DialogueData optionDDialogueData;
    [SerializeField] private DialogueData notAnsweringDialogueData;

    private bool isSlowMoEndsStarted;

    private Color startColor;
    private Vector3 startScale;
    private bool isAPressed;
    private bool isDPressed;
    private float aPressedTime;
    private float dPressedTime;

    private float bwFadeInTime;
    private float bwFadeOutTime;
    private float originalCameraFov;
    private float changedCameraFov;

    private ICustomer currentCustomer;

    private Material bwEffectMat;

    private Coroutine pressACoroutine;
    private Coroutine pressDCoroutine;

    private void Awake()
    {
        // Eðer Instance zaten varsa, bu nesneyi yok et
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        isSlowMoEndsStarted = false;

        startScale = optionAText.transform.localScale;
        startColor = optionAText.color;

        aPressedTime = 0f;
        dPressedTime = 0f;

        currentCustomer = null;

        originalCameraFov = Camera.main.fieldOfView;
        changedCameraFov = originalCameraFov * cameraFovMultiplier;

        bwEffectMat = blackAndWhiteEffect.GetComponent<Renderer>().material;

        visualPart.SetActive(false);
        bwEffectMat.SetFloat("_Alpha", 0f);
        blackAndWhiteEffect.SetActive(false);

    }

    private void Update()
    {
        if (visualPart.activeSelf)
        {
            isAPressed = Input.GetKey(KeyCode.A);
            isDPressed = Input.GetKey(KeyCode.D);

            if (isAPressed && currentCoroutine != CurrentCoroutine.PRESSA)
            {
                if (pressACoroutine != null)
                    StopCoroutine(pressACoroutine);

                pressACoroutine = StartCoroutine(APress());
            }
            else if (!isAPressed && currentCoroutine != CurrentCoroutine.NOTPRESSA)
            {
                if (pressACoroutine != null)
                    StopCoroutine(pressACoroutine);

                pressACoroutine = StartCoroutine(ANotPress());
            }

            if (isDPressed && !isAPressed && currentCoroutine != CurrentCoroutine.PRESSD)
            {
                if (pressDCoroutine != null)
                    StopCoroutine(pressDCoroutine);

                pressDCoroutine = StartCoroutine(DPress());
            }
            else if (!isDPressed && currentCoroutine != CurrentCoroutine.NOTPRESSD)
            {
                if (pressDCoroutine != null)
                    StopCoroutine(pressDCoroutine);

                pressDCoroutine = StartCoroutine(DNotPress());
            }

        }
    }

    public void StartTheCustomerChoice(string question, string optionA, string optionD, ICustomer customer, DialogueData optionADialogue, DialogueData optionDDialogue, DialogueData notAnsweringDialogue)
    {
        questionText.text = question;
        optionAText.text = "[A]\n" + optionA;
        optionDText.text = "[D]\n" + optionD;
        bwFadeInTime = timeToChoose * bwFadeInMultiplier;
        bwFadeOutTime = timeToChoose * bwFadeOutMultiplier;
        timer.fillAmount = 1f;

        optionADialogueData = optionADialogue;
        optionDDialogueData = optionDDialogue;
        notAnsweringDialogueData = notAnsweringDialogue;
        currentCustomer = customer;

        visualPart.SetActive(true);

        StartCoroutine(FadeInAndOut());
    }

    private void FinishChoice()
    {
        StopAllCoroutines();
        StartCoroutine(FastOut());

        visualPart.SetActive(false);
    }

    private IEnumerator FastOut()
    {
        float startAlpha = bwEffectMat.GetFloat("_Alpha");
        float currentFov = Camera.main.fieldOfView;
        float currentTimeScale = Time.timeScale;

        if (!slowMoEndsSource.isPlaying) slowMoEndsSource.PlayOneShot(slowMoEndsSound);


        float elapsedTime = 0f;
        float fastOutTime = 5f;

        while (elapsedTime < fastOutTime)
        {
            bwEffectMat.SetFloat("_Alpha", Mathf.Lerp(bwEffectMat.GetFloat("_Alpha"), 0f, elapsedTime / fastOutTime));
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, originalCameraFov, elapsedTime / fastOutTime);
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1f, elapsedTime / fastOutTime);
            if (slowMoBeginSource.isPlaying) slowMoBeginSource.volume = Mathf.Lerp(slowMoBeginSource.volume, 0f, elapsedTime / fastOutTime);
            if (clockTickingSource.isPlaying) clockTickingSource.volume = Mathf.Lerp(clockTickingSource.volume, 0f, elapsedTime / fastOutTime);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        bwEffectMat.SetFloat("_Alpha", 0f);
        blackAndWhiteEffect.SetActive(false);

        Camera.main.fieldOfView = originalCameraFov;

        Time.timeScale = 1f;

        if (slowMoBeginSource.isPlaying) slowMoBeginSource.Stop();
        if (clockTickingSource.isPlaying) clockTickingSource.Stop();

        slowMoBeginSource.volume = 1f;
        clockTickingSource.volume = 1f;


    }

    private IEnumerator APress()
    {

        currentCoroutine = CurrentCoroutine.PRESSA;

        Vector3 beginScale = optionAText.transform.localScale;

        Color beginColor = optionAText.color;

        while (aPressedTime < timeToPressDown)
        {
            optionAText.transform.localScale = Vector3.Lerp(beginScale, pressDownScale, aPressedTime / timeToPressDown);
            optionAText.color = Color.Lerp(beginColor, pressDownColor, aPressedTime / timeToPressDown);
            aPressedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        optionAText.transform.localScale = pressDownScale;
        optionAText.color = pressDownColor;

        FinishChoice();

        currentCustomer.CurrentAction = ICustomer.Action.GotAnswerA;

        DialogueManager.Instance.StartCustomerDialogue(currentCustomer, optionADialogueData);
    }

    private IEnumerator ANotPress()
    {

        currentCoroutine = CurrentCoroutine.NOTPRESSA;

        Vector3 beginScale = optionAText.transform.localScale;

        Color beginColor = optionAText.color;

        while (aPressedTime > 0f)
        {
            optionAText.transform.localScale = Vector3.Lerp(beginScale, startScale, 1 - (aPressedTime / timeToPressDown));
            optionAText.color = Color.Lerp(beginColor, startColor, 1 - (aPressedTime / timeToPressDown));
            aPressedTime -= Time.unscaledDeltaTime;
            yield return null;
        }

        aPressedTime = 0f;
        optionAText.transform.localScale = startScale;
        optionAText.color = startColor;

        pressACoroutine = null;
    }

    private IEnumerator DPress()
    {

        currentCoroutine = CurrentCoroutine.PRESSD;

        Vector3 beginScale = optionDText.transform.localScale;

        Color beginColor = optionDText.color;

        while (dPressedTime < timeToPressDown)
        {
            optionDText.transform.localScale = Vector3.Lerp(beginScale, pressDownScale, dPressedTime / timeToPressDown);
            optionDText.color = Color.Lerp(beginColor, pressDownColor, dPressedTime / timeToPressDown);
            dPressedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        optionDText.transform.localScale = pressDownScale;
        optionDText.color = pressDownColor;

        FinishChoice();

        currentCustomer.CurrentAction = ICustomer.Action.GotAnswerD;

        DialogueManager.Instance.StartCustomerDialogue(currentCustomer, optionDDialogueData);
    }

    private IEnumerator DNotPress()
    {

        currentCoroutine = CurrentCoroutine.NOTPRESSD;

        Vector3 beginScale = optionDText.transform.localScale;

        Color beginColor = optionDText.color;

        while (dPressedTime > 0f)
        {
            optionDText.transform.localScale = Vector3.Lerp(beginScale, startScale, 1 - (dPressedTime / timeToPressDown));
            optionDText.color = Color.Lerp(beginColor, startColor, 1 - (dPressedTime / timeToPressDown));
            dPressedTime -= Time.unscaledDeltaTime;
            yield return null;
        }

        dPressedTime = 0f;
        optionDText.transform.localScale = startScale;
        optionDText.color = startColor;

        pressDCoroutine = null;
    }

    private IEnumerator FadeInAndOut()
    {
        bwEffectMat.SetFloat("_Alpha", 0f);
        blackAndWhiteEffect.SetActive(true);

        float startCameraFov = Camera.main.fieldOfView;

        slowMoBeginSource.PlayOneShot(slowMoBeginSound);

        float timeElapsed = 0f;

        while (timeElapsed < bwFadeInTime)
        {
            bwEffectMat.SetFloat("_Alpha", Mathf.Lerp(0f, 1f, timeElapsed / bwFadeInTime));
            Time.timeScale = Mathf.Lerp(1f, slowedDownTimeSpeed, timeElapsed / bwFadeInTime);
            Camera.main.fieldOfView = Mathf.Lerp(startCameraFov, changedCameraFov, timeElapsed / bwFadeInTime);
            timeElapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        bwEffectMat.SetFloat("_Alpha", 1f);
        Time.timeScale = slowedDownTimeSpeed;
        Camera.main.fieldOfView = changedCameraFov;

        clockTickingSource.PlayOneShot(clockTickingSoundFx);

        startCameraFov = Camera.main.fieldOfView;

        timeElapsed = 0f;

        while (timeElapsed < bwFadeOutTime)
        {
            bwEffectMat.SetFloat("_Alpha", Mathf.Lerp(1f, 0f, timeElapsed / bwFadeOutTime));
            Time.timeScale = Mathf.Lerp(slowedDownTimeSpeed, 1f, timeElapsed / bwFadeOutTime);
            Camera.main.fieldOfView = Mathf.Lerp(startCameraFov, originalCameraFov, timeElapsed / bwFadeOutTime);
            timer.fillAmount = Mathf.Lerp(1f, 0f, timeElapsed / bwFadeOutTime);

            if (bwFadeOutTime - timeElapsed < 1f && !isSlowMoEndsStarted)
            {
                slowMoEndsSource.PlayOneShot(slowMoEndsSound);
                isSlowMoEndsStarted = true;
            }

            timeElapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        bwEffectMat.SetFloat("_Alpha", 0f);
        Time.timeScale = 1f;
        Camera.main.fieldOfView = originalCameraFov;
        timer.fillAmount = 0f;

        isSlowMoEndsStarted = false;

        FinishChoice();

        currentCustomer.CurrentAction = ICustomer.Action.NotGotAnswer;

        DialogueManager.Instance.StartCustomerDialogue(currentCustomer, notAnsweringDialogueData);

    }
}
