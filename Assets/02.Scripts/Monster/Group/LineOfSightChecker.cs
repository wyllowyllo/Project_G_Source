using UnityEngine;

namespace Monster.Group
{
    /// <summary>
    /// Line of Sight (시야) 체크를 담당하는 클래스.
    /// 두 지점 사이의 시야 차단 여부를 판정합니다.
    /// </summary>
    public class LineOfSightChecker
    {
        private readonly LayerMask _losBlockMask;
        private const float CheckHeight = 1.2f;

        public LineOfSightChecker(LayerMask losBlockMask)
        {
            _losBlockMask = losBlockMask;
        }

        /// <summary>
        /// 두 지점 사이에 시야가 확보되었는지 확인합니다.
        /// </summary>
        public bool HasLineOfSight(Vector3 from, Vector3 to)
        {
            Vector3 startPos = from + Vector3.up * CheckHeight;
            Vector3 endPos = to + Vector3.up * CheckHeight;

            Vector3 direction = endPos - startPos;
            float distance = direction.magnitude;

            if (distance < 0.01f)
            {
                return true;
            }

            direction /= distance;

            // Raycast로 장애물 체크
            if (Physics.Raycast(startPos, direction, out RaycastHit hit, distance, _losBlockMask, QueryTriggerInteraction.Ignore))
            {
                // 플레이어에게 맞았다면 시야 확보로 판정
                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    return true;
                }
                return false;
            }

            return true;
        }
    }
}
