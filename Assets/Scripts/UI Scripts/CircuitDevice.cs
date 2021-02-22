using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;
using UnityEngine.EventSystems;

public class CircuitDevice : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler    
{
    public bool draggable;
    public bool isFresh;
    public float snapInterval = 1.0f;
    public Guid associatedGuid;
    public LogicType logicType;
    public List<ConnectionPoint> inputs = new List<ConnectionPoint>();
    public List<ConnectionPoint> outputs = new List<ConnectionPoint>();

    private RectTransform parentRectTransform;
    private RectTransform panelRectTransform;

    private Transform originalContainer;

    private Vector2 pointerOffset;

    public void Start(){
        panelRectTransform = transform as RectTransform;
        parentRectTransform = transform.parent.transform as RectTransform;
    }

    public void OnBeginDrag (PointerEventData eventData)
    {
        //Debug.Log("OnBeginDrag");
        if(!draggable){
            eventData.eligibleForClick = true;
            return;
        }
        if(isFresh){

            // Duplicate ourselves to leave a copy behind on the Toolbox.
            var copy = GameObject.Instantiate(this, transform.parent);
            copy.transform.localPosition = transform.localPosition;
            copy.isFresh = true;
            // We have to tell the copy it is not the selected one...
            copy.GetComponent<Selector>().OnDeselect(new BaseEventData(EventSystem.current));
            transform.SetAsLastSibling();
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle (panelRectTransform, eventData.position, eventData.pressEventCamera, out pointerOffset);
        //Debug.Log("Offset: " + pointerOffset);

        // Find uppermost canvas to transfer to.
        var parentCanvases = GetComponentsInParent<Canvas>();
        var toplevelCanvas = parentCanvases[parentCanvases.Length - 1];
        originalContainer = transform.parent;

        //TransferToParent(toplevelCanvas.transform);
    }

    public void TransferToParent(Transform targetContainer){
        //Debug.Log("Transferring to parent: " + targetContainer.name);
        // Will need to detach wires here, too.

        // If the target is a circuitDisplay (usually is, except the toolbox),
        // place ourselves specifically into the gate container.
        CircuitDisplay cd = targetContainer.GetComponentInParent<CircuitDisplay>();
        if(cd != null){
            targetContainer = cd.gateContainer.transform;
        }

        transform.SetParent(targetContainer, true);
        transform.localScale = Vector3.one;
        parentRectTransform = targetContainer.GetComponent<RectTransform>();
        
    }

    public void OnDrag (PointerEventData eventData)
    {
        //Debug.Log("OnDrag");
        if(!draggable){
            eventData.eligibleForClick = true;
            return;
        }

        if(panelRectTransform == null){
            return;
        }

        // Check to see if we've dragged over a new container
        Transform newContainer = FindBestContainer();

        // If we are fresh, don't put ourselves back into the toolbox.
        // Otherwise, we are allowed to pop back into the toolbox.
        if(isFresh){
            if(newContainer != null && !ReferenceEquals(originalContainer, newContainer) && !newContainer.CompareTag("Toolbox")){
                TransferToParent(newContainer);
                transform.SetAsLastSibling();
                isFresh = false;
            }
        } else {
            if(newContainer != null && !ReferenceEquals(originalContainer, newContainer)){
                TransferToParent(newContainer);
                transform.SetAsLastSibling();
            }
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
            
            Vector2 clampedPosition = panelRectTransform.localPosition;

            
            panelRectTransform.localPosition = clampedPosition;

            // Fire all connectorMove delegates to recalculate wire positions.
            inputs.ForEach(i => i?.connectorMoveDelegate?.Invoke());
            outputs.ForEach(i => i?.connectorMoveDelegate?.Invoke());
        }
    }

    public void OnEndDrag(PointerEventData eventData){

        if(!draggable){
            eventData.eligibleForClick = true;
            return;
        }

        isFresh = false;

        // If we haven't landed in a circuitcontainer after the drag,
        // kill ourselves.
        var bestContainer = FindBestContainer();
        if(bestContainer == null || !bestContainer.CompareTag("GateContainer")){
            Destroy(this.gameObject);
        } else {
            TransferToParent(bestContainer);
            transform.SetAsLastSibling();

            if(associatedGuid == Guid.Empty){
                transform.GetComponentInParent<CircuitDisplay>().AddPhysicalGate(this);
            }
        }
    }

    Transform FindBestContainer(){
        var results = RaycastMouse();

        Transform newContainer = null;
        foreach(var result in results){
            if(result.gameObject.CompareTag("Toolbox")){
                Debug.Log("Found result: " + result.gameObject.name);
                newContainer = result.gameObject.transform;
                break;
            }
            if(result.gameObject.CompareTag("CircuitContainer")){
                Debug.Log("Found result: " + result.gameObject.name);
                newContainer = result.gameObject.GetComponent<CircuitDisplay>().gateContainer.transform;;
                break;
            }
        }

        return newContainer;
    }

    List<RaycastResult> RaycastMouse(){
         
        PointerEventData pointerData = new PointerEventData (EventSystem.current)
        {
            pointerId = -1,
        };
        
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        
        
         
        return results;
    }
}
