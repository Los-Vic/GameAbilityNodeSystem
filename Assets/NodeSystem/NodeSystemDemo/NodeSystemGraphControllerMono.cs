using NS;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class NodeSystemGraphControllerMono:MonoBehaviour
{
    public DemoGraphAsset asset;

    private NodeSystem _system;
    private NodeSystemGraphController _controller;
    
    [BoxGroup("Portal")]
    public ENodeDemoPortalType demoPortalType;
    [BoxGroup("Portal")]
    public NodeDemoPortalParam demoPortalParam;
    
    [BoxGroup("Portal")]
    [Button("RunGraphWithPortal", ButtonSizes.Large)]
    [GUIColor(0, 1, 0)]
    private void Run()
    {
        if(!Application.isPlaying)
            return;
        _controller.RunGraph(demoPortalType, demoPortalParam);
    }

    private void Start()
    {
        _system = new DemoNodeSystem();
        _system.InitSystem();
        _controller = new NodeSystemGraphController();
        _controller.Init(_system, asset);
    }

    private void Update()
    {
        _system.UpdateSystem(Time.deltaTime);
    }

    private void OnDestroy()
    {
        _controller.DeInit();
    }
}
