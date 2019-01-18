using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;

namespace TestMetaServer
{
    public class UserMapper : IUserMapper
    {
        static readonly byte[] rightHash = 
        {
            169,86,179,254,197,244,40,153,221,48,226,230,182,98,126,226,209,242,245,218,75,0,202,104,84,28,150,192,218,250,90,80,1,161,243,140,195,116,7,125,201,217,64,56,50,38,56,173,70,99,167,41,200,158,96,3,143,239,108,0,7,243,210,100
        };

        static List<Tuple<Guid, string>> users = new List<Tuple<Guid, string>>();

        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            var userRecord = users.FirstOrDefault(u => u.Item1 == identifier);

            return userRecord == null
                ? null
                : new UserIdentity() { UserName = userRecord.Item2 };
        }

        public static Guid? ValidateUser(string username, string password)
        {
            var sha512 = SHA512.Create();
            var hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(password));

            if (username == "admin" && hash.SequenceEqual(rightHash))
            {
                var userRecord = users.FirstOrDefault(u => u.Item2 == "admin");
                if (userRecord == null)
                {
                    var guid = Guid.NewGuid();
                    users.Add(Tuple.Create(guid, "admin"));
                    return guid;
                }
                else
                {
                    return userRecord.Item1;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
