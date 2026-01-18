using System;
using System.Collections.Generic;
using UnityEngine;

namespace Boss.Core
{
    // 보스 공격 예고 시스템
    // 공격 전 플레이어에게 시각적 힌트 제공
    public class BossTelegraph : MonoBehaviour
    {
        [Header("데칼 프리팹")]
        [SerializeField] private GameObject _circleDecalPrefab;
        [SerializeField] private GameObject _coneDecalPrefab;
        [SerializeField] private GameObject _lineDecalPrefab;
        [SerializeField] private GameObject _markerPrefab;

        [Header("경고 이펙트")]
        [SerializeField] private GameObject _warningEffectPrefab;
        [SerializeField] private Material _glowMaterial;

        private GameObject _activeDecal;
        private GameObject _activeEffect;
        private List<GameObject> _activeMarkers = new();

        public event Action OnTelegraphStart;
        public event Action OnTelegraphEnd;

        // 원형 범위 표시 (근접 공격, 소환 위치)
        public void ShowCircle(Vector3 center, float radius, float duration)
        {
            HideAll();

            if (_circleDecalPrefab != null)
            {
                _activeDecal = Instantiate(_circleDecalPrefab, center, Quaternion.identity);
                _activeDecal.transform.localScale = new Vector3(radius * 2f, 1f, radius * 2f);
            }

            OnTelegraphStart?.Invoke();

            if (duration > 0f)
            {
                Invoke(nameof(HideAll), duration);
            }
        }

        // 부채꼴 범위 표시 (브레스 공격)
        public void ShowCone(Vector3 origin, Vector3 direction, float angle, float range, float duration)
        {
            HideAll();

            if (_coneDecalPrefab != null)
            {
                Quaternion rotation = Quaternion.LookRotation(direction);
                _activeDecal = Instantiate(_coneDecalPrefab, origin, rotation);

                // 각도와 범위에 따라 스케일 조정
                float widthScale = range * Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad) * 2f;
                _activeDecal.transform.localScale = new Vector3(widthScale, 1f, range);
            }

            OnTelegraphStart?.Invoke();

            if (duration > 0f)
            {
                Invoke(nameof(HideAll), duration);
            }
        }

        // 직선 범위 표시 (돌진 공격, 투사체 궤적)
        public void ShowLine(Vector3 start, Vector3 end, float width, float duration)
        {
            HideAll();

            if (_lineDecalPrefab != null)
            {
                Vector3 direction = end - start;
                float length = direction.magnitude;
                Vector3 center = (start + end) * 0.5f;
                Quaternion rotation = Quaternion.LookRotation(direction);

                _activeDecal = Instantiate(_lineDecalPrefab, center, rotation);
                _activeDecal.transform.localScale = new Vector3(width, 1f, length);
            }

            OnTelegraphStart?.Invoke();

            if (duration > 0f)
            {
                Invoke(nameof(HideAll), duration);
            }
        }

        // 위치 마커 표시 (소환 위치, 착탄 예정 지점)
        public void ShowMarker(Vector3 position, float duration)
        {
            if (_markerPrefab != null)
            {
                GameObject marker = Instantiate(_markerPrefab, position, Quaternion.identity);
                _activeMarkers.Add(marker);

                if (duration > 0f)
                {
                    Destroy(marker, duration);
                }
            }
        }

        // 다중 마커 표시 (여러 소환 위치)
        public void ShowMarkers(Vector3[] positions, float duration)
        {
            foreach (var position in positions)
            {
                ShowMarker(position, duration);
            }

            OnTelegraphStart?.Invoke();
        }

        #region 공격 패턴별 헬퍼 메서드

        // 근접 공격 예고
        public void ShowMeleeWarning(float range)
        {
            ShowCircle(transform.position, range, 0f);
        }

        // 돌진 공격 예고
        public void ShowChargeWarning(Vector3 direction, float distance)
        {
            Vector3 start = transform.position;
            Vector3 end = start + direction * distance;
            ShowLine(start, end, 2f, 0f);
        }

        // 브레스 공격 예고
        public void ShowBreathWarning(float angle, float range)
        {
            ShowCone(transform.position, transform.forward, angle, range, 0f);
        }

        // 투사체 조준 예고
        public void ShowProjectileWarning(Vector3 targetPosition)
        {
            ShowLine(transform.position, targetPosition, 0.5f, 0f);
        }

        // 투사체 조준 업데이트
        public void UpdateProjectileWarning(Vector3 targetPosition)
        {
            if (_activeDecal != null)
            {
                Vector3 start = transform.position;
                Vector3 direction = targetPosition - start;
                float length = direction.magnitude;
                Vector3 center = (start + targetPosition) * 0.5f;

                _activeDecal.transform.position = center;
                _activeDecal.transform.rotation = Quaternion.LookRotation(direction);
                _activeDecal.transform.localScale = new Vector3(0.5f, 1f, length);
            }
        }

        // 소환 위치 예고
        public void ShowSummonWarning(Vector3[] positions)
        {
            ShowMarkers(positions, 0f);
        }

        #endregion

        // 경고 이펙트 표시 (보스 몸체 발광)
        public void ShowWarningEffect(Transform target, float duration)
        {
            HideEffect();

            if (_warningEffectPrefab != null && target != null)
            {
                _activeEffect = Instantiate(_warningEffectPrefab, target.position, Quaternion.identity, target);
            }

            if (duration > 0f)
            {
                Invoke(nameof(HideEffect), duration);
            }
        }

        // 모든 텔레그래프 숨기기
        public void HideAll()
        {
            CancelInvoke(nameof(HideAll));

            if (_activeDecal != null)
            {
                Destroy(_activeDecal);
                _activeDecal = null;
            }

            // 마커 정리
            foreach (var marker in _activeMarkers)
            {
                if (marker != null)
                {
                    Destroy(marker);
                }
            }
            _activeMarkers.Clear();

            HideEffect();
            OnTelegraphEnd?.Invoke();
        }

        // 이펙트만 숨기기
        public void HideEffect()
        {
            CancelInvoke(nameof(HideEffect));

            if (_activeEffect != null)
            {
                Destroy(_activeEffect);
                _activeEffect = null;
            }
        }

        private void OnDestroy()
        {
            HideAll();
        }
    }
}
