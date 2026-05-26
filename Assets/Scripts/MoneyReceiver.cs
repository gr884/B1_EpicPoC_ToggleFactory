using UnityEngine;

public class MoneyReceiver : MonoBehaviour, ISignalReceiver
{
    [Header("Receiver Settings")]
    [Tooltip("생산할 골드량")]
    public int goldAmount = 10;

    private Vector2Int myPos;
    private bool isOn = false;

    public void Initialize(Vector2Int pos, Vector2Int dir)
    {
        myPos = pos;
    }

    public void RecieveSignal()
    {
        isOn = !isOn;

        // 켜졌을 때
        if (isOn)
        {
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.AddGold(goldAmount);

            //! 임시 시각적 피드백
            GetComponent<SpriteRenderer>().color = Color.yellow;
            Debug.Log($"[{myPos}] 수신기가 켜져 {goldAmount}원 획득");
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            Debug.Log($"[{myPos}] 수신기 꺼짐");
        }
    }

    //* newPos로 위치 변경
    public void UpdatePosition(Vector2Int newPos)
    {
        myPos = newPos;
    }

    public void UpdatePositionAndDirection(Vector2Int newPos, Vector2Int newDir)
    {
        myPos = newPos;
    }
}
