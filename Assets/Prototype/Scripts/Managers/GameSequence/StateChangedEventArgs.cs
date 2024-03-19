namespace RPG.Managers.GameSequence
{
    public class StateChangedEventArgs : System.EventArgs
    {
        public State oldState;
        public State newState;
    }
}
