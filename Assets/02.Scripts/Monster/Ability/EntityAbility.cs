using Common;

namespace Monster.Ability
{
    // Ability 베이스 클래스: 재사용 가능한 단일 기능을 제공
    // Monster와 Boss에서 공통으로 사용 가능
    public abstract class EntityAbility
    {
        protected IEntityController _controller;

        public virtual void Initialize(IEntityController controller)
        {
            _controller = controller;
        }

        public virtual void Update() { }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }
    }
}