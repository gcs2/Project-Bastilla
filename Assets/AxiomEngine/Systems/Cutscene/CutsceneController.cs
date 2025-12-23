// ============================================================================
// RPGPlatform.Systems.Cutscene - Cutscene Controller
// Manages playback of hybrid cutscenes (FMV vs Real-Time)
// ============================================================================

using System;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Playables;

namespace RPGPlatform.Systems.Cutscene
{
    public enum CutsceneType
    {
        Timeline,
        Video
    }

    public class CutsceneController : MonoBehaviour
    {
        public static CutsceneController Instance { get; private set; }

        // Events for UI/View layers to handle actual rendering
        public event Action<VideoClip> OnPlayVideo;
        public event Action<PlayableAsset> OnPlayTimeline;
        public event Action OnCutsceneFinished;

        // For Testing
        public string LastPlayedType { get; private set; }
        public string LastPlayedName { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void PlayVideo(VideoClip clip)
        {
            if (clip == null) return;
            Debug.Log($"[CutsceneController] Playing Video: {clip.name}");
            LastPlayedType = "Video";
            LastPlayedName = clip.name;
            OnPlayVideo?.Invoke(clip);
            
            // In a real implementation, we'd wait for video end event.
            // For now, we assume immediate completion or handle it elsewhere.
        }

        public void PlayTimeline(PlayableAsset timeline)
        {
            if (timeline == null) return;
            Debug.Log($"[CutsceneController] Playing Timeline: {timeline.name}");
            LastPlayedType = "Timeline";
            LastPlayedName = timeline.name;
            OnPlayTimeline?.Invoke(timeline);
        }

        public void FinishCutscene()
        {
            Debug.Log("[CutsceneController] Cutscene Finished");
            OnCutsceneFinished?.Invoke();
        }
    }
}
