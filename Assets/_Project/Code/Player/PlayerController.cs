using UnityEngine;
using UnityEngine.InputSystem;
using Game.Combat;

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
        private Health _health;   // 없을 수도 있다. 넉백 중인지 묻기 위해서만 쓴다
        private InputActionAsset _actionsInstance;   // 이 컴포넌트 전용 복사본
        private InputActionMap _playerMap;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _dashAction;

        private float _moveInput;
        private bool _isGrounded;
        private float _coyoteTimer;      // 발판을 떠난 뒤 남은 점프 허용 시간
        private float _jumpBufferTimer;  // 미리 누른 점프가 유효한 남은 시간
        private bool _isJumping;         // 이번 점프의 상승 구간이 진행 중인가
        private bool _jumpCutApplied;    // 이번 점프에서 이미 높이를 깎았는가
        private float _facing = 1f;      // 마지막으로 바라본 방향 (+1 오른쪽 / -1 왼쪽)
        private float _dashBufferTimer;  // 미리 누른 슬라이드가 유효한 남은 시간
        private bool _isSliding;
        private float _dashCooldownTimer;
        private float _dashDir;
        private bool _wasDescending;     // 이번 슬라이드 중 내리막(grade&lt;0)을 탄 적이 있는가
        private Vector2 _groundNormal = Vector2.up;   // 발밑 지면의 법선. 평지면 (0,1)

        // 접촉면 조회용. 매 프레임 새로 만들면 쓰레기가 쌓이므로 미리 잡아둔다.
        private readonly ContactPoint2D[] _contactBuf = new ContactPoint2D[8];
        private ContactFilter2D _groundFilter;

        /// <summary>
        /// 지면의 기울기 = tan(경사각). 오른쪽이 오르막이면 양수, 내리막이면 음수, 평지면 0.
        /// 수평 속도 vx로 경사면을 따라가려면 세로 속도가 vx * 이 값이어야 한다.
        /// </summary>
        private float SlopeTangent => -_groundNormal.x / _groundNormal.y;

        /// <summary>다른 시스템(애니메이션 등)이 상태를 읽기 위한 통로.</summary>
        public bool IsGrounded => _isGrounded;
        public Vector2 Velocity => _rb.linearVelocity;
        public bool IsDashing => _isSliding;

        /// <summary>
        /// 슬라이드의 공방 판정이 살아 있는가. 기준은 슬라이드 '상태'가 아니라 <b>수평 속도</b>다 —
        /// 빠르면 때리고 안 맞고, 느려지면 둘 다 꺼진다. 무적과 히트박스가 같은 값을 읽으므로
        /// "안 맞는데 못 때리는" 구간이 생기지 않는다.
        ///
        /// 상태로 판정하면 슬라이드가 끝나는 순간(마찰·발판 이탈·점프·내리막 종료) 판정이
        /// 절벽처럼 사라져 적을 뚫는 도중에 맞는다. 속도는 서서히 줄기만 하므로 그 경계가 없고,
        /// 공중까지 자연히 이어진다.
        ///
        /// 넉백 중을 빼는 이유: 적의 몸통 넉백이 12로 임계값과 같아서, 맞고 밀려나는 것만으로
        /// 판정이 켜져 때린 적을 되받아친다. 맞은 것이 공격이 되면 안 된다.
        ///
        /// 이 값을 <b>PlayerController가 직접</b> Health에 옮기는 이유는 타이밍이다.
        /// 속도는 FixedUpdate에서 정해지는데 Unity는 FixedUpdate -> 물리 스텝(트리거 콜백)
        /// -> Update 순으로 돈다. 무적을 Update에서 걸면 최대 2 물리 스텝(100Hz/60fps 기준 20ms)
        /// 늦고, 그동안 적의 몸통 히트박스는 매 스텝 때린다 —
        /// "적이 붙었을 때 슬라이드로 빠져나간다"가 바로 그 경우다.
        /// </summary>
        public bool AtSlideSpeed => Mathf.Abs(_rb.linearVelocity.x) >= config.invincibleSpeed
                                    && (_health == null || !_health.IsKnockedBack);

        /// <summary>마지막으로 바라본 방향 (+1 오른쪽 / -1 왼쪽). 공격 방향이 이 값을 쓴다.</summary>
        public float Facing => _facing;

        /// <summary>
        /// 이 컴포넌트가 만든 입력 복사본의 Player 맵.
        /// 같은 오브젝트의 다른 컴포넌트(PlayerAttack 등)는 입력 에셋을 또 Instantiate하지 말고
        /// 이 맵에서 액션을 찾아 써야 한다. 복사본이 둘이 되면 한쪽만 Enable된 채로 남는다.
        /// </summary>
        public InputActionMap PlayerMap => _playerMap;

        private void Awake()
        {
            // GetComponent는 비싸므로 Awake에서 한 번만 캐싱한다 (CLAUDE.md 규칙).
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CapsuleCollider2D>();
            _health = GetComponent<Health>();

            _rb.gravityScale = 0f;                 // 중력은 우리가 직접 계산한다
            _rb.freezeRotation = true;             // 캐릭터가 굴러다니지 않게
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            _groundFilter = new ContactFilter2D();
            _groundFilter.SetLayerMask(groundLayer);
            _groundFilter.useTriggers = false;

            // 에셋을 그대로 쓰면 안 된다.
            // InputSystem_Actions는 프로젝트 전역 입력 에셋이라 Unity가 스스로 켜고 끄는데,
            // 여기서 같은 객체를 또 Enable/Disable하면 두 주체가 같은 내부 상태를 건드려
            // "Map must be contained in state" 류의 오류와 함께 입력 처리 전체가 죽는다.
            // 그래서 이 컴포넌트 전용 복사본을 만들어 쓴다.
            _actionsInstance = Instantiate(inputActions);

            // 두 번째 인자 true = 못 찾으면 예외를 던진다. 오타를 조용히 넘기지 않기 위함.
            _playerMap = _actionsInstance.FindActionMap("Player", true);
            _moveAction = _playerMap.FindAction("Move", true);
            _jumpAction = _playerMap.FindAction("Jump", true);
            // 대시는 템플릿에 이미 있는 Sprint(LeftShift / 좌스틱 클릭)를 그대로 쓴다.
            _dashAction = _playerMap.FindAction("Sprint", true);
        }

        private void OnEnable() => _playerMap?.Enable();

        private void OnDisable()
        {
            _playerMap?.Disable();
            // 슬라이드 도중에 꺼지면 무적이 켜진 채로 남는다. 켠 쪽이 끈다.
            if (_health != null) _health.Invincible = false;
        }

        private void OnDestroy()
        {
            // 복사본은 우리가 만들었으니 우리가 치운다. 안 그러면 플레이할 때마다 쌓인다.
            if (_actionsInstance != null) Destroy(_actionsInstance);
        }

        // 입력은 Update에서 읽는다. FixedUpdate는 프레임마다 돌지 않아 입력을 놓칠 수 있다.
        private void Update()
        {
            _moveInput = _moveAction.ReadValue<Vector2>().x;
            if (Mathf.Abs(_moveInput) > 0.01f) _facing = Mathf.Sign(_moveInput);

            // 점프 버퍼와 같은 구조. 착지 직전에 눌러도 착지하는 순간 슬라이드가 나가야
            // 슬라이드 → 점프 → 슬라이드 연계가 성립한다. 프레임을 정확히 맞출 것을 요구하면
            // 연계 자체가 성립하지 않는다.
            if (_dashAction.WasPressedThisFrame())
                _dashBufferTimer = config.jumpBufferTime;
            else
                _dashBufferTimer -= Time.deltaTime;

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
            UpdateDash();
            TryJump();
            ApplyHorizontal();
            ApplyGravity();
            CorrectCorner();

            // ponytail: Health.Invincible의 유일한 기록자가 슬라이드라고 가정한다. 나중에
            // 다른 무적(리스폰, 과열 잠금)이 생기면 매 스텝 여기서 덮이므로 카운터로 바꿀 것.
            // 속도가 다 확정된 뒤, 이 프레임의 물리 스텝(트리거 콜백)보다 먼저 건다.
            // 여기가 아니라 Update에서 걸면 슬라이드 시작이 무적보다 최대 2 물리 스텝 앞서
            // 나가고, 그동안 적의 몸통 히트박스에 그대로 맞는다.
            if (_health != null) _health.Invincible = AtSlideSpeed;
        }

        private void UpdateGrounded()
        {
            Bounds b = _collider.bounds;
            Vector2 center = new Vector2(b.center.x, b.min.y - config.groundCheckOffset);
            // OverlapBox는 Collider2D를 반환한다. C#은 객체를 bool로 자동 변환하지 않으므로
            // != null 로 명시해야 한다. (RaycastHit2D는 구조체라 bool 변환이 정의되어 있어 그냥 쓸 수 있다.)
            _isGrounded = Physics2D.OverlapBox(center, config.groundCheckSize, 0f, groundLayer) != null;
            _groundNormal = _isGrounded ? ProbeGroundNormal(b) : Vector2.up;

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

        /// <summary>
        /// 발밑으로 광선을 쏴 지면의 법선을 얻는다. 접지 여부 자체는 위의 OverlapBox가 이미
        /// 정했고(튜닝이 끝난 판정이라 건드리지 않는다) 여기서는 각도만 본다.
        ///
        /// 진행 방향 앞쪽을 미리 보는 방식은 쓰지 않는다. 시도해봤지만 평지에서 오르막에
        /// 닿기도 전에 위쪽 속도가 붙어 플레이어가 발사된다(실측 vy +3.9). 경사 진입에서
        /// 속도가 깎이는 문제는 코드가 아니라 지형 문제다 — 램프 끝이 바닥 표면에 꼭짓점으로
        /// 노출되면 캡슐이 거기 부딪혀 속도를 잃는다. 램프 아래끝을 바닥 안으로 묻어야 한다.
        ///
        /// ponytail: 램프를 묻어도 진입 0.5유닛 구간에서 vx가 9 -> 2.2로 잠깐 꺼진다.
        /// 캡슐 앞면이 램프에 닿는 동안 중심 광선은 아직 아래 바닥을 먼저 맞기 때문. 접지는
        /// 유지되고 groundAccel로 0.1초 만에 복구되므로 그대로 둔다. 거슬리면 접촉점 법선
        /// (Collider2D.GetContacts)으로 바꿀 것 — 광선을 앞으로 옮기는 방식은 이미 실패했다.
        /// </summary>
        private Vector2 ProbeGroundNormal(Bounds b)
        {
            // 광선 길이는 접지 판정 박스가 볼 수 있는 만큼 내려가야 한다.
            // 박스는 폭이 있어서 경사면 위쪽 모서리로 지면을 먼저 잡는데, 중심에서 쏘는 광선이
            // 거기까지 못 닿으면 "접지는 맞는데 경사는 평지"로 읽혀 경사 추종이 통째로 빠진다.
            // 박스 반폭 x tan(최대경사)가 그 높이차다.
            float slopeReach = config.groundCheckSize.x * 0.5f
                               * Mathf.Tan(config.maxSlopeAngle * Mathf.Deg2Rad);
            float reach = b.extents.y + config.groundCheckOffset + config.groundCheckSize.y + slopeReach;

            // 광선 시작점이 플레이어 콜라이더 안이지만 groundLayer로 걸러 쏘므로 자기 자신은 맞지 않는다.
            var hit = Physics2D.Raycast(b.center, Vector2.down, reach, groundLayer);

            // 너무 가파른 면은 경사로 취급하지 않는다. 그대로 두면 tan이 폭증해
            // 수평 속도가 세로 속도로 증폭되면서 벽을 타고 튀어오른다.
            float minNormalY = Mathf.Cos(config.maxSlopeAngle * Mathf.Deg2Rad);
            Vector2 normal = (hit && hit.normal.y >= minNormalY) ? hit.normal : Vector2.up;

            return ClimbContactNormal(b, normal, minNormalY);
        }

        /// <summary>
        /// 캡슐 앞면이 이미 오르막에 닿아 있는데 중심 광선은 그 아래 평지를 먼저 맞는 구간이 있다.
        /// 램프 아래끝을 바닥에 묻으면 반드시 생긴다 — 램프 표면이 바닥 위로 올라오기 전에
        /// 캡슐의 앞쪽이 먼저 그 면에 닿기 때문이다.
        ///
        /// 그 구간에서 경사가 평지로 읽히면 vy가 -groundStickSpeed로 고정되어 아래로 눌리는데,
        /// 캡슐은 경사면을 벽처럼 밀기만 하므로 올라갈 방법이 없다. 40도에서는 잠깐 버벅이고 말지만
        /// 45도에서는 완전히 멈춘다 (실측: vx 9 -> 1.100에서 영구 정지).
        ///
        /// 진행 방향 앞쪽을 광선으로 미리 보는 방식은 쓰면 안 된다 — 평지에서 램프에 닿기도 전에
        /// vy가 붙어 플레이어가 발사된다. 그래서 '실제로 닿아 있는 면'만 본다.
        /// </summary>
        private Vector2 ClimbContactNormal(Bounds b, Vector2 normal, float minNormalY)
        {
            // 가려는 방향이 있어야 '밀고 올라갈 면'을 고를 수 있다.
            float vx = _rb.linearVelocity.x;
            float dir = Mathf.Abs(vx) > 0.05f ? Mathf.Sign(vx)
                      : (Mathf.Abs(_moveInput) > 0.01f ? Mathf.Sign(_moveInput) : 0f);
            if (dir == 0f) return normal;

            int count = _collider.GetContacts(_groundFilter, _contactBuf);
            for (int i = 0; i < count; i++)
            {
                Vector2 n = _contactBuf[i].normal;
                if (n.y < minNormalY) continue;                                   // 벽이거나 오르기엔 너무 가파르다
                if (n.x * dir >= 0f) continue;                                    // 밀고 들어가는 오르막만 본다
                if ((_contactBuf[i].point.x - b.center.x) * dir <= 0f) continue;  // 뒤에 두고 온 면은 무시.
                                                                                  // 없으면 램프 끝에서 뒤꿈치가 계속 밀어 올려 통통 튄다
                if (n.y < normal.y) normal = n;                                   // 더 가파른 쪽이 지금 올라야 할 면이다
            }
            return normal;
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

            // 점프는 슬라이드를 끊되 수평 속도는 건드리지 않는다. 그 속도를 ApplyHorizontal이
            // momentumDecel로만 깎으므로 공중까지 실려 나간다 — 슬라이드 점프의 근거.
            if (_isSliding) EndSlide();
        }

        /// <summary>
        /// 지상 슬라이드. 고정 지속 시간이 없다 — dashDecel로 깎이다 maxSpeed까지 떨어지면 끝난다.
        /// 들어올 때 이미 빠르면 그 속도를 유지하므로 슬라이드 → 점프 → 슬라이드로 속도가 이어진다.
        /// </summary>
        private void UpdateDash()
        {
            if (!_isSliding) _dashCooldownTimer -= Time.fixedDeltaTime;

            if (!_isSliding && _dashBufferTimer > 0f && _isGrounded && _dashCooldownTimer <= 0f)
            {
                _dashBufferTimer = 0f;
                _isSliding = true;
                _dashDir = _facing;
                _wasDescending = false;

                // 여기가 모멘텀 체이닝의 핵심. 슬라이드 점프로 얻은 속도를 안고 착지해
                // 다시 슬라이드하면 dashSpeed로 깎이는 게 아니라 그 속도가 그대로 이어진다.
                float entrySpeed = Mathf.Max(config.dashSpeed, Mathf.Abs(_rb.linearVelocity.x));
                _rb.linearVelocity = new Vector2(_dashDir * entrySpeed, _rb.linearVelocity.y);
                return;   // 출발 프레임에는 깎지 않는다
            }

            if (!_isSliding) return;

            // 경사 가속. grade는 진행 방향 기준 기울기 — 오르막이면 양수, 내리막이면 음수다.
            // 내리막에서 목표 속도가 maxSpeed 위로 올라가므로 아래 종료 조건에 걸리지 않고
            // 슬라이드가 이어지며 속도가 누적된다. 평지에서는 grade가 0이라 예전과 완전히 같다.
            float grade = SlopeTangent * _dashDir;

            // 내리막에서 붙은 속도가 평지로 넘어오는 순간 dashDecel(마찰)로 그대로 갈리면
            // 슬로프 보너스가 사라진다. 경사가 더 돕지 않게 된 순간(내리막 -> 평지/오르막)
            // 슬라이드를 끝내 ApplyHorizontal의 모멘텀 보존(momentumDecel)으로 넘긴다 —
            // 슬라이드 점프 착지와 같은 경로. 평지에서 시작한 슬라이드는 grade가 계속 0이라
            // 이 분기를 타지 않으므로 원래 마찰 감속(32 -> 9.52)은 그대로다.
            if (grade < 0f) _wasDescending = true;
            else if (_wasDescending)
            {
                EndSlide();
                return;
            }

            float slideTarget = config.maxSpeed - grade * config.slopeDashBonus;

            // 붙는 속도(경사 가속)와 깎이는 속도(마찰)를 따로 쓴다.
            // 하나로 묶으면 경사 이득을 키우는 순간 평지 슬라이드 거리가 같이 줄어든다.
            // 램프는 짧아서(45도 4칸 = 0.14초) 천장이 아니라 이 가속도가 실제 이득을 정한다 —
            // 하나로 묶여 있을 때는 4칸 내리막의 이득이 정확히 0이었다(최고 속도가 진입값 32 그대로).
            float speedAlong = _rb.linearVelocity.x * _dashDir;
            float rate = slideTarget > speedAlong ? config.slopeDashAccel : config.dashDecel;

            float vx = Mathf.MoveTowards(_rb.linearVelocity.x, _dashDir * slideTarget,
                                         rate * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(vx, _rb.linearVelocity.y);

            // 평소 달리기 속도까지 마찰로 떨어졌거나 발판을 벗어나면 슬라이드가 끝난다.
            if (Mathf.Abs(vx) <= config.maxSpeed + 0.01f || !_isGrounded)
                EndSlide();
        }

        private void EndSlide()
        {
            _isSliding = false;
            _dashCooldownTimer = config.dashCooldown;
        }

        private void ApplyHorizontal()
        {
            // 맞고 밀려나는 중에는 조작이 속도를 덮어쓰지 않는다. 안 그러면 넉백 속도가
            // 다음 물리 스텝에 groundAccel로 지워져 맞은 티가 전혀 나지 않는다.
            // 지속 시간을 따로 두지 않는다 — Health가 속도를 0까지 깎으면 스스로 풀린다.
            if (_health != null && _health.IsKnockedBack) return;

            // 슬라이드 중에는 입력으로 수평 속도를 건드리지 않는다. 안 그러면 반대로 입력하는
            // 것만으로 슬라이드가 즉시 죽어서 거리가 들쭉날쭉해진다.
            if (_isSliding) return;

            bool wantsMove = Mathf.Abs(_moveInput) > 0.01f;

            // maxSpeed를 넘는 속도는 슬라이드에서 얻은 모멘텀이다. 같은 방향으로 가는 한
            // 평소 가감속(airAccel 100 등)으로 끌어내리지 않고 momentumDecel로만 깎는다.
            // 이 분기가 없으면 슬라이드 점프의 속도가 공중에서 즉시 증발해 연계가 성립하지 않는다.
            // 반대 방향을 입력하면 이 분기를 타지 않으므로 평소대로 급제동이 걸린다.
            float vxNow = _rb.linearVelocity.x;
            if (Mathf.Abs(vxNow) > config.maxSpeed
                && (!wantsMove || Mathf.Sign(_moveInput) == Mathf.Sign(vxNow)))
            {
                // 지면 마찰이 공중보다 세야 자연스럽다. 같은 값을 쓰면 평지에 내려선 뒤에도
                // 한참을 미끄러진다. 공중값을 올려서 맞추면 슬라이드 점프 연계가 끊긴다.
                float decel = _isGrounded ? config.groundMomentumDecel : config.momentumDecel;
                float kept = Mathf.MoveTowards(vxNow, Mathf.Sign(vxNow) * config.maxSpeed,
                                               decel * Time.fixedDeltaTime);
                _rb.linearVelocity = new Vector2(kept, _rb.linearVelocity.y);
                return;
            }

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
                // 접지 중에는 속도가 지면에 종속된다. 두 항으로 나뉜다.
                //  (1) 경사면을 따라가는 성분 — 내리막에서 -groundStickSpeed(2)만 쓰면
                //      지면이 내려가는 속도를 못 따라가 통통 튀고, 오르막에서는 콜라이더에 밀려 버벅인다.
                //  (2) 지면으로 눌러붙이는 성분 — 반드시 '월드 아래'가 아니라 '법선 반대'로 밀어야 한다.
                //      아래로 밀면 경사면에서 그 힘의 접선 성분이 남고, 물리 솔버가 파고든 속도를
                //      되돌릴 때 그게 전진 속도로 새어 들어간다. 40도 경사에서 스텝당 +0.98
                //      (= 98 u/s²)이 붙어 걷기만 해도 vx가 9에서 25까지 폭주했다.
                // 평지에서는 법선이 (0,1)이라 예전과 똑같이 -groundStickSpeed만 남는다.
                float vx = _rb.linearVelocity.x;
                _rb.linearVelocity = new Vector2(
                    vx - config.groundStickSpeed * _groundNormal.x,
                    vx * SlopeTangent - config.groundStickSpeed * _groundNormal.y);
                return;
            }

            // 상승 / 정점 / 하강에 각각 다른 중력을 쓴다.
            // 하강 중력은 상승과 독립적으로 계산되므로 한쪽만 튜닝해도 다른 쪽이 안 흔들린다.
            float g;
            if (IsNearApex())
                g = config.Gravity * config.apexGravityMultiplier;   // 정점 체공
            else if (_rb.linearVelocity.y < 0f)
                g = config.FallGravity;                              // 하강
            else
                g = config.Gravity;                                  // 상승

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
            // 이번 물리 스텝에 실제로 올라갈 거리만큼은 미리 봐야 한다.
            // 고정값(0.08)만 쓰면 점프 속도가 빠를 때 한 스텝에 0.2 이상 올라가면서
            // 탐지 구간을 통째로 건너뛰고 그냥 천장에 박는다.
            float probeUp = Mathf.Max(0.08f, _rb.linearVelocity.y * Time.fixedDeltaTime + 0.02f);

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
