using Microsoft.Data.SqlClient;

namespace APBD_4.Data;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}