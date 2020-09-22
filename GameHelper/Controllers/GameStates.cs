﻿// <copyright file="GameStates.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Controllers
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.RemoteMemoryObjects;
    using GameHelper.RemoteMemoryObjects.States;
    using GameOffsets.Controllers;
    using GameOffsets.Native;

    /// <summary>
    /// Reads and stores the global states of the game.
    /// </summary>
    public class GameStates : ControllerBase
    {
        /// <summary>
        /// Gets a dictionary containing all the Game States addresses.
        /// </summary>
        public Dictionary<string, IntPtr> AllStates
        {
            get;
            private set;
        }

        = new Dictionary<string, IntPtr>();

        /// <summary>
        /// Gets the AreaLoadingState object.
        /// </summary>
        public AreaLoadingState AreaLoading
        {
            get;
            private set;
        }

        = new AreaLoadingState(IntPtr.Zero);

        /// <summary>
        /// Gets the InGameState Object.
        /// </summary>
        public InGameState InGameStateObject
        {
            get;
            private set;
        }

        = new InGameState(IntPtr.Zero);

        /// <summary>
        /// Gets the current state the game is in.
        /// </summary>
        internal CurrentState CurrentStateInGame
        {
            get;
            private set;
        }

        = new CurrentState(default);

        /// <inheritdoc/>
        protected override void OnAddressUpdated(IntPtr newAddress)
        {
            if (newAddress != IntPtr.Zero)
            {
                this.UpdateAllStates();
            }
            else
            {
                this.ClearKnownStatesObjects();
            }

            CoroutineHandler.RaiseEvent(this.OnControllerReady);
        }

        /// <summary>
        /// This function Updates the states addresses.
        /// </summary>
        private void UpdateAllStates()
        {
            var reader = Core.Process.Handle;
            var staticAddressData = reader.ReadMemory<GameStateStaticObject>(this.Address);
            var gameStateData = reader.ReadMemory<GameStateObject>(staticAddressData.GameState);
            var states = reader.ReadStdMap<StdWString, IntPtr>(gameStateData.States);
            foreach (var state in states)
            {
                string name = reader.ReadStdWString(state.Key);
                this.UpdateKnownStatesObjects(name, state.Value);
                if (this.AllStates.ContainsKey(name))
                {
                    this.AllStates[name] = state.Value;
                }
                else
                {
                    this.AllStates.Add(name, state.Value);
                }
            }

            this.CurrentStateInGame.Address = staticAddressData.GameState;
        }

        /// <summary>
        /// Updates the known states RemoteMemoryObjects and silently skips the unknown ones.
        /// </summary>
        /// <param name="name">State name.</param>
        /// <param name="address">State address.</param>
        private void UpdateKnownStatesObjects(string name, IntPtr address)
        {
            switch (name)
            {
                case "AreaLoadingState":
                    this.AreaLoading.Address = address;
                    break;
                case "InGameState":
                    this.InGameStateObject.Address = address;
                    break;
                default:
                    break;
            }
        }

        private void ClearKnownStatesObjects()
        {
            this.AreaLoading.Address = IntPtr.Zero;
            this.CurrentStateInGame.Address = IntPtr.Zero;
        }
    }
}
