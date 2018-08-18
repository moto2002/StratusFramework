/******************************************************************************/
/*!
@file   PullEffect.cs
@author Christian Sagel
@par    email: c.sagel\@digipen.edu
@par    DigiPen login: c.sagel
@date   5/25/2016
*/
/******************************************************************************/
using UnityEngine;
using System.Collections.Generic;
using Stratus;
using System;

namespace Genitus 
{
  public class PullEffect : KineticEffectAttribute
  {
    protected override void OnApply(CombatController caster, CombatController target)
    {
      target.gameObject.Dispatch<KineticAction.PullEvent>(new KineticAction.PullEvent(caster.transform, Amount));
    }
  }
}