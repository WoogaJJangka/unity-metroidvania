using UnityEngine;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;

namespace Game.World
{
    /// <summary>
    /// 픽셀 퍼펙트 렌더링에서 캐릭터가 화면 위에서 1픽셀씩 떠는 것을 없앤다.
    ///
    /// 원인: PixelPerfectCamera는 스프라이트와 카메라를 각각 따로 픽셀 격자에 반올림한다.
    /// 화면상 위치 = round(캐릭터) - round(카메라) 인데, 둘의 간격 소수부가 0.5 근처면
    /// 두 반올림이 서로 다른 프레임에 넘어가면서 매 프레임 ±1픽셀로 튄다.
    /// 카메라가 댐핑으로 따라붙는 동안(= 가감속할 때마다) 계속 이 구간을 지난다.
    /// 실측: 평지 정속 이동 중 100프레임에 방향 전환 73회.
    ///
    /// 해결: 카메라를 '타깃에서 정수 픽셀만큼' 떨어진 자리로 옮긴다.
    /// 그러면 round(캐릭터) - round(카메라)가 항상 정확한 정수라 반올림이 어긋날 수 없다.
    /// (`Player/Visual`의 로컬 오프셋도 정수 픽셀이어야 성립한다. -0.875 = -14픽셀)
    ///
    /// **주의 — 이 잠금은 오차를 없애는 게 아니라 카메라 쪽으로 옮긴다.**
    /// 화면상 위치는 정수여야 하는데 실제 간격은 소수다. 캐릭터를 화면에 박아두면 그 소수부가
    /// 갈 곳은 카메라뿐이다. 그래서 **타깃과의 간격이 변하는 동안에는 카메라가 진행 방향과
    /// 반대로 한 픽셀씩 튄다.** 배경 전체가 같이 움직이므로 "화면 흔들림"으로 보인다.
    /// 실측(45도 경사): 세로 댐핑 0.6에서 오르막 8회 / 내리막 15회 역방향.
    ///
    /// 그래서 진짜 해결은 이 코드가 아니라 **간격을 안 변하게 두는 것**이다.
    /// `CinemachinePositionComposer`의 세로 댐핑을 0으로 두고 세로 데드존을 넉넉히 잡았다
    /// (0.5). 그러면 데드존 안에서는 카메라가 아예 안 움직이고, 밖에서는 타깃과 같은 속도로
    /// 따라가므로 간격이 상수다. 실측: 오르막 8 -> 0회, 내리막 15 -> 2회.
    /// **세로 댐핑을 다시 올리면 경사에서 화면이 떤다.** 부드럽게 만들고 싶으면 데드존을 키울 것.
    ///
    /// CinemachinePixelPerfect는 직교 크기만 고친다. 위치는 아무도 안 맞춰준다.
    /// </summary>
    [AddComponentMenu("Cinemachine/Procedural/Extensions/Camera Pixel Lock")]
    [DisallowMultipleComponent]
    public class CameraPixelLock : CinemachineExtension
    {
        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Finalize) return;

            Transform target = vcam.Follow;
            if (target == null) return;

            var brain = CinemachineCore.FindPotentialTargetBrain(vcam);
            if (brain == null || brain.OutputCamera == null) return;
            if (!brain.OutputCamera.TryGetComponent(out PixelPerfectCamera ppc)) return;
            if (!ppc.isActiveAndEnabled || ppc.assetsPPU <= 0) return;

            float unitsPerPixel = 1f / ppc.assetsPPU;
            Vector3 pos = state.RawPosition;
            Vector3 t = target.position;

            // 카메라를 타깃에서 '정수 픽셀'만큼 떨어진 자리에 놓는다. 격자에 올리는 일 자체는
            // PixelPerfectCamera가 마지막에 한다 — 여기서 미리 올려도 결과는 같다.
            pos.x = t.x + Mathf.Round((pos.x - t.x) / unitsPerPixel) * unitsPerPixel;
            pos.y = t.y + Mathf.Round((pos.y - t.y) / unitsPerPixel) * unitsPerPixel;

            state.RawPosition = pos;
        }
    }
}
