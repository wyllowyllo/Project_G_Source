using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class LoadingSceneController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Slider _progressBar;

        [Header("Settings")]
        [SerializeField] private float _minimumDisplayTime = 1f;

        private AsyncOperation _asyncLoad;

        private void Start()
        {
            if (string.IsNullOrEmpty(SceneLoader.TargetSceneName))
            {
                Debug.LogError("[LoadingScene] No target scene specified");
                return;
            }

            StartCoroutine(LoadTargetScene());
        }

        private IEnumerator LoadTargetScene()
        {
            float elapsedTime = 0f;

            // Start async load but don't activate yet
            _asyncLoad = SceneManager.LoadSceneAsync(SceneLoader.TargetSceneName);
            _asyncLoad.allowSceneActivation = false;

            // Wait for load to complete AND minimum time
            while (_asyncLoad.progress < 0.9f || elapsedTime < _minimumDisplayTime)
            {
                elapsedTime += Time.deltaTime;

                float loadProgress = Mathf.Clamp01(_asyncLoad.progress / 0.9f);
                float timeProgress = Mathf.Clamp01(elapsedTime / _minimumDisplayTime);

                if (_progressBar != null)
                    _progressBar.value = Mathf.Min(loadProgress, timeProgress);

                yield return null;
            }

            // Tell SceneLoader we're ready
            if (SceneLoader.Instance == null)
            {
                Debug.LogError("[LoadingScene] SceneLoader.Instance not found");
                _asyncLoad.allowSceneActivation = true;
                yield break;
            }

            SceneLoader.Instance.OnTargetSceneReady();

            // Wait for SceneLoader to fade to black and signal ready
            while (!SceneLoader.Instance.ReadyToActivate)
                yield return null;

            // Now activate the scene
            // Note: LoadingScene will be unloaded after this, so no more code will execute
            _asyncLoad.allowSceneActivation = true;
        }
    }
}
