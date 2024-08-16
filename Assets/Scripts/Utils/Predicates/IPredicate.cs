using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ghost.StateMachine
{
    public interface IPredicate
    {
        bool Evaluate();
    }

    public class And : IPredicate
    {
        [SerializeField] private List<IPredicate> rules = new();
        
        public bool Evaluate() => rules.All(r => r.Evaluate());
    }

    public class Or : IPredicate
    {
        [SerializeField] private List<IPredicate> rules = new();
        
        public bool Evaluate() => rules.Any(r => r.Evaluate());
    }

    public class Not : IPredicate
    {
        [SerializeField] private IPredicate rule;
        
        public bool Evaluate() => !rule.Evaluate();
    }
}