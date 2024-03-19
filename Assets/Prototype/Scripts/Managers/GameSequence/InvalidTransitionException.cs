namespace RPG.Managers.GameSequence
{
    public class InvalidTransitionException : System.Exception
    {
        public InvalidTransitionException(string message) : base(message) { }
    }
}
