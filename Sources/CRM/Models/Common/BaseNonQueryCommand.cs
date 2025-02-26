using Fanex.Data.Repository;

namespace CRM.Models.Common;

public abstract class BaseNonQueryCommand : NonQueryCommand
{
    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }
}