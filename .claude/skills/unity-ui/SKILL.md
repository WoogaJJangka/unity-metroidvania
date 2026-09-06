---
name: unity-ui
description: >
  이 프로젝트의 UI 결정 사항 — HUD 구성, 일시정지·타이틀 화면, 대화창, 한글 폰트 처리,
  Sorting Layer 배치. 이 게임의 UI를 만들거나 고칠 때 사용한다. Canvas/RectTransform/UXML 같은
  Unity UI 시스템 자체의 사용법은 여기가 아니라 unity 플러그인의 ui / ui-ugui / ui-uitk /
  optimize-text-mesh-pro 스킬이 담당한다.
---

# UI 결정 사항

> **이 문서와 코드가 어긋나면 코드가 옳다.**
> 마지막 갱신: 2026-09-06 (Phase 1 진행 중)

## ⚠️ 현재 이 문서는 거의 비어 있다

UI는 **Phase 5**부터 본격적으로 만든다. 지금 확정된 건 아래 두 가지뿐이고, 나머지는 그때 채운다.
빈 항목을 추측으로 채우지 말 것 — 틀린 내용이 들어가는 게 비어 있는 것보다 나쁘다.

## 확정된 것

**한글 폰트가 필요하다.** TextMeshPro는 폰트 에셋을 미리 구워야 글자가 나온다. 한글은 글자 수가
많아 전체를 구우면 아틀라스가 거대해지므로 **Dynamic 폰트 에셋 + 폴백**으로 간다. 구체적인 설정은
unity 플러그인의 `optimize-text-mesh-pro` 스킬을 따른다 (CJK 폰트 폴백을 직접 다룬다).

**Sorting Layer에 `UI`가 있다.** 순서: `Background / Midground / Default / Foreground / UI`.
(Phase 2에서 최종 확정 → 확정되면 `unity-architecture`에도 반영)

## 결정해야 하는 것 — 가장 큰 갈림길

- [ ] **uGUI(Canvas) vs UI Toolkit(UXML/USS)** — Phase 5 시작 전에 정한다.
  - uGUI: 자료가 압도적으로 많고, 월드 공간 UI(체력바를 적 머리 위에)가 쉽다. 2D 게임 HUD의 사실상 표준.
  - UI Toolkit: 레이아웃이 CSS 유사라 구조적이지만, 런타임 월드 공간 UI가 약하다.
  - **현재 기울기: uGUI.** 이 게임은 적 머리 위 체력바·데미지 숫자 같은 월드 공간 요소가 필요하고,
    입문자 기준 막혔을 때 검색으로 풀리는 양이 결정적이다. 다만 아직 확정 아님.

## 만들 것 (Phase 5)

- HUD: 체력, 능력 아이콘
- 일시정지 메뉴
- 타이틀 화면
- (Phase 6) 대화창 — ink 연동

## 아직 정해지지 않은 것

- [ ] **Phase 2** — Sorting Layer 최종 목록 확정
- [ ] **Phase 5** — uGUI / UI Toolkit 선택, HUD가 체력 변화를 구독하는 이벤트 이름
- [ ] **Phase 5** — 일시정지가 `Time.timeScale = 0`인지, 그렇다면 히트스톱과 어떻게 충돌하지 않게 할지
- [ ] **Phase 6** — 대화창이 ink 러너와 붙는 인터페이스

## 이 문서를 갱신하는 시점

Phase 5를 시작할 때 위 갈림길을 먼저 정하고, 정한 이유와 함께 "확정된 것"으로 옮긴다.
