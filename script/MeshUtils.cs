using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace VertexColorObject
{
    public class MeshUtils
    {
        public static Mesh GetMesh(GameObject gameObject)
        {
            Mesh thisMesh = null;
            if (gameObject)
            {
                MeshFilter curFilter = gameObject.GetComponent<MeshFilter>();
                SkinnedMeshRenderer curSkinned = gameObject.GetComponent<SkinnedMeshRenderer>();
                //≈–∂œ π”√curFilterªÚcurSkinned
                if (curFilter && !curSkinned)
                {
                    thisMesh = curFilter.sharedMesh;
                }
                if (curSkinned)
                {
                    thisMesh = curSkinned.sharedMesh;
                }
            }

            return thisMesh;
        }

        public static string SanitizeForFileName(string name)
        {
            var reg = new Regex("[\\/:\\*\\?<>\\|\\\"]");
            return reg.Replace(name, "_");
        }
    }

}