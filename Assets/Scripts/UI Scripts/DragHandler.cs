using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
 
public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
 
    private Vector2 pointerOffset;
    private RectTransform parentRectTransform;
    private RectTransform panelRectTransform;

    public bool snap;

    public float snapInterval;
    private bool clampedToLeft;
    private bool clampedToRight;
    private bool clampedToTop;
    private bool clampedToBottom;

    private CircuitDevice attachedDevice;
 
    public void Start(){
       
        parentRectTransform = transform.parent.transform as RectTransform;
        panelRectTransform = transform as RectTransform;
        attachedDevice = GetComponent<CircuitDevice>();

        clampedToLeft = false;
        clampedToRight = false;
        clampedToTop = false;
        clampedToBottom = false;
    }
 
    #region IBeginDragHandler implementation
 
    public void OnBeginDrag (PointerEventData eventData)
    {
        transform.SetAsLastSibling();
        RectTransformUtility.ScreenPointToLocalPointInRectangle (panelRectTransform, eventData.position, eventData.pressEventCamera, out pointerOffset);
    }
 
    #endregion
 
    #region IDragHandler implementation
 
    public void OnDrag (PointerEventData eventData)
    {
        if(panelRectTransform == null){
            return;
        }
        Vector2 localPointerPosition;
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle (parentRectTransform, eventData.position, eventData.pressEventCamera, out localPointerPosition)){
            // Debug.Log("Local pointer position rel. canvas: " + localPointerPosition);
            Vector3 unsnapped = localPointerPosition - pointerOffset;

            if(PlayerPrefs.GetInt("shouldSnap") == 1){
                panelRectTransform.localPosition = new Vector3(Mathf.Round(unsnapped.x / snapInterval) * snapInterval,
                                                           Mathf.Round(unsnapped.y / snapInterval) * snapInterval,
                                                           Mathf.Round(unsnapped.z / snapInterval) * snapInterval);
            } else {
                panelRectTransform.localPosition = unsnapped;
            }
            
            // ClampToWindow ();
                Vector2 clampedPosition = panelRectTransform.localPosition;
            // if (clampedToRight) {
            //     clampedPosition.x = (canvasRectTransform.rect.width * 0.5f) - (panelRectTransform.rect.width * (1 - panelRectTransform.pivot.x));
            // } else if(clampedToLeft){
            //     clampedPosition.x = (-canvasRectTransform.rect.width * 0.5f) + (panelRectTransform.rect.width * panelRectTransform.pivot.x);
            // }
 
            // if(clampedToTop){
            //     clampedPosition.y = (canvasRectTransform.rect.height * 0.5f) - (panelRectTransform.rect.height * (1 - panelRectTransform.pivot.y));
            // }else if(clampedToBottom){
            //     clampedPosition.y = (-canvasRectTransform.rect.height * 0.5f) + (panelRectTransform.rect.height * panelRectTransform.pivot.y);
            // }

            
            panelRectTransform.localPosition = clampedPosition;

            // If attached to a circuit device, we fire all
            // connectorMove delegates to recalculate wire positions.
            attachedDevice?.inputs.ForEach(i => i?.connectorMoveDelegate?.Invoke());
            attachedDevice?.outputs.ForEach(i => i?.connectorMoveDelegate?.Invoke());
        }
    }
 
    #endregion
 
    #region IEndDragHandler implementation
 
    public void OnEndDrag (PointerEventData eventData)
    {
 
    }
 
    #endregion
 
    // void ClampToWindow(){
    //     Vector3[] canvasCorners = new Vector3[4];
    //     Vector3[] panelRectCorners = new Vector3[4];
    //     canvasRectTransform.GetWorldCorners (canvasCorners);
    //     panelRectTransform.GetWorldCorners (panelRectCorners);
 
    //     if (panelRectCorners [2].x > canvasCorners [2].x) {
    //         Debug.Log ("Panel is to the right of canvas limits");
    //         if (!clampedToRight) {
    //             clampedToRight = true;
    //         }
    //     } else if (clampedToRight) {
    //         clampedToRight = false;
    //     } else if (panelRectCorners [0].x < canvasCorners [0].x) {
    //         Debug.Log ("Panel is to the left of canvas limits");
    //         if(!clampedToLeft){
    //             clampedToLeft = true;
    //         }
    //     }else if(clampedToLeft){
    //         clampedToLeft = false;
    //     }
 
    //     if (panelRectCorners [2].y > canvasCorners [2].y) {
    //         Debug.Log ("Panel is to the top of canvas limits");
    //         if (!clampedToTop) {
    //             clampedToTop = true;
    //         }
    //     }else if(clampedToTop){
    //         clampedToTop = false;
    //     }else if(panelRectCorners[0].y < canvasCorners[0].y){
    //         Debug.Log ("Panel is to the bottom of canvas limits");
    //         if(!clampedToBottom){
    //             clampedToBottom = true;
    //         }
    //     }else if(clampedToBottom){
    //         clampedToBottom = false;
    //     }
    // }
}