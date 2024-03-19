namespace RPG.Utils.AI
{
    // @neuro: this is a DTO and a static method namespace, move it into shared
    [System.Serializable]
    public class Statement
    {
        public enum Category {
            HURT_BY,
            LOS_TO,
            EVADING,
            IN_MELEE_RANGE_WITH,
            PATROLLING
        }

        public enum Match
        {
            NO,
            ANY,
            MATCH,
            TAG
        }

        public Category category;
        public Match match;
        public string match_arg1 = "";
    }
}