using System;

namespace Netologia.Quest
{
	/// <summary>
    /// Структура содержащая два граничных значения
    /// </summary>
	[Serializable]
	public struct IntervalInt : IEquatable<IntervalInt>
	{
        /// <summary>
        /// Минимальноее граничное значение
        /// </summary>
		public int Min;
        /// <summary>
        /// Максимальное граничное значение
        /// </summary>
		public int Max;

        public IntervalInt(int min, int max)
        {
            (Min, Max) = min <= max
	            ? (min, max)
	            : (max, min);
        }

		/// <summary>
		/// Случайное число из интервала (включительно min, исключая max)
		/// </summary>
		public int Random => UnityEngine.Random.Range(Min, Max);

		/// <summary>
		/// Попадает-ли заданное число в интервал (не строгое сравнение границ)
		/// </summary>
		/// <param name="value">Проверяемое число</param>
		/// <returns>Входит-ли число в интервал</returns>
		/// <remarks>Для настраиваемой проверки используется ConfContains</remarks>
		public bool SoftContains(int value) => Min <= value && value <= Max;
		/// <summary>
		/// Попадает-ли заданное число в интервал (строгое сравнение границ)
		/// </summary>
		/// <param name="value">Проверяемое число</param>
		/// <returns>Входит-ли число в интервал</returns>
		/// <remarks>Для настраиваемой проверки используется ConfContains</remarks>
		public bool StrictContains(int value) => Min < value && value < Max;
		/// <summary>
		/// Попадает-ли заданное число в интервал
		/// </summary>
		/// <param name="value">Проверяемое число</param>
		/// <param name="excludeMin">Не строго проверять по нижней границе</param>
		/// <param name="excludeMax">Не строго проверять по верхней границе</param>
		/// <returns>Входит-ли число в интервал</returns>
		public bool ConfContains(int value, bool excludeMin, bool excludeMax)
		{
			var result1 = excludeMin
				? value > Min
				: value >= Min;

			var result2 = excludeMax
				? value < Max
				: value <= Max;

			return result1 && result2;
		}

        public int Clamp(int value) => Math.Clamp(value, Min, Max);
        public int Clamp(float value) => Clamp((int)Math.Round(value));
        public int Clamp(double value) => Clamp((int)Math.Round(value));

		public bool Equals(IntervalInt interval)
			=> interval.Min == Min && interval.Max == Max;

		public override bool Equals(object obj)
			=> obj is IntervalInt interval && Equals(interval);
		public override string ToString() 
			=> $"{Min} ::: {Max}";
		public override int GetHashCode() 
			=> (Min, Max).GetHashCode();
		public static bool operator ==(IntervalInt a, IntervalInt b)
			=> a.Equals(b);
		public static bool operator !=(IntervalInt a, IntervalInt b)
			=> !a.Equals(b);
        public static implicit operator IntervalInt((int, int) pair)
            => new IntervalInt(pair.Item1, pair.Item2);
        public static explicit operator Interval(IntervalInt value)
	        => new Interval(value.Min, value.Max);
	}
}