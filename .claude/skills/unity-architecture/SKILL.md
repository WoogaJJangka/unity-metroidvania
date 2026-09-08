---
name: unity-architecture
description: >
  이 프로젝트(2D 메트로배니아)의 구조 결정 사항 — 폴더·네임스페이스 배치, _Bootstrap과 씬 구성,
  맵 전환·포털, 시스템 간 결합 방식, ScriptableObject 데이터 레이어, 세이브 구조. 새 시스템을
  어디에 두고 기존 시스템과 어떻게 연결할지 정할 때 사용한다. Unity 엔진 자체의 사용법이 아니라
  "이 프로젝트는 이렇게 하기로 했다"만 담는다. 엔진 일반 사용법은 unity 플러그인 스킬을 쓸 것.
---

# 프로젝트 구조 결정

> **이 문서와 코드가 어긋나면 코드가 옳다.** 어긋난 걸 발견하면 먼저 이 문서를 고치고 계속한다.
> 마지막 갱신: 2026-09-06 (Phase 1 진행 중)

## 확정된 결정

| 항목 | 결정 | 이유 |
|---|---|---|
| 우리 코드 위치 | `Assets/_Project/` 아래에만 | `Assets/Settings/`(URP·Input)는 GUID 참조 중이라 이동·개명 금지. `Assets/Welcome/`는 템플릿 튜토리얼 |
| 외부 에셋 | `Assets/ThirdParty/` | 우리 코드와 절대 섞지 않는다. 업데이트 시 통째로 교체 가능해야 함 |
| 네임스페이스 | `Game.<영역>` | `Game.Player`, `Game.Combat`, `Game.Quest`, `Game.World`, `Game.UI`, `Game.Save` |
| 맵 단위 | 맵 1개 = 씬 1개 (`Scenes/Maps/`) | 메이플식 맵 전환형 구조 |
| 미니게임 | Additive 로드 (`Scenes/MiniGames/`) | 종료 시 언로드하고 결과만 퀘스트로 반환 |
| 전역 상태 | `GameManager`가 `[RuntimeInitializeOnLoadMethod]`로 **스스로 생성** + `DontDestroyOnLoad` | 어느 씬에서 Play를 눌러도 동작해야 함. `_Bootstrap` 씬을 두면 "그 씬부터 시작해야 한다"는 제약이 생기므로 쓰지 않는다 |
| 맵 전환 구현 | `GameManager.TravelTo(씬, 스폰id)` — 페이드·비동기 로드·스폰 배치를 한 코루틴에서 처리 | 책임이 하나뿐이라 별도 `SceneTransitionManager` 클래스를 두지 않았다. 전환에 다른 책임(로딩 화면, 스트리밍)이 붙으면 그때 분리 |
| 플레이어 소유 | 맵 씬마다 각자 배치. 도착 시 `GameManager`가 `SpawnPoint`로 옮긴다 | 플레이어를 `DontDestroyOnLoad`로 만들면 맵 씬에서 바로 Play할 수 없다. 대신 **체력·능력 같은 지속 상태는 플레이어가 아니라 `GameManager`가 들어야 한다** (Phase 5) |
| 맵 이동 | 포털 트리거 → `{대상 씬 이름, 스폰 포인트 ID}` | 씬 이름만으로는 "어디로 나오는지"가 안 정해짐 |
| 튜닝 수치 | 전부 ScriptableObject (`Assets/_Project/Data/`) | MonoBehaviour 필드는 플레이 종료 시 되돌아감 → 튜닝 불가 |

## 결합 규칙

**시스템끼리 직접 참조하지 않는다.** 연결은 `Action`/`event`로 끊는다.

- 나쁨: `PlayerHealth`가 `HUDController`를 직접 들고 갱신
- 좋음: `PlayerHealth.OnHealthChanged` 이벤트 → HUD가 구독

매니저를 직접 참조하는 코드를 늘리지 않는다. 새 시스템을 붙일 때 "이걸 참조해야만 하나?"를 먼저 묻는다.

> ⚠️ 이벤트 구독은 `OnEnable`에서 걸고 **`OnDisable`에서 반드시 해제**한다. 씬을 오가는 구조라
> 해제를 빠뜨리면 파괴된 오브젝트가 계속 호출되어 조용히 깨진다.

## 데이터 레이어

ScriptableObject는 "씬에 존재하지 않는 데이터 덩어리"다. 현재 있는 것과 예정된 것:

| 에셋 | 상태 | 담는 것 |
|---|---|---|
| `MovementConfig` | ✅ 있음 | 이동·점프 수치 전부 (Phase 1) |
| `EnemyConfig` | 예정 (Phase 3) | 체력/이동속도/공격력/감지범위 → 적 종류를 데이터로 확장 |
| `QuestSO` | 예정 (Phase 6) | 목표 타입, 선행 조건, 보상 |

**새 시스템에 튜닝할 수치가 3개 이상 생기면 SO로 뽑는다.**

## 아직 정해지지 않은 것

각 Phase에서 결정되면 이 문서에 위 표 형식으로 추가한다.

- [ ] **Phase 2 (일부 남음)** — Sorting Layer 최종 목록. 지금은 모든 스프라이트가 `Default` 하나뿐이라 배경 아트가 들어오는 시점에 정한다. 타일맵도 같은 이유로 보류 (타일 아트가 없다)
- [ ] **Phase 4** — `AbilityFlags`(enum flags)를 누가 소유하는가: `GameManager` vs 별도 `PlayerState`
- [ ] **Phase 5** — `SaveData` 스키마와 버전 마이그레이션 정책. 저장 항목: 현재 씬·위치, 체력, 능력 플래그, 퀘스트 진행, 획득 아이템, 열린 지름길
- [ ] **Phase 6** — 퀘스트 상태를 세이브에 어떻게 싣는가. ink 텍스트 → ScriptableObject 임포터 위치
- [ ] **Phase 7** — `IMiniGame` 인터페이스와 결과 반환 경로

## 이 문서를 갱신하는 시점

- 새 폴더나 네임스페이스를 만들 때
- 시스템 간 연결 방식을 새로 정할 때
- 위 "아직 정해지지 않은 것" 중 하나가 결정될 때 → 체크박스를 지우고 확정 표로 옮긴다
