using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 공격 하나의 튜닝 수치. 때리는 쪽이 참조한다.
    /// 플레이어의 평타, 적의 할퀴기, 보스의 내려찍기가 각각 이 에셋을 하나씩 갖는다.
    ///
    /// 히트스톱과 화면 흔들림을 여기 둔 이유: 무거운 공격일수록 더 오래 멈추고 더 흔들려야
    /// 무겁게 느껴진다. 전역 상수로 두면 모든 공격의 무게가 똑같아진다.
    /// </summary>
    [CreateAssetMenu(fileName = "AttackConfig", menuName = "Game/Attack Config")]
    public class AttackConfig : ScriptableObject
    {
        [Header("피해")]
        [Tooltip("깎을 체력")]
        public float damage = 1f;

        [Tooltip("때린 쪽의 수평 속도에 비례해 피해를 조절하는 기준 속도 (units/sec). " +
                 "0이면 속도와 무관하게 damage 그대로 (기본). " +
                 "실제 피해 = damage x (때린 쪽 |vx| / 이 값) — 이 속도에서 damage가 그대로 들어가고 " +
                 "느리면 덜, 빠르면 더 아프다. 슬라이드에 dashSpeed를 넣으면 " +
                 "'진입 속도 = 표기 피해', 경사로 더 붙인 속도가 그대로 화력이 된다")]
        public float speedDamageReference = 0f;

        [Tooltip("속도 비례 곡선의 지수. 1이면 속도에 정비례해서 " +
                 "피해 차이가 속도 차이를 절대 못 넘는다 — 50과 31로 때려도 1.6배가 천장이다. " +
                 "2면 제곱이라 같은 속도 폭에서 2.6배까지 벌어지고, 기준 속도 아래는 " +
                 "빠르게 무력해진다(속도를 잃으면 위험하다). 0이면 속도와 무관하게 damage 고정")]
        public float speedDamageExponent = 1f;

        [Tooltip("맞은 쪽이 뒤로 밀리는 속도 (units/sec)")]
        public float knockbackSpeed = 10f;

        [Tooltip("넉백에 섞는 위쪽 속도. 살짝 띄워야 맞은 티가 난다. 0이면 수평으로만 밀린다")]
        public float knockbackLift = 3f;

        [Header("타이밍 — 이 셋이 공격의 리듬을 만든다")]
        [Tooltip("입력 후 히트박스가 켜지기까지의 예비 동작 시간 (sec). " +
                 "0이면 즉발이라 가볍고, 길수록 묵직하지만 답답해진다")]
        public float windup = 0.05f;

        [Tooltip("히트박스가 켜져 있는 시간 (sec). 이 동안 닿은 대상만 맞는다")]
        public float active = 0.08f;

        [Tooltip("히트박스가 꺼진 뒤 다시 공격할 수 없는 시간 (sec)")]
        public float recovery = 0.22f;

        [Header("타격감")]
        [Tooltip("명중 순간 게임 전체가 멈추는 시간 (sec). " +
                 "0.05 근처가 기본. 0.15을 넘으면 끊긴다는 느낌이 든다")]
        public float hitstop = 0.06f;

        [Tooltip("명중 시 화면 흔들림 세기. 0이면 안 흔들린다")]
        public float shakeForce = 0.35f;

        /// <summary>입력부터 다시 공격할 수 있을 때까지의 전체 길이.</summary>
        public float TotalDuration => windup + active + recovery;
    }
}
