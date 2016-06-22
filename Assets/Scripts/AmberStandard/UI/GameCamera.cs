using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour {

	public Camera getCamera()
    {
        return GetComponent<Camera>();
    }
}
