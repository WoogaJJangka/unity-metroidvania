using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    /// <summary>
    /// 2D 플랫포머 플레이어 이동.
    /// Unity 기본 중력을 끄고 중력을 직접 계산한다. 그래야 "상승보다 하강이 빠르게",
    /// "최고점에서 잠깐 체공" 같은 조작감 보정을 넣을 수 있다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private MovementConfig config;
        [SerializeField] private InputActionAsset inputActions;

        [Tooltip("지형으로 취급할 레이어. Ground 레이어를 체크한다")]
        [SerializeField] private LayerMask groundLayer;

        // C#에서 private 필드는 관례상 _로 시작한다 (CLAUDE.md 규칙).
        private Rigidbody2D _rb;
        private CapsuleCollider2D _collider;
        private InputActionMap _playerMap;
        private InputAction _moveAction;
        private InputAction _jumpAction;

        private float _moveInput;
        private bool _isGrounded;
        private float _coyoteTimer;      // 발판을 떠난 뒤 남은 점프 허용 시간
        private float _jumpBufferTimer;  // 미리 누른 점프가 유효한 남은 시간
        private bool _isJumping;         // 이번 점프의 상승 구간이 진행 중인가
        private bool _jumpCutApplied;    // 이번 점프에서 이미 높이를 깎았는가

        /// <summary>다른 시스템(애니메이션 등)이 상태를 읽기 위한 통로.</summary>
        public bool IsGrounded => _isGrounded;
        public Vector2 Velocity => _rb.linearVelocity;

        private void Awake()
        {
            // GetComponent는 비싸므로 Awake에서 한 번만 캐싱한다 (CLAUDE.md 규칙).
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CapsuleCollider2D>();

            _rb.gravityScale = 0f;                 // 중력은 우리가 직접 계산한다
            _rb.freezeRotation = true;             // 캐릭터가 굴러다니지 않게
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // 두 번째 인자 true = 못 찾으면 예외를 던진다. 오타를 조용히 넘기지 않기 위함.
            _playerMap = inputActions.FindActionMap("Player", true);
            _moveAction = _playerMap.FindAction("Move", true);
            _jumpAction = _playerMap.FindAction("Jump", true);
        }

        private void OnEnable() => _playerMap.Enable();
        private void OnDisable() => _playerMap.Disable();

        // 입력은 Update에서 읽는다. FixedUpdate는 프레임마다 돌지 않아 입력을 놓칠 수 있다.
        private void Update()
        {
            _moveInput = _moveAction.ReadValue<Vector2>().x;

            // 점프 버퍼: 누른 순간 타이머를 채우고 매 프레임 줄인다.
            // 착지 직전에 눌러도 착지하는 순간 점프가 나가서 "씹혔다"는 느낌이 사라진다.
            if (_jumpAction.WasPressedThisFrame())
                _jumpBufferTimer = config.jumpBufferTime;
            else
                _jumpBufferTimer -= Time.deltaTime;

            // 가변 점프: 상승 중에 버튼을 떼면 위로 가던 속도를 깎는다.
            // 짧게 누르면 낮게, 길게 누르면 높게 뛰는 감각이 여기서 나온다.
            if (_jumpAction.WasReleasedThisFrame() && _isJumping && !_jumpCutApplied
                && _rb.linearVelocity.y > 0f)
            {
                _rb.linearVelocity = new Vector2(
                    _rb.linearVelocity.x,
                    _rb.linearVelocity.y * config.jumpCutMultiplier);
                _jumpCutApplied = true;
            }
        }

        // 물리는 FixedUpdate에서 처리한다 (CLAUDE.md 규칙).
        private void FixedUpdate()
        {
            UpdateGrounded();
            TryJump();
            ApplyHorizontal();
            ApplyGravity();
            CorrectCorner();
        }

        private void UpdateGrounded()
        {
            Bounds b = _collider.bounds;
            Vector2 center = new Vector2(b.center.x, b.min.y - config.groundCheckOffset);
            // OverlapBox는 Collider2D를 반환한다. C#은 객체를 bool로 자동 변환하지 않으므로
            // != null 로 명시해야 한다. (RaycastHit2D는 구조체라 bool 변환이 정의되어 있어 그냥 쓸 수 있다.)
            _isGrounded = Physics2D.OverlapBox(center, config.groundCheckSize, 0f, groundLayer) != null;

            if (_isGrounded && _rb.linearVelocity.y <= 0.01f)
            {
                // 땅에 있는 동안은 코요테 타이머를 가득 채워 둔다.
                _coyoteTimer = config.coyoteTime;
                _isJumping = false;
                _jumpCutApplied = false;
            }
            else
            {
                _coyoteTimer -= Time.fixedDeltaTime;
            }
        }

        private void TryJump()
        {
            // 버퍼(방금 눌렀음)와 코요테(방금까지 땅이었음)가 동시에 살아 있으면 점프.
            if (_jumpBufferTimer <= 0f || _coyoteTimer <= 0f) return;

            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, config.JumpVelocity);
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;   // 한 번의 입력으로 두 번 뛰지 않게 즉시 소모
            _isJumping = true;
            _jumpCutApplied = false;
        }

        private void ApplyHorizontal()
        {
            bool wantsMove = Mathf.Abs(_moveInput) > 0.01f;

            float target = _moveInput * config.maxSpeed;
            if (wantsMove && IsNearApex())
                target += Mathf.Sign(_moveInput) * config.apexBonusSpeed;

            float current = _rb.linearVelocity.x;

            float accel;
            if (!wantsMove)
            {
                accel = _isGrounded ? config.groundDecel : config.airDecel;
            }
            else
            {
                accel = _isGrounded ? config.groundAccel : config.airAccel;
                // 가던 방향과 반대로 입력하면 더 세게 가속해 방향 전환을 날카롭게 만든다.
                if (Mathf.Abs(current) > 0.01f && Mathf.Sign(target) != Mathf.Sign(current))
                    accel *= config.turnAccelMultiplier;
            }

            float newX = Mathf.MoveTowards(current, target, accel * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(newX, _rb.linearVelocity.y);
        }

        private void ApplyGravity()
        {
            // 땅에 서 있고 점프 중이 아니면 약하게 아래로 눌러 지면에 붙인다.
            // 0으로 고정하면 안 된다 — 접지 판정은 콜라이더 바닥보다 살짝 아래까지 보므로
            // 실제로는 조금 떠 있는데 접지로 잡히는 순간이 있고, 그때 속도가 0이면
            // 중력도 건너뛰어 공중에 굳어버린다. 아래로 눌러두면 스스로 지면까지 내려온다.
            if (_isGrounded && !_isJumping)
            {
                if (_rb.linearVelocity.y <= 0f)
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -config.groundStickSpeed);
                return;
            }

            float g = config.Gravity;
            if (IsNearApex())
                g *= config.apexGravityMultiplier;          // 정점 체공
            else if (_rb.linearVelocity.y < 0f)
                g *= config.fallGravityMultiplier;          // 하강은 빠르게

            float vy = _rb.linearVelocity.y - g * Time.fixedDeltaTime;
            vy = Mathf.Max(vy, -config.maxFallSpeed);
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, vy);
        }

        /// <summary>공중에서 수직 속도가 거의 0인 구간 = 점프 최고점 부근.</summary>
        private bool IsNearApex()
        {
            return !_isGrounded && Mathf.Abs(_rb.linearVelocity.y) < config.apexThreshold;
        }

        /// <summary>
        /// 상승 중 머리의 한쪽 모서리만 천장에 걸렸을 때 옆으로 밀어준다.
        /// 이게 없으면 통과할 수 있어 보이는 틈에서 머리가 턱 걸려 답답해진다.
        /// </summary>
        private void CorrectCorner()
        {
            if (config.cornerCorrectionDistance <= 0f) return;
            if (_rb.linearVelocity.y <= 0.01f) return;

            Bounds b = _collider.bounds;
            const float inset = 0.03f;   // 모서리에서 살짝 안쪽에서 쏜다
            const float probeUp = 0.08f;

            Vector2 leftTop = new Vector2(b.min.x + inset, b.max.y);
            Vector2 rightTop = new Vector2(b.max.x - inset, b.max.y);

            bool leftBlocked = Physics2D.Raycast(leftTop, Vector2.up, probeUp, groundLayer);
            bool rightBlocked = Physics2D.Raycast(rightTop, Vector2.up, probeUp, groundLayer);

            // 양쪽 다 막혔으면 진짜 천장이므로 보정하지 않는다.
            if (leftBlocked == rightBlocked) return;

            float dir = leftBlocked ? 1f : -1f;   // 막힌 반대쪽으로 민다
            const float step = 0.02f;

            for (float d = step; d <= config.cornerCorrectionDistance; d += step)
            {
                Vector2 offset = new Vector2(dir * d, 0f);
                bool l = Physics2D.Raycast(leftTop + offset, Vector2.up, probeUp, groundLayer);
                bool r = Physics2D.Raycast(rightTop + offset, Vector2.up, probeUp, groundLayer);
                if (!l && !r)
                {
                    _rb.position += offset;   // 빠져나갈 수 있는 최소 거리만 이동
                    return;
                }
            }
        }

        // 씬 뷰에서 접지 판정 박스를 눈으로 확인하기 위한 것. 튜닝할 때 매우 유용하다.
        private void OnDrawGizmosSelected()
        {
            if (config == null) return;
            var col = GetComponent<CapsuleCollider2D>();
            if (col == null) return;

            Bounds b = col.bounds;
            Vector3 center = new Vector3(b.center.x, b.min.y - config.groundCheckOffset, 0f);
            Gizmos.color = Application.isPlaying && _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(center, config.groundCheckSize);
        }
    }
}
