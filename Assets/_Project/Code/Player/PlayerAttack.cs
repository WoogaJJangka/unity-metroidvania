using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Combat;

namespace Game.Player
{
    /// <summary>
    /// 근접 공격의 리듬만 담당한다 — 언제 히트박스를 켜고 끄는가.
    /// 실제 판정·피해·타격감은 Game.Combat.Hitbox가 한다.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private AttackConfig config;

        [Tooltip("휘두를 때 켜지는 히트박스 오브젝트. 플레이어 자식으로 두고 평소엔 꺼둔다")]
        [SerializeField] private GameObject hitbox;

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
