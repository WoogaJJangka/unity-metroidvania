# gmae_project — 2D 메트로배니아

## 프로젝트 개요
- **장르**: 2D 메트로배니아. 맵 전환형 오픈월드(메이플스토리식) + 오리/할로우나이트의 조작감·분위기
- **범위**: 전투, 탐험, 능력 해금 게이팅, 스토리 퀘스트, 미니게임
- **엔진**: Unity 6000.6.0f1 / Universal 2D 템플릿 (URP 2D Renderer)
- **게임 기획서(최상위 문서)**: `C:\Users\siwon\Documents\게임 개발 기획서-gmae_project.docx`
  - **핵심**: 슬라이딩 하나가 이동·공격·회피를 전부 한다. 공격 버튼에 의존하지 않는 것이 이 게임의 정체성
  - 로그라인: 의족을 단 우편배달부가 슬라이딩으로 지형과 적을 돌파하며 소포의 사연을 전한다
  - 미구현 축: **과열(過熱)** 자원(무한 슬라이딩 방지, 냉각/가열 지형), **패링**(투사체를 슬라이딩으로 튕김), **슬라이딩 개조**(Phase 4 해금의 실체)
- **개발 로드맵**: `C:\Users\siwon\.claude\plans\atomic-noodling-sunbeam.md`
  (맨 끝 "기획서 반영 — 로드맵 개정" 절이 현재 순서다. 그 위 Phase 3~5 본문은 개정 전 내용)
- **스토리 노트**: `STORY.md` — 인형 주인공/전쟁 후 시골 마을 아크 (초안, 기획서와 통합 전)
- **개발자**: C 언어 경험 있음, C#·Unity 입문. 새 개념이 나오면 한 줄 설명을 덧붙일 것.

## 폴더 규칙
```
Assets/
  _Project/            ← 우리가 만든 모든 것
    Art/               Sprites, Tilesets, Animations, UI
    Audio/             BGM, SFX
    Code/              Player, Enemy, Combat, World, Quest, UI, Save, Utils
    Data/              ScriptableObject 에셋 (MovementConfig, EnemyConfig, QuestSO …)
    Editor/            에디터 전용 (TestMapBuilder 등). 빌드에 안 들어간다
    Prefabs/           Player, Enemies, Props, UI
    Scenes/            Maps/, MiniGames/
  Settings/            URP 에셋 + InputSystem_Actions (템플릿 생성 — 경로 이동 금지)
  ThirdParty/          외부 무료 에셋(Kenney 등). 우리 코드와 절대 섞지 않는다.
  Welcome/             템플릿 튜토리얼. 건드리지 않는다.
```
- 새 파일은 항상 `Assets/_Project/` 아래에 만든다.
- **아트는 보류다.** 게임플레이 시스템이 먼저 선다. 실제 스프라이트·타일·배경을 만들지 않고 흰 사각형으로 계속 개발한다. 픽셀 규격(해상도, 타일 크기, PPU)도 아직 정하지 않는다 — 지금 정하면 시스템이 바뀔 때마다 다시 정하게 된다.
- 그래픽 작업을 실제로 시작할 때가 되면 먼저 `Assets/_Project/Art/ART_DIRECTION.md`를 읽는다. 이 문서가 픽셀 규격, 스타일, 에셋 이름, AI 작업 요청 형식의 기준이다.
- `Assets/Settings/`의 URP·Input 에셋은 **이동·이름 변경 금지** (프로젝트 설정이 GUID로 참조 중).

## 코딩 규칙
- 네임스페이스: `Game.<영역>` (예: `Game.Player`, `Game.Combat`, `Game.Quest`)
- 클래스·메서드 `PascalCase`, 지역변수·매개변수 `camelCase`, private 필드 `_camelCase`
- **수치를 코드에 박지 않는다.** 이동 속도, 체력, 쿨다운 등 튜닝 대상 값은 전부 ScriptableObject(`Assets/_Project/Data/`)로 노출해 플레이 중 조정 가능하게 만든다.
- 입력 처리는 `Update`, 물리(Rigidbody2D) 처리는 `FixedUpdate`. 이 구분을 어기지 않는다.
- 시스템 간 결합은 `Action`/`event`로 끊는다. 매니저를 직접 참조하는 코드를 늘리지 않는다.
- `GetComponent`는 `Awake`에서 캐싱한다. `Update` 안에서 호출하지 않는다.
- 파괴된 UnityEngine.Object는 `== null`이 true지만 실제 null이 아니다(가짜 null). `?.`/`??`를 Unity 오브젝트에 쓰지 않는다.

## 씬 구조
- `_Bootstrap` 씬에 `GameManager`를 두고 `DontDestroyOnLoad`로 유지 → 어느 씬에서 Play해도 동작
- 맵 1개 = 씬 1개 (`Assets/_Project/Scenes/Maps/`)
- 미니게임은 Additive 로드 (`Assets/_Project/Scenes/MiniGames/`)
- 맵 이동은 포털 트리거 → `{대상 씬 이름, 스폰 포인트 ID}`

## 작업 방식
- 스크립트 작성 후 **반드시 Unity 콘솔을 읽어 컴파일 에러를 확인**한다. 컴파일 성공 ≠ 동작.
- 실제 동작 확인은 Play 모드에서 한다.
- 조작감 수치(점프 높이, 코요테 타임 등) **미세 튜닝은 자동화하지 않는다.** 사용자가 직접 만져야 하는 영역.
- 세이브 직렬화·퀘스트 상태 전이처럼 물리와 무관한 로직은 Unity Test Framework(EditMode)로 테스트한다.
- 각 Phase 완료 시 git 태그를 남긴다 (`phase-1-movement` 등).

## 저장소
- 원격: https://github.com/WoogaJJangka/unity-metroidvania (Public)
- `main`은 항상 동작하는 상태를 유지한다. 작업은 `feature/*` 브랜치에서 하고 완료 후 `main`에 병합한다.
- 커밋 이메일은 `49576850+WoogaJJangka@users.noreply.github.com` (공개 저장소이므로 실제 이메일을 쓰지 않는다). 저장소 로컬 설정에 이미 지정되어 있다.

## 현재 상태 (2026-09-07 기준)
- **Phase 0 (환경 준비) 완료** — 태그 `phase-0-setup`
- **Phase 1 (조작감) 완료** — 태그 `phase-1-movement`. 점프·수평 이동 모두 사용자 확인 받음.
  최종값: `maxSpeed 9`, `groundAccel 110`, `groundDecel 60`, `airAccel 100`, `airDecel 40`,
  `timeToApex 0.28`, `timeToFall 0.2832`, `apexBonusSpeed 0`
