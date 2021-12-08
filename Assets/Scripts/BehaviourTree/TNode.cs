using System.Collections.Generic;

namespace BehaviorTree
{
    public enum NodeState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    public class TNode
    {
        protected NodeState _state;
        public NodeState State { get => _state; }

        private TNode _parent;
        protected List<TNode> children = new List<TNode>();
        private Dictionary<string, object> _dataContext =
            new Dictionary<string, object>();

        public TNode() { _parent = null; }
        public TNode(List<TNode> children) : this()
        {
            SetChildren(children);
        }

        public virtual NodeState Evaluate() => NodeState.FAILURE;

        public void SetChildren(List<TNode> children)
        {
            foreach (TNode c in children)
                Attach(c);
        }

        public void Attach(TNode child)
        {
            children.Add(child);
            child._parent = this;
        }

        public void Detach(TNode child)
        {
            children.Remove(child);
            child._parent = null;
        }

        public object GetData(string key)
        {
            object value = null;
            if (_dataContext.TryGetValue(key, out value))
                return value;

            TNode node = _parent;
            while (node != null)
            {
                value = node.GetData(key);
                if (value != null)
                    return value;
                node = node._parent;
            }
            return null;
        }

        public bool ClearData(string key)
        {
            if (_dataContext.ContainsKey(key))
            {
                _dataContext.Remove(key);
                return true;
            }

            TNode node = _parent;
            while (node != null)
            {
                bool cleared = node.ClearData(key);
                if (cleared)
                    return true;
                node = node._parent;
            }
            return false;
        }

        public void SetData(string key, object value)
        {
            _dataContext[key] = value;
        }

        public TNode Parent { get => _parent; }
        public List<TNode> Children { get => children; }
        public bool HasChildren { get => children.Count > 0; }
    }
}
