using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using Game.Combat;
using Game.Enemy;

namespace Game.EditorTools
{
    /// <summary>
    /// 테스트 맵의 원본. 씬을 손으로 고치지 말고 여기 숫자를 고친 뒤 다시 굽는다.
    /// 메뉴: Tools/테스트 맵 다시 굽기
    ///
    /// 구간은 왼쪽에서 오른쪽으로 1~5. 숫자키 1~5로 각 구간 시작점에 순간이동한다(DebugWarp).
    /// 구덩이에 빠지면 y=-4의 회수 바닥에 떨어진다. 걸어 나오지 말고 숫자키로 돌아올 것.
    ///
    /// 경사는 45도만 쓴다. 램프의 낮은 쪽 끝은 항상 바닥 콜라이더 안으로 묻는다 —
    /// 끝점이 바닥 표면에 정확히 걸치면 꼭짓점이 노출되고 캡슐이 거기 부딪혀 진입 속도를 잃는다.
    /// </summary>
    public static class TestMapBuilder
    {
        // ── 구간 경계 (x) ──
        const float Z1 = 0f, Z2 = 82f, Z3 = 202f, Z4 = 322f, Z5 = 484f, END = 648f;

        // ── Zone5 경사 시험 레인 ──
        // 8칸 45도 언덕에서 내려와 정지 더미 3기를 차례로 스친다. 속도 비례 피해를 재는 곳이라
        // 더미 간격이 곧 측정 지점이다 — 내리막을 벗어난 뒤 groundMomentumDecel(35)로 깎이므로
        // v² = v0² - 2 x 35 x 거리. 진입 46 기준 대략 45 / 36 / 22 에서 맞는다.
        const float LaneX0 = 578f;                // 레인 왼쪽 끝 (아레나와 2유닛 틈)
        const float LaneRampUp = 582f;            // 오르막 낮은 끝
        const float LaneFoot = LaneRampUp + 24f;  // 내리막이 평지에 닿는 x (= 606)
        const float LaneX1 = 646f;
        static readonly float[] DummyOffsets = { 2f, 12f, 24f };

        const float FloorTop = 0f;       // 주 바닥 윗면
        const float FloorThick = 12f;    // 화면 아래로 뚫고 나갈 만큼. 바닥이 공중에 뜬 판자로 안 보이게 한다
        const float RecoverTop = -4f;    // 구덩이에 빠졌을 때 받아주는 바닥

        static readonly Color C1 = new Color(0.28f, 0.30f, 0.34f);      // 기본
        static readonly Color C2 = new Color(0.24f, 0.34f, 0.28f);      // 점프
        static readonly Color C3 = new Color(0.38f, 0.30f, 0.22f);      // 슬라이드
        static readonly Color C4 = new Color(0.32f, 0.26f, 0.40f);      // 경사
        static readonly Color C5 = new Color(0.40f, 0.24f, 0.26f);      // 적
        static readonly Color CDeep = new Color(0.16f, 0.17f, 0.20f);   // 회수 바닥
        static readonly Color CProp = new Color(0.46f, 0.48f, 0.52f);   // 장애물
        static readonly Color CWater = new Color(0.25f, 0.55f, 0.85f, 0.45f);  // 냉각 지형
        static readonly Color CHot = new Color(0.90f, 0.35f, 0.15f, 0.45f);    // 가열 지형

        static Sprite _white;
        static Sprite _block;    // 윗면 + 좌우 벽면에 테두리. 밑면은 열려 있다
        static Sprite _seam;     // 윗면 테두리만. 지형끼리 맞닿는 면(언덕 데크)용
        static Transform _level;
        static int _groundLayer;

        // 경사면은 타일맵이 그린다. 회전 스프라이트로 그리면 흙 결이 45도로 돌아가 평지와 어긋난다.
        // 충돌은 여전히 회전 박스가 맡는다 — 타일맵에는 콜라이더가 없다.
        static Tilemap _tilemap;
        static Tile _edgeR, _edgeL, _seamR, _seamL, _fill, _top;