- **Phase 2 (맵 전환) 진행 중** — 브랜치 `feature/world`
  - ✅ 맵 전환 코어 완료: `GameManager`(자체 생성 + 페이드 + 비동기 로드), `Portal`, `SpawnPoint`.
    A↔B 왕복과 스폰 위치 복원을 Play 모드로 실측 검증
  - ✅ **타일맵 착수** — 아래 "아트 파이프라인" 참고. 사용자가 직접 그린 타일로 해결
  - ⏸ **Sorting Layer 보류** — 배경 아트가 아직 없다. 배경이 생기면 정한다
  - ✅ **지형 그림 전부 흙 타일로 교체** (2026-09-08) — 아래 "지형 아트 적용" 참고.
    **콜라이더 주체는 여전히 박스다.** `TilemapCollider2D`는 쓰지 않는다 — 그림만 타일/타일 스프라이트로
    바꾸고 물리는 손대지 않았다. 이 분리가 경사 램프(회전 박스)와의 공존 답이었다

- **아트 파이프라인 확립** (2026-09-07) — Play 모드 실측 검증. 상세는 `Assets/_Project/Art/ART_DIRECTION.md`
  - **픽셀 규격 전부 확정**: 타일 `16x16` / PPU `16` / **1 타일 = 1 유닛** /
    논리 해상도 `384x216` / 필터 `Point` / Mipmap 해제 / 무압축
  - 사용자가 직접 그림. `TILE_Test_Ground.png`(48x48 = 16x16 x 9칸), `CHR_Player_Idle.png`(32x32)
  - `RT_Ground.asset` — 상하좌우 4방향 이웃만 보는 규칙 9개 (모서리 4 + 엣지 4 + 채움 1).
    오목(안쪽) 모서리 4장은 실제 맵에서 어색해지면 추가한다
  - `PixelPerfectCamera`(16ppu / 384x216 / PixelSnapping / cropFrame None)
    + `CinemachinePixelPerfect` 확장. vcam lens ortho `6.75`
  - 캐릭터 스프라이트는 **세로 32px = 2유닛**, 피벗 하단 중앙, 콜라이더는 `1.8u`(28.8px) 유지.
    아트가 콜라이더보다 0.2유닛 큰 것은 의도한 차이다
  - 실측: `pixelRatio` 정수 유지, 카메라를 0.32 텍스처픽셀 옮겨도 렌더 결과가 **MD5 일치**
    (픽셀 스냅 작동), 1 텍스처픽셀 이동은 정확히 추종. 흐림·떨림 없음

- **슬라이드(지상 대시) 완료** — 사용자 확인 받음. Apex Legends식 모멘텀 슬라이드.
  최종값: `dashSpeed 32`, `dashDecel 50`, `momentumDecel 12`, `dashCooldown 0.15`
  - 입력은 템플릿에 이미 있던 `Sprint`(LeftShift / 좌스틱 클릭) 재사용. 새 액션 안 만듦
  - **고정 지속 시간이 없다.** `dashDecel` 마찰로 `maxSpeed`까지 떨어지거나 발판을 벗어나면 끝난다
  - **모멘텀 보존이 핵심.** `ApplyHorizontal`은 `maxSpeed` 초과분을 `momentumDecel`로만 깎는다.
    이 분기가 없으면 슬라이드 점프 속도가 `airAccel` 100에 끌려 0.2초 만에 증발해 연계가 성립하지 않는다
  - 진입 속도 `Max(dashSpeed, |현재 vx|)` → 슬라이드 → 점프 → 착지 → 슬라이드로 속도가 이어진다
  - 슬라이드 입력도 버퍼를 쓴다 (`jumpBufferTime` 공유). 착지 프레임을 맞춰야 하면 연계가 안 된다
  - 실측: 단독 슬라이드 32.0 → 0.46s → 9.52u. 연계 시 공중 감쇠 12/s²로 21.3 → 15.5, 재진입 32.0
  - ✅ **경사 가속 완료** — 아래 "경사 지원" 참고. 타일맵을 기다리지 않고 회전 박스 콜라이더로 해결

- **경사(slope) 지원 완료** (2026-09-06) — Play 모드 실측 검증
  - 램프는 타일 아트 없이 **회전한 박스 콜라이더**로 만든다. 타일맵을 기다릴 이유가 없었다
  - `maxSlopeAngle 50` (걸어 올라갈 수 있는 최대 각), `slopeDashBonus` (내리막 슬라이드 속도 천장) 추가
    — 현재값은 `slopeDashBonus 45` / `slopeDashAccel 110` / `groundMomentumDecel 35`
  - 접지 중 속도는 지면이 정한다: `vx` 그대로 + `vy = vx * tan(경사)`, 여기에 지면으로 눌러붙이는
    `groundStickSpeed`를 더한다. **누르는 방향은 월드 아래가 아니라 법선 반대여야 한다** (아래 참고)
  - 내리막 슬라이드는 목표 속도가 `maxSpeed - grade * slopeDashBonus`로 올라가 죽지 않는다.
    (붙는 속도는 2026-09-08에 `slopeDashAccel`로 분리했다 — 아래 "경사 슬라이드 보상" 참고)
  - 실측: 40° 내리막 걷기 `vx 9.000 / vy -7.500` (= 9·tan40°) 공중 프레임 0,
    32° 오르막 `vx 9.000 / vy +5.625` 손실 0, 40° 내리막 슬라이드가 정확히 25.667로 수렴
    (= `9 + 0.833 x 20`), `maxSlopeAngle`을 30으로 낮추면 40° 램프가 평지 취급되고 `vx`는 9를 안 넘음

