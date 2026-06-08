using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string _transitionSceneName = "TransitionScene";

    private AwaitableCompletionSource _animationAwaitableCompletionSource;
    
    public async void LoadScene(int index)
    {
        var lastScene = SceneManager.GetActiveScene();
        await SceneTransitionAsync(lastScene,SceneManager.GetSceneByBuildIndex(index).name);
    }

    public async void LoadScene(string name)
    {
        var lastScene = SceneManager.GetActiveScene();
        await SceneTransitionAsync(lastScene, name);
    }


    private async Task SceneTransitionAsync(Scene unloadScene, string sceneName)
    {
            await SceneManager.LoadSceneAsync(_transitionSceneName, LoadSceneMode.Additive);

            var transitionScene = SceneManager.GetSceneByName(_transitionSceneName);
            SceneManager.SetActiveScene(transitionScene);

            _animationAwaitableCompletionSource = new AwaitableCompletionSource();
            FindAnyObjectByType<AnimationsTrigger>(FindObjectsInactive.Exclude).Trigger(0, true, _animationAwaitableCompletionSource);
            await _animationAwaitableCompletionSource.Awaitable;

            await SceneManager.UnloadSceneAsync(unloadScene);
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            _animationAwaitableCompletionSource = new AwaitableCompletionSource();
            FindAnyObjectByType<AnimationsTrigger>(FindObjectsInactive.Exclude).Trigger(1, true, _animationAwaitableCompletionSource);
            await _animationAwaitableCompletionSource.Awaitable;
            
            await SceneManager.UnloadSceneAsync(_transitionSceneName);
    }
}
