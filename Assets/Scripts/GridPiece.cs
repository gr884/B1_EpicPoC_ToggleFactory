using System.Collections.Generic;
using UnityEngine;

public class GridPiece : MonoBehaviour, ISignalReceiver
{
    [Header("Piece Settings")]
    [Tooltip("필요한 틱 시간")]
    public int requiredCookTicks = 2;
    [Tooltip("신호를 전달할 기본 방향")]
    public Vector2Int baseDirection = Vector2Int.right;

    private Vector2Int myPos;
    private int currentCookTimer;

    private bool isOn = false;      // 켜져있는지
    private bool isCooking = false; // 신호를 보내는 중인지

    // 방출할 방향을 담아둘 리스트
    private List<Vector2Int> outputDirections = new List<Vector2Int>();

    // 업그레이드 상태
    public bool hasVerticalUpgrade { get; private set; } = false;
    public bool hasOppositeUpgrade { get; private set; } = false;

    //* 설치될 때 InteractionManager가 호출
    public void Initialize(Vector2Int pos, Vector2Int dir)
    {
        myPos = pos;
        baseDirection = dir;

        outputDirections.Clear();
        outputDirections.Add(baseDirection);

        // 생성될 때 이벤트 구독
        if (TickManager.Instance != null)
            TickManager.Instance.OnTick += HandleTick;
    }

    //* 파괴될 때 구독 해제
    void OnDestroy()
    {
        if (TickManager.Instance != null)
            TickManager.Instance.OnTick -= HandleTick;
    }

    //* 업그레이드가 하나라도 켜져 있으면 false 반환
    public bool IsUpgraded => hasVerticalUpgrade || hasOppositeUpgrade;

    //* 수직 업그레이드
    public void ApplyUpgradeVertical()
    {
        if (IsUpgraded) return;

        hasVerticalUpgrade = true;

        // 시계 90도 방향의 방향 추가
        Vector2Int verticalDir = new Vector2Int(baseDirection.y, -baseDirection.x);
        outputDirections.Add(verticalDir);
    }

    //* 반대 업그레이드
    public void ApplyUpgradeOpposite()
    {
        if (IsUpgraded) return;

        hasOppositeUpgrade = true;

        // 반대 방향 추가
        Vector2Int oppositeDir = new Vector2Int(-baseDirection.x, -baseDirection.y);
        outputDirections.Add(oppositeDir);
    }

    //* 외부에서 신호가 들어왔을 때 실행
    public void RecieveSignal()
    {
        // 이미 진행중이면 신호 무시
        if (isCooking) return;

        // 켜짐 상태 뒤집기
        isOn = !isOn;

        // 이제 켜졌으면(isOn이 false -> true로 변했다면) 신호 보내기 시작
        if (isOn)
        {
            // 틱 시작 초기설정
            isCooking = true;
            currentCookTimer = requiredCookTicks;

            //! 임시 시각적 표시
            GetComponent<SpriteRenderer>().color = Color.yellow;
            Debug.Log($"[{myPos}] 좌표 기물: 신호 시작");
        }
        // 켜져있다가 신호가 들어와 꺼졌다면(isOn이 true -> false로 변했다면) 변화 x
        else
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            Debug.Log($"[{myPos}] 기물: 꺼짐.");
        }
        
    }

    //* 매 틱마다 TickManager가 자동으로 호출
    private void HandleTick()
    {
        // 쿠킹중인 경우 틱을 줄임
        if (isCooking)
        {
            currentCookTimer--;

            // 타이머가 0초 이하로 줄어들었다면 다음 칸으로 신호 전달 후 켜짐 상태 유지
            if (currentCookTimer <= 0)
            {
                isCooking = false;
                EmitSignal();

                //! 임시 시각적 표시 되돌리기
                GetComponent<SpriteRenderer>().color = new Color(1f, .5f, 0f);
            }
        }
    }

    //* 다음 칸으로 신호 전달
    private void EmitSignal()
    {
        foreach(Vector2Int dir in outputDirections)
        {
            Vector2Int targetPos = myPos + dir;
            GridManager.Instance.SendSignalTo(targetPos);
            Debug.Log($"[{myPos}] 좌표 기물: [{targetPos}] 방향으로 신호 방출.");
        }
    }

    //* newPos로 위치 변경
    public void UpdatePosition(Vector2Int newPos)
    {
        myPos = newPos;
    }

    //* 이동 및 회전 재배치
    public void UpdatePositionAndDirection(Vector2Int newPos, Vector2Int newDir)
    {
        myPos = newPos;
        baseDirection = newDir;

        // 바뀐 방향에 맞게 모든 방향을 재배치
        outputDirections.Clear();
        outputDirections.Add(baseDirection);

        if (hasVerticalUpgrade)
        {
            outputDirections.Add(new Vector2Int(baseDirection.y, -baseDirection.x));
        }
        if (hasOppositeUpgrade)
        {
            outputDirections.Add(new Vector2Int(-baseDirection.x, -baseDirection.y));
        }
    }
}
