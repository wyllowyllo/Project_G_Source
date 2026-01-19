using Pool.Core;
using UnityEngine;

namespace Pool.Components
{
    // VFX 오브젝트의 자동 풀 반환을 처리
    // ParticleSystem이 없어도 동작하며, 풀링 시스템이 없으면 Destroy로 폴백
    public class PooledVFX : MonoBehaviour, IPooledObject
    {
        [SerializeField] private bool _useParticleDuration = true;
        [SerializeField] private float _manualDuration = 2f;
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
