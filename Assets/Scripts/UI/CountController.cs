using TMPro;
using UnityEngine;

public class CountController : MonoBehaviour
{
    public static CountController Instance;

    [SerializeField]
    private GameObject _hitCount;
    [SerializeField]
    private GameObject _attackCount;
    [SerializeField]
    private TextMeshProUGUI _buffText;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
