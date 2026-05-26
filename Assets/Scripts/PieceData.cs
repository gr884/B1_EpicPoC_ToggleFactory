using UnityEngine;

public class PieceData : MonoBehaviour
{
    [Header("Piece Data")]
    [Tooltip("기물의 설치 비용")]
    public int buildCost = 50;

    [Tooltip("수직 업그레이드 비용")]
    public int verticalUpgradeCost = 30;

    [Tooltip("반대 업그레이드 비용")]
    public int oppositeUpgradeCost = 30;

    [Tooltip("이 기물에 투자된 총 비용")]
    public int totalInvestedGold;

    void Awake()
    {
        totalInvestedGold = buildCost;
    }

    //* cost만큼 업그레이드 시 그만큼 누적 비용 추가
    public void AddUpgradeCost(int cost)
    {
        totalInvestedGold += cost;
    }
}
