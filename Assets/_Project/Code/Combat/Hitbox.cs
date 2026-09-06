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

        private CinemachineImpulseSource _impulse;

        // 한 번 휘두르는 동안 같은 대상을 여러 번 때리지 않게 기록해 둔다.
        // readonly는 "이 필드가 가리키는 대상을 바꾸지 않는다"는 뜻 — C의 T* const와 비슷하다.
        // 안에 든 내용은 그대로 바꿀 수 있다.
        private readonly HashSet<IDamageable> _hitThisSwing = new HashSet<IDamageable>();

        private void Awake()
        {
            if (origin == null) origin = transform;
            // 화면 흔들림은 Cinemachine Impulse가 담당한다. 소스는 보통 공격자 본체에 붙인다.
            _impulse = GetComponentInParent<CinemachineImpulseSource>();
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
            if (!_hitThisSwing.Add(target)) return;

            // Mathf.Sign은 0에 대해 +1을 돌려준다. 정확히 겹쳐 있으면 오른쪽으로 민다.
            float dir = Mathf.Sign(other.bounds.center.x - origin.position.x);

            target.TakeDamage(new DamageInfo
            {
                amount = config.damage,
                knockback = new Vector2(dir * config.knockbackSpeed, config.knockbackLift),
                source = origin.gameObject,
            });

            Hitstop.Play(config.hitstop);
            if (_impulse != null && config.shakeForce > 0f)
                _impulse.GenerateImpulse(config.shakeForce);
        }
    }
}
