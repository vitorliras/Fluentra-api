namespace Fluentra.Domain.Entities.Shadowing;

public sealed class ScenePracticeProgress
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int SceneId { get; private set; }
    public double AccuracyRate { get; private set; }
    public bool Passed { get; private set; }
    public string EvaluationJson { get; private set; } = null!;
    public DateTime UpdatedAtUtc { get; private set; }

    protected ScenePracticeProgress()
    {
    }

    public ScenePracticeProgress(int userId, int sceneId, double accuracyRate, bool passed, string evaluationJson)
    {
        UserId = userId;
        SceneId = sceneId;
        AccuracyRate = accuracyRate;
        Passed = passed;
        EvaluationJson = evaluationJson;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateEvaluation(double accuracyRate, bool passed, string evaluationJson)
    {
        AccuracyRate = accuracyRate;
        Passed = passed;
        EvaluationJson = evaluationJson;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
