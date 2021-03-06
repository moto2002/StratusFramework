﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stratus;
using System;

namespace Genitus
{
  public abstract partial class CombatController<CharacterType> : CombatController 
    where CharacterType : Character//<AttributesType>
  {
    //------------------------------------------------------------------------/
    // Fields
    //------------------------------------------------------------------------/
    /// <summary>
    /// The character used by this controller
    /// </summary>
    [Tooltip("The character used by this controller")]
    public CharacterType character;

    //------------------------------------------------------------------------/
    // Properties
    //------------------------------------------------------------------------/
    /// <summary>
    /// Whether this controller can currently receive damage
    /// </summary>
    public bool isReceivingDamage => !(invulnerable || this.health.current <= 0f);
    /// <summary>
    /// The health of this character. When reached 0, the character is considered KO.
    /// </summary>
    public AttributeInstance health { get; private set; }
    /// <summary>
    /// Represents the innate defense bonus, reducing incoming damage
    /// </summary>
    public AttributeInstance defense { get; private set; }

    //------------------------------------------------------------------------/
    // Messages
    //------------------------------------------------------------------------/
    protected override void Subscribe()
    {
      base.Subscribe();
    }

    /// <summary>
    /// Restores this character's health and removes all negative effects
    /// </summary>
    protected override void OnRestore()
    {
      float heal = this.health.maximum;
      var healEvent = new Combat.HealEvent();
      healEvent.value = heal;
      this.gameObject.Dispatch<Combat.HealEvent>(healEvent);
      this.onRestore();
    }

    /// <summary>
    /// Handles damage logic
    /// </summary>
    /// <param name="value"></param>
    protected override bool OnDamage(float value)
    {
      // Calculate damage after defense
      var damage = value - this.defense.maximum;

      // If there is damage to be taken
      if (damage > 0)
      {
        float healthPercentageLost = this.health.Reduce(damage);
        var damageReceived = new DamageReceivedEvent();
        damageReceived.damage = damage;
        damageReceived.healthPercentageLost = healthPercentageLost;
        this.gameObject.Dispatch<DamageReceivedEvent>(damageReceived);

        // If the damage is exceeded 
        if (this.health.current <= 0f)
        {
          this.Incapacitate();
        }

        if (this.logging)
          StratusDebug.Log("Received " + damage + " damage! Now at " + this.health.current + " health!", this);

        // Announce that health has been modified
        this.gameObject.Dispatch<HealthModifiedEvent>(new HealthModifiedEvent(this));

        return true;
      }

      // If no damage was taken
      this.gameObject.Dispatch<DamageBlockedEvent>(new DamageBlockedEvent());
      return false;
    }

    protected override void OnHeal(float value)
    {
      this.health.Add(value);      
    }

    protected override void OnInvulnerable(bool toggle)
    {
      this.invulnerable = toggle;
      if (this.invulnerable)
        this.defense.SetModifier(1000);
      else
        this.defense.ClearModifiers();
    }

  }

}