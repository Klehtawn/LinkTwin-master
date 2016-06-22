using UnityEngine;
using System.Collections;

public class PostProcessVanilla : MonoBehaviour {

    public CanvasGroup blackImageLayer;

	// Use this for initialization
	void Start () {

        brightness = 1.0f;
	}

    private float _brightness = 1.0f;

    public float brightness
    {
        get
        {
            return _brightness;
        }
        set
        {
            _brightness = Mathf.Clamp01(value);
            if (_brightness >= 0.99f)
                _brightness = 1.0f;

            if (blackImageLayer != null)
            {
                bool _active = _brightness < 0.99f;
                blackImageLayer.gameObject.SetActive(_active);
                if (_active)
                    blackImageLayer.alpha = 1.0f - _brightness;
            }
        }
    }
}
