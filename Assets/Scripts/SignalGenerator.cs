using UnityEngine;

public class SignalGenerator : MonoBehaviour
{
    [Header("Generator Settings")]
    [Tooltip("생성기가 신호를 생성할 틱")]
    public int generateTickInterval = 4;
    [Tooltip("신호를 방출할 방향")]
    public Vector2Int currentDirection = Vector2Int.right;

    private Vector2Int myPos;
    private int currentTimer;

    //* 설치될 때 초기화
    public void Initialize(Vector2Int pos, Vector2Int dir)
    {
        myPos = pos;
        currentDirection = dir;
        currentTimer = generateTickInterval;

        if (TickManager.Instance != null)
            TickManager.Instance.OnTick += HandleTick;
    }

    //* 제거될 때 구독 취소
    void OnDestroy()
    {
        if (TickManager.Instance != null)
            TickManager.Instance.OnTick -= HandleTick;
    }

    //* 틱마다 타이머 낮추기
    private void HandleTick()
    {
        currentTimer--;

        // 타이머가 다 됬으면 리셋 후 신호 방출
        if (currentTimer <= 0)
        {
            currentTimer = generateTickInterval;
            EmitSignal();
        }
    }

    //* 신호 방출
    private void EmitSignal()
    {
        Vector2Int targetPos = myPos + currentDirection;
        Debug.Log($"[{myPos}] 발생기: [{targetPos}] 방향으로 신호 발생");

        //! 임시 시각적 피드백
        GetComponent<SpriteRenderer>().color = Color.cyan;
        Invoke("ResetColor", 0.2f);

        // 다음 칸으로 신호 발사
        GridManager.Instance.SendSignalTo(targetPos);
    }

    private void ResetColor()
    {
        GetComponent<SpriteRenderer>().color = Color.softGreen;
    }

    //* newPos로 위치 변경
    public void UpdatePosition(Vector2Int newPos)
    {
        myPos = newPos;
    }

    //* newPos 좌표와 newDir에 맞게 방향 재설정
    public void UpdatePositionAndDirection(Vector2Int newPos, Vector2Int newDir)
    {
        myPos = newPos;
        currentDirection = newDir;
    }
}
