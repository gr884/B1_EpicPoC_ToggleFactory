using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("Gold Settings")]
    [Tooltip("시작 시 소지 골드")]
    public int currentGold = 200;

    [Header("UI Reference")]
    [Tooltip("화면에 표시될 골드 텍스트")]
    public TextMeshProUGUI goldText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateGoldUI();
    }

    //* amount만큼 골드를 추가
    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
        Debug.Log($"{amount}골드 획득. 현재 금액: {currentGold}");
    }

    //* amount만큼 골드 사용
    public bool UseGold(int amount)
    {
        // 사용 가능하면 사용하고 true 리턴
        if (currentGold >= amount)
        {
            currentGold -= amount;
            UpdateGoldUI();
            return true;
        }

        // 사용이 불가능하면 false 리턴
        return false;
    }


    //* UI에 골드 최신화
    public void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = $"Gold: {currentGold}";
        }
    }
}
