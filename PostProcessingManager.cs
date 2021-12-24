using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Sneaksters.PostProcessing
{
    public class PostProcessingManager : MonoBehaviour
    {

        public List<PostProcessProfile> profiles = new List<PostProcessProfile>();

        List<PostProcessVolume> profileVolumes = new List<PostProcessVolume>();

        public int scheduledProfile1 = -1;
        public int scheduledProfile2 = -1;

        public float durationOfLerp = 0f;

        
        public float lerpTimeSoFar = 0f;

        public bool lerpingInProgress = false;

        [ContextMenu("Lerp profile")]
        void TestLerpProfile() {
            LerpProfile(0, 1, 2f);
        }

        public void LerpProfile(int profile1 = 0, int profile2 = 1, float duration = 2f) {
            lerpingInProgress = true;
            scheduledProfile1 = profile1;
            scheduledProfile2 = profile2;
            durationOfLerp = duration;
            lerpTimeSoFar = 0;
        }

        public void SetProfile(int profile) {
            for (int i = 0; i < profileVolumes.Count; i++) {
                if (i == profile) {
                    profileVolumes[i].weight = 1;
                } else {
                    profileVolumes[i].weight = 0;
                }
            }
        }

        void Start() {
            int thing = 0;
            // create a gameobject to store each post processing thing
            foreach (PostProcessProfile profile in profiles) {
                GameObject go = new GameObject();
                go.transform.SetParent(transform, false);
                go.name = profile.name + " Profile";
                go.layer = 8; // post processing layer
                PostProcessVolume vol = go.AddComponent<PostProcessVolume>();
                vol.isGlobal = true;
                vol.profile = profile;

                if (thing == 0) {vol.weight = 1;} else {vol.weight = 0;}
                thing++;
                profileVolumes.Add(vol);
            }

            SetProfile(-1);
        }

        // Update is called once per frame
        void Update()
        {
            if (lerpingInProgress) {
                lerpTimeSoFar += Time.deltaTime;

                // calculate, based on the duration and time so far, how far through the lerp we should be
                float profileTargetWeight = lerpTimeSoFar / durationOfLerp;

                // the one we're coming out of has an inverse weight
                float profileOriginalWeight = 1f - profileTargetWeight;

                profileVolumes[scheduledProfile1].weight = profileOriginalWeight;
                profileVolumes[scheduledProfile2].weight = profileTargetWeight;

                if (profileTargetWeight >= 1f) {
                    profileVolumes[scheduledProfile1].weight = 0f;
                    profileVolumes[scheduledProfile2].weight = 1f;

                    // reset all the things
                    lerpingInProgress = false;

                    scheduledProfile1 = -1;
                    scheduledProfile2 = -1;
                    durationOfLerp = 0f;
                    lerpTimeSoFar = 0f;
                    
                }
            }
        }
    }
}