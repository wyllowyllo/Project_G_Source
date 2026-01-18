using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class SceneLoader : MonoBehaviour
    {
        private const float SCENE_STABILIZATION_WAIT_TIME = 0.1f;
        private const float RAYCAST_BLOCK_THRESHOLD = 0.5f;

        public static SceneLoader Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private string _loadingSceneName = "LoadingScene";
        [SerializeField] private float _fadeDuration = 0.3f;

        [Header("Fade")]
        [SerializeField] private CanvasGroup _fadeCanvasGroup;

        public static string TargetSceneName { get; private set; }
        public bool IsLoading { get; private set; }

        private bool _readyToActivate;
        public bool ReadyToActivate => _readyToActivate;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SetAlpha(0f);
            if (_fadeCanvasGroup != null)
                _fadeCanvasGroup.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 타겟 씬이 로드되면 _readyToActivate를 false로 설정
            if (_readyToActivate && scene.name == TargetSceneName)
            {
                _readyToActivate = false;
            }
        }

        public static void LoadScene(string sceneName)
        {
            if (Instance == null)
            {
                Debug.LogError("[SceneLoader] Instance not found");
                SceneManager.LoadScene(sceneName);
                return;
            }
            Instance.StartLoadScene(sceneName);
        }

        private void StartLoadScene(string sceneName)
        {
            if (IsLoading) return;
            IsLoading = true;
            TargetSceneName = sceneName;
            StartCoroutine(LoadSequence());
        }

        private IEnumerator LoadSequence()
        {
            // 1. Fade to black
            yield return StartCoroutine(FadeToBlack());

            // 2. Load loading scene (synchronous for simplicity)
            SceneManager.LoadScene(_loadingSceneName);

            // 3. Wait a frame for scene to initialize
            yield return null;
            yield return null;

            // 4. Fade from black (show loading screen)
            yield return StartCoroutine(FadeFromBlack());
        }

        public void OnTargetSceneReady()
        {
            if (!IsLoading) return;
            StartCoroutine(TransitionToTarget());
        }

        private IEnumerator TransitionToTarget()
        {
            // 1. Fade to black
            yield return StartCoroutine(FadeToBlack());

            // 2. Signal that scene can activate
            _readyToActivate = true;

            // 3. Wait for scene to actually load (OnSceneLoaded will be called)
            while (_readyToActivate)
                yield return null;

            // 4. Wait frames for scene to stabilize
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(SCENE_STABILIZATION_WAIT_TIME);

            // 5. Fade from black
            yield return StartCoroutine(FadeFromBlack());

            IsLoading = false;
        }

        private IEnumerator FadeToBlack()
        {
            if (_fadeCanvasGroup == null) yield break;

            _fadeCanvasGroup.gameObject.SetActive(true);
            _fadeCanvasGroup.blocksRaycasts = true;
            float startAlpha = _fadeCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / _fadeDuration);
                yield return null;
            }

            SetAlpha(1f);
        }

        private IEnumerator FadeFromBlack()
        {
            if (_fadeCanvasGroup == null) yield break;

            float startAlpha = _fadeCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / _fadeDuration);
                yield return null;
            }

            SetAlpha(0f);
            _fadeCanvasGroup.gameObject.SetActive(false);
        }

        private void SetAlpha(float alpha)
        {
            if (_fadeCanvasGroup == null) return;
            _fadeCanvasGroup.alpha = alpha;
            _fadeCanvasGroup.blocksRaycasts = alpha > RAYCAST_BLOCK_THRESHOLD;
        }
    }
}
