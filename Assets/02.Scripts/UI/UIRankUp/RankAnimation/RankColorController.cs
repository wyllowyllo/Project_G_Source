using UnityEngine;

public class RankColorController : MonoBehaviour
{
    [Header("색상 설정 (등급별)")]
    [SerializeField] private RankColorScheme[] _rankColorSchemes = new RankColorScheme[]
    {
        new RankColorScheme { rank = "C", primaryColor = new Color(0.486f, 0.702f, 0.259f), secondaryColor = new Color(0.333f, 0.545f, 0.184f) },
        new RankColorScheme { rank = "B", primaryColor = new Color(0.784f, 1f, 0f), secondaryColor = new Color(0.620f, 0.792f, 0f) },
        new RankColorScheme { rank = "A", primaryColor = new Color(1f, 0.843f, 0f), secondaryColor = new Color (0.749f, 0.243f, 1f)},
        new RankColorScheme { rank = "S", primaryColor = new Color(1f, 0.431f, 0.780f), secondaryColor = new Color(1f, 0.549f, 0f) }
    };

    [System.Serializable]
    public class RankColorScheme
    {
        public string rank;
        public Color primaryColor;
        public Color secondaryColor;
    }

    // 등급에 맞는 색상가져오기
    public RankColorScheme GetColorScheme(string rank)
    {
        foreach (var scheme in _rankColorSchemes)
        {
            if (scheme.rank == rank)
                return scheme;
        }

        // 기본값 반환
        return new RankColorScheme 
        { 
            rank = "Default", 
            primaryColor = Color.white, 
            secondaryColor = Color.gray 
        };
    }

    // 런타임에 랭크 색상 수정
    public void ApplyRankColors(string rank, Color primary, Color secondary)
    {
        foreach (var scheme in _rankColorSchemes)
        {
            if (scheme.rank == rank)
            {
                scheme.primaryColor = primary;
                scheme.secondaryColor = secondary;
                return;
            }
        }
    }

    //특정 랭크의 Primary 색상 가져오기
    public Color GetPrimaryColor(string rank)
    {
        return GetColorScheme(rank).primaryColor;
    }

    // 특정 랭크의 Secondary 색상 가져오기
    public Color GetSecondaryColor(string rank)
    {
        return GetColorScheme(rank).secondaryColor;
    }
}