        [MenuItem("Tools/테스트 맵 다시 굽기")]
        public static void Build()
        {
            _white = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/WhiteSquare.png");
            _block = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Tilesets/TILE_Test_Ground_Block.png");
            _seam = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Tilesets/TILE_Test_Ground_NoSide.png");
            SetUpTiles();
            _groundLayer = LayerMask.NameToLayer("Ground");

            var levelGo = GameObject.Find("/Level");
            if (levelGo == null) levelGo = new GameObject("Level");
            _level = levelGo.transform;
            for (int i = _level.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(_level.GetChild(i).gameObject);

            ClearTilemap();
            Bounds();
            Zone1Basics();
            Zone2Jump();
            Zone3Slide();
            Zone4Slope();
            Zone5Enemy();
            Markers();
            PlaceActors();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(levelGo.scene);
            Debug.Log("[TestMapBuilder] 맵을 다시 구웠다. 구간 5개, 폭 " + (END - Z1) + " 유닛.");
        }

        // ─────────────────────────── 구간 ───────────────────────────

        static void Bounds()
        {
            var g = Group("Bounds");
            Box(g, "Wall_L", -3f, 3f, 2f, 22f, CProp);
            Box(g, "Wall_R", END + 1f, 3f, 2f, 22f, CProp);
            // 구덩이를 바닥 없는 낭떠러지로 두면 떨어진 뒤 아무것도 못 한다.
            // 맨 뒤. 구덩이 틈으로만 보여야 한다. (앞에서부터 0 지형·램프 / -1 바닥 / -3 회수 바닥)
            Floor(g, "RecoveryFloor", -2f, END, RecoverTop, CDeep)
                .GetComponent<SpriteRenderer>().sortingOrder = -3;
        }

        static void Zone1Basics()
        {
            var g = Group("Zone1_기본");
            Floor(g, "Floor", Z1, Z2 - 2f, FloorTop, C1);

            // 걸어 올라가지는 계단 3개 + 점프해야 넘는 벽 1개
            Box(g, "Step_1u", 25f, 0.5f, 2f, 1f, CProp);
            Box(g, "Step_2u", 29f, 1.0f, 2f, 2f, CProp);
            Box(g, "Step_3u", 33f, 1.5f, 2f, 3f, CProp);
            Box(g, "Wall_4u", 37f, 2.0f, 2f, 4f, CProp);

            // 모서리 보정 시험: 머리 한쪽만 걸리는 천장 틈 (틈 폭 2)
            Box(g, "Ceiling_L", 48f, 4.5f, 8f, 1f, CProp);
            Box(g, "Ceiling_R", 58f, 4.5f, 8f, 1f, CProp);

            Box(g, "Bump_1u", 68.5f, 0.5f, 1f, 1f, CProp);
            Box(g, "Bump_2u", 72.5f, 1.0f, 1f, 2f, CProp);
        }

        static void Zone2Jump()
        {
            var g = Group("Zone2_점프");
            // 구덩이 폭을 2 → 6으로 늘린다. 평지 최대 점프 거리가 약 5라 마지막 두 개는 슬라이드가 필요하다.
            Floor(g, "Floor_a", Z2, 100f, FloorTop, C2);
            Floor(g, "Floor_b", 102f, 112f, FloorTop, C2);    // 앞 구덩이 2
            Floor(g, "Floor_c", 115f, 125f, FloorTop, C2);    // 앞 구덩이 3
            Floor(g, "Floor_d", 129f, 139f, FloorTop, C2);    // 앞 구덩이 4
            Floor(g, "Floor_e", 144f, 156f, FloorTop, C2);    // 앞 구덩이 5
            Floor(g, "Floor_f", 162f, Z3 - 2f, FloorTop, C2); // 앞 구덩이 6

            // 올라서는 높이 1~5. 4부터는 그냥 점프로 안 되고 계단으로 이어 붙여야 한다.
            Box(g, "Ledge_1u", 167.5f, 0.5f, 3f, 1f, CProp);
            Box(g, "Ledge_2u", 173.5f, 1.0f, 3f, 2f, CProp);
            Box(g, "Ledge_3u", 179.5f, 1.5f, 3f, 3f, CProp);
            Box(g, "Ledge_4u", 185.5f, 2.0f, 3f, 4f, CProp);
            Box(g, "Ledge_5u", 191.5f, 2.5f, 3f, 5f, CProp);
        }

        static void Zone3Slide()
        {
            var g = Group("Zone3_슬라이드");
            Floor(g, "Runway", Z3, 240f, FloorTop, C3);       // 속도를 붙이는 직선
            Floor(g, "Landing", 249f, 272f, FloorTop, C3);    // 앞 구덩이 9 — 슬라이드 점프 전용
            Floor(g, "Chain_a", 279f, 292f, FloorTop, C3);    // 앞 구덩이 7
            Floor(g, "Chain_b", 299f, Z4 - 2f, FloorTop, C3); // 앞 구덩이 7

            // 뛰면 머리를 박는 복도. '점프하지 말고 달려서 통과' 시험.
            Box(g, "LowCeiling", 311.5f, 3.5f, 13f, 1f, CProp);

            // 과열 지형. 활주로 위에 나란히 둬서 같은 속도로 지나며 차이를 본다.
            HeatZoneBox(g, "WaterPool", 214f, 8f, 0f, CWater);   // 안 쌓인다 = 쉼터
            HeatZoneBox(g, "HotZone", 228f, 8f, 3f, CHot);       // 세 배로 쌓인다
        }

        /// <summary>과열 배수를 바꾸는 구역. 콜라이더는 트리거라 달리는 데 방해가 없다.</summary>
        static void HeatZoneBox(Transform parent, string name, float x0, float w, float mul, Color c)
        {
            var go = Box(parent, name, x0 + w * 0.5f, 2f, w, 4f, c, false, false);
            go.GetComponent<SpriteRenderer>().sortingOrder = -2;   // 지형 뒤, 바닥 앞
            go.AddComponent<BoxCollider2D>().isTrigger = true;

            // rateMultiplier는 private [SerializeField]다. SerializedObject로 쓴다.
            var so = new SerializedObject(go.AddComponent<Game.World.HeatZone>());
            so.FindProperty("rateMultiplier").floatValue = mul;
            so.ApplyModifiedProperties();
        }

        static void Zone4Slope()
        {
            var g = Group("Zone4_경사45");
            Floor(g, "Approach", Z4, 382f, FloorTop, C4);
            Floor(g, "Runout", 402f, Z5 - 4f, FloorTop, C4);

            // 오르막 → 평지 → 내리막. 내려온 뒤 긴 평지에서 모멘텀이 얼마나 가는지 본다.
            Ramp(g, "RampUp_4", 340f, 0f, 4f, true, C4);
            Box(g, "Deck_4", 350f, 1f, 12f, 6f, C4, true, true, false);   // 좌우는 램프에 맞닿는다
            Ramp(g, "RampDown_4", 356f, 4f, 4f, false, C4);

            // V자 골짜기. 바닥은 회수 바닥(-4)을 그대로 쓴다.
            Ramp(g, "ValleyDown", 382f, 0f, 4f, false, C4);
            Ramp(g, "ValleyUp", 398f, -4f, 4f, true, C4);

            // 골짜기 어깨. 바닥 조각의 끝면이 경사에 맞닿아 벽면 타일이 언덕 속에 세로로 남는다.
            // 그 한 열만 평지 윗면 + 흙으로 덮는다. (바닥 안쪽 열에 찍으면 원래 그림과 똑같아 무해하다)
            CapFloorEdge(381, 0);   // Approach 오른쪽 끝
            CapFloorEdge(402, 0);   // Runout 왼쪽 끝

            // 큰 경사(8). 45도 내리막 슬라이드가 어디까지 붙는지 재는 곳.
            Ramp(g, "RampUp_8", 420f, 0f, 8f, true, C4);
            Box(g, "Deck_8", 432f, 3f, 8f, 10f, C4, true, true, false);   // 좌우는 램프에 맞닿는다
            Ramp(g, "RampDown_8", 436f, 8f, 8f, false, C4);
        }

        static void Zone5Enemy()
        {
            var g = Group("Zone5_적");
            // 양 끝을 구덩이로 끊는다. 적은 발밑 낭떠러지에서 스스로 돌아서므로 구간 밖으로 안 나간다.
            Floor(g, "Arena", Z5, 576f, FloorTop, C5);
            Box(g, "ShooterDeck", 517f, 1.5f, 10f, 3f, C5);
            Box(g, "Cover_L", 496.5f, 1f, 1f, 2f, CProp);
            Box(g, "Cover_R", 545.5f, 1f, 1f, 2f, CProp);

            // ── 경사 시험 레인 ──
            // 아레나와 2유닛 틈을 둔다. 적은 발밑 낭떠러지에서 돌아서므로 레인으로 넘어오지 않는다.
            Floor(g, "SlopeLane", LaneX0, LaneX1, FloorTop, C5);
            Ramp(g, "LaneRampUp_8", LaneRampUp, 0f, 8f, true, C5);
            Box(g, "LaneDeck", LaneRampUp + 12f, 3f, 8f, 10f, C5, true, true, false);
            Ramp(g, "LaneRampDown_8", LaneRampUp + 16f, 8f, 8f, false, C5);

            // 램프가 바닥 조각의 끝면에 맞닿는 자리. 안 덮으면 벽면 타일이 언덕 속에 세로로 남는다.
            CapFloorEdge(Mathf.RoundToInt(LaneX0), 0);

            // 걸어서 오면 한참이다. 숫자키 6으로 바로 온다 (DebugWarp가 ZoneStart_N을 찾는다).
            var stop = new GameObject("ZoneStart_6");
            stop.transform.SetParent(g, false);
            stop.transform.localPosition = new Vector3(LaneX0 + 2f, 1.5f, 0f);
        }

        /// <summary>
        /// 가만히 서서 맞아주는 적의 설정. 순찰·추격 속도가 0이고 감지 거리도 0이라
        /// 상태가 Patrol에서 안 변한다. 속도 비례 피해를 재려면 대상이 안 움직여야 한다 —
        /// 움직이는 적은 어느 속도에서 맞았는지 알 수가 없다.
        /// 없으면 만들고 있으면 갱신한다. 지워도 다시 생긴다.
        /// </summary>
        static EnemyConfig DummyConfigAsset()
        {
            const string path = "Assets/_Project/Data/DummyConfig.asset";
            var cfg = AssetDatabase.LoadAssetAtPath<EnemyConfig>(path);
            if (cfg == null)
            {
                cfg = ScriptableObject.CreateInstance<EnemyConfig>();
                AssetDatabase.CreateAsset(cfg, path);
            }
            cfg.patrolSpeed = 0f;
            cfg.chaseSpeed = 0f;
            cfg.detectRange = 0f;
            cfg.loseRange = 0.1f;    // detectRange와 같으면 경계에서 상태가 덜덜 떤다
            cfg.attackRange = 0f;
            EditorUtility.SetDirty(cfg);
            return cfg;
        }

        // ─────────────────────────── 표식과 배우 ───────────────────────────

        static void Markers()
        {
            var g = Group("ZoneMarkers");
            float[] xs = { Z1, Z2, Z3, Z4, Z5 };
            Color[] cs = { C1, C2, C3, C4, C5 };
            for (int z = 0; z < xs.Length; z++)
            {
                Color bright = cs[z] * 2.2f;
                bright.a = 1f;
                Box(g, "Pillar_" + (z + 1), xs[z] + 0.25f, 6f, 0.5f, 12f, bright, false, false);
                // 구간 번호를 칸 수로 표시한다. 카메라 시야(세로 13.5)에 들어오게 낮게 둔다.
                for (int i = 0; i <= z; i++)
                    Box(g, "Mark_" + (z + 1) + "_" + i, xs[z] + 1.5f + i * 1.2f, 6f, 0.8f, 0.8f, bright, false, false);

                var stop = new GameObject("ZoneStart_" + (z + 1));
                stop.transform.SetParent(g, false);
                stop.transform.localPosition = new Vector3(xs[z] + 3f, 1.5f, 0f);
            }
        }

        static void PlaceActors()
        {
            // 플레이어와 포털은 1구간에, 적은 5구간에 둔다.
            Move("Player", new Vector3(Z1 + 3f, 1.5f, 0f));
            Move("SpawnPoint_from_B", new Vector3(Z1 + 6f, 1f, 0f));
            Move("Portal_to_B", new Vector3(Z2 - 6f, 1f, 0f));
            RestoreCamera();

            var enemies = GameObject.Find("/Enemies");
            if (enemies == null) return;

            // 이전에 구운 사본을 지운다. 다시 구울 때마다 쌓이면 안 된다.
            for (int i = enemies.transform.childCount - 1; i >= 0; i--)
            {
                var c = enemies.transform.GetChild(i).gameObject;
                if (c.name.EndsWith("(clone)")) Object.DestroyImmediate(c);
            }

            var chaser = GameObject.Find("/Enemies/Enemy_Chaser");
            var shooter = GameObject.Find("/Enemies/Enemy_Shooter");
            if (chaser == null || shooter == null) return;

            // 콜라이더가 1x1 중심이라 바닥 윗면 + 0.5가 정확한 안착 높이다.
            chaser.transform.position = new Vector3(Z5 + 8f, 0.5f, 0f);
            shooter.transform.position = new Vector3(517f, 3.5f, 0f);   // ShooterDeck 윗면 y=3

            var c2 = Object.Instantiate(chaser, new Vector3(556f, 0.5f, 0f), Quaternion.identity, enemies.transform);
            c2.name = "Enemy_Chaser_2(clone)";
            var s2 = Object.Instantiate(shooter, new Vector3(570f, 0.5f, 0f), Quaternion.identity, enemies.transform);
            s2.name = "Enemy_Shooter_2(clone)";

            PlaceDummies(chaser, enemies.transform);

            // 속도 비례 피해는 눈에 안 보인다. 체력 UI가 생기기 전까지 이 표시가 대신한다.
            var tools = GameObject.Find("DebugTools");
            if (tools != null && tools.GetComponent<Game.Utils.DebugStats>() == null)
                tools.AddComponent<Game.Utils.DebugStats>();
        }

        /// <summary>정지 더미 3기. 근접 적을 복제해 설정만 갈아 끼운다 — 콜라이더·레이어·
        /// 접촉 히트박스 배선을 통째로 다시 만들 이유가 없다.</summary>
        static void PlaceDummies(GameObject source, Transform parent)
        {
            var cfg = DummyConfigAsset();
            for (int i = 0; i < DummyOffsets.Length; i++)
            {
                var d = Object.Instantiate(source, new Vector3(LaneFoot + DummyOffsets[i], 0.5f, 0f),
                                           Quaternion.identity, parent);
                d.name = "Dummy_" + (i + 1) + "(clone)";

                // config와 maxHp는 private [SerializeField]다. SerializedObject로 쓴다.
                var ai = new SerializedObject(d.GetComponent<EnemyAI>());
                ai.FindProperty("config").objectReferenceValue = cfg;
                ai.ApplyModifiedProperties();

                // 몇 번을 때려도 안 죽어야 반복 측정이 된다.
                var hp = new SerializedObject(d.GetComponent<Health>());
                hp.FindProperty("maxHp").floatValue = 999f;
                hp.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// CinemachineBrain을 켜고 카메라를 vcam 자리로 되돌린다.
        /// 에디트 모드에서 카메라를 옮겨 캡처하려면 브레인을 꺼야 하는데(안 그러면 vcam이
        /// 도로 끌고 간다), 그걸 되돌리지 않은 채 씬을 저장하면 <b>플레이해도 카메라가
        /// 그 자리에 그대로 멈춰 있다.</b> 증상은 "캐릭터가 안 보이고 화면이 고정"이다.
        /// 실제로 한 번 이렇게 커밋됐다. 굽기가 씬의 원본이므로 여기서 매번 되돌린다.
        /// </summary>
        static void RestoreCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;

            var brain = cam.GetComponent<Unity.Cinemachine.CinemachineBrain>();
            if (brain != null) brain.enabled = true;

            var vcam = Object.FindAnyObjectByType<Unity.Cinemachine.CinemachineCamera>();
            if (vcam != null)
                cam.transform.position = new Vector3(vcam.transform.position.x,
                                                     vcam.transform.position.y, -10f);
        }

        static void ClearTilemap()
        {
            var tmGo = GameObject.Find("/Grid/Tilemap_Ground");
            _tilemap = tmGo == null ? null : tmGo.GetComponent<Tilemap>();
            if (_tilemap == null) { Debug.LogWarning("[TestMapBuilder] Tilemap_Ground가 없다. 경사면을 못 그린다."); return; }
            _tilemap.GetComponent<TilemapRenderer>().sortingOrder = 0;   // 바닥(-1)보다 앞
            _tilemap.ClearAllTiles();
        }

        /// <summary>
        /// 대각선 타일 에셋을 없으면 만들고 있으면 갱신한다. 좌우 반전은 같은 스프라이트에
        /// 타일의 transform 행렬만 뒤집어 쓴다 — 반전용 PNG를 따로 두지 않는다.
        /// </summary>
        static void SetUpTiles()
        {
            const string slope = "Assets/_Project/Art/Tilesets/TILE_Test_Ground_Slope45.png";
            const string sheet = "Assets/_Project/Art/Tilesets/TILE_Test_Ground.png";
            var edge = SubSprite(slope, "Slope45_Edge");
            var seam = SubSprite(slope, "Slope45_Seam");
            var dirt = SubSprite(sheet, "Ground_C");

            _edgeR = MakeTile("Tile_Slope45_Edge_R", edge, false);
            _edgeL = MakeTile("Tile_Slope45_Edge_L", edge, true);
            _seamR = MakeTile("Tile_Slope45_Seam_R", seam, false);
            _seamL = MakeTile("Tile_Slope45_Seam_L", seam, true);
            _fill  = MakeTile("Tile_Ground_Fill", dirt, false);
            _top   = MakeTile("Tile_Ground_Top", SubSprite(sheet, "Ground_T"), false);
        }

        static Sprite SubSprite(string path, string name)
        {
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path))
                if (o is Sprite sp && sp.name == name) return sp;
            Debug.LogWarning("[TestMapBuilder] 스프라이트를 못 찾았다: " + path + " / " + name);
            return null;
        }

