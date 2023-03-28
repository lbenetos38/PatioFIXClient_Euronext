using PatioFIX.Common.Configuration;
using System;

namespace PatioFIX.Common
{
    /// <summary>
    /// Ενα απλο ημερησιο προγραμμα λειτουργίας
    /// </summary>
    public class SimpleSchedule
    {
        public class ScheduleDay
        {
            public bool IsActive { get; set; }
            public bool HasError { get; set; }
            public string Error { get; set; }
            public string InitializationValue { get; set; }
            public TimeSpan From { get; set; }
            public TimeSpan To { get; set; }

            public ScheduleDay()
            {
                this.IsActive = false;
                this.From = TimeSpan.Zero;
                this.To = TimeSpan.Zero;
            }
        }


        readonly ScheduleDay[] schedule = new ScheduleDay[7];
        bool m_isInitialized = false;
        bool m_isDisabled = true;

        /// <summary>
        /// Κανει initialize το SimpleSchedule
        /// <para>
        /// Διαβαζει απο το appsettings.json και απο το SimpleSchedule section:
        ///                  Disable: false,
        ///                  Monday: "10:00-17:15",
        ///                  Tuesday: "10:00-17:15",
        ///                  Wednesday: "10:00-17:15",
        ///                  Thursday: "10:00-17:15",
        ///                  Friday: "10:00-17:15",
        ///                  Saturday: "",
        ///                  Sunday: ""
        /// </para>
        /// </summary>
        /// <param name="sconfig"></param>
        public void LoadSchedule(SimpleSchedulerSection sconfig)
        {
            //
            m_isDisabled = sconfig.Disable;
            //
            schedule[(int)DayOfWeek.Sunday] = _ReadScheduleEntry(sconfig.Sunday);
            schedule[(int)DayOfWeek.Monday] = _ReadScheduleEntry(sconfig.Monday);
            schedule[(int)DayOfWeek.Tuesday] = _ReadScheduleEntry(sconfig.Tuesday);
            schedule[(int)DayOfWeek.Wednesday] = _ReadScheduleEntry(sconfig.Wednesday);
            schedule[(int)DayOfWeek.Thursday] = _ReadScheduleEntry(sconfig.Thursday);
            schedule[(int)DayOfWeek.Friday] = _ReadScheduleEntry(sconfig.Friday);
            schedule[(int)DayOfWeek.Saturday] = _ReadScheduleEntry(sconfig.Saturday);
            m_isInitialized = true;
        }
        static ScheduleDay _ReadScheduleEntry(string _value)
        {
            var day = new ScheduleDay();
            day.InitializationValue = _value;

            if (string.IsNullOrWhiteSpace(_value))
            {
                return day;
            }

            var parts = _value.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return day;

            try
            {
                day.From = TimeSpan.Parse(parts[0]);
                day.To = TimeSpan.Parse(parts[1]);

                if (day.From < day.To)
                    day.IsActive = true;
                else
                    throw new Exception("Invalid Time Range");
            }
            catch (Exception ex)
            {
                day.Error = ex.Message;
                day.IsActive = false;
                day.HasError = true;
            }

            return day;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="day"></param>
        /// <param name="_value"></param>
        /// <exception cref="PtException"></exception>
        internal static void _ValidateScheduleEntry(string day, string _value)
        {
            if (string.IsNullOrWhiteSpace(_value))
            {
                return;
            }

            var parts = _value.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new PtException($"{day}.ScheduleEntry '{_value}' is INVALID");

            try
            {
                var from = TimeSpan.Parse(parts[0]);
                var to = TimeSpan.Parse(parts[1]);

                if (from >= to)
                    throw new PtException($"{day}.ScheduleEntry '{_value}' has INVALID Time Range");
            }
            catch (Exception ex)
            {
                throw new PtException($"{day}.ScheduleEntry '{_value}' {ex.Message}");
            }
        }

        /// <summary>
        /// Μας λεει εαν ειναι disabled το SimpleSchedule
        /// </summary>
        public bool IsDisabled => m_isDisabled;

        /// <summary>
        /// Μας λεει εαν την τρεχουσα χρονικη στιγμη, είμαστε ON ή OFF σύμφωνα με το
        /// schedule που φορτωθηκε στο LoadSchedule()
        /// Εαν το SimpleSchedule είναι Disabled τοτε το Schedule einai πάντα ΟΝ
        /// </summary>
        /// <returns></returns>
        public bool IsScheduleOnOrDisabled()
        {
            if (m_isInitialized == false)
            {
                throw new PtException("SimpleSchedule is not Initialized!");
            }
            if (m_isDisabled == true)
            {
                return true;
            }

            var now = DateTime.Now;
            var scheduleDay = schedule[(int)now.DayOfWeek];


            if (scheduleDay.IsActive == false)
                return false;

            var _time = now.TimeOfDay;
            if (scheduleDay.From <= _time && _time <= scheduleDay.To)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="theLogger"></param>
        public void DumpSchedule(Logger theLogger)
        {
            if (m_isInitialized == false)
            {
                throw new PtException("Schedule is not Initialized!");
            }

            if (m_isDisabled == false)
            {
                theLogger.Info($"---------------- Schedule ----------------");
                _DumpScheduleDay(theLogger, "Sunday\t\t", schedule[(int)DayOfWeek.Sunday]);
                _DumpScheduleDay(theLogger, "Monday\t\t", schedule[(int)DayOfWeek.Monday]);
                _DumpScheduleDay(theLogger, "Tuesday\t", schedule[(int)DayOfWeek.Tuesday]);
                _DumpScheduleDay(theLogger, "Wednesday\t", schedule[(int)DayOfWeek.Wednesday]);
                _DumpScheduleDay(theLogger, "Thursday\t", schedule[(int)DayOfWeek.Thursday]);
                _DumpScheduleDay(theLogger, "Friday\t\t", schedule[(int)DayOfWeek.Friday]);
                _DumpScheduleDay(theLogger, "Saturday\t", schedule[(int)DayOfWeek.Saturday]);
            }
            else
            {
                theLogger.Info("Schedule::\t\t\tDISABLED");
            }
        }
        static void _DumpScheduleDay(Logger theLogger, string nameOfDay, ScheduleDay sd)
        {
            if (sd.HasError)
            {
                theLogger.Info($"{nameOfDay} = ERROR ({sd.Error})");
            }
            else if (sd.IsActive == false)
            {
                theLogger.Info($"{nameOfDay} = NOT ACTIVE");
            }
            else
            {
                theLogger.Info($"{nameOfDay} = from:{sd.From}, till:{sd.To}");
            }
        }
    }
}
