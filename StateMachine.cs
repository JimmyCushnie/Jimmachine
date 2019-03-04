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
        private IDictionary<TState, Action> OnRun = new Dictionary<TState, Action>();

        public StateMachine(TState initialState = default)
        {
            this.CurrentState = initialState;
        }

        public Configurer Configure(TState state) => new Configurer(state, this);

        public void Fire(TTrigger trigger)
        {
            foreach (var permit in Permits)
            {
                if (permit.Original.Equals(CurrentState) && permit.Trigger.Equals(trigger))
                {
                    permit.Execute?.Invoke();
                    this.CurrentState = permit.NextState;

                    break;
                }
            }
        }

        public void RunCurrentState(TState state)
        {
            OnRun.TryGetValue(state, out var action);
            action?.Invoke();
        }

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
                Machine.OnRun[State] = action;
                return this;
            }

            public Configurer Permit(TTrigger trigger, TState changeTo, Action execute = null)
            {
                Machine.Permits.Add(new Permit(State, trigger, changeTo, execute));
            }
        }
    }
}