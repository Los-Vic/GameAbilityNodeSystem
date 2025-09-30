using GCL;

namespace Gray.NG
{
    public class NodeGraphManager
    {
        private ClassObjectPool _pool;

        public void Init()
        {
        }

        public void UnInit()
        {
            _pool.Clear();
        }

        public NodeGraphDirector CreateDirector(RuntimeNodeGraph graph)
        {
            return null;
        }
        
    }
}