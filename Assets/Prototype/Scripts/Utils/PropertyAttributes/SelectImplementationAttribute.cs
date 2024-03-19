using System.Collections.Generic;
#if UNITY_EDITOR
using System.Linq;
#endif

namespace RPG.Utils.PropertyAttributes
{
    /// <summary>
    ///     Allows in-editor implementation selection for SerializeReference-d objects.
    ///     Effectively it's kinda like single-use ScriptableObject instances.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class SelectImplementationAttribute : UnityEngine.PropertyAttribute { }

#if UNITY_EDITOR
    /// <remarks>
    ///     Requires derived types' constructors to be parameterless.
    /// </remarks>
    [UnityEditor.CustomPropertyDrawer(typeof(SelectImplementationAttribute))]
    public class SelectImplementationDrawer : UnityEditor.PropertyDrawer
    {
        /// <remarks>
        ///     Source: https://github.com/TextusGames/UnitySerializedReferenceUI/blob/6d0d3d2f3b1864bc7a2d0f0a6875ff86c42b30db/Assets/Textus/SerializeReferenceUI/Core/ManagedReferenceUtility.cs#L13
        /// </remarks>
        public static void SetManagedReferenceValue(object args)
        {
            var (_p, _t) = ((UnityEditor.SerializedProperty p, System.Type t))args;
            _p.serializedObject.Update();
            _p.managedReferenceValue = _t == null ? null : System.Activator.CreateInstance(_t);
            _p.serializedObject.ApplyModifiedProperties();
        }
        /// <remarks>
        ///     Source: https://github.com/Unity-Technologies/UnityCsReference/blob/23b613cc7ab4e7837ca7db09aa8db2ed8ee13a72/Editor/Mono/ScriptAttributeGUI/ScriptAttributeUtility.cs#L283
        /// </remarks>
        private static System.Type GetTypeFromFullTypeName(string managedReferenceFullTypename)
        {
            var parts = managedReferenceFullTypename.Split(' ');
            if (parts.Length == 2)
                return System.Type.GetType($"{parts[1]}, {parts[0]}");
            return null;
        }
        public override float GetPropertyHeight(UnityEditor.SerializedProperty property, UnityEngine.GUIContent label)
        {
            /*
             * This throws "Invalid reference id '0' while trying to get instance object typename."
             * when resizing lists of SerializeProperty-s.
             * Supposedly fixed in 2020.2: https://issuetracker.unity3d.com/issues/invalid-reference-id-0-while-trying-to-get-instance-object-typename-error-when-resizing-array-of-serializereference
             * Not sure if actually affects anything.
             */
            return UnityEditor.EditorGUI.GetPropertyHeight(property, true);
        }
        public override void OnGUI(UnityEngine.Rect position, UnityEditor.SerializedProperty property, UnityEngine.GUIContent label)
        {
            var _interfaceType = GetTypeFromFullTypeName(property.managedReferenceFieldTypename);
            var _dynamicTypeName = property.managedReferenceFullTypename;
            if (_interfaceType != null && !string.IsNullOrEmpty(_dynamicTypeName))
            {
                var _dynamicType = GetTypeFromFullTypeName(_dynamicTypeName);
                if (_dynamicType != null && !_interfaceType.IsAssignableFrom(_dynamicType))
                {
                    /*
                     * Nested [SelectImplementation] PropertyDrawer-s tend to assign 
                     * last known object to new fields, so you can have a default [SelectImplementation] 
                     * field value being a reference to an object of a class that is 
                     * not compatible with the field in question. 
                     * This tries to fix these cases by unsetting the offending reference. 
                     * Note that if newer fields are compatible with the last known
                     * value (such as when growing an array), this does not help, 
                     * and you'll end up with newer values being references to older
                     * instance (so they'll be modified simultaneously).
                     * This needs to be fixed manually via re-initializing the fields
                     * via the context menu implemented below.
                     * TODO: maybe we could override this behaviour somehow? Look at relevant UnityCsReference bits.
                     */
                    SetManagedReferenceValue((property, (System.Type)null));
                    _dynamicTypeName = property.managedReferenceFullTypename;
                }
            }
            UnityEditor.EditorGUI.BeginProperty(position, label, property);
            UnityEditor.EditorGUI.LabelField(
                new UnityEngine.Rect(position) { height = UnityEditor.EditorGUIUtility.singleLineHeight },
                label
            );
            var _indent = UnityEditor.EditorGUI.indentLevel;
            UnityEditor.EditorGUI.indentLevel = 0;
            var _pickerLabel = "- None -";
            if (!string.IsNullOrEmpty(_dynamicTypeName))
                _pickerLabel = _dynamicTypeName.Split(' ')[1].Split('.').Last();
            if (
                UnityEngine.GUI.Button(
                    new UnityEngine.Rect(position) {
                        x = position.x + UnityEditor.EditorGUIUtility.labelWidth + UnityEditor.EditorGUIUtility.standardVerticalSpacing,
                        width = position.width - UnityEditor.EditorGUIUtility.labelWidth - UnityEditor.EditorGUIUtility.standardVerticalSpacing,
                        height = UnityEditor.EditorGUIUtility.singleLineHeight
                    },
                    new UnityEngine.GUIContent(_pickerLabel, _dynamicTypeName)
                )
            ) {
                var _pickerMenu = new UnityEditor.GenericMenu();
                _pickerMenu.AddItem(
                    new UnityEngine.GUIContent(_interfaceType != null ? "- None -" : "- Unresolved Type! -"),
                    false,
                    SetManagedReferenceValue,
                    (property, (System.Type)null));
                if (_interfaceType != null) foreach (
                    var _t in System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(
                        assembly => assembly.GetTypes()
                    ).Where( // rules lifted from https://github.com/TextusGames/UnitySerializedReferenceUI/blob/6d0d3d2f3b1864bc7a2d0f0a6875ff86c42b30db/Assets/Textus/SerializeReferenceUI/Core/ManagedReferenceUtility.cs#L48
                        t => _interfaceType.IsAssignableFrom(t)
                        // Skip abstract classes because they should not be instantiated
                        && !t.IsAbstract
                        // Skip generic classes because they can not be instantiated
                        && !t.ContainsGenericParameters
                        // Skip types that has no public empty constructors (activator can not create them)
                        && !(
                            t.IsClass // Structs still can be created
                            && t.GetConstructor(System.Type.EmptyTypes) == null
                        )
                        // Skips unity engine Objects (because they are not serialized by SerializeReference)
                        && !t.IsSubclassOf(typeof(UnityEngine.Object))
                    ).ToArray()
                ) {
                    _pickerMenu.AddItem(new UnityEngine.GUIContent(_t.ToString()), false, SetManagedReferenceValue, (property, _t));
                }
                _pickerMenu.ShowAsContext();
            }
            UnityEditor.EditorGUI.indentLevel = _indent;
            UnityEditor.EditorGUI.PropertyField(position, property, UnityEngine.GUIContent.none, true);
            UnityEditor.EditorGUI.EndProperty();
        }
    }
    #endif
}