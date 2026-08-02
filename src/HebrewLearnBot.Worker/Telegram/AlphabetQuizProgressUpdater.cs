using HebrewLearnBot.Application.Quiz;

namespace HebrewLearnBot.Worker.Telegram;

// Общая логика обновления счётчиков FSM буквенного квиза: используется и после ответа
// (CallbackHandlers), и после пропуска вопроса (MessageHandlers).
public static class AlphabetQuizProgressUpdater
{
    public static AlphabetQuizConversationData Update(AlphabetQuizConversationData data, bool isCorrect)
    {
        var recentLetterIds = new List<int>(data.RecentLetterIds) { data.LetterId };
        if (recentLetterIds.Count > QuizConstants.RecentWindowSize)
        {
            recentLetterIds.RemoveRange(0, recentLetterIds.Count - QuizConstants.RecentWindowSize);
        }

        var wrongLetterIds = new List<int>(data.WrongLetterIds);
        if (!isCorrect && wrongLetterIds.Count < QuizConstants.MaxWrongWordsTracked)
        {
            wrongLetterIds.Add(data.LetterId);
        }

        return new AlphabetQuizConversationData
        {
            QuestionToken = data.QuestionToken,
            QuestionMessageId = data.QuestionMessageId,
            LetterId = data.LetterId,
            CorrectIndex = data.CorrectIndex,
            OptionLetterIds = data.OptionLetterIds,
            OptionLabels = data.OptionLabels,
            AskedCount = data.AskedCount + 1,
            CorrectCount = data.CorrectCount + (isCorrect ? 1 : 0),
            RecentLetterIds = recentLetterIds,
            WrongLetterIds = wrongLetterIds
        };
    }
}
