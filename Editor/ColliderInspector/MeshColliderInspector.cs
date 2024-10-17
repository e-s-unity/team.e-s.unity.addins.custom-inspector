#nullable enable
#pragma warning disable CS0618

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Es.Unity.Addins.CustomInspectors
{
    /// <summary>
    /// 
    /// </summary>
    [UnityEditor.CustomEditor(typeof(UnityEngine.MeshCollider))]
    public class MeshColliderInspector : ColliderInspectorBase<UnityEngine.MeshCollider>
    {
        private protected override void DrawShapeProperties() {
            Target.convex = EditorGUILayout.Toggle(new GUIContent("Convex"), Target.convex);
            Target.sharedMesh = (UnityEngine.Mesh)EditorGUILayout.ObjectField(new GUIContent("Mesh"), Target.sharedMesh, typeof(UnityEngine.Mesh));
            Target.cookingOptions = (MeshColliderCookingOptions)EditorGUILayout.EnumFlagsField(new GUIContent("Cooking Options"), Target.cookingOptions);
        }
    }
}
