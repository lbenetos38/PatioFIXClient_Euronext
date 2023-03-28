using Microsoft.Extensions.Configuration;
using System;

namespace PatioFIX.Common.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class SimpleSchedulerSection
    {
        /// <summary>
        /// 
        /// </summary>
        public string SectionName { get; } = "SimpleSchedule";

        public bool Disable { get; } = false;

        public string Monday { get; }
        public string Tuesday { get; }
        public string Wednesday { get; }
        public string Thursday { get; }
        public string Friday { get; }
        public string Saturday { get; }
        public string Sunday { get; }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="required"></param>
        /// <exception cref="ArgumentException"></exception>
        internal SimpleSchedulerSection(IConfigurationSection root, bool required = false)
        {
            var section = root.GetSection(this.SectionName);
            if (section.Exists())
            {

                var value = section["Disable"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.Disable = Convert.ToBoolean(value);
                }


                value = section["Monday"];
                SimpleSchedule._ValidateScheduleEntry("Monday", value);
                this.Monday = value;


                value = section["Tuesday"];
                SimpleSchedule._ValidateScheduleEntry("Tuesday", value);
                this.Tuesday = value;


                value = section["Wednesday"];
                SimpleSchedule._ValidateScheduleEntry("Wednesday", value);
                this.Wednesday = value;


                value = section["Thursday"];
                SimpleSchedule._ValidateScheduleEntry("Thursday", value);
                this.Thursday = value;


                value = section["Friday"];
                SimpleSchedule._ValidateScheduleEntry("Friday", value);
                this.Friday = value;


                value = section["Saturday"];
                SimpleSchedule._ValidateScheduleEntry("Saturday", value);
                this.Saturday = value;


                value = section["Sunday"];
                SimpleSchedule._ValidateScheduleEntry("Sunday", value);
                this.Sunday = value;

            }
            else
            {
                if (required)
                {
                    throw new ArgumentException($"There is no {section.Path} section but is a required one");
                }
            }
        }




    }
}
