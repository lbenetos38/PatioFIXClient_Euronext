using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatioFIX
{
	/// <summary>
	/// In Greece, the standard time is Eastern European Time (Greek: Ώρα Ανατολικής Ευρώπης; EET; UTC+02:00). 
	/// Daylight saving time, which moves one hour ahead to UTC+03:00 is observed from the last Sunday in March 
	/// to the last Sunday in October. Greece adopted EET in 1916
	/// </summary>
	public static class GreeceTimeHelper
	{
		/// <summary>
		/// Standard time: 	UTC +2
		/// </summary>
		public static readonly int UTC_OFFSET = 2;
		/// <summary>
		/// Daylight saving time:	UTC +3
		/// </summary>
		public static readonly int DST_OFFSET = 3;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="year"></param>
		/// <returns></returns>
		static int GetLastMarchSunday(int year)
		{
			DateTime _dt = new DateTime(year, /*March*/3, 1);
			int lastSunday = 0;

			while (_dt.Month == 3)
			{
				if (_dt.DayOfWeek == DayOfWeek.Sunday)
					lastSunday = _dt.Day;
				_dt = _dt.AddDays(1);
			}
			return lastSunday;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="year"></param>
		/// <returns></returns>
		static int GetLastOctoberSunday(int year)
		{
			DateTime _dt = new DateTime(year, /*October*/10, 1);
			int lastSunday = 0;

			while (_dt.Month == 10)
			{
				if (_dt.DayOfWeek == DayOfWeek.Sunday)
					lastSunday = _dt.Day;
				_dt = _dt.AddDays(1);
			}
			return lastSunday;
		}


		/// <summary>
		/// Επιστρέφει την διαφορα σε ωρες μεταξυ 'UTC' και 'Ελληνικης τοπικης ωρας'.
		/// Επιστρεφει την τρεχουσα διαφορα
		/// </summary>
		/// <returns></returns>
		static public int GetUTCOffset()
		{
			var dt = DateTime.Now;

			return GetUTCOffset(dt.Year, dt.Month, dt.Day);
		}


		/// <summary>
		/// Επιστρεφει την διαφορα ωρων μεταξυ της προσδιοριζομενης UTC ωρας με την 'Ελληνικη τοπικη ωρα'.
		/// Δηλαδη αυτον τον αριθμο (ωρών) που θα παρουμε (+2 ή +3) εαν τον προσθεσοιυμε στην UTC ωρα θα παρουμε την ατιστοιχη 'Ελληνικη τοπικη ωρα'.
		/// </summary>
		/// <param name="year"></param>
		/// <param name="month"></param>
		/// <param name="day"></param>
		/// <returns></returns>
		static public int GetUTCOffset(int year, int month, int day)
		{

			if (month == /*March*/3)
			{
				var lastSunday = GetLastMarchSunday(year);
				if (day >= lastSunday)
					return DST_OFFSET;
			}
			else if (month == /*April*/4)
			{
				return DST_OFFSET;
			}
			else if (month == /*May*/5)
			{
				return DST_OFFSET;
			}
			else if (month == /*June */6)
			{
				return DST_OFFSET;
			}
			else if (month == /*July */7)
			{
				return DST_OFFSET;
			}
			else if (month == /*August */8)
			{
				return DST_OFFSET;
			}
			else if (month == /*September */9)
			{
				return DST_OFFSET;
			}
			else if (month == /*October */10)
			{
				var lastSunday = GetLastOctoberSunday(year);
				if (day < lastSunday)
					return DST_OFFSET;
			}

			return UTC_OFFSET;
		}



	}
}
