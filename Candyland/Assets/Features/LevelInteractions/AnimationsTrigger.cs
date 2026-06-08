using System;
using System.Collections;
using UnityEngine;

public class AnimationsTrigger : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Animation[] _animations;
    
    public void Trigger(int i, bool stopAllAnim = false, AwaitableCompletionSource onAnimFinish = null)
    {
        if (_animations.Length < i)
        {
            Debug.LogError($"Animation trigger: {name} is asked to trigger animation {i} but it only has {_animations.Length} animations");
            return;
        }

        if (stopAllAnim)
        {
            foreach (var animation in _animations)
            {
                animation.Stop();
            }
        }
        
        _animations[i].Play();
        if(onAnimFinish != null)
            StartCoroutine(AnimationIsPlayingCoroutine(_animations[i], onAnimFinish));
    }


    private IEnumerator AnimationIsPlayingCoroutine(Animation animation,AwaitableCompletionSource onAnimFinish)
    {
        while (animation.isPlaying)
        {
            yield return null;
        }
        
        onAnimFinish.SetResult();
    }
}
