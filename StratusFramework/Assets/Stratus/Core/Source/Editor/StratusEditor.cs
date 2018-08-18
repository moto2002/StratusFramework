using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using Rotorz.ReorderableList;
using UnityEditorInternal;
using Stratus.Interfaces;
using System.Linq;
using UnityEngine.Serialization;

namespace Stratus
{
  /// <summary>
  /// Base editor for all Stratus components.
  /// </summary>
  public abstract partial class StratusEditor : UnityEditor.Editor
  {
    //------------------------------------------------------------------------/
    // Declarations
    //------------------------------------------------------------------------/
    /// <summary>
    /// Maps serialized properties to validating functions
    /// </summary>
    public class PropertyConstraintMap : Dictionary<SerializedProperty, Func<bool>> { }
    /// <summary>
    /// Used to encapsulate the drawing of a particular group
    /// </summary>
    public class DrawGroupRequest
    {
      public string label;
      public System.Action<Rect> onDraw;
      public Func<bool> onValidate;

      public DrawGroupRequest(System.Action<Rect> drawFunction, Func<bool> validateFunction = null)
      {
        this.onDraw = drawFunction;
        this.onValidate = validateFunction;
      }

      public bool isValid => onValidate != null ? onValidate() : true;
    }

    //------------------------------------------------------------------------/
    // Properties
    //------------------------------------------------------------------------/
    /// <summary>
    /// Specific constraints that dictate whether a specified property should be displayed
    /// </summary>
    public PropertyConstraintMap propertyConstraints { get; set; } = new PropertyConstraintMap();
    /// <summary>
    /// Provided callbacks for when specific properties are changed
    /// </summary>
    public Dictionary<SerializedProperty, System.Action> propertyChangeCallbacks { get; set; } = new Dictionary<SerializedProperty, System.Action>();
    /// <summary>
    /// Overrides for drawing specific properties from specific types
    /// </summary>
    public Dictionary<SerializedProperty, System.Action<SerializedProperty>> propertyDrawOverrides { get; set; } = new Dictionary<SerializedProperty, Action<SerializedProperty>>();
    /// <summary>
    /// A map of all available properties by name
    /// </summary>
    public Dictionary<string, SerializedProperty> propertyMap { get; set; } = new Dictionary<string, SerializedProperty>();
    /// <summary>
    /// The custom attributes for this property
    /// </summary>
    public Dictionary<SerializedProperty, Attribute[]> propertyAttributes { get; private set; } = new Dictionary<SerializedProperty, Attribute[]>();
    /// <summary>
    /// The custom attributes for this property
    /// </summary>
    public Dictionary<SerializedProperty, Dictionary<Type, Attribute>> propertyAttributesMap { get; private set; } = new Dictionary<SerializedProperty, Dictionary<Type, Attribute>>();
    /// <summary>
    /// The set of properties of the most-derived class
    /// </summary>
    public Tuple<Type, SerializedProperty[]> declaredProperties => propertyGroups.Last();
    /// <summary>
    /// A map of all property groups by the type
    /// </summary>
    public Dictionary<Type, SerializedProperty[]> propertiesByType { get; set; } = new Dictionary<Type, SerializedProperty[]>();
    /// <summary>
    /// A list of all different property groups, starting from the base class to the most-derived class
    /// </summary>
    public List<Tuple<Type, SerializedProperty[]>> propertyGroups { get; set; } = new List<Tuple<Type, SerializedProperty[]>>();
    /// <summary>
    /// Whether to draw labels for types above property groups
    /// </summary>
    public virtual bool drawTypeLabels => false;
    /// <summary>
    /// A collection of all registered lists to be drawn with reoderable within this editor
    /// </summary>
    protected Dictionary<SerializedProperty, ReorderableList> reorderableLists { get; set; } = new Dictionary<SerializedProperty, ReorderableList>();
    /// <summary>
    /// Custom draw functions to be invoked after property drawing
    /// </summary>
    protected List<DrawGroupRequest> drawGroupRequests { get; set; } = new List<DrawGroupRequest>();
    /// <summary>
    /// The default label style for headers
    /// </summary>
    public GUIStyle labelStyle { get; set; }
    /// <summary>
    /// The default background style used
    /// </summary>
    public GUIStyle backgroundStyle { get; set; }
    /// <summary>
    /// The default style used for each section
    /// </summary>
    public GUIStyle sectionStyle { get; set; }
    /// <summary>
    /// Whether any custom GUI styles have been configured
    /// </summary>
    private bool doneFirstUpdate { get; set; }
    /// <summary>
    /// Whethe the default styles are being overwritten
    /// </summary>
    public bool overrideStyles { get; set; } = false;
    /// <summary>
    /// The current GUI event in Unity
    /// </summary>
    protected UnityEngine.Event currentEvent => UnityEngine.Event.current;
    /// <summary>
    /// Whether to use reorderable lists for drawing arrays and lists
    /// </summary>
    public bool drawReorderableLists { get; set; } = true;
    /// <summary>
    /// Any requests to be processed at the end of this frame
    /// </summary>
    protected List<System.Action> endOfFrameRequests { get; private set; } = new List<System.Action>();
    /// <summary>
    /// A list of all messages added
    /// </summary>
    public List<Validation> messages { get; private set; } = new List<Validation>();
    //public System.Action onEnteredEditMode { get; protected set; }
    //public System.Action onExitingEditMode { get; protected set; }
    //public System.Action onEnteredPlayMode { get; protected set; }
    //public System.Action onExitingdPlayMode { get; protected set; }
    /// <summary>
    /// Whether there are messages to be shown
    /// </summary>
    public bool hasMessages => messages.NotEmpty();
    /// <summary>
    /// The base type for the component type this editor is for. It marks the stopping point to look at properties.
    /// </summary>
    protected abstract Type baseType { get; }
    /// <summary>
    /// The number of properties drawn by the base editor GUI
    /// </summary>
    protected int drawnProperties { get; set; }
    /// <summary>
    /// Whether the target component is still valid
    /// </summary>
    protected bool isTargetValid => !target.Destroyed();

    

