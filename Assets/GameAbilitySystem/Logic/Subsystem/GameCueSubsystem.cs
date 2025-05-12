using System;

namespace GAS.Logic
{
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

        #region Attribute Val Change

        public void RegisterPlayAttributeValChange(object obj, Action<PlayAttributeValChangeCueContext> onChange)
        {
            _playAttributeValChangeCueObservable.RegisterObserver(obj, onChange);
        }

        public void UnregisterPlayAttributeValChange(object obj)
        {
            _playAttributeValChangeCueObservable.UnRegisterObserver(obj);
        }

        internal void PlayAttributeValChangeCue(ref PlayAttributeValChangeCueContext context)
        {
            _playAttributeValChangeCueObservable.NotifyObservers(context);
        }

        #endregion

        #region Ability Fx

        public void RegisterAbilityFxObservables(object obj, Action<PlayAbilityFxCueContext> onPlay, Action<StopAbilityFxCueContext> onStop)
        {
            _playAbilityFxCueObservable.RegisterObserver(obj, onPlay);
            _stopAbilityFxCueObservable.RegisterObserver(obj, onStop);
        }

        public void UnregisterAbilityFxObservables(object obj)
        {
            _playAbilityFxCueObservable.UnRegisterObserver(obj);
            _stopAbilityFxCueObservable.UnRegisterObserver(obj);
        }

        public void PlayAbilityFxCue(ref PlayAbilityFxCueContext context)
        {
            _playAbilityFxCueObservable.NotifyObservers(context);
        }

        public void StopAbilityFxCue(ref StopAbilityFxCueContext context)
        {
            _stopAbilityFxCueObservable.NotifyObservers(context);
        }

        #endregion

        #region Unit Create/Destroy

        public void RegisterUnitCreateDestroyObservables(object obj, Action<UnitCreateCueContext> onCreate, Action<UnitDestroyCueContext> onDestroy)
        {
            _unitCreateCueObservable.RegisterObserver(obj, onCreate);
            _unitDestroyCueObservable.RegisterObserver(obj, onDestroy);
        }

        public void UnregisterUnitCreateDestroyObservables(object obj)
        {
            _unitCreateCueObservable.UnRegisterObserver(obj);
            _unitDestroyCueObservable.UnRegisterObserver(obj);
        }

        public void PlayUnitCreateCue(ref UnitCreateCueContext context)
        {
            _unitCreateCueObservable.NotifyObservers(context);
        }

        public void PlayUnitDestroyCue(ref UnitDestroyCueContext context)
        {
            _unitDestroyCueObservable.NotifyObservers(context);
        }

        #endregion
      
    }
}