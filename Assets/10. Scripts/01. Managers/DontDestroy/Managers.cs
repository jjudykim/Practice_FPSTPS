using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Managers : SingletonBase<Managers>
{
    public GameManager Game { get; private set; }
    public InputManager Input { get; private set; }
    public SaveDataManager SaveData { get; private set; }
    public ExSceneManager Scene { get; private set; }
    public CombatManager Combat { get; private set; }
    public FloatingTextManager FloatingText { get; private set; }
    
    protected override bool AllowAutoCreate => true;
    protected override void OnInitialize()
    {
        base.OnInitialize();

        Game = new GameManager();
        Input = new InputManager();
        SaveData = new SaveDataManager();
        Combat = new CombatManager();
        Scene = new ExSceneManager();
        FloatingText = new FloatingTextManager();
        
        Scene.Init();
        FloatingText.Init();
        Game.Init();
        
        Game.Awake();
    }
    
    private void Update()
    {
        Input.Update();
    }
}

  [시스템 및 매니저]
  - GameManager 내 게임 상태(준비, 진행, 승리, 패배) 관리 로직 추가
  - 페이드 효과를 포함한 씬 로딩 시스템(ExSceneManager) 구현
  - 데미지 표시를 위한 FloatingTextManager 및 오브젝트 풀링 추가
  - SaveDataManager 내 주요 재화 및 성장 지표를 Observable 형태로 개선
 
  [전투 및 콘텐츠]
  - 적 처치 시 골드/경험치 아이템 드랍 및 플레이어 흡수 로직 구현
  - 플레이어 구르기 거리 조정 및 구르기 중 무적 판정 추가
  - 엘리트 적 처치 시 게임 클리어 트리거 연동
  - 보상 방(Reward) 및 상점 방(Shop) 컨트롤러 기초 구현
 
  [UI 및 편의성]
  - 게임 결과 UI(승리/패배) 및 마을 복귀 기능 추가
  - 로비 내 플레이어 이름 입력 시스템 및 저장 슬롯 상세 정보 표시
  - 월드 스페이스 상호작용 UI 가독성 개선 (카메라 방향 응시)
  - 일시정지(Pause) 메뉴 및 메뉴 호출 입력(ESC) 추가
 
  [데이터 및 환경]
  - 아이템 데이터 구조 확장 (아이콘, 스택 제한, 효과 수치 등)
  - 씬 내 컨트롤러 참조 오류 수정 및 UI 레이아웃 최적화