    //------------------------------------------------------------------------/
    // Virtual Methods
    //------------------------------------------------------------------------/
    protected abstract void OnStratusEditorEnable();
    protected virtual void OnStratusEditorDisable() {}
    protected virtual void OnFirstUpdate() {}
    internal abstract void OnStratusGenericEditorEnable();
    internal abstract void OnGenericStratusEditorValidate();

    //------------------------------------------------------------------------/
    // Messages
    //------------------------------------------------------------------------/
    private void OnEnable()
    {
      if (target == null)
      {
        return;
      }

      InitializeBaseEditor();
      EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
      EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

      if (isTargetValid)
        this.OnStratusEditorDisable();
    }

    protected virtual void OnPlayModeStateChanged(PlayModeStateChange stateChange)
    {
      //switch (stateChange)
      //{
      //  case PlayModeStateChange.EnteredEditMode:
      //    onEnteredEditMode?.Invoke();
      //    break;
      //
      //  case PlayModeStateChange.ExitingEditMode:
      //    onExitingEditMode?.Invoke();
      //    break;
      //
      //  case PlayModeStateChange.EnteredPlayMode:
      //    onEnteredPlayMode?.Invoke();
      //    break;
      //
      //  case PlayModeStateChange.ExitingPlayMode:
      //    onExitingdPlayMode?.Invoke();
      //    break;
      //}
    }

    public void InitializeBaseEditor()
    {
      AddProperties();
      OnStratusGenericEditorEnable();
      OnStratusEditorEnable();
    }

    public override void OnInspectorGUI()
    {
      StratusGUIStyles.OverrideDefaultFont();

      // Invoke the very first time
      if (!doneFirstUpdate)
        DoFirstUpdate();

      // Now fulfill any custom requests at the end of inspection
      ProcessEndOfFrameRequests();

      // Update the serialized object, saving data
      serializedObject.Update();

      // Show any messages, if present
      if (messages.NotEmpty())
        DrawMessages();

      // Now draw the base editor
      if (drawnProperties > 0)
        OnBaseEditorGUI();

      // Now draw any custom draw functions
      if (drawGroupRequests.Count > 0)
      {
        foreach (var drawRequest in drawGroupRequests)
        {
          if (drawRequest.isValid)
          {
            Rect rect = EditorGUILayout.BeginVertical(backgroundStyle);
            drawRequest.onDraw(rect);
            EditorGUILayout.EndVertical();
          }
        }
      }

      StratusGUIStyles.RevertDefaultFont();
    }

