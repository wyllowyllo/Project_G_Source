using UnityEngine;

namespace Common
{
    /// <summary>
    /// 플레이어 Transform 참조를 전역으로 관리하는 싱글톤.
    /// GameObject.Find 호출을 씬 로드 시 1회로 제한하여 성능을 개선합니다.
    /// </summary>
    public class PlayerReferenceProvider : MonoBehaviour
    {
        public static PlayerReferenceProvider Instance { get; private set; }

        [SerializeField] private Transform _playerTransform;

        /// <summary>
        /// 플레이어 Transform 참조
        /// </summary>
        public Transform PlayerTransform => _playerTransform;

        private void Awake()
        {
            // 싱글톤 인스턴스 설정
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("PlayerReferenceProvider: 이미 인스턴스가 존재합니다. 중복 제거합니다.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // 플레이어 자동 탐색 (씬 로드 시 한 번만)
            if (_playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    _playerTransform = player.transform;
                    Debug.Log("PlayerReferenceProvider: 플레이어를 찾았습니다.");
                }
                else
                {
                    Debug.LogWarning("PlayerReferenceProvider: Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
                }
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
