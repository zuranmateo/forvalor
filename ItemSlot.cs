using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class ItemSlot : MonoBehaviour, IDropHandler, IPointerExitHandler
{
    //public GameObject spearmanImagePrefab;
    //public Vector2 LocationOfImages;
    public int identity = 0;
    private GameObject Prefab;
    private void Awake()
    {

    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && eventData.pointerDrag == Prefab)
        {
            identity = 0;
            Prefab = null;
            //Debug.Log(this.gameObject.name + identity);
        }
    }

    void IDropHandler.OnDrop(PointerEventData eventData)
    {
        //Debug.Log("OnDrop");
        if(eventData.pointerDrag != null)
        {
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
            DragAndDrop dragdnddrop = eventData.pointerDrag.GetComponent<DragAndDrop>();
            identity = dragdnddrop.identity;
            Prefab = eventData.pointerDrag;
        }
        else
        {
            identity = 0;
        }
        //Debug.Log(this.gameObject.name + identity + Prefab);
    }

}
