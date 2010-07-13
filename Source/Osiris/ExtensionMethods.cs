using System;

namespace Osiris
{
	public static class ExtensionMethods
	{
		public static T GetService<T>(this IServiceProvider serviceProvider)
		{
			return (T) serviceProvider.GetService(typeof(T));
		}
	}
}
