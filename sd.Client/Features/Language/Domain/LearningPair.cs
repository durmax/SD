namespace sd.Client.Features.Language.Domain;

public readonly record struct LearningPair(string From, string To)
{
    public LearningPair Reverse() => new(To, From);
}
