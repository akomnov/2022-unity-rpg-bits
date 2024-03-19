namespace RPG.Managers.PersistentManagers.ClientSequences
{
    public abstract class ClientSequenceBase : GameSequenceBase, IClientSequence
    {
        public ClientSequenceBase(IClientSequenceManager manager) : base(manager) { }

        protected new IClientSequenceManager Manager
        {
            get { return (IClientSequenceManager)base.Manager; }
            set { base.Manager = value; }
        }
    }
}