- **Phase 3 (전투 코어) 진행 중** — 브랜치 `feature/world`
  - ✅ **때리는 쪽 완료** — Play 모드 실측 검증. `IDamageable`/`DamageInfo`, `Health`, `Hitbox`,
    `Hitstop`, `PlayerAttack` + `AttackConfig`/`CombatConfig`
  - 입력은 템플릿에 이미 있던 `Attack`(마우스 좌클릭 / 게임패드 X / Enter) 재사용. 새 액션 안 만듦
  - 입력 에셋을 또 `Instantiate`하지 않는다. `PlayerController.PlayerMap`으로 같은 복사본을 공유한다.
    복사본이 둘이 되면 한쪽만 Enable된 채 남는다
  - 히트박스는 **레이어로 거른다.** 누가 누구를 때리는지는 코드가 아니라 Physics2D 충돌 매트릭스가 정한다
    (`PlayerHitbox x Enemy`, `EnemyHitbox x Player`만 열려 있고 나머지는 전부 닫힘)
  - `Hitbox`는 `OnTriggerEnter2D`와 `OnTriggerStay2D`를 **둘 다** 받는다. Enter만 쓰면 켜지는 순간
    이미 겹쳐 있던 대상을 놓치고, Stay만 쓰면 상대 Rigidbody2D가 잠들었을 때 호출이 끊긴다.
    중복은 `_hitThisSwing` HashSet이 막는다 (한 번 휘두르기에 한 대상 한 번)
  - 실측: 넉백 1.200u (예측 `kbSpeed²/(2·decay)` = 1.25), 3타에 사망, 한 번의 타격에서
    `timeScale` 최저 0.000(히트스톱)과 카메라 이탈 0.347u(화면 흔들림)를 동시에 확인
  - ✅ **주 공격 = 슬라이드로 전환 완료** (2026-09-07). 히트박스를 켜는 주체가 `Attack` 버튼에서
    `PlayerController.IsDashing`으로 바뀌었다. 슬라이드에는 고정 지속 시간이 없으므로(마찰로 끝난다)
    코루틴으로 흉내 내지 않고 상태를 매 프레임 따라간다. 켜지는 순간이 `Hitbox`의 "휘두르기 한 번"
    경계라 한 슬라이드에 같은 적을 한 번만 때린다
  - ✅ **보조 공격 = 거리 확보용 밀치기.** 킬 루트가 되면 안 되므로 damage 0.25 / knockbackSpeed 18.
    `PlayerController`가 `IsAttacking`을 읽지 않아 **밀치기 → 즉시 슬라이드 연계가 코드 없이 성립**한다
  - ✅ **맞는 쪽 완료** (2026-09-07) — Play 모드 실측 검증
    - `Health.Invincible`(외부가 켜는 무적) + `PlayerAttack`이 슬라이드 상태를 그대로 옮긴다 = **공방일체**
    - `IDamageable.TakeDamage`가 `bool`을 반환한다. 무적으로 씹힌 타격에 히트스톱·화면 흔들림이 나가면
      "무적으로 뚫었다"가 "맞았다"로 읽힌다
    - `Hitbox.continuous` — 적의 몸통 접촉 피해처럼 **계속 켜져 있는 판정**은 켜짐/꺼짐 경계가 없어
      `_hitThisSwing` 기록을 쓰면 한 번 때리고 영영 못 때린다. 켜면 기록을 건너뛰고 연타는 맞는 쪽 무적이 막는다
    - `PlayerController.ApplyHorizontal`은 `Health.IsKnockedBack` 동안 조기 반환한다. 없으면 넉백 속도가
      다음 물리 스텝에 `groundAccel`로 지워져 맞은 티가 전혀 안 난다
    - `PlayerDeath` — 죽으면 현재 씬 재로드. 체크포인트·페이드 없는 최소 구현(Phase 5에서 교체)
    - 실측: 걸어서 접촉 → HP 5→4, 넉백 vx -6.80 / **슬라이드로 통과 → 플레이어 HP 유지, 적 HP 3→2**,
      슬라이드 최고 vx 32.0
  - ✅ **적 AI 완료** (2026-09-07) — Play 모드 실측 검증. `EnemyConfig` + `EnemyAI`(enum FSM) + `Projectile`
    - 상태는 `Patrol / Chase / Attack / Hurt / Dead` 다섯 개. Idle은 안 만들었다 — `patrolSpeed 0`이
      곧 제자리 지키기라서 상태를 하나 더 둘 이유가 없다
    - **근접형과 원거리형이 한 스크립트다.** 갈리는 곳은 `projectile` 필드 하나 —
      비어 있으면 계속 달려들고(피해는 몸통 `ContactHitbox`), 채워져 있으면 `attackRange`에서 멈춰 쏜다.
      클래스를 나누면 감지·순찰·넉백 처리가 통째로 복사된다
    - **`detectRange`와 `loseRange`를 따로 둔다(8 / 12).** 같으면 경계선에서 추격/순찰이 매 프레임
      번갈아 바뀌며 덜덜 떤다
    - **`chaseSpeed 5`는 플레이어 `maxSpeed 9`보다 느리다.** 뿌리칠 수 있어야 슬라이드가 답이 된다.
      적은 "붙으면 아픈 벽"이지 "가로막는 벽"이 아니다 — Player x Enemy 충돌은 꺼져 있어 통과한다
    - 순찰 경로를 씬에 찍지 않는다. 벽 광선과 발밑 낭떠러지 광선으로 스스로 돌아선다
    - `Projectile`은 이동과 수명만 맡는다. 피해·넉백·타격감은 같은 오브젝트의 `Hitbox`가 그대로 한다.
      `origin`을 비워 두면 탄 자신이 기준이 되어 넉백 방향이 저절로 맞는다.
      `blockLayer`에 **Ground와 Player를 둘 다** 넣는다 — 맞는 쪽이 빠지면 무적으로 뚫고 지나간 탄이
      남아 뒤에서 다시 때린다. `EnemyHitbox x Ground` 충돌 칸도 열어야 벽에 막힌다 (기본값은 닫힘)
    - 실측: 거리 17.4(loseRange 밖) → `Patrol` vx -2.00 / 거리 9 → `Chase`로 전환해 5 u/s로 접근 /
      사수는 `Attack`에서 vx 0.00으로 멈춰 발사, 탄이 플레이어 HP 2→1로 깎고 사라짐
  - ⏸ **Phase 3 남은 것** — `EnemyConfig`를 쓰는 적 프리팹화(지금은 씬 오브젝트 2기),
    적 사망 연출, 보스
  - ⏸ **과열(過熱) 보류** — 적 AI 뒤에 온다. "몇 번 슬라이드하면 막히는가"는 적과 싸워봐야 정해지고,
    먼저 만들면 숫자를 감으로 박은 뒤 전부 다시 맞추게 된다

