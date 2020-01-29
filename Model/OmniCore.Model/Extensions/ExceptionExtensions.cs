using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;

namespace OmniCore.Model.Extensions
{
    public static class ExceptionExtensions
    {
        public static string AsDebugFriendly(this Exception exception)
        {
            var readable = new StringBuilder();

            var exceptionsList = new List<List<Exception>>();

            exceptionsList.WithException(exception, new List<Exception>());
            readable.AppendLine();

            int x = 0;
            foreach (var exceptionList in exceptionsList)
            {
                x++;
                readable.AppendLine($"***\t({x})\tThrown exceptions (inside out):");
                exceptionList.Reverse();
                int y = 0;
                foreach (var exceptionEntry in exceptionList)
                {
                    y++;
                    readable.AppendLine($"***\t({x}.{y})\t{exceptionEntry.GetType().Name}: {exceptionEntry.Message}");
                    readable.AppendLine(exceptionEntry.StackTrace);
                    readable.AppendLine();
                }
            }
           
            return readable.ToString();
        }

        private static List<List<Exception>> WithException(this List<List<Exception>> list, Exception exception, List<Exception> referenceEntry)
        {
            switch (exception)
            {
                case AggregateException aggregateException:
                {
                    foreach (var aggregateChild in aggregateException.InnerExceptions)
                        list.WithException(aggregateChild, new List<Exception>(referenceEntry));
                    break;
                }
                default:
                {
                    list.Add(referenceEntry.WithException(exception));
                    break;
                }
            }

            return list;
        }

        private static List<Exception> WithException(this List<Exception> list, Exception exception)
        {
            if (exception == null)
                return list;
            list.Add(exception);
            return list.WithException(exception.InnerException);
        }
    }
}
