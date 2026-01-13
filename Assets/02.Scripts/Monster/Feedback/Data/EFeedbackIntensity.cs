namespace Monster.Feedback.Data
{
    // 피드백 강도 레벨 정의
    // 피격 유형에 따라 다른 강도의 피드백 적용
    public enum EFeedbackIntensity
    {
        Light,      // 약한 타격, 도트 데미지
        Normal,     // 일반 공격
        Critical,   // 크리티컬 히트
        Heavy,      // 강공격, 차지 공격
        Death       // 사망
    }
}
