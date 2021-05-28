using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VertexColorObject;

namespace VertexColorPainter
{
    public class VertexColorPainterWindow : EditorWindow //�����༭��������Ҫ�̳���EditorWindow��
    {
        //���õ�һЩ����
        #region Varialbes

        //bool
        bool allowPainting = false; //��allowPaintingΪtrueʱ�ſ��Կ�ʼ����
        bool isPainting = false; //��isPaintingΪtrueʱ�����������ϻ���һ��
        bool painted = false; //�������˶���ɫ֮��painted������Ϊtrue

        private int colorChannel = (int)Channel.RGBA; //����ɫ���Ƶ��ĸ�ͨ��

        //brush
        private Color brushColor = Color.white;
        private float brushSize = 1.0f;
        private float powBrushSize = 1.0f; //�Ա�ˢ�ߴ翪�����ñ�ˢ�ߴ�ķ�Χ����һ��
        private float brushIntensity = 1.0f; //Ӱ���ˢ���Ƶ�ǿ��
        private float brushOpacity = 1.0f; //ֻӰ��GUI��ʾ��͸���ȣ�����Ӱ���ˢǿ��
        private float brushStep = 0.5f; //�ʴ�֮��ļ��
        private float brushSmoothness = 1.0f; //��ˢ��Ӳ�̶�
        private float brushFocalShift =1.0f; //����ƫ�ƣ���ˢ�м�ʵ�Ĳ��ֵķ�Χ

        //mouse
        private Vector2 mousePosition = Vector2.zero;
        private Vector2 lastMousePositon = Vector2.zero; //��¼��һ���ʴ���λ��
        private RaycastHit curHit; //�����λ�÷��䣬�������ཻ������

        //mesh
        GameObject thisGameObject;
        VCObject thisVertexColorObject;
        Mesh thisMesh;


        #endregion

        //ʹ��[MenuItem]������һ���˵�
        //�����ʱ��������·��ĵ�һ������
        //_%&#C�ǿ�ݼ�
        //%Ϊctrl��&Ϊalt��#Ϊshift��CΪ���̰���C
        [MenuItem("Window/Vertex Color Painter _%&#C")]
        public static void OpenPainterWindow()
        {
            VertexColorPainterWindow window = (VertexColorPainterWindow)EditorWindow.GetWindow(typeof(VertexColorPainterWindow), true, "Vertex Painter");
            window.OnSelectionChange();
            window.Show();
        }

        private void OnEnable()
        {
            //ע��duringSceneGui������SceneGUI
            SceneView.duringSceneGui -= this.OnSceneGUI;
            SceneView.duringSceneGui += this.OnSceneGUI;
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
        }

