using UnityEngine;
using Game.Player;

namespace Game.World
{
    /// <summary>
    /// 밟으면 다른 맵으로 넘어가는 트리거. 대상 씬과 그 씬의 스폰 지점 id를 함께 지정한다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Portal : MonoBehaviour
    {
        [Tooltip("이동할 씬 이름. Build Settings에 등록되어 있어야 한다")]
        public string targetScene;

        [Tooltip("대상 씬에서 플레이어가 설 SpawnPoint의 id")]
        public string targetSpawnId = "default";

        private void Reset()
        {
            // 컴포넌트를 처음 붙일 때 콜라이더를 트리거로 만들어 준다. 매번 체크하는 것을 잊지 않게.
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;

            if (string.IsNullOrEmpty(targetScene))
            {
                Debug.LogError($"[Portal] '{name}'에 대상 씬이 지정되지 않았습니다.", this);
                return;
            }

            // 전환 중 중복 발동 방지는 GameManager가 담당한다.
            GameManager.Instance.TravelTo(targetScene, targetSpawnId);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.6f);
            var col = GetComponent<Collider2D>();
            if (col != null) Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
