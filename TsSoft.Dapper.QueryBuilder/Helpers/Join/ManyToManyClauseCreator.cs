using System;
using System.Collections.Generic;
using System.Linq;
using TsSoft.Dapper.QueryBuilder.Metadata;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

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

            var splitter = GetSplitter(manyToManyJoinAttribute);
            var selects = new List<string>();
            if (manyToManyJoinAttribute.TableSelectColumns != null && manyToManyJoinAttribute.TableSelectColumns.Any())
            {
                foreach (var tableSelectColumn in manyToManyJoinAttribute.TableSelectColumns)
                {
                    selects
                        .AddRange(
                            tableSelectColumn.Value.Select(
                                selectClause =>
                                    selectClause.IsExpression
                                        ? selectClause.Select
                                        : string.Format("{0}.{1}", selectClause.Table, selectClause.Select)));
                }
            }
            else
            {
                selects.Add(string.Format("{0}.*", manyToManyJoinAttribute.JoinedTable));
            }

            var joins = new List<string>
            {
                CreateJoinCommunication(manyToManyJoinAttribute),
                CreateJoinJoined(manyToManyJoinAttribute),
            };
            var result = new JoinClause
            {
                JoinSqls = joins,
                SelectsSql = selects,
                Splitter = splitter,
                JoinType = manyToManyJoinAttribute.JoinType,
                HasJoin = true,
                Order = joinAttribute.Order,
            };
            return result;
        }

        private string CreateJoinCommunication(ManyToManyJoinAttribute manyToManyJoinAttribute)
        {
            return string.Format("{0} on {0}.{1} = {2}.{3}{4}", manyToManyJoinAttribute.CommunicationTable,
                                 manyToManyJoinAttribute.CommunicationTableCurrentTableField,
                                 manyToManyJoinAttribute.CurrentTable,
                                 manyToManyJoinAttribute.CurrentTableField,
                                 manyToManyJoinAttribute.AddOnType == AddOnType.ForCommunication ? GetAddOnClauses(manyToManyJoinAttribute) : string.Empty);
        }

        private string CreateJoinJoined(ManyToManyJoinAttribute manyToManyJoinAttribute)
        {
            return string.Format("{0} on {0}.{1} = {2}.{3}{4}", manyToManyJoinAttribute.JoinedTable,
                          manyToManyJoinAttribute.JoinedTableField, manyToManyJoinAttribute.CommunicationTable,
                          manyToManyJoinAttribute.CommunicationTableJoinedTableField,
                          manyToManyJoinAttribute.AddOnType == AddOnType.ForJoined ? GetAddOnClauses(manyToManyJoinAttribute) : string.Empty);
        }
    }
}