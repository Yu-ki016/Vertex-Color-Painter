using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VertexColorObject;

namespace VertexColorPainter
{
    public class VertexColorPainterWindow : EditorWindow //创建编辑器窗口需要继承自EditorWindow类
    {
        //设置的一些变量
        #region Varialbes

        //bool
        bool allowPainting = false; //当allowPainting为true时才可以开始绘制
        bool isPainting = false; //当isPainting为true时，会在网格上画上一笔
        bool painted = false; //当绘制了顶点色之后painted会设置为true

        private int colorChannel = (int)Channel.RGBA; //将颜色绘制到哪个通道

        //brush
        private Color brushColor = Color.white;
        private float brushSize = 1.0f;
        private float powBrushSize = 1.0f; //对笔刷尺寸开方，让笔刷尺寸的范围更大一点
        private float brushIntensity = 1.0f; //影响笔刷绘制的强度
        private float brushOpacity = 1.0f; //只影响GUI显示的透明度，不会影响笔刷强度
        private float brushStep = 0.5f; //笔触之间的间距
        private float brushSmoothness = 1.0f; //笔刷软硬程度
        private float brushFocalShift =1.0f; //焦点偏移，笔刷中间实心部分的范围

        //mouse
        private Vector2 mousePosition = Vector2.zero;
        private Vector2 lastMousePositon = Vector2.zero; //记录上一个笔触的位置
        private RaycastHit curHit; //从鼠标位置发射，与网格相交的射线

        //mesh
        GameObject thisGameObject;
        VCObject thisVertexColorObject;
        Mesh thisMesh;


        #endregion

        //使用[MenuItem]来创建一个菜单
        //当点击时会调用它下方的第一个函数
        //_%&#C是快捷键
        //%为ctrl，&为alt，#为shift，C为键盘按键C
        [MenuItem("Window/Vertex Color Painter _%&#C")]
        public static void OpenPainterWindow()
        {
            VertexColorPainterWindow window = (VertexColorPainterWindow)EditorWindow.GetWindow(typeof(VertexColorPainterWindow), true, "Vertex Painter");
            window.OnSelectionChange();
            window.Show();
        }

        private void OnEnable()
        {
            //注册duringSceneGui，绘制SceneGUI
            SceneView.duringSceneGui -= this.OnSceneGUI;
            SceneView.duringSceneGui += this.OnSceneGUI;
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
        }

        //当选中的物体发生变化时会调用OnSelectionChange()
        private void OnSelectionChange()
        {
            //下面的语句用于更新网格
            thisGameObject = Selection.activeGameObject;
            thisVertexColorObject = null;
            thisMesh = null;
            if (thisGameObject)
            {
                thisVertexColorObject = Selection.activeGameObject.GetComponent<VCObject>();
                thisMesh = MeshUtils.GetMesh(Selection.activeGameObject);
            }
            Repaint();
        }

        //OnInspectorUpdate()会以每秒10帧的速度调用，可以用于更新网格
        private void OnInspectorUpdate()
        {
            OnSelectionChange();
        }

