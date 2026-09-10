using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Combat;

namespace Game.Player
{
    /// <summary>
    /// 플레이어 공격 두 가지의 리듬만 담당한다 — 언제 히트박스를 켜고 끄는가.
    /// 실제 판정·피해·타격감은 Game.Combat.Hitbox가 한다.
    ///
    /// 주 공격은 슬라이드다. 버튼이 아니라 PlayerController.IsDashing 상태가 켠다 —
    /// 이동과 공격이 같은 동작이라는 것이 이 게임의 차별점이라 공격 버튼을 따로 두지 않는다.
    /// 슬라이드 무적은 여기가 아니라 PlayerController가 건다 — 물리 타이밍 때문이다.
    /// 보조 공격(Attack 버튼)은 피해가 아니라 <b>거리 확보</b>가 목적이다. 붙은 적을
    /// 밀어내 슬라이드로 들어갈 공간을 만든다. 그래서 damage는 낮고 knockbackSpeed는 높다.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAttack : MonoBehaviour
    {
        [Header("보조 공격 — 거리 확보용")]
        [Tooltip("보조 공격의 수치. 피해는 낮게, 넉백은 크게 잡는다. " +
                 "밀려나는 거리 = knockbackSpeed² / (2 x CombatConfig.knockbackDecay)")]
        [SerializeField] private AttackConfig config;

        [Tooltip("휘두를 때 켜지는 히트박스 오브젝트. 플레이어 자식으로 두고 평소엔 꺼둔다")]
        [SerializeField] private GameObject hitbox;

        [Header("주 공격 — 슬라이드")]
        [Tooltip("슬라이드 중 계속 켜져 있는 히트박스. 몸을 덮게 두면 방향에 따라 뒤집을 필요가 없다. " +
                 "비워 두면 슬라이드 공격이 꺼진 채로 동작한다 (보조 공격은 그대로 쓸 수 있다)")]
        [SerializeField] private GameObject slideHitbox;

        /// <summary>휘두르는 중인가. 애니메이션과 이동 제한이 나중에 이 값을 읽는다.</summary>
        public bool IsAttacking { get; private set; }

        private PlayerController _player;
        private InputAction _attackAction;
        private float _cooldownTimer;
        private Vector3 _hitboxLocalPos;   // 오른쪽을 볼 때의 위치. 왼쪽이면 x를 뒤집는다

        private void Awake()
        {
            _player = GetComponent<PlayerController>();

            if (hitbox == null)
            {
                Debug.LogError($"[PlayerAttack] '{name}'에 히트박스가 지정되지 않았습니다.", this);
                enabled = false;
                return;
            }

            _hitboxLocalPos = hitbox.transform.localPosition;
            hitbox.SetActive(false);

            // 슬라이드 히트박스는 없어도 나머지가 돌아가야 한다. 여기서 컴포넌트를 꺼 버리면
            // 아직 배선하지 않은 씬에서 보조 공격까지 같이 죽는다.
            if (slideHitbox == null)
                Debug.LogWarning($"[PlayerAttack] '{name}'에 슬라이드 히트박스가 없습니다. 주 공격이 꺼진 상태입니다.", this);
            else
                slideHitbox.SetActive(false);
        }

        // Awake가 아니라 Start에서 액션을 찾는 이유:
        // PlayerController가 Awake에서 입력 에셋 복사본을 만든다. 같은 오브젝트에 붙은
        // 컴포넌트끼리 Awake 순서는 보장되지 않지만, 모든 Awake는 모든 Start보다 먼저 돈다.
        // 입력 에셋을 여기서 또 Instantiate하지 않는 것이 중요하다 — 복사본이 둘이 되면
        // 같은 키를 두 번 읽거나 한쪽만 Enable된 채로 남는다.
        private void Start()
        {
            _attackAction = _player.PlayerMap.FindAction("Attack", true);
        }

        // 입력은 Update에서 읽는다 (CLAUDE.md 규칙).
        private void Update()
        {
            // 주 공격: 판정 기준은 슬라이드 '상태'가 아니라 속도다(PlayerController.AtSlideSpeed).
            // 무적과 같은 값을 읽으므로 "안 맞는데 못 때리는" 구간이 없다 — 슬라이드 점프로
            // 날아가는 동안에도 뚫으면서 때린다. 켜지는 순간이 Hitbox의 "휘두르기 한 번"
            // 경계가 되어 한 번 빨라지는 동안 같은 적을 한 번만 때린다.
            bool fast = _player.AtSlideSpeed;
            if (slideHitbox != null && slideHitbox.activeSelf != fast)
                slideHitbox.SetActive(fast);

            // 방어 절반(Health.Invincible)은 PlayerController가 건다. 속도가 FixedUpdate에서
            // 정해지므로 여기(Update)에서 걸면 한 프레임 늦고, 그 사이 물리 스텝에서 맞는다.
            // 자세한 이유는 AtSlideSpeed 주석 참고.

            _cooldownTimer -= Time.deltaTime;

            if (_attackAction.WasPressedThisFrame() && !IsAttacking && _cooldownTimer <= 0f)
                StartCoroutine(Swing());
        }

        private IEnumerator Swing()
        {
            IsAttacking = true;

            // 휘두르는 방향은 입력 시점의 시선으로 고정한다. 도중에 방향을 바꿀 때
            // 히트박스가 따라 돌면 등 뒤에 있던 적이 맞는다.
            var p = _hitboxLocalPos;
            p.x = Mathf.Abs(p.x) * _player.Facing;
            hitbox.transform.localPosition = p;

            // WaitForSeconds는 timeScale을 따른다. 히트스톱으로 멈춘 동안 휘두르기도 같이
            // 멈추는 것이 맞다 — 정지 중에 혼자 진행하면 정지가 풀린 것처럼 보인다.
            yield return new WaitForSeconds(config.windup);
            hitbox.SetActive(true);

            yield return new WaitForSeconds(config.active);
            hitbox.SetActive(false);

            _cooldownTimer = config.recovery;
            IsAttacking = false;
        }
    }
}
