#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class SagaButtonPosition : MonoBehaviour
{
#if UNITY_EDITOR
    static Texture texture;

    void OnDrawGizmos()
    {
        if (texture == null)
            texture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Sprites/menu_button.png");
        RectTransform rt = GetComponent<RectTransform>();
        Rect r = new Rect(transform.position.x + rt.rect.x, transform.position.y + rt.rect.y, rt.rect.width, rt.rect.height);
        Gizmos.DrawGUITexture(r, texture);
    }
#endif
}
