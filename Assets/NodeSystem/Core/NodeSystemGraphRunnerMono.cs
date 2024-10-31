using UnityEngine;

namespace NS
{
    public class NodeSystemGraphRunnerMono:MonoBehaviour
    {
        public NodeSystemGraphAsset asset;
        private NodeSystem _nodeSystem;
        private NodeSystemGraphRunner _graphRunner;
        private void Start()
        {
            _nodeSystem = new();
            _nodeSystem.InitSystem();
            _graphRunner = _nodeSystem.RunnerFactory.CreateGraphRunner();
            _graphRunner.Init(_nodeSystem, asset);
            _graphRunner.StartRunner();
        }

        private void Update()
        {
            _graphRunner.UpdateRunner(Time.deltaTime);
        }
    }
}