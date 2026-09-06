using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// 맵에 들어올 때 플레이어가 서는 자리. 포털이 씬 이름과 이 id를 함께 지정한다.
    /// 씬 이름만으로는 "어디로 나오는지"가 정해지지 않기 때문이다.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [Tooltip("이 맵 안에서 유일해야 하는 이름. 포털이 이 값으로 찾는다")]
        public string id = "default";

        /// <summary>현재 로드된 씬에서 id가 일치하는 스폰 지점을 찾는다. 없으면 null.</summary>
        public static SpawnPoint Find(string id)
        {
            foreach (var sp in FindObjectsByType<SpawnPoint>())
                if (sp.id == id) return sp;
            return null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.9f);
        }
    }
}
