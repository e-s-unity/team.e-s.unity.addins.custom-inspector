#nullable enable

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
    [UnityEditor.CustomEditor(typeof(UnityEngine.SphereCollider))]
    public class SphereColliderInspector : ColliderInspectorBase<UnityEngine.SphereCollider>
    {
        private protected override void DrawShapeProperties() {
            Target.center = EditorGUILayout.Vector3Field(new GUIContent("Center"), Target.center);
            Target.radius = EditorGUILayout.FloatField(new GUIContent("Radius"), Target.radius);
        }
    }
}
