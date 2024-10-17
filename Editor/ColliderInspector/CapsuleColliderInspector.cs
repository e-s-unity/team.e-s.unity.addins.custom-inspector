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
    [UnityEditor.CustomEditor(typeof(UnityEngine.CapsuleCollider))]
    public class CapsuleColliderInspector : ColliderInspectorBase<UnityEngine.CapsuleCollider>
    {
        private protected override void DrawShapeProperties() {
            float lw = EditorGUIUtility.labelWidth;
            Target.center = EditorGUILayout.Vector3Field(new GUIContent("Center"), Target.center);
            Target.radius = EditorGUILayout.FloatField(new GUIContent("Radius"), Target.radius);
            Target.height = EditorGUILayout.FloatField(new GUIContent("Height"), Target.height);

            Target.direction = (int)((CapsuleColliderDirections)EditorGUILayout.EnumPopup(new GUIContent("Direction"), (CapsuleColliderDirections)Target.direction));
        }

        public enum CapsuleColliderDirections
        {
            X = 0,
            Y = 1,
            Z = 2,
        }
    }
}
