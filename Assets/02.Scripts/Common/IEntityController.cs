using UnityEngine;
using UnityEngine.AI;

namespace Common
{
    // Monster와 Boss가 공통으로 구현하는 인터페이스
    // Ability 시스템에서 재사용을 위해 사용
    public interface IEntityController
    {
        NavMeshAgent NavAgent { get; }
        Transform transform { get; }
        Transform PlayerTransform { get; }
        Animator Animator { get; }
        float RotationSpeed { get; }
    }
}
