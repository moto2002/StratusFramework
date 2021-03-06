using UnityEngine;
using Stratus;
using UnityEditor;
using Rotorz.ReorderableList;

namespace Stratus
{
  namespace Types
  {
    [CustomPropertyDrawer(typeof(SymbolTable), true)]
    public class SymbolTableDrawer : PropertyDrawer
    {
      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      {
        var symbols = property.FindPropertyRelative("symbols");
        ReorderableListGUI.Title(label);
        ReorderableListGUI.ListField(symbols);        
      }
    } 
  }

}