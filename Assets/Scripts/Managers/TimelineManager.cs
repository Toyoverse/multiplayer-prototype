using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Managers
{
    public class TimelineManager : MonoBehaviour
    {
        [SerializeField] private PlayableDirector director;
        [SerializeField] private List<AnimationSequence> animations;
        private ScriptsReferences refs => ScriptsReferences.Instance;

        #region Public methods

        public void PlayAnimation(PLAYABLE_TYPE type)
        {
            director.playableAsset = GetAnimationFromType(type);
            if(director.playableAsset != null)
                director.Play();
        }
    
        #endregion
    
        #region Private methods

        private void Start() => director.stopped += OnStopped;

        private void OnDisable() => director.stopped -= OnStopped;

        private void OnStopped(PlayableDirector playableDirector)
        {
            if(refs.localManager.myMatchResult is SIMPLE_RESULT.NONE)
                refs.localManager.OnEndRoundAnimation();
            else 
                refs.localManager.OnEndMatchAnimation();
        }

        private PlayableAsset GetAnimationFromType(PLAYABLE_TYPE type)
        {
            foreach (var anim in animations)
            {
                if (anim.type == type)
                    return anim.playable;
            }
            return null;
        }
    
        #endregion
    }

    [Serializable]
    public class AnimationSequence
    {
        public PLAYABLE_TYPE type;
        public PlayableAsset playable;
    }
}