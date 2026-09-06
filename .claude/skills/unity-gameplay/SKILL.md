---
name: unity-gameplay
description: >
  이 프로젝트의 게임플레이 시스템 구현 규칙 — 플레이어 이동·점프 감각 보정, 접지 판정, 전투
  히트박스와 타격감, 적 AI 상태머신, 능력 해금 게이팅, 퀘스트·미니게임 연결. 플레이어/적/전투/
  능력 관련 스크립트를 만들거나 고칠 때, 조작감이나 타격감이 이상할 때 사용한다. 수치 자체를
  튜닝하는 일이 아니라 시스템을 어떻게 짜기로 했는지를 담는다.
---

# 게임플레이 시스템 규칙

> **이 문서와 코드가 어긋나면 코드가 옳다.**
> 마지막 갱신: 2026-09-06 (Phase 1 진행 중)

## ⚠️ 조작감 수치는 자동으로 건드리지 않는다

`MovementConfig`의 값(점프 높이, 코요테 타임 등)은 **사용자가 직접 만지는 영역**이다.
"점프가 기분 좋은가"는 사람만 판단할 수 있다. 요청이 없으면 수치를 바꾸지 않는다.
바꾸는 건 **구조**이지 **숫자**가 아니다.

## 이동 (Phase 1 — 구현 중)

`Assets/_Project/Code/Player/` — `PlayerController` + `MovementConfig`(SO)

### 점프 수치가 파생되는 방식

`MovementConfig`는 점프를 **속도와 중력이 아니라 "높이와 시간"으로** 지정한다. 중력·점프속도는
거기서 계산된다. 이렇게 해야 "더 높이"와 "더 빠르게"를 따로 조절할 수 있다.

```
Gravity     = 2·jumpHeight / timeToApex²      (상승 구간, h = ½gt² 를 g에 대해 푼 것)
JumpVelocity= Gravity · timeToApex
FallGravity = 2·jumpHeight / timeToFall²      (하강 구간 — 상승과 독립)
```

**상승과 하강에 다른 중력을 쓴다.** `timeToFall < timeToApex` 로 두면 빨리 떨어지는 경쾌한 점프가
된다. 한쪽을 바꿔도 다른 쪽이 안 흔들리는 게 이 구조의 목적이다.

### 감각 보정 — 없으면 "조작이 씹힌다"고 느껴진다

| 보정 | 하는 일 | 구현 위치 |
|---|---|---|
| 코요테 타임 | 발판에서 떨어진 뒤에도 잠깐 점프 허용 | `Update`에서 타이머 감소 |
| 점프 버퍼 | 착지 직전에 누른 점프를 기억했다 발동 | `Update`에서 입력 저장, 착지 시 소비 |
| 가변 점프 | 버튼을 떼면 상승 속도에 `jumpCutMultiplier` 곱함 | 버튼 릴리즈 시 1회 |
| 정점 체공 | 최고점 부근(`apexThreshold` 이하)에서 중력 감소 + 수평 속도 보너스 | `FixedUpdate` |
| 낙하 제한 | `maxFallSpeed`로 클램프 | `FixedUpdate` |
| 모서리 보정 | 상승 중 머리가 천장 모서리에 걸리면 옆으로 밀어줌 | `FixedUpdate` |
| 접지 흡착 | 접지 중 `groundStickSpeed`로 아래로 눌러둠 | 0이면 경사·이음새에서 튄다 |

### 접지 판정

`Physics2D.OverlapBox` + `LayerMask("Ground")`. 크기는 `groundCheckSize`, 콜라이더 바닥에서
`groundCheckOffset` 만큼 내린 위치. **`FixedUpdate`에서 판정한다.**

## 전투 (Phase 3 — 예정)

- `IDamageable` 인터페이스 + `DamageInfo`(데미지, 넉백 방향, 공격 주체)
- 히트박스/허트박스는 Trigger `Collider2D`를 **레이어로 분리** (`PlayerHitbox`, `EnemyHitbox`)
- **타격감 3종 세트 — 이게 없으면 공격이 허공을 젓는 느낌이 난다**
  1. 히트스톱: 명중 순간 `Time.timeScale`을 수 프레임 정지
  2. 넉백: 공격자·피격자 **양쪽에**
  3. 화면 흔들림: Cinemachine Impulse
- 무적 시간(i-frame) + 스프라이트 깜빡임
- 적 상태머신: `enum` 기반 FSM (Idle/Patrol/Chase/Attack/Hurt/Dead).
  **처음부터 Behaviour Tree를 도입하지 않는다.**
- `EnemyConfig`(SO)로 적 종류를 데이터로 늘린다

## 능력 해금 & 게이팅 (Phase 4 — 예정)

메트로배니아의 실제 설계 도구. 세계를 "넓게" 만드는 게 아니라 **능력으로 잠그고 여는** 것이다.

- 능력: 대시 / 이중점프 / 벽점프·벽타기 / 글라이드
- `AbilityFlags`(enum flags)로 획득 여부를 비트 관리 → 세이브에 그대로 실림
- 게이팅 지형: 해당 능력 없이는 못 넘는 높이·틈·벽을 배치해 동선을 통제
- 능력 획득 연출이 곧 보상감

**새 능력을 추가할 때 빠뜨리기 쉬운 등록 단계** (Phase 4에서 확정되면 여기를 체크리스트로 채운다):
- [ ] `AbilityFlags`에 비트 추가
- [ ] 세이브 필드 반영
- [ ] 게이트 지형 배치
- [ ] 획득 연출

## 아직 정해지지 않은 것

- [ ] **Phase 2** — 레이어 목록 확정: Ground, Player, Enemy, PlayerHitbox, EnemyHitbox, Interactable + `Physics2D` 충돌 매트릭스
- [ ] **Phase 3** — 히트스톱을 `Time.timeScale`로 할지 개별 애니메이터 정지로 할지 (timeScale은 UI·연출까지 멈춘다)
- [ ] **Phase 6** — 퀘스트 목표 타입: 수집/처치/도달/대화/미니게임 클리어
- [ ] **Phase 7** — `IMiniGame` 결과를 퀘스트에 반환하는 경로
