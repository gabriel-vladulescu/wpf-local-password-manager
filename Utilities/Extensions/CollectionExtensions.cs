using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AccountManager.Utilities.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds a range of items to an ObservableCollection
        /// </summary>
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null || items == null)
                return;

            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Removes all items matching a predicate
        /// </summary>
        public static int RemoveAll<T>(this ObservableCollection<T> collection, Func<T, bool> predicate)
        {
            if (collection == null || predicate == null)
                return 0;

            var itemsToRemove = collection.Where(predicate).ToList();
            
            foreach (var item in itemsToRemove)
            {
                collection.Remove(item);
            }
            
            return itemsToRemove.Count;
        }

        /// <summary>
        /// Checks if a collection is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        /// <summary>
        /// Safely gets an item at the specified index
        /// </summary>
        public static T SafeGet<T>(this IList<T> list, int index, T defaultValue = default(T))
        {
            if (list == null || index < 0 || index >= list.Count)
                return defaultValue;

            return list[index];
        }

        /// <summary>
        /// Converts IEnumerable to ObservableCollection
        /// </summary>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new ObservableCollection<T>(source ?? Enumerable.Empty<T>());
        }

        /// <summary>
        /// Performs an action on each item in the collection
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (collection == null || action == null)
                return;

            foreach (var item in collection)
            {
                action(item);
            }
        }
    }
}