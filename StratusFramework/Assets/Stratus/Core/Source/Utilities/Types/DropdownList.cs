using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Stratus
{
  /// <summary>
  /// A dropdow
  /// </summary>
  public class DropdownList<T> where T : class
  {
    //------------------------------------------------------------------------/
    // Properties
    //------------------------------------------------------------------------/
    public string[] displayedOptions { get; private set; }
    public int selectedIndex { get; set; }
    public T selected => isList ? list[selectedIndex] : array[selectedIndex];

    //------------------------------------------------------------------------/
    // Fields
    //------------------------------------------------------------------------/
    private List<T> list;
    private T[] array;
    private bool isList;

    //------------------------------------------------------------------------/
    // Methods
    //------------------------------------------------------------------------/
    public DropdownList(List<T> list, Func<T, string> namer, T initial = null)
    {
      this.list = list;
      isList = true;
      displayedOptions = list.Names(namer);

      if (initial != null)
        SetIndex(initial);
    }

    public DropdownList(List<T> list, Func<T, string> namer, int index = 0)
    {
      this.list = list;
      isList = true;
      displayedOptions = list.Names(namer);
      selectedIndex = 0;
    }

    public DropdownList(T[] array, Func<T, string> namer, T initial = null)
    {
      this.array = array;
      isList = false;
      displayedOptions = array.Names(namer);

      if (initial != null)
        SetIndex(initial);
    }

    public DropdownList(T[] array, Func<T, string> namer, int index = 0)
    {
      this.array = array;
      isList = false;
      displayedOptions = array.Names(namer);
      selectedIndex = 0;
    }

    /// <summary>
    /// Sets the current index of this list to that of the given element
    /// </summary>
    /// <param name="element"></param>
    public void SetIndex(T element)
    {
      if (isList)
        selectedIndex = list.FindIndex(x => x == element);
      else
        selectedIndex = array.FindIndex(x => x == element);
    }
  }

  /// <summary>
  /// A generic dropdown list that refers to a list of any given Object type
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ObjectDropdownList<T> where T : UnityEngine.Object
  {
    //------------------------------------------------------------------------/
    // Properties
    //------------------------------------------------------------------------/
    public string[] displayedOptions { get; private set; }
    public int selectedIndex { get; set; }
    public T selected => isList ? list[selectedIndex] : array[selectedIndex];

    //------------------------------------------------------------------------/
    // Fields
    //------------------------------------------------------------------------/
    private List<T> list;
    private T[] array;
    private bool isList;

    //------------------------------------------------------------------------/
    // Methods
    //------------------------------------------------------------------------/
    public ObjectDropdownList(List<T> list, T initial = null)
    {
      this.list = list;
      isList = true;
      displayedOptions = list.Names();

      if (initial)
        SetIndex(initial);
    }

    public ObjectDropdownList(List<T> list, int index = 0)
    {
      this.list = list;
      isList = true;
      displayedOptions = list.Names();
      selectedIndex = 0;
    }

    public ObjectDropdownList(T[] array, T initial = null)
    {
      this.array = array;
      isList = false;
      displayedOptions = array.Names();

      if (initial)
        SetIndex(initial);
    }

    public ObjectDropdownList(T[] array, int index = 0)
    {
      this.array = array;
      isList = false;
      displayedOptions = array.Names();
      selectedIndex = 0;
    }

    /// <summary>
    /// Sets the current index of this list to that of the given element
    /// </summary>
    /// <param name="element"></param>
    public void SetIndex(T element)
    {
      if (isList)
        selectedIndex = list.FindIndex(x => x == element);
      else
        selectedIndex = array.FindIndex(x => x == element);
    }

  }
}