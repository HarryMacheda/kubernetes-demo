using Npgsql;

public class PostgresQuestionRepository : IQuestionRepository
{
    private readonly string _connectionString;

    public PostgresQuestionRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<IQuestion> CreateQuestionAsync(IQuestion question)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO Questions (QuizId, Index, Type, CreatedAt, CreatedBy, QuestionText)
                    VALUES (@QuizId, @Index, @Type, @CreatedAt, @CreatedBy, @QuestionText)
                    RETURNING Id;
                    ";

                command.Parameters.AddWithValue("@QuizId", question.QuizId);
                command.Parameters.AddWithValue("@Index", question.Index);
                command.Parameters.AddWithValue("@Type", question.Type);
                command.Parameters.AddWithValue("@CreatedAt", question.CreatedAt);
                command.Parameters.AddWithValue("@CreatedBy", question.CreatedBy);
                command.Parameters.AddWithValue("@QuestionText", question.QuestionText);

                var result = await command.ExecuteScalarAsync();
                question.Id = Convert.ToInt32(result);
            }
        }

        return question;
    }

    public async Task<IQuestion?> GetQuestionByIdAsync(int id)
    {
        IQuestion question = null;
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT QuizId, Index, Type, CreatedAt, CreatedBy, QuestionText
                    FROM Questions
                    WHERE Id = @Id;";

                command.Parameters.AddWithValue("@Id", id);

                var result = await command.ExecuteReaderAsync();
                if (await result.ReadAsync())
                {
                    question = new Question
                    {
                        Id = result.GetInt32(result.GetOrdinal("Id")),
                        QuizId = result.GetInt32(result.GetOrdinal("QuizId")),
                        Index = result.GetInt32(result.GetOrdinal("Index")),
                        Type = (QuestionType)result.GetInt32(result.GetOrdinal("Type")),
                        CreatedAt = result.GetDateTime(result.GetOrdinal("CreatedAt")),
                        CreatedBy = result.GetGuid(result.GetOrdinal("CreatedBy")),
                        QuestionText = result.GetString(result.GetOrdinal("QuestionText"))
                    };
                }
            }
        }

        return question;
    }

    public async Task<IEnumerable<IQuestion>> GetAllQuestionsForQuizAsync(int quizId)
    {
        var questions = new List<IQuestion>();
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT QuizId, Index, Type, CreatedAt, CreatedBy, QuestionText
                    FROM Questions
                    WHERE QuizId = @Id;";

                command.Parameters.AddWithValue("@Id", quizId);

                var result = await command.ExecuteReaderAsync();
                if (await result.ReadAsync())
                {
                    questions.Add(new Question
                    {
                        Id = result.GetInt32(result.GetOrdinal("Id")),
                        QuizId = result.GetInt32(result.GetOrdinal("QuizId")),
                        Index = result.GetInt32(result.GetOrdinal("Index")),
                        Type = (QuestionType)result.GetInt32(result.GetOrdinal("Type")),
                        CreatedAt = result.GetDateTime(result.GetOrdinal("CreatedAt")),
                        CreatedBy = result.GetGuid(result.GetOrdinal("CreatedBy")),
                        QuestionText = result.GetString(result.GetOrdinal("QuestionText"))
                    });
                }
            }
        }

        return questions;
    }

    public async Task UpdateQuestionAsync(IQuestion question)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE Questions
                    SET QuizId = @QuizId, Index = @Index, Type = @Type, CreatedAt = @CreatedAt, CreatedBy = @CreatedBy, QuestionText = @QuestionText
                    WHERE Id = @Id
                    RETURNING Id;
                    ";

                command.Parameters.AddWithValue("@QuizId", question.QuizId);
                command.Parameters.AddWithValue("@Index", question.Index);
                command.Parameters.AddWithValue("@Type", question.Type);
                command.Parameters.AddWithValue("@CreatedAt", question.CreatedAt);
                command.Parameters.AddWithValue("@CreatedBy", question.CreatedBy);
                command.Parameters.AddWithValue("@QuestionText", question.QuestionText);

                var result = await command.ExecuteNonQueryAsync();
                question.Id = Convert.ToInt32(result);
            }
        }
    }

    public async Task DeleteQuestionAsync(int id)
    {
         using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    DELETE Questions
                    WHERE Id = @Id";

                var reader = await command.ExecuteNonQueryAsync();
            }
        }
    }
}