- **화면 떨림 해결 + 테스트 맵 전면 개편** (2026-09-08) — Play 모드 실측 검증
  - `CameraPixelLock`(CinemachineExtension, `Code/World/`) — 픽셀 퍼펙트 떨림의 실제 원인과 해법.
    아래 "오늘 겪은 것" 참고. `CM Player Camera`에 붙어 있다
  - `Player/Visual` 로컬 y를 `-0.9` → `-0.875`(= -14/16)로 옮겼다. 픽셀 격자 위에 올려 세로 떨림을 없앤다
  - **카메라 최종값** (`CM Player Camera`) — 경사 출렁임과 시야를 같이 잡은 결과.
    `Damping (0.4, 0, 0)` / `DeadZone (0.18, 0.5)` / `Lookahead 0.15` / `vcam lens ortho 9.0`.
    세로 댐핑 0과 큰 세로 데드존이 한 쌍이다 — 아래 "오늘 겪은 것"의 경사 항목을 읽지 않고 건드리지 말 것
  - **`PlayerController.ClimbContactNormal` 추가** — 45도 램프 진입에서 영구 정지하던 것을 고쳤다.
    아래 "오늘 겪은 것" 참고
  - **테스트 맵을 코드로 굽는다.** `Assets/_Project/Editor/TestMapBuilder.cs`가 원본이다.
    씬을 손으로 고치지 말고 여기 숫자를 고친 뒤 메뉴 `Tools/테스트 맵 다시 굽기`를 누른다
  - 폭 580유닛 / 구간 5개: `1 기본`(계단·천장 틈) `2 점프`(구덩이 2~6, 단 1~5)
    `3 슬라이드`(구덩이 9·7·7, 낮은 천장) `4 경사45`(4단 언덕·V자 골짜기·8단 큰 경사)
    `5 적`(사수 데크·엄폐물, 근접 2 + 사수 2). 구간 사이는 2유닛 틈으로 끊어 적이 넘어가지 못한다
  - **경사는 45도만 쓴다.** 램프 **콜라이더**는 회전 박스이고 낮은 쪽 끝은 항상 바닥 안에 2유닛 묻는다
    (묻는 건 콜라이더뿐이다 — 그림은 2026-09-08부터 타일맵이 그린다. 아래 "지형 아트 적용" 참고)
  - 구덩이에 빠지면 `y=-4`의 회수 바닥이 받는다. 낭떠러지로 두면 떨어진 뒤 아무것도 못 한다
  - `DebugWarp`(`Code/Utils/`) — 숫자키 1~5로 각 구간 시작점 순간이동.
    `CinemachineCore.OnTargetObjectWarped`를 같이 불러야 카메라가 맵을 가로질러 날아오지 않는다
  - 실측: 45도 오르막 걷기 `vx 9.0 / vy +6.5~7.6` 정지 없음, 45도 내리막 걷기 `vx 7.586 / vy -10.414`,
    45도 내리막 슬라이드 최고 `vx 34.97`(램프 8칸), 램프 정상에서 0.47유닛 튀어 오름(가속 이탈 — 의도대로 둔다)

- **경사 슬라이드 보상** (2026-09-08) — 사용자 요청. "경사에서 슬라이딩 이점이 없다"가 실제로 맞았다.
  Play 모드 실측(60fps 고정, 진입 지점·슬라이드 시점 동일)
  - **원인은 천장이 아니라 붙는 속도였다.** `slideTarget`(=`maxSpeed + 경사 x slopeDashBonus`)은
    45도에서 44였는데 실제로는 근처도 못 갔다. 램프가 짧아서다 — 45도 4칸은 0.14초뿐이라
    `dashDecel 50`으로는 +7밖에 못 붙는데, 그 전에 평지 구간에서 이미 32 → 24.6으로 깎여 들어간다.
    **결과적으로 4칸 내리막의 최고 속도가 진입값 32 그대로, 이득이 정확히 0이었다.**
  - `slopeDashAccel` 추가(110). **마찰(`dashDecel`)과 반드시 따로 둔다** — 하나로 묶여 있으면
    경사 가속을 키우는 순간 평지 슬라이드 거리가 같이 줄어든다. 목표가 현재 속도보다 위면
    `slopeDashAccel`, 아래면 `dashDecel`로 갈린다 (평지·오르막은 자동으로 예전과 같아진다)
  - `slopeDashBonus` 35 → 45 (45도 천장 54), `groundMomentumDecel` 66 → 35
  - **거리는 따로 손대지 않는다.** 감속만 정해두면 거리 = `(v² - maxSpeed²) / (2 x 감속)`이라
    빠를수록 제곱으로 멀리 간다. `groundMomentumDecel`을 낮춘 것이 곧 거리 보상이다
  - 실측 (최고 vx / 슬라이드 시작부터 maxSpeed 복귀까지의 거리):

    | 구간 | 이전 | 이후 |
    |---|---|---|
    | 평지 | 32.00 / 9.55u | 32.00 / 9.54u (변화 없음) |
    | 45도 4칸 내리막 | 32.00 / 14.60u | **35.99 / 25.58u** |
    | 45도 8칸 내리막 | 34.67 / 19.83u | **46.33 / 40.87u** |
    | 45도 8칸 오르막 | 32.00 / 12.65u | 32.00 / 12.92u (변화 없음) |

  - 고속에서도 지형을 안 뚫는다: 한 물리 스텝 최대 이동 0.467u < 캡슐 폭 0.8u, `Continuous` 켜짐

- **지형 아트 적용** (2026-09-08) — 사용자 요청. 흰 사각형 지형을 전부 흙 타일로 교체.
  **그림만 바꾸고 콜라이더는 하나도 안 건드렸다.** 검증도 그 사실로 한다 (아래)
  - 스프라이트 3종. `Slope45`만 사용자가 그렸고 나머지 둘은 `TILE_Test_Ground.png`에서 잘라낸 것이다

    | 파일 | 크기 / 9슬라이스 테두리 | 쓰는 곳 |
    |---|---|---|
    | `TILE_Test_Ground_Block.png` | 48x32 / 좌16·하0·우16·상16 | 옆면이 드러나는 지형 (바닥, 구덩이 벽, 발판, 계단) |
    | `TILE_Test_Ground_NoSide.png` | 16x32 / 상16만 | 다른 지형에 맞닿는 면 (언덕 데크, 폭 1유닛 기둥) |
    | `TILE_Test_Ground_Slope45.png` | 16x32 (16x16 두 장) | 45도 계단. 위=대각선 풀, 아래=계단 사이 흙 이음새 |

  - **밑면 테두리는 항상 뺀다.** 사방을 두르면 조각마다 외곽선이 생겨 지형이 "초록 테두리 상자 여러 개"로
    읽힌다. 아래를 열어두면 흙이 계속 이어져 한 덩어리가 된다
  - `SpriteRenderer.drawMode = Tiled` + `sr.size`로 깐다. **Transform 스케일을 쓰면 안 된다** —
    픽셀이 비정수 배율로 늘어난다. Tiled는 스프라이트가 `FullRect`여야 한다 (`spriteMeshType: 0`).
    Tight면 경고만 뜨고 안 깔린다
  - **경사면은 타일맵이 그리고 충돌은 회전 박스가 맡는다.** 회전 스프라이트로 그리면 흙 결이 45도로
    돌아가 평지와 어긋난다. `/Grid/Tilemap_Ground`에 열 단위로 `[대각선] / [이음새] / [흙 …]`을 찍는다.
    타일맵에 `TilemapCollider2D`는 없고 타일도 `colliderType = None`이다
  - 내리막용 PNG를 따로 두지 않는다. 타일 에셋의 `transform`에 `Scale(-1,1,1)`만 넣어 좌우를 뒤집는다
  - 타일 에셋 6개(`Data/Tiles/`)는 `TestMapBuilder`가 없으면 만들고 있으면 갱신한다. 지워도 다시 생긴다
  - **그리기 순서: 0 지형·램프타일 / -1 바닥 / -3 회수 바닥.** 바닥을 뒤로 빼야 그 위에 얹힌 지형이
    바닥 윗면의 풀 띠를 가린다. **램프를 바닥 뒤로 보내면 안 된다** — 이번엔 풀 띠가 언덕 아래를 가로질렀다
  - 지형이 맞닿는 자리는 **한 칸 더 묻는다.** 언덕 데크 밑면은 바닥 속 -2, 램프 타일은 바닥 윗면보다
    한 칸 아래까지. 안 묻으면 바닥의 풀 띠가 언덕 속에 가로줄로 남는다.
    바닥 조각의 *끝면*이 경사에 맞닿는 골짜기 어깨(x=381, 402)는 `CapFloorEdge`로 그 한 열만 덮는다
  - `FloorThick` 2 → 12 (바닥이 공중에 뜬 판자로 안 보이게), 램프 두께 `h x √2` (속이 빈 삼각형 방지)
  - 실측: Zone4 전 구간(x 322~470, 0.25 간격 593점) 레이캐스트 지면 프로파일 **불일치 0 / 최대오차 0.000**.
    Play 모드 45도 램프 위 `grounded=True`, `normal=(-0.71, 0.71)`, 미끄러짐 없음

