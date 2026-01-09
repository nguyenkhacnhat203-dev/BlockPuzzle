using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class PopupManager : Singleton<PopupManager>
{



    [Header("Popup Prefabs")]
    public GameObject popup_Setting;
  

    [Header("Popup Parent (Canvas)")]
    public Transform popupParent;

    [Header("State")]
    public bool isShowPopup;

    private Canvas parentCanvas;


    public Button Setting;
   




    protected override void Awake()
    {
        base.Awake();
        if (Setting != null)
        {
            AddPointerDownListener(Setting, () => OnDown());
        }



        parentCanvas = GetComponent<Canvas>();
        Setting.onClick.AddListener(ShowPopup_Setting);



    }




    private void AddPointerDownListener(Button button, UnityEngine.Events.UnityAction action)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entryDown = new EventTrigger.Entry();
        entryDown.eventID = EventTriggerType.PointerDown;
        entryDown.callback.AddListener((data) => { action(); });

        trigger.triggers.Add(entryDown);
    }

    void OnDown()
    {
        isShowPopup = true;
    }



    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        if (parentCanvas != null)
        {
            parentCanvas.worldCamera = Camera.main;
        }
    }


    public void ShowPopup_Setting()
    {
        if (popup_Setting == null) return;
        CreatePopup(popup_Setting);
    }

 

    private void CreatePopup(GameObject prefab)
    {
        AudioManager.Instance.Btn_Click();
        isShowPopup = true;
        GameObject popup = Instantiate(prefab, popupParent);
        popup.SetActive(true);

        UpdateCamera();
    }
}