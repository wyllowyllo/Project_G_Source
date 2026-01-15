namespace Skill
{
    public interface IGlideAnimationReceiver
    {
        void OnJumpExecute();
        void OnGlideTransition();
        void OnGlideComplete();
        void OnLandingComplete();
        void OnDiveBombLandingComplete();
        void OnDiveBombReady();
    }
}
