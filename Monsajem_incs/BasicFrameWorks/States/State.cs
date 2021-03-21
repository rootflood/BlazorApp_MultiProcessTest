using System;

namespace States
{
    public abstract class State
    {
        public bool BackLock=false;
        public bool NextLock = false;
        State NextState;
        State BackState;


        public void Next(State NextState)
        {
            if (NextLock)
                return;
            this.NextState = NextState;
            NextState.BackState = this;
            while (NextState.NextState != null)
                NextState = NextState.NextState;
            NextState.Focuse();
        }

        public void Replace(State ReplaceState)
        {
            if (NextLock)
                return;
            BackState.NextState = ReplaceState;
            ReplaceState.BackState = BackState;

            while (ReplaceState.NextState != null)
                ReplaceState = ReplaceState.NextState;
            ReplaceState.Focuse();
        }

        public virtual void Back()
        {
            if (BackLock)
                return;
            var BackState = this.BackState;
            this.BackState.NextState = null;
            this.BackState = null;
            BackState.Focuse();
        }

        public State Route()
        {
            State RoutedItem = this;
            while (RoutedItem.NextState != null)
                RoutedItem = RoutedItem.NextState;
            return RoutedItem;
        }
        public abstract void Focuse();
    }
}
