using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    /// <summary>
    /// 分页查询结果
    /// </summary>
    public interface IPagingQueryResult<T>
    {
        PagingArgs Paging { get; }

        int Total { get; set; }

        T[] Datas { get; set; }

    }
}
