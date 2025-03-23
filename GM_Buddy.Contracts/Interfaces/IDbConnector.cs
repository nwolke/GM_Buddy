using System.Data;

namespace GM_Buddy.Contracts.Interfaces;

public interface IDbConnector
{
    IDbConnection CreateConnection();
}