        static Tile MakeTile(string assetName, Sprite sprite, bool mirror)
        {
            const string dir = "Assets/_Project/Data/Tiles";
            if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets/_Project/Data", "Tiles");
            string path = dir + "/" + assetName + ".asset";
            var t = AssetDatabase.LoadAssetAtPath<Tile>(path);
            bool isNew = t == null;
            if (isNew) t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = sprite;
            t.colliderType = Tile.ColliderType.None;   // 충돌은 회전 박스가 맡는다
            t.transform = mirror ? Matrix4x4.Scale(new Vector3(-1f, 1f, 1f)) : Matrix4x4.identity;
            t.flags = TileFlags.LockTransform | TileFlags.LockColor;
            if (isNew) AssetDatabase.CreateAsset(t, path);
            else EditorUtility.SetDirty(t);
            return t;
        }

        /// <summary>
        /// 바닥 한 열을 "옆면 없는 평지"로 다시 칠한다. cx는 칸 좌표, topY는 바닥 윗면 높이.
        /// </summary>
        static void CapFloorEdge(int cx, int topY)
        {
            if (_tilemap == null) return;
            _tilemap.SetTile(new Vector3Int(cx, topY - 1, 0), _top);
            for (int y = topY - 2; y >= topY - (int)FloorThick; y--)
                _tilemap.SetTile(new Vector3Int(cx, y, 0), _fill);
        }

