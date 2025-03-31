using mcZen.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Cass.Bracket.Web
{
    public class UserManager(IOptions<UserManager.Options> _options)
    {
        public void Save(Models.User user)
        {
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Models.User>();
            if (user.Id == 0)
            {
                if (Get(user.Email) != null) { throw new UsernameTakenException();  }
                
                ConnectionFactory.Execute(Commands.Insert<int>((id) => user.Id = id, "BracketUser", "@user_id",
                    new SqlParameter("@user_Name", user.Name),
                    new SqlParameter("@user_email", user.Email),
                    new SqlParameter("@user_password", hasher.HashPassword(user, user.Password))), _options.Value.ConnectionString);
            }
            else
            {
                ConnectionFactory.Execute(Commands.Update("BracketUser", new SqlParameter("@user_id", user.Id),
                    new SqlParameter("@user_Name", user.Name),
                    new SqlParameter("@user_email", user.Email),
                    new SqlParameter("@user_password", hasher.HashPassword(user, user.Password))), _options.Value.ConnectionString);
            }
        }

        public Models.User? Get(string email)
        {
            Models.User? retVal = null;
            ConnectionFactory.Execute(new CommandReader((r) => {
                retVal = new Models.User()
                {
                    Id = r.GetInt32(0),
                    Email = r.GetString(1),
                    Name = r.GetString(2),
                    Password = r.GetString(3),
                    Created = r.GetDateTimeOffset(4)
                };
                return false;
                },
                "SELECT user_id, user_email, user_name, user_password, user_created FROM BracketUser where user_email=@email", System.Data.CommandType.Text, _options.Value.Timeout, new SqlParameter("@email", email)), _options.Value.ConnectionString);
            return retVal;
        }

        public Models.User? Signin(string username, string password)
        {
            var user = Get(username);
            if (user == null) { return null; }
            
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Models.User>();
            var result = hasher.VerifyHashedPassword(user, user.Password, password);
            if (result == PasswordVerificationResult.Failed) return null;
            return user;
        }

        public class Options
        {
            public required string ConnectionString { get; set; }
            public TimeSpan Timeout { get; set; }
        }
    }
}