### Phase 1에서 끝난 것
- `PlayerController` + `MovementConfig` — 가변 점프, 코요테 타임, 점프 버퍼, 정점 체공, 모서리 보정, 방향 전환 가속
- 테스트 맵 `Assets/_Project/Scenes/Maps/TestBox.unity` (점프 거리·높이·천장 틈 시험 구간)
- Cinemachine 3 카메라 (데드존·룩어헤드·댐핑)
- **점프 감각 튜닝 완료** — 사용자 확인 받음. 상승 `timeToApex=0.28`, 낙하 `timeToFall=0.2832` (독립 파라미터)

- **보조 기능 3종 Play 모드 실측 검증 완료** (2026-09-06)
  - 코요테 타임: 발판을 벗어난 공중 상태에서 남은 0.1로 점프 발동 확인
  - 점프 버퍼: 낙하 중 입력이 착지 순간 발동(`vy` 0 → +22.04) 확인
  - 모서리 보정: 천장 틈 왼쪽 모서리에서 x가 33.200 → 33.380으로 밀려 통과 확인

### Phase 1에서 남은 것
- 수평 이동 감각 튜닝 (`maxSpeed`, `groundAccel`/`airAccel` 등) — **사용자 확인 필요**. Phase 1 완료 게이트.
- 애니메이션 상태 전환 — **보류**. 스프라이트가 흰 사각형뿐이라 지금 Animator를 짜면 버릴 코드가 된다. 실제 캐릭터 아트가 생긴 뒤에 착수.
- `CinemachineConfiner2D` — **보류**. 테스트 맵은 회색 박스라 카메라가 경계를 넘어가도 문제가 없다. 실제 맵을 만드는 Phase 2에서 함께.

### 오늘 겪은 것 (반복하지 말 것)
- **Play 모드에서 맵을 구우면 저장이 안 된다.** `EditorSceneManager.MarkSceneDirty`가
  `This cannot be used during play mode`로 터지고, 구운 결과는 Play를 멈추는 순간 통째로 사라진다.
  증상은 "분명히 구웠는데 화면에 옛날 그림"이다. 굽기 전에 `Application.isPlaying`부터 확인할 것.
- **그림만 바꾼 변경의 검증은 레이캐스트 프로파일 스윕이 가장 싸다.** 위에서 아래로 촘촘히 쏴서
  "의도한 지형 높이"와 비교하면 콜라이더가 안 변했음을 한 번에 증명한다. 스프라이트·정렬만 만진
  변경에 Play 실측을 매번 돌릴 이유가 없다 (Play 확인은 마지막에 한 번).
- **에디트 모드에서 카메라를 옮겨 캡처하려면 `CinemachineBrain`을 꺼야 한다.** 켜져 있으면 다음
  에디터 틱에 vcam이 카메라를 도로 끌고 간다. 증상은 "카메라를 옮겼는데 다른 곳이 찍힌다". 끝나면 되돌릴 것.
- **묻는 것은 콜라이더만이다.** 램프 낮은 쪽을 바닥에 묻을 때 그림까지 늘리면 램프의 풀 띠가 지면
  아래로 삐져나온다. 그림은 표면 길이만 그리고 콜라이더는 `BoxCollider2D.offset`으로 연장한다.
- **`cropFrame=None`에서 논리 해상도는 정확한 프레임이 아니라 최소 보장 시야다.** 정수 배율을
  유지하면서 화면을 더 보여주므로 큰 모니터일수록 많이 보인다. 모니터별 시야를 고정하고 싶으면
  세로 픽셀 수가 720/1080/1440의 공약수여야 한다 (180은 되고 216은 안 된다).
- **Cinemachine이 카메라를 몰면 실제 배율은 `refResolution`이 아니라 vcam lens ortho가 정한다.**
  `CinemachinePixelPerfect`가 lens 값에 가장 가까운 정수 배율을 고르기 때문. lens `6.75` /
  화면 2560x1440에서 배율 6(ortho 7.5)이 아니라 **배율 7**(ortho 6.4286)이 선택됐다.
  또한 **브레인이 매 프레임 `cam.orthographicSize`를 덮어쓰므로** 이 확장 없이 `PixelPerfectCamera`만
  붙이면 계산된 ortho가 화면에 반영되지 않는다 (해상도를 바꿔도 화면이 안 변해서 한참 헤맸다).
- **픽셀 퍼펙트에서 캐릭터가 떠는 이유는 스프라이트와 카메라를 *따로* 반올림하기 때문이다.**
  `PixelPerfectCamera`는 스프라이트와 카메라를 각각 1/16 격자에 올린다. 화면상 위치는
  `round(캐릭터) - round(카메라)`인데, 두 값의 간격 소수부가 0.5 근처면 두 반올림이 서로 다른
  프레임에 넘어가면서 **매 프레임 ±1픽셀로 튄다.** 카메라가 댐핑으로 따라붙는 동안 = 가감속할 때마다
  이 구간을 지나므로 "움직일 때만 떤다". 가만히 서 있으면 멀쩡하다(실측: 정지 90프레임 y 변화 0).
  - 해법은 `CameraPixelLock` — 카메라를 **격자에 올린 타깃에서 정수 픽셀만큼** 떨어진 자리로 옮긴다.
    그러면 두 반올림의 차가 항상 정확한 정수라 어긋날 수 없다.
  - 실측(평지 9u/s 이동, **60fps 고정**, 215프레임): 잠금 없이 **59회** 방향 전환 → 잠금 후 **3회**.
    144fps에서도 45회 → 3회. 프레임률과 무관하게 값어치를 한다.
  - 스프라이트의 로컬 오프셋도 격자 위에 있어야 한다. `Visual`의 `-0.9`는 14.4픽셀이라 격자 밖이고,
    그 0.4픽셀 때문에 캐릭터 y가 바뀔 때(경사·점프) 세로가 ±1픽셀 튄다. `-0.875`(=14픽셀)로 고쳤다.
  - **이 잠금은 오차를 없애는 게 아니라 카메라 쪽으로 옮긴다.** 화면상 위치는 정수여야 하는데
    실제 간격은 소수다. 캐릭터를 화면에 박아두면 그 소수부가 갈 곳은 카메라뿐이라,
    타깃과의 간격이 변하는 동안에는 배경이 진행 반대로 한 픽셀씩 튄다. 아래 경사 항목 참고.
