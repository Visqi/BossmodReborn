﻿using System;

namespace BossMod
{
    // base for boss modules - provides all the common features, so that look is standardized
    public class BossModule : IDisposable
    {
        public WorldState WorldState { get; init; }
        public StateMachine StateMachine { get; init; } = new();
        public StateMachine.State? InitialState = null;
        public MiniArena Arena { get; init; } = new();

        public BossModule(WorldState w)
        {
            WorldState = w;
            WorldState.PlayerInCombatChanged += EnterExitCombat;
            WorldState.ActorCreated += OnActorCreated;
            WorldState.ActorDestroyed += OnActorDestroyed;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                WorldState.PlayerInCombatChanged -= EnterExitCombat;
                WorldState.ActorCreated -= OnActorCreated;
                WorldState.ActorDestroyed -= OnActorDestroyed;
            }
        }

        public virtual void Draw(float cameraAzimuth)
        {
            StateMachine.Update();
            StateMachine.Draw();
        }

        protected virtual void ActorCreated(WorldState.Actor actor) { }
        protected virtual void ActorDestroyed(WorldState.Actor actor) { }
        protected virtual void Reset() { }

        private void EnterExitCombat(object? sender, bool inCombat)
        {
            Reset();
            StateMachine.ActiveState = inCombat ? InitialState : null;
        }

        private void OnActorCreated(object? sender, WorldState.Actor actor)
        {
            ActorCreated(actor);
        }

        private void OnActorDestroyed(object? sender, WorldState.Actor actor)
        {
            ActorDestroyed(actor);
        }
    }
}
