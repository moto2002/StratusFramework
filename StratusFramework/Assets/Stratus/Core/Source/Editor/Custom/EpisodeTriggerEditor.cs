using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Stratus.Gameplay
{
  [CustomEditor(typeof(StratusEpisodeTrigger))]
  public class EpisodeTriggerEditor : StratusBehaviourEditor<StratusEpisodeTrigger>
  {
    private ObjectDropdownList<StratusSegment> segments;
    private SerializedProperty episodeProperty;

    protected override void OnStratusEditorEnable()
    {
      MakeSegmentList();
      episodeProperty = propertyMap["episode"];
      propertyChangeCallbacks.Add(episodeProperty, MakeSegmentList);
    }

    protected override bool DrawDeclaredProperties()
    {
      bool changed = false;
      changed |= DrawSerializedProperty(propertyMap["episode"]);

      if (segments != null)
      {
        segments.selectedIndex = EditorGUILayout.Popup("Segment", segments.selectedIndex, segments.displayedOptions);
        changed |= DrawSerializedProperty(propertyMap["eventType"]);
        target.segment = segments.selected;
      }
      return changed;
    }
    
    private void MakeSegmentList()
    {
      segments = null;

      if (!target.episode)
      {
        target.segment = null;
        return;
      }

      segments = new ObjectDropdownList<StratusSegment>(target.episode.segments, target.segment);
    }

  }

}