- **에디터 게임 뷰의 프레임률을 고정하지 않고 렌더링 수치를 재지 말 것.** 포커스가 없으면 수천 fps로
  도는데, 그러면 프레임당 카메라 이동이 픽셀보다 훨씬 작아져 반올림 오차가 실제보다 크게 나온다.
  이것 때문에 "경사에서 카메라가 프레임마다 튄다(역방향 15회)"로 판단하고 한참 엉뚱한 곳을 팠는데,
  **60fps로 고정해 다시 재니 0~2회**였고 진짜 원인은 따로 있었다(아래 항목).
  검증 스크립트 안에서 `QualitySettings.vSyncCount = 0; Application.targetFrameRate = 60;`을 걸 것.
- **경사에서 화면이 흔들리는 진짜 원인은 픽셀이 아니라 세로 댐핑이었다.** 45도에서는 세로 속도가
  가로와 같아서(±7.6 ~ -10.4 u/s) 세로 댐핑 0.6이 만드는 지연이 크게 벌어졌다 늦게 따라온다.
  실측(8칸 램프, 60fps): 화면상 세로 위치가 **오르막 2.56유닛 / 내리막 2.25유닛** 출렁였다.
  화면 세로가 12.9유닛이던 시절엔 화면의 20%가 위아래로 쓸린다는 뜻이다.
  - 해법: `CinemachinePositionComposer`의 **세로 댐핑을 0**으로, **세로 데드존을 0.22 → 0.5**로,
    룩어헤드를 0.35 → 0.15로. 데드존 안에서는 카메라가 아예 안 움직이고 밖에서는 타깃과 같은
    속도로 따라가므로 간격이 상수가 된다. 결과 **오르막 1.25 / 내리막 1.06유닛**(절반).
  - **세로 댐핑을 다시 올리면 경사에서 화면이 출렁인다.** 부드럽게 하고 싶으면 데드존을 키울 것.
    데드존이 충분히 크면(0.5 = 9유닛) 점프(3.2유닛)는 통째로 안에 들어와 카메라가 반응하지 않는다.
- **시야는 vcam lens ortho가 정한다** (`refResolution` 아님, 위 항목 참고). `6.75` → **`9.0`**으로
  넓혔다. 2560x1440에서 배율 5가 선택되어 **32 x 18 유닛**(이전 22.9 x 12.9). 배율이 정수라
  선택지는 띄엄띄엄하다 — 1440p 기준 `7.5`면 배율 6(26.7x15), `11.25`면 배율 4(40x22.5).
- **45도 램프 진입에서 플레이어가 영구 정지했다.** `ProbeGroundNormal`은 콜라이더 중심에서 아래로
  광선을 쏘는데, 램프 아래끝을 바닥에 묻으면 **캡슐 앞면이 이미 램프 면에 닿아 있어도 중심 광선은
  그 아래 평지를 먼저 맞는다.** 경사가 평지로 읽히면 `vy`가 `-groundStickSpeed`로 고정되어 아래로
  눌리는데 캡슐은 45도 면을 벽처럼 밀기만 하므로 올라갈 방법이 없다.
  실측: `vx 9.0` → `1.100`(= groundAccel 한 스텝)에서 완전 정지, 400스텝 동안 x가 0.000 움직임.
  40도에서는 잠깐 버벅이고 빠져나가서(기존 메모의 "9 → 2.2") 문제로 안 보였을 뿐이다.
  - 해법은 `ClimbContactNormal` — **실제 접촉면**(`Collider2D.GetContacts`)에서 법선을 가져온다.
    조건 셋: 걸을 수 있는 각도 + 진행 방향을 막는 방향(`n.x * dir < 0`) + 접촉점이 중심보다 **앞**.
    마지막 조건이 없으면 램프 정상에서 뒤꿈치가 계속 밀어 올려 통통 튄다.
  - **앞쪽을 광선으로 미리 보는 방식은 여전히 금지다.** 평지에서 램프에 닿기도 전에 발사된다.
    "닿아 있는 것만 본다"가 핵심 차이다.
- **런타임에 `PixelPerfectCamera` 설정을 바꿔도 즉시 반영되지 않는다.** 내부 계산이 캐시된다.
  `editor_stop` -> 에디트 모드에서 변경 -> `editor_play` 순서로 검증할 것.
- **에디트 모드에서 `Physics2D.Raycast`를 쓰기 전에 `Physics2D.SyncTransforms()`를 부를 것.**
  물리 스텝이 없어서 콜라이더가 트랜스폼 변경(특히 회전)을 아직 안 따라간다. 회전한 램프가
  회전 전 AABB로 잡혀서 "지형에 턱이 있다"고 오판했다.
- **에디터에서 Play 모드로 키 입력을 주입하는 건 포기하는 게 빠르다.** `editorInputBehaviorInPlayMode`와
  `backgroundBehavior`를 바꿔도 포커스가 없으면 안 통했다(`QueueStateEvent`를 게임 컨텍스트에서
  넣어도 마찬가지). **검증 스크립트는 `PlayerController`의 `_moveInput`을 리플렉션으로 직접 채우고
  `[DefaultExecutionOrder(1000)]`으로 `Update` 뒤에 돌린다.** 이쪽이 확실하다.
- **스프라이트를 교체할 때 `SpriteRenderer.color`를 같이 확인한다.** 흰 사각형 시절에 구분하려고
  넣어둔 착색이 남아 있으면 새 스프라이트의 모든 픽셀에 곱해진다. 실제로 파란 착색
  `(0.35, 0.75, 1.0)`이 남아 캐릭터 색이 전부 틀어졌다 — R이 35%로 깎이고 B만 살아남는다.
  **색이 다르게 보이면 여기부터 본다.** 착색을 흰색으로 되돌린 뒤 원본 PNG와 렌더 결과가
  460/460 픽셀 완전 일치함을 실측했다 (Linear 색공간·2D 조명·Point 필터 전부 색을 보존한다).
  - 진단 순서: `SpriteRenderer.color` → `Light2D` 색·강도 → 텍스처 `sRGB` 플래그 → 압축 설정 →
    PNG의 `iCCP`/`gAMA` 청크. 우리 PNG는 `sRGB` 청크만 있어 프로파일 문제는 없다.
