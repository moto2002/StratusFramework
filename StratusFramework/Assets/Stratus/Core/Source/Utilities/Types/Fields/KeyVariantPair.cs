using UnityEngine;
using System;

namespace Stratus
{
  /// <summary>
  /// A pair between a variant an a generic key
  /// </summary>
  /// <typeparam name="KeyType"></typeparam>
  public class KeyVariantPair<KeyType> where KeyType : IComparable
  {
    //--------------------------------------------------------------------/
    // Fields
    //--------------------------------------------------------------------/
    /// <summary>
    /// The key used for this variant pair
    /// </summary>
    public KeyType key;
    /// <summary>
    /// The variant used by the pair
    /// </summary>
    public Variant value;

    //--------------------------------------------------------------------/
    // Properties
    //--------------------------------------------------------------------/
    /// <summary>
    /// The current type for this variant pair
    /// </summary>
    public Variant.VariantType type { get { return value.currentType; } }

    /// <summary>
    /// Information about the symbol
    /// </summary>
    public string annotation => $"{key} ({value.currentType})";

    //--------------------------------------------------------------------/
    // Constructors
    //--------------------------------------------------------------------/
    public KeyVariantPair(KeyType key, int value) { this.key = key; this.value = new Variant(value); }
    public KeyVariantPair(KeyType key, float value) { this.key = key; this.value = new Variant(value); }
    public KeyVariantPair(KeyType key, bool value) { this.key = key; this.value = new Variant(value); }
    public KeyVariantPair(KeyType key, string value) { this.key = key; this.value = new Variant(value); }
    public KeyVariantPair(KeyType key, Vector3 value) { this.key = key; this.value = new Variant(value); }
    public KeyVariantPair(KeyType key, Variant value) { this.key = key; this.value = new Variant(value); }
    public KeyVariantPair(KeyVariantPair<KeyType> other) { key = other.key; value = new Variant(other.value); }

    //--------------------------------------------------------------------/
    // Methods
    //--------------------------------------------------------------------/
    public ValueType GetValue<ValueType>()
    {
      return value.Get<ValueType>();
    }

    public void SetValue<ValueType>(ValueType value)
    {
      this.value.Set(value);
    }

    public bool Compare(KeyVariantPair<KeyType> other)
    {
      // https://msdn.microsoft.com/en-us/library/system.icomparable(v=vs.110).aspx
      if (this.key.CompareTo(other.key) < 0)
        return false;

      return this.value.Compare(other.value);
    }

    public KeyVariantPair<KeyType> Copy()
    {
      return new KeyVariantPair<KeyType>(key, value);
    }

    public override string ToString()
    {
      return $"{this.key} ({value.ToString()})";
    }
  }
}
