namespace Monster.Ability
{
    // Ability 베이스 클래스: 재사용 가능한 단일 기능을 제공
    // 원칙:
    //   - 순수 기능만 수행
    //   - 상태 전환 명령 금지
    //   - 다른 Ability 직접 참조 금지
    public abstract class EntityAbility
    {
        protected AI.MonsterController _controller;

        // Ability 초기화 (Controller에서 호출)
        public virtual void Initialize(AI.MonsterController controller)
        {
            _controller = controller;
        }

        // 매 프레임 업데이트가 필요한 경우 구현
        public virtual void Update() { }

        // Ability 활성화
        public virtual void OnEnable() { }

        // Ability 비활성화
        public virtual void OnDisable() { }
    }
}