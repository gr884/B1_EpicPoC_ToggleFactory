using System;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    public static TickManager Instance;

    [Tooltip("1틱에 필요한 시간")]
    public float tickInterval = .5f;
    private float timer = 0f;

    // 틱 발생을 방송할 이벤트 채널
    public event Action OnTick;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 시간이 주기를 넘었으면 방송
        if (timer >= tickInterval)
        {
            timer = 0f;
            // OnTick을 구독하고 있는 기물이 하나라도 있으면 방송
            OnTick?.Invoke();
        }
    }
}
