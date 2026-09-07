using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// 적 한 종류의 튜닝 수치. 체력은 여기 없다 — Health 컴포넌트가 이미 갖고 있고,
    /// 두 군데에 두면 어느 쪽이 진짜인지 매번 확인해야 한다.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Game/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("이동")]
        [Tooltip("플레이어를 못 봤을 때 순찰 속도 (units/sec). 0이면 제자리를 지킨다")]
        public float patrolSpeed = 2f;

        [Tooltip("추격 속도 (units/sec). 플레이어 maxSpeed 9보다 느려야 슬라이드로 뿌리칠 수 있다")]
        public float chaseSpeed = 5f;

        [Header("감지")]
        [Tooltip("이 거리 안에 플레이어가 들어오면 추격을 시작한다")]
        public float detectRange = 8f;

        [Tooltip("이 거리를 넘으면 추격을 포기한다. detectRange보다 커야 한다 — " +
                 "같으면 경계선에서 추격/순찰이 매 프레임 번갈아 바뀌며 덜덜 떤다")]
        public float loseRange = 12f;

        [Header("원거리 — EnemyAI의 projectile을 비우면 전부 무시된다")]
        [Tooltip("이 거리 안으로 들어오면 멈춰서 쏜다")]
        public float attackRange = 6f;

        [Tooltip("발사 간격 (sec)")]
        public float fireInterval = 1.5f;

        [Tooltip("탄 속도 (units/sec)")]
        public float projectileSpeed = 9f;

        [Tooltip("탄이 스스로 사라지기까지의 시간 (sec). 지형에 막히지 않아도 이 시간이면 사라진다")]
        public float projectileLife = 3f;

        [Header("지형 감지")]
        [Tooltip("발밑 낭떠러지를 확인할 때 앞으로 내다보는 거리")]
        public float ledgeCheckAhead = 0.7f;

        [Tooltip("낭떠러지 확인 광선의 길이. 이보다 깊으면 낭떠러지로 본다")]
        public float ledgeCheckDown = 1.2f;
    }
}
