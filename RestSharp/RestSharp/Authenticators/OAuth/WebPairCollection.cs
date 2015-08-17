namespace RestSharp.Authenticators.OAuth
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    internal class WebPairCollection : IList<WebPair>
    {
        private IList<WebPair> _parameters;

        public WebPairCollection(IEnumerable<WebPair> parameters)
        {
            this._parameters = new List<WebPair>(parameters);
        }

        public WebPairCollection()
        {
            this._parameters = new List<WebPair>(0);
        }

        public WebPairCollection(int capacity)
        {
            this._parameters = new List<WebPair>(capacity);
        }

        public WebPairCollection(IDictionary<string, string> collection)
            : this()
        {
            this.AddCollection(collection);
        }

#if !WINDOWS_PHONE && !SILVERLIGHT && !PocketPC
        public WebPairCollection(NameValueCollection collection)
            : this()
        {
            this.AddCollection(collection);
        }
#endif

        public virtual int Count
        {
            get
            {
                return this._parameters.Count;
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return this._parameters.IsReadOnly;
            }
        }

        public virtual IEnumerable<string> Names
        {
            get
            {
                return this._parameters.Select(p => p.Name);
            }
        }

        public virtual IEnumerable<string> Values
        {
            get
            {
                return this._parameters.Select(p => p.Value);
            }
        }

        public virtual WebPair this[string name]
        {
            get
            {
                return this.SingleOrDefault(p => p.Name.Equals(name));
            }
        }

        public virtual WebPair this[int index]
        {
            get
            {
                return this._parameters[index];
            }

            set
            {
                this._parameters[index] = value;
            }
        }

        public void AddCollection(IDictionary<string, string> collection)
        {
            foreach (var key in collection.Keys)
            {
                var parameter = new WebPair(key, collection[key]);
                this._parameters.Add(parameter);
            }
        }

#if !WINDOWS_PHONE && !SILVERLIGHT && !PocketPC
        public virtual void AddRange(NameValueCollection collection)
        {
            this.AddCollection(collection);
        }
#endif

        public virtual void AddRange(WebPairCollection collection)
        {
            this.AddCollection(collection);
        }

        public virtual void AddRange(IEnumerable<WebPair> collection)
        {
            this.AddCollection(collection);
        }

        public virtual void Sort(Comparison<WebPair> comparison)
        {
            var sorted = new List<WebPair>(this._parameters);
            sorted.Sort(comparison);
            this._parameters = sorted;
        }

        public virtual bool RemoveAll(IEnumerable<WebPair> parameters)
        {
            var success = true;
            var array = parameters.ToArray();

            for (var p = 0; p < array.Length; p++)
            {
                var parameter = array[p];
                success &= this._parameters.Remove(parameter);
            }

            return success && array.Length > 0;
        }

        public virtual void Add(string name, string value)
        {
            var pair = new WebPair(name, value);
            this._parameters.Add(pair);
        }

        public virtual IEnumerator<WebPair> GetEnumerator()
        {
            return this._parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public virtual void Add(WebPair parameter)
        {
            this._parameters.Add(parameter);
        }

        public virtual void Clear()
        {
            this._parameters.Clear();
        }

        public virtual bool Contains(WebPair parameter)
        {
            return this._parameters.Contains(parameter);
        }

        public virtual void CopyTo(WebPair[] parameters, int arrayIndex)
        {
            this._parameters.CopyTo(parameters, arrayIndex);
        }

        public virtual bool Remove(WebPair parameter)
        {
            return this._parameters.Remove(parameter);
        }

        public virtual int IndexOf(WebPair parameter)
        {
            return this._parameters.IndexOf(parameter);
        }

        public virtual void Insert(int index, WebPair parameter)
        {
            this._parameters.Insert(index, parameter);
        }

        public virtual void RemoveAt(int index)
        {
            this._parameters.RemoveAt(index);
        }

        private void AddCollection(IEnumerable<WebPair> collection)
        {
            foreach (var parameter in collection)
            {
                var pair = new WebPair(parameter.Name, parameter.Value);
                this._parameters.Add(pair);
            }
        }

#if !WINDOWS_PHONE && !SILVERLIGHT && !PocketPC
        private void AddCollection(NameValueCollection collection)
        {
            var parameters = collection.AllKeys.Select(key => new WebPair(key, collection[key]));
            foreach (var parameter in parameters)
            {
                this._parameters.Add(parameter);
            }
        }
#endif
    }
}
