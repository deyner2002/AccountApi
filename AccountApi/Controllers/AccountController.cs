using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using AccountApi.DTOs;
using System.Net;
using System.Threading.Channels;
using AccountApi.Models;
using System.Reflection.PortableExecutable;

namespace AccountApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {

        public readonly ConnectionStrings _ConnectionStrings;

        public AccountController(IOptions<ConnectionStrings> ConnectionStrings)
        {
            _ConnectionStrings = ConnectionStrings.Value;
        }

        [HttpPost]
        [Route("SaveUserAccount")]
        public IActionResult SaveUserAccount(int userId, string accountType)
        {
            if (accountType == null || accountType == string.Empty)
            {
                return BadRequest("El accountType es requerido");
            }
            try
            {
                using (SqlConnection connection = new SqlConnection(_ConnectionStrings.WebConnection))
                {
                    connection.Open();

                    string query = string.Format("INSERT INTO [dbo].[UserAccount] (Id, UserId, AccountType, CreationDate, ModificationDate, CreationUser, ModificationUser, Status) " +
                "VALUES (NEXT VALUE FOR SequenceUserAccount, {0}, '{1}', GETDATE(), GETDATE(), 'Admin','Admin', 'A'); ",
                userId, accountType);

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                return Ok("se registró correctamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al procesar el archivo: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("GetUserAccount")]
        public UserAccount GetUserAccount(int UserId)
        {
            UserAccount userAccount;
            using (SqlConnection connection = new SqlConnection(_ConnectionStrings.WebConnection))
            {
                connection.Open();

                string query = string.Format("SELECT AccountType FROM UserAccount WHERE UserId = {0}", UserId);
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                        if (reader.Read())
                        {
                            userAccount = new UserAccount
                            {
                                UserId = UserId,
                                AccountType = reader["AccountType"] is DBNull ? null : (string)reader["AccountType"],
                                Permissions = new List<string>()
                            };
                        }
                        else
                        {
                            return null;
                        }

                    string queryPermissions = string.Format("SELECT Name FROM Permission WHERE AccountType = '{0}'", userAccount.AccountType);
                    using (SqlCommand commandPermissions = new SqlCommand(queryPermissions, connection))
                    using (SqlDataReader readerPermissions = commandPermissions.ExecuteReader())
                    {
                        while (readerPermissions.Read())
                        {
                            userAccount.Permissions.Add(readerPermissions["Name"] is DBNull ? string.Empty : (string)readerPermissions["Name"]);
                        }
                    }
                    return userAccount;
                }
            }
        }
    }
}
