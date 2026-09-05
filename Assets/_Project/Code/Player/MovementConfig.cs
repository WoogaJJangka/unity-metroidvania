using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// 플레이어 이동의 모든 튜닝 수치를 담는 에셋.
    /// ScriptableObject는 "씬에 존재하지 않는 데이터 덩어리"다. 프로젝트 창의 에셋 파일로 저장되므로
    /// 플레이 중에 값을 바꿔도 되돌아가지 않는다 → 조작감 튜닝에 필수.
    /// (일반 MonoBehaviour 필드는 플레이 종료 시 값이 원래대로 돌아간다.)
    /// </summary>
    [CreateAssetMenu(fileName = "MovementConfig", menuName = "Game/Movement Config")]
    public class MovementConfig : ScriptableObject
    {
        [Header("수평 이동")]
        [Tooltip("최고 이동 속도 (units/sec)")]
        public float maxSpeed = 9f;

        [Tooltip("지상 가속도. 클수록 즉각 반응 (units/sec²)")]
        public float groundAccel = 90f;

        [Tooltip("지상 감속도. 클수록 빨리 멈춤")]
        public float groundDecel = 110f;

        [Tooltip("공중 가속도. 지상보다 낮게 두면 공중 제어가 무겁게 느껴진다")]
        public float airAccel = 60f;

        [Tooltip("공중 감속도")]
        public float airDecel = 40f;

        [Tooltip("진행 방향과 반대로 입력했을 때 가속 배수. 방향 전환을 날카롭게 만든다")]
        public float turnAccelMultiplier = 2f;

        [Header("점프 — 높이와 시간으로 지정")]
        [Tooltip("최대 점프 높이 (units). 중력과 점프 속도는 이 값에서 자동 계산된다")]
        public float jumpHeight = 3.2f;

        [Tooltip("최고점까지 걸리는 시간 (sec). 작을수록 민첩하고 클수록 붕 뜬 느낌")]
        public float timeToApex = 0.38f;

        [Header("점프 감각 보정")]
        [Tooltip("하강 시 중력 배수. 1보다 크면 올라갈 때보다 빨리 떨어져 경쾌해진다")]
        public float fallGravityMultiplier = 1.8f;

        [Tooltip("점프 버튼을 일찍 떼면 상승 속도에 곱하는 값. 작을수록 짧은 점프가 잘 된다")]
        [Range(0f, 1f)]
        public float jumpCutMultiplier = 0.5f;

        [Tooltip("이 속도 이하이면 '최고점 부근'으로 간주 (units/sec)")]
        public float apexThreshold = 2.5f;

        [Tooltip("최고점 부근에서의 중력 배수. 1보다 작으면 정점에서 체공하는 느낌이 난다")]
        public float apexGravityMultiplier = 0.5f;

        [Tooltip("최고점 부근에서 더해지는 수평 속도. 공중 제어에 여유를 준다")]
        public float apexBonusSpeed = 1.5f;

        [Tooltip("최대 낙하 속도 제한. 없으면 높은 곳에서 통제 불능이 된다")]
        public float maxFallSpeed = 22f;

        [Header("입력 보정 — 없으면 '조작이 씹힌다'고 느껴진다")]
        [Tooltip("발판에서 떨어진 뒤에도 점프를 허용하는 시간 (sec)")]
        public float coyoteTime = 0.1f;

        [Tooltip("착지 직전 누른 점프를 기억하는 시간 (sec)")]
        public float jumpBufferTime = 0.15f;

        [Header("접지 판정")]
        [Tooltip("발밑 판정 박스의 크기")]
        public Vector2 groundCheckSize = new Vector2(0.45f, 0.12f);

        [Tooltip("콜라이더 바닥에서 판정 박스를 얼마나 내릴지")]
        public float groundCheckOffset = 0.02f;

        [Tooltip("접지 중 지면에 붙여두는 아래 방향 속도. 0으로 두면 판정이 후하게 잡힐 때 " +
                 "공중에 뜬 채로 굳는다. 경사나 이음새에서 튀지 않게 하는 역할도 한다")]
        public float groundStickSpeed = 2f;

        [Header("모서리 보정")]
        [Tooltip("상승 중 머리가 천장 모서리에 걸릴 때 옆으로 밀어줄 최대 거리. 0이면 비활성")]
        public float cornerCorrectionDistance = 0.25f;

        // ── 아래는 위 값에서 계산되는 읽기 전용 값 ──
        // C#의 '식 본문 속성(expression-bodied property)': 저장되는 필드가 아니라
        // 읽을 때마다 계산되는 값이다. C의 매크로 함수와 비슷하다고 보면 된다.

        /// <summary>중력 가속도(양수). h = ½·g·t² 를 g에 대해 푼 값.</summary>
        public float Gravity => (2f * jumpHeight) / (timeToApex * timeToApex);

        /// <summary>점프 시작 속도. v = g·t.</summary>
        public float JumpVelocity => Gravity * timeToApex;
    }
}
