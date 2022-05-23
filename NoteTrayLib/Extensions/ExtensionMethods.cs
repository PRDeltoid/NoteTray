using System.Collections.ObjectModel;

namespace NoteTrayLib.Extensions;

public static class ExtensionMethods
{
	public static int Remove<T>(this ObservableCollection<T> collection, Func<T, bool> predicate)
	{
		List<T> itemsToRemove = collection.Where(predicate).ToList();

		foreach (T itemToRemove in itemsToRemove)
		{
			collection.Remove(itemToRemove);
		}

		return itemsToRemove.Count;
	}
}