using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace VertexColorObject
{
    [Serializable] //为了实现撤销功能，使用这个属性使Record类可序列化
    public class Record
    {
        public Color[] colors;
    }

    [ExecuteInEditMode] //这个属性是让脚本能够在EditMode（不在游戏运行状态）下运行
    public class VCObject : MonoBehaviour
    {
#if UNITY_EDITOR


        [SerializeField] Record meshRecorder = new Record();
        int index = 0;

        public void Record()
        {
            var mesh = MeshUtils.GetMesh(gameObject);//获取网格
            if (mesh != null)
            {
                Undo.RecordObject(this, "[" + index + "]"); //记录操作
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
