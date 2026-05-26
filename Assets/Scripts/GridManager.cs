using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    private Dictionary<Vector2Int, GameObject> grid = new Dictionary<Vector2Int, GameObject>();

    void Awake()
    {
        Instance = this;
    }

    //* pos 좌표가 비어있는지 확인
    public bool IsTileEmpty(Vector2Int pos)
    {
        return !grid.ContainsKey(pos);
    }

    //* 설치된 기물을 grid에 등록
    public void RegisterPiece(Vector2Int pos, GameObject piece)
    {
        grid.Add(pos, piece);
    }

    //* pos 좌표에 신호 전달
    public void SendSignalTo(Vector2Int pos)
    {
        // 그리드의 pos에 타일이 존재하는지 확인하고 있으면 신호 전달
        if (grid.TryGetValue(pos, out GameObject pieceObj))
        {
            ISignalReceiver receiver = pieceObj.GetComponent<ISignalReceiver>();
            if (receiver != null)
                receiver.RecieveSignal();
        }
    }

    //* pos 좌표의 기물 오브젝트를 반환
    public GameObject GetPieceAt(Vector2Int pos)
    {
        if (grid.TryGetValue(pos, out GameObject pieceObj))
        {
            return pieceObj;
        }

        return null;
    }

    //* pos의 기물을 제거하고 돈 회수
    public void RemovePiece(Vector2Int pos)
    {
        // pos의 기물 확인
        if (grid.TryGetValue(pos, out GameObject pieceObj))
        {
            // 해당 기물에 사용된 돈 계산
            PieceData data = pieceObj.GetComponent<PieceData>();
            int refundAmount = 0;

            if (data != null)
            {
                refundAmount = data.totalInvestedGold;
            }
            
            // 계산된 돈 환원 및 기물 제거
            ResourceManager.Instance.AddGold(refundAmount);

            grid.Remove(pos);
            Destroy(pieceObj);
        }
    }

    //* pos의 기물을 grid에서 임시 제거
    public void UnregisterPiece(Vector2Int pos)
    {
        if (grid.ContainsKey(pos))
        {
            grid.Remove(pos);
        }
    }
}
