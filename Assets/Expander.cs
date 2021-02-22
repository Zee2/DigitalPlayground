using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expander : MonoBehaviour
{
    public enum Dimension { Width, Height }

    public Dimension dimension;
    public float collapsedHeight;
    public float expandedHeight;

    public float speed;

    public AnimationCurve curve;

    private float toggleTime;
    private RectTransform rect;

    public bool isExpanded = false;

    public RectTransform neighbor;
    private float value;

    private float originalNeighborValue;

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        if(neighbor != null){
            originalNeighborValue = dimension == Dimension.Width ? neighbor.rect.xMin : neighbor.rect.yMin;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isExpanded){
            value += speed * Time.deltaTime;
        } else {
            value -= speed * Time.deltaTime;
        }
        value = Mathf.Clamp01(value);

        if(dimension == Dimension.Width){
            rect.sizeDelta = new Vector2(Mathf.Lerp(collapsedHeight, expandedHeight, curve.Evaluate(value)), rect.sizeDelta.y );
        } else {
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, Mathf.Lerp(collapsedHeight, expandedHeight, curve.Evaluate(value)));
        }

        if(neighbor != null){

            neighbor.offsetMin = new Vector2(rect.sizeDelta.x + 32, neighbor.offsetMin.y);
        }
    }

    public void Toggle(){
        isExpanded = !isExpanded;
    }
}
