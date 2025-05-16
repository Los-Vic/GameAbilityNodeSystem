using System;

namespace GAS.Logic
{
    public struct RegisterCueParam
    {
        public Action<PlayAttributeValChangeCueContext> OnAttributeChange;
        public Action<PlayAbilityFxCueContext> OnPlayAbilityCue;
        public Action<StopAbilityFxCueContext> OnStopAbilityCue;
        public Action<UnitCreateCueContext> OnUnitCreate;
        public Action<UnitDestroyCueContext> OnUnitDestroy;
    }
    
    public class GameCueSubsystem:GameAbilitySubsystem
    {
        private readonly Observable<PlayAttributeValChangeCueContext> _playAttributeValChangeCueObservable = new();
        private readonly Observable<PlayAbilityFxCueContext> _playAbilityFxCueObservable = new();
        private readonly Observable<StopAbilityFxCueContext> _stopAbilityFxCueObservable = new();
        private readonly Observable<UnitCreateCueContext> _unitCreateCueObservable = new();
        private readonly Observable<UnitDestroyCueContext> _unitDestroyCueObservable = new();

        public override void UnInit()
        {
            _playAttributeValChangeCueObservable.Clear();
            _playAbilityFxCueObservable.Clear();
            _stopAbilityFxCueObservable.Clear();
            _unitCreateCueObservable.Clear();
            _unitDestroyCueObservable.Clear();
            base.UnInit();
        }

        internal void RegisterCueObservables(object obj, RegisterCueParam param)
        {
            _playAttributeValChangeCueObservable.RegisterObserver(obj, param.OnAttributeChange);
            _playAbilityFxCueObservable.RegisterObserver(obj, param.OnPlayAbilityCue);
            _stopAbilityFxCueObservable.RegisterObserver(obj, param.OnStopAbilityCue);
            _unitCreateCueObservable.RegisterObserver(obj, param.OnUnitCreate);
            _unitDestroyCueObservable.RegisterObserver(obj, param.OnUnitDestroy);
        }

        internal void UnregisterCueObservables(object obj)
        {
            _playAttributeValChangeCueObservable.UnRegisterObserver(obj);
            _playAbilityFxCueObservable.UnRegisterObserver(obj);
            _stopAbilityFxCueObservable.UnRegisterObserver(obj);
            _unitCreateCueObservable.UnRegisterObserver(obj);
            _unitDestroyCueObservable.UnRegisterObserver(obj);
        }
        
        internal void PlayAttributeValChangeCue(ref PlayAttributeValChangeCueContext context)
        {
            _playAttributeValChangeCueObservable.NotifyObservers(context);
        }

        public void PlayAbilityFxCue(ref PlayAbilityFxCueContext context)
        {
            _playAbilityFxCueObservable.NotifyObservers(context);
        }

        public void StopAbilityFxCue(ref StopAbilityFxCueContext context)
        {
            _stopAbilityFxCueObservable.NotifyObservers(context);
        }

        public void PlayUnitCreateCue(ref UnitCreateCueContext context)
        {
            _unitCreateCueObservable.NotifyObservers(context);
        }

        public void PlayUnitDestroyCue(ref UnitDestroyCueContext context)
        {
            _unitDestroyCueObservable.NotifyObservers(context);
        }
    }
}