using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Panels")]
    public GameObject contextMenuPanel;
    public GameObject mainMenuOptions;      // 이동, 업그레이드, 제거 패널
    public GameObject upgradeSubOptions;    // 수직/반대 업그레이드 패널

    public GameObject closeOverlayButton;

    [Header("Buttons & Texts")]
    public Button mainUpgradeButton;
    public TextMeshProUGUI upgradeButtonText;

    public TextMeshProUGUI verticalSubButtonText;
    public TextMeshProUGUI oppositeSubButtonText;

    private GridPiece currentSelectedPiece;

    void Awake()
    {
        Instance = this;
        CloseMenu();
    }

    //* piece에 해당하는 선택 메뉴를 열기
    public void OpenMenu(GridPiece piece, Vector2 screenPos)
    {
        currentSelectedPiece = piece;

        // screenPos 위치로 패널 이동 후 켜기
        contextMenuPanel.transform.position = screenPos;
        contextMenuPanel.SetActive(true);
        closeOverlayButton.SetActive(true);

        // 메인 메뉴가 먼저 보이게 지정
        mainMenuOptions.SetActive(true);
        upgradeSubOptions.SetActive(false);

        // 가격표 데이터를 가져와 서브 버튼 텍스트 수정
        PieceData data = piece.GetComponent<PieceData>();
        if (data != null)
        {
            if (verticalSubButtonText != null)
                verticalSubButtonText.text = $"수직 방향 추가 ({data.verticalUpgradeCost}G)";
            if (oppositeSubButtonText != null)
                oppositeSubButtonText.text = $"반대 방향 추가 ({data.oppositeUpgradeCost}G)";
        }

        //* 업그레이드 상태에 맞게 버튼 활성/비활성화
        // 업그레이드가 되어 있으면 버튼 비활성화
        if (piece.IsUpgraded)
        {
            mainUpgradeButton.interactable = false;
            upgradeButtonText.text = "업그레이드 불가";
        }
        // 업그레이드가 되어있지 않으면 활성화
        else
        {
            mainUpgradeButton.interactable = true;
            upgradeButtonText.text = "업그레이드";
        }
    }

    //* 패널 닫기
    public void CloseMenu()
    {
        closeOverlayButton.SetActive(false);
        contextMenuPanel.SetActive(false);
        currentSelectedPiece = null;
    }

    //* 업그레이드 버튼 클릭 시 해당 피스 업그레이드
    public void OnClickMainUpgrade()
    {
        // 메인 선택지 패널을 숨기고 세부 선택지를 띄움
        mainMenuOptions.SetActive(false);
        upgradeSubOptions.SetActive(true);
    }

    //* 이동 버튼 클릭 시
    public void OnClickMove()
    {
        if (currentSelectedPiece == null) return;

        // 선택한 기물의 좌표를 정수형으로 추출
        Vector2Int piecePos = new Vector2Int(
            Mathf.RoundToInt(currentSelectedPiece.transform.position.x),
            Mathf.RoundToInt(currentSelectedPiece.transform.position.y)
        );

        InteractionManager.Instance.StartMovePiece(currentSelectedPiece.gameObject, piecePos);

        CloseMenu();
    }

    //* 제거 버튼 클릭 시
    public void OnClickRemove()
    {
        if (currentSelectedPiece == null) return;

        // 선택된 기물을 제거하고 돈을 환원받음
        Vector2Int piecePos = new Vector2Int(
            Mathf.RoundToInt(currentSelectedPiece.transform.position.x),
            Mathf.RoundToInt(currentSelectedPiece.transform.position.y)
        );
        GridManager.Instance.RemovePiece(piecePos);

        CloseMenu();
    }

    //* 수직 방향 버튼 클릭시 수직방향으로 업그레이드
    public void OnClickVerticalSelect()
    {
        if (currentSelectedPiece != null)
        {
            PieceData data = currentSelectedPiece.GetComponent<PieceData>();
            int upgradeCost = data.verticalUpgradeCost;

            if (ResourceManager.Instance.UseGold(upgradeCost))
            {
                currentSelectedPiece.ApplyUpgradeVertical();
                data.AddUpgradeCost(upgradeCost);
            }
        }
        CloseMenu();
    }

    //* 반대 방향 버튼 클릭시 반대방향으로 업그레이드
    public void OnClickOppositeSelect()
    {
        if (currentSelectedPiece != null)
        {
            PieceData data = currentSelectedPiece.GetComponent<PieceData>();
            int upgradeCost = data.oppositeUpgradeCost;

            if (ResourceManager.Instance.UseGold(upgradeCost))
            {
                currentSelectedPiece.ApplyUpgradeOpposite();
                data.AddUpgradeCost(upgradeCost);
            }
        }
        CloseMenu();
    }
}
