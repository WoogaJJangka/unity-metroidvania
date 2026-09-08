using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace Game.Utils
{
    /// <summary>
    /// 테스트 맵 전용 이동 도구. 숫자키 1~9로 각 구간(ZoneStart_N) 시작점에 순간이동한다.
    /// 맵이 580유닛이라 걸어 다니면 구간 하나 확인하는 데만 한참 걸린다.
    ///
    /// 입력은 키보드 장치를 직접 읽는다. InputActions에 액션을 새로 만들지 않는다 —
    /// 디버그 도구 때문에 전역 입력 에셋을 건드릴 이유가 없다.
    /// </summary>
    public class DebugWarp : MonoBehaviour
    {
        private Transform[] _stops;
        private Transform _player;
        private Rigidbody2D _rb;

        private void Start()
        {
            var found = new System.Collections.Generic.List<Transform>();
            for (int i = 1; i <= 9; i++)
            {
                var go = GameObject.Find("ZoneStart_" + i);
                if (go != null) found.Add(go.transform);
            }
            _stops = found.ToArray();

            var p = GameObject.Find("Player");
            if (p == null) return;
            _player = p.transform;
            _rb = p.GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || _player == null || _stops == null) return;

            for (int i = 0; i < _stops.Length; i++)
            {
                if (!kb[Key.Digit1 + i].wasPressedThisFrame) continue;

                Vector3 to = _stops[i].position;
                Vector3 delta = to - _player.position;
                _player.position = to;
                if (_rb != null)
                {
                    _rb.position = to;
                    _rb.linearVelocity = Vector2.zero;
                }
                // 알려주지 않으면 카메라가 댐핑으로 맵 전체를 가로질러 날아온다.
                CinemachineCore.OnTargetObjectWarped(_player, delta);
                return;
            }
        }
    }
}
