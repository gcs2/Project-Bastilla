// ============================================================================
// SunEater.Cutscenes - Arrival Logic
// Observes TravelManager to trigger context-sensitive cutscenes (Timeline vs FMV)
// ============================================================================

using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Playables;
using RPGPlatform.Core;
using RPGPlatform.Systems.Travel;
using RPGPlatform.Systems.Cutscene;
using RPGPlatform.Data;

namespace SunEater.Cutscenes
{
    public class SunEaterCutsceneLogic : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GenreProfile _genreProfile;

        [Header("Vorgossos Assets")]
        [SerializeField] private VideoClip _motherOfMonstersFMV;
        [SerializeField] private PlayableAsset _standardLandingTimeline;

        // Dependencies
        private TravelManager _travelManager;
        private IMoralityService _moralityService;
        private CutsceneController _cutsceneController;

        public void Initialize(TravelManager travel, IMoralityService morality, CutsceneController cutscenes)
        {
            _travelManager = travel;
            _moralityService = morality;
            _cutsceneController = cutscenes;

            if (_travelManager != null)
                _travelManager.OnTravelCompleted += HandleArrival;
        }

        private void OnDestroy()
        {
            if (_travelManager != null)
                _travelManager.OnTravelCompleted -= HandleArrival;
        }

        private void HandleArrival(LocationData location)
        {
            if (location.LocationId.ToLower() == "vorgossos")
            {
                CheckVorgossosArrival();
            }
            else
            {
                // Generic arrival logic could use GenreProfile defaults
                if (_genreProfile != null && _genreProfile.DefaultLandingTimeline != null)
                {
                    _cutsceneController.PlayTimeline(_genreProfile.DefaultLandingTimeline);
                }
            }
        }

        private void CheckVorgossosArrival()
        {
            // Requirement: "If PlayerMorality < -50, trigger Runway-generated FMV"
            // "Mother of Monsters" court logic
            // Assuming "humanism" axis where negative is Transhumanist/Cruel
            
            float humanity = _moralityService.GetAxisValue("humanism");
            Debug.Log($"[SunEaterLogic] Arrived at Vorgossos. Humanity: {humanity}");

            if (humanity < -50)
            {
                Debug.Log("[SunEaterLogic] Triggering Dark Path FMV: Mother of Monsters");
                if (_motherOfMonstersFMV != null)
                    _cutsceneController.PlayVideo(_motherOfMonstersFMV);
            }
            else
            {
                Debug.Log("[SunEaterLogic] Triggering Standard Landing Timeline");
                if (_standardLandingTimeline != null)
                    _cutsceneController.PlayTimeline(_standardLandingTimeline);
            }
        }
    }
}
