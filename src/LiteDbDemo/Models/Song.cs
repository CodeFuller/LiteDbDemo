using System;

namespace LiteDbDemo.Models
{
	public class Song
	{
		public int TrackNumber { get; set; }

		public string Title { get; set; }

		public TimeSpan Length { get; set; }
	}
}
