using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrefToggle : MonoBehaviour
{
    public string prefKey;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Toggle>().isOn = PlayerPrefs.GetInt(prefKey) == 1;
    }

    // Update is called once per frame
    public void onValueChanged(Toggle change)
    {
        PlayerPrefs.SetInt(prefKey, change.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}