- **스프라이트에 Transform 스케일을 걸지 않는다.** PPU가 맞으면 스케일은 항상 1이어야 한다.
  흰 사각형 시절의 `(0.8, 1.8)` 스케일을 남겨두면 픽셀이 비정수 배율로 늘어나 흐려진다.
- **룰 타일용 3x3 시트는 "한 덩어리 그림"으로 그리는 게 맞다.** 사방을 두른 외곽선과 둥근 투명
  모서리는 격자 선이 아니라 지형 덩어리의 바깥 테두리다. 검증할 때 **시트 전체를 반복 배치하면 안 되고**
  9칸으로 잘라 모서리/엣지/채움 규칙대로 조립해야 한다 (전체를 반복해서 "이어붙일 수 없다"고 오판했다).
- **텍스처를 `GetPixels32()`로 읽으려면 `isReadable`이 켜져 있어야 한다.** 꺼두는 게 맞으므로
  픽셀 분석은 에디터가 아니라 PNG 파일을 직접 디코딩해서 한다.
- **넉백은 속도를 대입하고 끝내면 안 된다.** 깎아주는 주체가 없으면 받은 속도를 그대로 안고
  계속 날아간다 — 한 대 맞은 허수아비가 15유닛을 날아가 맵 밖으로 떨어졌다. `Health.FixedUpdate`가
  수평 속도를 `knockbackDecay`로 0까지 깎는다. **지속 시간을 따로 두지 않는다** — 속도가 0이 될
  때까지만 깎으면 "세게 맞으면 오래 밀린다"가 저절로 성립하고, 시간과 감속을 따로 맞출 일이 없다.
- **`unity command eval`의 CLI 왕복은 2초를 넘는다.** 짧은 현상(히트스톱 0.06초)을 eval 두 번으로
  나눠 관측하려 하면 이미 끝난 뒤라 "발동 안 함"으로 오판한다. 실제로 히트스톱이 멀쩡한데
  `timeScale=1`로 읽혀 한참을 헤맸다. **게임 안에서 코루틴 프로브를 돌려 `PlayerPrefs`에 기록하고
  나중에 읽을 것.** (`Time.realtimeSinceStartup` 측정 결과 요청 2.000초 → 실제 2.003초)
- **적을 죽여놓고 다음 검증을 하지 말 것.** 허수아비 2기가 이미 파괴된 줄 모르고 히트스톱을
  측정해서 "허공을 친" 결과를 놓고 원인을 찾았다. 검증 전에 대상이 살아 있는지부터 확인한다.
- **입력 에셋을 그대로 Enable/Disable 하면 안 된다.** `InputSystem_Actions`는 프로젝트 전역 에셋이라 Unity가 스스로 관리하는데, 컴포넌트에서 같은 객체를 또 켜고 끄면 `Map must be contained in state` 오류와 함께 입력이 죽고 플레이 모드가 스스로 종료된다. `Instantiate()`로 전용 복사본을 만들어 쓸 것 (`PlayerController.Awake` 참고).
- **플레이 모드 중에는 리컴파일하지 않는다.** 도메인 리로드가 걸리면서 위와 같은 `Map must be contained in state` / `Map index on InputActionMap is out of range`가 `OnEnable`에서 터진다. `Instantiate()` 복사본을 써도 막히지 않는다 — 복사본 자체가 리로드로 죽은 채 `OnEnable`이 돌기 때문이다. 증상은 그 세션 동안 입력이 통째로 죽는 것. **스크립트를 고쳤으면 Play를 멈추고 리컴파일한 뒤 다시 Play한다.** MCP로 작업할 때는 `editor_stop` → `recompile` → `editor_play` 순서를 지킬 것.
- 물리 틱은 50Hz → **100Hz**로 올려둠 (`ProjectSettings/TimeManager.asset`). Unity 6.6에서 이 값은 float이 아니라 `Fixed Timestep.m_Count / 141120000` 형태의 유리수라 인스펙터 밖에서 바꾸려면 `m_Count`를 조정해야 한다.
- 이산 적분 오차 때문에 실제 최고 도달 높이는 `jumpHeight`보다 약 0.1 낮다. 3유닛 계단은 문제없이 넘으므로 지금은 보정하지 않음.
- **에디터 창이 백그라운드면 Play 모드 프레임이 멈춘다.** `playing=True`인데 `Time.frameCount`가 고정되면 이것이다(플레이 모드가 종료된 게 아니다). `runInBackground`를 켜서 해결했고, 이 값은 부팅 시 읽히므로 **에디터 재시작이 필요**하다. `PlayerSettings.runInBackground = true`는 디스크에 안 써지니 `Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings")` + SerializedObject로 쓸 것.
- **경사면에서 지면으로 누르는 힘은 반드시 법선 반대 방향이어야 한다.** 월드 아래로 누르면
  그 힘의 접선 성분이 남고, 물리 솔버가 파고든 속도를 되돌릴 때 그게 **전진 속도로 새어 들어간다.**
  40° 경사에서 스텝당 +0.98(= 98 u/s²)이 붙어 걷기만 해도 `vx`가 9에서 25까지 폭주했다.
  평지에서는 법선이 (0,1)이라 두 방식이 같아서 이 버그가 안 보인다.
- **경사 램프의 아래쪽 끝은 바닥 콜라이더 안으로 묻는다.** 램프 끝점이 바닥 표면에 정확히 걸치면
  꼭짓점이 노출되고, 캡슐이 거기 부딪혀 진입 속도가 9에서 2.7까지 깎인다. 레벨 제작 규칙이지 코드 문제가 아니다.
- **경사를 진행 방향 앞쪽에서 미리 탐지하지 말 것.** 오르막 진입에서 속도가 깎이는 걸 막으려고
  시도했는데, 평지에서 램프에 닿기도 전에 `vy`가 +3.9로 붙어 플레이어가 발사된다. 위 두 항목이 진짜 해법이다.
- **접지 판정 박스가 보는 범위만큼 법선 광선도 내려가야 한다.** 박스는 폭이 있어 경사면 위쪽
  모서리로 지면을 먼저 잡는데, 중심 광선이 거기 못 닿으면 "접지는 맞는데 평지"로 읽혀
  경사 처리가 통째로 빠진다. 필요한 추가 길이 = 박스 반폭 x tan(최대경사).
