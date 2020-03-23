using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace JinCreek.Server.Admin
{
    public static class Utils
    {
        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Order order)
        {
            return order switch
            {
                Order.Asc => source.OrderBy(keySelector),
                Order.Desc => source.OrderByDescending(keySelector),
                _ => throw new ArgumentOutOfRangeException(nameof(order), order, null)
            };
        }

        // see Sorting using property name as string, https://entityframeworkcore.com/knowledge-base/34899933/
        public static IOrderedQueryable<TSource> OrderBy<TSource>(IQueryable<TSource> source, string propertyName, Order order)
        {
            var methodName = order switch
            {
                Order.Asc => "OrderBy",
                Order.Desc => "OrderByDescending",
                _ => throw new ArgumentOutOfRangeException(nameof(order), order, null)
            };
            var parameter = Expression.Parameter(typeof(TSource), "x");
            Expression property = Expression.Property(parameter, propertyName);
            return (IOrderedQueryable<TSource>)typeof(Queryable).GetMethods()
                .First(x => x.Name == methodName && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TSource), property.Type).Invoke(null,
                    new object[] { source, Expression.Lambda(property, parameter) });
        }

        /// <summary>
        /// CSVをパースしてTのリストを返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TMap"></typeparam>
        /// <param name="csv"></param>
        /// <param name="modelState"></param>
        /// <returns></returns>
        public static List<T> ParseCsv<T, TMap>(TextReader csv, ModelStateDictionary modelState) where TMap : ClassMap
        {
            using var csvReader = new CsvReader(csv, CultureInfo.InvariantCulture);
            csvReader.Configuration.RegisterClassMap<TMap>();

            // ヘッダのバリデーション
            csvReader.Configuration.HeaderValidated = (isValid, headerNames, headerNameIndex, context) =>
            {
                // see https://github.com/JoshClose/CsvHelper/blob/13.0.0/src/CsvHelper/Configuration/ConfigurationFunctions.cs#L18
                if (isValid) return;
                var indexText = headerNameIndex > 0 ? $" at header name index {headerNameIndex}" : string.Empty;
                var message = headerNames.Length == 1
                    ? $"Header with name '{headerNames[0]}'{indexText} was not found."
                    : $"Header containing names '{string.Join("' or '", headerNames)}'{indexText} was not found.";
                modelState.AddModelError("header", message);
            };
            if (!csvReader.Read())
            {
                csvReader.Context.Record = new string[] { };
            }
            csvReader.ReadHeader();
            csvReader.ValidateHeader<T>(); // csvReader.Configuration.HeaderValidatedがコールバックされる
            if (!modelState.IsValid)
            {
                return null; // ヘッダにエラーがあればここで終了する
            }

            csvReader.Configuration.BadDataFound = context =>
            {
                modelState.AddModelError("key", "BadDataFound");
            };
            csvReader.Configuration.MissingFieldFound = (headerNames, index, context) =>
            {
                modelState.AddModelError("key", "MissingFieldFound");
            };
            csvReader.Configuration.ReadingExceptionOccurred = exception =>
            {
                var context = exception.ReadingContext;
                var key = context.HeaderRecord[context.CurrentIndex];
                var message = $"(Row:{context.Row}) {exception.InnerException?.Message ?? exception.Message}";
                modelState.AddModelError(key, message);
                return false;
            };
            return csvReader.GetRecords<T>().ToList();
        }

        /// <summary>
        /// CSVレコードのバリデーションをする
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records"></param>
        /// <param name="modelState"></param>
        public static void TryValidateCsvRecords<T>(IEnumerable<T> records, ModelStateDictionary modelState)
        {
            var index = 2; // ヘッダを除いて2行目から
            foreach (var record in records)
            {
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(record, new ValidationContext(record, null, null), validationResults, true))
                {
                    foreach (var result in validationResults) modelState.AddModelError(result.MemberNames.Single(), $"(Row:{index}) {result.ErrorMessage}");
                }
                index++;
            }
        }
    }

    public class PageParam
    {
        [Range(1, int.MaxValue)] public int? Page { get; set; }
        [Range(1, 1000)] public int PageSize { get; set; } = 20;
    }

    public enum Order
    {
        Asc,
        Desc,
    }

    /// <summary>
    /// ページ結果
    /// see https://github.com/encode/django-rest-framework/blob/f8c16441fa69850fd581c23807a8a0e38f3239d4/rest_framework/pagination.py#L220
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class PaginatedResponse<T>
    {
        public int Count { get; set; }
        public IEnumerable<T> Results { get; set; }
    }
}
