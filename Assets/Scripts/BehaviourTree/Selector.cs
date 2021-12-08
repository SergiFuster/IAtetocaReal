using System.Collections.Generic;

namespace BehaviorTree
{
    public class Selector : TNode
    {
        public Selector() : base() { }
        public Selector(List<TNode> children) : base(children) { }

        public override NodeState Evaluate()
        {
            foreach (TNode node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.FAILURE:
                        continue;
                    case NodeState.SUCCESS:
                        _state = NodeState.SUCCESS;
                        return _state;
                    case NodeState.RUNNING:
                        _state = NodeState.RUNNING;
                        return _state;
                    default:
                        continue;
                }
            }
            _state = NodeState.FAILURE;
            return _state;
        }
    }
}
