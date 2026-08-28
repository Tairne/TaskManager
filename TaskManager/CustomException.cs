namespace TaskManager
{
    public class DuplicateLoginException : Exception
    {
        public DuplicateLoginException(string login)
            : base($"Login '{login}' is already in use.")
        {
        }
    }
}
