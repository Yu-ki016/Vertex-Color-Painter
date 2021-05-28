using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace VertexColorObject
{
    [Serializable] //Ϊ��ʵ�ֳ������ܣ�ʹ���������ʹRecord������л�
    public class Record
    {
        public Color[] colors;
    }

    [ExecuteInEditMode] //����������ýű��ܹ���EditMode��������Ϸ����״̬��������
    public class VCObject : MonoBehaviour
    {
#if UNITY_EDITOR


        [SerializeField] Record meshRecorder = new Record();
        int index = 0;

        public void Record()
        {
            var mesh = MeshUtils.GetMesh(gameObject);//��ȡ����
            if (mesh != null)
            {
                Undo.RecordObject(this, "[" + index + "]"); //��¼����
                index++;
                meshRecorder.colors = new Color[mesh.colors.Length];
                Array.Copy(mesh.colors, meshRecorder.colors, mesh.colors.Length);
            }
        }

        public void UndoRedo()
        {
            var mesh = MeshUtils.GetMesh(gameObject);
            mesh.colors = this.meshRecorder.colors;
        }

        private void OnEnable()
        {
            UnityEditor.Undo.undoRedoPerformed += UndoRedo;
        }

        private void OnDestroy()
        {
            UnityEditor.Undo.undoRedoPerformed -= UndoRedo;
        }


#endif
    }

}
