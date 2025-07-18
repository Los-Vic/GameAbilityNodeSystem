﻿using System;
using GameplayCommonLibrary.Handler;
using MissQ;

namespace GAS.Logic
{
    public enum EGameEventType
    {
       None,
       
       OnPrepareStart,
       OnPostPrepareStart,
       OnBattleStart,
    }

    public enum EGameEventFilter
    {
        None = 0,
        SrcIsOwner = 1,
        SrcIsNotOwner = 2,
        SrcIsSelfUnits = 3,
        SrcIsAllyUnits = 4,
        SrcIsSelfAllyUnits = 5,
        SrcIsRivalUnits = 6
    }

    public enum EGameEventTimePolicy
    {
        Immediate = 0, //立刻发出事件
        NextFrame = 1, //下一帧
    }
    
    public struct GameEventCreateParam
    {
        public EGameEventType EventType;
        public EGameEventTimePolicy TimePolicy;
        public Handler<GameUnit> EventSrcUnit; //nullable
        public Handler<GameAbility> EventSrcAbility; //nullable
        public Handler<GameEffect> EventSrcEffect;  //nullable
        public Handler<GameUnit> EventTargetUnit;   //nullable
        public FP EventValue1;
        public FP EventValue2;
        public FP EventValue3;
        public string EventString;
    }

    public struct GameEventInitParam
    {
        public GameEventCreateParam CreateParam;
        public Handler<GameEventArg>  Handler;
    }
}