    public virtual void OnBaseEditorGUI()
    {
      bool propertiesChanged = false;

      // Reverse order: Draw all the types up until the most-derived
      for (int i = 0; i < propertyGroups.Count - 1; i++)
      {
        var properties = propertyGroups[i];

        // If there's no properties for this type
        if (properties.Item2.Length < 1)
          continue;

        // If all properties fail the constraints check
        if (!ValidateConstraints(properties.Item2))
          continue;


        if (drawTypeLabels)
          EditorGUILayout.LabelField(properties.Item1.Name, StratusGUIStyles.headerWhite);

        EditorGUILayout.BeginVertical(backgroundStyle);
        propertiesChanged |= DrawProperties(properties.Item2);
        EditorGUILayout.EndVertical();
      }

      // Now draw the declared properties
      if (declaredProperties.Item2.Length > 0)
      {
        if (drawTypeLabels)
          EditorGUILayout.LabelField(declaredProperties.Item1.Name, StratusGUIStyles.headerWhite);
        EditorGUILayout.BeginVertical(backgroundStyle);
        propertiesChanged |= DrawDeclaredProperties();
        EditorGUILayout.EndVertical();
      }

      // If any properties changed, inform!
      if (propertiesChanged && !(target == null || target.Equals(null)))
        OnGenericStratusEditorValidate();
    }

    public virtual void DrawMessages()
    {
      foreach (var message in messages)
      {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.HelpBox(message.message, message.type.Convert());
        if (message.hasContext)
        {
          StratusEditorUtility.OnMouseClick(GUILayoutUtility.GetLastRect(), null, () =>
          {
            var menu = new GenericMenu();

            if (message.onSelect != null)
              menu.AddItem(new GUIContent("Select"), false, () => { message.onSelect(); });
            if (message.onValidate == null)
              menu.AddItem(new GUIContent("Remove"), false, () => RemoveMessage(message));

            menu.ShowAsContext();
          });
        }
        EditorGUILayout.EndHorizontal();
      }
    }

    private void ProcessEndOfFrameRequests()
    {
      if (endOfFrameRequests.NotEmpty())
      {
        foreach (var request in endOfFrameRequests)
          request.Invoke();
        endOfFrameRequests.Clear();
      }
    }

    private void RemoveMessage(Validation message)
    {
      endOfFrameRequests.Add(() => { messages.Remove(message); });
    }

    //------------------------------------------------------------------------/
    // Helpers
    //------------------------------------------------------------------------/
    /// <summary>
    /// Gets all the serialized property for the given Unity Object of a specified type
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static SerializedProperty[] GetSerializedProperties(SerializedObject serializedObject, Type type)
    {
      FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
      List<SerializedProperty> properties = new List<SerializedProperty>();

      for (int i = 0; i < fields.Length; i++)
      {
        FieldInfo field = fields[i];
        if (field != null && (field.Attributes != FieldAttributes.NotSerialized))
        {
          //Trace.Script($"- {field.Name}");
          SerializedProperty property = serializedObject.FindProperty(field.Name);
          properties.Add(property);
        }
      }

      return properties.ToArray(); ;
    }

    public static SerializedProperty[] GetSerializedProperties(SerializedObject serializedObject)
    {
      List<SerializedProperty> properties = new List<SerializedProperty>();
      //Trace.Script($"Properties for {serializedObject.targetObject.GetType().Name}");
      SerializedProperty iter = serializedObject.GetIterator();
      if (iter != null)
      {
        // THe first property is m_Script
        iter.NextVisible(true);
        //properties.Add(iter);
        //Trace.Script($"- {iter.name}");

        while (iter.NextVisible(false))
        {
          properties.Add(iter);
          //Trace.Script($"- {iter.name}");
        }
      }
      iter.Reset();
      return properties.ToArray();
    }

    /// <summary>
    /// Draws a serialized property, saving any recorded changes
    /// </summary>
    /// <param name="property"></param>
    /// <returns>True if the property changed, false otherwise.</returns>

    public bool DrawSerializedProperty(SerializedProperty property, SerializedObject serializedObject)
    {
      EditorGUI.BeginChangeCheck();

      // If this property has been requested to be drawn in a custom manner
      if (propertyDrawOverrides.ContainsKey(property))
      {
        propertyDrawOverrides[property].Invoke(property);
      }
      else
      {
        // Arrays
        if (property.isArray && property.propertyType != SerializedPropertyType.String && drawReorderableLists)
          DrawReorderableList(property, property.displayName);
        // Use normal drawers
        else
        {
          bool overridden = false;
          if (!overridden)
            EditorGUILayout.PropertyField(property, true);
        }
      }

      // If property was changed, save
      if (EditorGUI.EndChangeCheck())
      {
        // Record change
        Undo.RecordObject(target, property.name);

        // Apply the modified property
        serializedObject.ApplyModifiedProperties();

        // Inform that this property has been changed
        if (propertyChangeCallbacks.ContainsKey(property))
          propertyChangeCallbacks[property].Invoke();
        return true;
      }

      return false;
    }

