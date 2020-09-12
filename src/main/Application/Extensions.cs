using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ei8.Cortex.Diary.Common;

namespace ei8.Cortex.Diary.Nucleus.Application
{
    internal static class Extensions
    {
        internal static NotificationLog ToInternalType(this EventSourcing.Common.NotificationLog value)
        {
            return new NotificationLog()
            {
                FirstNotificationLogId = value.FirstNotificationLogId,
                HasFirstNotificationLog = value.HasFirstNotificationLog,
                HasNextNotificationLog = value.HasNextNotificationLog,
                HasPreviousNotificationLog = value.HasPreviousNotificationLog,
                IsArchived = value.IsArchived,
                NextNotificationLogId = value.NextNotificationLogId,
                NotificationList = new ReadOnlyCollection<Common.Notification>(value.NotificationList.Select(n => n.ToInternalType()).ToList()),
                NotificationLogId = value.NotificationLogId,
                PreviousNotificationLogId = value.PreviousNotificationLogId,
                TotalCount = value.TotalCount,
                TotalNotification = value.TotalNotification
            };
        }

        internal static Common.Notification ToInternalType(this EventSourcing.Common.Notification value)
        {
            return new Common.Notification()
            {
                AuthorId = value.AuthorId,
                Data = value.Data,
                Id = value.Id,
                SequenceId = value.SequenceId,
                Timestamp = value.Timestamp,
                TypeName = value.TypeName,
                Version = value.Version
            };
        }
    }
}
