using Fanex.Logging;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Fanex.Affiliate.Link.Helpers
{
    public static class LogHelper
    {
        private const int LimitedLevelOfInnerException = 3;

        public static void WriteError(Exception exception, string additionalMessage = null)
        {
            var content = BuildMessage(exception);
            if (!string.IsNullOrWhiteSpace(additionalMessage))
            {
                content.AppendLine(additionalMessage);
            }

            AddDatabaseInfo(content, exception);
            AddExceptionInfo(content, exception, LimitedLevelOfInnerException);

            Logger.Log.Error(content.ToString(), exception);
        }

        private static StringBuilder BuildMessage(Exception exception)
        {
            var builder = new StringBuilder().AppendLine($"Error Message: {exception.Message}").AppendLine();
            builder.AppendLine("REQUEST INFO")
                   .AppendLine($"Source:{exception.Source}")
                   .AppendLine($"TargetSite:{exception.TargetSite}");

            return builder;
        }

        private static void AddDatabaseInfo(StringBuilder builder, Exception exception)
        {
            builder.AppendLine();
            builder.AppendLine("DATABASE INFO");

            var dbInfo = GetDbExceptionInfo(exception);

            foreach (var item in dbInfo)
            {
                builder.AppendLine($"{item.Key}: {item.Value}");
            }
        }

        private static IDictionary<string, object> GetDbExceptionInfo(Exception exception)
        {
            var dbInfo = new Dictionary<string, object>();
            BindSqlExceptionInfo(exception, dbInfo);
            return dbInfo;
        }

        private static void BindSqlExceptionInfo(Exception exception, IDictionary<string, object> dbInfo)
        {
            if (exception is SqlException sqlException)
            {
                dbInfo["Server"] = sqlException.Server;
                dbInfo["Procedure"] = sqlException.Procedure;
                dbInfo["Line"] = sqlException.LineNumber;
            }
        }

        private static void AddExceptionInfo(StringBuilder builder, Exception exception, int limitedInnerException)
        {
            builder.AppendLine();
            builder.AppendLine($"EXCEPTION INFO LEVEL: {LimitedLevelOfInnerException - limitedInnerException}");
            builder.AppendLine($"Message: {exception.Message}");
            builder.AppendLine($"Source: {exception.Source}");
            builder.AppendLine($"TargetSite: {exception.TargetSite}");
            builder.AppendLine($"StackTrace: {exception.StackTrace}");

            if (exception.InnerException != null && limitedInnerException > 0)
            {
                var innerExceptionLevel = limitedInnerException - 1;
                AddExceptionInfo(builder, exception.InnerException, innerExceptionLevel);
            }
        }
    }
}