    /// <summary>
    /// Draws a serialized property, saving any recorded changes
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    public bool DrawSerializedProperty(SerializedProperty prop)
    {
      return DrawSerializedProperty(prop, serializedObject);
    }

    /// <summary>
    /// Draws a serialized property, saving any recorded changes
    /// </summary>
    /// <param name="prop"></param>
    /// <returns>True if the property changed, false if it was not drawn or found.</returns>
    public bool DrawSerializedProperty(string propertyName)
    {
      if (!propertyMap.ContainsKey(propertyName))
        return false;

      return DrawSerializedProperty(propertyMap[propertyName], serializedObject);
    }

    /// <summary>
    /// Draws the given array type using a reorderable list drawer
    /// </summary>
    /// <param name="property"></param>
    /// <param name="label"></param>
    public static void DrawReorderableList(SerializedProperty property, string label)
    {
      ReorderableListGUI.Title(label);
      ReorderableListGUI.ListField(property);
    }

    /// <summary>
    /// Draws the given array type using a reorderable list drawer
    /// </summary>
    /// <param name="property"></param>
    /// <param name="label"></param>
    public static void DrawReorderableList<T>(List<T> list, string label, ReorderableListControl.ItemDrawer<T> drawItem)
    {
      ReorderableListGUI.Title(label);
      ReorderableListGUI.ListField(list, drawItem);
    }

    /// <summary>
    /// Draws the given array type using a reorderable list drawer
    /// </summary>
    /// <param name="property"></param>
    /// <param name="label"></param>
    public static void DrawReorderableList<T>(List<T> list, ReorderableListControl.ItemDrawer<T> drawItem)
    {
      ReorderableListGUI.ListField(list, drawItem);
    }

    public static ReorderableList GetListWithFoldout(SerializedObject serializedObject, SerializedProperty property, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton)
    {
      var list = new ReorderableList(serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton);

      list.drawHeaderCallback = (Rect rect) =>
      {
        var newRect = new Rect(rect.x + 10, rect.y, rect.width - 10, rect.height);
        property.isExpanded = EditorGUI.Foldout(newRect, property.isExpanded, property.displayName);
      };
      list.drawElementCallback =
        (Rect rect, int index, bool isActive, bool isFocused) =>
        {
          if (!property.isExpanded)
          {
            GUI.enabled = index == list.count;
            return;
          }

          var element = list.serializedProperty.GetArrayElementAtIndex(index);
          rect.y += 2;
          EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        };
      list.elementHeightCallback = (int indexer) =>
      {
        if (!property.isExpanded)
          return 0;
        else
          return list.elementHeight;
      };

      return list;
    }

    //------------------------------------------------------------------------/
    // Procedures
    //------------------------------------------------------------------------/
    private void DoFirstUpdate()
    {
      if (backgroundStyle == null)
        backgroundStyle = StratusGUIStyles.background;
      if (labelStyle == null)
        labelStyle = StratusGUIStyles.skin.label;

      OnFirstUpdate();
      doneFirstUpdate = true;
    }

    protected virtual bool DrawDeclaredProperties()
    {
      return DrawProperties(declaredProperties.Item2);
    }

    /// <summary>
    /// Draws the given set of properties according to any present constraints in the editor
    /// </summary>
    /// <param name="properties"></param>
    private bool DrawProperties(SerializedProperty[] properties)
    {
      bool propertiesChanged = false;
      foreach (var prop in properties)
      {
        bool hasConstraint = propertyConstraints.ContainsKey(prop);
        if (hasConstraint)
        {
          bool canBeDrawn = propertyConstraints[prop].Invoke();
          if (!canBeDrawn)
            continue;
        }

        propertiesChanged |= DrawSerializedProperty(prop, serializedObject);
      }
      return propertiesChanged;
    }