        //��ѡ�е����巢���仯ʱ�����OnSelectionChange()
        private void OnSelectionChange()
        {
            //�����������ڸ�������
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

        //OnInspectorUpdate()����ÿ��10֡���ٶȵ��ã��������ڸ�������
        private void OnInspectorUpdate()
        {
            OnSelectionChange();
        }

        //���Ʊ༭������
        #region Editor Window
        //ʹ��OnGUI���������Ʊ༭������
        private void OnGUI()
        {
            if (thisGameObject)
            {
                if (thisMesh)
                {
                    if (thisVertexColorObject)
                    {
                        //ʹ��GUILayout.Toggle����һ����ѡ��
                        //GUILayout֧�ֵ���������������Ķ���https://docs.unity3d.com/ScriptReference/GUILayout.html
                        allowPainting = GUILayout.Toggle(allowPainting, "Enalbe Painting");

                        if (allowPainting) //ȡ������ʹ�õĹ���
                        {
                            Tools.current = Tool.None;
                        }

                        //ѡ����Ƶ�ͨ��
                        //��BeginHorizontal()��EndHorizontal()�м������ᴴ����ͬһ��
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Channel��");
                        string[] channelName = { "RGBA", "RGB", "R", "G", "B", "A" };
                        int[] channelID = { 0, 1, 2, 3, 4, 5 };
                        colorChannel = EditorGUILayout.IntPopup(colorChannel, channelName, channelID);
                        GUILayout.EndHorizontal();

                        //��ˢ��ɫ
                        GUILayout.BeginHorizontal();
                        brushColor = EditorGUILayout.ColorField("Brush Color", brushColor);
                        //��ѡ����ǵ�ͨ��ʱ������ɫ��Ϊ�ҽ�
                        if (colorChannel != (int)Channel.RGBA && colorChannel != (int)Channel.RGB)
                        {
                            brushColor = new Color(brushColor.maxColorComponent, brushColor.maxColorComponent, brushColor.maxColorComponent, brushColor.a);
                        }

                        //�����ɫ
                        if (GUILayout.Button("Fill Color"))
                        {
                            ColorUtils.FillVertexColor(thisMesh, brushColor, (Channel)colorChannel);
                        }

                        GUILayout.EndHorizontal();

                        //������ˢ��һЩ����
                        brushSize = EditorGUILayout.Slider("Brush Size", brushSize, 0.1f, 10f);
                        powBrushSize = (Mathf.Pow(brushSize, 2)) / 10.0f;// ʹ��powBrushSize�����ñ�ˢ�ߴ�����ķ�Χ����һЩ
                        brushStep = EditorGUILayout.Slider("Brush Step", brushStep, 0.01f, 10.0f);
                        brushIntensity = EditorGUILayout.Slider("Brush Intensity", brushIntensity, 0, 1);
                        brushOpacity = EditorGUILayout.Slider("Brush Opacity", brushOpacity, 0, 1);
                        brushSmoothness = EditorGUILayout.Slider("Brush Smoothness", brushSmoothness, 0.1f, 2.5f);
                        brushFocalShift = EditorGUILayout.Slider("Brush Focal Shift", brushFocalShift, 1, 10);

                        //������ƺõ�����
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

                        GUILayout.Label("brushStepΪ���Ƶıʴ��ļ��\n" +
                                        "brushOpacityֻӰ�����ʱ��UI��͸���ȣ���ֹUI��ס����\n" +
                                        "brushFocalShiftӰ���ˢ�м�ʵ�Ĳ��ֵķ�Χ��Ϊ1ʱ��ʵ�Ĳ���\n" +
                                        "�������˼ǵð�save��������");
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
                    GUILayout.Label("GameObject��û�п��Ի��Ƶ�mesh");
                }
            }
            else
            {
                GUILayout.Label("��ѡ����Ҫ���Ƶ�GameObject");
            }
            

            Repaint();
        }

        #endregion

        //����3D��UI
        #region SceneGUI
        void OnSceneGUI(SceneView sceneView)
        {
            ProcessInputs(); //�������λ�ú�isPainting

            if (thisMesh && thisVertexColorObject)
            {
                if (allowPainting)
                {
                    LimitSceneSelection(); //��ֹ���ƹ���ѡ����������

                    bool isHit = false; //�����ж����λ���Ƿ�������

                    Ray worldRay = HandleUtility.GUIPointToWorldRay(mousePosition); //�����λ�÷�������
                    Matrix4x4 mtx = thisVertexColorObject.transform.localToWorldMatrix;
                    RaycastHit tempHit;
                    //unity��Ҫ��Ray��Mesh�Ľ���Ļ���Ҫ��Collider��
                    //ʹ��IntersectRayMesh()����ͨ��������ʵ��Ray��Mesh����ײ���
                    //��������Ŀ��https://gist.github.com/MattRix/9205bc62d558fef98045
                    isHit = RXLookingGlass.IntersectRayMesh(worldRay, thisMesh, mtx, out tempHit);

                    if (isHit)
                    {
                        curHit = tempHit; //�������ཻ��RaycastHit

                        //����3D��GUI
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
                        //�������ɿ����ʱ�洢һ�μ�¼�����ڳ���
                        thisVertexColorObject.Record();
                        painted = false;
                    }

                }
            

                sceneView.Repaint();

            }
        }
        #endregion

        
        #region Utility Methods
        //�������λ�ú�ispainting
        void ProcessInputs()
        {

            Event e = Event.current;
            mousePosition = e.mousePosition;

            isPainting = false;

            if (e.type == EventType.MouseDown && 
                !e.alt &&
                e.button == 0) //����������û�а�סalt
            {
                isPainting = true;
            }

            if (e.type == EventType.MouseDrag && 
                (mousePosition - lastMousePositon).magnitude > powBrushSize * brushStep && 
                !e.alt &&
                e.button == 0)//����������ק������ƶ��ľ�����ڱʴ���ӣ���û�а�סaltʱ
            {
                isPainting = true;
            }

            if (e.type == EventType.MouseUp)
            {
                isPainting = false;
            }

        }

        //��ֹ����ʱѡ����������
        private void LimitSceneSelection()
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);
        }

        //���ƶ���ɫ
        private void PaintVertexColor()
        {
            if (thisMesh)
            {
                Vector3[] vertices = thisMesh.vertices;
                Color[] colors;

                //���¶���ɫ
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
                    //������任������ռ�
                    Vector3 vertPos = thisVertexColorObject.transform.TransformPoint(vertices[i]);

                    float magnitude = (vertPos - curHit.point).magnitude;//���㶥�������λ�õľ���
                    if (magnitude > brushSize)
                    {
                        continue;
                    }
                    //����˥��
                    float falloff = ColorUtils.LinearFalloff(magnitude, powBrushSize);
                    falloff = ColorUtils.GetFalloff(falloff, brushFocalShift, brushSmoothness);
                    falloff *= brushIntensity;
                    //�����ɫ
                    colors[i] = ColorUtils.ColorLerp(colors[i], brushColor, falloff, (Channel)colorChannel);

                }
                thisMesh.colors = colors;

                painted = true;
                lastMousePositon = mousePosition; //������һ���㣬��¼��ʱ�����λ��
            }

        }

        private Color GetSolidDiscColor(Channel channal)
        {
            if (channal == Channel.A)
            {
                var A = Mathf.Pow(brushColor.a, 1 / 2.2f);
                return new Color(A, A, A, brushOpacity * brushIntensity);
            }

            //gammaУ��
            //ˢ��ȥ�Ķ���ɫ�Ա�ˢ��ʾ����ɫΪ׼
            var R = Mathf.Pow(brushColor.r, 1 / 2.2f);
            var G = Mathf.Pow(brushColor.g, 1 / 2.2f);
            var B = Mathf.Pow(brushColor.b, 1 / 2.2f);
            return new Color(R, G, B, brushOpacity * brushIntensity);
        }

        #endregion
    }

}
