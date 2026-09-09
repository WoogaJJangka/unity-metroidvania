using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 공격 판정. 켜져 있는 동안 닿는 IDamageable에게 한 번씩 피해를 준다.
    /// 언제 켜고 끌지는 이 컴포넌트가 정하지 않는다 — 공격자(PlayerAttack 등)가 정한다.
    ///
    /// 콜라이더는 반드시 Trigger여야 하고, 레이어는 PlayerHitbox / EnemyHitbox 중 하나여야 한다.
    /// 누가 누구를 때릴 수 있는지는 코드가 아니라 Physics2D 충돌 매트릭스가 정한다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hitbox : MonoBehaviour
    {
        [SerializeField] private AttackConfig config;

        [Tooltip("넉백 방향을 재는 기준점. 보통 공격자 본체. 비우면 이 오브젝트를 쓴다")]
        [SerializeField] private Transform origin;

        [Tooltip("계속 켜져 있는 판정인가 (적의 몸통 접촉 피해 등). " +
                 "휘두르기는 켜질 때마다 대상 기록을 비워 중복을 막지만, 계속 켜져 있는 판정은 " +
                 "그 경계가 없어 한 번 때리면 영영 못 때린다. 켜 두면 기록을 쓰지 않고 " +
                 "연타는 맞는 쪽의 무적 시간이 막는다")]
        [SerializeField] private bool continuous;

        private CinemachineImpulseSource _impulse;
        private Rigidbody2D _attackerBody;   // 속도 비례 피해용. 없으면 비례가 꺼진다

        // 한 번 휘두르는 동안 같은 대상을 여러 번 때리지 않게 기록해 둔다.
        // readonly는 "이 필드가 가리키는 대상을 바꾸지 않는다"는 뜻 — C의 T* const와 비슷하다.
        // 안에 든 내용은 그대로 바꿀 수 있다.
        private readonly HashSet<IDamageable> _hitThisSwing = new HashSet<IDamageable>();

        private void Awake()
        {
            if (origin == null) origin = transform;
            // 화면 흔들림은 Cinemachine Impulse가 담당한다. 소스는 보통 공격자 본체에 붙인다.
            _impulse = GetComponentInParent<CinemachineImpulseSource>();
            // 히트박스는 공격자의 자식이거나(플레이어 슬라이드) 자기 자신이다(투사체).
            // 어느 쪽이든 GetComponentInParent가 때린 쪽의 몸을 찾아준다.
            _attackerBody = GetComponentInParent<Rigidbody2D>();
        }

        /// <summary>켜질 때마다 기록을 비운다. 이 경계가 곧 "휘두르기 한 번"이다.</summary>
        private void OnEnable() => _hitThisSwing.Clear();

        // Enter만 쓰면 히트박스가 켜지는 순간 이미 겹쳐 있던 대상을 놓칠 수 있고,
        // Stay만 쓰면 상대 Rigidbody2D가 잠들었을 때 호출이 끊긴다.
        // 둘 다 받고 중복은 _hitThisSwing이 막는다.
        private void OnTriggerEnter2D(Collider2D other) => TryHit(other);
        private void OnTriggerStay2D(Collider2D other) => TryHit(other);

        private void TryHit(Collider2D other)
        {
            // 콜라이더가 자식에 달려 있어도 본체의 Health를 찾아야 한다.
            var target = other.GetComponentInParent<IDamageable>();
            if (target == null || !target.IsAlive) return;

            // HashSet.Add는 이미 들어 있으면 false를 돌려준다. 넣기와 중복 검사가 한 번에 된다.
            if (!continuous && !_hitThisSwing.Add(target)) return;

            // Mathf.Sign은 0에 대해 +1을 돌려준다. 정확히 겹쳐 있으면 오른쪽으로 민다.
            float dir = Mathf.Sign(other.bounds.center.x - origin.position.x);

            // 속도 비례 피해. 슬라이드가 곧 공격이므로 얼마나 빨랐는지가 곧 화력이다.
            // 세로 속도는 빼고 수평만 본다 — 슬라이드가 수평 운동이고, 낙하 속도가
            // 피해로 새어 들어가면 그냥 떨어지기만 해도 세게 때리는 게 된다.
            float amount = config.damage;
            if (config.speedDamageReference > 0f && _attackerBody != null)
                amount *= Mathf.Pow(Mathf.Abs(_attackerBody.linearVelocity.x) / config.speedDamageReference,
                                    config.speedDamageExponent);

            bool landed = target.TakeDamage(new DamageInfo
            {
                amount = amount,
                knockback = new Vector2(dir * config.knockbackSpeed, config.knockbackLift),
                source = origin.gameObject,
            });

            // 무적으로 무시된 타격에는 연출을 내지 않는다. 슬라이드로 적을 통과할 때마다
            // 화면이 멈추고 흔들리면 "무적으로 뚫었다"가 "맞았다"로 읽힌다.
            if (!landed) return;

            Hitstop.Play(config.hitstop);
            if (_impulse != null && config.shakeForce > 0f)
                _impulse.GenerateImpulse(config.shakeForce);
        }
    }
}
