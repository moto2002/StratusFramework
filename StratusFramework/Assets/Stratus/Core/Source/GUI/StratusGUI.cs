using UnityEngine;
using Stratus.Utilities;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Stratus
{
  /// <summary>
  /// A programmatic overlay for debugging use. You can use the preset window
  /// for quick prototyping, or make your own windows.
  /// </summary>
  [Singleton("Stratus Overlay", true, true)]
  public partial class StratusGUI : Singleton<StratusGUI>
  {
    //------------------------------------------------------------------------/
    // Declarations
    //------------------------------------------------------------------------/
    public class OverlayWindows
    {
      public Window Watch;
      public Window Buttons; 
      public Console Console;
    }

    public delegate void OnGUILayout(Rect rect);

    //------------------------------------------------------------------------/
    // Properties
    //------------------------------------------------------------------------/
    /// <summary>
    /// Displays the current FPS
    /// </summary>
    public static bool showFPS { get; set; } = true;
    /// <summary>
    /// The current screen size of the game window
    /// </summary>
    public static Vector2 screenSize
    {
      get
      {
        #if UNITY_EDITOR
        string[] res = UnityEditor.UnityStats.screenRes.Split('x');
        Vector2 screenSize = new Vector2(int.Parse(res[0]), int.Parse(res[1]));
        #else
        Vector2 screenSize = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        #endif
        return screenSize;
      }
    }

    //------------------------------------------------------------------------/
    // Fields
    //------------------------------------------------------------------------/
    /// <summary>
    /// The anchored position of the default overlay window
    /// </summary>
    private OverlayWindows Windows = new OverlayWindows();

    /// <summary>
    /// All custom windows written by the user
    /// </summary>
    private Dictionary<string, Window> CustomWindows = new Dictionary<string, Window>();

    /// <summary>
    /// Displays the FPS in the game Window
    /// </summary>
    private FPSCounter fpsCounter = new FPSCounter();

    ///// <summary>
    ///// Draw requests
    ///// </summary>
    //private List<OnGUILayout> drawRequests = new List<OnGUILayout>();

    //------------------------------------------------------------------------/
    // Messages
    //------------------------------------------------------------------------/
    protected override void OnAwake()
    {
      Reset();
      Scene.onSceneChanged += OnSceneChanged; 
    }

    private void Update()
    {
      if (showFPS) fpsCounter.Update();
    }

    private void OnGUI()
    {
      Draw();      
    }

    //------------------------------------------------------------------------/
    // Methods: Static
    //------------------------------------------------------------------------/
    /// <summary>
    /// Keeps watch of a given property/field
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="varExpr">An expression of a given variable of the form: '(()=> foo')</param>
    /// <param name="behaviour">The owner object of this variable</param>
    /// <example>Overlay.Watch(()=> foo, this);</example>
    public static void Watch<T>(Expression<Func<T>> varExpr, string description = null, MonoBehaviour behaviour = null)
    {
      var variableRef = Reflection.GetReference(varExpr);
      var watcher = new Watcher(variableRef, description, behaviour);
      instance.Windows.Watch.Add(watcher);      
    }
    /// <summary>
    /// Adds a button to the overlay which invokes a function with no parameters.
    /// </summary>
    /// <param name="description">What description to use for the button</param>
    /// <param name="onButtonDown">The function to be invoked when the button is pressed</param>
    public static void AddButton(string description, Button.Callback onButtonDown)
    {      
      var button = new Button(description, onButtonDown);
      instance.Windows.Buttons.Add(button);
    }

    /// <summary>
    /// Adds a button to the overlay which invokes a function with no parameters
    /// </summary>
    /// <param name="onButtonDown"></param>
    public static void AddButton(Button.Callback onButtonDown)
    {
      var button = new Button(onButtonDown.Method.Name, onButtonDown);
      instance.Windows.Buttons.Add(button);
    }

    /// <summary>
    /// Adds a button to the overlay which invokes a function with any parameters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="description">What description to use for the button</param>
    /// <param name="onButtonDown">The function to be invoked when the button is pressed, using a lambda expresion to pass it: '()=>Foo(7)</param>
    public static void AddButton<T>(string description, Button<T>.Callback onButtonDown)
    {
      var button = new Button<T>(description, onButtonDown);
      instance.Windows.Buttons.Add(button);
    }

    public static void GUILayoutArea(Anchor anchor, Vector2 size, System.Action<Rect> onGUI)
    {      
      Rect rect = StratusGUI.CalculateAnchoredPositionOnScreen(anchor, size);
      UnityEngine.GUILayout.BeginArea(rect);      
      onGUI(rect);
      UnityEngine.GUILayout.EndArea();
    }

    public static void GUILayoutArea(Anchor anchor, Vector2 size, GUIContent content, System.Action<Rect> onGUI)
    {
      Rect rect = StratusGUI.CalculateAnchoredPositionOnScreen(anchor, size);
      UnityEngine.GUILayout.BeginArea(rect, content);
      onGUI(rect);
      UnityEngine.GUILayout.EndArea();
    }

    public static void GUILayoutArea(Anchor anchor, Vector2 size, GUIContent content, GUIStyle style, System.Action<Rect> onGUI)
    {
      Rect rect = StratusGUI.CalculateAnchoredPositionOnScreen(anchor, size);
      UnityEngine.GUILayout.BeginArea(rect, content, style);
      onGUI(rect);
      UnityEngine.GUILayout.EndArea();
    }

    public static void GUILayoutArea(Anchor anchor, Vector2 size, GUIStyle style, System.Action<Rect> onGUI)
    {
      Rect rect = StratusGUI.CalculateAnchoredPositionOnScreen(anchor, size);
      UnityEngine.GUILayout.BeginArea(rect, style);
      onGUI(rect);
      UnityEngine.GUILayout.EndArea();
    }

    public static void GUIBox(Rect rect, Color tint, GUIStyle style)
    {
      Color currentColor = GUI.color;
      GUI.color = tint;
      GUI.Box(rect, string.Empty, style);
      GUI.color = currentColor;
    }

    public static void GUIBox(Rect rect, Color tint)
    {
      Color currentColor = GUI.color;
      GUI.color = tint;
      GUI.Box(rect, string.Empty);
      GUI.color = currentColor;
    }



    //------------------------------------------------------------------------/
    // Methods: Private
    //------------------------------------------------------------------------/
    /// <summary>
    /// Resets all windows to their defaults
    /// </summary>
    void Reset()
    {
      Windows.Watch = new Window("Watch", new Vector2(0.2f, 0.5f), Color.grey, Anchor.TopRight);
      Windows.Buttons = new Window("Buttons", new Vector2(0.3f, 0.4f), Color.grey, Anchor.BottomRight);
    }

    /// <summary>
    /// When the scene changes, reset all windows!
    /// </summary>
    void OnSceneChanged()
    {      
      Reset();
    }

    /// <summary>
    /// Draws all overlay elements
    /// </summary>
    void Draw()
    {
      // Draw all the innate windows
      Windows.Watch.Draw();
      Windows.Buttons.Draw();     

      // Draw all custom windows
      foreach (var window in CustomWindows)
        window.Value.Draw();

      // Draw all custom content

      // Show FPS
      if (showFPS) DisplayFPS();
    }

    void DisplayFPS()
    {
    }

  }
}
