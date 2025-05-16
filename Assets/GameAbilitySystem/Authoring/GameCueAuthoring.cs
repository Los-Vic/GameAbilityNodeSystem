using GAS.Logic;

namespace GAS
{
    public class GameCueAuthoring
    {
        private readonly GameAbilitySystem _gameAbilitySystem;
        public GameCueAuthoring(GameAbilitySystem abilitySystem)
        {
            _gameAbilitySystem = abilitySystem;
        }

        public void Init()
        {
            _gameAbilitySystem.RegisterCueObservables(this, new RegisterCueParam()
            {
                OnAttributeChange = OnAttributeChange,
                OnPlayAbilityCue = OnPlayAbilityCue,
                OnStopAbilityCue = OnStopAbilityCue,
                OnUnitCreate = OnPlayUnitCreateCue,
                OnUnitDestroy = OnPlayUnitDestroyCue
            });
        }

        public void UnInit()
        {
            _gameAbilitySystem.UnregisterCueObservables(this);
        }

        private void OnPlayUnitDestroyCue(UnitDestroyCueContext obj)
        {
        }

        private void OnPlayUnitCreateCue(UnitCreateCueContext obj)
        {
        }

        private void OnStopAbilityCue(StopAbilityFxCueContext obj)
        {
        }

        private void OnPlayAbilityCue(PlayAbilityFxCueContext obj)
        {
        }

        private void OnAttributeChange(PlayAttributeValChangeCueContext context)
        {
            //forward event directly
            var unit = _gameAbilitySystem.GetGameUnitByInstanceID(context.UnitInstanceID);
            if (context.AttributeType != ESimpleAttributeType.None)
            {
                var attribute = unit.GetSimpleAttribute(context.AttributeType);
                attribute.OnPlayValChangeCue.NotifyObservers(new AttributeChangeForCue()
                {
                    OldVal = context.OldVal,
                    NewVal = context.NewVal,
                });
            }
            else
            {
                var attribute = unit.GetCompositeAttribute(context.CompositeAttributeType);
                attribute.OnPlayValChangeCue.NotifyObservers(new AttributeChangeForCue()
                {
                    OldVal = context.OldVal,
                    NewVal = context.NewVal,
                });
            }
        }
    }
}