        //绘制编辑器窗口
        #region Editor Window
        //使用OnGUI函数来绘制编辑器窗口
        private void OnGUI()
        {
            if (thisGameObject)
            {
                if (thisMesh)
                {
                    if (thisVertexColorObject)
                    {
                        //使用GUILayout.Toggle创建一个复选框
                        //GUILayout支持的其他组件请自行阅读：https://docs.unity3d.com/ScriptReference/GUILayout.html
                        allowPainting = GUILayout.Toggle(allowPainting, "Enalbe Painting");

                        if (allowPainting) //取消激活使用的工具
                        {
                            Tools.current = Tool.None;
                        }

                        //选择绘制的通道
                        //在BeginHorizontal()和EndHorizontal()中间的组件会创建到同一行
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Channel：");
                        string[] channelName = { "RGBA", "RGB", "R", "G", "B", "A" };
                        int[] channelID = { 0, 1, 2, 3, 4, 5 };
                        colorChannel = EditorGUILayout.IntPopup(colorChannel, channelName, channelID);
                        GUILayout.EndHorizontal();

                        //笔刷颜色
                        GUILayout.BeginHorizontal();
                        brushColor = EditorGUILayout.ColorField("Brush Color", brushColor);
                        //当选择的是单通道时，把颜色变为灰阶
                        if (colorChannel != (int)Channel.RGBA && colorChannel != (int)Channel.RGB)
                        {
                            brushColor = new Color(brushColor.maxColorComponent, brushColor.maxColorComponent, brushColor.maxColorComponent, brushColor.a);
                        }

                        //填充颜色
                        if (GUILayout.Button("Fill Color"))
                        {
                            ColorUtils.FillVertexColor(thisMesh, brushColor, (Channel)colorChannel);
                        }

                        GUILayout.EndHorizontal();

                        //调整笔刷的一些参数
                        brushSize = EditorGUILayout.Slider("Brush Size", brushSize, 0.1f, 10f);
                        powBrushSize = (Mathf.Pow(brushSize, 2)) / 10.0f;// 使用powBrushSize可以让笔刷尺寸调整的范围更广一些
                        brushStep = EditorGUILayout.Slider("Brush Step", brushStep, 0.01f, 10.0f);
                        brushIntensity = EditorGUILayout.Slider("Brush Intensity", brushIntensity, 0, 1);
                        brushOpacity = EditorGUILayout.Slider("Brush Opacity", brushOpacity, 0, 1);
                        brushSmoothness = EditorGUILayout.Slider("Brush Smoothness", brushSmoothness, 0.1f, 2.5f);
                        brushFocalShift = EditorGUILayout.Slider("Brush Focal Shift", brushFocalShift, 1, 10);

                        //保存绘制好的网格
                        if (GUILayout.Button("save") && thisMesh != null)
                        {
                            string path = EditorUtility.SaveFilePanel("Export .asset file", "Assets", MeshUtils.SanitizeForFileName(thisMesh.name), "asset");
                            if (path.Length > 0)
                            {
                                var dataPath = Application.dataPath;
                                if (!path.StartsWith(dataPath))
                                {
                                    Debug.LogError("Invalid path: Path must be under " + dataPath);
                                }
                                else
                                {
                                    path = path.Replace(dataPath, "Assets");
                                    AssetDatabase.CreateAsset(Instantiate(thisMesh), path);
                                    Debug.Log("Asset exported: " + path);
                                }
                            }
                        }

                        GUILayout.Label("brushStep为绘制的笔触的间隔\n" +
                                        "brushOpacity只影响绘制时的UI的透明度，防止UI挡住视线\n" +
                                        "brushFocalShift影响笔刷中间实心部分的范围，为1时无实心部分\n" +
                                        "绘制完了记得按save保存网格");
                    }
                    else
                    {
                        if (GUILayout.Button("Convert " + thisGameObject.name + " to VertexColorObject"))
                        {
                            thisGameObject.AddComponent<VCObject>();
                        }
                    }
                }
                else
                {
                    GUILayout.Label("GameObject中没有可以绘制的mesh");
                }
            }
            else
            {
                GUILayout.Label("请选中需要绘制的GameObject");
            }
            

            Repaint();
        }

        #endregion

