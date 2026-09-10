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

        [Tooltip("최고점까지 올라가는 시간 (sec). 작을수록 튀어오르듯 민첩해진다")]
        public float timeToApex = 0.28f;

        [Tooltip("최고점에서 원래 높이까지 떨어지는 시간 (sec). " +
                 "상승 시간과 독립적이므로 한쪽만 바꿔도 다른 쪽은 그대로 유지된다. " +
                 "이 값이 timeToApex보다 작으면 '빨리 떨어지는' 경쾌한 점프가 된다")]
        public float timeToFall = 0.283f;

        [Header("점프 감각 보정")]

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

        [Header("지상 대시 슬라이드")]
        [Tooltip("슬라이드 출발 속도 (units/sec). maxSpeed보다 충분히 커야 '치고 나간다'는 느낌이 난다")]
        public float dashSpeed = 32f;

        [Tooltip("슬라이드 진입에 필요한 최소 속도 (maxSpeed 대비 비율). 최고속도로 달리는 중에만 " +
                 "슬라이드가 나가므로 '가속해서 붙은 속도를 쓴다'가 된다 — 제자리에서는 못 쓴다. " +
                 "1을 그대로 쓰지 않는 이유는 경사다: 지면 흡착이 vx를 조금씩 흔들어서 " +
                 "정확히 maxSpeed를 요구하면 램프 위에서 슬라이드가 씹힌다")]
        [Range(0f, 1f)]
        public float slideMinSpeedRatio = 0.95f;

        [Tooltip("슬라이드 중 마찰 감속도 (units/sec²). 속도가 maxSpeed까지 떨어지면 슬라이드가 끝난다. " +
                 "고정 지속 시간은 없고 이 값이 슬라이드 길이를 정한다")]
        public float dashDecel = 50f;

        [Tooltip("공중에서 maxSpeed를 넘는 속도(슬라이드로 얻은 모멘텀)가 깎이는 감속도 (units/sec²). " +
                 "작을수록 슬라이드 → 점프 → 슬라이드로 속도가 잘 이어진다. 이 값이 크면 " +
                 "공중에서 속도가 증발해 연계가 끊긴다")]
        public float momentumDecel = 12f;

        [Tooltip("접지 중 모멘텀 감속도 (units/sec²). 내리막에서 번 속도가 평지에서 maxSpeed까지 " +
                 "떨어지는 거리를 정한다 — 거리 = (진입속도² - maxSpeed²) / (2 x 이 값). " +
                 "작을수록 경사에서 번 속도가 멀리까지 실려 나간다. 거리는 속도의 제곱으로 늘어나므로 " +
                 "이 값만 정해두면 '빠를수록 멀리'는 저절로 성립한다. " +
                 "공중값과 나눈 이유는 공중값을 올리면 슬라이드 점프 연계가 끊기기 때문")]
        public float groundMomentumDecel = 35f;

        [Tooltip("대시가 끝난 뒤 다시 쓸 수 있을 때까지의 시간 (sec)")]
        public float dashCooldown = 0.15f;

        [Tooltip("이 수평 속도 이상이면 무적이다 (units/sec). 무적을 슬라이드 '상태'가 아니라 " +
                 "'속도'로 판정한다 — 슬라이드는 마찰·발판 이탈·점프·내리막 종료로 예고 없이 끝나는데, " +
                 "속도는 서서히 줄기만 하므로 무적이 절벽처럼 사라지지 않는다. " +
                 "슬라이드로만 낼 수 있는 속도여야 하니 maxSpeed보다 커야 한다 — " +
                 "작게 잡으면 그냥 걷기만 해도 무적이 된다")]
        public float invincibleSpeed = 12f;

        [Header("과열 — 슬라이드 자원")]
        [Tooltip("과열 스택 수. 이만큼 쌓이면 슬라이드가 막힌다. 0이면 과열이 통째로 꺼진다. " +
                 "게이지(0~1)가 아니라 칸으로 두는 이유는 '몇 번 더 슬라이드할 수 있는가'가 " +
                 "한눈에 보여야 하기 때문 — 서서히 줄어드는 막대는 그 수를 세게 해주지 않는다")]
        public int heatMaxStacks = 4;

        [Tooltip("슬라이드 판정 속도(invincibleSpeed) 이상으로 이만큼 달리면 1스택이 쌓인다 (sec). " +
                 "평지 슬라이드 한 번이 딱 0.4초라 기본값은 '슬라이드 1회 = 1스택'이다. " +
                 "긴 내리막은 그만큼 여러 칸을 먹는다 — 시간으로 세야 오래 빠른 것이 대가를 치른다")]
        public float heatPerStack = 0.4f;

        [Tooltip("판정 속도 아래로 떨어진 뒤 1스택이 식는 시간 (sec). " +
                 "가득 찬 뒤 다시 슬라이드할 수 있게 되기까지가 정확히 이 시간이다 — " +
                 "스택이 하나 비는 순간이 곧 잠금 해제라 '얼마나 식어야 풀리는가'를 따로 둘 필요가 없다")]
        public float heatRecoverTime = 1f;

        [Header("경사")]
        [Tooltip("걸어 올라갈 수 있는 최대 경사각 (도). 이보다 가파르면 경사로 취급하지 않아 " +
                 "수평 속도가 세로 속도로 증폭되며 튀어오르는 일을 막는다")]
        [Range(0f, 80f)]
        public float maxSlopeAngle = 50f;

        [Tooltip("내리막 슬라이드가 도달할 수 있는 속도의 천장 (units/sec). " +
                 "경사 1(45도)당 이만큼 maxSpeed 위로 목표가 올라간다 — 45도면 9 + 이 값. " +
                 "오르막에서는 반대로 목표가 내려가 더 빨리 끝난다. 0이면 경사 가속 없음. " +
                 "주의: 목표(maxSpeed + 경사 x 이 값)가 dashSpeed보다 낮으면 내리막이 가속이 아니라 " +
                 "감속이 된다 — 평지에서 그냥 슬라이드하는 것보다 느려져 경사를 탈 이유가 없어진다")]
        public float slopeDashBonus = 45f;

        [Tooltip("내리막 슬라이드가 목표 속도로 붙는 가속도 (units/sec²). " +
                 "**천장(slopeDashBonus)이 아니라 이 값이 실제 이득을 정한다** — 램프는 짧아서 " +
                 "(45도 4칸 = 0.14초) 천장에 닿기 전에 끝나기 때문이다. " +
                 "마찰(dashDecel)과 따로 두는 이유: 하나로 묶으면 경사 가속을 키울 때 평지 슬라이드 " +
                 "거리가 같이 줄어든다")]
        public float slopeDashAccel = 110f;

        [Header("모서리 보정")]
        [Tooltip("상승 중 머리가 천장 모서리에 걸릴 때 옆으로 밀어줄 최대 거리. 0이면 비활성")]
        public float cornerCorrectionDistance = 0.25f;

        // ── 아래는 위 값에서 계산되는 읽기 전용 값 ──
        // C#의 '식 본문 속성(expression-bodied property)': 저장되는 필드가 아니라
        // 읽을 때마다 계산되는 값이다. C의 매크로 함수와 비슷하다고 보면 된다.

        /// <summary>상승 구간 중력 가속도(양수). h = ½·g·t² 를 g에 대해 푼 값.</summary>
        public float Gravity => (2f * jumpHeight) / (timeToApex * timeToApex);

        /// <summary>점프 시작 속도. v = g·t.</summary>
        public float JumpVelocity => Gravity * timeToApex;

        /// <summary>
        /// 하강 구간 중력 가속도. 상승과 같은 공식이지만 timeToFall로 계산하므로
        /// 상승 속도를 바꿔도 낙하 속도는 영향을 받지 않는다.
        /// </summary>
        public float FallGravity => (2f * jumpHeight) / (timeToFall * timeToFall);
    }
}
