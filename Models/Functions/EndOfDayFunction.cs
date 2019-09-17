﻿using Models.Core;
using System;

namespace Models.Functions
{
    /// <summary>
    /// Tracks the value of a child function but only refreshes the value at
    /// end of day.
    /// </summary>
    [Serializable]
    public class EndOfDayFunction : Model, IFunction
    {
        /// <summary>
        /// Yesterday's value.
        /// </summary>
        private double value;

        /// <summary>
        /// The child function.
        /// </summary>
        [ChildLink]
        private IFunction child = null;

        /// <summary>
        /// Invoked at the end of each day. Updates the stored value.
        /// </summary>
        [EventSubscribe("EndOfDay")]
        private void OnEndOfDay(object sender, EventArgs args)
        {
            if (Name == "NumberFunction" && (Apsim.Find(this, typeof(Clock)) as Clock).Today.DayOfYear == 51)
            {

            }
            value = child.Value();
        }

        /// <summary>
        /// Returns the the value of a child function as of the end of
        /// yesterday.
        /// </summary>
        /// <param name="arrayIndex">Ignored.</param>
        public double Value(int arrayIndex = -1)
        {
            return value;
        }
    }
}
