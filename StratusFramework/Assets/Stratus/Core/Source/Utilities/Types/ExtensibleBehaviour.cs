﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq.Expressions;
using System.Linq;

namespace Stratus
{
  /// <summary>
  /// A generic behaviour that has extensions that can be added or removed to it
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public abstract class ExtensibleBehaviour : StratusBehaviour, IExtensible
  {
    //--------------------------------------------------------------------------------------------/
    // Fields
    //--------------------------------------------------------------------------------------------/
    [HideInInspector]
    public List<MonoBehaviour> extensionBehaviours = new List<MonoBehaviour>();
    [HideInInspector]
    public int selectedExtensionIndex;

    //--------------------------------------------------------------------------------------------/
    // Properties
    //--------------------------------------------------------------------------------------------/
    public IExtensionBehaviour[] extensions => GetExtensionBehaviours(this.extensionBehaviours);
    private Dictionary<Type, IExtensionBehaviour> extensionsMap { get; set; } = new Dictionary<Type, IExtensionBehaviour>();
    public bool hasExtensions => extensionBehaviours.Count > 0;
    private static Dictionary<IExtensionBehaviour, ExtensibleBehaviour> extensionOwnershipMap { get; set; } = new Dictionary<IExtensionBehaviour, ExtensibleBehaviour>();
    public static HideFlags extensionFlags { get; } = HideFlags.HideInInspector;
    //public int selectedExtensionIndex { get { return _selectedExtension; } set { _selectedExtension = value; } }

    //--------------------------------------------------------------------------------------------/
    // Virtual
    //--------------------------------------------------------------------------------------------/
    protected abstract void OnAwake();
    protected abstract void OnStart();

    //--------------------------------------------------------------------------------------------/
    // Messages
    //--------------------------------------------------------------------------------------------/
    private void Awake()
    {
      OnAwake();

      foreach (var extension in extensions)
      {
        extensionsMap.Add(extension.GetType(), extension);
        extension.OnExtensibleAwake(this);        
      }
    }

    private void Start()
    {
      OnStart();

      foreach (var extension in extensions)
        extension.OnExtensibleStart();

    }

    //--------------------------------------------------------------------------------------------/
    // Methods
    //--------------------------------------------------------------------------------------------/
    /// <summary>
    /// Adds the extension to this behaviour
    /// </summary>
    /// <param name="extension"></param>
    public void Add(IExtensionBehaviour extension)
    {
      //extension.extensibleField = this;      
      (extension as MonoBehaviour).hideFlags = ExtensibleBehaviour.extensionFlags;
      extensionBehaviours.Add((MonoBehaviour)extension);
      extensionsMap.Add(extension.GetType(), extension);
    }

    /// <summary>
    /// Removes the extension from this behaviour
    /// </summary>
    /// <param name="extension"></param>
    public void Remove(IExtensionBehaviour extension)
    {
      extensionBehaviours.Remove((MonoBehaviour)extension);
      extensionsMap.Remove(extension.GetType());
    }

    /// <summary>
    /// Removes the extension from this behaviour
    /// </summary>
    /// <param name="extension"></param>
    public void Remove(int index)
    {
      var extension = extensionBehaviours[index];
      Type extensionType = extension.GetType();
      Trace.Script($"Noww removing {extensionType}");
      extensionsMap.Remove(extensionType);
      extensionBehaviours.RemoveAt(index);
    }

    /// <summary>
    /// Retrieves the extension of the given type, if its present
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetExtension<T>() where T : IExtensionBehaviour
    {
      Type type = typeof(T);
      if (!extensionsMap.ContainsKey(type))
        Trace.Error($"The extension of type {type} is not present!", this);
      return (T)extensionsMap[type];
    }

    /// <summary>
    /// Retrieves the extension of the given type, if its present
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool HasExtension(Type type) => extensionsMap.ContainsKey(type);

    /// <summary>
    /// Retrieves the extension of the given type, if its present
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool HasExtension<T>() where T : IExtensionBehaviour
    {
      return this.GetExtension<T>() != null;
    }

    /// <summary>
    /// Retrieves the extension of the given type, if its present
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool HasExtension(IExtensionBehaviour behaviour)
    {
      return this.HasExtension(behaviour.GetType());
    }

    /// Retrieves the extensible that the extension is for
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="extension"></param>
    /// <returns></returns>
    public static T GetExtensible<T>(IExtensionBehaviour extension) where T : ExtensibleBehaviour
    {
      return extensionOwnershipMap[extension] as T;
    }

    public static IExtensionBehaviour[] GetExtensionBehaviours(MonoBehaviour[] monoBehaviours)
    {
      return (from MonoBehaviour e in monoBehaviours select (e as IExtensionBehaviour)).ToArray();
    }

    public static IExtensionBehaviour[] GetExtensionBehaviours(List<MonoBehaviour> monoBehaviours)
    {
      return (from MonoBehaviour e in monoBehaviours select (e as IExtensionBehaviour)).ToArray();
    }

    [ContextMenu("Show Extensions")]
    private void ShowExtensions()
    {
      foreach(var extension in this.extensionBehaviours)
      {
        extension.hideFlags = HideFlags.None;
      }
    }

    [ContextMenu("Hide Extensions")]
    private void HideExtensions()
    {
      foreach (var extension in this.extensionBehaviours)
      {
        extension.hideFlags = HideFlags.HideInInspector;
      }
    }


    /// <summary>

    
  }

  /// <summary>
  /// Tells the Editor which extensible class this extension is for
  /// </summary>
  public class CustomExtensionAttribute : Attribute
  {
    public CustomExtensionAttribute(params Type[] supportedExtensibles)
    {
      this.supportedExtensibles = supportedExtensibles;
    }

    public Type[] supportedExtensibles { get; set; }

  }

  /// <summary>
  /// Allows additional configuration of an extensible behaviour
  /// </summary>
  public class ExtensibleBehaviourAttribute : Attribute
  {
    public ExtensibleBehaviourAttribute(params Type[] extensionTypes)
    {
      this.extensionTypes = extensionTypes;
    }

    public Type[] extensionTypes{ get; set; }
  }


  /// <summary>
  /// Interface type used to validate all extensible behaviours
  /// </summary>
  public interface IExtensible { }

  ///// <summary>
  ///// Interface type used to validate all extensible behaviours
  ///// </summary>
  //public interface IExtensibleBehaviour { }

  /// <summary>
  /// Interface type a behaviours that is an extension of another
  /// </summary>
  public interface IExtensionBehaviour
  {
    void OnExtensibleAwake(ExtensibleBehaviour extensible);
    void OnExtensibleStart();
  }

  /// <summary>
  /// Interface type used to validate all extensible behaviours
  /// </summary>
  public interface IExtensionBehaviour<T> : IExtensionBehaviour where T : ExtensibleBehaviour
  {
    /// <summary>
    /// The extensible component this extension is for
    /// </summary>
    T extensible { set; get; }    
  }

  public static class ExtensibleExtensions
  {
    //public static void SetExtensible(this IExtensionBehaviour extension, ExtensibleBehaviour extensible) where T: ExtensibleBehaviour
    //{
    //  extension.set
    //}
  }

}