using Quartz;
using Quartz.Impl;

namespace SGC.Utils.ScheduledTask
{
    public class JobScheduler
    {
        public static async System.Threading.Tasks.Task StartAsync()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();

            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.Start();

            IJobDetail jobNoche = JobBuilder.Create<JobNoche>()
                .WithIdentity("jobNoche", "group1")
                .Build();

            ITrigger triggerClienteDeudor = TriggerBuilder.Create()
                .WithIdentity("triggerNoche", "group1")
                .StartNow()
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(22, 00))
                .Build();

            await scheduler.ScheduleJob(jobNoche, triggerClienteDeudor);

            IJobDetail jobMedioDia = JobBuilder.Create<JobMedioDia>()
                .WithIdentity("jobMedioDia", "group2")
                .Build();

            ITrigger triggerMedioDia = TriggerBuilder.Create()
                .WithIdentity("triggerMedioDia", "group2")
                .StartNow()
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(07, 00))
                .Build();

            await scheduler.ScheduleJob(jobMedioDia, triggerMedioDia);

            IJobDetail jobRelatorCorreo = JobBuilder.Create<JobMedioDia>()
                .WithIdentity("jobRelatorCorreo", "group3")
                .Build();

            ITrigger triggerRelatorCorreo = TriggerBuilder.Create()
                .WithIdentity("triggerRelatorCorreo", "group3")
                .StartNow()
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(18, 00))
                .Build();

            await scheduler.ScheduleJob(jobRelatorCorreo, triggerRelatorCorreo);
        }
    }
}