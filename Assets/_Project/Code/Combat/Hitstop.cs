using System.Collections;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 명중한 순간 게임 전체를 아주 잠깐 멈춘다. 타격감의 큰 몫이 여기서 나온다 —
    /// 데미지 숫자가 아니라 이 정지가 "닿았다"는 신호를 준다.
    ///
    /// 씬에 배치하지 않는다. GameManager와 같은 방식으로 게임 시작 시 스스로 만들어진다.
    /// 그래야 어느 씬에서 Play를 눌러도 동작하고, 전투 코드가 매니저를 참조하지 않아도 된다.
    /// </summary>
    public class Hitstop : MonoBehaviour
    {
        private static Hitstop _runner;
        private static Coroutine _current;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            // 에디터에서 정지 도중 플레이를 멈추면 timeScale이 0인 채로 남는다.
            // 다음 플레이가 통째로 얼어붙으므로 시작할 때마다 되돌려 놓는다.
            Time.timeScale = 1f;

            if (_runner != null) return;
            var go = new GameObject("Hitstop");
            DontDestroyOnLoad(go);
            _runner = go.AddComponent<Hitstop>();
        }

        /// <summary>seconds 동안 게임을 멈춘다. 이미 멈춰 있으면 그 시간부터 다시 잰다.</summary>
        public static void Play(float seconds)
        {
            if (seconds <= 0f || _runner == null) return;

            // 겹쳐 들어올 때 앞의 코루틴을 살려두면, 그쪽이 먼저 끝나면서 timeScale을 1로
            // 되돌려 뒤의 정지가 통째로 씹힌다. 그래서 항상 앞의 것을 끊고 새로 잰다.
            if (_current != null) _runner.StopCoroutine(_current);
            _current = _runner.StartCoroutine(_runner.Freeze(seconds));
        }

        private IEnumerator Freeze(float seconds)
        {
            Time.timeScale = 0f;

            // timeScale이 0이면 Time.time과 WaitForSeconds가 함께 멈춘다.
            // 스스로 풀려나야 하므로 정지와 무관한 실시간으로 재야 한다.
            yield return new WaitForSecondsRealtime(seconds);

            Time.timeScale = 1f;
            _current = null;
        }
    }
}
