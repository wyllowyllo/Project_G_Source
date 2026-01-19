using UnityEngine;

namespace Skill
{
    public class VFXAutoDestroy : MonoBehaviour
    {
        [SerializeField] private bool _useParticleDuration = true;
        [SerializeField] private float _manualDuration = 2f;
        [SerializeField] private float _destroyDelay = 0.5f;

        private void Start()
        {
            float duration = _manualDuration;

            if (_useParticleDuration)
            {
                var ps = GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    duration = ps.main.duration + ps.main.startLifetime.constantMax;
                }
            }

            Destroy(gameObject, duration + _destroyDelay);
        }
    }
}
