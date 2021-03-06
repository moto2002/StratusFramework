using UnityEngine;
using Stratus;
using UnityEditor;
using Stratus.Editor;
using System;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Callbacks;
using System.Collections.Generic;

namespace Stratus
{
  namespace AI
  {
    public class BehaviorTreeEditorWindow : StratusEditorWindow<BehaviorTreeEditorWindow>
    {
      //----------------------------------------------------------------------/
      // Declarations
      //----------------------------------------------------------------------/
      public enum Mode
      {
        Editor = 0,
        Debugger = 1,
      }

      public class BehaviourTreeView : HierarchicalTreeView<BehaviorTree.BehaviorNode>
      {
        private BehaviorTreeEditorWindow window => BehaviorTreeEditorWindow.instance;
        private BehaviorTree tree => BehaviorTreeEditorWindow.instance.behaviorTree;


        public BehaviourTreeView(TreeViewState state, IList<BehaviorTree.BehaviorNode> data) : base(state, data)
        {
        }

        protected override bool IsParentValid(BehaviorTree.BehaviorNode parent)
        {
          //Trace.Script($"Parent type = {parent.dataType}");
          if (parent.data == null)
            return false;
          if (parent.data is Composite)
            return true;
          else if (parent.data is Decorator && !parent.hasChildren)
            return true;

          return false;
        }

        protected override void OnItemContextMenu(GenericMenu menu, BehaviorTree.BehaviorNode treeElement)
        {
          // Tasks
          if (treeElement.data is Task)
          {
            BehaviorTree.BehaviorNode parent = treeElement.GetParent<BehaviorTree.BehaviorNode>();
            menu.AddItem("Duplicate", false, () => window.AddNode((Behavior)treeElement.data.Clone(), parent));
                        
            menu.AddPopup("Add/Decorator", BehaviorTreeEditorWindow.decoratorTypes.displayedOptions, (int index) =>
            {
              window.AddParentNode(BehaviorTreeEditorWindow.decoratorTypes.AtIndex(index), treeElement);
            });
          }

          // Composites
          else if (treeElement.data is Composite)
          {
            menu.AddPopup("Add/Tasks", BehaviorTreeEditorWindow.taskTypes.displayedOptions, (int index) =>
            {
              window.AddChildNode(BehaviorTreeEditorWindow.taskTypes.AtIndex(index), treeElement);
            });
            
            menu.AddPopup("Add/Composites", BehaviorTreeEditorWindow.compositeTypes.displayedOptions, (int index) =>
            {
              window.AddChildNode(BehaviorTreeEditorWindow.compositeTypes.AtIndex(index), treeElement);
            });  
            
            menu.AddPopup("Add/Decorator", BehaviorTreeEditorWindow.decoratorTypes.displayedOptions, (int index) =>
            {
              window.AddParentNode(BehaviorTreeEditorWindow.decoratorTypes.AtIndex(index), treeElement);
            });

            menu.AddPopup("Replace", BehaviorTreeEditorWindow.compositeTypes.displayedOptions, (int index) =>
            {
              window.ReplaceNode(treeElement, BehaviorTreeEditorWindow.compositeTypes.AtIndex(index));              

            }); 
          }
          // Decorators
          else if (treeElement.data is Decorator)
          {
            if (!treeElement.hasChildren)
            {
              menu.AddPopup("Add/Tasks", BehaviorTreeEditorWindow.taskTypes.displayedOptions, (int index) =>
              {
                window.AddChildNode(BehaviorTreeEditorWindow.taskTypes.AtIndex(index), treeElement);
              });

              menu.AddPopup("Add/Composites", BehaviorTreeEditorWindow.compositeTypes.displayedOptions, (int index) =>
              {
                window.AddChildNode(BehaviorTreeEditorWindow.compositeTypes.AtIndex(index), treeElement);
              });
            }
          }


          // Common
          if (treeElement.hasChildren)
          {
            menu.AddItem("Remove/Include Children", false, () => window.RemoveNode(treeElement));
            menu.AddItem("Remove/Exclude Children", false, () => window.RemoveNodeOnly(treeElement));
          }
          else
          {
            menu.AddItem("Remove", false, () => window.RemoveNode(treeElement));
          }
        }

