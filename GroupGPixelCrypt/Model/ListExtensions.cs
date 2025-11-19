using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupGPixelCrypt.Model
{
    /// <summary>
    /// Contains extension methods for lists.
    /// </summary>
    public static class ListExtensions
    {
        #region Methods

        public static ICollection<IList<T>> ChunkBy<T>(this IList<T> source, int chunkSize)
        {
            if (chunkSize <= 0)
            {
                throw new ArgumentException("Chunk size must be greater than zero.", nameof(chunkSize));
            }
            IList<IList<T>> chunks = new List<IList<T>>();
            for (int i = 0; i < source.Count; i += chunkSize)
            {
                IList<T> chunk = source.Skip(i).Take(chunkSize).ToList();
                chunks.Add(chunk);
            }
            return chunks;
        }

        #endregion
    }
}
