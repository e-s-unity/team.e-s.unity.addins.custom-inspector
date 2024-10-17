#nullable enable

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Drawing.Drawing2D;


#if UNITY_EDITOR

using UnityEditor;

namespace Es.Unity.Addins.CustomInspectors
{
    /// <summary>
    /// 
    /// </summary>
    [UnityEditor.CustomEditor(typeof(UnityEngine.Rigidbody))]
    public class RigidbodyInspector : UnityEditor.Editor 
    {

        public override bool RequiresConstantRepaint() => true;

        protected UnityEngine.Rigidbody This => (UnityEngine.Rigidbody)base.target;

        static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3) {
            float v321 = p3.x * p2.y * p1.z;
            float v231 = p2.x * p3.y * p1.z;
            float v312 = p3.x * p1.y * p2.z;
            float v132 = p1.x * p3.y * p2.z;
            float v213 = p2.x * p1.y * p3.z;
            float v123 = p1.x * p2.y * p3.z;
            return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
        }

        static float VolumeOfMesh(Mesh mesh) {
            float volume = 0;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            for(int i = 0; i < mesh.triangles.Length; i += 3) {
                Vector3 p1 = vertices[triangles[i + 0]];
                Vector3 p2 = vertices[triangles[i + 1]];
                Vector3 p3 = vertices[triangles[i + 2]];
                volume += SignedVolumeOfTriangle(p1, p2, p3);
            }
            return Mathf.Abs(volume);
        }

        private Mesh? Mesh {
            get {
                var filter = this.This.gameObject.GetComponent<MeshFilter>();
                if(filter != null) {
                    return filter.sharedMesh;
                }
                else {
                    return null;
                }
            }
        }

        /// <summary>
        /// [m^3]
        /// </summary>
        protected float Volume {
            get => this.Mesh != null ? VolumeOfMesh(this.Mesh) : 1;
        }

        /// <summary>
        /// [kg]
        /// </summary>
        protected float Mass {
            get {
                return This.mass;
            }
            set {
                float kg = value;
                if(!float.IsNaN(kg) && !float.IsInfinity(kg) && kg >= 0.0f) {
                    This.mass = kg;
                }
                else {
                    Debug.LogWarning($"Invalid mass value \"{kg}\".");
                }
            }
        }

        /// <summary>
        /// [kg/m^3]
        /// </summary>
        public float InternalDensity {
            get {
                if(float.IsNaN(this.Volume) || this.Volume == 0) {
                    return 0;
                }
                else {
                    return this.Mass / this.Volume;
                }
            }

            internal set {
                if(float.IsNaN(value)) {
                    this.Mass = 0;
                }
                else {
                    if(this.Volume > 0) {
                        this.Mass = value * this.Volume;
                    }
                }
            }
        }

        /// <summary>
        /// [g/cm^3]
        /// </summary>
        public float Density {
            get {
                if(this.Volume > 0) {
                    return this.InternalDensity / 1000.0f;
                }
                else {
                    return 1000;
                }
            }
            set {
                if(this.Volume > 0) {
                    float write = value * 1000.0f;
                    if(!float.IsNaN(write)) {
                        this.InternalDensity = write;
                    }
                }
                else {
                    this.InternalDensity = 1000;
                }
            }
        }


        private static readonly GUIContent _isKinematicContent = new("Is Kinematic", "Whether the object is ignoring any physics effects, a gravity or external enforcements etc.");
        private static readonly GUIContent _gravityContent = new("Gravity", "Whether the rigidbody is affected gravity.");
        private static readonly GUIContent _massContent = new("Mass", "The mass of the rigidbody.");

        private static int IndentLevel { get => EditorGUI.indentLevel; set => EditorGUI.indentLevel = value; }
        private static float LabelWidth { get => EditorGUIUtility.labelWidth; set => EditorGUIUtility.labelWidth = value; }

        private static float ViewWidth => EditorGUIUtility.currentViewWidth - 20;

        private static bool _Foldout_Translation = false;
        private static bool _Foldout_Rotation = false;

        private static (Vector3 Velocity, float Drag) DrawVelocityAndDrag(GUIContent content, Vector3 velocity, float drag) {
            (Vector3, float) @return = (velocity, drag);
            using(new EditorGUILayout.HorizontalScope()) {
                using(new EditorGUI.IndentLevelScope(-IndentLevel)) {
                    @return.Item1 = EditorGUILayout.Vector3Field(content, velocity, GUILayout.Width(ViewWidth - 100));
                    EditorGUILayout.LabelField(new GUIContent("Drag"), GUILayout.Width(20));
                    @return.Item2 = EditorGUILayout.FloatField(GUIContent.none, drag, GUILayout.Width(80));
                }
            }
            return @return;
        }

