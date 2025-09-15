using GCL;
using GAS.Logic;
using UnityEngine;

namespace GAS
{
    public class GameUnitProxy:MonoBehaviour
    {
        public Handler<GameUnit> GameUnit { get; private set; }
    }
}