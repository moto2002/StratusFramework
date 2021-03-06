﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Stratus
{
  public static partial class Extensions
  {
    /// <summary>
    /// Clones all the elements of this list, if they are cloneable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="listToClone"></param>
    /// <returns></returns>
    public static List<T> Clone<T>(this List<T> listToClone) where T : ICloneable
    {
      return listToClone.Select(item => (T)item.Clone()).ToList();
    }

    /// <summary>
    /// Shuffles the list using a randomized range based on its size.
    /// </summary>
    /// <typeparam name="T">The type of the list.</typeparam>
    /// <param name="list">A reference to the list.</param>
    /// <remarks>Courtesy of Mike Desjardins #UnityTips</remarks>
    /// <returns>A new, shuffled list.</returns>
    public static List<T> Shuffle<T>(this List<T> list)
    {
      for (int i = 0; i < list.Count; ++i)
      {
        T index = list[i];
        int randomIndex = UnityEngine.Random.Range(i, list.Count);
        list[i] = list[randomIndex];
        list[randomIndex] = index;
      }

      return list;
    }

    /// <summary>
    /// Returns a random element from the list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static T Random<T>(this List<T> list)
    {
      int randomSelection = UnityEngine.Random.Range(0, list.Count);
      return list[randomSelection];
    }

    /// <summary>
    /// Returns a random element from the array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static T Random<T>(this T[] array)
    {
      int randomSelection = UnityEngine.Random.Range(0, array.Length);
      return array[randomSelection];
    }

    /// <summary>
    /// Returns true if the list is empty
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list.</param>
    /// <returns>True if the list is empty, false otherwise</returns>
    public static bool Empty<T>(this List<T> list)
    {
      if (list.Count == 0)
        return true;
      return false;
    }

    /// <summary>
    /// Returns true if the array is empty
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The array.</param>
    /// <returns>True if the array is empty, false otherwise</returns>
    public static bool Empty<T>(this T[] array)
    {
      return array.Length == 0;
    }

    /// <summary>
    /// Returns true if the array is not empty
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The array.</param>
    /// <returns>True if the array is not empty, false otherwise</returns>
    public static bool NotEmpty<T>(this T[] array)
    {
      return array.Length > 0;
    }

    public static bool NotEmpty<T>(this ICollection<T> collection)
    {
      return collection.Count > 0;
    }

    public static bool NotNullOrEmpty<T>(this ICollection<T> collection)
    {
      return collection != null && collection.Count > 0;
    }

    public static bool NotEmpty<T>(this Stack<T> stack)
    {
      return stack.Count > 0;
    }

    /// <summary>
    /// Returns true if the list is not empty
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list.</param>
    /// <returns>True if the list is not empty, false otherwise</returns>
    public static bool NotEmpty<T>(this List<T> list)
    {
      return list.Count > 0;
    }

    public static T Last<T>(this List<T> list)
    {
      return list[list.Count - 1];
    }


    public static void RemoveLast<T>(this List<T> list)
    {
      if (list.NotEmpty())
      {
        list.RemoveAt(list.Count - 1);
      }

    }

    public static T First<T>(this List<T> list)
    {
      return list[0];
    }

    public static T Last<T>(this T[] array)
    {
      return array[array.Length - 1];
    }

    public static T First<T>(this T[] array)
    {
      return array[0];
    }

    public static T FirstOrNull<T>(this T[] array)
    {
      return array.NotEmpty() ? array[0] : default(T);
    }

    public static T FirstOrNull<T>(this List<T> list)
    {
      return list.NotEmpty() ? list[0] : default(T);
    }

    public static void Swap<T>(this IList<T> list, int indexA, int indexB)
    {
      T tmp = list[indexA];
      list[indexA] = list[indexB];
      list[indexB] = tmp;
    }

    public static void Swap<T>(this IList<T> list, T objA, T objB)
    {
      int indexA = list.IndexOf(objA);
      int indexB = list.IndexOf(objB);
      //T tmp = list[indexA];
      list[indexA] = objB;
      list[indexB] = objA;
    }

    /// <summary>
    /// Returns an array of strings, consisting of the names identified on their name property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static string[] Names<T>(this IList<T> list) where T : UnityEngine.Object
    {
      string[] names = new string[list.Count];
      for (int i = 0; i < list.Count; ++i)
        names[i] = list[i].name;
      return names;
    }

    /// <summary>
    /// Returns an array of strings, consisting of the names identified on their name property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static string[] Names<T>(this IList<T> list, Func<T, string> nameFunc)
    {
      string[] names = new string[list.Count];
      for (int i = 0; i < list.Count; ++i)
        names[i] = nameFunc(list[i]);
      return names;
    }

    /// <summary>
    /// Returns an array of strings, consisting of the names identified on their name property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static string[] Names<T>(this T[] array) where T : UnityEngine.Object
    {
      string[] names = new string[array.Length];
      for (int i = 0; i < array.Length; ++i)
        names[i] = array[i].name;
      return names;
    }

    /// <summary>
    /// Returns an array of strings, consisting of the names identified on their name property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static string[] Names<T>(this T[] array, Func<T, string> nameFunc)
    {
      string[] names = new string[array.Length];
      for (int i = 0; i < array.Length; ++i)
        names[i] = nameFunc(array[i]);
      return names;
    }

    /// <summary>
    /// Returns an array of strings, consisting of the names identified on their name property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static string[] TypeNames<T>(this IList<T> list) where T : UnityEngine.Object
    {
      string[] names = new string[list.Count];
      for (int i = 0; i < list.Count; ++i)
        names[i] = list[i].GetType().Name;
      return names;
    }

    /// <summary>
    /// Given a list, returns an array of strings based on the naming function provided
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static string[] ToString<T>(this IList<T> list, Func<T, string> nameFunc)
    {
      string[] names = new string[list.Count];
      for (int i = 0; i < list.Count; ++i)
        names[i] = nameFunc(list[i]);
      return names;
    }

    /// <summary>
    /// Finds the index of the given element in the array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    public static int FindIndex<T>(this T[] array, Predicate<T> match)
    {
      return Array.FindIndex(array, match);
    }

    /// <summary>
    /// Finds the index of the given element in the array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    public static int FindIndex<T>(this T[] array, T value)
    {
      return Array.FindIndex(array, x => x.Equals(value));
    }

    /// <summary>
    /// Adds all elements not already present into the given list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="array"></param>
    public static void AddRangeUnique<T>(this List<T> list, T[] array)
    {
      list.AddRange(array.Where(x => !list.Contains(x)));
    }

    //public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
    //{
    //  if (list is List<T>)
    //  {
    //    (list as List<T>)?.AddRange(items);
    //  }
    //  else
    //  {
    //    foreach (var item in items)
    //    {
    //      list.Add(item);
    //    }
    //  }
    //}

    //public static bool Contains<T>(this IList<T> list, Func<T, bool> predicate)
    //{
    //  foreach(var element in list)
    //  {
    //    if (predicate(element))
    //      return true;
    //  }
    //  return false;
    //}

    ///// <summary>
    ///// Adds all elements not already present into the given list
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="list"></param>
    ///// <param name="array"></param>
    //public static void AddRangeUnique<T>(this IList<T> list, T[] array)
    //{
    //  list.AddRange(array.Where(x => !list.Contains(x)));
    //}
       
    /// <summary>
    /// Returns a new array concantenating both arrays
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static T[] Concat<T>(this T[] first, T[] second)
    {
      return Enumerable.Concat(first, second).ToArray();
    }

    /// <summary>
    /// Removes all null values from this list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void RemoveNull<T>(this List<T> list)
    {
      list.RemoveAll(x => x == null || x.Equals(null));
    }

    /// <summary>
    /// Checks whether this list has elements with duplicate keys, given a function
    /// that extracts the key for each element
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static bool HasDuplicateKeys<T>(this List<T> list, Func<T, string> keyFunction)
    {
      Dictionary<string, T> keyValuePairs = new Dictionary<string, T>();
      foreach (T element in list)
      {
        string key = keyFunction(element);
        if (keyValuePairs.ContainsKey(key))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Returns the first element in this list that has a duplicate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static T FindFirstDuplicate<T>(this List<T> list, Func<T, string> keyFunction)
    {
      Dictionary<string, T> keyValuePairs = new Dictionary<string, T>();
      foreach (T element in list)
      {
        string key = keyFunction(element);
        if (keyValuePairs.ContainsKey(key))
          return element;
      }
      return default(T);
    }

    /// <summary>
    /// Returns an array of strings, consisting of the names identified on their name property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array">The given array</param>
    /// <param name="filter">An array of names to omit</param>
    /// <returns></returns>
    public static string[] TypeNames<T>(this T[] array, string[] filter = null)
    {
      string[] names = new string[array.Length];
      for (int i = 0; i < array.Length; ++i)
      {

        names[i] = array[i].GetType().Name;
      }
      return names;
    }


    /// <summary>
    /// Filters the left array with the contents of the right one. It will return a new
    /// array that omits elements from this array present in the other one.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static T[] Filter<T>(this T[] array, T[] filter)
    {
      T[] result = array.Where(x => filter.Contains(x)).ToArray();
      return result;
    }

    /// <summary>
    /// Finds the first element of this array that matches the predicate function
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static T FindFirst<T>(this T[] array, Func<T, bool> predicate)
    {
      foreach (var element in array)
      {
        if (predicate(element))
          return element;
      }
      return default(T);
    }

    /// <summary>
    /// Adds the element if it's not null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static bool AddIfNotNull<T>(this IList<T> list, T item)
    {
      if (item != null)
      {
        list.Add(item);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Adds the items from another list, except null ones
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static bool AddRangeNotNull<T>(this IList<T> list, IList<T> other) where T : class
    {
      bool foundNull = false;
      foreach(var item in other)
      {
        foundNull |= !(AddIfNotNull(list, item));
      }
      return foundNull;
    }

    public static void AddRangeFiltered<T>(this IList<T> list, IList<T> other, Predicate<T> predicate)
    {
      foreach (var item in other)
      {
        bool valid = predicate(item);
        if (valid)
          list.Add(item);
      }
    }

    public static void AddRangeFiltered<T>(this IList<T> list, IList<T> other, Func<T, bool> predicate)
    {
      AddRangeFiltered(list, other, ConvertToPredicate(predicate));
    }

    public static bool HasIndex<T>(this IList<T> list, int index)
    {
      return (index >= 0) && ((list.Count - 1) >= index);
    }

    public static T AtIndexOrDefault<T>(this IList<T> list, int index)
    {
      return list.HasIndex(index) ? list[index] : default(T);
    }

    public static T AtIndexOrDefault<T>(this IList<T> list, int index, T defaultValue)
    {
      return list.HasIndex(index) ? list[index] : defaultValue;
    }

    /// <summary>
    /// Iterates over the given list, removing any invalid elements (described hy the validate functon)
    /// Returns true if any elements were removed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="predicate"></param>
    /// <param name="iterateFunction"></param>
    public static bool IterateAndRemoveInvalid<T>(this List<T> list, System.Action<T> iterateFunction, Predicate<T> predicate)
    {
      bool removed = false;
      List<T> invalid = new List<T>();
      foreach (var element in list)
      {
        // Remove invalid elements
        bool valid = predicate(element);
        removed |= valid;

        if (!valid)
        {
          invalid.Add(element);
          continue;
        }

        // Apply the iteration function
        iterateFunction(element);
      }

      list.RemoveAll(x => invalid.Contains(x));
      return removed;
    }

    public static bool IterateAndRemoveInvalid<T>(this List<T> list, System.Action<T> iterateFunction, Func<T, bool> predicate)
    {
      return IterateAndRemoveInvalid(list, iterateFunction, ConvertToPredicate(predicate));
    }

    /// <summary>
    /// Iterates over the given list, removing any invalid elements (described hy the validate functon)    
    /// Returns true if any elements were removed
    /// </summary>    
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="predicate"></param>
    /// <param name="iterateFunction"></param>
    public static bool RemoveInvalid<T>(this List<T> list, Predicate<T> predicate)
    {
      bool removed = false;
      List<T> invalid = new List<T>();
      foreach (var element in list)
      {
        // Remove invalid elements
        bool valid = predicate(element);
        removed |= valid;

        if (!valid)
        {
          invalid.Add(element);
        }
      }

      list.RemoveAll(x => invalid.Contains(x));
      return removed;
    }

    public static bool RemoveInvalid<T>(this List<T> list, Func<T, bool> predicate)
    {
      return RemoveInvalid(list, ConvertToPredicate(predicate));
    }

    ///// <summary>
    ///// Iterates over the given list, removing any invalid elements (described hy the validate functon)
    ///// using backwards traversal. Returns true if any elements were removed.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="list"></param>
    ///// <param name="predicate"></param>
    ///// <param name="iterateFunction"></param>
    //public static bool IterateAndRemoveInvalid<T>(this IList<T> list, System.Action<T> iterateFunction, Predicate<T> predicate)
    //{
    //  bool removed = false;
    //  for (int i = list.Count - 1; i >= 0; i--)
    //  {
    //    T element = list[i];
    //    bool valid = predicate(element);
    //    removed |= valid;
    //
    //    if (!valid)
    //    {
    //      list.RemoveAt(i);
    //    }
    //    else
    //    {
    //      iterateFunction(element);
    //    }
    //  }
    //  return removed;
    //}
    //
    //public static bool IterateAndRemoveInvalid<T>(this IList<T> list, System.Action<T> iterateFunction, Func<T, bool> predicate)
    //{
    //  return IterateAndRemoveInvalid(list, iterateFunction, ConvertToPredicate(predicate));
    //}
    //
    ///// <summary>
    ///// Iterates over the given list, removing any invalid elements (described hy the validate functon)
    ///// using backwards traversal. Returns true if any elements were removed.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="list"></param>
    ///// <param name="predicate"></param>
    ///// <param name="iterateFunction"></param>
    //public static bool RemoveInvalid<T>(this IList<T> list, Predicate<T> predicate)
    //{
    //  bool removed = false;
    //  for (int i = list.Count - 1; i >= 0; i--)
    //  {
    //    T element = list[i];
    //    bool valid = predicate(element);
    //    removed |= valid;
    //
    //    if (!valid)
    //    {
    //      list.RemoveAt(i);
    //    }
    //  }
    //  return removed;
    //}
    //
    //public static bool RemoveInvalid<T>(this IList<T> list, Func<T, bool> predicate)
    //{
    //  return RemoveInvalid(list, ConvertToPredicate(predicate));
    //}

    public static T[] GetNonNull<T>(this T[] array)
    {
      List<T> list = new List<T>();
      foreach(var item in array)
      {
        if (item != null)
          list.Add(item);
      }
      return list.ToArray();
    }

    /// <summary>
    /// Copies this array, inserting the element to the front
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="element"></param>
    /// <returns></returns>
    public static T[] AddFront<T>(this T[] array, T element)
    {
      T[] newArray = new T[array.Length + 1];
      newArray[0] = element;
      Array.Copy(array, 0, newArray, 1, array.Length);
      return newArray;
    }

    /// <summary>
    /// Copies this array, inserting the element to the front
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="element"></param>
    /// <returns></returns>
    public static T[] AddBack<T>(this T[] array, T element)
    {
      T[] newArray = new T[array.Length + 1];
      Array.Copy(array, 0, newArray, 0, array.Length);
      newArray[newArray.Length - 1] = element;
      return newArray;
    }

    /// <summary>
    /// Copies the array, without the first element present
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    public static T[] RemoveFirst<T>(this T[] array)
    {
      T[] newArray = new T[array.Length - 1];
      Array.Copy(array, 1, newArray, 0, array.Length - 1);
      return newArray;
    }


    /// <summary>
    /// Copies the array, without the first element present
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    public static T[] RemoveBack<T>(this T[] array)
    {
      T[] newArray = new T[array.Length - 1];
      Array.Copy(array, 0, newArray, 0, array.Length - 1);
      return newArray;
    }

    /// <summary>
    /// Copies the array, without the selected element present
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    public static T[] Remove<T>(this T[] array, T element)
    {
      int elementIndex = array.FindIndex(x => x.Equals(element));


      // If it's the first element
      if (elementIndex == -1)
        return array;
      else if (elementIndex == 0)
        return array.RemoveFirst();
      // If it's the last element
      else if (elementIndex == array.Length)
        return array.RemoveBack();

      T[] newArray = new T[array.Length - 1];
      Array.Copy(array, 0, newArray, 0, elementIndex - 1);
      Array.Copy(array, elementIndex + 1, newArray, 0, array.Length - 1);
      return newArray;
    }

    public static bool Contains<T>(this T[] array, T element)
    {
      foreach(var item in array)
      {
        if (item.Equals(element))
          return true;
      }
      return false;
    }


    public static bool Contains<T>(this T[] array, T element, Func<T, T, bool> comparer)
    {
      foreach (var item in array)
      {
        if (comparer(item, element))
          return true;
      }
      return false;
    }

    public static bool Contains<T>(this T[] array, T element, Comparer<T> comparer)
    {
      foreach (var item in array)
      {
        if (comparer.Compare(item, element) == 0)
          return true;
      }
      return false;
    }

    public static U[] OfType<T, U>(this T[] array)
      where T : class
      where U : class, T
    {
      return array.Select(c => c as U).Where(c => c != null).ToArray();
    }

    public static Predicate<T> ConvertToPredicate<T>(Func<T, bool> func)
    {
      return new Predicate<T>(func);
    }

  }

}