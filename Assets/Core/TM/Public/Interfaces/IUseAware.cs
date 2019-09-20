﻿namespace TM.Public
{
    public interface IUseStartAware
    {
        void OnUseStart(UsingContext context);
    }

    public interface IUseEndAware
    {
        void OnUseEnd();
    }
}