        //绘制3D的UI
        #region SceneGUI
        void OnSceneGUI(SceneView sceneView)
        {
            ProcessInputs(); //更新鼠标位置和isPainting

            if (thisMesh && thisVertexColorObject)
            {
                if (allowPainting)
                {
                    LimitSceneSelection(); //防止绘制过程选中其他物体

                    bool isHit = false; //用于判断鼠标位置是否有网格

                    Ray worldRay = HandleUtility.GUIPointToWorldRay(mousePosition); //从鼠标位置发生射线
                    Matrix4x4 mtx = thisVertexColorObject.transform.localToWorldMatrix;
                    RaycastHit tempHit;
                    //unity想要求Ray与Mesh的交点的话需要有Collider，
                    //使用IntersectRayMesh()函数通过反射来实现Ray和Mesh的碰撞检测
                    //来自于项目：https://gist.github.com/MattRix/9205bc62d558fef98045
                    isHit = RXLookingGlass.IntersectRayMesh(worldRay, thisMesh, mtx, out tempHit);

                    if (isHit)
                    {
                        curHit = tempHit; //与网格相交的RaycastHit

                        //绘制3D的GUI
                        Handles.color = GetSolidDiscColor((Channel)colorChannel);
                        Handles.DrawSolidDisc(curHit.point, curHit.normal, powBrushSize);
                        Handles.color = Color.white;
                        Handles.DrawWireDisc(curHit.point, curHit.normal, powBrushSize);

                        if (isPainting)
                        {
                            PaintVertexColor();
                        }

                    }

                    if (painted && Event.current.type == EventType.MouseUp)
                    {
                        //绘制完松开鼠标时存储一次记录，用于撤销
                        thisVertexColorObject.Record();
                        painted = false;
                    }

                }
            

                sceneView.Repaint();

            }
        }
        #endregion

        
        #region Utility Methods
        //更新鼠标位置和ispainting
        void ProcessInputs()
        {

            Event e = Event.current;
            mousePosition = e.mousePosition;

            isPainting = false;

            if (e.type == EventType.MouseDown && 
                !e.alt &&
                e.button == 0) //当左键点击且没有按住alt
            {
                isPainting = true;
            }

            if (e.type == EventType.MouseDrag && 
                (mousePosition - lastMousePositon).magnitude > powBrushSize * brushStep && 
                !e.alt &&
                e.button == 0)//当鼠标左键拖拽，鼠标移动的距离大于笔触间接，且没有按住alt时
            {
                isPainting = true;
            }

            if (e.type == EventType.MouseUp)
            {
                isPainting = false;
            }

        }

        //防止绘制时选中其他物体
        private void LimitSceneSelection()
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);
        }

        //绘制顶点色
        private void PaintVertexColor()
        {
            if (thisMesh)
            {
                Vector3[] vertices = thisMesh.vertices;
                Color[] colors;

                //更新顶点色
                if (thisMesh.colors.Length > 0)
                {
                    colors = thisMesh.colors;
                }
                else
                {
                    colors = new Color[vertices.Length];
                }

                for (int i = 0; i < vertices.Length; i++)
                {
                    //将顶点变换到世界空间
                    Vector3 vertPos = thisVertexColorObject.transform.TransformPoint(vertices[i]);

                    float magnitude = (vertPos - curHit.point).magnitude;//计算顶点与鼠标位置的距离
                    if (magnitude > brushSize)
                    {
                        continue;
                    }
                    //计算衰减
                    float falloff = ColorUtils.LinearFalloff(magnitude, powBrushSize);
                    falloff = ColorUtils.GetFalloff(falloff, brushFocalShift, brushSmoothness);
                    falloff *= brushIntensity;
                    //混合颜色
                    colors[i] = ColorUtils.ColorLerp(colors[i], brushColor, falloff, (Channel)colorChannel);

                }
                thisMesh.colors = colors;

                painted = true;
                lastMousePositon = mousePosition; //绘制完一个点，记录此时的鼠标位置
            }

        }

        private Color GetSolidDiscColor(Channel channal)
        {
            if (channal == Channel.A)
            {
                var A = Mathf.Pow(brushColor.a, 1 / 2.2f);
                return new Color(A, A, A, brushOpacity * brushIntensity);
            }

            //gamma校正
            //刷上去的顶点色以笔刷显示的颜色为准
            var R = Mathf.Pow(brushColor.r, 1 / 2.2f);
            var G = Mathf.Pow(brushColor.g, 1 / 2.2f);
            var B = Mathf.Pow(brushColor.b, 1 / 2.2f);
            return new Color(R, G, B, brushOpacity * brushIntensity);
        }

        #endregion
    }

}
