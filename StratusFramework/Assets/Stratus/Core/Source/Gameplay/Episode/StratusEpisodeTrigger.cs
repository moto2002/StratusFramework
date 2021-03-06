using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stratus.Gameplay
{
  /// <summary>
  /// A trigger for events within the episode system
  /// </summary>
  public class StratusEpisodeTrigger : Trigger
  {
    //------------------------------------------------------------------------/
    // Fields
    //------------------------------------------------------------------------/
    public StratusEpisode episode;
    [Tooltip("The segment to check against")]
    public StratusSegment segment;
    [Tooltip("What type to event to listen for")]
    public StratusSegment.EventType eventType;
    //[Tooltip("How to refer to the segment ")]
    //public Segment.ReferenceType referenceType = Segment.ReferenceType.Reference;
    //[Tooltip("The label of the segment to check against")]
    //[DrawIf("referenceType", Segment.ReferenceType.Label, ComparisonType.Equals)]
    //public string segmentLabel;
    //[DrawIf("referenceType", Segment.ReferenceType.Reference, ComparisonType.Equals)]

    public override string automaticDescription
    {
      get
      {
        if (segment != null)
          return $"On {eventType} {episode.label}.{segment.label}";
        return string.Empty;
      }
    }

    //------------------------------------------------------------------------/
    // Messages
    //------------------------------------------------------------------------/
    protected override void OnAwake()
    {
      if (eventType == StratusSegment.EventType.Enter)
        Scene.Connect<StratusSegment.EnteredEvent>(this.OnSegmentEnteredEvent);
      else if (eventType == StratusSegment.EventType.Exit)
        Scene.Connect<StratusSegment.ExitedEvent>(this.OnSegmentExitedEvent);
    }

    protected override void OnReset()
    {
      segment = GetComponent<StratusSegment>();
      if (segment)
        episode = segment.episode;
    }    

    //------------------------------------------------------------------------/
    // Methods
    //------------------------------------------------------------------------/

    void OnSegmentEnteredEvent(StratusSegment.EnteredEvent e)
    {
      if (ValidateSegment(e.segment))
        Activate();
    }

    void OnSegmentExitedEvent(StratusSegment.ExitedEvent e)
    {
      if (ValidateSegment(e.segment))
        Activate();
    }

    bool ValidateSegment(StratusSegment segment)
    {
      return this.segment == segment;
      //if (referenceType == ReferenceType.Label)
      //  return this.segmentLabel == segment.label;
      //else if (referenceType == ReferenceType.Reference)
      //  return this.segment = segment;
      //return false;
    }


  }

}