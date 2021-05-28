using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VertexColorPainter
{
    //定义颜色通道
    public enum Channel
    {
        RGBA = 0,
        RGB,
        R,
        G,
        B,
        A,
    }

    public static class ColorUtils
    {
        public static void FillVertexColor(Mesh mesh, Color brushColor, Channel channel)
        {
            if (mesh)
            {
                Vector3[] vertices = mesh.vertices;
                Color[] vertexColor;
                if (mesh.colors.Length > 0)
                {
                    vertexColor = mesh.colors;
                }
                else
                {
                    vertexColor = new Color[vertices.Length];
                }
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertexColor[i] = ColorLerp(vertexColor[i], brushColor, 1.0f, channel);
                }
                mesh.colors = vertexColor;
            }
            else
            {
                Debug.LogWarning("Nothing to fill");
            }
        }

        //下面两个函数用来计算笔刷的衰减
        public static float LinearFalloff(float distance, float brushRadius)
        {
            return Mathf.Clamp01(1 - distance / brushRadius);
        }
        public static float GetFalloff(float linerFalloff, float focalShift, float smoothness)
        {
            float falloff = Mathf.Clamp01(linerFalloff * focalShift);
            falloff = Mathf.Pow(falloff, smoothness);
            return falloff;
        }

        //混合颜色
        public static Color ColorLerp(Color curColor, Color brushColor, float value, Channel channel)
        {
            value = Mathf.Clamp01(value);
            if (value == 0f)
            {
                return curColor;
            }
            else
            {
                switch (channel)
                {
                    case Channel.RGBA:
                        return new Color(curColor.r + (brushColor.r - curColor.r) * value,
                                         curColor.g + (brushColor.g - curColor.g) * value,
                                         curColor.b + (brushColor.b - curColor.b) * value,
                                         curColor.a + (brushColor.a - curColor.a) * value);
                    case Channel.RGB:
                        return new Color(curColor.r + (brushColor.r - curColor.r) * value,
                                         curColor.g + (brushColor.g - curColor.g) * value,
                                         curColor.b + (brushColor.b - curColor.b) * value,
                                         curColor.a);
                    case Channel.R:
                        return new Color(curColor.r + (brushColor.r - curColor.r) * value,
                                         curColor.g,
                                         curColor.b,
                                         curColor.a);
                    case Channel.G:
                        return new Color(curColor.r,
                                         curColor.g + (brushColor.g - curColor.g) * value,
                                         curColor.b,
                                         curColor.a);
                    case Channel.B:
                        return new Color(curColor.r,
                                         curColor.g,
                                         curColor.b + (brushColor.b - curColor.b) * value,
                                         curColor.a);
                    case Channel.A:
                        return new Color(curColor.r,
                                         curColor.g,
                                         curColor.b,
                                         curColor.a + (brushColor.a - curColor.a) * value);
                }
                //表示错误
                return Color.cyan;
            }
        }

        
    }
}

