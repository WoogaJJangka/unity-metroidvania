using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Game.Player;

namespace Game.World
{
    /// <summary>
    /// 씬을 넘나들며 살아남는 유일한 전역 객체. 맵 전환과 페이드를 담당한다.
    /// 씬에 배치하지 않는다 — 게임이 시작될 때 코드로 스스로 만들어진다.
    /// 그래야 어느 맵에서 Play를 눌러도 동작한다.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Tooltip("페이드 아웃/인 각각에 걸리는 시간 (초)")]
        public float fadeDuration = 0.25f;

        private Image _fade;
        private bool _travelling;

        /// <summary>맵 전환 중인가. 포털이 중복 발동하지 않게 하는 용도.</summary>
        public bool IsTravelling => _travelling;

        // [RuntimeInitializeOnLoadMethod] = 씬이 로드되기 전에 Unity가 자동 호출하는 진입점.
        // C의 생성자 호출 없이도 실행되는 초기화 코드라고 보면 된다.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("GameManager");
            DontDestroyOnLoad(go);
            go.AddComponent<GameManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BuildFadeOverlay();
        }

        /// <summary>화면 전체를 덮는 검은 이미지를 코드로 만든다. 프리팹을 두지 않기 위함.</summary>
        private void BuildFadeOverlay()
        {
            var canvasGo = new GameObject("FadeCanvas");
            canvasGo.transform.SetParent(transform);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;   // 무엇보다 위에

            var imageGo = new GameObject("FadeImage");
            imageGo.transform.SetParent(canvasGo.transform);

            _fade = imageGo.AddComponent<Image>();
            _fade.color = new Color(0f, 0f, 0f, 0f);
            _fade.raycastTarget = false;

            var rt = _fade.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>대상 씬을 로드하고 지정한 스폰 지점에 플레이어를 놓는다.</summary>
        public void TravelTo(string sceneName, string spawnId)
        {
            if (_travelling) return;
            StartCoroutine(TravelRoutine(sceneName, spawnId));
        }

        // 코루틴: 여러 프레임에 걸쳐 진행되는 함수. yield return 한 지점에서 멈췄다가 다음 프레임에 이어진다.
        private IEnumerator TravelRoutine(string sceneName, string spawnId)
        {
            _travelling = true;

            yield return FadeTo(1f);

            var op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone) yield return null;

            PlacePlayerAt(spawnId);

            yield return FadeTo(0f);

            _travelling = false;
        }

        private IEnumerator FadeTo(float targetAlpha)
        {
            float start = _fade.color.a;
            float t = 0f;
            while (t < fadeDuration)
            {
                // 페이드는 게임 정지(timeScale=0)와 무관해야 하므로 unscaledDeltaTime을 쓴다.
                t += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(start, targetAlpha, t / fadeDuration);
                _fade.color = new Color(0f, 0f, 0f, a);
                yield return null;
            }
            _fade.color = new Color(0f, 0f, 0f, targetAlpha);
        }

        private void PlacePlayerAt(string spawnId)
        {
            var spawn = SpawnPoint.Find(spawnId);
            if (spawn == null)
            {
                Debug.LogError($"[GameManager] 스폰 지점 '{spawnId}'를 새 씬에서 찾지 못했습니다. 플레이어를 옮기지 않습니다.");
                return;
            }

            var player = FindAnyObjectByType<PlayerController>();
            if (player == null)
            {
                Debug.LogError("[GameManager] 새 씬에 PlayerController가 없습니다.");
                return;
            }

            var rb = player.GetComponent<Rigidbody2D>();
            rb.position = spawn.transform.position;
            rb.linearVelocity = Vector2.zero;
            // Rigidbody2D는 물리 스텝에서 위치가 반영되므로 transform도 같이 맞춰
            // 이번 프레임에 카메라가 옛 위치를 잡는 것을 막는다.
            player.transform.position = spawn.transform.position;
        }
    }
}
