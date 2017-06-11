﻿// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2017 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

namespace DaggerfallWorkshop.Game.Questing
{
    /// <summary>
    /// Interface to a quest action. 
    /// </summary>
    public interface IQuestAction
    {
        /// <summary>
        /// The Regex.Match string pattern expected from source input.
        /// Must be provided by action implementor.
        /// </summary>
        string Pattern { get; }
        
        /// <summary>
        /// Returns true if action called SetComplete().
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// Returns true if this action considers itself a trigger condition.
        ///  * Trigger will be checked even on inactive tasks
        ///  * When trigger evaluates true the task will become active
        /// </summary>
        bool IsTriggerCondition { get; }

        /// <summary>
        /// States the task should return out after calling Update().
        /// This is used by UI actions like "say" which need a frame tick to display.
        /// Otherwise these actions all happen in same frame and final action appears on top.
        /// TaskReturn flag will be lowered by Task once acted upon.
        /// Remaining actions will be processed on next task loop - in script order.
        /// May need to revisit this design. If a task is ended elsewhere after TaskReturn
        /// then not all actions might run as intended.
        /// </summary>
        bool TaskReturn { get; set; }

        /// <summary>
        /// Helper to test if source is a match for Pattern.
        /// </summary>
        Match Test(string source);

        /// <summary>
        /// Factory new instance of this action from source line.
        /// Overrides should always call base to set debug source line.
        /// </summary>
        IQuestAction CreateNew(string source, Quest parentQuest);

        /// <summary>
        /// Get action state data to serialize.
        /// </summary>
        object GetSaveData();

        /// <summary>
        /// Restore action state from serialized data.
        /// </summary>
        void RestoreSaveData(object dataIn);

        /// <summary>
        /// Update action activity.
        /// Called once per frame by owning task.
        /// </summary>
        void Update(Task caller);

        /// <summary>
        /// Check trigger condition status.
        /// Allows task to become active when condition returns true.
        /// </summary>
        bool CheckTrigger(Task caller);

        /// <summary>
        /// Sets action as complete so as not to be called again by task.
        /// Used for one-and-done actions.
        /// </summary>
        void SetComplete();
    }

    /// <summary>
    /// Base class template for all quest actions and conditions used by tasks.
    /// Handles some of the boilerplate of IQuestAction.
    /// This class can be used to test and factory new action interfaces from itself.
    /// Actions belong to a task and perform a specific function when task is active and conditions are met.
    /// An action will persist until task terminates.
    /// Think of actions as objects with a lifetime rather than a simple unit of one-shot execution.
    /// For example, the "vengeance" sound played in nighttime Daggerfall is an action that persists until Lysandus is put to rest.
    /// Currently still unclear on when actions get reset (e.g. when does "play sound 10 times" counter get reset?).
    /// </summary>
    public abstract class ActionTemplate : QuestResource, IQuestAction
    {
        bool complete = false;
        bool triggerCondition = false;
        bool taskReturn = false;
        string debugSource;

        public bool IsComplete { get { return complete; } }
        public bool IsTriggerCondition { get { return triggerCondition; } protected set { triggerCondition = value; } }
        public bool TaskReturn { get { return taskReturn; } set { taskReturn = value; } }

        public abstract string Pattern { get; }

        public string DebugSource
        {
            get { return debugSource; }
        }

        public ActionTemplate(Quest parentQuest)
            : base(parentQuest)
        {
        }

        public virtual Match Test(string source)
        {
            return Regex.Match(source, Pattern);
        }

        public virtual IQuestAction CreateNew(string source, Quest parentQuest)
        {
            debugSource = source;

            return this;
        }

        public virtual void Update(Task caller)
        {
        }

        public virtual bool CheckTrigger(Task caller)
        {
            return false;
        }

        public virtual object GetSaveData()
        {
            return string.Empty;
        }

        public virtual void RestoreSaveData(object dataIn)
        {
        }

        public virtual void SetComplete()
        {
            complete = true;
        }
    }
}