#nullable enable
#pragma warning disable CS0618

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using static Codice.Client.Common.EventTracking.TrackFeatureUseEvent.Features.DesktopGUI.Filters;
using UnityEditorInternal;

namespace Es.Unity.Addins.CustomInspectors
{
    
    public abstract class ColliderInspectorBase<TCollider> : UnityEditor.Editor
        where TCollider : UnityEngine.Collider
    {

        public sealed override bool RequiresConstantRepaint() => false;

        protected TCollider Target => (TCollider)base.target;

        protected static float LabelWidth => EditorGUIUtility.labelWidth;

        private static void DrawColliderEditButton(UnityEditor.Editor self) {
            // use this to get edit button
            var content = EditorGUIUtility.TrTempContent("Edit Collider");
            EditorGUILayout.EditorToolbarForTarget(content, (UnityEngine.Object)self);
        }

        protected static void Space() => EditorGUILayout.Space();
        protected static void Label(string text, params GUILayoutOption[] options) => EditorGUILayout.LabelField(text, options);
        protected static void Label(GUIContent content, params GUILayoutOption[] options) => EditorGUILayout.LabelField(content, options);

        private static bool _IsExpanded_AdvancedSettings = false;

        private static UnityEngine.PhysicMaterial DefaultPhysicMaterial => new(){
            staticFriction  = 0.6f,
            dynamicFriction = 0.6f,
            bounciness = 0,
            frictionCombine = PhysicMaterialCombine.Average,
            bounceCombine = PhysicMaterialCombine.Average,
        };

        private static void DrawCommonProperties(Collider target) {

#if false
            /* Collider Settings */
            target.isTrigger = EditorGUILayout.Toggle(new GUIContent("Is Trigger", "Set whether the collider provides collisions."), target.isTrigger);
            Space();
#endif

            /* Advanced Settings */
            _IsExpanded_AdvancedSettings = EditorGUILayout.Foldout(_IsExpanded_AdvancedSettings, new GUIContent("Advanced"), EditorStyles.foldout);
            if(_IsExpanded_AdvancedSettings) {

                if(!target.isTrigger) {

                    /* Material Profiles */
                    Space();
                    var pmat = target.sharedMaterial = (UnityEngine.PhysicMaterial)EditorGUILayout.ObjectField(new GUIContent("Physic Material", "Set a profile of physic properties, Friction and Bounciness of the rigidbody."), target.sharedMaterial, typeof(UnityEngine.PhysicMaterial));
                    if(pmat == null) pmat = DefaultPhysicMaterial;
                    if(pmat != null) {
                        using(new EditorGUI.IndentLevelScope()) {
                            using(new EditorGUI.DisabledScope(true)) {
                                using(new EditorGUILayout.HorizontalScope()) {
                                    EditorGUILayout.LabelField(new GUIContent("Friction"), GUILayout.Width(EditorGUIUtility.labelWidth));
                                    using(new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel)) {
                                        float w = (EditorGUIUtility.currentViewWidth - 20);
                                        w -= EditorGUIUtility.labelWidth;
                                        w -= EditorGUIUtility.standardVerticalSpacing * (3 * 2 + 1);
                                        EditorGUILayout.LabelField(new GUIContent("Static"), GUILayout.Width(w / 4));
                                        EditorGUILayout.FloatField(GUIContent.none, pmat.staticFriction, GUILayout.Width(w / 4));
                                        EditorGUILayout.LabelField(new GUIContent("Dynamic"), GUILayout.Width(w / 4));
                                        EditorGUILayout.FloatField(GUIContent.none, pmat.dynamicFriction, GUILayout.Width(w / 4));
                                    }
                                }
                                EditorGUILayout.Slider(new GUIContent("Bounciness"), pmat.bounciness, 0, 1);
                            }
                        }
                    }
                 
                }
                //DrawOpticMaterialProperty(new GUIContent("Optic Material", "Set a profile of optic properties, Refract and Reflect of the ray."), target);

                /* Event System */
                Space();
                target.providesContacts = EditorGUILayout.Toggle(new GUIContent("Provides Contacts"), target.providesContacts);
                Space();
                /* Layer */
#if false
                using(new EditorGUI.DisabledScope(true)) {
                    string layerName = LayerMask.LayerToName(target.gameObject.layer);
                    string layerExpression = $"{target.gameObject.layer}: {layerName}";
                    //EditorGUILayout.TextField(new GUIContent("Layer", "The 'INDEX' number of the layer where this object there is in."), layerExpression);
                }

                Space();
                Label("Layer Override");
                using(new EditorGUI.IndentLevelScope()) {
                    target.layerOverridePriority = EditorGUILayout.DelayedIntField(new GUIContent("Priority"), target.layerOverridePriority);
                    target.includeLayers = DrawLayerMaskProperty(new GUIContent("Include"), target.includeLayers);
                    target.excludeLayers = DrawLayerMaskProperty(new GUIContent("Exclude"), target.excludeLayers);
                }
#else
                EditorGUILayout.LabelField(new GUIContent("Layer"));
                using(new EditorGUI.IndentLevelScope()) {
                    target.gameObject.layer = EditorGUILayout.LayerField(new GUIContent("Self", "The 'INDEX' number of the layer where this object there is in."), target.gameObject.layer);
                    EditorGUILayout.LabelField(new GUIContent("Override"));
                    using(new EditorGUI.IndentLevelScope()) {
                        target.layerOverridePriority = EditorGUILayout.DelayedIntField(new GUIContent("Priority"), target.layerOverridePriority);
                        target.includeLayers = DrawLayerMaskProperty(new GUIContent("Include"), target.includeLayers);
                        target.excludeLayers = DrawLayerMaskProperty(new GUIContent("Exclude"), target.excludeLayers);
                    }
                }

#endif
            }

        }

        private static LayerMask DrawLayerMaskProperty(GUIContent content, LayerMask current) {
            LayerMask tempMask = EditorGUILayout.MaskField(label: content,
                                                           mask: InternalEditorUtility.LayerMaskToConcatenatedLayersMask(current),
                                                           displayedOptions: InternalEditorUtility.layers);
            return tempMask;
        }

        public sealed override void OnInspectorGUI() {


            {
                float w = EditorGUIUtility.currentViewWidth - 20;
                Color c = GUI.color;
                (bool left, bool right) pressed = (false, false);
                using(new EditorGUILayout.HorizontalScope()) {
                    if(Target.isTrigger) GUI.color = Color.gray;
                    pressed.left = GUILayout.Button(new GUIContent("Collider", "Set to the \"isTrigger\" property as false."), EditorStyles.miniButtonLeft, GUILayout.Width(w / 2));
                    GUI.color = !Target.isTrigger ? Color.gray : c;
                    pressed.right = GUILayout.Button(new GUIContent("Trigger", "Set to the \"isTrigger\" property as true."), EditorStyles.miniButtonRight, GUILayout.Width(w / 2));
                    GUI.color = c;
                }
                if(pressed.left) {
                    Target.isTrigger = false;
                }
                else if(pressed.right) {
                    Target.isTrigger = true;
                }
            }
            EditorGUILayout.Space();

            DrawColliderEditButton(this);
            Space();

            EditorGUILayout.LabelField(new GUIContent("Shape", "The feature of the shape of colldider."));
            using(new EditorGUI.IndentLevelScope()) {
                this.DrawShapeProperties();
            }

            Space();
            DrawCommonProperties(Target);

        }

        protected private abstract void DrawShapeProperties();

    }
}
