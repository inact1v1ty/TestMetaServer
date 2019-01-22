using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;

namespace TestMetaServer
{
    public class UserMapper : IUserMapper
    {
        public static string DefaultPassword 
        {
            get {
                return "A956B3FEC5F42899DD30E2E6B6627EE2D1F2F5DA4B00CA68541C96C0DAFA5A5001A1F38CC374077DC9D94038322638AD4663A729C89E60038FEF6C0007F3D264";
            }
        }

        static List<Tuple<Guid, string>> users = new List<Tuple<Guid, string>>();

        public ClaimsPrincipal GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            var userRecord = users.FirstOrDefault(u => u.Item1 == identifier);

            return userRecord == null
                ? null
                : new ClaimsPrincipal(new UserIdentity() {
                    Name = "admin",
                    IsAuthenticated = true,
                    AuthenticationType = "Password"
                    });
        }

        public static Guid? ValidateUser(string username, string password)
        {
            var sha512 = SHA512.Create();
            var hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(password));

            byte[] rightPassword;

            using(var db = new LiteDatabase(@"Meta.db")) {
                rightPassword = StringToByteArray(
                    db.GetCollection<KeyValue>("Settings")
                        .FindById("password")
                        .Value
                );
            }

            if (username == "admin" && hash.SequenceEqual(rightPassword))
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

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-","");
        }

        public static byte[] StringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
