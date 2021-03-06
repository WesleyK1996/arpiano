using System;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmTool
{
    /// <summary>
    /// A Track is a collection of Features that are ordered in time.
    /// </summary>
    public abstract class Track : ScriptableObject
    {
        /// <summary>
        /// The name of the Track.
        /// </summary>
        public new string name
        {
            get
            {
                return _name;
            }
        }

        [SerializeField]
        protected string _name;
    }

    /// <summary>
    /// A Track is a collection of Features that are ordered in time.
    /// </summary>
    /// <typeparam name="T">The type of Features contained in this Track.</typeparam>
    public abstract class Track<T> : Track where T : Feature
    {
        public T this[int index]
        {
            get
            {
                return _features[index];
            }
        }

        /// <summary>
        /// The number of features contained in this Track.
        /// </summary>
        public int count
        {
            get
            {
                return _features.Count;
            }
        }

        [SerializeField]
        private List<T> _features = new List<T>();

        [NonSerialized]
        private List<int> cachedTimestamps = new List<int>();
        private Dictionary<int, int> cachedIndices = new Dictionary<int, int>();

        private static Type concreteType;

        static Track()
        {
            foreach (Type type in typeof(T).Assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(Track<T>)) && !type.IsAbstract)
                {
                    concreteType = type;
                    return;
                }
            }
        }

        /// <summary>
        /// Add a Feature to the Track.
        /// </summary>
        /// <param name="feature">The Feature to add.</param>
        public void Add(T feature)
        {
            if (_features.Count == 0 || feature.timestamp > _features[_features.Count - 1].timestamp)
            {
                _features.Add(feature);

                return;
            }

            int index = GetIndex(feature.timestamp);

            _features.Insert(index, feature);

            ClearCache(feature.timestamp);
        }

        /// <summary>
        /// Remove a Feature from the Track.
        /// </summary>
        /// <param name="feature">The Feature to remove.</param>
        public void Remove(T feature)
        {
            _features.Remove(feature);
        }

        /// <summary>
        /// Finds all features within a certain time frame.
        /// </summary>
        /// <param name="features">The list of Features to populate</param>
        /// <param name="start">The starting point in seconds.</param>
        /// <param name="end">The end point in seconds.</param>
        public void GetFeatures(List<T> features, float start, float end)
        {
            int startIndex = GetIndex(start);
            int endIndex = GetIndex(end);

            for (int i = startIndex; i < endIndex; i++)
                features.Add(_features[i]);
        }

        /// <summary>
        /// Finds all features within a certain time frame, including features with a length that intersects the time frame.
        /// </summary>
        /// <param name="features">The list of Features to populate</param>
        /// <param name="start">The starting point in seconds.</param>
        /// <param name="end">The end point in seconds.</param>
        public void GetIntersectingFeatures(List<T> features, float start, float end)
        {
            int startIndex = GetIntersectingIndex(start);
            int endIndex = GetIndex(end);

            for (int i = startIndex; i < endIndex; i++)
            {
                T feature = _features[i];

                if (feature.timestamp + feature.length > start)
                    features.Add(feature);
            }
        }

        /// <summary>
        /// Returns the index of the Feature that is closest to a timestamp.
        /// </summary>
        /// <param name="timestamp">The timestamp in seconds.</param>
        /// <returns>The index of the Feature that is closest to the timestamp.</returns>
        public int GetIndex(float timestamp)
        {
            int index = BinarySearch(timestamp);

            if (index < 0)
                index = ~index;

            while (index > 1 && _features[index - 1].timestamp >= timestamp)
                index--;

            return index;
        }

        private int BinarySearch(float timestamp)
        {
            int min = 0;
            int max = _features.Count - 1;

            while (min <= max)
            {
                int mid = min + (max - min >> 1);
                int comp = _features[mid].timestamp.CompareTo(timestamp);

                if (comp == 0)
                    return mid;

                if (comp < 0)
                    min = mid + 1;
                else
                    max = mid - 1;
            }

            return ~min;
        }

        private int GetIntersectingIndex(float timestamp)
        {
            int time = Mathf.RoundToInt(timestamp / 5) * 5;

            int index = GetCacheIndex(time);

            int cached = 0;

            if (index > 0)
                cached = cachedIndices[cachedTimestamps[index - 1]];

            for (int i = cached; i < _features.Count; i++)
            {
                T feature = _features[i];

                if (feature.timestamp + feature.length > timestamp)
                {
                    if (!cachedIndices.ContainsKey(time))
                    {
                        cachedTimestamps.Insert(index, time);
                        cachedIndices.Add(time, i);
                    }

                    return i;
                }
            }

            return _features.Count;
        }

        private int GetCacheIndex(int timestamp)
        {
            int index = cachedTimestamps.BinarySearch(timestamp);

            if (index < 0)
                index = Mathf.Max(~index - 1, 0);

            return index;
        }

        private void ClearCache(float timestamp)
        {
            int index = GetCacheIndex((int)timestamp);

            for (int i = index; i < cachedTimestamps.Count; i++)
                cachedIndices.Remove(cachedTimestamps[i]);

            cachedTimestamps.RemoveRange(index, cachedTimestamps.Count - index);
        }

        /// <summary>
        /// Create a track of type T with a name.
        /// </summary>
        /// <param name="name">The name of the Track</param>
        /// <returns>The new Track.</returns>
        public static Track<T> Create(string name)
        {
            if (concreteType == null)
            {
                Debug.LogWarning("No Track found for " + typeof(T).Name);
                return null;
            }

            Track<T> track = CreateInstance(concreteType) as Track<T>;

            (track as ScriptableObject).name = name;
            track._name = name;

            return track;
        }
    }
}