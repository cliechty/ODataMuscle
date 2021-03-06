﻿using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq.Expressions;

namespace ODataMuscle
{
    public static class ODataMuscleClientExtensions
    {
        // Copied the SetLink, AddLink, DeleteLink, LoadProperty
        // http://weblogs.asp.net/cibrax/archive/2010/10/27/getting-rid-of-the-magic-strings-in-a-wcf-data-service-client.aspx

        public static void SetLink<TSource, TPropType>(this DataServiceContext context,
            TSource source,
            Expression<Func<TSource, TPropType>> propertySelector,
            object target)
        {
            string expandString = ExpandPropertyName(propertySelector);
            context.SetLink(source, expandString, target);
        }

        public static void AddLink<TSource, TPropType>(this DataServiceContext context,
            TSource source, 
            Expression<Func<TSource, TPropType>> propertySelector, 
            object target)
        {
            string expandString = ExpandPropertyName(propertySelector);
            context.AddLink(source, expandString, target);
        }

        public static void DeleteLink<TSource, TPropType>(this DataServiceContext context,
            TSource source, 
            Expression<Func<TSource, TPropType>> propertySelector, 
            object target)
        {
            string expandString = ExpandPropertyName(propertySelector);
            context.DeleteLink(source, expandString, target);
        }

        public static void LoadProperty<TSource, TPropType>(this DataServiceContext context,
            TSource source, 
            Expression<Func<TSource, TPropType>> propertySelector)
        {
            string expandString = ExpandPropertyName(propertySelector);
            context.LoadProperty(source, expandString);
        }

        public static DataServiceQuery<T> Expand<T, TProperty>(this DataServiceQuery<T> entities, 
            Expression<Func<T, TProperty>> propertyExpressions)
        {
            string propertyName = ExpandPropertyName(propertyExpressions);
            return entities.Expand(propertyName);
        }

        public static DataServiceQuery<T> Expand<T>(this DataServiceQuery<T> entities,
            params Expression<Func<T, object>>[] propertyExpressions)
        {
            foreach (Expression<Func<T, object>> propertyExpression in propertyExpressions)
            {
                string propertyName = ExpandPropertyName(propertyExpression);
                entities = entities.Expand(propertyName);
            }
            return entities;
        }

        public static string Expand<T, TProperty>(this IEnumerable<T> collection,
            Expression<Func<T, TProperty>> propertyExpressions)
        {
            string propertyName = ExpandPropertyName(propertyExpressions);
            return propertyName;
        }

        private static string ExpandPropertyName(Expression exp)
        {
            string str = string.Empty;
            switch (exp.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var mx = (exp as MemberExpression);
                    str = ExpandPropertyName(mx.Expression);
                    str += (str.Length > 0 ? "/" : "") + mx.Member.Name;
                    break;
                case ExpressionType.Lambda:
                    str = ExpandPropertyName((exp as LambdaExpression).Body);
                    break;
                case ExpressionType.Call:
                    var mcx = (exp as MethodCallExpression);
                    //this might have to iterate them right to left and not left to right for propery argument order
                    for (int index = 0; index < mcx.Arguments.Count; index++)
                    {
                        Expression arg = mcx.Arguments[index];
                        str += (str.Length > 0 ? "/" : "") + ExpandPropertyName(arg);
                    }
                    break;
                case ExpressionType.Quote:
                    var ux = (exp as UnaryExpression);
                    str = ExpandPropertyName(ux.Operand);
                    break;
            }
            return str;
        }
    }
}