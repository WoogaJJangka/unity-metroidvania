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

## 현재 상태 (2026-09-05 기준)
- **Phase 0 (환경 준비) 완료** — 태그 `phase-0-setup`
- **Phase 1 (조작감) 진행 중** — 브랜치 `feature/movement`, 마지막 커밋 `68a015a`

### Phase 1에서 끝난 것
- `PlayerController` + `MovementConfig` — 가변 점프, 코요테 타임, 점프 버퍼, 정점 체공, 모서리 보정, 방향 전환 가속
- 테스트 맵 `Assets/_Project/Scenes/Maps/TestBox.unity` (점프 거리·높이·천장 틈 시험 구간)
- Cinemachine 3 카메라 (데드존·룩어헤드·댐핑)
- **점프 감각 튜닝 완료** — 사용자 확인 받음. 상승 `timeToApex=0.28`, 낙하 `timeToFall=0.2832` (독립 파라미터)

### Phase 1에서 남은 것
- 수평 이동 감각 튜닝 (`maxSpeed`, `groundAccel`/`airAccel` 등) — 아직 사용자 확인 안 받음
- 모서리 보정·코요테 타임의 실제 체감 확인 (코드는 있으나 플레이로 검증 안 함)
- 애니메이션 상태 전환 (Idle / Run / Jump / Fall / Land) — 미착수
- `CinemachineConfiner2D`로 카메라 이동 범위 제한 — 미착수

### 오늘 겪은 것 (반복하지 말 것)
- **입력 에셋을 그대로 Enable/Disable 하면 안 된다.** `InputSystem_Actions`는 프로젝트 전역 에셋이라 Unity가 스스로 관리하는데, 컴포넌트에서 같은 객체를 또 켜고 끄면 `Map must be contained in state` 오류와 함께 입력이 죽고 플레이 모드가 스스로 종료된다. `Instantiate()`로 전용 복사본을 만들어 쓸 것 (`PlayerController.Awake` 참고).
- 물리 틱은 50Hz → **100Hz**로 올려둠 (`ProjectSettings/TimeManager.asset`). Unity 6.6에서 이 값은 float이 아니라 `Fixed Timestep.m_Count / 141120000` 형태의 유리수라 인스펙터 밖에서 바꾸려면 `m_Count`를 조정해야 한다.
- 이산 적분 오차 때문에 실제 최고 도달 높이는 `jumpHeight`보다 약 0.1 낮다. 3유닛 계단은 문제없이 넘으므로 지금은 보정하지 않음.

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
- MCP가 에디터에 붙으려면 `com.unity.pipeline` 패키지가 필요하다 (`unity pipeline install --project-path <경로>`).
