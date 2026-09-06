# gmae_project — 2D 메트로배니아

## 프로젝트 개요
- **장르**: 2D 메트로배니아. 맵 전환형 오픈월드(메이플스토리식) + 오리/할로우나이트의 조작감·분위기
- **범위**: 전투, 탐험, 능력 해금 게이팅, 스토리 퀘스트, 미니게임
- **엔진**: Unity 6000.6.0f1 / Universal 2D 템플릿 (URP 2D Renderer)
- **개발 로드맵**: `C:\Users\siwon\.claude\plans\atomic-noodling-sunbeam.md`
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

## 현재 상태 (2026-09-06 기준)
- **Phase 0 (환경 준비) 완료** — 태그 `phase-0-setup`
- **Phase 1 (조작감) 완료** — 태그 `phase-1-movement`. 점프·수평 이동 모두 사용자 확인 받음.
  최종값: `maxSpeed 9`, `groundAccel 110`, `groundDecel 60`, `airAccel 100`, `airDecel 40`,
  `timeToApex 0.28`, `timeToFall 0.2832`, `apexBonusSpeed 0`
- **Phase 2 (맵 전환) 진행 중** — 브랜치 `feature/world`
  - ✅ 맵 전환 코어 완료: `GameManager`(자체 생성 + 페이드 + 비동기 로드), `Portal`, `SpawnPoint`.
    A↔B 왕복과 스폰 위치 복원을 Play 모드로 실측 검증
  - ⏸ **타일맵 보류** — 타일 아트가 없어 룰 타일을 지금 세팅해도 쓸 데가 없다. Kenney 에셋 도입 시 착수
  - ⏸ **Sorting Layer 보류** — 스프라이트가 전부 `Default` 하나뿐. 배경 아트 생기면 정한다

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
- **입력 에셋을 그대로 Enable/Disable 하면 안 된다.** `InputSystem_Actions`는 프로젝트 전역 에셋이라 Unity가 스스로 관리하는데, 컴포넌트에서 같은 객체를 또 켜고 끄면 `Map must be contained in state` 오류와 함께 입력이 죽고 플레이 모드가 스스로 종료된다. `Instantiate()`로 전용 복사본을 만들어 쓸 것 (`PlayerController.Awake` 참고).
- 물리 틱은 50Hz → **100Hz**로 올려둠 (`ProjectSettings/TimeManager.asset`). Unity 6.6에서 이 값은 float이 아니라 `Fixed Timestep.m_Count / 141120000` 형태의 유리수라 인스펙터 밖에서 바꾸려면 `m_Count`를 조정해야 한다.
- 이산 적분 오차 때문에 실제 최고 도달 높이는 `jumpHeight`보다 약 0.1 낮다. 3유닛 계단은 문제없이 넘으므로 지금은 보정하지 않음.
- **에디터 창이 백그라운드면 Play 모드 프레임이 멈춘다.** `playing=True`인데 `Time.frameCount`가 고정되면 이것이다(플레이 모드가 종료된 게 아니다). `runInBackground`를 켜서 해결했고, 이 값은 부팅 시 읽히므로 **에디터 재시작이 필요**하다. `PlayerSettings.runInBackground = true`는 디스크에 안 써지니 `Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings")` + SerializedObject로 쓸 것.
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
- MCP가 에디터에 붙으려면 `com.unity.pipeline` 패키지가 필요하다 (`unity pipeline install --project-path <경로>`).