        /// <summary>
        /// 45도 계단을 칸 단위로 찍는다. 한 열마다 [대각선] / [이음새] / [흙]으로 쌓는다.
        /// 바닥 한 칸 아래까지 메워야 한다 — 안 그러면 바닥 윗면의 풀 띠가 언덕 속에 가로로 남는다.
        /// </summary>
        static void PaintRamp(int x0, int y0, int h, bool up)
        {
            if (_tilemap == null) return;
            int bottom = (up ? y0 : y0 - h) - 1;
            for (int i = 0; i < h; i++)
            {
                int cx = x0 + i;
                int cy = up ? y0 + i : y0 - 1 - i;   // 표면이 지나는 칸
                _tilemap.SetTile(new Vector3Int(cx, cy, 0), up ? _edgeR : _edgeL);
                _tilemap.SetTile(new Vector3Int(cx, cy - 1, 0), up ? _seamR : _seamL);
                for (int y = cy - 2; y >= bottom; y--)
                    _tilemap.SetTile(new Vector3Int(cx, y, 0), _fill);
            }
        }

        // ─────────────────────────── 조립 도구 ───────────────────────────

        static Transform Group(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_level, false);
            return go.transform;
        }

        static GameObject Floor(Transform parent, string name, float x0, float x1, float top, Color c)
        {
            var go = Box(parent, name, (x0 + x1) * 0.5f, top - FloorThick * 0.5f, x1 - x0, FloorThick, c);
            // 한 층 뒤로 뺀다. 안 그러면 바닥 윗면의 풀 테두리가 그 위에 얹힌 지형을 가로질러 그려진다.
            go.GetComponent<SpriteRenderer>().sortingOrder = -1;
            return go;
        }

