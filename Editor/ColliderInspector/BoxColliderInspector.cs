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
    [UnityEditor.CustomEditor(typeof(UnityEngine.BoxCollider))]
    public class BoxColliderInspector : ColliderInspectorBase<BoxCollider>
    {
        private protected override void DrawShapeProperties() {
            Target.center   = EditorGUILayout.Vector3Field(new GUIContent("Center"), Target.center);
            Target.size     = EditorGUILayout.Vector3Field(new GUIContent("Size"), Target.size);
            Target.extents  = EditorGUILayout.Vector3Field(new GUIContent("Extents"), Target.extents);
        }
    }
}
