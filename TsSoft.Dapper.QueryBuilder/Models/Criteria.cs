using System.Collections.Generic;
using System.Linq;
using TsSoft.Dapper.QueryBuilder.Metadata;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Models
{
    public class Criteria
    {
        /// <summary>
        /// Для запросов с пагинацией
        /// Сколько записей выбрать
        /// </summary>
        public int Take { get; set; }

        /// <summary>
        /// Для запросов с пагинацией
        /// Сколько записей пропустить
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// Тип запроса (Простой, с пагинацией, только количество и т.д.)
        /// </summary>
        public QueryType QueryType { get; set; }

        /// <summary>
        /// Сортировка
        /// В ключе содержится условие сортировки для даппера (например, Tasks.Deadline)
        /// В значении направление
        /// </summary>
        public IDictionary<string, OrderType> Order { get; set; }

        /// <summary>
        /// Имеется сортировка
        /// </summary>
        public bool HasOrder
        {
            get { return Order != null && Order.Any(); }
        }
    }
}