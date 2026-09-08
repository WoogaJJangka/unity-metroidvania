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
