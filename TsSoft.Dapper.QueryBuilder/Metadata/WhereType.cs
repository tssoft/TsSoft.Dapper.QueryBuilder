using System.ComponentModel;

namespace TsSoft.Dapper.QueryBuilder.Metadata
{
    public enum WhereType
    {
        [Description("=")]
        Eq,
        [Description("<>")]
        NotEq,
        [Description(">")]
        Gt,
        [Description("<")]
        Lt,
        [Description(">=")]
        GtEq,
        [Description("<=")]
        LtEq,
        [Description("Like")]
        Like,
        [Description("is null")]
        IsNull,
        [Description("is not null")]
        IsNotNull,
        [Description("in")]
        In,
    }
}