    /// <summary>
    /// Adds all SerializedProperties to be inspected
    /// </summary>
    internal void AddProperties()
    {
      // Add all properties to the map
      //SerializedProperty[] allProperties = GetSerializedProperties(serializedObject);
      //foreach (var property in allProperties)
      //{
      //  if (!propertyMap.ContainsKey(property.name))
      //    propertyMap.Add(property.name, property);
      //}

      // Reset the number of drawn properties
      this.drawnProperties = 0;

      // For every type, starting from the most derived up to the base, get its serialized properties      
      Type declaredType = target.GetType();
      Type currentType = declaredType;
      Type previousType = null;
      //SerializedProperty[] properties = null;

      while (currentType != baseType)
      {
        //Trace.Script($"Adding properties for {currentType.Name}");
        // Add the properties onto the map
        // If the previous type was generic, add those properties onto the instantiated type
        SerializedProperty[] properties = GetSerializedProperties(serializedObject, currentType);
        //bool isGeneric = currentType.IsGenericType;
        //FieldInfo[] objectFields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var property in properties)
        {
          // Record this property
          if (property == null)
            Debug.LogError($"A property was found to not be serialized properly while inspecting {target.name}. Did you forget a [Serializable] attribute on a class definition?");

          propertyMap.Add(property.name, property);

          // Record the attributes for this property
          Attribute[] attributes = property.GetFieldAttributes();
          propertyAttributes.Add(property, attributes);
          propertyAttributesMap.AddIfMissing(property, new Dictionary<Type, Attribute>());
          foreach (var attr in attributes)
          {
            propertyAttributesMap[property].AddIfMissing(attr.GetType(), attr);
            //Trace.Script($"{property.displayName} has the attribute '{attr.GetType().Name}'");
          }
          OnPropertyAttributesAdded(property);

          // Check whether this property is an array
          if (property.isArray && property.propertyType != SerializedPropertyType.String)
          {
            ReorderableList list = GetListWithFoldout(serializedObject, property, true, true, true, true);
            reorderableLists.Add(property, list);
          }
        }

        // Add all the properties for this type into the property map by type        
        if (!currentType.IsGenericType)
        {
          propertiesByType.Add(currentType, properties);
          propertyGroups.Add(new Tuple<Type, SerializedProperty[]>(currentType, properties));
        }
        else
        {
          SerializedProperty[] joinedProperties = propertiesByType[previousType].Concat(properties);
          var lastGroup = propertyGroups.Last();
          propertyGroups.RemoveLast();

          propertyGroups.Add(new Tuple<Type, SerializedProperty[]>(previousType, joinedProperties));
          propertiesByType[previousType] = joinedProperties;

        }

        // Move on to the parent type (if any)
        previousType = currentType;
        currentType = currentType.BaseType;
      }
      

      propertyGroups.Reverse();
    }



    /// <summary>
    /// Checks whether all the properties are under constraints. Returns false if none
    /// of the properties can be drawn.
    /// </summary>
    /// <param name="properties"></param>
    /// <returns></returns>
    private bool ValidateConstraints(SerializedProperty[] properties)
    {
      foreach (var prop in properties)
      {
        bool foundConstraint = propertyConstraints.ContainsKey(prop);

        // If no constraint was found for this property, it means 
        // that at least one property can be drawn
        if (!foundConstraint)
          return true;
        // If the property was found and validated, it means we can draw it
        else
        {
          bool validated = propertyConstraints[prop]();
          if (validated)
            return true;
        }
      }

      // No constraints were validated
      return false;
    }

    protected void DrawEditor(UnityEditor.Editor editor, string header, int headerSize = 12)
    {      
      EditorGUILayout.Space();
      EditorGUILayout.BeginHorizontal();
      {        
        EditorGUILayout.InspectorTitlebar(false, editor.target, false);
      }
      EditorGUILayout.EndHorizontal();
      EditorGUI.indentLevel = 1;
      editor.OnInspectorGUI();
      EditorGUI.indentLevel = 0;
    }

    /// <summary>
    /// Always returns true
    /// </summary>
    /// <returns></returns>
    protected bool True() => true;

    /// <summary>
    /// Always returns false
    /// </summary>
    protected bool False() => false;

    /// <summary>
    /// Adds a new section to the editor
    /// </summary>
    /// <param name="drawFunction"></param>
    /// <param name="validateFunction"></param>
    protected void AddArea(System.Action<Rect> drawFunction, Func<bool> validateFunction = null)
    {
      drawGroupRequests.Add(new DrawGroupRequest(drawFunction, validateFunction));
    }

