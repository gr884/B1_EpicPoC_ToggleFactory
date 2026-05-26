using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance;

    [Tooltip("반투명한 미리보기 기물")]
    public GameObject previewGhost;

    [Header("Prefabs")]
    [Tooltip("신호 발생기 프리팹")]
    public GameObject generatorPrefab;
    [Tooltip("신호 전달기 프리팹")]
    public GameObject piecePrefab;
    [Tooltip("돈 수신기 프리팹")]
    public GameObject receiverPrefab;

    private PlayerInput playerInput;
    private Vector2 mouseScreenPosition;

    // 현재 들고 있는 기물의 프리팹
    private GameObject currentSelectedPrefab;

    // 이동용 변수
    private GameObject movingPiece = null;  // 현재 마우스로 이동중인 기물
    private Vector2Int originalPosition;     // 이동 취소 시 돌아갈 기존 좌표

    // 회전 기능용 변수
    private Vector2Int currentSpawnDirection = Vector2Int.right;

    void Awake()
    {
        Instance = this;
        playerInput = new PlayerInput();
        currentSelectedPrefab = generatorPrefab;
        UpdateGhostRotation();
    }

    void OnEnable()
    {
        playerInput.Enable();
        playerInput.Gameplay.PointerPosition.performed += ctx => mouseScreenPosition = ctx.ReadValue<Vector2>();
        playerInput.Gameplay.Click.performed += ctx => PlacePiece();
    }

    void OnDisable()
    {
        playerInput.Disable();
    }

    void Update()
    {
        // 이동 중이라면 아래 로직 발동 x
        if (movingPiece != null)
        {
            // 이동 중에 우클릭을 누르면 취소
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancleMove();
                return;    
            }
        }
        else
        {
            // 1번 키: 생성기, 2번 키: 방향기, 3번 키: 생성기로 할당
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
                currentSelectedPrefab = generatorPrefab;
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
                currentSelectedPrefab = piecePrefab;
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
                currentSelectedPrefab = receiverPrefab;

            // 마우스 우클릭 시 업그레이드 패널 띄우기
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Vector2Int gridPos = new Vector2Int(
                    Mathf.RoundToInt(previewGhost.transform.position.x),
                    Mathf.RoundToInt(previewGhost.transform.position.y)
                );

                // 클릭한 위치의 오브젝트 확인
                GameObject clickedObj = GridManager.Instance.GetPieceAt(gridPos);
                if (clickedObj != null)
                {
                    // 전달기면 업그레이드
                    GridPiece piece = clickedObj.GetComponent<GridPiece>();
                    if (piece != null)
                    {
                        UIManager.Instance.OpenMenu(piece, Mouse.current.position.ReadValue());
                    }
                }
                // 허공을 우클릭하면 메뉴판 닫기
                else
                {
                    if (UIManager.Instance != null)
                        UIManager.Instance.CloseMenu();
                }
            }
        }

        // 회전 키에 따라 시계/반시계로 회전
        if (Keyboard.current.qKey.wasPressedThisFrame) RotateDirection(false);  // Q : 반시계 회전
        if (Keyboard.current.eKey.wasPressedThisFrame) RotateDirection(true);   // E : 시계 회전

        // 마우스의 위치에 ghost를 지속적으로 위치
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        Vector3 snapPos = new Vector3(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y), 0);

        // 이동 중이면 마우스에 이동 기물을 붙이고, 아니라면 고스트를 붙임
        if (movingPiece != null)
        {
            movingPiece.transform.position = snapPos;
        }
        else
        {
            if (previewGhost != null)
                previewGhost.transform.position = snapPos;
        }
    }

    //* 기물을 ghost 위치에 복제해 생성
    void PlacePiece()
    {
        // 다른 게임오브젝트 위에 있으면 무시
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // 생성할 위치에 비어있는지 확인 및 추가
        Vector3 targetPos = (movingPiece != null) ? movingPiece.transform.position : previewGhost.transform.position;
        Vector2Int gridPos = new Vector2Int(
            Mathf.RoundToInt(targetPos.x),
            Mathf.RoundToInt(targetPos.y)
        );

        if (GridManager.Instance.IsTileEmpty(gridPos))
        {
            // 각도 계산
            float angle = Mathf.Atan2(currentSpawnDirection.y, currentSpawnDirection.x) * Mathf.Rad2Deg;
            Quaternion pieceRotation = Quaternion.Euler(0, 0, angle);

            // 존재하는 기물 이동 배치 시
            if (movingPiece != null)
            {
                GridManager.Instance.RegisterPiece(gridPos, movingPiece);

                UpdatePieceInternalPosition(movingPiece, gridPos, currentSpawnDirection);

                SpriteRenderer sr = movingPiece.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 1f;
                    sr.color = c;
                }

                movingPiece = null;
                if (previewGhost != null) previewGhost.SetActive(true);
            }
            // 신규 기물을 상점에서 샀을 때
            else
            {
                // 기물의 설치를 위해 자금이 있는지 확인
                PieceData prefabData = currentSelectedPrefab.GetComponent<PieceData>();
                int cost = prefabData.buildCost;

                if (ResourceManager.Instance.UseGold(cost))
                {
                    GameObject newPiece = Instantiate(currentSelectedPrefab, previewGhost.transform.position, pieceRotation);
                    GridManager.Instance.RegisterPiece(gridPos, newPiece);

                    // 생성된 기물 초기화
                    SignalGenerator genScript = newPiece.GetComponent<SignalGenerator>();
                    if (genScript != null)
                        genScript.Initialize(gridPos, currentSpawnDirection);

                    GridPiece pieceScript = newPiece.GetComponent<GridPiece>();
                    if (pieceScript != null)
                        pieceScript.Initialize(gridPos, currentSpawnDirection);
                        
                    MoneyReceiver receiverScript = newPiece.GetComponent<MoneyReceiver>();
                    if (receiverScript != null)
                        receiverScript.Initialize(gridPos, currentSpawnDirection);
                }
            }
            
        }
        else
        {
            //Todo 설치 불가 이펙트가 들어갈 위치
            Debug.Log("해당 위치에는 설치가 불가능합니다");
        }

    }

    //* 기물 내의 모든 스크립트들의 좌표 데이터를 강제 동기화
    private void UpdatePieceInternalPosition(GameObject piece, Vector2Int newPos, Vector2Int newDir)
    {
        GridPiece gp = piece.GetComponent<GridPiece>();
        if (gp != null) gp.UpdatePositionAndDirection(newPos, newDir);

        SignalGenerator sg = piece.GetComponent<SignalGenerator>();
        if (sg != null) sg.UpdatePositionAndDirection(newPos, newDir);

        MoneyReceiver mr = piece.GetComponent<MoneyReceiver>();
        if (mr != null) mr.UpdatePositionAndDirection(newPos, newDir);
    }

    //* pos 위치의 piece를 제외시키고 반투명하게 만들어주기
    public void StartMovePiece(GameObject piece, Vector2Int pos)
    {
        movingPiece = piece;
        originalPosition = pos;
        
        // 이동을 시작할 때 piece 기물의 방향을 적용
        GridPiece gp = piece.GetComponent<GridPiece>();
        SignalGenerator sg = piece.GetComponent<SignalGenerator>();
        if (gp != null) currentSpawnDirection = gp.baseDirection;
        else if (sg != null) currentSpawnDirection = sg.currentDirection;
        UpdateGhostRotation();

        // 그리드에서 잠시 제외시켜 빈칸으로 만들기
        GridManager.Instance.UnregisterPiece(pos);

        // 이동 중에 ghost 타일 잠시 끄기
        previewGhost.SetActive(false);

        // 들고 있을 때 시각적 피드백
        SpriteRenderer sr = movingPiece.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = .5f;
            sr.color = c;
        }
    }

    //* 이동 취소 및 원위치 복귀
    private void CancleMove()
    {
        if (movingPiece == null) return;

        // 기존 위치로 이동
        movingPiece.transform.position = new Vector3(originalPosition.x, originalPosition.y, 0);
        GridManager.Instance.RegisterPiece(originalPosition, movingPiece);

        // 투명도 복구
        SpriteRenderer sr = movingPiece.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        // 초깃값 설정
        movingPiece = null;
        previewGhost.SetActive(true);
    }

    //* 지정된 방향에 맞게 회전
    private void RotateDirection(bool clockwise)
    {
        if (clockwise)
        {
            currentSpawnDirection = new Vector2Int(currentSpawnDirection.y, -currentSpawnDirection.x);
        }
        else
        {
            currentSpawnDirection = new Vector2Int(-currentSpawnDirection.y, currentSpawnDirection.x);
        }
        UpdateGhostRotation();
    }

    //* ghost에 회전 적용
    private void UpdateGhostRotation()
    {
        float angle = Mathf.Atan2(currentSpawnDirection.y, currentSpawnDirection.x) * Mathf.Rad2Deg;
        Quaternion newRot = Quaternion.Euler(0, 0, angle);

        if (previewGhost != null) previewGhost.transform.rotation = newRot;
        if (movingPiece != null) movingPiece.transform.rotation = newRot;
    }
}
