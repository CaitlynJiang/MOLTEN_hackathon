using UnityEngine;

namespace Reefkeeper
{
    [DefaultExecutionOrder(210)]
    public sealed class ReefBrushAudio : MonoBehaviour
    {
        public ReefBrushPickup pickup;
        public AudioClip scrubClip;
        [Range(0, 1)] public float volume = 0.35f;
        public AudioSource Source { get; private set; }
        float activeUntil = -1;

        void Awake()
        {
            Source = gameObject.AddComponent<AudioSource>();
            Source.clip = scrubClip; Source.loop = true; Source.playOnAwake = false;
            Source.volume = volume; Source.priority = 64; Source.spatialBlend = 0.6f;
            Source.rolloffMode = AudioRolloffMode.Linear; Source.minDistance = 0.3f;
            Source.maxDistance = 3; Source.dopplerLevel = 0;
        }
        void LateUpdate()
        {
            var brush = pickup.workshop.brush;
            if (brush.Scrubbing) activeUntil = Time.unscaledTime + 0.08f;
            // Bridge tiny gaps at a stroke's turning point, but stop on release, lost contact or completion.
            bool active = brush.Held && brush.InContact && Time.unscaledTime < activeUntil
                && !pickup.practiceSpot.Complete && (!pickup.Carrying || pickup.IsScrubbing);
            Source.volume = volume;
            if (active && scrubClip) { if (!Source.isPlaying) Source.Play(); }
            else if (Source.isPlaying) Source.Stop();
        }
        void OnDisable() { activeUntil = -1; if (Source) Source.Stop(); }
    }
}
