using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageInfo_Panel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _title;
    [SerializeField]
    private TextMeshProUGUI _description;

    [SerializeField]
    private Button _back;

    void Start()
    {
        if (_back != null)
        {
            _back.onClick.AddListener(Hide);

            // TODO: 나중에 꼭 살려라 수현아
            //if (DeckBuildingManager.Instance != null)
                //_back.onClick.AddListener(DeckBuildingManager.Instance.On_Tags);
        }
            
    }

    public void Show(int id)
    {
        StageData stageData = DataRepository.Instance.GetStage(id);

        _title.text = stageData.StageName;
        _description.text = stageData.StageDescription;

        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _title.text = "";
        _description.text = "";
        this.gameObject.SetActive(false);
    }
}