    ///// <summary>
    ///// Adds a constraint that decides whether a given property is drawn
    ///// </summary>
    ///// <param name="propertyName"></param>
    ///// <param name="constraint"></param>
    //protected void AddConstraint(string propertyName, System.Func<bool> constraint)
    //{
    //  propertyConstraints.Add(propertyMap[propertyName], constraint);
    //}

    /// <summary>
    /// Adds a constraint that decides whether a given property is drawn
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="constraint"></param>
    protected void AddConstraint(System.Func<bool> constraint, params SerializedProperty[] properties)
    {
      foreach(var property in properties)
      {
        propertyConstraints.Add(property, constraint);
      }
    }

    /// <summary>
    /// Adds a constraint that decides whether a given group of properties is drawn
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="constraint"></param>
    protected void AddConstraint(System.Func<bool> constraint, params string[] propertyNames)
    {
      foreach (var propertyName in propertyNames)
      {
        propertyConstraints.Add(propertyMap[propertyName], constraint);
      }
    }

    /// <summary>
    /// Adds a constraint that decides whether a given property is drawn
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="constraint"></param>
    protected void RemoveConstraint(string propertyName)
    {
      propertyConstraints.Remove(propertyMap[propertyName]);
    }



    /// <summary>
    /// Adds a constraint that decides whether a given group of properties is drawn
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="constraint"></param>
    protected void RemoveConstraints(params string[] propertyNames)
    {
      foreach (var propertyName in propertyNames)
      {
        propertyConstraints.Remove(propertyMap[propertyName]);
      }
    }

    /// <summary>
    /// Adds a validation message on top of the editor
    /// </summary>
    /// <param name="message"></param>
    /// <param name="type"></param>
    /// <param name="target"></param>
    public void AddMessage(string message, MessageType type, UnityEngine.Object target, Func<bool> onValidate = null)
    {
      var contextMessage = new Validation(message, type.Convert(), target);
      messages.Add(contextMessage);
    }

    /// <summary>
    /// Adds a validation message on top of the editor
    /// </summary>
    /// <param name="message"></param>
    /// <param name="type"></param>
    /// <param name="target"></param>
    public void AddMessage(Validation message)
    {
      messages.Add(message);
    }

    /// <summary>
    /// Hides a property from being drawn by default
    /// </summary>
    /// <param name="propertyName"></param>
    public void HideProperty(string propertyName)
    {
      AddConstraint(False, propertyName);
    }

    /// <summary>
    /// When the specified property is changed, invokes the given function
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="onChanged"></param>
    public void AddPropertyChangeCallback(string propertyName, System.Action onChanged)
    {
      SerializedProperty property = propertyMap[propertyName];
      AddPropertyChangeCallback(property, onChanged);
    }

    /// <summary>
    /// When the specified property is changed, invokes the given function
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="onChanged"></param>
    public void AddPropertyChangeCallback(SerializedProperty property, System.Action onChanged)
    {
      propertyChangeCallbacks.Add(property, onChanged);
    }

  }

  /// <summary>
  /// Base editor for all Stratus components
  /// </summary>
  public abstract class BehaviourEditor<T> : StratusEditor where T : MonoBehaviour
  {
    protected new T target { get; private set; }
    protected override Type baseType => typeof(MonoBehaviour);

    protected virtual void OnBehaviourEditorValidate() { }

    internal override void OnStratusGenericEditorEnable()
    {
      SetTarget();
    }

    internal override void OnGenericStratusEditorValidate()
    {
      if (!target)
        SetTarget();

      if (target)
        OnBehaviourEditorValidate();
    }

    private void SetTarget()
    {
      target = base.target as T;
    }

  }

  /// <summary>
  /// Base editor for all Stratus components
  /// </summary>
  public abstract class ScriptableEditor<T> : StratusEditor where T : ScriptableObject
  {
    public override bool drawTypeLabels { get; } = true;

    protected new T target { get; private set; }
    protected override Type baseType => typeof(ScriptableObject);

    protected virtual void OnScriptableEditorValidate() { }

    internal override void OnStratusGenericEditorEnable()
    {
      target = base.target as T;
    }

    internal override void OnGenericStratusEditorValidate()
    {
      if (target)
        OnScriptableEditorValidate();
    }

  }

}