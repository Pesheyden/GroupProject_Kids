using System;
using System.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string _transitionSceneName = "TransitionScene";
    [SerializeField] private TwoPlayerStartButton playersRequired;
    [SerializeField] private GameObject startButton;

    private AwaitableCompletionSource _animationAwaitableCompletionSource;

    public async void LoadScene(int index)
    {
        if(playersRequired.AllConnected == false)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(startButton);

            return;
        }

        var lastScene = SceneManager.GetActiveScene();
        await SceneTransitionAsync(lastScene,index);
    }
    
    private async Task SceneTransitionAsync(Scene unloadScene, int sceneIndex)
    {
            await SceneManager.LoadSceneAsync(_transitionSceneName, LoadSceneMode.Additive);

            var transitionScene = SceneManager.GetSceneByName(_transitionSceneName);
            SceneManager.SetActiveScene(transitionScene);

            _animationAwaitableCompletionSource = new AwaitableCompletionSource();
            FindAnyObjectByType<AnimationsTrigger>(FindObjectsInactive.Exclude).Trigger(0, true, _animationAwaitableCompletionSource);
            await _animationAwaitableCompletionSource.Awaitable;

            await SceneManager.UnloadSceneAsync(unloadScene);
            Debug.Log(sceneIndex);
            await SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);

            _animationAwaitableCompletionSource = new AwaitableCompletionSource();
            FindAnyObjectByType<AnimationsTrigger>(FindObjectsInactive.Exclude).Trigger(1, true, _animationAwaitableCompletionSource);
            await _animationAwaitableCompletionSource.Awaitable;
            
            await SceneManager.UnloadSceneAsync(_transitionSceneName);
    }
}