        protected override void OnContextMenu(GenericMenu menu)
        {
          // Composite
          menu.AddPopup("Add/Composites", BehaviorTreeEditorWindow.compositeTypes.displayedOptions, (int index) =>
          {
            window.AddChildNode(BehaviorTreeEditorWindow.compositeTypes.AtIndex(index));
          });

          // Actions
          menu.AddPopup("Add/Tasks", BehaviorTreeEditorWindow.taskTypes.displayedOptions, (int index) =>
          {
            window.AddChildNode(BehaviorTreeEditorWindow.taskTypes.AtIndex(index));
          });

          menu.AddItem("Clear", false, () => window.RemoveAllNodes());
        }

        protected override void OnBeforeRow(Rect rect, TreeViewItem<BehaviorTree.BehaviorNode> treeViewItem)
        {
          if (treeViewItem.item.data is Composite)
          {
            StratusGUI.GUIBox(rect, Composite.color);
          }
          else if (treeViewItem.item.data is Task)
          {
            StratusGUI.GUIBox(rect, Task.color);
          }
          else if (treeViewItem.item.data is Decorator)
          {
            StratusGUI.GUIBox(rect, Decorator.color);
          }
        }
      }

      //----------------------------------------------------------------------/
      // Fields
      //----------------------------------------------------------------------/
      [SerializeField]
      private BehaviourTreeView treeInspector;
      [SerializeField]
      private TreeViewState treeViewState;
      [SerializeField]
      private Mode mode = Mode.Editor;
      [SerializeField]
      private Agent agent;
      [SerializeField]
      public BehaviorTree behaviorTree;

      private SerializedSystemObject currentNodeSerializedObject;
      const string folder = "Stratus/Experimental/AI/";
      private Vector2 inspectorScrollPosition, blackboardScrollPosition;
      private StratusSerializedPropertyMap behaviorTreeProperties;
      private StratusEditor blackboardEditor;
      private string[] toolbarOptions = new string[]
      {
        nameof(Mode.Editor),
        nameof(Mode.Debugger),
      };

      //----------------------------------------------------------------------/
      // Properties
      //----------------------------------------------------------------------/
      private static Type compositeType { get; } = typeof(Composite);
      private static Type decoratorType { get; } = typeof(Decorator);

      /// <summary>
      /// All supported behavior types
      /// </summary>
      public static TypeSelector behaviorTypes { get; } = TypeSelector.FilteredSelector(typeof(Behavior), typeof(Service), false, true);

      /// <summary>
      /// All supported decorator types
      /// </summary>
      public static TypeSelector compositeTypes { get; } = new TypeSelector(typeof(Composite), false, true);

      /// <summary>
      /// All supported decorator types
      /// </summary>
      public static TypeSelector taskTypes { get; } = new TypeSelector(typeof(Task), false, true);

      /// <summary>
      /// All supported decorator types
      /// </summary>
      public static TypeSelector serviceTypes { get; } = new TypeSelector(typeof(Service), false, true);

      /// <summary>
      /// All supported decorator types
      /// </summary>
      public static TypeSelector decoratorTypes { get; } = new TypeSelector(typeof(Decorator), false, true);
      
      /// <summary>
      /// The blackboard being used by the tree
      /// </summary>
      private Blackboard blackboard => behaviorTree.blackboard;

      /// <summary>
      /// The scope of the blackboard being inspected
      /// </summary>
      private Blackboard.Scope scope;

      /// <summary>
      /// The nodes currently being inspected
      /// </summary>
      public IList<BehaviorTree.BehaviorNode> currentNodes { get; private set; }

      /// <summary>
      /// The nodes currently being inspected
      /// </summary>
      public BehaviorTree.BehaviorNode currentNode { get; private set; }

      public bool hasSelection => currentNodes != null;

      /// <summary>
      /// Whether the editor for the BT has been initialized
      /// </summary>
      private bool isTreeSet => behaviorTree != null && behaviorTreeProperties != null;

      /// <summary>
      /// Whether the blackboard has been set
      /// </summary>
      private bool isBlackboardSet => blackboardEditor;

