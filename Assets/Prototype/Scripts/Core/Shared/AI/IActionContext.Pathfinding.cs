namespace RPG.Core.Shared.AI
{
    public partial interface IActionContext
    {
        float GetShortestDistance(object aGO, object bGO);
        (object path, float undershoot) FindPath(object subjectGO, object toGO);
        bool SetPathDestination(object subjectGO, object targetGO, float stoppingDistance, bool autoRepath);
        bool SetPath(object subjectGO, object precalculatedPath, float stoppingDistance);
        void UnsetPath(object subjectGO);
        bool StoppedPathing(object subjectGO);
    }
}