        /// <param name="sides">좌우가 밖으로 드러나는 면인가. 다른 지형에 맞닿는 면은 false —
        /// 벽면 타일을 붙이면 지형 속에 풀 이음매가 생긴다.</param>
        static GameObject Box(Transform parent, string name, float cx, float cy, float w, float h,
                              Color c, bool collide = true, bool ground = true, bool sides = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(cx, cy, 0f);
            if (ground) go.layer = _groundLayer;

            var sr = go.AddComponent<SpriteRenderer>();
            if (ground)
            {
                // 흙은 늘리지 말고 타일링한다. 스케일을 쓰면 픽셀이 비정수 배율로 늘어난다.
                // 벽면 테두리가 좌우 1유닛씩이라 폭 2 미만은 이음매용을 쓴다.
                sr.sprite = (sides && w >= 2f) ? _block : _seam;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.tileMode = SpriteTileMode.Continuous;
                sr.size = new Vector2(w, h);
                go.transform.localScale = Vector3.one;
                if (collide) go.AddComponent<BoxCollider2D>().size = new Vector2(w, h);
            }
            else
            {
                sr.sprite = _white;
                sr.color = c;
                go.transform.localScale = new Vector3(w, h, 1f);
                if (collide) go.AddComponent<BoxCollider2D>();
            }
            return go;
        }

