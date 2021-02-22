using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[ExecuteInEditMode]
public class WireSystem : MonoBehaviour
{

    public Color onColor;
    public Color offColor;

    public GameObject wireSegmentPrefab;

    public List<RectTransform> currentWireSegments = new List<RectTransform>();

    public ConnectionPoint start;
    public List<ConnectionPoint> ends = new List<ConnectionPoint>();

    public Guid netGuid;

    private List<Image> wireImages = new List<Image>();

    private bool value;

    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = Vector3.zero;

        // Register delegates
        start.connectorMoveDelegate += UpdateWirePositions;
        foreach(var end in ends){
            end.connectorMoveDelegate += UpdateWirePositions;
        }

        UpdateWirePositions();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetValue(bool value){
        foreach(var wire in wireImages) {
            wire.color = value ? onColor : offColor;
        }

        start.SetValue(value);
        foreach(var end in ends){
            end.SetValue(value);
        }

        this.value = value;
    }

    void UpdateWirePositions(){
        
        if(currentWireSegments.Count != ends.Count && wireSegmentPrefab != null){
            
            wireImages.Clear();
            foreach(var segment in currentWireSegments){
                currentWireSegments.Remove(segment);
                Destroy(segment.gameObject);
            }
            foreach(var end in ends){
                var newSegment = GameObject.Instantiate(wireSegmentPrefab, this.transform).GetComponent<RectTransform>();
                currentWireSegments.Add(newSegment);
                wireImages.Add(newSegment.GetComponentsInChildren<Image>()[0]);
            }
            
        }

        var startPos = start.transform.position;
        for(int i = 0; i < ends.Count; i++){
            var endPos = ends[i].transform.position;

            

            var vector = endPos - startPos;
            var projected = Vector3.ProjectOnPlane(vector, transform.forward);

            currentWireSegments[i].position = (startPos + endPos) / 2.0f;

            var scaledVector = transform.parent.InverseTransformVector(vector);

            currentWireSegments[i].sizeDelta = new Vector2(Vector3.Magnitude(scaledVector), 50);

            currentWireSegments[i].localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, projected));
        }

        
    }

    public void OnDestroy(){
        Debug.Log("Wire system being destroyed");
        start.connectorMoveDelegate -= UpdateWirePositions;
        foreach(var end in ends){
            end.connectorMoveDelegate -= UpdateWirePositions;
        }
    }

    void OnDrawGizmos(){
        // Gizmos.DrawSphere()
    }
}
