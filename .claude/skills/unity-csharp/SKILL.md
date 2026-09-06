---
name: unity-csharp
description: >
  이 프로젝트의 C#·Unity API 작성 관례 — 네이밍, Update와 FixedUpdate 분리, GetComponent 캐싱,
  Unity의 가짜 null, 이벤트 해제, C 경험자가 C#에서 헷갈리는 지점. 스크립트를 새로 쓰거나
  기존 코드를 고칠 때, 코드 리뷰할 때 사용한다. C# 문법 일반 강의가 아니라 이 프로젝트에서
  실제로 지키는 규칙과 실제로 밟은 지뢰만 담는다.
---

# C# 작성 관례

> **이 문서와 코드가 어긋나면 코드가 옳다.**
> 마지막 갱신: 2026-09-06 (Phase 1 진행 중)

## 네이밍

| 대상 | 표기 | 예 |
|---|---|---|
| 클래스·메서드·속성 | `PascalCase` | `PlayerController`, `JumpVelocity` |
| 지역변수·매개변수 | `camelCase` | `maxSpeed`, `deltaTime` |
| private 필드 | `_camelCase` | `_rigidbody`, `_coyoteTimer` |
| public 필드 (SO 튜닝값) | `camelCase` | `jumpHeight` — Inspector에 "Jump Height"로 표시됨 |

## 절대 규칙 (어기면 조용히 깨진다)

### 1. 입력은 `Update`, 물리는 `FixedUpdate`

`FixedUpdate`는 고정 간격으로 호출되므로 한 프레임에 0번 또는 2번 돌 수 있다. 여기서 버튼 눌림을
읽으면 **입력이 씹히거나 두 번 먹는다.** 반대로 `Update`에서 `Rigidbody2D`를 만지면 물리가 떨린다.

```
Update       → 입력 읽기, 타이머 감소(코요테·점프 버퍼), 애니메이션 파라미터
FixedUpdate  → Rigidbody2D 속도 적용, OverlapBox 접지 판정
```

입력을 `Update`에서 읽어 플래그(`_jumpPressed`)에 저장하고, `FixedUpdate`에서 소비한 뒤 내린다.

### 2. `GetComponent`는 `Awake`에서 캐싱

`Update` 안에서 부르지 않는다. C의 포인터 조회와 달리 이름 검색이 섞인 비싼 호출이다.

```csharp
private Rigidbody2D _rigidbody;
private void Awake() => _rigidbody = GetComponent<Rigidbody2D>();
```

### 3. Unity 오브젝트에 `?.` / `??` 를 쓰지 않는다

**이게 이 프로젝트에서 가장 위험한 함정이다.** `Destroy()`된 `UnityEngine.Object`는 `== null`이
true를 반환하지만 **실제 참조는 null이 아니다.** Unity가 `==` 연산자를 오버로드해 "파괴됨"을
null처럼 보이게 만든 것이다. `?.`와 `??`는 C# 언어 기능이라 이 오버로드를 무시하고 진짜 null만
본다 → 파괴된 오브젝트를 살아있다고 판단하고 `MissingReferenceException`이 터진다.

```csharp
if (_target != null) _target.Hit();   // 옳음
_target?.Hit();                        // 틀림 — 파괴된 오브젝트를 통과시킨다
```

### 4. 이벤트는 `OnEnable`에서 걸고 `OnDisable`에서 해제

씬을 오가는 구조라 해제를 빠뜨리면 파괴된 오브젝트가 계속 호출된다. C의 콜백 등록과 달리
GC가 있어도 **구독은 자동으로 풀리지 않는다** — 오히려 구독자가 GC되지 않게 붙잡는다.

### 5. 수치를 코드에 박지 않는다

튜닝 대상 값(속도, 체력, 쿨다운)은 ScriptableObject로 노출한다. → `unity-architecture` 참조

## C 경험자가 걸리는 지점

| C | C# | 주의 |
|---|---|---|
| `struct` = 값 | `struct`도 값, `class`는 **참조** | `class`를 대입하면 복사가 아니라 별칭이다 |
| 매크로 함수 | 식 본문 속성 `=>` | `public float Gravity => (2f * jumpHeight) / (timeToApex * timeToApex);` — 필드가 아니라 읽을 때마다 계산 |
| `enum` = 정수 | `[Flags] enum` = 비트 집합 | Phase 4 `AbilityFlags`에서 사용. `flags.HasFlag(Ability.Dash)` |
| 수동 free | GC | 하지만 **이벤트 구독·코루틴은 수동 해제 대상** |
| `float f = 1.0;` 경고 | `float f = 1.0f;` **필수** | `f` 접미사 없으면 `double`이라 컴파일 에러 |
| 배열 고정 크기 | `List<T>` | 단 `Update` 안에서 `new List<>()` 하지 않는다 (GC 압박) |

## 컴파일 후 반드시

**컴파일 성공 ≠ 동작.** 스크립트를 쓴 뒤 Unity 콘솔을 읽어 에러·경고를 확인한다.

```bash
unity command eval "return string.Join(\"\n\", System.Linq.Enumerable.Empty<string>());"
```

콘솔 확인과 Play 모드 검증 절차는 `unity-testing` 참조.

## 아직 정해지지 않은 것

- [ ] **Phase 3** — `IDamageable` 인터페이스 시그니처와 `DamageInfo` 를 struct로 할지 class로 할지
- [ ] **Phase 3** — 적 FSM을 `enum` + `switch`로 갈지 상태 클래스로 갈지 (계획서는 enum FSM 권장, Behaviour Tree 금지)
- [ ] **Phase 5** — `JsonUtility` 제약 대응: 딕셔너리·다형성 직렬화 불가 → 우회 방식 결정
