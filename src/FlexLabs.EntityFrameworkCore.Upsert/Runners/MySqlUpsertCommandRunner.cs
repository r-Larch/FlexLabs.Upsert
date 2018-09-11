﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlexLabs.EntityFrameworkCore.Upsert.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FlexLabs.EntityFrameworkCore.Upsert.Runners
{
    /// <summary>
    /// Upsert command runner for the MySql.Data.EntityFrameworkCore or the Pomelo.EntityFrameworkCore.MySql providers
    /// </summary>
    public class MySqlUpsertCommandRunner : RelationalUpsertCommandRunner
    {
        public override bool Supports(string name) => name == "MySql.Data.EntityFrameworkCore" || name == "Pomelo.EntityFrameworkCore.MySql";
        protected override string EscapeName(string name) => "`" + name + "`";
        protected override string SourcePrefix => "VALUES(";
        protected override string SourceSuffix => ")";
        protected override string TargetPrefix => null;

        public override string GenerateCommand(IEntityType entityType, ICollection<ICollection<(string ColumnName, ConstantValue Value)>> entities, ICollection<string> joinColumns,
            List<(string ColumnName, KnownExpression Value)> updateExpressions)
        {
            var result = new StringBuilder("INSERT ");
            if (updateExpressions == null)
                result.Append("IGNORE ");
            result.Append($"INTO {GetSchema(entityType)}`{entityType.Relational().TableName}` (");
            result.Append(string.Join(", ", entities.First().Select(e => EscapeName(e.ColumnName))));
            result.Append(") VALUES (");
            result.Append(string.Join("), (", entities.Select(ec => string.Join(", ", ec.Select(e => Parameter(e.Value.ArgumentIndex))))));
            result.Append(")");
            if (updateExpressions != null)
            {
                result.Append(" ON DUPLICATE KEY UPDATE ");
                result.Append(string.Join(", ", updateExpressions.Select((e, i) => $"{EscapeName(e.ColumnName)} = {ExpandExpression(e.Value)}")));
            }
            return result.ToString();
        }
    }
}
