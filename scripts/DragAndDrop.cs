using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public bool NeverMoved = true;
    public int identity = 0;
    private CanvasGroup CanvasGroup;
    private RectTransform RectTransform;
    public Canvas canvas;

    public Vector2 StartingLocation;
    public bool firstLocation = true;

    public string SlotsTag = "SpawningSlots";
    public UIplayerController PlayerController;

    public GameObject inventoryPrefab;
    private void Awake()
    {
        if(PlayerController == null)
        {
            PlayerController = FindAnyObjectByType<UIplayerController>();
        }
        if (canvas == null)
        {
            canvas = GameObject.Find("PlayerUI").GetComponent<Canvas>(); ;
        }
        NeverMoved = true;
        RectTransform = GetComponent<RectTransform>();
        CanvasGroup = GetComponent<CanvasGroup>();
        if(firstLocation == true)
        {
            StartingLocation = RectTransform.anchoredPosition;
            firstLocation = false;
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        RectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        //Debug.Log("OnDrag");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("OnPointerDown");
        //StartingLocation = RectTransform.anchoredPosition;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("OnBeginDrag");
        CanvasGroup.alpha = 0.6f;
        CanvasGroup.blocksRaycasts = false;
        PlayerController.FormationCost();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CanvasGroup.alpha = 1f;
        CanvasGroup.blocksRaycasts = true;

        if (NeverMoved)
        {
            GameObject clone = Instantiate(inventoryPrefab, eventData.pointerDrag.transform.parent);
            clone.GetComponent<RectTransform>().anchoredPosition = StartingLocation;
            DragAndDrop dad = clone.GetComponent<DragAndDrop>();
            dad.StartingLocation = StartingLocation;
            dad.firstLocation = false;
            NeverMoved  = false;
            clone.name = inventoryPrefab.name;
            //Debug.Log(inventoryPrefab.name + " " + clone.name);
        }

        // Preverimo, èe smo spustili nad slotom
        if (eventData.pointerEnter == null || !eventData.pointerEnter.CompareTag(SlotsTag))
        {
            Destroy(gameObject); // Èe ni bil spušèen na slot, unièimo
        }
        PlayerController.FormationCost();
    }
}
