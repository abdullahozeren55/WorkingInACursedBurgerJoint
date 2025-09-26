using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public enum CameraName
    {
        Null,
        FirstPerson,
        Monitor,
        Customer0
    }

    [System.Serializable]
    public class CameraEntry
    {
        public CameraName camName;
        public CinemachineVirtualCamera vCam;
    }

    [SerializeField] private GameObject crosshair;
    [Space]
    [SerializeField] private CameraEntry[] cameras;

    private FirstPersonController firstPersonController;
    private KeyCode interactKey;

    private CameraEntry currentCam;
    private int basePriority = 10;

    private void Awake()
    {
        if (Instance == null)
        {
            // If not, set this instance as the singleton
            Instance = this;

            // Optionally, mark GameManager as not destroyed between scene loads
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this one to enforce the singleton pattern
            Destroy(gameObject);
        }

        firstPersonController = FindFirstObjectByType<FirstPersonController>();
        interactKey = firstPersonController.throwKey;
    }

    public void SwitchToCamera(CameraName name)
    {
        if (name == currentCam.camName || name == CameraName.Null)
            return;

        // Lower priority of current camera
        foreach (CameraEntry entry in cameras)
        {
            if (entry.camName == name)
            {
                currentCam.vCam.Priority = basePriority;
                currentCam = entry;
                currentCam.vCam.Priority = basePriority + 1;

                break;
            }
        }
    }

    public void InitializeCamera(CameraName name)
    {
        if (currentCam != null)
            return;

        foreach (CameraEntry entry in cameras)
        {
            if (entry.camName == name)
            {
                currentCam = entry;
                currentCam.vCam.Priority = basePriority + 1;

                break;
            }
        }
    }

    public CinemachineVirtualCamera GetCamera()
    {
        return currentCam.vCam;
    }

    public void SwitchToFirstPersonCamera()
    {
        SwitchToCamera(CameraName.FirstPerson);
    }
}
