using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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

    [SerializeField]
    private GameObject _ghostPrefab;

    private GameObject ghost;
    public RuntimeBlock draggingBlock;
    private Canvas canvas;

    public bool isDragging;

    private void OnEnable()
    {
        EventBus.Subscribe<DragBeginEvent>(OnDragBegin);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<DragBeginEvent>(OnDragBegin);
    }

    private void Awake()
    {
        instance = this;
    }

    private void OnDragBegin(DragBeginEvent e)
    {
        isDragging = true;

        draggingBlock = e.block;
        canvas = e.canvas;

        ghost = Instantiate(_ghostPrefab, canvas.transform);
        ghost.transform.position = e.startWorldPos;
        ghost.GetComponent<Draggable_Block>().Show(draggingBlock);
    }

    public void Update()
    {
        if (!isDragging) return;
        if (ghost == null) return;

        ghost.transform.position = Input.mousePosition;

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

        foreach (var hit in results)
        {
            TimelineDropZone dropTarget =
                hit.gameObject.GetComponentInParent<TimelineDropZone>();

            if (dropTarget == null)
                continue;

            dropTarget.OnDrop_inTimeline(draggingBlock);
            return true; // ✅ 성공
        }

        return false; // ❌ 실패
    }
}
