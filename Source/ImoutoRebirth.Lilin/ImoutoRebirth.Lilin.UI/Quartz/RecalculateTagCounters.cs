﻿using ImoutoRebirth.Common.Quartz;
using ImoutoRebirth.Lilin.Application.TagSlice;
using MediatR;
using Microsoft.Extensions.Options;
using Quartz;

namespace ImoutoRebirth.Lilin.UI.Quartz;

public class RecalculateTagsCountersJob : IJob
{
    private readonly IMediator _mediator;

    public RecalculateTagsCountersJob(IMediator mediator) => _mediator = mediator;

    public Task Execute(IJobExecutionContext context) => _mediator.Send(new UpdateTagsCountersCommand());

    public class Description : IQuartzJobDescription
    {
        private readonly int _repeatEveryMinutes;

        public Description(IOptions<RecalculateTagCountersSettings> options)
        {
            _repeatEveryMinutes = options.Value.RepeatEveryMinutes;
        }

        public IJobDetail GetJobDetails()
            => JobBuilder.Create<RecalculateTagsCountersJob>()
                .WithIdentity("Recalculate tags counters")
                .Build();

        public ITrigger GetJobTrigger()
            => TriggerBuilder.Create()
                .WithIdentity("Recalculate tags counters trigger")
                .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(_repeatEveryMinutes))
                .Build();
    }
}

public class RecalculateTagCountersSettings
{
    public int RepeatEveryMinutes { get; set; } = 5;
}
