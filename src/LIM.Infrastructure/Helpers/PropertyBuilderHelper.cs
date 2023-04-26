using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LIM.Infrastructure.Helpers;

public static class PropertyBuilderHelper
{
    /// <summary>
        /// Настройка обязательного строкового поля
        /// </summary>
        /// <param name="builder">Поле</param>
        /// <param name="maxLength">Длина строки</param>
        /// <returns></returns>
        public static PropertyBuilder<string> IsRequiredString(this PropertyBuilder<string> builder, int maxLength)
        {
            return builder
                .IsUnicode(false)
                .HasMaxLength(maxLength)
                .IsRequired();
        }

        /// <summary>
        /// Настройка необязательного строкового поля
        /// </summary>
        /// <param name="builder">Поле</param>
        /// <param name="maxLength">Длина строки</param>
        /// <returns></returns>
        public static PropertyBuilder<string?> IsString(this PropertyBuilder<string?> builder, int maxLength)
        {
            return builder
                .IsUnicode(false)
                .HasMaxLength(maxLength);
        }

        /// <summary>
        /// Установка типа даты
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static PropertyBuilder<DateTime> IsDate(this PropertyBuilder<DateTime> builder)
        {
            return builder
                .HasColumnType("date");
        }

        public static PropertyBuilder<DateTime> IsTimeStamp(this PropertyBuilder<DateTime> builder)
        {
            return builder
                .HasColumnType("timestamp");
        }

        public static PropertyBuilder<DateTime?> IsTimeStamp(this PropertyBuilder<DateTime?> builder)
        {
            return builder
                .HasColumnType("timestamp");
        }

        /// <summary>
        /// Установка типа даты
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static PropertyBuilder<DateTime?> IsDate(this PropertyBuilder<DateTime?> builder)
        {
            return builder
                .HasColumnType("date");
        }

        /// <summary>
        /// Настройка jsonb для списка
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static PropertyBuilder<List<TEntity>> IsJson<TEntity>(this PropertyBuilder<List<TEntity>> builder)
            where TEntity : class
        {
            JsonSerializerOptions? options = null;
            builder
                .IsUnicode(false)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v,options),
#pragma warning disable CS8603
                    v => JsonSerializer.Deserialize<List<TEntity>>(v, options),
#pragma warning restore CS8603
                    new ValueComparer<List<TEntity>>(
                        (c1, c2) => c2 != null && c1 != null && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

            return builder;
        }

        /// <summary>
        /// Настройка jsonb для списка
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static PropertyBuilder<TEntity> IsJson<TEntity>(this PropertyBuilder<TEntity> builder)
            where TEntity : class
        {
            builder
                .IsUnicode(false)
                .HasColumnType("jsonb");

            return builder;
        }
}