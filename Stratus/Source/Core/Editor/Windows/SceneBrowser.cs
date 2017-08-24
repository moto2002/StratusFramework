/******************************************************************************/
/*!
@file   SceneSelector.cs
@author Christian Sagel
@par    email: ckpsm@live.com
*/
/******************************************************************************/
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Stratus.Utilities;

namespace Stratus
{
  public class SceneBrowser : EditorWindow
  {
    //------------------------------------------------------------------------/
    // Fields
    //------------------------------------------------------------------------/
    private Vector2 ScrollPosition;
    private static string Title = "Scene Browser";

    [SerializeField]
    public static List<SceneAsset> BookmarkedScenes { get { return Preferences.members.bookmarkedScenes; } }
    private static GUIStyle RemoveButtonStyle
    {
      get
      {
        var style = EditorStyles.miniButton;
        style.fontStyle = FontStyle.Bold;
        style.font.material.color = Color.red;
        style.alignment = TextAnchor.MiddleCenter;
        style.fixedWidth = 20;
        return style;
      }
    }

    public static SceneBrowser Instance;
    private static SceneAsset sceneToAdd;
    private bool showBookmarkScenes, showBuildScenes;

    //------------------------------------------------------------------------/
    // Messages
    //------------------------------------------------------------------------/
    [MenuItem("Stratus/Windows/Scene Browser %g")]
    public static void Open()
    {
      EditorWindow.GetWindow(typeof(SceneBrowser), false, Title);
    }

    //private void OnEnable()
    //{
    //  SerializedObject so = new SerializedObject(this);
    //  sceneToAddProp = so.FindProperty("sceneToAdd");
    //}

    private void OnGUI()
    {
      EditorGUILayout.BeginVertical();
      this.ScrollPosition = EditorGUILayout.BeginScrollView(this.ScrollPosition, false, false);
      {
        this.ShowBookmarkedScenes();
        EditorGUILayout.Space();
        this.AddBookmarkedScene();
        this.ShowScenesInBuild();
      }
      EditorGUILayout.EndScrollView();
      EditorGUILayout.EndVertical();
    }

    //------------------------------------------------------------------------/
    // GUI
    //------------------------------------------------------------------------/
    void ShowScenesInBuild()
    {
      GUILayout.Label("Build", EditorStyles.centeredGreyMiniLabel);
      for (var i = 0; i < EditorBuildSettings.scenes.Length; ++i)
      {
        var scene = EditorBuildSettings.scenes[i];
        if (scene.enabled)
        {
          var sceneName = Path.GetFileNameWithoutExtension(scene.path);
          var pressed = GUILayout.Button(i + ": " + sceneName, EditorStyles.toolbarButton);
          if (pressed)
          {
            var button = UnityEngine.Event.current.button;
            if (button == 0 && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
              EditorSceneManager.OpenScene(scene.path);
            }
          }
        }
      }
    }


    void ShowBookmarkedScenes()
    {
      GUILayout.Label("Bookmarks", EditorStyles.centeredGreyMiniLabel);
      foreach (var scene in BookmarkedScenes)
      {
        EditorGUILayout.BeginHorizontal();
        // Open scene
        if (GUILayout.Button(scene.name, EditorStyles.toolbarButton))
        {
          var button = UnityEngine.Event.current.button;
          // Left click
          if (button == 0)
          {
            var scenePath = AssetDatabase.GetAssetPath(scene);
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
              EditorSceneManager.OpenScene(scenePath);
            }
          }
          // Right click
          else if (button == 1)
          {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove"), false,
              () =>
              {
                //Trace.Script("Removing " + scene.name);
                RemoveScene(scene);
              }
              );
            menu.ShowAsContext();
          }
        }
        EditorGUILayout.EndHorizontal();
      }

    }

    void RemoveScene(SceneAsset scene)
    {
      BookmarkedScenes.Remove(scene);
      Preferences.Save();
      Repaint();
    }

    //------------------------------------------------------------------------/
    // Methods
    //------------------------------------------------------------------------/   
    //[MenuItem("Assets/Bookmark")]
    //public static void BookmarkScene()
    //{
    //  var obj = Selection.activeObject;
    //  var scene = obj as SceneAsset;
    //  if (scene)
    //  {
    //    BookmarkedScenes.Add(scene);
    //    Preferences.Save();
    //    Trace.Script(scene.name + " has been bookmarked!");
    //  }
    //}

    void AddBookmarkedScene()
    {
      //EditorGUILayout.PropertyField(sceneToAddProp);
      EditorGUILayout.BeginHorizontal();
      sceneToAdd = EditorGUILayout.ObjectField(sceneToAdd, typeof(SceneAsset), false) as SceneAsset;
      if (GUILayout.Button("Add", EditorStyles.miniButtonRight) && !BookmarkedScenes.Contains(sceneToAdd))
      {
        BookmarkedScenes.Add(sceneToAdd);
      }

      EditorGUILayout.EndHorizontal();

    }



  }

}