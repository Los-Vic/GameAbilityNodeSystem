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
                OnUnitDestroy = OnPlayUnitDestroyCue,
                OnPlayEffectCue = OnPlayEffectCue,
                OnStopEffectCue = OnStopEffectCue,
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
            if (!_gameAbilitySystem.GetRscFromHandler(context.UnitHandler, out var unit))
                return;
            if (context.AttributeType != ESimpleAttributeType.None)
            {
                var attribute = unit.GetSimpleAttribute(context.AttributeType);
                _gameAbilitySystem.PlayAttributeOnChangeCue(attribute, new AttributeChangeForCue()
                {
                    OldVal = context.OldVal,
                    NewVal = context.NewVal,
                });
            }
            else
            {
                var attribute = unit.GetCompositeAttribute(context.CompositeAttributeType);
                _gameAbilitySystem.PlayAttributeOnChangeCue(attribute, new AttributeChangeForCue()
                {
                    OldVal = context.OldVal,
                    NewVal = context.NewVal,
                });
            }
        }
        
        private void OnStopEffectCue(StopEffectFxCueContext obj)
        {
        }

        private void OnPlayEffectCue(PlayEffectFxCueContext obj)
        {
        }

    }
}