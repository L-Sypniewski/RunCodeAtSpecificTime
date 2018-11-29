using System;
using System.Threading;
using System.Threading.Tasks;

namespace RunCodeAtTime
{
    public class ScheduleRunner
    {
        CancellationTokenSource m_ctSource;

        public ScheduleRunner()
        {
            m_ctSource = new CancellationTokenSource();
        }

        public enum Scheduler
        {
            EveryMinutes,
            EveryHour,
            EveryHalfDay,
            EveryDay,
            EveryWeek,
            EveryMonth,
            EveryYear,
        }

        /// <summary>
        /// Schedule the time the need to be call. Omit scheduler to null if action should be performed only once.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="action"></param>
        /// <param name="actactionParameterion"></param>
        /// <param name="scheduler"></param>
        public void runCodeAt<T>(DateTime date, Action<T> action, T actionParameter, Scheduler? scheduler = null)
        {
            TimeSpan ts = CalculateTimeSpan(ref date, scheduler);
            RunTask<T>(date, action, actionParameter, scheduler, ts);
        }
        /// <summary>
        /// Schedule the time the need to be call. Omit scheduler to null if action should be performed only once.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="action"></param>
        /// <param name="scheduler"></param>
        public void runCodeAt(DateTime date, Action action, Scheduler? scheduler = null)
        {
            TimeSpan ts = CalculateTimeSpan(ref date, scheduler);
            RunTask<object>(date, action, null, scheduler, ts);
        }

        private void RunTask<T>(DateTime date, Delegate action, T actionParameter, Scheduler? scheduler, TimeSpan ts)
        {
            //waits certain time and runs the code, in meantime you can cancel the task at any time
            Task.Delay(ts).ContinueWith((x) =>
            {
                //run the code at the time
                if (action is Action<T>)
                {
                    (action as Action<T>).Invoke(actionParameter);
                }
                else if (action is Action)
                {
                    (action as Action).Invoke();
                }
                else
                {
                    throw new ArgumentException("'actionParameter' parmeter must be Action or Action<T>!");
                }

                //setup call next time
                if (scheduler.HasValue)
                {   if (action is Action<T>)
                        runCodeAt(getNextDate(date, (Scheduler)scheduler), (Action<T>)action, actionParameter, scheduler);
                    else
                        runCodeAt(getNextDate(date, (Scheduler)scheduler), (Action)action, scheduler);
                }

            }, m_ctSource.Token);
        }

        private TimeSpan CalculateTimeSpan(ref DateTime date, Scheduler? scheduler)
        {
            var dateNow = DateTime.Now;
            TimeSpan ts;
            if (!scheduler.HasValue)
            {
                if (date < dateNow)
                    throw new ArgumentException("Provided date has to be grater than a current date!");
                ts = date - dateNow;
            }
            else
            {
                date = getNextDate(date, (Scheduler)scheduler);
                ts = date - dateNow;
            }
            return ts;
        }

        /// <summary>
        /// returns next date the code to be run
        /// </summary>
        /// <param name="date"></param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        private DateTime getNextDate(DateTime date, Scheduler scheduler)
        {
            switch (scheduler)
            {
                case Scheduler.EveryMinutes:
                    return date.AddMinutes(1);
                case Scheduler.EveryHour:
                    return date.AddHours(1);
                case Scheduler.EveryHalfDay:
                    return date.AddHours(12);
                case Scheduler.EveryDay:
                    return date.AddDays(1);
                case Scheduler.EveryWeek:
                    return date.AddDays(7);
                case Scheduler.EveryMonth:
                    return date.AddMonths(1);
                case Scheduler.EveryYear:
                    return date.AddYears(1);
                default:
                    throw new Exception("Invalid scheduler!");
            }

        }        
    }
}