        public override void OnInspectorGUI() {

            /* Physics Mode */
#if false
            This.isKinematic = EditorGUILayout.Toggle(_isKinematicContent, This.isKinematic);
#else
            {
                float w = EditorGUIUtility.currentViewWidth - 20;
                Color c = GUI.color;
                (bool left, bool right) pressed = (false, false);
                using(new EditorGUILayout.HorizontalScope()) {
                    if(This.isKinematic) GUI.color = Color.gray;
                    pressed.left = GUILayout.Button(new GUIContent("Physic", "Set to the \"isKinematic\" property as false."), EditorStyles.miniButtonLeft, GUILayout.Width(w / 2));
                    GUI.color = !This.isKinematic ? Color.gray : c;
                    pressed.right = GUILayout.Button(new GUIContent("Kinematic", "Set to the \"isKinematic\" property as true."), EditorStyles.miniButtonRight, GUILayout.Width(w / 2));
                    GUI.color = c;
                }
                if(pressed.left) {
                    This.isKinematic = false;
                }
                else if(pressed.right) {
                    This.isKinematic = true;
                }
            }
            EditorGUILayout.Space();
#endif

            if(!This.isKinematic) {
                This.useGravity = EditorGUILayout.Toggle(_gravityContent, This.useGravity);
            }
            else {
                using(new EditorGUI.DisabledScope(This.isKinematic)) {
                    EditorGUILayout.Toggle(_gravityContent, false);
                }
            }
            EditorGUILayout.Space();

            
            This.detectCollisions = EditorGUILayout.Toggle(new GUIContent("Detect Collisions"), This.detectCollisions);
            if(This.detectCollisions) {
                using(new EditorGUI.IndentLevelScope()) {
                    This.collisionDetectionMode = (CollisionDetectionMode)EditorGUILayout.EnumPopup(new GUIContent("Detection Mode"), This.collisionDetectionMode);
                }
            }
            EditorGUILayout.Space();


            if(This.isKinematic) return; // All of properties under following, available only when the rigidbody is non-isKinematic = Physic.

            /* Collision Mode */
            This.interpolation = (RigidbodyInterpolation)EditorGUILayout.EnumPopup(new GUIContent("Interpolation"), This.interpolation);
            EditorGUILayout.Space();

            this.Mass = EditorGUILayout.DelayedFloatField(_massContent, this.Mass);
            if(this.Mass > 0) {
                using(new EditorGUI.IndentLevelScope()) {
                    This.centerOfMass = EditorGUILayout.Vector3Field(new GUIContent("Centroid Offset", "The center of mass."), This.centerOfMass);
                }
            }

            EditorGUILayout.Space();
            if(_Foldout_Translation = EditorGUILayout.Foldout(_Foldout_Translation, new GUIContent("Translation"))) {
                using(new EditorGUI.IndentLevelScope()) {
#if true
                    using(new EditorGUI.DisabledScope(This.isKinematic)) {
                        Vector3 lv = This.velocity;
                        using(var check = new EditorGUI.ChangeCheckScope()) {
                            lv.x = (float)Math.Round(lv.x, 4);
                            lv.y = (float)Math.Round(lv.y, 4);
                            lv.z = (float)Math.Round(lv.z, 4);
                            lv = EditorGUILayout.Vector3Field(new GUIContent("Linear Velocity"), lv);
                            if(check.changed && !This.isKinematic) This.velocity = lv;
                        }
                    }
                    This.drag = EditorGUILayout.FloatField(new GUIContent("Linear Drag"), This.drag);
#else
                    var linear = DrawVelocityAndDrag(new GUIContent("Linear Velocity"), This.velocity, This.drag);
                    This.velocity = linear.Velocity;
                    This.drag = linear.Drag;
#endif

                    // Constraint (Translate)
                    RigidbodyConstraintsSetting temp = new(This.constraints);

                    int storedIndent = IndentLevel;
                    using(new EditorGUILayout.HorizontalScope()) {
                        EditorGUILayout.LabelField(new GUIContent("Freeze"), GUILayout.Width(LabelWidth));
                        using(new EditorGUI.IndentLevelScope(-IndentLevel)) {
                            EditorGUILayout.LabelField("X", GUILayout.Width(20));
                            temp.X = EditorGUILayout.Toggle(GUIContent.none, temp.X, GUILayout.Width(40));
                            EditorGUILayout.LabelField("Y", GUILayout.Width(20));
                            temp.Y = EditorGUILayout.Toggle(GUIContent.none, temp.Y, GUILayout.Width(40));
                            EditorGUILayout.LabelField("Z", GUILayout.Width(20));
                            temp.Z = EditorGUILayout.Toggle(GUIContent.none, temp.Z, GUILayout.Width(40));
                        }
                    }
                    IndentLevel = storedIndent;

                    This.constraints = temp.ToEnum();
                    base.serializedObject.ApplyModifiedProperties();

                }
            }
            EditorGUILayout.Space();
            IndentLevel = 0;
            if(_Foldout_Rotation = EditorGUILayout.Foldout(_Foldout_Rotation, "Rotation")) {
                using(new EditorGUI.IndentLevelScope()) {
                    using(new EditorGUI.DisabledScope(This.isKinematic)) {
                        Vector3 after_av = EditorGUILayout.Vector3Field(new GUIContent("Angular Velocity"), This.angularVelocity);
                        if(!This.isKinematic) This.angularVelocity = after_av;
                    }
                    This.angularDrag = EditorGUILayout.FloatField(new GUIContent("Angular Drag"), This.angularDrag);

                    // Constraint (Translate)
                    RigidbodyConstraintsSetting temp_rot = new(This.constraints);

                    int storedIndent = IndentLevel;
                    using(new EditorGUILayout.HorizontalScope()) {
                        EditorGUILayout.LabelField(new GUIContent("Freeze"), GUILayout.Width(LabelWidth));
                        using(new EditorGUI.IndentLevelScope(-IndentLevel)) {
                            EditorGUILayout.LabelField("X", GUILayout.Width(20));
                            temp_rot.rX = EditorGUILayout.Toggle(GUIContent.none, temp_rot.rX, GUILayout.Width(40));
                            EditorGUILayout.LabelField("Y", GUILayout.Width(20));
                            temp_rot.rY = EditorGUILayout.Toggle(GUIContent.none, temp_rot.rY, GUILayout.Width(40));
                            EditorGUILayout.LabelField("Z", GUILayout.Width(20));
                            temp_rot.rZ = EditorGUILayout.Toggle(GUIContent.none, temp_rot.rZ, GUILayout.Width(40));
                        }
                    }
                    IndentLevel = storedIndent;

                    This.constraints = temp_rot.ToEnum();
                    base.serializedObject.ApplyModifiedProperties();

                    if(this.Mass > 0) {
                        This.automaticInertiaTensor = EditorGUILayout.Toggle(new GUIContent("Automatic Inertia Tensor"), This.automaticInertiaTensor);
                        if(!This.automaticInertiaTensor) {
                            using(new EditorGUI.IndentLevelScope()) {
                                This.inertiaTensor = EditorGUILayout.Vector3Field(new GUIContent("Inertia Tensor"), This.inertiaTensor);
                            }
                        }
                    }
                }
            }


        }


    }


    public class RigidbodyConstraintsSetting
    {
        private RigidbodyConstraints _Flags = RigidbodyConstraints.None;


        /// <summary>
        /// Instantizes a new instance of the <see cref="RigidbodyConstraintsSetting"/> class.
        /// </summary>
        public RigidbodyConstraintsSetting(RigidbodyConstraints flags) {
            _Flags = flags;
        }

        public RigidbodyConstraints ToEnum() => _Flags;

        public bool All {
            get => _Flags == RigidbodyConstraints.FreezeAll;
            set => _Flags = value ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
        }

        public bool Rotation {
            get => _Flags.HasFlag(RigidbodyConstraints.FreezeRotation);
            set {
                if(value) {
                    _Flags |= RigidbodyConstraints.FreezeRotation;
                }
                else {
                    _Flags &= ~RigidbodyConstraints.FreezeRotation;
                }
            }
        }

        public bool Position {
            get => _Flags.HasFlag(RigidbodyConstraints.FreezePosition);
            set {
                if(value) {
                    _Flags |= RigidbodyConstraints.FreezePosition;
                }
                else {
                    _Flags &= ~RigidbodyConstraints.FreezePosition;
                }
            }
        }

        public bool X {
            get => _Flags.HasFlag(RigidbodyConstraints.FreezePositionX);
            set => _Set(value, RigidbodyConstraints.FreezePositionX);
        }

        public bool Y {
            get => _Flags.HasFlag(RigidbodyConstraints.FreezePositionY);
            set => _Set(value, RigidbodyConstraints.FreezePositionY);
        }

        public bool Z {
            get => _Flags.HasFlag(RigidbodyConstraints.FreezePositionZ);
            set => _Set(value, RigidbodyConstraints.FreezePositionZ);
        }


        public bool rX {
            get => _Flags.HasFlag(RigidbodyConstraints.FreezeRotationX);
            set => _Set(value, RigidbodyConstraints.FreezeRotationX);
        }

        public bool rY {
            get => _Flags.HasFlag(RigidbodyConstraints.FreezeRotationY);
            set => _Set(value, RigidbodyConstraints.FreezeRotationY);
        }

        public bool rZ {
            get => _Flags.HasFlag(RigidbodyConstraints.FreezeRotationZ);
            set => _Set(value, RigidbodyConstraints.FreezeRotationZ);
        }

        private void _Set(bool Includes, RigidbodyConstraints target_s) {
            if(Includes) {
                _Flags = _Include(_Flags, target_s);
            }
            else {
                _Flags = _Exclude(_Flags, target_s);
            }
        }

        private static RigidbodyConstraints _Exclude(RigidbodyConstraints @base, RigidbodyConstraints target_s) {
            @base &= ~target_s;
            return @base;
        }

        private static RigidbodyConstraints _Include(RigidbodyConstraints @base, RigidbodyConstraints target_s) {
            @base |= target_s;
            return @base;
        }



    }

}

#endif
