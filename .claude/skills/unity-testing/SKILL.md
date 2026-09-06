---
name: unity-testing
description: >
  이 프로젝트에서 무엇을 테스트하고 무엇을 테스트하지 않는지, 그리고 코드를 고친 뒤 반드시 밟는
  검증 절차 — Unity 콘솔 확인, EditMode 테스트 작성 대상, Play 모드 검증, unity CLI로 테스트 실행.
  스크립트를 수정한 직후, 테스트를 새로 쓸 때, "동작하는지 확인해줘" 요청을 받았을 때 사용한다.
---

# 테스트와 검증

> 마지막 갱신: 2026-09-06 (Phase 1 진행 중)

## 코드를 고친 뒤 반드시 (순서 지킬 것)

**1. 컴파일 에러 확인 — 컴파일 성공 ≠ 동작.**

에디터가 떠 있는지 먼저 본다:

```bash
unity status
```

`ready`가 뜨면 살아있는 에디터에 C#을 직접 실행할 수 있다 (도메인 리로드 없음, 200~600ms):

```bash
unity command eval "return UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;"
```

**2. EditMode 테스트가 있는 영역이면 돌린다** (아래 대상 참조).

**3. Play 모드 검증은 사용자가 한다.** 물리·조작감·연출은 사람이 직접 만져봐야 안다.
무엇을 확인해야 하는지 구체적으로 알려주고 넘긴다 — "테스트했다"고 말하지 않는다.

## 테스트할 것 / 안 할 것

| 대상 | 테스트 | 이유 |
|---|---|---|
| 세이브 직렬화·역직렬화 (Phase 5) | ✅ EditMode | 물리와 무관한 순수 로직. 깨지면 진행 상황이 날아간다 |
| 퀘스트 상태 전이 (Phase 6) | ✅ EditMode | 조건 분기가 많고 손으로 재현하기 오래 걸린다 |
| `MovementConfig` 파생값 (`Gravity`, `JumpVelocity`, `FallGravity`) | ✅ EditMode | 순수 수식. 공식이 틀어져도 눈으로 안 보인다 |
| `AbilityFlags` 비트 연산 (Phase 4) | ✅ EditMode | 게이팅이 조용히 열리거나 잠긴다 |
| 조작감 수치 (점프 높이, 코요테 타임) | ❌ | **사람이 판단하는 영역.** 테스트로 고정하면 튜닝을 방해한다 |
| 물리 충돌·접지 판정 | ❌ 자동화 안 함 | PlayMode 테스트는 느리고 불안정. Play 모드에서 직접 본다 |
| 애니메이션 전환, 연출 | ❌ | 눈으로 본다 |

**원칙: 물리와 무관한 순수 로직만 테스트한다.** 그 외는 Play 모드.

## 테스트 실행

```bash
unity test C:\Users\siwon\gmae_project
```

테스트 파일 위치는 Phase 5에서 확정한다 (현재 테스트 없음).

## 툴체인 함정 (실제로 겪은 것)

- **에디터를 백그라운드 Bash 작업으로 띄우지 말 것.** 작업이 끝나면 자식 프로세스인 에디터까지 죽는다. PowerShell `Start-Process`로 세션과 분리해 띄운다.
- **`Packages/manifest.json`을 바꾼 뒤에는 에디터 재시작이 필요하다.** 부팅 중에 바꾸면 무시된다.
- Git Bash에서 `tasklist /FI "IMAGENAME eq ..."` 필터는 오작동한다. `tasklist | grep`으로 확인하고 `head`로 자르지 않는다 (Unity Hub.exe 항목이 여러 개라 Unity.exe 행이 잘려 나간다).
- `unity command eval`은 **파일이 아니라 문장 블록**을 컴파일한다. `using` 선언이나 클래스 정의를 넣으면 컴파일 에러다. 전체 경로로 타입을 쓴다.

## 아직 정해지지 않은 것

- [ ] **Phase 5** — 테스트 어셈블리 위치와 `.asmdef` 구성 (`Assets/_Project/Tests/EditMode/`?)
- [ ] **Phase 5** — 세이브 스키마 버전이 올라갈 때 구버전 로드 테스트를 어떻게 유지할지
