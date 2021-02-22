using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[ExecuteInEditMode]
public class WireBuilder : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public Color onColor;
    public Color offColor;

    public GameObject wireSegmentPrefab;

    public RectTransform wireSegment;

    public ConnectionPoint anchor;

    public Guid netGuid;

    private RawImage wireImage;

    private bool value;

    private RectTransform parentTransform;

    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = Vector3.zero;

        //UpdateWirePositions();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // public void SetValue(bool value){
    //     foreach(var wire in wireImages) {
    //         wire.color = value ? onColor : offColor;
    //     }

    //     start.SetValue(value);
    //     foreach(var end in ends){
    //         end.SetValue(value);
    //     }

    //     this.value = value;
    // }

    public void OnBeginDrag(PointerEventData eventData){
        parentTransform = transform.parent.GetComponent<RectTransform>();
        wireSegment = GameObject.Instantiate(wireSegmentPrefab, this.transform).GetComponent<RectTransform>();
        wireImage = wireSegment.GetComponent<RawImage>();
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData){
        Vector3 startPos = anchor.transform.position;
        Vector3 localStartPos = parentTransform.InverseTransformPoint(startPos);
        Vector2 localEndPos;


        ConnectionPoint connection = FindConnectionPoint();

        if(connection != null){
            localEndPos = parentTransform.InverseTransformPoint(connection.transform.position);
        } else {
            RectTransformUtility.ScreenPointToLocalPointInRectangle (parentTransform, eventData.position, Camera.main, out localEndPos);
        }

        var vector = (Vector3)localEndPos - localStartPos;
        var projected = Vector3.ProjectOnPlane(vector, transform.forward);
        wireSegment.localPosition = (localStartPos + (Vector3)localEndPos) / 2.0f;
        wireSegment.sizeDelta = new Vector2(Vector3.Magnitude(((Vector3)localEndPos - localStartPos)), 50);
        wireSegment.localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, projected));

        Rect newUvRect = wireImage.uvRect;
        newUvRect.width = wireSegment.sizeDelta.x / 50.0f;
        wireImage.uvRect = newUvRect;
    }

    public void OnEndDrag(PointerEventData eventData){
        ConnectionPoint target = FindConnectionPoint();

        if(target == null) {
            anchor.currentWireBuilder = null;
            Destroy(this.gameObject);
            return;
        }

        bool valid = true;

        if(target.connectionType == anchor.connectionType){ valid = false; }

        if(valid){
            Debug.Log("Good connection");
            var source = anchor.connectionType == ConnectionPoint.ConnectionType.Output ? anchor : target;
            var dest = target.connectionType == ConnectionPoint.ConnectionType.Input ? target : anchor;

            Debug.Assert(source != dest);

            CircuitDisplay cd = GetComponentInParent<CircuitDisplay>();

            if(cd.CanMakeConnection(source, dest)){
                cd.MakeConnection(source, dest);
            } else {
                Debug.Log("CircuitDisplay says connection illegal");
            }
        }
        
        Debug.Log("Cancelling, invalid connection");
        anchor.currentWireBuilder = null;
        Destroy(this.gameObject);
    }

    ConnectionPoint FindConnectionPoint(){
         
        PointerEventData pointerData = new PointerEventData (EventSystem.current)
        {
            pointerId = -1,
        };
        
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        
        foreach(var result in results){
            if(result.gameObject.CompareTag("ConnectionPoint")){
                return result.gameObject.GetComponent<ConnectionPoint>();
            }
        }

        return null;
    }
    
}
