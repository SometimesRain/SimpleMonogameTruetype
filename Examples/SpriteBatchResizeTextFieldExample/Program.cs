using System;

namespace SimpleMonogameTruetype.Example
{
	public static class Program
	{
		[STAThread]
		static void Main()
		{
			using (var game = new Game1(400, 600))
				game.Run();
		}
	}
}
