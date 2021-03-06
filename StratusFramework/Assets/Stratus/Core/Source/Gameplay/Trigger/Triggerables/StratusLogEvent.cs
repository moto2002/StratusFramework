using UnityEngine;
using Stratus;
using System;
using Stratus.Dependencies.Ludiq.Reflection;

namespace Stratus.Gameplay
{
  /// <summary>
  /// Simple event that logs a message to the console when triggered.
  /// </summary>
  public class StratusLogEvent : StratusTriggerable
  {
    public enum LogType
    {
      Description,
      Member
    }
    

    ///[Tooltip("What type og log")]
    ///public LogType type;
    ///[Tooltip("The member to log")]
    ///[DrawIf("type", LogType.Member, ComparisonType.Equals)]
    ///[Filter(Methods = false, Properties = true, NonPublic = true, ReadOnly = true, Static = true, Inherited = true, Fields = true)]
    ///public UnityMember member;

    protected override void OnAwake()
    {
      
    }

    protected override void OnReset()
    {
      descriptionMode = DescriptionMode.Manual;
    }

    protected override void OnTrigger()
    {
      StratusDebug.Log(description, this);

      //string value = null;
      //
      //switch (type)
      //{
      //  case LogType.Description:
      //    value = description;
      //    break;
      //  case LogType.Member:
      //    value = member.Get().ToString();
      //    break;
      //  default:
      //    break;
      //}
      //
      //Trace.Script(value, this);      
    }
    
  }
}
