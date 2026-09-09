using UnityEngine;
using Game.Combat;
using Game.Player;

namespace Game.Enemy
{
    /// <summary>적의 행동 단계. enum FSM으로 시작한다 — Behaviour Tree는 아직 필요 없다.</summary>
    public enum EnemyState { Patrol, Chase, Attack, Hurt, Dead }

    /// <summary>
    /// 근접 추격형과 원거리형을 한 스크립트가 덮는다. 갈리는 곳은 한 군데다:
    /// projectile이 비어 있으면 계속 달려들고(피해는 몸통의 ContactHitbox가 준다),
    /// 채워져 있으면 attackRange에서 멈춰 쏜다. 클래스를 둘로 나누면 감지·순찰·넉백 처리가
    /// 그대로 복사된다.
    ///
    /// 이 게임의 플레이어는 슬라이드 중 무적이라 적을 통째로 뚫고 지나갈 수 있다.
    /// 그래서 적은 "붙으면 아픈 벽"이지 "가로막는 벽"이 아니다. chaseSpeed를 플레이어
    /// maxSpeed보다 느리게 두는 이유도 같다 — 뿌리칠 수 있어야 슬라이드가 답이 된다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public class EnemyAI : MonoBehaviour
    {
        [SerializeField] private EnemyConfig config;

        [Tooltip("지형으로 취급할 레이어. 벽·낭떠러지 판정에 쓴다")]
        [SerializeField] private LayerMask groundLayer;

        [Tooltip("비우면 근접 돌격형이 된다. 채우면 원거리형")]
        [SerializeField] private GameObject projectile;

        [Tooltip("탄이 나가는 자리. 비우면 본체 중심에서 나간다")]
        [SerializeField] private Transform muzzle;

        /// <summary>지금 어느 단계인가. 애니메이션과 디버그가 읽는다.</summary>
        public EnemyState State { get; private set; } = EnemyState.Patrol;

        private Rigidbody2D _rb;
        private Collider2D _col;
        private Health _health;
        private Transform _target;
        private float _facing = -1f;
        private float _fireTimer;
        private float _hitstunTimer;   // 넉백이 멎은 뒤 남은 경직 시간

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            _health = GetComponent<Health>();
            _rb.freezeRotation = true;   // 밀리면서 굴러다니지 않게
        }

        // 이벤트 구독은 OnEnable, 해제는 OnDisable에서 짝을 맞춘다 (CLAUDE.md 규칙).
        private void OnEnable()
        {
            _health.Died += OnDied;
            _health.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            _health.Died -= OnDied;
            _health.Damaged -= OnDamaged;
        }

        // 플레이어는 Awake가 아니라 Start에서 찾는다. Awake 순서는 보장되지 않지만
        // 모든 Awake는 모든 Start보다 먼저 돈다.
        private void Start()
        {
            var player = FindAnyObjectByType<PlayerController>();
            if (player != null) _target = player.transform;
            else Debug.LogWarning($"[EnemyAI] '{name}'이 플레이어를 찾지 못했습니다. 순찰만 합니다.", this);
        }

        private void OnDied() => State = EnemyState.Dead;

        // 경직은 맞는 즉시 채우고 넉백이 멎은 뒤부터 줄인다. 그래야 "밀려나다가 멈춰서
        // 잠깐 굳는다"가 되고, 벽에 부딪혀 넉백이 일찍 끝나도 굳는 시간은 그대로다.
        private void OnDamaged(DamageInfo info) => _hitstunTimer = config.hitstun;

