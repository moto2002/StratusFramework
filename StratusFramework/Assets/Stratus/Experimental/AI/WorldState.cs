/******************************************************************************/
/*!
@file   WorldState.cs
@author Christian Sagel
@par    email: c.sagel\@digipen.edu
@par    DigiPen login: c.sagel
*/
/******************************************************************************/
using UnityEngine;
using System.Collections;
using Stratus;
using System;
using System.Collections.Generic;
using System.Text;
using Stratus.Types;

namespace Stratus
{
  namespace AI
  {

    /// <summary>
    /// In order to search the space of actions, the planner needs to present
    /// the state of the world in some way that lets it easily apply the preconditions
    /// and effects of actions. One compact way to represent the state of the world
    /// is with a list of world property symbols that contain an enumarated attribute key,
    /// a value.
    /// </summary>
    [Serializable]
    public class WorldState : SymbolTable
    {
      /// <summary>
      /// Modifies a single symbol of the WorldState
      /// </summary>
      public class ModifySymbolEvent : Stratus.StratusEvent
      {
        public Symbol Symbol;
        public ModifySymbolEvent(Symbol symbol) { Symbol = symbol; }
      }

      /// <summary>
      /// Applies a symbol to this world state.
      /// </summary>
      /// <param name="symbol"></param>
      public void Apply(Symbol symbol)
      {
        var existingSymbol = Find(symbol.key);
        if (existingSymbol != null)
          existingSymbol.value = symbol.value;
        else
        {
          // Make a copy of that other symbol
          symbols.Add(new Symbol(symbol));
        }
      }

      /// <summary>
      /// Applies a change to the given symbol in this world state. 
      /// If not present, it will add it.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="value"></param>
      public void Apply<T>(string key, T value)
      {
        var existingSymbol = Find(key);
        if (existingSymbol != null)
          existingSymbol.value.Set(value);
        else
          symbols.Add(Symbol.Construct(key, value));
      }

      /// <summary>
      /// Checks if this world state fulfills of the other.
      /// Compare all symbols.
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public bool Satisfies(WorldState other)
      {
        foreach (var symbol in other.symbols)
        {
          // Look for a matching symbol
          var matchingSymbol = Find(symbol.key);
          if (matchingSymbol != null)
          {
            // If the symbols were not equal...
            if (!matchingSymbol.value.Compare(symbol.value))
            {
              //Trace.Script(Symbols[symbol.Key].Print() + " is not equal to " + symbol.Value.Print());
              return false;
            }
          }
          // The symbol was not found
          else
          {
            return false;
          }

        }

        // All symbols were a match
        return true;
      }

      /// <summary>
      /// Checks whether this WorldState contains the given symbol with the same value
      /// </summary>
      /// <param name="symbol"></param>
      /// <returns></returns>
      public bool Contains(Symbol symbol)
      {
        var existingSymbol = Find(symbol.key);
        // Look for a matching symbol
        if (existingSymbol != null)
        {
          if (existingSymbol.value.Compare(symbol.value))
            return true;
        }
        return false;
      }

      /// <summary>
      /// Merges the symbols of the other world state with this one. 
      /// It will overwrite matching symbols, and add any not found in the current one.
      /// </summary>
      /// <param name="other"></param>
      public void Merge(WorldState other)
      {
        foreach (var otherSymbol in other.symbols)
        {
          Apply(otherSymbol);
        }
      }

      public WorldState Copy()
      {
        var newState = new WorldState();
        newState = this;
        return newState;
      }

    }
  } 
}