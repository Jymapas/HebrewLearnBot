using HebrewLearnBot.Application.Quiz;

namespace HebrewLearnBot.Worker.Telegram;

// Общая логика обновления счётчиков FSM-квиза: используется и после ответа
// (CallbackHandlers), и после пропуска вопроса (MessageHandlers).
public static class QuizProgressUpdater
{
    public static QuizConversationData Update(QuizConversationData data, bool isCorrect)
    {
        var recentWordIds = new List<int>(data.RecentWordIds) { data.WordId };
        if (recentWordIds.Count > QuizConstants.RecentWindowSize)
        {
            recentWordIds.RemoveRange(0, recentWordIds.Count - QuizConstants.RecentWindowSize);
        }

        var wrongWordIds = new List<int>(data.WrongWordIds);
        if (!isCorrect && wrongWordIds.Count < QuizConstants.MaxWrongWordsTracked)
        {
            wrongWordIds.Add(data.WordId);
        }

        return new QuizConversationData
        {
            Mode = data.Mode,
            TopicId = data.TopicId,
            QuestionToken = data.QuestionToken,
            QuestionMessageId = data.QuestionMessageId,
            WordId = data.WordId,
            CorrectIndex = data.CorrectIndex,
            OptionWordIds = data.OptionWordIds,
            OptionLabels = data.OptionLabels,
            AskedCount = data.AskedCount + 1,
            CorrectCount = data.CorrectCount + (isCorrect ? 1 : 0),
            RecentWordIds = recentWordIds,
            WrongWordIds = wrongWordIds
        };
    }
}