        // 물리는 FixedUpdate에서 처리한다 (CLAUDE.md 규칙).
        private void FixedUpdate()
        {
            if (State == EnemyState.Dead) return;

            // 맞고 밀려나는 중에는 속도를 건드리지 않는다. 건드리면 넉백이 다음 스텝에
            // 지워져 때린 티가 전혀 안 난다 — 플레이어 쪽과 같은 이유, 같은 처리.
            if (_health.IsKnockedBack)
            {
                State = EnemyState.Hurt;
                return;
            }

            // 넉백이 멎은 뒤에도 잠깐 못 움직인다. 이 틈이 밀치기 -> 슬라이드 연계의 자리다.
            // 없으면 밀려나기를 끝내는 순간 chaseSpeed로 되붙어서, 거리를 벌어도 쓸 틈이 없다.
            if (_hitstunTimer > 0f)
            {
                _hitstunTimer -= Time.fixedDeltaTime;
                State = EnemyState.Hurt;
                Move(0f);
                return;
            }

            if (State == EnemyState.Hurt) State = EnemyState.Chase;   // 맞았으면 때린 쪽을 쫓는다

            // 대상이 없으면 거리를 무한으로 봐서 자연히 순찰로 떨어진다.
            float dist = _target == null
                ? Mathf.Infinity
                : Vector2.Distance(_rb.position, (Vector2)_target.position);

            switch (State)
            {
                case EnemyState.Patrol:
                    if (dist <= config.detectRange) { State = EnemyState.Chase; break; }
                    // 벽에 막히거나 발밑이 끊기면 돌아선다. 순찰 경로를 씬에 찍지 않아도 된다.
                    if (BlockedAhead() || !GroundAhead()) _facing = -_facing;
                    Move(config.patrolSpeed * _facing);
                    break;

                case EnemyState.Chase:
                    if (dist > config.loseRange) { State = EnemyState.Patrol; break; }
                    FaceTarget();
                    if (projectile != null && dist <= config.attackRange)
                    {
                        State = EnemyState.Attack;
                        _fireTimer = 0f;   // 사거리에 들어온 즉시 한 발
                        Move(0f);
                        break;
                    }
                    // 낭떠러지 앞에서는 멈춘다. 떨어져 죽는 적은 위협이 아니라 선물이다.
                    Move(GroundAhead() ? config.chaseSpeed * _facing : 0f);
                    break;

                case EnemyState.Attack:
                    if (dist > config.attackRange) { State = EnemyState.Chase; break; }
                    FaceTarget();
                    Move(0f);
                    _fireTimer -= Time.fixedDeltaTime;
                    if (_fireTimer <= 0f)
                    {
                        Fire();
                        _fireTimer = config.fireInterval;
                    }
                    break;
            }
        }

        private void Move(float vx) => _rb.linearVelocity = new Vector2(vx, _rb.linearVelocity.y);

        private void FaceTarget()
        {
            float dx = _target.position.x - _rb.position.x;
            if (Mathf.Abs(dx) > 0.05f) _facing = Mathf.Sign(dx);
        }

        /// <summary>진행 방향 발밑에 딛을 것이 있는가.</summary>
        private bool GroundAhead()
        {
            Bounds b = _col.bounds;
            var origin = new Vector2(b.center.x + _facing * config.ledgeCheckAhead, b.min.y + 0.05f);
            return Physics2D.Raycast(origin, Vector2.down, config.ledgeCheckDown, groundLayer);
        }

        /// <summary>진행 방향이 벽으로 막혔는가.</summary>
        private bool BlockedAhead()
        {
            Bounds b = _col.bounds;
            return Physics2D.Raycast(b.center, new Vector2(_facing, 0f),
                                     b.extents.x + 0.15f, groundLayer);
        }

        private void Fire()
        {
            Vector3 pos = muzzle != null ? muzzle.position : transform.position;
            var shot = Instantiate(projectile, pos, Quaternion.identity);

            var p = shot.GetComponent<Projectile>();
            if (p != null)
                p.Launch(new Vector2(_facing * config.projectileSpeed, 0f), config.projectileLife);
        }

        // 씬 뷰에서 감지 거리와 발밑 광선을 눈으로 본다. 수치를 맞출 때 없으면 답답하다.
        private void OnDrawGizmosSelected()
        {
            if (config == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, config.detectRange);
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, config.loseRange);

            var col = GetComponent<Collider2D>();
            if (col == null) return;
            Bounds b = col.bounds;
            var origin = new Vector3(b.center.x + _facing * config.ledgeCheckAhead, b.min.y + 0.05f, 0f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + Vector3.down * config.ledgeCheckDown);
        }
    }
}
