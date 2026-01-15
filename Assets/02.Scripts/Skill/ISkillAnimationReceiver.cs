namespace Skill
{
    public interface ISkillAnimationReceiver
    {
        void OnSkillDamageFrame();
        void OnSkillComplete();
        void StartSkillTrail();
        void StopSkillTrail();
    }
}
