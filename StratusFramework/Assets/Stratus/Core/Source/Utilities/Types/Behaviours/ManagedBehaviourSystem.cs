﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stratus
{
  [Singleton(instantiate = true)]
  public class ManagedBehaviourSystem : Singleton<ManagedBehaviourSystem>
  {
    //--------------------------------------------------------------------------------------------/
    // Fields
    //--------------------------------------------------------------------------------------------/
    private static List<ManagedBehaviour> behaviours = new List<ManagedBehaviour>();

    //--------------------------------------------------------------------------------------------/
    // Static Methods
    //--------------------------------------------------------------------------------------------/
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    //private static void OnSceneLoaded()
    //{
    //  Instantiate();
    //}

    //--------------------------------------------------------------------------------------------/
    // Messages
    //--------------------------------------------------------------------------------------------/
    protected override void OnAwake()
    {
      //AddCurrentBehaviours();
      //foreach (var behaviour in behaviours)
      //  behaviour.OnBehaviourAwake();
    }

    private void Start()
    {
      foreach (var behaviour in behaviours)
        behaviour.OnManagedStart();
    }

    private void Update()
    {
      foreach (var behaviour in behaviours)
      {
        if (behaviour.enabled)
          behaviour.OnManagedUpdate();
      }
    }

    private void FixedUpdate()
    {
      foreach (var behaviour in behaviours)
      {
        if (behaviour.enabled)
          behaviour.OnManagedFixedUpdate();
      }
    }

    private void LateUpdate()
    {
      foreach (var behaviour in behaviours)
      {
        if (behaviour.enabled)
          behaviour.OnManagedLateUpdate();
      }
    }

    //--------------------------------------------------------------------------------------------/
    // Methods
    //--------------------------------------------------------------------------------------------/
    public static void Add(ManagedBehaviour behaviour)
    {
      Instantiate();
      behaviours.Add(behaviour);
    }

    public static void Remove(ManagedBehaviour behaviour)
    {
      behaviours.Remove(behaviour);
    }

    private static void AddCurrentBehaviours()
    {
      ManagedBehaviour[] behaviours = Scene.GetComponentsInAllActiveScenes<ManagedBehaviour>();
      StratusDebug.Log($"Adding {behaviours.Length} behaviours");
      ManagedBehaviourSystem.behaviours.AddRange(behaviours);
    }




  }

}