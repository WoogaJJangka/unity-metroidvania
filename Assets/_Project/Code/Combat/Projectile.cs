using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 날아가는 탄. 이동과 수명만 맡는다 — 피해·넉백·타격감은 같은 오브젝트의 Hitbox가
    /// 이미 다 한다. Hitbox의 origin을 비워 두면 탄 자신이 기준이 되어 넉백 방향이
    /// 저절로 "탄이 날아온 반대쪽"이 된다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [Tooltip("닿으면 사라지는 레이어. 보통 Ground + 맞는 쪽(Player). " +
                 "맞는 쪽을 넣어야 무적으로 뚫고 지나가도 탄이 남아 뒤에서 다시 때리지 않는다")]
        [SerializeField] private LayerMask blockLayer;

        private Rigidbody2D _rb;
        private float _life;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;   // 탄은 떨어지지 않는다
        }

        /// <summary>쏜 쪽이 부른다. 속도와 수명을 함께 준다.</summary>
        public void Launch(Vector2 velocity, float life)
        {
            _rb.linearVelocity = velocity;
            _life = life;
        }

        // 수명은 물리와 무관하므로 Update에서 센다.
        private void Update()
        {
            _life -= Time.deltaTime;
            if (_life <= 0f) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // LayerMask는 비트 집합이다. C의 (mask & (1 << bit))과 같은 검사.
            if ((blockLayer.value & (1 << other.gameObject.layer)) != 0)
                Destroy(gameObject);
        }
    }
}
