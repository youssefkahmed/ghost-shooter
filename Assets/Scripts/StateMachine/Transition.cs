using System;

namespace Ghost.StateMachine
{
    public abstract class Transition
    {
        public IState To { get; protected set; }
        public abstract bool Evaluate();
    }

    public class Transition<T> : Transition
    {
        public readonly T Condition;

        public Transition(IState to, T condition)
        {
            To = to;
            Condition = condition;
        }

        public override bool Evaluate()
        {
            // Check if the condition variable is a Func<bool> and call the Invoke method if it is not null
            bool? result = (Condition as Func<bool>)?.Invoke();
            if (result.HasValue)
            {
                return result.Value;
            }
            
            // Check if the condition variable is an ActionPredicate and call the Evaluate method if it is not null
            result = (Condition as ActionPredicate)?.Evaluate();
            if (result.HasValue)
            {
                return result.Value;
            }

            // Check if the condition variable is an IPredicate and call the Evaluate method if it is not null
            result = (Condition as IPredicate)?.Evaluate();
            if (result.HasValue)
            {
                return result.Value;
            }

            // If the condition variable is not a Func<bool>, an ActionPredicate, or an IPredicate, return false
            return false;
        }
    }

    /// <summary>
    /// Represents a predicate that uses a Func delegate to evaluate a condition.
    /// </summary>
    public class FuncPredicate : IPredicate
    {
        private readonly Func<bool> _func;

        public FuncPredicate(Func<bool> func)
        { 
            _func = func;
        }

        public bool Evaluate()
        {
            return _func.Invoke();
        }
    }

    /// <summary>
    /// Represents a predicate that encapsulates an action and evaluates to true once the action has been invoked.
    /// </summary>
    public class ActionPredicate : IPredicate
    {
        public bool Flag;

        public ActionPredicate(ref Action eventReaction)
        {
            eventReaction += () => { Flag = true; };
        }

        public bool Evaluate()
        {
            bool result = Flag;
            Flag = false;
            return result;
        }
    }
}