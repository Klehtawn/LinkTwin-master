using UnityEngine;
using System.Collections;

public class PixelBlock
{
    byte[] _buffer;
    int _width, _height, _depth;

    public byte[] buffer
    {
        get
        {
            return _buffer;
        }
    }

    public Color32[] bufferRGBA
    {
        get
        {
            Color32[] buf = new Color32[size];
            for (int i = 0; i < buf.Length; i++ )
            {
                buf[i] = new Color32(_buffer[_depth * i], _buffer[_depth * i + 1], _buffer[_depth * i + 2], _buffer[_depth * i + 3]);
            }
            return buf;
        }
    }

    public int width
    {
        get
        {
            return _width;
        }
    }

    public int height
    {
        get
        {
            return _height;
        }
    }

    public int depth
    {
        get
        {
            return _depth;
        }
    }

    public int size
    {
        get
        {
            return _width * _height;
        }
    }

    public int sizeBytes
    {
        get
        {
            return _width * _height * _depth;
        }
    }

    public int rowSize
    {
        get
        {
            return _width * _depth;
        }
    }

    public int offset(int x, int y)
    {
        return (Mathf.Clamp(x, 0, _width - 1) + Mathf.Clamp(y, 0, _height - 1) * _width) * _depth; 
    }

    public void set(int ofs, byte[] value)
    {
        int sz = Mathf.Min(_depth, value.Length);
        for (int k = 0; k < sz; k++)
            _buffer[ofs + k] = value[k];
    }

    public void set(int x, int y, byte[] value)
    {
        set(offset(x, y), value);
    }

    public void set(int ofs, Color value)
    {
        _buffer[ofs + 0] = (byte)(value.r * 255.0f);
        if (_depth > 1)
            _buffer[ofs + 1] = (byte)(value.g * 255.0f);
        if (_depth > 2)
            _buffer[ofs + 2] = (byte)(value.b * 255.0f);
        if(_depth > 3)
            _buffer[ofs + 3] = (byte)(value.a * 255.0f);
    }

    public void set(int ofs, Color32 value)
    {
        _buffer[ofs + 0] = value.r;
        if (_depth > 1)
            _buffer[ofs + 1] = value.g;
        if (_depth > 2)
            _buffer[ofs + 2] = value.b;
        if (_depth > 3)
            _buffer[ofs + 3] = value.a;
    }

    public Color rgb(int x, int y)
    {
        return rgb(offset(x, y));
    }

    public Color rgb(int ofs)
    {
        return new Color32(_buffer[ofs + 0], _buffer[ofs + 1], _buffer[ofs + 2], 255);
    }

    public Color rgba(int ofs)
    {
        return new Color32(_buffer[ofs + 0], _buffer[ofs + 1], _buffer[ofs + 2], _buffer[ofs + 3]);
    }

    public float alpha(int x, int y)
    {
        return alpha(offset(x, y));
    }

    public float alpha(int ofs)
    {
        return (float)_buffer[ofs + 3] / 255.0f;
    }

    public PixelBlock(int width, int height, int depthBytes = 4)
    {
        this._width = width;
        this._height = height;
        this._depth = depthBytes;

        _buffer = new byte[width * height * depthBytes];
    }
}

public class BufferBlit
{
    public enum BlendOp
    {
        Opaque,
        Alpha,
        Additive
    };

    public static void Fill(PixelBlock dst, byte[] value)
    {
        //int sz = Mathf.Min(dst.depth, value.Length);
        for (int i = 0; i < dst.sizeBytes; i += dst.depth)
        {
            dst.set(i, value);
        }
    }

    public static void Fill(PixelBlock dst, Color value)
    {
        byte[] _val = new byte[4];
        _val[0] = (byte)(value.r * 255.0f);
        _val[1] = (byte)(value.g * 255.0f);
        _val[2] = (byte)(value.b * 255.0f);
        _val[3] = (byte)(value.a * 255.0f);
        Fill(dst, _val);
    }

    public static void Fill(PixelBlock dst, int dstx, int dsty, int dstWidth, int dstHeight, byte[] value)
    {
        for (int y = 0; y < dstHeight; y++)
        {
            for (int x = 0; x < dstWidth; x++)
            {
                dst.set(dstx + x, dsty + y, value);
            }
        }
    }

