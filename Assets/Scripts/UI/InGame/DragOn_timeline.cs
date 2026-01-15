using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public struct DragBeginEvent
{
    public PointerEventData pointer;
    public RuntimeBlock block;
    public Vector3 startWorldPos;
    public Canvas canvas;
}


public class DragOn_timeline : MonoBehaviour
{
    public static DragOn_timeline instance;

    [Header("새로운 레이아웃 전용")]
    public bool New_Layout = false;
    public GameObject New_Layout_ghost; // 설명 슬롯

    [SerializeField]
    private GameObject _ghostPrefab;

    private GameObject ghost;
    public RuntimeBlock draggingBlock;
    private Canvas canvas;
    private RectTransform canvasRect;
    private RectTransform ghostRect;

    public bool isDragging;

    private void OnEnable()
    {
        EventBus.Subscribe<DragBeginEvent>(OnDragBegin);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<DragBeginEvent>(OnDragBegin);
        if (ServiceLocator.Instance != null && ServiceLocator.Instance.Cursor != null)
        {
            ServiceLocator.Instance.Cursor.OnDragEnd();
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void OnDragBegin(DragBeginEvent e)
    {
        isDragging = true;
        if (ServiceLocator.Instance != null && ServiceLocator.Instance.Cursor != null)
        {
            ServiceLocator.Instance.Cursor.OnDragStart();
        }


        draggingBlock = e.block;
        canvas = e.canvas;
        canvasRect = canvas.GetComponent<RectTransform>();

        // 레이아웃에 따른 프리펩 선정
        GameObject prefab = New_Layout ? New_Layout_ghost : _ghostPrefab;

        ghost = Instantiate(prefab, canvas.transform);
        ghostRect = ghost.GetComponent<RectTransform>();

        if (ghost.GetComponent<Draggable_Block>() != null)
        {
            ghost.GetComponent<Draggable_Block>().Show(draggingBlock);
        }
        else
        {
            ghost.GetComponent<Film_UI>().Show(draggingBlock);
        }

        UpdateGhostPosition(e.startWorldPos);

        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundID.SFX_Spell_Cancle);
    }

    public void Update()
    {
        if (!isDragging) return;
        if (ghost == null) return;

        //ghost.transform.position = Input.mousePosition;
        UpdateGhostPosition(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }

    void EndDrag()
    {
        bool dropped = TryDrop();

        if (!dropped)
        {
            // ❌ 드롭 실패 처리
            OnDropFailed();
        }

        Destroy(ghost);
        ghost = null;
        isDragging = false;
        if (ServiceLocator.Instance != null && ServiceLocator.Instance.Cursor != null)
        {
            ServiceLocator.Instance.Cursor.OnDragEnd();
        }

        if (BattleUIManager.Instance != null)
            BattleUIManager.Instance.HandleBar_raycastOn();
    }

    // 위치변환 함수
    private void UpdateGhostPosition(Vector3 v3)
    {
        Vector2 localPoint;
        Vector2 v2 = new Vector2(v3.x, v3.y);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            v2,
            canvas.worldCamera,   // ⭐ Camera 모드에서는 반드시 필요
            out localPoint
        );

        ghostRect.localPosition = localPoint;
    }

    void OnDropFailed()
    {
        // 아무 처리 안 함
        if(TimelineManager.Instance != null)
        {
            TimelineManager.Instance.ReturnToHand(draggingBlock);
        }
    }

    bool TryDrop()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        if (results.Count > 0)
        {
            TimelineDropZone dropTarget = results[0].gameObject.GetComponentInParent<TimelineDropZone>();

            if (dropTarget != null)
            {
                dropTarget.OnDrop_inTimeline(draggingBlock);
                return true;
            }
        }
        

        //foreach (var hit in results)
        //{
        //    TimelineDropZone dropTarget =
        //        hit.gameObject.GetComponentInParent<TimelineDropZone>();

        //    if (dropTarget == null)
        //        continue;

        //    dropTarget.OnDrop_inTimeline(draggingBlock);
        //    return true; // ✅ 성공
        //}

        return false; // ❌ 실패
    }
}
