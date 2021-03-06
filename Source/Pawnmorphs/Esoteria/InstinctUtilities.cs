﻿// InstinctUtlities.cs modified by Iron Wolf for Pawnmorph on 12/07/2019 1:35 PM
// last updated 12/07/2019  1:35 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     static class containing useful instinct related utilities
    /// </summary>
    public static class InstinctUtilities
    {
        private const float BETA = 50f / 2.0f; //converts 'resistance stat' to sapience
        private const float ALPHA = 50f / 10f; //converts intelligence to sapience
        private const float INSTINCT_MULTIPLIER = 1 / 15f; //scales change in instinct to change in sapience 
        private const int AVERAGE_INT = 3;
        private const float AVERAGE_RESISTANCE_STAT = 0.5f;
        /// <summary>
        ///     The average resistance of pawns
        /// </summary>
        /// use this value to scale control values to a better range
        public const float AVERAGE_MAX_SAPIENCE = AVERAGE_INT * ALPHA + AVERAGE_RESISTANCE_STAT * BETA;


        /// <summary>
        /// a very small value 
        /// </summary>
        public const float EPSILON = 0.001f; 

        [NotNull]
        private static readonly Dictionary<SapienceLevel, string> _labelDict;

        private const float INSTINCT_PER_TICK_SCALAR = 1;
        /// <summary>
        /// Gets the instinct change per tick.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        public static float GetInstinctChangePerTick([NotNull] Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            var sRFactor = -pawn.GetStatValue(PMStatDefOf.SapienceRecoverFactor);
            var netFactor =  sRFactor;
            return INSTINCT_PER_TICK_SCALAR *  netFactor * TimeMetrics.TICK_PERIOD; //TODO what else should influence recover rate? 
        }

        static InstinctUtilities()
        {
            var eType = typeof(SapienceLevel);
            var values = Enum.GetValues(eType).Cast<SapienceLevel>();
            _labelDict = new Dictionary<SapienceLevel, string>(); 
            foreach (SapienceLevel sapienceLevel in values)
            {
                _labelDict[sapienceLevel] = sapienceLevel.ToString().Translate(); 
            }

        }

        /// <summary>
        /// Gets the label.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        public static string GetLabel(this SapienceLevel level)
        {
            return _labelDict[level]; 
        }

        /// <summary>
        ///     Calculates the total control has left
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="instinct">The instinct.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">pawn</exception>
        public static float CalculateControl([NotNull] Pawn pawn, float instinct)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            float res = CalculateNetResistance(pawn);
            float lambda = 1 / pawn.GetStatValue(PMStatDefOf.SapientAnimalA);

            return res - instinct * lambda;
        }

        /// <summary>
        ///     Calculates the change in control caused by the given instinct change.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="instinctChange">The instinct change.</param>
        /// <returns></returns>
        public static float CalculateControlChange([NotNull] Pawn pawn, float instinctChange)
        {
            var div =  pawn.GetStatValue(PMStatDefOf.SapientAnimalA);
            var sign = div < 0 ? -1 : 1;
            div = Mathf.Max(Mathf.Abs(div), EPSILON) * sign; //prevent division by zero but preserve sign  

            return -INSTINCT_MULTIPLIER * instinctChange / (div);
        }

        /// <summary>
        ///     Calculates the net resistance of this pawn
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">pawn</exception>
        public static float CalculateNetResistance([NotNull] Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            int i = pawn.skills?.GetSkill(SkillDefOf.Intellectual)?.Level ?? 0;
            float rs = pawn.GetStatValue(PMStatDefOf.SapientAnimalResistance);
            return i * ALPHA + rs * BETA;
        }

        /// <summary>
        /// The average resistance
        /// </summary>
        public const float AVERAGE_RESISTANCE = AVERAGE_INT * ALPHA + AVERAGE_RESISTANCE_STAT * BETA; 

        /// <summary>
        /// Gets the control need of the given pawn
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        [CanBeNull]
        public static Need_Control GetControlNeed([NotNull] Pawn pawn)
        {
            return pawn.needs?.TryGetNeed<Need_Control>();
        }
    }
}