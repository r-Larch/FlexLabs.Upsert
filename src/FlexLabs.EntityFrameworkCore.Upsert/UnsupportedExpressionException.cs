﻿using System;
using System.Linq.Expressions;
using FlexLabs.EntityFrameworkCore.Upsert.Internal;


namespace FlexLabs.EntityFrameworkCore.Upsert
{
    /// <summary>
    /// Thrown when using unsupported expressions in the update clause
    /// See: https://go.flexlabs.org/upsert.expressions
    /// </summary>
    public class UnsupportedExpressionException : Exception
    {
        private UnsupportedExpressionException(string message, string? helpLink = null)
            : base(message)
        {
            HelpLink = helpLink;
        }

        internal UnsupportedExpressionException(System.Linq.Expressions.Expression expression)
            : base(Resources.ThisTypeOfExpressionIsNotCurrentlySupported + " " + expression + ". " + Resources.SimplifyTheExpressionOrTryADifferentOne +
                  Resources.FormatSeeLinkForMoreDetails(HelpLinks.SupportedExpressions))
        {
            HelpLink = HelpLinks.SupportedExpressions;
        }

        internal static UnsupportedExpressionException MySQLConditionalUpdate()
            => new(Resources.UsingConditionalUpdatesIsNotSupportedInMySQLDueToDatabaseSyntaxLimitations + " " + 
                Resources.FormatSeeLinkForMoreDetails(HelpLinks.MySQLConditionalUpdate),
                HelpLinks.MySQLConditionalUpdate);

        internal static Exception JsonMemberBinding(Expression expression)
        {
            return new UnsupportedExpressionException($"Modifying JSON members is not supported. Unsupported Expression: {expression}", HelpLinks.SupportedExpressions);
        }

        internal static Exception JsonMemberAccess(IColumnBase column, Expression expression)
        {
            return new UnsupportedExpressionException($"Reading JSON members is not supported. Unsupported Access Expression: {expression}", HelpLinks.SupportedExpressions);
        }
    }
}
