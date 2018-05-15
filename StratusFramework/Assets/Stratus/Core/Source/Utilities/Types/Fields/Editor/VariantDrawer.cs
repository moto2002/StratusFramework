using UnityEngine;
using UnityEditor;

namespace Stratus
{
  namespace Types
  {
    [CustomPropertyDrawer(typeof(Variant))]
    public class VariantDrawer : PropertyDrawer
    {
      public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
      {
        return EditorGUIUtility.singleLineHeight;
      }

      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      {
        var typeProperty = property.FindPropertyRelative("type");
        var type = (Variant.Types)typeProperty.enumValueIndex;

        label = EditorGUI.BeginProperty(position, label, property);
        Rect contentPosition = EditorGUI.PrefixLabel(position, label);
        var width = contentPosition.width;
        EditorGUI.indentLevel = 0;

        // Type
        contentPosition.width = width * 0.30f;
        EditorGUI.PropertyField(contentPosition, typeProperty, GUIContent.none);
        contentPosition.x += contentPosition.width + 4f;

        // Value
        contentPosition.width = width * 0.70f;
        string valueName = string.Empty;
        switch (type)
        {
          case Variant.Types.Integer:
            valueName = "integerValue";
            break;
          case Variant.Types.Boolean:
            valueName = "booleanValue";
            break;
          case Variant.Types.Float:
            valueName = "floatValue";
            break;
          case Variant.Types.String:
            valueName = "stringValue";
            break;
          case Variant.Types.Vector3:
            valueName = "vector3Value";
            break;
          default:
            break;
        }
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative(valueName), GUIContent.none);
        
        EditorGUI.EndProperty();

        if (GUI.changed)
          property.serializedObject.ApplyModifiedProperties();

      }

    } 
  }

}