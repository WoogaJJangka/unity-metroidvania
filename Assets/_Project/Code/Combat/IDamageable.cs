using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 한 번의 타격에 실려 가는 정보. 때린 쪽이 채우고 맞은 쪽이 읽는다.
    ///
    /// class가 아니라 struct인 이유: 타격은 프레임마다 여러 번 일어날 수 있는데
    /// class로 만들면 매번 힙 할당이 생겨 GC가 주기적으로 프레임을 끊는다.
    /// struct는 C의 구조체처럼 값으로 복사되므로 할당이 없다.
    /// </summary>
    public struct DamageInfo
    {
        /// <summary>깎을 체력.</summary>
        public float amount;

        /// <summary>맞은 쪽에 그대로 대입할 속도 (units/sec). 월드 기준.</summary>
        public Vector2 knockback;

        /// <summary>때린 주체. "누가 죽였는가"를 나중에 물어보기 위해 남긴다.</summary>
        public GameObject source;
    }

    /// <summary>
    /// 맞을 수 있는 것. 플레이어든 적이든 부술 수 있는 상자든 이것만 구현하면
    /// 히트박스는 상대가 무엇인지 몰라도 때릴 수 있다.
    ///
    /// interface는 C에는 없는 개념이다. "이 함수들을 반드시 가지고 있다"는 약속만 정의하고
    /// 구현은 각자 한다. 함수 포인터만 모아둔 구조체를 넘기던 것과 목적이 같다.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(DamageInfo info);

        /// <summary>이미 죽은 대상을 다시 때리지 않기 위한 확인용.</summary>
        bool IsAlive { get; }
    }
}
