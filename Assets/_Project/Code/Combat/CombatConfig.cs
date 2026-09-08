using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 맞는 쪽의 튜닝 수치. Health가 참조한다.
    /// 플레이어와 적이 서로 다른 무적 시간을 갖게 하려고 에셋으로 분리했다 —
    /// 보통 플레이어는 길게(살아남을 여지), 적은 짧게(연타가 들어가게) 준다.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatConfig", menuName = "Game/Combat Config")]
    public class CombatConfig : ScriptableObject
    {
        [Header("무적 시간")]
        [Tooltip("피격 후 다시 맞지 않는 시간 (sec). 0이면 히트박스가 겹친 동안 계속 맞는다")]
        public float invincibleDuration = 0.5f;

        [Tooltip("무적 동안 깜빡이는 주기 (sec). 작을수록 빠르게 명멸한다")]
        public float blinkInterval = 0.05f;

        [Header("넉백")]
        [Tooltip("넉백으로 받은 수평 속도가 0까지 깎이는 감속도 (units/sec²). " +
                 "이게 없으면 맞은 대상이 받은 속도를 그대로 안고 화면 밖까지 날아간다 — " +
                 "속도를 대입만 하고 아무도 줄이지 않기 때문. " +
                 "밀려나는 거리 = knockbackSpeed² / (2 x 이 값)")]
        public float knockbackDecay = 40f;
    }
}
