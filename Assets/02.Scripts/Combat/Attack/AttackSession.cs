namespace Combat.Attack
{
    public class AttackSession
    {
        private static int _nextId;

        public int Id { get; }
        public int ComboStep { get; }
        public bool IsActive { get; private set; }

        public AttackSession(int comboStep)
        {
            Id = _nextId++;
            ComboStep = comboStep;
            IsActive = true;
        }

        public void Invalidate()
        {
            IsActive = false;
        }
    }
}
