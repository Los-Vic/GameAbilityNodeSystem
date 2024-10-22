namespace NodeSystem
{
    public class NodeSystemNodeRunner
    {
        public static readonly NodeSystemNodeRunner DefaultRunner = new();

        public virtual void InitNode(NodeSystemNode nodeAsset)
        {
            
        }

        public virtual void ExecuteNode(float dt = 0)
        {
            
        }
    }
}