using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class VersionWidget : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        if (GetComponentInChildren<Text>() != null)
            GetComponentInChildren<Text>().text = "VERSION " + Application.version;
	}
}
