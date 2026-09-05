# unity-metroidvania

Unity 6 기반 2D 메트로배니아. 맵 전환형 오픈월드에 전투·탐험·능력 해금·스토리 퀘스트·미니게임을 결합하는 것을 목표로 개발 중입니다.

> 개발 초기 단계입니다. 현재 Phase 0(환경 구축)이 완료된 상태이며 플레이 가능한 빌드는 아직 없습니다.

## 개발 환경

| 항목 | 버전 |
|---|---|
| Unity | 6000.6.0f1 (Unity 6.6) |
| 렌더 파이프라인 | URP 17.6.0 (2D Renderer) |
| 입력 | Input System 1.20.0 |
| 카메라 | Cinemachine 3.1.7 |

## 시작하기

```bash
git clone https://github.com/<owner>/unity-metroidvania.git
cd unity-metroidvania
git lfs pull
```

Unity Hub에서 `6000.6.0f1` 버전으로 프로젝트를 엽니다. `Library/`는 저장소에 포함되지 않으므로 첫 실행 시 에셋 임포트에 몇 분이 걸립니다.

**Git LFS가 필요합니다.** 이미지·오디오·폰트가 LFS로 관리되므로, `git lfs install`을 하지 않고 클론하면 해당 파일들이 포인터 텍스트로만 받아집니다.

## 프로젝트 구조

```
Assets/
  _Project/        직접 작성한 모든 것
    Art/           Sprites, Tilesets, Animations, UI
    Audio/         BGM, SFX
    Code/          Player, Enemy, Combat, World, Quest, UI, Save, Utils
    Data/          ScriptableObject 에셋 (튜닝 수치는 전부 여기로 노출)
    Prefabs/       Player, Enemies, Props, UI
    Scenes/        Maps/, MiniGames/
  Settings/        URP 에셋 + Input Actions (Unity 템플릿 생성)
  ThirdParty/      외부 에셋. 직접 작성한 코드와 분리해 관리
```

자세한 개발 규칙(코딩 컨벤션, 씬 구조, 작업 방식)은 [CLAUDE.md](CLAUDE.md)를 참고하세요.

## 브랜치 전략

- `main` — 항상 동작하는 상태를 유지합니다. Phase 완료 시 태그를 남깁니다 (`phase-0-setup`, `phase-1-movement`, …)
- `feature/*` — 기능 단위 작업 브랜치. 완료 후 `main`에 병합합니다

## 라이선스

**아직 정해지지 않았습니다.** 코드와 에셋의 라이선스가 다를 수 있어 별도로 정리할 예정입니다.

외부 에셋을 도입하면 `Assets/ThirdParty/` 아래에 출처와 라이선스를 함께 기록합니다. 현재 포함된 외부 콘텐츠는 Unity Universal 2D 템플릿의 기본 파일(`Assets/Welcome/`, `Assets/Settings/`)뿐입니다.
