using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConnectionPoint : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, ISelectHandler, IDeselectHandler
{

    public enum ConnectionType {
        Input,
        Output
    }

    public ConnectionType connectionType;

    public int connectionIndex;
    public GameObject wireBuilderPrefab;
    public Color onColor;
    public Color offColor;

    private Image indicator;

    public delegate void OnConnectorMoveDelegate();
    public OnConnectorMoveDelegate connectorMoveDelegate;

    public bool value { get; private set; }

    public WireBuilder currentWireBuilder;

    public void SetValue(bool value){
        indicator.color = value ? onColor : offColor;
        this.value = value;
    }

    // Start is called before the first frame update
    void Awake()
    {
        indicator = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("ConnectionPoint OnBeginDrag");
        currentWireBuilder = GameObject.Instantiate(wireBuilderPrefab, GetComponentInParent<CircuitDisplay>().wireContainer.transform).GetComponent<WireBuilder>();
        currentWireBuilder.anchor = this;
        currentWireBuilder.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData){
        currentWireBuilder?.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData){
        currentWireBuilder?.OnEndDrag(eventData);
    }

    public void OnSelect(BaseEventData eventData){
        Debug.Log("ConnectionPoint Select");
    }

    public void OnDeselect(BaseEventData eventData){
    }
}
