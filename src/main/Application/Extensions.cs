using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ei8.Cortex.Diary.Common;

namespace ei8.Cortex.Diary.Nucleus.Application
{
    internal static class Extensions
    {
        internal static Graph.Common.RelativeType ToExternalType(this RelativeType value)
        {
            return (Graph.Common.RelativeType)Enum.Parse(typeof(Graph.Common.RelativeType), ((int)value).ToString());
        }

        internal static Graph.Common.NeuronQuery ToExternalType(this NeuronQuery value)
        {
            var result = new Graph.Common.NeuronQuery();
            result.Id = value.Id?.ToArray();
            result.IdNot = value.IdNot?.ToArray();
            result.Postsynaptic = value.Postsynaptic?.ToArray();
            result.PostsynapticNot = value.PostsynapticNot?.ToArray();
            result.Presynaptic = value.Presynaptic?.ToArray();
            result.PresynapticNot = value.PresynapticNot?.ToArray();
            result.TagContains = value.TagContains?.ToArray();
            result.TagContainsNot = value.TagContainsNot?.ToArray();

            result.RelativeValues = Extensions.ConvertNullableEnumToExternal<Diary.Common.RelativeValues, Graph.Common.RelativeValues>(
                value.RelativeValues, 
                v => ((int)v).ToString()
                );

            result.NeuronActiveValues = Extensions.ConvertNullableEnumToExternal<Diary.Common.ActiveValues, Graph.Common.ActiveValues>(
                value.NeuronActiveValues,
                v => ((int)v).ToString()
                );

            result.TerminalActiveValues = Extensions.ConvertNullableEnumToExternal<Diary.Common.ActiveValues, Graph.Common.ActiveValues>(
                value.TerminalActiveValues,
                v => ((int)v).ToString()
                );

            result.Limit = value.Limit;

            return result;
        }

        private static TNew? ConvertNullableEnumToExternal<TOrig, TNew>(TOrig? original, Func<TOrig?, string> evaluator) 
            where TOrig : struct 
            where TNew : struct
        {
            TNew? r = null;
            if (original.HasValue)
                r = (TNew)Enum.Parse(
                    typeof(TNew),
                    evaluator(original.Value)
                    );
            return r;
        }

        internal static Neuron ToInternalType(this Graph.Common.Neuron value)
        {
            return new Neuron()
            {
                Id = value.Id,
                Tag = value.Tag,
                Terminal = value.Terminal != null ? new Terminal()
                {
                    AuthorId = value.Terminal.AuthorId,
                    AuthorTag = value.Terminal.AuthorTag,
                    Effect = value.Terminal.Effect,
                    Id = value.Terminal.Id,
                    PostsynapticNeuronId = value.Terminal.PostsynapticNeuronId,
                    PresynapticNeuronId = value.Terminal.PresynapticNeuronId,
                    Strength = value.Terminal.Strength,
                    Timestamp = value.Terminal.Timestamp,
                    Version = value.Terminal.Version,
                    Active = value.Terminal.Active
                } : null,
                Version = value.Version,
                AuthorId = value.AuthorId,
                AuthorTag = value.AuthorTag,
                RegionId = value.RegionId,
                RegionTag = value.RegionTag,
                Timestamp = value.Timestamp,
                Active = value.Active
            };
        }

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