    private static void Blend(PixelBlock dst, int dstOfs, PixelBlock src, int srcOfs, BlendOp op)
    {
        int pixelSz = Mathf.Min(src.depth, dst.depth);
        if(op == BlendOp.Opaque)
        {
            for (int k = 0; k < pixelSz; k++ )
                dst.buffer[dstOfs + k] = src.buffer[srcOfs + k];
        }

        if (op == BlendOp.Alpha)
        {
            Color dstCol = dst.rgba(dstOfs);
            Color srcCol = src.rgba(srcOfs);

            Color c = srcCol * srcCol.a + dstCol * (1.0f - srcCol.a);
            dst.set(dstOfs, c);
        }
    }

    public static void Blit(PixelBlock dst, int dstx, int dsty, PixelBlock src, BlendOp blendOp = BlendOp.Opaque)
    {
        Blit(dst, dstx, dsty, src, 0, 0, src.width, src.height, blendOp);
    }

    public static void Blit(PixelBlock dst, int dstx, int dsty, PixelBlock src, int srcx, int srcy, int srcWidth, int srcHeight, BlendOp blendOp)
    {
        //int pixelSz = Mathf.Min(src.depth, dst.depth);

        for(int y = 0; y < srcHeight; y++)
        {
            for(int x = 0; x < srcWidth; x++)
            {
                int srcOfs = src.offset(srcx + x, srcy + y);
                int dstOfs = dst.offset(dstx + x, dsty + y);

                Blend(dst, dstOfs, src, srcOfs, blendOp);
            }
        }
    }

    public static void Copy(PixelBlock dst, byte[] pixels)
    {
        for (int i = 0; i < dst.sizeBytes; i++)
            dst.buffer[i] = pixels[i];
    }
    public static void Copy(PixelBlock dst, Color[] pixels)
    {
        for(int i = 0; i < dst.size; i++)
        {
            dst.set(i * dst.depth, pixels[i]);
        }
    }

    public static void Copy(PixelBlock dst, Color32[] pixels)
    {
        for (int i = 0; i < dst.size; i++)
        {
            dst.set(i * dst.depth, pixels[i]);
        }
    }

    public static PixelBlock Resize(PixelBlock src, int newWidth, int newHeight)
    {
        PixelBlock dst = new PixelBlock(newWidth, newHeight);

        float scalex = (float)src.width / (float)newWidth;
        float scaley = (float)src.height / (float)newHeight;

        int stx = Mathf.CeilToInt((scalex - 1.0f) * 0.5f);
        int sty = Mathf.CeilToInt((scaley - 1.0f) * 0.5f);

        //float borderWeightX = (scalex - 1.0f) * 0.5f - Mathf.Floor((scalex - 1.0f) * 0.5f);
        //float borderWeightY = (scaley - 1.0f) * 0.5f - Mathf.Floor((scaley - 1.0f) * 0.5f);

        //float invCount = 1.0f / ((2.0f * stx + 1.0f) * (2.0f * sty + 1.0f));

        for (int y = 0; y < newHeight; y++ )
        {
            for (int x = 0; x < newWidth; x++)
            {
                Color dstCol = new Color(0.0f, 0.0f, 0.0f, 0.0f);

                float dstCenterX = ((float)x + 0.5f) * scalex;
                float dstCenterY = ((float)y + 0.5f) * scaley;

                float cc = 0.0f;
                for(int py = -sty; py <= sty; py++)
                {
                    for(int px = -stx; px <= stx; px++)
                    {
                        int ofs = src.offset((int)dstCenterX + px, (int)dstCenterY + py);
                        Color pix = src.rgba(ofs);

                        float weight = 1.0f;

                        /*if(Mathf.Abs(px) == px)
                            weight *= borderWeightX;
                        if (Mathf.Abs(py) == py)
                            weight *= borderWeightY;*/

                        dstCol += pix;// *weight;
                        cc += weight;
                    }
                }
                dst.set(dst.offset(x, y), dstCol / cc);////* invCount);
            }
        }

        return dst;
    }
}