      //----------------------------------------------------------------------/
      // Messages
      //----------------------------------------------------------------------/
      protected override void OnWindowEnable()
      {
        if (this.treeViewState == null)
          this.treeViewState = new TreeViewState();

        if (behaviorTree)
        {
          this.treeInspector = new BehaviourTreeView(treeViewState, behaviorTree.tree.elements);
          this.OnTreeSet();
        }
        else
        {
          TreeBuilder<BehaviorTree.BehaviorNode, Behavior> treeBuilder = new TreeBuilder<BehaviorTree.BehaviorNode, Behavior>();
          this.treeInspector = new BehaviourTreeView(treeViewState, treeBuilder.ToTree());
        }

        this.treeInspector.onSelectionIdsChanged += this.OnSelectionChanged;
        this.treeInspector.Reload();
      }

      protected override void OnWindowGUI()
      {
        StratusEditorGUI.BeginAligned(TextAlignment.Center);
        this.mode = (Mode)GUILayout.Toolbar((int)this.mode, this.toolbarOptions, GUILayout.ExpandWidth(false));
        StratusEditorGUI.EndAligned();

        GUILayout.Space(padding);
        switch (this.mode)
        {
          case Mode.Editor:
            this.DrawEditor();
            break;
          case Mode.Debugger:
            this.DrawDebugger();
            break;
        }

      }

      //----------------------------------------------------------------------/
      // Procedures
      //----------------------------------------------------------------------/
      private void DrawEditor()
      {
        //EditProperty(nameof(this.behaviorTree));
        if (this.EditObjectFieldWithHeader(ref this.behaviorTree, "Behavior Tree"))
          this.OnTreeSet();

        if (!isTreeSet)
          return;

        //if (StratusEditorUtility.currentEvent.type != EventType.Repaint)
        //  return;

        Rect rect = this.guiPosition; //  this.currentPosition;
        rect = StratusEditorUtility.PadVertical(rect, lineHeight * 4f);
        rect = StratusEditorUtility.Pad(rect) ;
        //rect.y += StratusEditorUtility.lineHeight;

        // Hierarchy: LEFT
        rect.width *= 0.5f;
        DrawHierarchy(rect);
        // Inspector: TOP-RIGHT
        rect.x += rect.width;
        rect.width -= StratusEditorGUI.standardPadding;
        rect.height *= 0.5f;
        rect.height -= padding * 2f;
        DrawInspector(rect);
        // Blackboard: BOTTOM-RIGHT
        rect.y += rect.height;
        rect.y += padding;
        DrawBlackboard(rect);
      }

      private void DrawDebugger()
      {
        //EditProperty(nameof(this.agent));
        //EditProperty(nameof(debugTarget));
        this.EditObjectFieldWithHeader(ref this.agent, "Agent");
      }

      private void DrawHierarchy(Rect rect)
      {
        //if (behaviorTree != null)
        GUILayout.BeginArea(rect);
        GUILayout.Label("Hierarchy", StratusGUIStyles.header);
        GUILayout.EndArea();
        rect = StratusEditorUtility.PadVertical(rect, lineHeight);
        //rect.y += lineHeight * 2f;
        treeInspector?.TreeViewGUI(rect);
      }

      private void DrawInspector(Rect rect)
      {
        GUILayout.BeginArea(rect);
        GUILayout.Label("Inspector", StratusGUIStyles.header);

        if (hasSelection)
        {
          if (currentNodes.Count == 1)
          {
            GUILayout.Label(currentNode.dataTypeName, EditorStyles.largeLabel);
            this.inspectorScrollPosition = EditorGUILayout.BeginScrollView(this.inspectorScrollPosition, GUI.skin.box);
            currentNodeSerializedObject.DrawEditorGUILayout();
            EditorGUILayout.EndScrollView();
          }
          else
          {
            GUILayout.Label("Editing multiple nodes is not supported!", EditorStyles.largeLabel);
          }
        }

        GUILayout.EndArea();
      }

      private void DrawBlackboard(Rect rect)
      {
        GUILayout.BeginArea(rect);
        GUILayout.Label("Blackboard", StratusGUIStyles.header);
        {
          // Set the blackboard
          SerializedProperty blackboardProperty = this.behaviorTreeProperties.GetProperty(nameof(BehaviorTree.blackboard));
          bool changed = this.EditProperty(blackboardProperty, "Asset");
          if (changed && this.blackboard != null)
            this.OnBlackboardSet();

          EditorGUILayout.Space();

          // Draw the blackboard
          if (this.blackboardEditor != null)
          {
            // Controls
            StratusEditorGUI.BeginAligned(TextAlignment.Center);
            StratusEditorGUI.EnumToolbar(ref scope);
            StratusEditorGUI.EndAligned();

            this.blackboardScrollPosition = EditorGUILayout.BeginScrollView(this.blackboardScrollPosition, GUI.skin.box);
            switch (scope)
            {
              case Blackboard.Scope.Local:
                blackboardEditor.DrawSerializedProperty(nameof(Blackboard.locals));
                break;
              case Blackboard.Scope.Global:
                blackboardEditor.DrawSerializedProperty(nameof(Blackboard.globals));
                break;
            }
            EditorGUILayout.EndScrollView();
          }

        }
        GUILayout.EndArea();
      }

