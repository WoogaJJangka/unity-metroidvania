using System.Collections.Generic;
using UnityEngine;
using Game.Combat;
using Game.Player;

namespace Game.Utils
{
    /// <summary>
    /// 테스트 맵 전용 화면 표시. 속도 비례 피해는 눈으로 볼 수가 없어서 —
    /// 체력 UI는 Phase 5고, 적은 그냥 사라진다 — 마지막으로 들어간 피해와
    /// 그때의 속도를 같이 띄운다. 둘을 나란히 봐야 "빠르면 아프다"가 확인된다.
    ///
    /// OnGUI는 느리지만 디버그 표시에는 충분하다. HUD를 만들 때 통째로 지운다.
    /// </summary>
    public class DebugStats : MonoBehaviour
    {
        private PlayerController _player;
        private Rigidbody2D _rb;
        private Health _playerHealth;
        private readonly List<Health> _watched = new List<Health>();
        private string _lastHit = "-";
        private GUIStyle _style;

        private void Start()
        {
            _player = FindAnyObjectByType<PlayerController>();
            if (_player == null) { enabled = false; return; }
            _rb = _player.GetComponent<Rigidbody2D>();
            _playerHealth = _player.GetComponent<Health>();

            // 적이 맞을 때마다 알림을 받는다. 플레이어 자신은 뺀다.
            foreach (var h in FindObjectsByType<Health>(FindObjectsInactive.Exclude))
            {
                if (h.gameObject == _player.gameObject) continue;
                _watched.Add(h);
                h.Damaged += OnEnemyDamaged;
            }
        }

        // 구독은 반드시 짝을 맞춰 해제한다 (CLAUDE.md 규칙).
        // 죽은 적은 파괴되어 가짜 null이 되므로 != null 로 명시 비교한다.
        private void OnDisable()
        {
            foreach (var h in _watched)
                if (h != null) h.Damaged -= OnEnemyDamaged;
        }

        private void OnEnemyDamaged(DamageInfo info)
        {
            float vx = Mathf.Abs(_rb.linearVelocity.x);
            _lastHit = $"피해 {info.amount:F2}  @ vx {vx:F1}";
        }

        private void OnGUI()
        {
            if (_style == null)
                _style = new GUIStyle(GUI.skin.label) { fontSize = 28, normal = { textColor = Color.white } };

            float vx = _rb.linearVelocity.x;
            string inv = _player.AtSlideSpeed ? "무적/공격 ON" : "off";
            GUI.Label(new Rect(12f, 8f, 720f, 34f),
                      $"vx {vx,7:F2}    {inv}    HP {_playerHealth.Current:F1}", _style);
            GUI.Label(new Rect(12f, 40f, 720f, 34f), $"마지막 타격: {_lastHit}", _style);
        }
    }
}
