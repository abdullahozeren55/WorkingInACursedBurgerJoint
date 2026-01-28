using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "NewFootstepData", menuName = "Data/FootstepData")]
public class FootstepData : ScriptableObject
{
    // --- YENÝ: Zemin Layer Ayarý ---
    [Header("Detection Settings")]
    [Tooltip("Raycast hangi layerlara çarpsýn? (Kendi layerýný ve Triggerlarý hariç tut!)")]
    public LayerMask GroundLayerMask;

    [System.Serializable]
    public struct SurfaceAudio
    {
        public SurfaceType Surface;
        public AudioClip[] Clips;
    }

    [Header("Surface Sounds")]
    public List<SurfaceAudio> SurfaceSounds;

    [Header("Fallbacks")]
    public AudioClip[] DefaultClips;

    public AudioClip[] GetClipsForSurface(SurfaceType type)
    {
        var match = SurfaceSounds.FirstOrDefault(x => x.Surface == type);

        if (match.Clips != null && match.Clips.Length > 0)
        {
            return match.Clips;
        }
        return DefaultClips;
    }
}