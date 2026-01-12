using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// 월드 스페이스에 표시되는 데미지 숫자 UI.
    /// 위로 떠오르며 페이드 아웃됩니다.
    /// </summary>
    public class DamageNumberUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshPro _textMesh;

        [Header("Animation")]
        [SerializeField] private float _floatHeight = 1.5f;
        [SerializeField] private float _duration = 1f;
        [SerializeField] private Ease _moveEase = Ease.OutCubic;
        [SerializeField] private Ease _fadeEase = Ease.InQuad;

        [Header("Appearance")]
        [SerializeField] private float _normalFontSize = 3f;
        [SerializeField] private float _criticalFontSize = 4.5f;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _criticalColor = new Color(1f, 0.3f, 0.3f);

        [Header("Random Offset")]
        [SerializeField] private float _horizontalRandomOffset = 0.5f;

        private Sequence _animSequence;

        public void Initialize(float damage, bool isCritical)
        {
            if (_textMesh == null)
            {
                _textMesh = GetComponent<TextMeshPro>();
            }

            if (_textMesh == null)
            {
                Debug.LogWarning("[DamageNumberUI] TextMeshPro 컴포넌트를 찾을 수 없습니다.");
                Destroy(gameObject);
                return;
            }

            // 텍스트 설정
            _textMesh.text = Mathf.RoundToInt(damage).ToString();
            _textMesh.fontSize = isCritical ? _criticalFontSize : _normalFontSize;
            _textMesh.color = isCritical ? _criticalColor : _normalColor;

            // 랜덤 오프셋
            float randomX = Random.Range(-_horizontalRandomOffset, _horizontalRandomOffset);
            transform.position += new Vector3(randomX, 0, 0);

            PlayAnimation(isCritical);
        }

        private void PlayAnimation(bool isCritical)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + Vector3.up * _floatHeight;

            _animSequence = DOTween.Sequence();

            // 위로 이동
            _animSequence.Append(transform.DOMove(endPos, _duration).SetEase(_moveEase));

            // 페이드 아웃
            //_animSequence.Join(_textMesh.DOFade(0f, _duration).SetEase(_fadeEase));

            // 크리티컬이면 스케일 펀치
            if (isCritical)
            {
                _animSequence.Join(transform.DOPunchScale(Vector3.one * 0.3f, 0.2f, 1));
            }

            _animSequence.OnComplete(() => Destroy(gameObject));
        }

        private void LateUpdate()
        {
            // 항상 카메라를 향하도록 (빌보드)
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }

        private void OnDestroy()
        {
            _animSequence?.Kill();
        }
    }
}
