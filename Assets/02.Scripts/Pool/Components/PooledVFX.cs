using Pool.Core;
using UnityEngine;

namespace Pool.Components
{
    // VFX 오브젝트의 자동 풀 반환을 처리
    // ParticleSystem이 없어도 동작하며, 풀링 시스템이 없으면 Destroy로 폴백
    public class PooledVFX : MonoBehaviour, IPooledObject
    {
        [Header("Duration Settings")]
        [Tooltip("체크 해제 시 파티클 시스템이 있어도 수동 기간 사용")]
        [SerializeField] private bool _useParticleDuration = true;
        [Tooltip("수동 기간 (useParticleDuration이 false일 때 적용)")]
        [SerializeField] private float _manualDuration = 2f;
        [Tooltip("재생 완료 후 풀 반환까지 대기 시간")]
        [SerializeField] private float _releaseDelay = 0.5f;

        private ParticleSystem _particleSystem;
        private float _duration;
        private float _spawnTime;
        private bool _isActive;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            CalculateDuration();
        }

        private void Start()
        {
            // 풀링 시스템이 없으면 기존 방식대로 동작
            if (ObjectPoolManager.Instance == null)
            {
                Destroy(gameObject, _duration + _releaseDelay);
                return;
            }

            _spawnTime = Time.time;
            _isActive = true;
        }

        private void Update()
        {
            if (!_isActive || ObjectPoolManager.Instance == null) return;

            if (Time.time >= _spawnTime + _duration + _releaseDelay)
            {
                _isActive = false;
                PoolSpawner.Release(gameObject);
            }
        }

        public void OnSpawnFromPool()
        {
            _spawnTime = Time.time;
            _isActive = true;

            if (_particleSystem != null)
            {
                _particleSystem.Clear(true);
                _particleSystem.Play(true);
            }
        }

        public void OnReturnToPool()
        {
            _isActive = false;

            if (_particleSystem != null)
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public void SetDuration(float duration)
        {
            _useParticleDuration = false;
            _manualDuration = duration;
            _duration = duration;
        }

        private void CalculateDuration()
        {
            if (_useParticleDuration && _particleSystem != null)
            {
                var main = _particleSystem.main;
                _duration = main.duration + main.startLifetime.constantMax;
            }
            else
            {
                _duration = _manualDuration;
            }
        }
    }
}
