using System;
using System.Collections;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 체력과 피격 처리. 플레이어든 적이든 맞는 쪽에 붙인다.
    /// 히트박스는 상대가 무엇인지 모른 채 IDamageable로만 때리므로, 이 하나로 양쪽을 다 덮는다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private CombatConfig config;

        [Tooltip("최대 체력")]
        [SerializeField] private float maxHp = 3f;

        [Tooltip("체력이 0이 되면 오브젝트를 지운다. 플레이어처럼 죽어도 남아야 하는 대상은 꺼둔다")]
        [SerializeField] private bool destroyOnDeath = true;

        /// <summary>피격 시 알린다. HUD, 피격 이펙트, AI의 경직 처리가 여기 붙는다.</summary>
        public event Action<DamageInfo> Damaged;

        /// <summary>사망 시 알린다. 오브젝트가 지워지기 전에 호출된다.</summary>
        public event Action Died;

        /// <summary>
        /// 피격 무적과 별개로 "지금은 안 맞는다"를 외부가 켜는 스위치.
        /// 플레이어의 슬라이드가 이걸 쓴다 — 슬라이딩이 곧 회피라는 것이 이 게임의 설계다.
        /// 타이머가 아니라 상태라서, 켠 쪽이 끄는 책임도 진다.
        /// </summary>
        public bool Invincible { get; set; }

        public float Current { get; private set; }
        public float Max => maxHp;
        public bool IsAlive => Current > 0f;

        private Rigidbody2D _rb;
        private SpriteRenderer[] _renderers;
        private float _invincibleTimer;
        private Coroutine _blink;
        private bool _knockedBack;

        /// <summary>
        /// 넉백으로 밀려나는 중인가. 이 몸을 움직이는 쪽(적 AI, 플레이어 조작)은
        /// 이 값이 true인 동안 속도를 건드리지 않아야 한다. 안 그러면 맞자마자
        /// 제자리로 되돌아와서 맞은 티가 나지 않는다.
        /// </summary>
        public bool IsKnockedBack => _knockedBack;

        /// <summary>
        /// 넉백을 지금 끝낸다. <b>맞은 쪽이 스스로 속도를 정하기 시작했을 때</b> 부른다
        /// (플레이어가 넉백 도중 슬라이드로 빠져나가는 경우).
        ///
        /// 없으면 안 되는 이유: 넉백은 지속 시간이 아니라 <b>속도가 0이 될 때까지</b>로 끝난다.
        /// 그 사이 다른 시스템이 속도를 높게 유지하면 아래 FixedUpdate가 영영 0에 못 닿아
        /// 넉백 상태가 그 시스템이 끝날 때까지 붙어 있는다. 실제로 넉백 중 슬라이드가
        /// 그랬다 — 슬라이드 내내 IsKnockedBack이 true라 무적·히트박스·과열이 통째로 죽었다.
        /// </summary>
        public void EndKnockback() => _knockedBack = false;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            // 스프라이트가 자식에 있을 수 있다(플레이어의 Visual). 한 번만 모아둔다.
            _renderers = GetComponentsInChildren<SpriteRenderer>();
            Current = maxHp;
        }

        private void Update()
        {
            if (_invincibleTimer > 0f) _invincibleTimer -= Time.deltaTime;
        }

        // 물리는 FixedUpdate에서 처리한다 (CLAUDE.md 규칙).
        private void FixedUpdate()
        {
            if (!_knockedBack) return;

            // 넉백은 "속도를 대입하고 끝"이 아니다. 줄여주는 주체가 없으면 받은 속도를
            // 그대로 안고 계속 날아간다 — 실제로 한 대 맞은 허수아비가 15유닛을 날아갔다.
            // 지속 시간을 따로 두지 않고 속도가 0이 될 때까지만 깎는다. 그래야 세게 맞으면
            // 오래 밀리고 약하게 맞으면 금방 멈춘다는 관계가 저절로 성립한다.
            var v = _rb.linearVelocity;
            float vx = Mathf.MoveTowards(v.x, 0f, config.knockbackDecay * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(vx, v.y);

            if (Mathf.Abs(vx) < 0.01f) _knockedBack = false;
        }

        public bool TakeDamage(DamageInfo info)
        {
            if (!IsAlive || Invincible || _invincibleTimer > 0f) return false;

            Current = Mathf.Max(0f, Current - info.amount);
            _invincibleTimer = config.invincibleDuration;

            // 넉백은 힘이 아니라 속도를 그대로 대입한다.
            // AddForce로 주면 질량에 따라 밀리는 거리가 달라져서 적 종류마다 반응이 제각각이 된다.
            // 밀리는 거리는 디자인 값이지 물리 결과가 아니다.
            if (info.knockback != Vector2.zero)
            {
                _rb.linearVelocity = info.knockback;
                _knockedBack = true;
            }

            Damaged?.Invoke(info);

            if (!IsAlive)
            {
                Die();
                return true;
            }

            if (_blink != null) StopCoroutine(_blink);
            _blink = StartCoroutine(Blink());
            return true;
        }

        private void Die()
        {
            SetRenderersVisible(true);
            Died?.Invoke();
            if (destroyOnDeath) Destroy(gameObject);
        }

        /// <summary>무적 동안 스프라이트를 켰다 껐다 해서 "지금은 안 맞는다"를 보여준다.</summary>
        private IEnumerator Blink()
        {
            float t = 0f;
            bool visible = false;

            // WaitForSeconds는 timeScale의 영향을 받는다. 히트스톱으로 게임이 멈추면
            // 깜빡임도 같이 멈추는 게 맞다 — 정지 중에 혼자 깜빡이면 정지가 풀린 것처럼 보인다.
            while (t < config.invincibleDuration)
            {
                SetRenderersVisible(visible);
                visible = !visible;
                yield return new WaitForSeconds(config.blinkInterval);
                t += config.blinkInterval;
            }

            SetRenderersVisible(true);
            _blink = null;
        }

        private void SetRenderersVisible(bool visible)
        {
            foreach (var r in _renderers)
            {
                // 파괴된 UnityEngine.Object는 == null이 true지만 진짜 null은 아니다.
                // 그래서 ?. 를 쓰면 안 된다 (CLAUDE.md 규칙). 명시적으로 비교한다.
                if (r != null) r.enabled = visible;
            }
        }
    }
}