- **MCP eval에서 `InputSystem.Update()`를 직접 부르면 안 된다.** 에디터 컨텍스트로 실행되어
  입력 이벤트가 에디터 상태 버퍼로 들어가고 플레이 모드 버퍼에는 안 간다. 증상은 `kb.dKey.isPressed`가
  true인데 게임의 액션은 (0,0)을 읽는 것. `QueueStateEvent`만 하고 게임 루프가 처리하게 둘 것.
- 레이캐스트로 뭔가를 감지할 때 **탐지 거리는 한 물리 스텝의 이동량보다 커야 한다.** 고정 거리를 쓰면 빠를 때 구간을 건너뛴다 (`CorrectCorner`의 `probeUp` 참고).
- **속도를 코드로 직접 지정하는 콜라이더에는 마찰 0 물리 머티리얼을 반드시 붙인다** (`Assets/_Project/Data/PlayerNoFriction.physicsMaterial2D`). 머티리얼이 없으면 Unity 2D 기본 마찰 0.4가 걸리고, `groundStickSpeed`(2.0)로 지면을 누르는 힘과 곱해져 매 스텝 `0.4 x 2.0 = 0.8`씩 수평 속도가 사라진다. 감속 80 u/s²에 해당하며 `groundAccel` 110의 대부분을 상쇄한다.
  - **진단 방법**: `groundDecel`/`airDecel`을 일시적으로 0으로 두고 속도를 준 뒤 vx가 유지되는지 본다. 줄어들면 우리 코드 밖에서 속도를 먹는 것이 있다는 뜻. 감속값이 살아 있으면 손실이 감속에 묻혀 보이지 않는다.
  - 안착 **이후**에만 나타난다. 착지 직후 아직 가라앉는 중에는 접촉 충격이 달라 안 보이므로, 착지 순간만 보고 판단하면 놓친다.

## 툴체인 (Unity CLI)
`C:\Users\siwon\AppData\Local\Unity\bin\unity.exe` (사용자 PATH에 등록됨). Hub GUI 없이 대부분을 자동화할 수 있다.

| 목적 | 명령 |
|---|---|
| 실행 중 에디터 상태 | `unity status` |
| 에디터가 노출한 툴 목록 | `unity list --project-path <경로>` |
| C# 즉시 실행 (도메인 리로드 없음, 200~600ms) | `unity command eval "return Application.unityVersion;"` |
| 프로젝트 열기 | `unity open <경로>` |
| 테스트 실행 | `unity test <경로>` |
| 빌드 | `unity build <경로>` |

**주의사항 (실제로 겪은 것)**
- 에디터를 **백그라운드 Bash 작업으로 띄우면 안 된다.** 작업이 끝나면 자식 프로세스인 에디터까지 함께 죽는다. PowerShell `Start-Process`로 세션과 분리해 띄울 것.
- `Packages/manifest.json`을 바꾼 뒤에는 **에디터를 재시작**해야 반영된다. 부팅 중에 바꾸면 무시된다.
- Git Bash에서 `tasklist /FI "IMAGENAME eq ..."` 필터는 오작동한다. 프로세스 확인은 `tasklist | grep` 형태로 하고 `head`로 자르지 말 것 (Unity Hub.exe 항목이 여러 개라 Unity.exe 행이 잘려 나간다).
- **CLI 바이너리도 이름이 `Unity.exe`다.** 프로세스 이름만으로 에디터를 찾으면 `unity mcp`/`unity status` 프로세스가 잡혀 "에디터가 떠 있다"고 오판한다. 실행 경로로 걸러야 한다: `Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object { $_.ExecutablePath -like "*Hub\Editor*" }`. 가장 확실한 판정은 `unity status`가 `ready`를 반환하는지 보는 것.
- **`clear_console`은 콘솔 버퍼를 비우지 않는다.** 비운 줄 알고 다시 읽으면 몇 시간 전 오류가
  그대로 나온다. 이것 때문에 이미 고쳐진 문제를 계속 재현되는 줄 알고 `PlayerController`의
  입력 처리를 세 번이나 갈아엎었다(전부 되돌림). **판정은 반드시 `seq`로 한다** — 작업 전 마지막
  `seq`를 적어두고 그보다 큰 항목만 새 로그로 본다.
- **긴 검증 코루틴은 플레이어가 죽는 순간 같이 죽는다.** `PlayerDeath`가 씬을 리로드하면서
  코루틴 호스트가 사라져 결과가 영영 안 써진다. 증상은 `PlayerPrefs`가 계속 "running"인 것.
  관찰만 할 구간은 `Health.Invincible`을 켜 두거나, **짧은 eval 여러 번으로 나눠서** 본다.
- **적처럼 계속 움직이는 대상은 `Time.timeScale`을 낮추고 관찰한다.** CLI 왕복이 2초라 그 사이에
  적이 10유닛을 이동해 "먼 거리에서의 상태"를 볼 수가 없다. `timeScale = 0.05`로 두면
  왕복 4초가 게임 안에서 0.2초가 되어 배치 직후 상태를 그대로 읽을 수 있다. **읽고 나서 1로 되돌릴 것.**
- **Play 모드에 키 입력을 주입하려면 게임 뷰 포커스 문제를 먼저 풀어야 한다.** 에디터가 포커스를 잃으면
  `backgroundBehavior = ResetAndDisableNonBackgroundDevices` 때문에 키보드 이벤트가 통째로 버려진다.
  증상은 `QueueStateEvent`를 아무리 넣어도 `kb.dKey.isPressed`가 계속 false인 것. `editor_focus`로는 안 풀린다.
  검증 코루틴 안에서 `InputSystem.settings.backgroundBehavior = IgnoreFocus`와
  `editorInputBehaviorInPlayMode = AllDeviceInputAlwaysGoesToGameView`를 켰다가 **끝나면 되돌린다**
  (되돌리지 않으면 프로젝트 설정 에셋이 더러워진다).
- **`eval` / `eval_file`에는 `using` 지시문을 쓸 수 없다.** 스크립트 본문이 메서드 안에 들어가므로
  `using UnityEngine;`은 파싱 에러가 된다. 타입을 전부 정규화해서 쓸 것 (`Game.Combat.Health` 등).
  `UnityEngine`·`UnityEditor`는 이미 열려 있고 `System.Text.StringBuilder`는 정규화가 필요하다.
- **Git Bash에서 `/Player` 같은 하이어라키 경로는 Windows 경로로 변환된다.** `unity command`에
  `--target /Player`를 넘기면 `C:/Program Files/Git/Player`로 바뀐다. `export MSYS_NO_PATHCONV=1`을 먼저 할 것.
- MCP가 에디터에 붙으려면 `com.unity.pipeline` 패키지가 필요하다 (`unity pipeline install --project-path <경로>`).
