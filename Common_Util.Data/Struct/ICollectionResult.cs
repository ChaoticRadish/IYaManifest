using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    public interface ICollectionResult<T>
    {
        T[] Datas { get; }

        /// <summary>
        /// 返回的数据总量
        /// </summary>
        int Total { get; }
        /// <summary>
        /// 有可能的数据总量, 可能会大于 <see cref="Total"/>, 比如分页查询时, 
        /// <see cref="Total"/> 为返回的总量
        /// <see cref="PossibleTotal"/> 为所有页面数量的总和
        /// </summary>
        int PossibleTotal { get; }
    }
}
