using System;
using System.Collections.Generic;
using System.Linq;
using TsSoft.Dapper.QueryBuilder.Metadata;

namespace TsSoft.Dapper.QueryBuilder.Helpers.Join
{
    public class ManyToManyClauseCreator : JoinClauseCreator
    {
        public override JoinClause Create(JoinAttribute joinAttribute)
        {
            ManyToManyJoinAttribute manyToManyJoinAttribute;
            if ((manyToManyJoinAttribute = joinAttribute as ManyToManyJoinAttribute) == null)
            {
                throw new ArgumentException("Attribute must be ManyToManyJoinAttribute");
            }

            string splitter = string.Format("SplitOn{0}{1}", manyToManyJoinAttribute.JoinedTable,
                                            manyToManyJoinAttribute.JoinedTableField);
            var selects = new List<string>();
            if (manyToManyJoinAttribute.TableSelectColumns != null && manyToManyJoinAttribute.TableSelectColumns.Any())
            {
                foreach (var tableSelectColumn in manyToManyJoinAttribute.TableSelectColumns)
                {
                    selects.AddRange(
                        tableSelectColumn.Value.Select(column => string.Format("{0}.{1}", tableSelectColumn.Key, column)));
                }
            }
            else
            {
                selects.Add(string.Format("{0}.*", manyToManyJoinAttribute.JoinedTable));
            }

            var joins = new List<string>
                {
                    string.Format("{0} on {0}.{1} = {2}.{3}", manyToManyJoinAttribute.CommunicationTable,
                                  manyToManyJoinAttribute.CommunicationTableCurrentTableField, manyToManyJoinAttribute.CurrentTable,
                                  manyToManyJoinAttribute.CurrentTableField),
                    string.Format("{0} on {0}.{1} = {2}.{3}", manyToManyJoinAttribute.JoinedTable,
                                  manyToManyJoinAttribute.JoinedTableField, manyToManyJoinAttribute.CommunicationTable,
                                  manyToManyJoinAttribute.CommunicationTableJoinedTableField),
                };
            var result = new JoinClause
                {
                    JoinSqls = joins,
                    SelectsSql = selects,
                    Splitter = splitter,
                    JoinType = manyToManyJoinAttribute.JoinType,
                    HasJoin = true,
                };
            return result;
        }
    }
}