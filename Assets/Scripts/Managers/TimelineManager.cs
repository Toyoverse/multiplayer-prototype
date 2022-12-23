using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Managers
{
    public class TimelineManager : MonoBehaviour
    {
        [SerializeField] private PlayableDirector toyoDirector;
        [SerializeField] private PlayableDirector cardsDirector;
        [SerializeField] private List<AnimationSequence> animations;
        private ScriptsReferences refs => ScriptsReferences.Instance;

        #region Public methods

        public void PlayToyoAnimation(PLAYABLE_TYPE type)
        {
            toyoDirector.playableAsset = GetAnimationFromType(type);
            if(toyoDirector.playableAsset != null)
                toyoDirector.Play();
        }
        
        public void PlayCardsAnimation()
        {
            if(cardsDirector.playableAsset != null)
                cardsDirector.Play();
        }
    
        #endregion
    
        #region Private methods

        private void Start()
        {
            toyoDirector.stopped += OnToyoDirectorStopped;
            cardsDirector.stopped += OnCardsDirectorStopped;
        }

        private void OnDisable()
        {
            toyoDirector.stopped -= OnToyoDirectorStopped;
            cardsDirector.stopped -= OnCardsDirectorStopped;
        }

        private void OnToyoDirectorStopped(PlayableDirector playableDirector)
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
        
        private void OnCardsDirectorStopped(PlayableDirector playableDirector)
            => refs.handManager.EnableCardsAmountUI();
        
    
        #endregion
    }

    [Serializable]
    public class AnimationSequence
    {
        public PLAYABLE_TYPE type;
        public PlayableAsset playable;
    }
}