using System;
using System.Collections.Generic;

namespace Jimmachine
{
    public class StateMachine<TState> where TState : Enum
    {
        public TState CurrentState { get; private set; }

        private IList<Transition> Transitions = new List<Transition>();
        private IDictionary<TState, Action> StateRunActions = new Dictionary<TState, Action>();

        public StateMachine(TState initialState = default)
        {
            this.CurrentState = initialState;
        }

        private List<TState> AlreadyConfigured = new List<TState>();
        /// <summary> Set the state's OnRun method and add transitions to other states </summary>
        public Configurer Configure(TState state)
        {
            if (AlreadyConfigured.Contains(state)) throw new Exception($"already configured state {state}!");
            AlreadyConfigured.Add(state);

            return new Configurer(state, this);
        }

        /// <summary> returns true if the state was changed, false if that was not a configured transition </summary>
        public bool SwitchTo(TState newState)
        {
            foreach (var transition in Transitions)
            {
                if (transition.OriginalState.Equals(CurrentState) && transition.NextState.Equals(newState))
                {
                    transition.Execute?.Invoke();
                    this.CurrentState = transition.NextState;

                    CurrentStateRunAction = 
                        StateRunActions.TryGetValue(transition.NextState, out var action) ? action : null;

                    return true;
                }
            }

            return false;
        }

        private Action CurrentStateRunAction;
        /// <summary> call this every frame/tick/whatever </summary>
        public void RunCurrentState()
            => CurrentStateRunAction?.Invoke();

        private struct Transition
        {
            public TState OriginalState;
            public TState NextState;
            public Action Execute;

            public Transition(TState original, TState nextState, Action execute)
            {
                this.OriginalState = original;
                this.NextState = nextState;
                this.Execute = execute;
            }
        }

        public class Configurer
        {
            public readonly TState State;
            private readonly StateMachine<TState> Machine;

            internal Configurer(TState state, StateMachine<TState> machine)
            {
                this.State = state;
                this.Machine = machine;
            }

            public Configurer OnRun(Action action)
            {
                Machine.StateRunActions[State] = action;

                if (Machine.CurrentState.Equals(State))
                    Machine.CurrentStateRunAction = action;

                return this;
            }

            public Configurer AllowTransitionTo(TState changeTo, Action execute = null)
            {
                Machine.Transitions.Add(new Transition(State, changeTo, execute));
                return this;
            }
        }
    }
}