        /// <summary>
        /// 45도 램프. (x0,y0)은 램프의 낮은 쪽 표면 끝점. h는 높이차이자 가로 거리다.
        /// up=true면 오른쪽으로 올라가고, false면 오른쪽으로 내려간다.
        /// </summary>
        static void Ramp(Transform parent, string name, float x0, float y0, float h, bool up, Color c)
        {
            const float s = 0.70710678f;
            const float bury = 2f;    // 낮은 쪽 끝을 바닥 안으로 밀어 넣는 길이
            // 높이만큼 두껍게 해야 램프 밑이 삼각형으로 비지 않는다. 표면 위치는 그대로다 —
            // 두께는 법선 반대(아래)로만 자란다.
            float thick = h * 1.41421356f;

            Vector2 dir = up ? new Vector2(s, s) : new Vector2(s, -s);   // 오른쪽 진행 방향
            Vector2 nUp = up ? new Vector2(-s, s) : new Vector2(s, s);   // 표면 바깥(위) 법선
            Vector2 p0 = new Vector2(x0, y0);
            float len = h * 1.41421356f;
            Vector2 p1 = p0 + dir * len;

            Vector2 mid = (p0 + p1) * 0.5f;
            Vector2 center = mid - nUp * (thick * 0.5f);

            var go = Box(parent, name, center.x, center.y, len, thick, c, true, true, false);
            go.transform.localRotation = Quaternion.Euler(0f, 0f, up ? 45f : -45f);

            // 그림은 타일맵이 맡는다. 이 오브젝트는 콜라이더만 남긴다.
            Object.DestroyImmediate(go.GetComponent<SpriteRenderer>());
            PaintRamp(Mathf.RoundToInt(x0), Mathf.RoundToInt(y0), Mathf.RoundToInt(h), up);

            // 묻는 건 콜라이더만이다. 그림까지 묻으면 램프의 풀 띠가 지면 아래로 삐져나온다.
            // 낮은 쪽만 연장한다 — 높은 쪽을 늘리면 평지 위로 튀어나와 턱이 된다.
            var col = go.GetComponent<BoxCollider2D>();
            col.size = new Vector2(len + bury, thick);
            col.offset = new Vector2(up ? -bury * 0.5f : bury * 0.5f, 0f);
        }

        static void Move(string name, Vector3 pos)
        {
            var go = GameObject.Find(name);
            if (go == null) return;
            go.transform.position = pos;
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null) rb.position = pos;
        }
    }
}
