using UnityEngine;
using Game.Player;

namespace Game.World
{
    /// <summary>
    /// 과열이 쌓이는 속도를 바꾸는 지형. 기획서의 "냉각과 과열" 축이 이 값 하나다 —
    /// 물웅덩이는 0(슬라이드해도 안 달아오른다), 뜨거운 지대는 2 이상.
    ///
    /// 레벨 디자인 도구지 전투 규칙이 아니다. 여기서 속도·피해·무적을 건드리지 않는다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HeatZone : MonoBehaviour
    {
        [Tooltip("이 안에 있는 동안 과열 생성량에 곱하는 값. 0이면 안 쌓이고(냉각 지형), " +
                 "2면 두 배로 쌓인다(가열 지형)")]
        [SerializeField] private float rateMultiplier;

        // 인스펙터에서 붙이는 순간 트리거로 만들어 둔다. 안 그러면 벽이 된다.
        private void Reset() => GetComponent<Collider2D>().isTrigger = true;

        private void OnTriggerEnter2D(Collider2D other) => Apply(other, rateMultiplier);

        // ponytail: 겹친 구역을 세지 않는다. 뜨거운 지대 안에 물웅덩이를 겹쳐 두면 웅덩이를
        // 나오는 순간 배수가 2가 아니라 1로 풀린다. 겹치는 맵이 실제로 생기면 스택으로 바꿀 것.
        private void OnTriggerExit2D(Collider2D other) => Apply(other, 1f);

        private static void Apply(Collider2D other, float mul)
        {
            // 히트박스 자식은 다른 레이어라 여기 안 들어온다. 몸통 콜라이더만 온다.
            var player = other.GetComponent<PlayerController>();
            if (player != null) player.HeatRateMultiplier = mul;
        }
    }
}
