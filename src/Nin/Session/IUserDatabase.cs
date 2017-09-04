namespace Common.Session
{
    public interface IUserDatabase
    {
        void AssertHasRole(string role, string username, string password);
        string[] Authenticate(string username, string password);
        string GetUserInstitution(string username);
    }
}