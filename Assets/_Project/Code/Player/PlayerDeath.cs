using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Combat;

namespace Game.Player
{
    /// <summary>
    /// 죽으면 현재 맵을 다시 로드한다.
    ///
    /// ponytail: 체크포인트도 페이드도 없는 최소 구현이다. 벤치/세이브 포인트는 Phase 5의
    /// 세이브 시스템이 들어올 때 이 한 줄을 갈아끼우면 된다. 지금 필요한 건 "죽으면 뭔가
    /// 일어난다"뿐이다 — 죽어도 아무 일이 없으면 전투를 검증할 수가 없다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class PlayerDeath : MonoBehaviour
    {
        private Health _health;

        private void Awake() => _health = GetComponent<Health>();

        // 이벤트 구독은 OnEnable, 해제는 OnDisable에서 짝을 맞춘다 (CLAUDE.md 규칙).
        private void OnEnable() => _health.Died += Respawn;
        private void OnDisable() => _health.Died -= Respawn;

        private void Respawn()
        {
            // 히트스톱 도중에 죽으면 timeScale이 0인 채로 씬이 넘어간다. 여기서 되돌린다.
            Time.timeScale = 1f;
            // LoadScene은 물리 콜백 안에서 불려도 안전하다. Unity가 실제 로드를
            // 프레임 끝으로 미룬다.
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
