namespace CRM.Repositories;

using System.Collections;
using Fanex.Data;
using Fanex.Data.Repository;

public interface IBaseRepository
{
    Task<CRM.Models.Common.CommandResult> ExecuteNonQueryAsync(INonQueryCommand command);
}

public class BaseRepository : IBaseRepository
{
    public async Task<CRM.Models.Common.CommandResult> ExecuteNonQueryAsync(INonQueryCommand command)
    {
        using var db = ObjectDbFactory.CreateInstance(command.GetSettingKey());
        await db.ExecuteNonQueryAsync(command);
        var commandResult = GetCommandResult(db.GetOutputParams());

        return commandResult;
    }

    private static CRM.Models.Common.CommandResult GetCommandResult(Hashtable hash)
    {
        var errCode = hash["errCode"];
        var errMsg = hash["errMsg"];

        return new CRM.Models.Common.CommandResult
        {
            Code = errCode is DBNull ? 0 : Convert.ToInt32(errCode),
            Message = errMsg is DBNull ? string.Empty : Convert.ToString(errMsg)
        };
    }
}