using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FryableData;

[CreateAssetMenu(fileName = "NewHolderData", menuName = "Data/Holder")]
public class HolderData : ScriptableObject
{
    [Header("Identity")]
    public ItemIcon[] iconData;
    public string[] focusTextKeys;
    public PlayerManager.HandGrabTypes handGrabType;
    public PlayerManager.HandRigTypes handRigType = PlayerManager.HandRigTypes.SingleHandGrab;
    [Space]
    [Header("Tray Settings")]
    [Tooltip("Tepsideki 6 slot için ayrý ayrý ince ayarlar.")]
    public TraySlotOffset[] slotOffsets; // Inspector'da 6 eleman açýp ayarla
    [Space]
    public GameObject smokePrefabLocal;

    [Tooltip("Eline aldýðýnda scale bozulmasýn diye resetlenecek deðer (Genelde 1,1,1)")]
    public Vector3 grabbedLocalScale = Vector3.one;
    public Vector3 trayLocalScale = Vector3.one;
    [Space]
    public bool isUseable = false;
    public bool isThrowable = true;
    public float throwMultiplier = 1f;
    [Space]
    public Vector3 grabPositionOffset;
    public Vector3 grabRotationOffset;
    public Vector3 grabLocalPositionOffset;
    public Vector3 grabLocalRotationOffset;
    [Space]
    public AudioClip[] audioClips;
    [Space]
    public float grabSoundVolume = 1f;
    public float grabSoundMinPitch = 0.85f;
    public float grabSoundMaxPitch = 1.15f;
    [Space]
    public float dropSoundVolume = 1f;
    public float dropSoundMinPitch = 0.85f;
    public float dropSoundMaxPitch = 1.15f;
    [Space]
    public float throwSoundVolume = 1f;
    public float throwSoundMinPitch = 0.85f;
    public float throwSoundMaxPitch = 1.15f;
    [Space]
    public float packUpSoundVolume = 1f;
    public float packUpSoundMinPitch = 0.85f;
    public float packUpSoundMaxPitch = 1.15f;
    [Space]
    public float traySoundVolume = 1f;
    public float traySoundMinPitch = 0.8f;
    public float traySoundMaxPitch = 1.2f;
    [Space]
    public float soundCooldown = 0.1f;
    public float throwThreshold = 6f;
    public float dropThreshold = 2f;
}
