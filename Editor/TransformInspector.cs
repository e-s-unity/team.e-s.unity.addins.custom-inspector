#nullable enable

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace Es.Unity.Addins.CustomInspectors
{

    [UnityEditor.CustomEditor(typeof(UnityEngine.Transform))]
    public class TransformInspector : UnityEditor.Editor
    {


        private static GUIContent _Content_Translation = new GUIContent("Translation", "The position of the transform.");
        private static GUIContent _Content____Rotation = new GUIContent("Rotation", "The rotation of the transform.");
        private static GUIContent _Content_______Scale = new GUIContent("Scale", "The scale of the transform.");

        public override bool RequiresConstantRepaint() => true;
           

        protected UnityEngine.Transform Target => (UnityEngine.Transform)base.target;

        private Vector3 _Temp_Position;
        private Vector3 _Temp_Rotation;
        private Vector3 _Temp____Scale;

        private bool _DrawAsWorld = false;

        


        private bool HasParent => this.Target.parent != null;

        private static void SetScale(Transform transform, Vector3 worldScale) {
            var gs = transform.lossyScale;
            transform.localScale = new(worldScale.x / gs.x, worldScale.y / gs.y, worldScale.z / gs.z);
        }

        private static readonly GUIContent EmptyContent = new("@");
        private static Matrix4x4 DrawMatrix4x4Field(GUIContent content, Matrix4x4 current) {

            Vector4 drawRow(GUIContent content, Vector4 currentRow) {
                float w = (EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 20 - EditorGUIUtility.standardVerticalSpacing * 6) / 4;
                using(new EditorGUILayout.HorizontalScope()) {
                    EditorGUILayout.LabelField(content, GUILayout.Width(EditorGUIUtility.labelWidth));
                    currentRow.x = EditorGUILayout.FloatField(GUIContent.none, currentRow.x, GUILayout.Width(w));
                    currentRow.y = EditorGUILayout.FloatField(GUIContent.none, currentRow.y, GUILayout.Width(w));
                    currentRow.z = EditorGUILayout.FloatField(GUIContent.none, currentRow.z, GUILayout.Width(w));
                    currentRow.w = EditorGUILayout.FloatField(GUIContent.none, currentRow.w, GUILayout.Width(w));
                }
                return currentRow;
                //return EditorGUILayout.Vector4Field(content, currentRow);
            }

            current.SetRow(0, drawRow(content        , current.GetRow(0)));
            current.SetRow(1, drawRow(EmptyContent, current.GetRow(1)));
            current.SetRow(2, drawRow(EmptyContent, current.GetRow(2)));
            current.SetRow(3, drawRow(EmptyContent, current.GetRow(3)));

            return current;
        }


#if false

        private (bool Left, bool Right) _ButtonPressed = (false, false);

        private static GUIContent[] _DoubleButtonContents = new GUIContent[2]{
            new("Global", "Display a transform in world space."),
            new("Local", "Display a transform in local space."),
        };

        private static GUIStyle? __MiniButtonStyle_L;
        private static GUIStyle? __MiniButtonStyle_R;

        private static GUIStyle _MiniButtonStyle_L => __MiniButtonStyle_L ??= new(EditorStyles.miniButtonLeft);
        private static GUIStyle _MiniButtonStyle_R => __MiniButtonStyle_R ??= new(EditorStyles.miniButtonRight);

        public override void OnInspectorGUI() {
            EditorGUI.indentLevel = 0;

            /* Which */
            float w = EditorGUIUtility.currentViewWidth - 20 - EditorGUIUtility.labelWidth;
            //float w = EditorGUIUtility.labelWidth;
            using(new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.PrefixLabel("@");
                if(!HasParent) {
                    _DrawAsWorld = false;
                    using(new EditorGUI.DisabledScope(true)) {
                        GUILayout.Button(_DoubleButtonContents[0], _MiniButtonStyle_L, GUILayout.Width(w / 2));
                        GUILayout.Button(_DoubleButtonContents[1], _MiniButtonStyle_R, GUILayout.Width(w / 2));
                    }
                }
                else {
                    Color c = GUI.color;
                    if(!_DrawAsWorld) GUI.color = Color.gray;
                    _ButtonPressed.Left = GUILayout.Button(_DoubleButtonContents[0], _MiniButtonStyle_L, GUILayout.Width(w / 2));
                    GUI.color = _DrawAsWorld ? Color.gray : c;
                    _ButtonPressed.Right = GUILayout.Button(_DoubleButtonContents[1], _MiniButtonStyle_R, GUILayout.Width(w / 2));
                    GUI.color = c;
                }
                if(_ButtonPressed.Left) {
                    _DrawAsWorld = true;
                }
                else if(_ButtonPressed.Right) {
                    _DrawAsWorld = false;
                }
            }

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            /* Property */
            using(var check = new EditorGUI.ChangeCheckScope()) {
                if(_DrawAsWorld) {
                    _Temp_Position = EditorGUILayout.Vector3Field(_Content_Translation, Target.position);
                    _Temp_Rotation = EditorGUILayout.Vector3Field(_Content____Rotation, Target.eulerAngles);
                    _Temp____Scale = EditorGUILayout.Vector3Field(_Content_______Scale, Target.lossyScale);
                }
                else {
                    _Temp_Position = EditorGUILayout.Vector3Field(_Content_Translation, Target.localPosition);
                    _Temp_Rotation = EditorGUILayout.Vector3Field(_Content____Rotation, Target.localEulerAngles);
                    _Temp____Scale = EditorGUILayout.Vector3Field(_Content_______Scale, Target.localScale);
                }

                if(check.changed) {
                    if(_DrawAsWorld) {
                        Target.position = _Temp_Position;
                        Target.eulerAngles = _Temp_Rotation;
                        Target.localScale = new(_Temp____Scale.x / Target.lossyScale.x, _Temp____Scale.y / Target.lossyScale.y, _Temp____Scale.z / Target.lossyScale.z);
                    }
                    else {
                        Target.localPosition = _Temp_Position;
                        Target.localEulerAngles = _Temp_Rotation;
                        Target.localScale = _Temp____Scale;
                    }
                    EditorUtility.SetDirty(Target);
                    Undo.RecordObject(Target, "Change Transform");
                }

            }

        }

#else

        private static GUIStyle? __LabelButtonStyle;
        private static GUIStyle _LabelButtonStyle => __LabelButtonStyle ??= new(EditorStyles.miniButton);

        private bool _DrawAsWorld_Translation = false;
        private bool _DrawAsWorld_Rotation = false;
        private bool _DrawAsWorld_Scale = false;

        public override void OnInspectorGUI() {

            bool hasAnyChange = false;

            hasAnyChange |= DrawTransformComponent(ref _DrawAsWorld_Translation, "Translation", "position",
                () => DrawVector3Field(() => Target.position, _ => Target.position = _),
                () => DrawVector3Field(() => Target.localPosition, _ => Target.localPosition = _));

            hasAnyChange |= DrawTransformComponent(ref _DrawAsWorld_Rotation, "Rotation", "rotation", 
                () => DrawVector3Field(() => Target.eulerAngles, _ => Target.eulerAngles = _),
                () => DrawVector3Field(() => Target.localEulerAngles, _ => Target.localEulerAngles = _));

            hasAnyChange |= DrawTransformComponent(ref _DrawAsWorld_Scale, "Scale", "scale",
                () => DrawVector3Field(() => Target.lossyScale, _ => SetScale(Target, _)),
                () => DrawVector3Field(() => Target.localScale, _ => Target.localScale = _));

            if(hasAnyChange) {
                EditorUtility.SetDirty(Target);
                Undo.RecordObject(Target, "Change Transform");
            }


#if false
            EditorGUILayout.Space();
            DrawMatrix4x4Field(new GUIContent("Matrix"), Target.localToWorldMatrix);
#endif

        }

        private bool DrawTransformComponent(ref bool drawAsWorld, string label, string tipName, Func<bool> worldFieldDrawer, Func<bool> localFieldDrawer) {
            bool hasAnyChange = false;
            using(new EditorGUILayout.HorizontalScope()) {
                if(HasParent) {
                    if(GUILayout.Button(new GUIContent(label, $"The {tipName} in {(drawAsWorld ? "world" : "local")} space."),
                    drawAsWorld ? EditorStyles.boldLabel : EditorStyles.label,
                    GUILayout.Width(EditorGUIUtility.labelWidth))) {
                        drawAsWorld = !drawAsWorld; // Toggle
                    }
                }
                else {
                    EditorGUILayout.LabelField(new GUIContent(label, $"The {tipName} in both of world or local space."), EditorStyles.boldLabel, GUILayout.Width(EditorGUIUtility.labelWidth));
                }
                if(drawAsWorld) {
                    hasAnyChange |= worldFieldDrawer();
                }
                else {
                    hasAnyChange |= localFieldDrawer();
                }
            }
            return hasAnyChange;
        }

        private bool DrawVector3Field(Func<Vector3> get, Action<Vector3> set) {
            bool hasChanged = false;
            using(var check = new EditorGUI.ChangeCheckScope()) {
                var after = EditorGUILayout.Vector3Field(GUIContent.none, get());
                hasChanged |= check.changed;
                if(check.changed) {
                    set(after);
                }
            }
            return hasChanged;
        }

#endif

        }

}

#endif