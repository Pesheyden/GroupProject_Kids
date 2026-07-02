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
        if(playersRequired && playersRequired.AllConnected == false)
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
            FindInActiveScene<AnimationsTrigger>().Trigger(0, true, _animationAwaitableCompletionSource);
            await _animationAwaitableCompletionSource.Awaitable;

            await SceneManager.UnloadSceneAsync(unloadScene);
            Debug.Log(sceneIndex);
            await SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);

            _animationAwaitableCompletionSource = new AwaitableCompletionSource();
            Debug.Log(FindInActiveScene<AnimationsTrigger>());
            FindInActiveScene<AnimationsTrigger>().Trigger(1, true, _animationAwaitableCompletionSource);

            await _animationAwaitableCompletionSource.Awaitable;
            
            await SceneManager.UnloadSceneAsync(_transitionSceneName);
    }
    
    public static T FindInActiveScene<T>() where T : Component
    {
        Scene activeScene = SceneManager.GetActiveScene();
        foreach (var root in activeScene.GetRootGameObjects())
        {
            T found = root.GetComponentInChildren<T>(true);
            if (found != null)
                return found;
        }

        return null;
    }
}
