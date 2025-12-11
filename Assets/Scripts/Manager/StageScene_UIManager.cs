using UnityEngine;

public class StageScene_UIManager : MonoBehaviour
{
    public static StageScene_UIManager Instance;

    [SerializeField]
    private GameObject _light;

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

    public void Light_On()
    {
        _light.SetActive(true);
    }

    public void Light_Off()
    {
        _light.SetActive(false);
    }
}
