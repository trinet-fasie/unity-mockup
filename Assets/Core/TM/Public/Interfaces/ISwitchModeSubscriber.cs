﻿namespace TM.Public
{
    public interface ISwitchModeSubscriber
    {
        void OnSwitchMode(GameMode newMode, GameMode oldMode);
    }
}
