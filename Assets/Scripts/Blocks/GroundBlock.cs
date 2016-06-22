using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[SelectionBase]
public class GroundBlock : Block
{

    public int tint = 0;

    public new Renderer renderer;

    private float brightness = 0.0f;
    private Color color = Color.white;

    private int brightnessUniform;
    private int colorUniform;

    public TextMesh debugText;

    public override void Awake()
    {
        base.Awake();
        blockType = BlockType.Ground;
    }

	// Use this for initialization
	public override void Start ()
    {
        base.Start();
        brightnessUniform = Shader.PropertyToID("_Brightness");
        colorUniform = Shader.PropertyToID("_Color");

#if !UNITY_EDITOR
        debugText.gameObject.SetActive(false);
#endif
        UpdateUsageText();
	}
	
	// Update is called once per frame
	public override void Update () {
        base.Update();
	}

    public void SetBrightness(float b)
    {
        if(b != brightness)
        {
            renderer.material.SetFloat(brightnessUniform, b);
            brightness = b;
        }
    }

    public void SetColor(Color c)
    {
        if(c != color)
        {
            color = c;
            renderer.material.SetColor(colorUniform, c);
        }
    }

    private int usage = 0;
    public void IncrementUsage()
    {
        usage++;
        UpdateUsageText();
    }

    public override void OnGamePreStart()
    {
        base.OnGamePreStart();
        usage = 0;
    }

    void UpdateUsageText()
    {
        if (debugText.gameObject.activeSelf == false) return;

        if (usage > 0)
            debugText.text = usage.ToString();
        else
            debugText.text = "";
    }
}
