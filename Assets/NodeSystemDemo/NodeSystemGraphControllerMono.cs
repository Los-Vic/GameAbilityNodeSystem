using NS;
using Sirenix.OdinInspector;
using UnityEngine;

public class NodeSystemGraphControllerMono:MonoBehaviour
{
    public NodeSystemGraphAsset asset;

    private NodeSystem _system;
    private NodeSystemGraphController _controller;
    
    [BoxGroup("Event")]
    public ENodeEventType eventType;
    [BoxGroup("Event")]
    public NodeEventParam eventParam;
    
    [BoxGroup("Event")]
    [Button("RunGraphWithEvent", ButtonSizes.Large)]
    [GUIColor(0, 1, 0)]
    private void Run()
    {
        if(!Application.isPlaying)
            return;
        _controller.RunGraph(eventType, eventParam);
    }

    private void Start()
    {
        _system = new NodeSystem();
        _system.InitSystem();
        _controller = new NodeSystemGraphController();
        _controller.Init(_system, asset);
    }

    private void Update()
    {
        _controller.UpdateGraphs(Time.deltaTime);
    }

    private void OnDestroy()
    {
        _controller.DeInit();
    }
}
