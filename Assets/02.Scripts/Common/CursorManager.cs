using UnityEngine;

namespace Common
{
    public class CursorManager : MonoBehaviour
    {
        public static CursorManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool _lockOnStart = true;

        public bool IsLocked => Cursor.lockState == CursorLockMode.Locked;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("CursorManager: 이미 인스턴스가 존재합니다. 중복 제거합니다.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (_lockOnStart)
            {
                LockCursor();
            }
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleCursor();
            }

            if (Input.GetMouseButtonDown(1) && !IsLocked)
            {
                LockCursor();
            }
        }

        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ToggleCursor()
        {
            if (IsLocked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
