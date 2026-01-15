namespace Boss.Ability
{
    // Boss Ability 베이스 클래스: 재사용 가능한 단일 기능을 제공
    public abstract class BossAbility
    {
        protected AI.BossController _controller;

        public virtual void Initialize(AI.BossController controller)
        {
            _controller = controller;
        }

        public virtual void Update() { }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }
    }
}
