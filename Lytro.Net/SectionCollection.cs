using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lytro
{
    public class SectionCollection<TSection> : ICollection<TSection>
        where TSection : Section
    {
        private List<TSection> sections = new List<TSection>();

        public void Add(TSection item)
        {
            this.sections.Add(item);
        }

        public void Clear()
        {
            this.sections.Clear();
        }

        public bool Contains(TSection item)
        {
            return this.sections.Contains(item);
        }

        public void CopyTo(TSection[] array, int arrayIndex)
        {
            this.sections.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.sections.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(TSection item)
        {
            return this.sections.Remove(item);
        }

        public IEnumerator<TSection> GetEnumerator()
        {
            return this.sections.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public TSection this[int index]
        {
            get
            {
                return this.sections[index];
            }
        }
    }
}