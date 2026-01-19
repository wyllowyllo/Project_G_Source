using Pool.Core;
using UnityEngine;

namespace Pool.Components
{
    [RequireComponent(typeof(ParticleSystem))]
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

        private void Update()
        {
            if (!_isActive) return;

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

            // 파티클 리셋 및 재시작
            _particleSystem.Clear(true);
            _particleSystem.Play(true);
        }

        public void OnReturnToPool()
        {
            _isActive = false;
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
