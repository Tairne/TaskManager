namespace TaskManager
{
    public class DuplicateLoginException : Exception
    {
        public DuplicateLoginException(string login)
            : base($"Login '{login}' is already in use.")
        {
        }
    }

    public class RequestLimitExceeded : Exception
    {
        public RequestLimitExceeded(int limit)
            : base($"The page size should not exceed {limit} entries")
        { 
        }
    }
}