      //----------------------------------------------------------------------/
      // Methods: Private
      //----------------------------------------------------------------------/
      private void AddChildNode(Type type, BehaviorTree.BehaviorNode parent = null)
      {
        if (parent != null)
          behaviorTree.AddBehavior(type, parent);
        else
          behaviorTree.AddBehavior(type);

        Save();
      }

      private void AddParentNode(Type type, BehaviorTree.BehaviorNode child)
      {
        behaviorTree.AddParentBehavior(type, child);
        Save();
      }

      private void AddNode(Behavior behavior, BehaviorTree.BehaviorNode parent = null)
      {
        if (parent != null)
          behaviorTree.AddBehavior(behavior, parent);
        else
          behaviorTree.AddBehavior(behavior);

        Save();
      }
      private void RemoveNode(BehaviorTree.BehaviorNode node)
      {
        currentNodeSerializedObject = null;
        currentNodes = null;

        this.behaviorTree.RemoveBehavior(node);
        Save();
      }

      private void RemoveNodeOnly(BehaviorTree.BehaviorNode node)
      {
        currentNodeSerializedObject = null;
        currentNodes = null;

        this.behaviorTree.RemoveBehaviorExcludeChildren(node);
        Save();
      }

      private void ReplaceNode(BehaviorTree.BehaviorNode node, Type behaviorType)
      {
        currentNodeSerializedObject = null;
        currentNodes = null;
        this.behaviorTree.ReplaceBehavior(node, behaviorType);
        Save();
      }

      private void RemoveAllNodes()
      {
        behaviorTree.ClearBehaviors();
        currentNodeSerializedObject = null;
        currentNodes = null;
        Save();
      }

      private void Refresh()
      {
        this.treeInspector.SetTree(this.behaviorTree.tree.elements);
        this.Repaint();
      }

      private void OnTreeSet()
      {
        this.behaviorTree.Assert();
        this.behaviorTreeProperties = new StratusSerializedPropertyMap(this.behaviorTree, typeof(StratusScriptable));

        // Blackboard
        this.blackboardEditor = null;
        if (this.blackboard)
          this.OnBlackboardSet();

        this.Refresh();
      }

      private void OnBlackboardSet()
      {
        this.blackboardEditor = StratusEditor.CreateEditor(this.behaviorTree.blackboard) as StratusEditor;
      }

      private void Save()
      {
        EditorUtility.SetDirty(behaviorTree);
        Undo.RecordObject(behaviorTree, "Behavior Tree Edit");
        this.Refresh();
      }

      private void OnSelectionChanged(IList<int> ids)
      {
        this.currentNodeSerializedObject = null;
        //this.currentNodeProperty = null;

        this.currentNodes = this.treeInspector.GetElements(ids);
        if (this.currentNodes.Count > 0)
        {
          this.currentNode = currentNodes[0];
          this.currentNodeSerializedObject = new SerializedSystemObject(currentNode.data);
          //SerializedObject boo = new SerializedObject(currentNode.data);
          //this.currentNodeProperty = this.treeElementsProperty.GetArrayElementAtIndex(ids[0]);
        }
      }

      //----------------------------------------------------------------------/
      // Methods: Static
      //----------------------------------------------------------------------/
      [OnOpenAsset]
      public static bool OnOpenAsset(int instanceID, int line)
      {
        var myTreeAsset = EditorUtility.InstanceIDToObject(instanceID) as BehaviorTree;
        if (myTreeAsset != null)
        {
          Open(myTreeAsset);
          return true;
        }
        return false;
      }

      public static void Open(BehaviorTree tree)
      {
        OnOpen("Behavior Tree");
        instance.SetTree(tree);
      }

      public void SetTree(BehaviorTree tree)
      {
        this.behaviorTree = tree;
        this.OnTreeSet();
      }




    }
  }

}