namespace GameFish;

/// <summary>
/// Add this to stuff that uses a <see cref="SkinnedModelRenderer"/> to animate itself.
/// </summary>
public interface IAnimated
{
    public SkinnedModelRenderer AnimatedModel { get; set; }
}
