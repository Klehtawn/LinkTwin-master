using UnityEngine;
using System.Collections;
using System;

public class IconRepresentation : MonoBehaviour {

    public Sprite icon;
    public Color color = Color.gray;

    private PixelBlock smallIconPixels;
    public PixelBlock GetSmallIcon(int sz)
    {
        if(smallIconPixels != null && smallIconPixels.width == sz && smallIconPixels.height == sz)
        {
            return smallIconPixels;
        }
        // convert small icon
        if (icon != null)
        {           
            smallIconPixels = new PixelBlock((int)icon.textureRect.width, (int)icon.textureRect.height);
            BufferBlit.Copy(smallIconPixels, icon.texture.GetPixels32());
            smallIconPixels = BufferBlit.Resize(smallIconPixels, sz, sz);

            return smallIconPixels;
        }

        return null;
    }
}
