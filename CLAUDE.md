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
- **개발자**: C 언어 경험 있음, C#·Unity 입문. 새 개념이 나오면 한 줄 설명을 덧붙일 것.

## 폴더 규칙
```
Assets/
  _Project/            ← 우리가 만든 모든 것
    Art/               Sprites, Tilesets, Animations, UI
    Audio/             BGM, SFX
    Code/              Player, Enemy, Combat, World, Quest, UI, Save, Utils
    Data/              ScriptableObject 에셋 (MovementConfig, EnemyConfig, QuestSO …)
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
  - ⏸ **`TestBox` 지형 전체 타일맵 전환 보류** — `BaseGround` 자리에만 시범 배치했다.
    전환하면 `TilemapCollider2D` + `CompositeCollider2D`로 콜라이더 주체가 바뀌므로
    경사 램프(회전 박스)와 어떻게 공존시킬지 먼저 정해야 한다

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
  - `maxSlopeAngle 50` (걸어 올라갈 수 있는 최대 각), `slopeDashBonus 20` (내리막 슬라이드 가속) 추가
  - 접지 중 속도는 지면이 정한다: `vx` 그대로 + `vy = vx * tan(경사)`, 여기에 지면으로 눌러붙이는
    `groundStickSpeed`를 더한다. **누르는 방향은 월드 아래가 아니라 법선 반대여야 한다** (아래 참고)
  - 내리막 슬라이드는 목표 속도가 `maxSpeed - grade * slopeDashBonus`로 올라가 죽지 않는다.
    같은 `dashDecel`이 마찰이자 가속도라 목표가 위면 가속, 아래면 감속으로 저절로 갈린다
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
- **`cropFrame=None`에서 논리 해상도는 정확한 프레임이 아니라 최소 보장 시야다.** 정수 배율을
  유지하면서 화면을 더 보여주므로 큰 모니터일수록 많이 보인다. 모니터별 시야를 고정하고 싶으면
  세로 픽셀 수가 720/1080/1440의 공약수여야 한다 (180은 되고 216은 안 된다).
- **Cinemachine이 카메라를 몰면 실제 배율은 `refResolution`이 아니라 vcam lens ortho가 정한다.**
  `CinemachinePixelPerfect`가 lens 값에 가장 가까운 정수 배율을 고르기 때문. lens `6.75` /
  화면 2560x1440에서 배율 6(ortho 7.5)이 아니라 **배율 7**(ortho 6.4286)이 선택됐다.
  또한 **브레인이 매 프레임 `cam.orthographicSize`를 덮어쓰므로** 이 확장 없이 `PixelPerfectCamera`만
  붙이면 계산된 ortho가 화면에 반영되지 않는다 (해상도를 바꿔도 화면이 안 변해서 한참 헤맸다).
- **런타임에 `PixelPerfectCamera` 설정을 바꿔도 즉시 반영되지 않는다.** 내부 계산이 캐시된다.
  `editor_stop` -> 에디트 모드에서 변경 -> `editor_play` 순서로 검증할 것.
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
