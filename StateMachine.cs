using System;
using System.Collections.Generic;

namespace Jimmachine
{
    public class StateMachine<TState, TTrigger>
        where TState : Enum
        where TTrigger : Enum
    {
        public TState CurrentState { get; private set; }

        private IList<Permit> Permits = new List<Permit>();
        private IDictionary<TState, Action> StateRunActions = new Dictionary<TState, Action>();

        public StateMachine(TState initialState = default)
        {
            this.CurrentState = initialState;
        }

        private List<TState> AlreadyConfigured = new List<TState>();
        /// <summary> Set the state's OnRun method and the Triggers that can be used on it </summary>
        public Configurer Configure(TState state)
        {
            if (AlreadyConfigured.Contains(state)) throw new Exception($"already configured state {state}!");
            AlreadyConfigured.Add(state);

            return new Configurer(state, this);
        }

        /// <summary> returns true if the state was changed </summary>
        public bool Fire(TTrigger trigger)
        {
            foreach (var permit in Permits)
            {
                if (permit.Original.Equals(CurrentState) && permit.Trigger.Equals(trigger))
                {
                    permit.Execute?.Invoke();
                    this.CurrentState = permit.NextState;

                    CurrentStateRunAction = 
                        StateRunActions.TryGetValue(permit.NextState, out var action) ? action : null;

                    return true;
                }
            }

            return false;
        }

        private Action CurrentStateRunAction;
        /// <summary> call this every frame/tick/whatever </summary>
        public void RunCurrentState()
            => CurrentStateRunAction?.Invoke();

        private struct Permit
        {
            public TState Original;
            public TTrigger Trigger;
            public TState NextState;
            public Action Execute;

            public Permit(TState original, TTrigger trigger, TState nextState, Action execute)
            {
                this.Original = original;
                this.Trigger = trigger;
                this.NextState = nextState;
                this.Execute = execute;
            }
        }

        public class Configurer
        {
            private readonly TState State;
            private readonly StateMachine<TState, TTrigger> Machine;

            internal Configurer(TState state, StateMachine<TState, TTrigger> machine)
            {
                this.State = state;
                this.Machine = machine;
            }

            public Configurer OnRun(Action action)
            {
                Machine.StateRunActions[State] = action;
                return this;
            }

            public Configurer Permit(TTrigger trigger, TState changeTo, Action execute = null)
            {
                Machine.Permits.Add(new Permit(State, trigger, changeTo, execute));
                return this;
            }
        }
    }
}