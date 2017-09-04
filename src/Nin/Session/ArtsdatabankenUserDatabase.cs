using System;
using System.Collections.Specialized;
using Artsdatabanken.SystemIntegrasjon2.BLL;
using Common.Session;
using Nin.Configuration;
using Nin.IO.SqlServer;

namespace Nin.Session
{
    public class FakeUserDatabase : IUserDatabase
    {
        public void AssertHasRole(string role, string username, string password)
        {
        }

        public string[] Authenticate(string username, string password)
        {
            return new[] { "Administrator", "Dataleverandør" };
        }

        public string GetUserInstitution(string username)
        {
            return "fakeInstitution";
        }
    }

    public class ArtsdatabankenUserDatabase : IUserDatabase
    {
        private ArtsdatabankenMembershipProvider membershipProvider;
        private ArtsdatabankenRoleProvider roleProvider;
        private const string ConnectionstringName = "ArtsdatabankenSIConnectionString";

        public void AssertHasRole(string role, string username, string password)
        {
            ConfigureAuthProviders();

            if (string.IsNullOrEmpty(username))
                throw new Exception("Brukernavn mangler.");

            if (string.IsNullOrEmpty(password))
                throw new Exception("Passord mangler.");

            if (!membershipProvider.ValidateUser(username, password))
                throw new UnauthorizedAccessException();

            if (roleProvider.IsUserInRole(username, "Administrator"))
                return;

            if (!role.Equals("Dataleverandør"))
                throw new UnauthorizedAccessException();

            if (roleProvider.IsUserInRole(username, "Dataleverandør"))
                return;

            throw new UnauthorizedAccessException();
        }

        public string[] Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
                throw new UnauthorizedAccessException("Brukernavn må angis.");
            if (string.IsNullOrEmpty(username))
                throw new UnauthorizedAccessException("Passord må angis.");

            ConfigureAuthProviders();
            if (!membershipProvider.ValidateUser(username, password))
                throw new UnauthorizedAccessException();

            return roleProvider.GetRolesForUser(username);
        }

        private void ConfigureAuthProviders()
        {
            const string applicationName = "NaturtypekartTest";
            if (membershipProvider == null)
                membershipProvider = ConfigurateMembershipProvider(applicationName);
            if (roleProvider == null)
                roleProvider = ConfigurateRoleProvider(applicationName);
        }

        private static ArtsdatabankenMembershipProvider ConfigurateMembershipProvider(string applicationName)
        {
            var membershipProvider = new ArtsdatabankenMembershipProvider { ApplicationName = applicationName };

            var config = new NameValueCollection
            {
                {"applicationName", applicationName},
                {"connectionStringName", ConnectionstringName},
                {"enablePasswordRetrieval", "true"},
                {"enablePasswordReset", "true"},
                {"passwordFormat", "Clear"},
                {"requiresQuestionAndAnswer", "true"},
                {"writeExceptionsToEventLog", "false"}
            };

            membershipProvider.Initialize("AspNetSqlMembershipProvider", config);
            return membershipProvider;
        }

        private static ArtsdatabankenRoleProvider ConfigurateRoleProvider(string applicationName)
        {
            var roleProvider = new ArtsdatabankenRoleProvider { ApplicationName = applicationName };

            var config = new NameValueCollection
            {
                {"applicationName", applicationName},
                {"connectionStringName", ConnectionstringName},
                {"cookieTimeout", "240"}
            };

            roleProvider.Initialize("AspNetSqlRoleProvider", config);
            return roleProvider;
        }

        public string GetUserInstitution(string username)
        {
            var connectionString = Config.Settings.ExternalDependency.UserDatabaseConnectionString;
            const string sql = "SELECT " +
                               "usi.name " +
                               "FROM " +
                               "US_Institution usi, " +
                               "US_Users usu " +
                               "WHERE " +
                               "usi.PK_InstitutionID = usu.FK_PrimaryInstitutionID " +
                               "AND " +
                               "usu.Username = @username";

            using (var sqlCommand = new SqlStatement(sql, connectionString))
            {
                sqlCommand.AddParameter("@username", username);
                return (string) sqlCommand.ExecuteScalar();
            }
        }
    }
}