namespace RPG.Core.Shared.AI
{
    public partial interface IActionContext
    {
        void Bark(object subject, string message);
    }
}