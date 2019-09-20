using System;
using System.Collections.Generic;
using TM;

namespace TM
{
    public class Logic : ILogic
    {
        private WrappersCollection collection;

        public void SetCollection(WrappersCollection collection)
        {
            this.collection = collection;
        }

        public void Initialize()
        {
            
        }

        public void Update()
        {
            if (!((dynamic) collection.Get(3)).IsPressed()) 
            {
              ((dynamic) collection.Get(2)).TurnOn();
              ((dynamic) collection.Get(4)).Text = "hello!";
            } 
            else 
            {
              ((dynamic) collection.Get(2)).TurnOff();
              ((dynamic) collection.Get(4)).Text = "goodby!";
            }
            
            if (((dynamic) collection.Get(1)).IsPressed()) {
              ((dynamic) collection.Get(5)).ChangePose();
            }
        }

        public void Events()
        {
            
        }
    }
}
